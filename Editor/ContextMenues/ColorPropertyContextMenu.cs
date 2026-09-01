using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    [InitializeOnLoad]
    internal static class ColorPropertyContextMenu
    {
        static ColorPropertyContextMenu() =>
            EditorApplication.contextualPropertyMenu += OnContextualPropertyMenu;

        private static void OnContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Color)
                return;

            Color color = property.colorValue;
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Copy as a C# code"), false,
                () => EditorGUIUtility.systemCopyBuffer = color.ToCode());
        }
    }
}
