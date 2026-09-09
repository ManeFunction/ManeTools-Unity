using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Builds a default UITK inspector: fields grouped into <c>mie-block</c>s.
    /// <see cref="SpaceAttribute"/> starts a new block; <see cref="HeaderAttribute"/> does the same
    /// and adds a <c>mie-header</c> label. Unity's built-in Header/Space drawers are hidden on those
    /// fields so they do not duplicate that chrome.
    /// </summary>
    public static class ManeInspectorLayout
    {
        private const string BlockClass = "mie-block";
        private const string ScriptPath = "m_Script";

        public static void Fill(VisualElement root, SerializedObject serializedObject)
        {
            VisualElement block = null;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                SerializedProperty property = iterator.Copy();

                if (property.propertyPath == ScriptPath)
                    continue;

                HeaderAttribute[] headers = property.GetAttributes<HeaderAttribute>();
                bool hasHeader = headers.Length > 0;
                bool hasSpace = property.GetAttributes<SpaceAttribute>().Length > 0;

                if (block == null || hasHeader || hasSpace)
                {
                    block = CreateBlock();
                    root.Add(block);
                }

                if (hasHeader)
                {
                    foreach (HeaderAttribute header in headers)
                        block.Add(HeaderDrawer.Create(header.header));
                }

                PropertyField field = CreateField(property);
                if (hasHeader)
                    HeaderDrawer.HideUnityDecorator(field);
                if (hasSpace)
                    SpaceDrawer.HideUnityDecorator(field);

                block.Add(field);
            }
        }

        private static VisualElement CreateBlock()
        {
            VisualElement block = new();
            block.AddToClassList(BlockClass);
            return block;
        }

        private static PropertyField CreateField(SerializedProperty property)
        {
            PropertyField field = new(property)
            {
                name = "PropertyField:" + property.propertyPath
            };
            field.Bind(property.serializedObject);
            return field;
        }
    }
}
