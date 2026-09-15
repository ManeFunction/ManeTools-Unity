using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    public static class Decorations
    {
        private const string DecoratorContainerClass = "unity-decorator-drawers-container";

        public static VisualElement CreateUnsupportedFieldTypeWarning(
            SerializedProperty property,
            string attributeName,
            string expectedTypes) =>
            CreateWarningField(property,
                $"[{attributeName}] can only be applied to {expectedTypes} fields.\n'{property.displayName}' is {property.propertyType}.");

        public static VisualElement CreateWarningField(SerializedProperty property, string message) =>
            WithHostDecorator(new PropertyField(property), InfoBoxDrawer.Create(message, InfoBoxType.Warning));

        public static VisualElement WithHostDecorator(VisualElement field, VisualElement decoration)
        {
            field.RegisterCallback<AttachToPanelEvent>(_ => AttachToHostDecorator(field, decoration));
            return field;
        }

        private static void AttachToHostDecorator(VisualElement field, VisualElement decoration)
        {
            PropertyField host = field.GetFirstAncestorOfType<PropertyField>();
            if (host == null)
                return;

            HideHostLabel(host, field);

            VisualElement container = FindDecoratorContainer(host);
            if (container == null)
            {
                container = new VisualElement();
                container.AddToClassList(DecoratorContainerClass);
                host.Insert(0, container);
            }

            if (decoration.parent != container)
                container.Add(decoration);
        }

        private static VisualElement FindDecoratorContainer(PropertyField host)
        {
            foreach (VisualElement child in host.Children())
            {
                if (child.ClassListContains(DecoratorContainerClass))
                    return child;
            }

            return null;
        }

        private static void HideHostLabel(PropertyField host, VisualElement drawerRoot)
        {
            host.Query<Label>(className: PropertyField.labelUssClassName).ForEach(label =>
            {
                if (IsUnder(label, drawerRoot))
                    return;

                label.style.display = DisplayStyle.None;
            });
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
