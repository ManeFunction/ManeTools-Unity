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
            string expectedTypes)
        {
            VisualElement root = new();
            root.Add(InfoBoxDrawer.Create(
                $"[{attributeName}] can only be applied to {expectedTypes} fields.\n'{property.displayName}' is {property.propertyType}.",
                InfoBoxType.Warning));
            root.Add(new PropertyField(property));
            return root;
        }
    }
}