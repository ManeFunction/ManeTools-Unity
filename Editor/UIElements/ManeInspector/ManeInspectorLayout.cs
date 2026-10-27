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
    /// <c>Fill</c> is the ManeStyle path: <c>mie-block</c>s, headers, <see cref="FoldoutBlock"/>, and
    /// <see cref="FeatureBlock"/> for <see cref="FeatureToggleAttribute"/>;
    /// <see cref="NonBreakingSpaceAttribute"/> adds spacing without starting a new block.
    /// <see cref="FillDefault"/> is Unity's default look with a stock <see cref="Foldout"/> for
    /// <see cref="FoldoutAttribute"/>.
    /// </summary>
    public static class ManeInspectorLayout
    {
        private const string BlockClass = "mie-block";
        private const string ScriptPath = "m_Script";
        // Unity's own name for the PropertyFields of a default inspector.
        private const string FieldNamePrefix = "PropertyField:";
        // Root of Unity's own MinMaxCurve drawer, the same on 6.0 and later.
        private const string UnityMinMaxCurveRowClass = "unity-particle-system-min-max-curve__row";
        // Unity rebuilds a PropertyField's content on rebind, sometimes without a geometry change to notice it by.
        private const long ReplaceCheckIntervalMs = 200;

        private static HashSet<Type> _decoratorAttributes;
        private static List<(Type Drawn, bool UseForChildren)> _typeDrawers;

        /// <summary>
        /// Fills <paramref name="root"/> with ManeStyle blocks, headers, foldouts, and feature blocks.
        /// </summary>
        public static void Fill(VisualElement root, SerializedObject serializedObject) =>
            Fill(root, serializedObject, null);

        /// <summary>
        /// Fills <paramref name="root"/> with ManeStyle blocks, headers, foldouts, and feature blocks,
        /// starting from the property at <paramref name="firstProperty"/>. Fields before it are skipped,
        /// so a custom inspector can draw them itself. Null or empty starts from the first field.
        /// </summary>
        public static void Fill(VisualElement root, SerializedObject serializedObject, string firstProperty)
        {
            VisualElement block = null;

            foreach (LayoutEntry entry in Walk(serializedObject, firstProperty))
            {
                SerializedProperty property = entry.Property;

                if (block == null || entry.StartsBlock)
                {
                    block = entry.Feature != null ? CreateFeatureBlock(entry.Feature, property)
                        : entry.Foldout != null ? CreateFoldout(entry.Foldout, property)
                        : CreateBlock();
                    root.Add(block);
                }

                // The toggle of a feature block is the field itself; the fields after it go to the block content.
                if (entry.Feature != null)
                    continue;

                bool hasHeader = entry.Headers.Length > 0;
                if (hasHeader && entry.Foldout == null)
                {
                    foreach (HeaderAttribute header in entry.Headers)
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
                    if (entry.HasSpace)
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

            foreach (LayoutEntry entry in Walk(serializedObject))
            {
                if (entry.Foldout != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The name of the element drawn for the field at <paramref name="propertyPath"/>, so it can be found with
        /// <c>root.Q(FieldName(path))</c>. It is Unity's own <c>PropertyField:</c> name, so it is the same in
        /// <c>Fill</c>, <see cref="FillDefault"/>, and Unity's default inspector.
        /// </summary>
        public static string FieldName(string propertyPath) => FieldNamePrefix + propertyPath;

        // Path of the last field in the first block whose title is this text, or null.
        internal static string FindBlockEnd(SerializedObject serializedObject, string block)
        {
            if (serializedObject?.targetObject == null || string.IsNullOrEmpty(block))
                return null;

            string lastPath = null;
            bool inBlock = false;

            foreach (LayoutEntry entry in Walk(serializedObject))
            {
                if (entry.StartsBlock)
                {
                    if (inBlock)
                        return lastPath;

                    inBlock = entry.HasTitle(block);
                }

                if (inBlock)
                    lastPath = entry.Property.propertyPath;
            }

            return lastPath;
        }

        /// <summary>
        /// Fills <paramref name="root"/> with Unity default fields, using a stock foldout for <see cref="FoldoutAttribute"/>.
        /// </summary>
        public static void FillDefault(VisualElement root, SerializedObject serializedObject)
        {
            SerializedProperty script = serializedObject.FindProperty(ScriptPath);
            if (script != null)
            {
                PropertyField scriptField = CreateField(script);
                scriptField.SetEnabled(false);
                root.Add(scriptField);
            }

            VisualElement container = root;

            foreach (LayoutEntry entry in Walk(serializedObject))
            {
                if (entry.Foldout != null)
                {
                    Foldout element = CreateUnityFoldout(entry.Foldout, entry.Property);
                    root.Add(element);
                    container = element;
                }
                else if (entry.StartsBlock)
                    container = root;

                container.Add(CreateField(entry.Property));
            }
        }

        // Visible fields from firstProperty on (from the first one when it is null or empty), without the script field.
        private static IEnumerable<LayoutEntry> Walk(SerializedObject serializedObject, string firstProperty = null)
        {
            bool started = string.IsNullOrEmpty(firstProperty);

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == ScriptPath)
                    continue;

                if (!started)
                {
                    if (iterator.propertyPath != firstProperty)
                        continue;

                    started = true;
                }

                yield return new LayoutEntry(iterator.Copy());
            }
        }

        private static VisualElement CreateBlock()
        {
            VisualElement block = new();
            block.AddToClassList(BlockClass);
            return block;
        }

        // The attribute only counts on a bool, which is what the block toggle binds to.
        private static FeatureToggleAttribute GetFeatureToggle(SerializedProperty property) =>
            property.propertyType == SerializedPropertyType.Boolean ? property.GetAttribute<FeatureToggleAttribute>() : null;

        // Without a header text, the field name (or its Label text).
        internal static string FeatureTitle(FeatureToggleAttribute feature, SerializedProperty property) =>
            string.IsNullOrEmpty(feature.Header) ? PropertyHost.DisplayName(property) : feature.Header;

        private static FeatureBlock CreateFeatureBlock(FeatureToggleAttribute feature, SerializedProperty property)
        {
            FeatureBlock block = new() { Header = FeatureTitle(feature, property) };
            // Named as the field, so a lookup by path finds the block even when it has no other field.
            block.Toggle.name = FieldName(property.propertyPath);
            block.Toggle.tooltip = property.tooltip;
            block.BindToggle(property);
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
        // A plain serializable class with only layout attributes is drawn as a group of ChildFields,
        // the same as a SerializeInterface field without its dropdown.
        private static VisualElement CreateManeField(SerializedProperty property)
        {
            bool isCurve = MinMaxCurveField.IsMinMaxCurve(property);
            bool isCurveArray = !isCurve && MinMaxCurveField.IsMinMaxCurveArray(property);
            if (!isCurve && !isCurveArray)
                return IsNestedClass(property) ? CreateNestedClass(property) : CreateField(property);

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

        private static VisualElement CreateNestedClass(SerializedProperty property)
        {
            VisualElement group = SerializeInterfaceDrawer.CreateGroup(property, PropertyHost.DisplayName(property));
            group.name = FieldName(property.propertyPath);
            group.tooltip = property.tooltip;
            return group;
        }

        // A serializable class or struct with fields, no property attribute but layout ones (decorators need the
        // PropertyField that hosts them), and no property drawer of its own for its type.
        private static bool IsNestedClass(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Generic || property.isArray || !property.hasVisibleChildren)
                return false;

            foreach (PropertyAttribute attribute in property.GetAttributes<PropertyAttribute>())
            {
                if (!IsLayoutAttribute(attribute))
                    return false;
            }

            Type type = MemberLookup.GetField(property)?.FieldType;
            return type != null && !HasTypeDrawer(type);
        }

        // True if a CustomPropertyDrawer is declared for the type, or for a base type with useForChildren.
        private static bool HasTypeDrawer(Type type)
        {
            if (_typeDrawers == null)
            {
                _typeDrawers = new List<(Type, bool)>();
                FieldInfo typeField = typeof(CustomPropertyDrawer).GetField("m_Type",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo childrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (Type drawer in TypeCache.GetTypesDerivedFrom<PropertyDrawer>())
                {
                    foreach (CustomPropertyDrawer target in drawer.GetCustomAttributes<CustomPropertyDrawer>(false))
                    {
                        if (typeField?.GetValue(target) is Type drawn)
                            _typeDrawers.Add((drawn, childrenField?.GetValue(target) is true));
                    }
                }
            }

            Type definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            foreach ((Type drawn, bool useForChildren) in _typeDrawers)
            {
                if (drawn == type || drawn == definition)
                    return true;
                if (useForChildren && drawn.IsAssignableFrom(type))
                    return true;
            }

            return false;
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
                    // The host keeps the field name, so a lookup by path finds one element.
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
                name = FieldName(property.propertyPath),
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
                name = FieldName(property.propertyPath),
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
                name = FieldName(property.propertyPath)
            };
            field.Bind(property.serializedObject);
            return field;
        }

        // One visible field and the layout attributes that decide which block it goes to.
        private readonly struct LayoutEntry
        {
            public readonly SerializedProperty Property;
            public readonly FoldoutAttribute Foldout;
            public readonly FeatureToggleAttribute Feature;
            public readonly HeaderAttribute[] Headers;
            public readonly bool HasSpace;

            public LayoutEntry(SerializedProperty property)
            {
                Property = property;
                Foldout = property.GetAttribute<FoldoutAttribute>();
                Feature = GetFeatureToggle(property);
                Headers = property.GetAttributes<HeaderAttribute>();
                HasSpace = property.GetAttributes<SpaceAttribute>().Length > 0;
            }

            // A Header, Space, Foldout, or FeatureToggle starts a new block; NonBreakingSpace does not.
            public bool StartsBlock => Foldout != null || Feature != null || Headers.Length > 0 || HasSpace;

            // The title Fill shows for the block this field starts: the feature title over the foldout header,
            // and either of them over the field's headers, which Fill does not draw then.
            public bool HasTitle(string title)
            {
                if (Feature != null)
                    return FeatureTitle(Feature, Property) == title;
                if (Foldout != null)
                    return Foldout.Header == title;

                foreach (HeaderAttribute header in Headers)
                {
                    if (header.header == title)
                        return true;
                }

                return false;
            }
        }
    }
}
