using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    public static class Decorations
    {
        public static VisualElement CreateUnsupportedFieldTypeWarning(
            SerializedProperty property,
            string attributeName,
            string expectedTypes) =>
            CreateWarningField(property,
                $"[{attributeName}] can only be applied to {expectedTypes} fields.\n'{property.displayName}' is {property.propertyType}.");

        public static VisualElement CreateWarningField(SerializedProperty property, string message)
        {
            VisualElement root = new();
            root.Add(InfoBoxDrawer.Create(message, InfoBoxType.Warning));
            root.Add(new PropertyField(property));
            return root;
        }
    }
}