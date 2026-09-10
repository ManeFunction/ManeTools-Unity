using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal static class FieldAffix
    {
        private const string RowClass = "mie-affix-row";
        private const string RowFieldsClass = "mie-affix-row--fields";
        private const string PrefixClass = "mie-prefix";
        private const string PostfixClass = "mie-postfix";
        private const string BaseFieldClass = "unity-base-field";
        private const string InputClass = "unity-base-field__input";
        private const string ChipFieldClass = "unity-composite-field__field";

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
            if (property is { isArray: true })
            {
                ApplyCollectionTitle(host, property);
                return;
            }

            VisualElement field = FindBaseField(host);
            VisualElement input = FindInput(field);
            if (input == null)
                return;

            string chipClass = prefix ? PrefixClass : PostfixClass;
            VisualElement row = EnsureRow(field, input);
            if (row.Q(className: chipClass) != null)
                return;

            Label chip = new(text);
            chip.AddToClassList(chipClass);
            if (prefix)
                row.Insert(0, chip);
            else
                row.Add(chip);
        }

        private static void ApplyCollectionTitle(PropertyField host, SerializedProperty property)
        {
            string title =
                $"{property.GetAttribute<PrefixAttribute>()?.Text}{property.displayName}{property.GetAttribute<PostfixAttribute>()?.Text}";

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

        private static VisualElement EnsureRow(VisualElement field, VisualElement input)
        {
            VisualElement parent = input.parent;
            if (parent != null && parent.ClassListContains(RowClass))
                return parent;

            VisualElement row = new();
            row.AddToClassList(RowClass);
            if (HasFieldsAncestor(field))
                row.AddToClassList(RowFieldsClass);
            ApplySheet(row);

            int index = field.IndexOf(input);
            field.Insert(index, row);
            row.Add(input);
            return row;
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

        private static bool HasFieldsAncestor(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.ClassListContains(ManeEditorStyles.FieldsClass))
                    return true;
            }

            return false;
        }

        private static VisualElement FindInput(VisualElement field)
        {
            if (field == null)
                return null;

            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(InputClass))
                    return child;
                if (child.ClassListContains(RowClass))
                    return child.Q(className: InputClass);
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
