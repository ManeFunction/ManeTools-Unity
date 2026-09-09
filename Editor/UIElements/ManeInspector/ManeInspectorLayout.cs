using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Builds inspector field layout.
    /// <see cref="Fill"/> is the ManeStyle path: <c>mie-block</c>s, headers, and <see cref="FoldoutBlock"/>.
    /// <see cref="FillDefault"/> is Unity's default look with a stock <see cref="Foldout"/> for
    /// <see cref="FoldoutAttribute"/>.
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

                FoldoutAttribute foldout = property.GetAttribute<FoldoutAttribute>();
                HeaderAttribute[] headers = property.GetAttributes<HeaderAttribute>();
                bool hasFoldout = foldout != null;
                bool hasHeader = headers.Length > 0;
                bool hasSpace = property.GetAttributes<SpaceAttribute>().Length > 0;

                if (block == null || hasHeader || hasSpace || hasFoldout)
                {
                    block = hasFoldout ? CreateFoldout(foldout, property) : CreateBlock();
                    root.Add(block);
                }

                if (hasHeader && !hasFoldout)
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

        public static bool HasFoldout(SerializedObject serializedObject)
        {
            if (serializedObject?.targetObject == null)
                return false;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == ScriptPath)
                    continue;

                if (iterator.GetAttribute<FoldoutAttribute>() != null)
                    return true;
            }

            return false;
        }

        public static void FillDefault(VisualElement root, SerializedObject serializedObject)
        {
            VisualElement container = root;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                SerializedProperty property = iterator.Copy();

                if (property.propertyPath == ScriptPath)
                {
                    PropertyField scriptField = CreateField(property);
                    scriptField.SetEnabled(false);
                    root.Add(scriptField);
                    continue;
                }

                FoldoutAttribute foldout = property.GetAttribute<FoldoutAttribute>();
                bool hasHeader = property.GetAttributes<HeaderAttribute>().Length > 0;
                bool hasSpace = property.GetAttributes<SpaceAttribute>().Length > 0;

                if (foldout != null)
                {
                    Foldout element = CreateUnityFoldout(foldout, property);
                    root.Add(element);
                    container = element;
                }
                else if (hasHeader || hasSpace)
                    container = root;

                container.Add(CreateField(property));
            }
        }

        private static VisualElement CreateBlock()
        {
            VisualElement block = new();
            block.AddToClassList(BlockClass);
            return block;
        }

        private static FoldoutBlock CreateFoldout(FoldoutAttribute foldout, SerializedProperty property)
        {
            return new FoldoutBlock
            {
                name = property.propertyPath,
                text = foldout.Header
            };
        }

        private static Foldout CreateUnityFoldout(FoldoutAttribute foldout, SerializedProperty property)
        {
            string typeName = property.serializedObject.targetObject.GetType().FullName;
            return new Foldout
            {
                text = foldout.Header,
                value = true,
                viewDataKey = "Mane.Foldout." + typeName + "." + property.propertyPath
            };
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
