using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Shared inspector decorations: warnings and attaching extra UI to a property host.
    /// </summary>
    public static class Decorations
    {
        /// <summary>
        /// Warning that <paramref name="attributeName"/> cannot be used on this property type.
        /// </summary>
        public static VisualElement CreateUnsupportedFieldTypeWarning(
            SerializedProperty property,
            string attributeName,
            string expectedTypes) =>
            CreateWarningField(property,
                $"[{attributeName}] can only be applied to {expectedTypes} fields.\n'{property.displayName}' is {property.propertyType}.");

        /// <summary>
        /// Property field with a warning info box attached to the host decorator.
        /// </summary>
        public static VisualElement CreateWarningField(SerializedProperty property, string message) =>
            WithHostDecorator(new PropertyField(property), InfoBoxDrawer.Create(message, InfoBoxType.Warning));

        /// <summary>
        /// Attaches <paramref name="decoration"/> to the host <see cref="PropertyField"/> decorator container.
        /// </summary>
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

            PropertyHost.HideLabelsOutside(field);

            VisualElement container = PropertyHost.GetDecoratorContainer(host, create: true);
            decoration.AddToClassList(PropertyHost.KeepClass);
            if (decoration.parent != container)
                container.Add(decoration);
        }
    }
}
