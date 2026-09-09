using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Builds a default UITK inspector: fields grouped into <c>mie-block</c>s.
    /// <see cref="SpaceAttribute"/> starts a new block; <see cref="HeaderAttribute"/> does the same
    /// and adds a <c>mie-header</c> label. The built-in Header/Space decorators are hidden at the
    /// top level so they do not duplicate that chrome.
    /// </summary>
    public static class ManeInspectorLayout
    {
        public const string HeaderDecoratorClass = "mie-header-decorator";
        public const string SpaceDecoratorClass = "mie-space-decorator";

        private const string DecoratorContainerClass = "unity-decorator-drawers-container";
        private const string BlockClass = "mie-block";
        private const string HeaderClass = "mie-header";
        private const string ScriptPath = "m_Script";

        public static void Fill(VisualElement root, SerializedObject serializedObject)
        {
            VisualElement block = null;
            bool hasDataField = false;

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

                if (block == null || ((hasHeader || hasSpace) && hasDataField))
                {
                    block = CreateBlock();
                    root.Add(block);
                }

                if (hasHeader)
                {
                    foreach (HeaderAttribute header in headers)
                    {
                        Label label = new(header.header);
                        label.AddToClassList(HeaderClass);
                        block.Add(label);
                    }
                }

                PropertyField field = CreateField(property);
                HideTopLevelHeaderSpaceDecorators(field);
                block.Add(field);
                hasDataField = true;
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

        private static void HideTopLevelHeaderSpaceDecorators(PropertyField field)
        {
            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                int retries = 0;

                void TryHide()
                {
                    VisualElement container = FindDirectDecoratorContainer(field);
                    if (container == null)
                    {
                        if (retries++ < 10)
                            field.schedule.Execute(TryHide);
                        return;
                    }

                    bool anyVisible = false;
                    foreach (VisualElement child in container.Children())
                    {
                        if (child.ClassListContains(HeaderDecoratorClass) ||
                            child.ClassListContains(SpaceDecoratorClass) ||
                            child is IMGUIContainer)
                        {
                            child.style.display = DisplayStyle.None;
                            continue;
                        }

                        anyVisible = true;
                    }

                    if (!anyVisible)
                        container.style.display = DisplayStyle.None;
                }

                TryHide();
            });
        }

        private static VisualElement FindDirectDecoratorContainer(PropertyField field)
        {
            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(DecoratorContainerClass))
                    return child;

                foreach (VisualElement grandchild in child.Children())
                {
                    if (grandchild.ClassListContains(DecoratorContainerClass))
                        return grandchild;
                }
            }

            return null;
        }
    }
}
