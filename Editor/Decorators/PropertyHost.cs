using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// The host field of decorators: the <see cref="PropertyField"/> they are attached to.
    /// Decorators set what they want on it (label, affixes, read-only, availability), and the host applies
    /// all of it together, so decorators on one field never undo each other. It also owns what decorators
    /// need to know about the inner structure of a <see cref="PropertyField"/>.
    /// </summary>
    internal sealed class PropertyHost
    {
        /// <summary>
        /// Class of the element that holds decorator drawers inside a <see cref="PropertyField"/>.
        /// </summary>
        public const string DecoratorContainerClass = "unity-decorator-drawers-container";

        /// <summary>
        /// Decorator content with this class stays visible when <see cref="UnityDecoratorHider"/>
        /// hides the IMGUI decorators of a field.
        /// </summary>
        public const string KeepClass = "mie-host-content";

        private const string BaseFieldLabelClass = "unity-base-field__label";
        private const int BindRetries = 10;

        private static readonly ConditionalWeakTable<PropertyField, PropertyHost> Hosts = new();

        private static readonly FieldInfo LabelField = typeof(PropertyField)
            .GetField("m_Label", BindingFlags.Instance | BindingFlags.NonPublic);

        private string _label;
        private string _prefix;
        private string _postfix;
        private bool _readOnly;
        private bool? _available;
        private bool _hideWhenUnavailable;

        // The first live hook. When the field rebuilds its content, the old hooks are detached
        // and the first new one starts a fresh set of contributions.
        private VisualElement _anchor;
        private Action<SerializedProperty> _onBound;
        private SerializedProperty _bound;
        private bool _binding;

        private PropertyHost(PropertyField field)
        {
            Field = field;

            // The field builds its content after the decorators attach, and rebuilds it later (lists, foldouts).
            field.RegisterCallback<GeometryChangedEvent>(_ => Apply());
        }

        /// <summary>
        /// The decorated field.
        /// </summary>
        public PropertyField Field { get; }

        /// <summary>
        /// Text that replaces the field label. Null or empty keeps the label.
        /// </summary>
        public string Label
        {
            get => _label;
            set => Set(ref _label, value);
        }

        /// <summary>
        /// Chip before the input, or the start of a collection title.
        /// </summary>
        public string Prefix
        {
            get => _prefix;
            set => Set(ref _prefix, value);
        }

        /// <summary>
        /// Chip after the input, or the end of a collection title.
        /// </summary>
        public string Postfix
        {
            get => _postfix;
            set => Set(ref _postfix, value);
        }

        /// <summary>
        /// Draws the field disabled, whatever the availability says.
        /// </summary>
        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                if (_readOnly == value)
                    return;

                _readOnly = value;
                Refresh();
            }
        }

        /// <summary>
        /// Enables or disables the field, or hides it when <paramref name="hide"/> is set and it is not available.
        /// </summary>
        public void SetAvailable(bool available, bool hide)
        {
            if (_available == available && _hideWhenUnavailable == hide)
                return;

            _available = available;
            _hideWhenUnavailable = hide;
            Refresh();
        }

        /// <summary>
        /// Calls <paramref name="onBound"/> with the bound property once the field is bound,
        /// then again whenever its serialized object changes.
        /// </summary>
        public void WhenBound(Action<SerializedProperty> onBound)
        {
            if (onBound == null)
                return;

            _onBound += onBound;

            if (_bound != null)
            {
                onBound(_bound);
                return;
            }

            if (_binding || _anchor == null)
                return;

            _binding = true;
            // The tracking runs on the anchor, so the IMGUI hider must keep it in the tree.
            _anchor.AddToClassList(KeepClass);
            TryBind(_anchor, 0);
        }

        /// <summary>
        /// Hidden element for a <see cref="DecoratorDrawer"/> to return. Once it is attached,
        /// <paramref name="configure"/> receives the host of the field.
        /// </summary>
        public static VisualElement CreateHook(Action<PropertyHost> configure)
        {
            VisualElement hook = new()
            {
                style = { display = DisplayStyle.None }
            };

            bool configured = false;
            hook.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                if (configured)
                    return;

                PropertyHost host = Of(hook);
                if (host == null)
                    return;

                configured = true;
                host.Adopt(hook);
                configure?.Invoke(host);
            });

            return hook;
        }

        /// <summary>
        /// The host of the field that contains <paramref name="element"/>, or null outside a <see cref="PropertyField"/>.
        /// </summary>
        public static PropertyHost Of(VisualElement element)
        {
            PropertyField field = element as PropertyField ?? element?.GetFirstAncestorOfType<PropertyField>();
            return field == null ? null : Hosts.GetValue(field, f => new PropertyHost(f));
        }

        /// <summary>
        /// The decorator container of <paramref name="field"/>, created when <paramref name="create"/> is set.
        /// </summary>
        public static VisualElement GetDecoratorContainer(PropertyField field, bool create = false)
        {
            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(DecoratorContainerClass))
                    return child;
            }

            if (!create)
                return null;

            VisualElement container = new();
            container.AddToClassList(DecoratorContainerClass);
            field.Insert(0, container);
            return container;
        }

        /// <summary>
        /// Hides the labels of the host field, except those inside <paramref name="drawerRoot"/>,
        /// for drawers that draw their own label.
        /// </summary>
        public static void HideLabelsOutside(VisualElement drawerRoot)
        {
            PropertyField host = drawerRoot.GetFirstAncestorOfType<PropertyField>();
            if (host == null)
                return;

            host.Query<UnityEngine.UIElements.Label>(className: PropertyField.labelUssClassName).ForEach(label =>
            {
                if (IsUnder(label, drawerRoot))
                    return;

                label.style.display = DisplayStyle.None;
            });
        }

        /// <summary>
        /// Title of a collection field: Prefix + (Label or display name) + Postfix.
        /// </summary>
        public static string CollectionTitle(SerializedProperty property) =>
            $"{property.GetAttribute<PrefixAttribute>()?.Text}{DisplayName(property)}{property.GetAttribute<PostfixAttribute>()?.Text}";

        /// <summary>
        /// The <see cref="LabelAttribute"/> text, or the display name.
        /// </summary>
        public static string DisplayName(SerializedProperty property)
        {
            string label = property.GetAttribute<LabelAttribute>()?.Text;
            return string.IsNullOrEmpty(label) ? property.displayName : label;
        }

        private void Adopt(VisualElement hook)
        {
            if (_anchor != null && _anchor.panel != null)
                return;

            // First hook, or the field was rebuilt: the old contributions belong to detached decorators.
            _anchor = hook;
            _label = _prefix = _postfix = null;
            _readOnly = false;
            _available = null;
            _hideWhenUnavailable = false;
            _onBound = null;
            _bound = null;
            _binding = false;
        }

        private void TryBind(VisualElement anchor, int attempt)
        {
            if (anchor != _anchor)
                return;

            SerializedProperty property = Field.GetBoundSerializedProperty();
            if (property?.serializedObject == null)
            {
                if (attempt < BindRetries)
                    anchor.schedule.Execute(() => TryBind(anchor, attempt + 1));
                return;
            }

            _bound = property.Copy();
            anchor.TrackSerializedObjectValue(_bound.serializedObject, _ =>
            {
                if (anchor == _anchor)
                    _onBound?.Invoke(_bound);
            });
            _onBound?.Invoke(_bound);
        }

        private void Set(ref string slot, string value)
        {
            if (string.IsNullOrEmpty(value))
                value = null;

            if (slot == value)
                return;

            slot = value;
            Refresh();
        }

        private void Refresh()
        {
            Apply();
            _anchor?.schedule.Execute(Apply);
        }

        private void Apply()
        {
            if (_anchor == null || _anchor.panel == null)
                return;

            ApplyVisibility();
            ApplyEnabled();
            ApplyTexts();
        }

        private void ApplyVisibility()
        {
            if (_available == null)
                return;

            Field.style.display = _available == false && _hideWhenUnavailable ? DisplayStyle.None : StyleKeyword.Null;
        }

        private void ApplyEnabled()
        {
            if (!_readOnly && _available == null)
                return;

            bool enabled = !_readOnly && _available != false;
            foreach (VisualElement child in Field.Children())
            {
                // Decorator content (info boxes, headers) stays readable.
                if (child.ClassListContains(DecoratorContainerClass))
                    continue;

                child.SetEnabled(enabled);
            }
        }

        private void ApplyTexts()
        {
            if (_label == null && _prefix == null && _postfix == null)
                return;

            SerializedProperty property = Field.GetBoundSerializedProperty();
            if (IsCollection(property))
            {
                ApplyCollectionTitle(property);
                return;
            }

            if (_label != null)
                ApplyRowLabel(_label);

            // A MinMaxCurve draws its own row; chips would break it.
            if (MinMaxCurveField.IsMinMaxCurve(property))
                return;

            if (_prefix != null)
                FieldAffix.AddChips(Field, _prefix, prefix: true);
            if (_postfix != null)
                FieldAffix.AddChips(Field, _postfix, prefix: false);
        }

        // Strings are arrays of chars to SerializedProperty, but they draw as a single field.
        private static bool IsCollection(SerializedProperty property) =>
            property is { isArray: true } && property.propertyType != SerializedPropertyType.String;

        private void ApplyCollectionTitle(SerializedProperty property)
        {
            string title = CollectionTitle(property);

            // The public label setter calls Rebind() and rebuilds the list. Decorators run after
            // Bind(), so write the backing field and the live ListView header instead.
            LabelField?.SetValue(Field, title);

            BaseListView list = FindListView(Field);
            if (list != null && list.headerTitle != title)
                list.headerTitle = title;
        }

        private void ApplyRowLabel(string text)
        {
            // Outermost label first: Query walks in document order, so nested fields come later.
            foreach (VisualElement element in Field.Query<VisualElement>().ToList())
            {
                if (element.GetFirstAncestorOfType<PropertyField>() != Field)
                    continue;

                if (element is Foldout foldout)
                {
                    if (foldout.text != text)
                        foldout.text = text;
                    return;
                }

                if (element is UnityEngine.UIElements.Label label && label.ClassListContains(BaseFieldLabelClass))
                {
                    if (label.text != text)
                        label.text = text;
                    return;
                }
            }
        }

        private static BaseListView FindListView(VisualElement root)
        {
            foreach (VisualElement child in root.hierarchy.Children())
            {
                if (child is BaseListView list)
                    return list;
                if (child is PropertyField)
                    continue;

                BaseListView nested = FindListView(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static bool IsUnder(VisualElement element, VisualElement ancestor)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current == ancestor)
                    return true;
            }

            return false;
        }
    }
}
