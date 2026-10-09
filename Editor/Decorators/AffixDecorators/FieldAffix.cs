using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal static class FieldAffix
    {
        private const string PrefixClass = "mie-prefix";
        private const string PostfixClass = "mie-postfix";
        private const string HasPrefixClass = "mie-has-prefix";
        private const string BaseFieldClass = "unity-base-field";
        private const string InputClass = "unity-base-field__input";
        private const string ChipFieldClass = "unity-composite-field__field";
        private const string MinMaxFieldClass = "mie-minmax-field";

        private static readonly FieldInfo LabelField = typeof(PropertyField)
            .GetField("m_Label", BindingFlags.Instance | BindingFlags.NonPublic);

        private static StyleSheet _sheet;

        public static VisualElement CreateHook(string text, bool prefix)
        {
            VisualElement hook = new()
            {
                style = { display = DisplayStyle.None }
            };
            hook.RegisterCallback<AttachToPanelEvent>(_ => Bind(hook, text, prefix));
            return hook;
        }

        private static void Bind(VisualElement hook, string text, bool prefix)
        {
            PropertyField host = hook.GetFirstAncestorOfType<PropertyField>();
            if (host == null)
                return;

            void TryApply() => Apply(host, text, prefix);

            host.RegisterCallback<GeometryChangedEvent>(_ => TryApply());
            hook.schedule.Execute(TryApply);
            TryApply();
        }

        private static void Apply(PropertyField host, string text, bool prefix)
        {
            SerializedProperty property = host.GetBoundSerializedProperty();
            if (MinMaxCurveField.IsMinMaxCurve(property))
                return;

            if (property is { isArray: true })
            {
                ApplyCollectionTitle(host, property);
                return;
            }

            VisualElement field = FindBaseField(host);
            if (field != null && field.ClassListContains(MinMaxFieldClass))
            {
                // Min / Max drawer: the affix goes around each endpoint input, not around the whole row.
                foreach (VisualElement endpoint in field.Query(className: ChipFieldClass).ToList())
                {
                    if (endpoint.GetFirstAncestorOfType<PropertyField>() == host)
                        AddChip(endpoint, text, prefix);
                }

                return;
            }

            AddChip(field, text, prefix);
        }

        // Chips are siblings of the input. The input must stay a direct child of its field: numeric fields cast
        // the input's parent to the field when dragging the label, and field styles expect that structure.
        private static void AddChip(VisualElement field, string text, bool prefix)
        {
            VisualElement input = FindInput(field);
            if (input == null)
                return;

            string chipClass = prefix ? PrefixClass : PostfixClass;
            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(chipClass))
                    return;
            }

            ApplySheet(field);
            Label chip = new(text);
            chip.AddToClassList(chipClass);
            if (prefix)
                field.AddToClassList(HasPrefixClass);
            field.Insert(field.IndexOf(input) + (prefix ? 0 : 1), chip);
        }

        public static void ApplyCollectionTitle(PropertyField host, SerializedProperty property)
        {
            string name = property.GetAttribute<LabelAttribute>()?.Text;
            if (string.IsNullOrEmpty(name))
                name = property.displayName;

            string title =
                $"{property.GetAttribute<PrefixAttribute>()?.Text}{name}{property.GetAttribute<PostfixAttribute>()?.Text}";

            // Public label setter calls Rebind() and rebuilds the list. Decorators run after
            // Bind(), so write the backing field and the live ListView header instead.
            LabelField?.SetValue(host, title);

            BaseListView list = FindListView(host);
            if (list != null)
                list.headerTitle = title;
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

        private static VisualElement FindBaseField(PropertyField host)
        {
            foreach (VisualElement element in host.Query(className: BaseFieldClass).ToList())
            {
                if (element.GetFirstAncestorOfType<PropertyField>() != host)
                    continue;
                if (element.ClassListContains(ChipFieldClass))
                    continue;

                return element;
            }

            return null;
        }

        private static VisualElement FindInput(VisualElement field)
        {
            if (field == null)
                return null;

            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(InputClass))
                    return child;
            }

            return null;
        }

        private static void ApplySheet(VisualElement root)
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("FieldAffix.uss was not found next to FieldAffix.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(FieldAffix));
    }
}
