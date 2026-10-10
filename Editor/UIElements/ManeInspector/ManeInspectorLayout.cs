using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Builds inspector field layout.
    /// <see cref="Fill"/> is the ManeStyle path: <c>mie-block</c>s, headers, and <see cref="FoldoutBlock"/>;
    /// <see cref="NonBreakingSpaceAttribute"/> adds spacing without starting a new block.
    /// <see cref="FillDefault"/> is Unity's default look with a stock <see cref="Foldout"/> for
    /// <see cref="FoldoutAttribute"/>.
    /// </summary>
    public static class ManeInspectorLayout
    {
        private const string BlockClass = "mie-block";
        private const string ScriptPath = "m_Script";

        /// <summary>
        /// Fills <paramref name="root"/> with ManeStyle blocks, headers, and foldouts.
        /// </summary>
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

                VisualElement field = CreateManeField(property);

                NonBreakingSpaceAttribute nonBreakingSpace = property.GetAttribute<NonBreakingSpaceAttribute>();
                if (nonBreakingSpace != null)
                    block.Add(NonBreakingSpaceDrawer.Create(nonBreakingSpace.Height));

                if (field is PropertyField propertyField)
                {
                    if (hasHeader)
                        HeaderDrawer.HideUnityDecorator(propertyField);
                    if (hasSpace)
                        SpaceDrawer.HideUnityDecorator(propertyField);
                    if (nonBreakingSpace != null)
                        NonBreakingSpaceDrawer.HideUnityDecorator(propertyField);
                }

                block.Add(field);
            }
        }

        /// <summary>
        /// True if any visible field has <see cref="FoldoutAttribute"/>.
        /// </summary>
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

        /// <summary>
        /// Fills <paramref name="root"/> with Unity default fields, using a stock foldout for <see cref="FoldoutAttribute"/>.
        /// </summary>
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

        // MinMaxCurve fields and lists get MinMaxCurveField, unless other property attributes need the PropertyField path.
        // Label renames the field; Prefix / Postfix are ignored on the row and only extend a list title.
        private static VisualElement CreateManeField(SerializedProperty property)
        {
            if (!HasOnlyLayoutAttributes(property))
                return CreateField(property);

            if (MinMaxCurveField.IsMinMaxCurve(property))
                return CreateMinMaxCurveField(property);

            if (MinMaxCurveField.IsMinMaxCurveArray(property))
                return CreateMinMaxCurveList(property);

            return CreateField(property);
        }

        private static bool HasOnlyLayoutAttributes(SerializedProperty property)
        {
            foreach (PropertyAttribute attribute in property.GetAttributes<PropertyAttribute>())
            {
                if (attribute is not (HeaderAttribute or SpaceAttribute or TooltipAttribute or FoldoutAttribute
                    or LabelAttribute or PrefixAttribute or PostfixAttribute or NonBreakingSpaceAttribute))
                    return false;
            }

            return true;
        }

        private static MinMaxCurveField CreateMinMaxCurveField(SerializedProperty property)
        {
            MinMaxCurveField field = new(PropertyHost.DisplayName(property))
            {
                name = "PropertyField:" + property.propertyPath,
                tooltip = property.tooltip
            };
            field.BindProperty(property);
            return field;
        }

        private static ListView CreateMinMaxCurveList(SerializedProperty property)
        {
            SerializedProperty array = property.Copy();
            string typeName = property.serializedObject.targetObject.GetType().FullName;

            ListView list = new()
            {
                name = "PropertyField:" + property.propertyPath,
                headerTitle = PropertyHost.CollectionTitle(property),
                tooltip = property.tooltip,
                viewDataKey = "Mane.List." + typeName + "." + property.propertyPath,
                showFoldoutHeader = true,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = true,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                selectionType = SelectionType.Multiple,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeItem = () => new MinMaxCurveField(),
                bindItem = (element, index) =>
                {
                    if (index < array.arraySize)
                        ((MinMaxCurveField)element).BindProperty(array.GetArrayElementAtIndex(index));
                }
            };
            list.BindProperty(array);
            return list;
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
