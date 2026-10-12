using System;
using System.Collections.Generic;
using System.Reflection;
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
        // Root of Unity's own MinMaxCurve drawer, the same on 6.0 and later.
        private const string UnityMinMaxCurveRowClass = "unity-particle-system-min-max-curve__row";
        // Unity rebuilds a PropertyField's content on rebind, sometimes without a geometry change to notice it by.
        private const long ReplaceCheckIntervalMs = 200;

        private static HashSet<Type> _decoratorAttributes;

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

        // Path of the last field in the first block opened by a Foldout or Header with this text, or null.
        // A block ends at the next Header, Space, or Foldout, as in Fill and FillDefault.
        internal static string FindBlockEnd(SerializedObject serializedObject, string block)
        {
            if (serializedObject?.targetObject == null || string.IsNullOrEmpty(block))
                return null;

            string lastPath = null;
            bool inBlock = false;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == ScriptPath)
                    continue;

                FoldoutAttribute foldout = iterator.GetAttribute<FoldoutAttribute>();
                HeaderAttribute[] headers = iterator.GetAttributes<HeaderAttribute>();
                bool hasSpace = iterator.GetAttributes<SpaceAttribute>().Length > 0;

                if (foldout != null || headers.Length > 0 || hasSpace)
                {
                    if (inBlock)
                        return lastPath;

                    inBlock = foldout?.Header == block || Array.Exists(headers, header => header.header == block);
                }

                if (inBlock)
                    lastPath = iterator.propertyPath;
            }

            return inBlock ? lastPath : null;
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

        // MinMaxCurve fields and lists get MinMaxCurveField. With only layout attributes it is used directly:
        // Label renames the field; Prefix / Postfix are ignored on the row and only extend a list title.
        // With decorators (InfoBox, ReadOnly, AvailableIf...) it stays in a PropertyField, which runs them.
        // Any other property drawer attribute keeps the PropertyField as it is.
        private static VisualElement CreateManeField(SerializedProperty property)
        {
            bool isCurve = MinMaxCurveField.IsMinMaxCurve(property);
            bool isCurveArray = !isCurve && MinMaxCurveField.IsMinMaxCurveArray(property);
            if (!isCurve && !isCurveArray)
                return CreateField(property);

            bool decorated = false;
            foreach (PropertyAttribute attribute in property.GetAttributes<PropertyAttribute>())
            {
                if (IsLayoutAttribute(attribute))
                    continue;
                if (!IsDecoratorAttribute(attribute))
                    return CreateField(property);

                decorated = true;
            }

            if (decorated)
                return CreateDecoratedMinMaxCurve(property, isCurveArray);

            return isCurve ? CreateMinMaxCurveField(property) : CreateMinMaxCurveList(property);
        }

        private static bool IsLayoutAttribute(PropertyAttribute attribute) =>
            attribute is HeaderAttribute or SpaceAttribute or TooltipAttribute or FoldoutAttribute
                or LabelAttribute or PrefixAttribute or PostfixAttribute or NonBreakingSpaceAttribute;

        // Attributes drawn by a DecoratorDrawer. They draw around the field, so any field control can follow them.
        private static bool IsDecoratorAttribute(PropertyAttribute attribute)
        {
            if (_decoratorAttributes == null)
            {
                _decoratorAttributes = new HashSet<Type>();
                FieldInfo typeField = typeof(CustomPropertyDrawer).GetField("m_Type",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (Type drawer in TypeCache.GetTypesDerivedFrom<DecoratorDrawer>())
                {
                    foreach (CustomPropertyDrawer target in drawer.GetCustomAttributes<CustomPropertyDrawer>(false))
                    {
                        if (typeField?.GetValue(target) is Type attributeType)
                            _decoratorAttributes.Add(attributeType);
                    }
                }
            }

            return _decoratorAttributes.Contains(attribute.GetType());
        }

        // The PropertyField runs the decorators but draws the value with Unity's MinMaxCurve row (or a list of them).
        // That content is hidden and the ManeStyle field takes its place, again after every rebuild of the content.
        private static PropertyField CreateDecoratedMinMaxCurve(SerializedProperty property, bool isArray)
        {
            PropertyField field = CreateField(property);
            SerializedProperty tracked = property.Copy();
            VisualElement replacement = null;

            field.RegisterCallback<GeometryChangedEvent>(_ => Replace());
            field.schedule.Execute(Replace).Every(ReplaceCheckIntervalMs);
            return field;

            void Replace()
            {
                if (replacement != null && replacement.parent == field)
                    return;

                VisualElement unityContent = FindUnityContent(field, isArray);
                if (unityContent == null)
                    return;

                if (replacement == null)
                {
                    replacement = isArray ? CreateMinMaxCurveList(tracked) : CreateMinMaxCurveField(tracked);
                    // The host keeps the field name that EditorButton blocks look up.
                    replacement.name = null;
                }

                unityContent.style.display = DisplayStyle.None;
                // Before Unity's content, so decorators that look for the first label or list find this one.
                field.Insert(field.IndexOf(unityContent), replacement);
            }
        }

        // The direct child of the field that holds Unity's MinMaxCurve row or list, or null before it is built.
        private static VisualElement FindUnityContent(PropertyField field, bool isArray)
        {
            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(PropertyHost.DecoratorContainerClass))
                    continue;

                if (Contains(child, isArray))
                    return child;
            }

            return null;

            // Nested PropertyFields are the row's own parts, or list items: they are not searched.
            static bool Contains(VisualElement element, bool isArray)
            {
                if (isArray ? element is BaseListView : element.ClassListContains(UnityMinMaxCurveRowClass))
                    return true;
                if (element is PropertyField || element is MinMaxCurveField)
                    return false;

                foreach (VisualElement child in element.Children())
                {
                    if (Contains(child, isArray))
                        return true;
                }

                return false;
            }
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
