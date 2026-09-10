using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SerializeInterfaceAttribute))]
    internal sealed class SerializeInterfaceDrawer : PropertyDrawer
    {
        private const string RootClass = "mie-serialize-interface";
        private const string RootFieldsClass = "mie-serialize-interface--fields";
        private const string FieldClass = "mie-serialize-interface__field";
        private const string FoldoutClass = "mie-serialize-interface--foldout";
        private const string ArrowClass = "mie-serialize-interface__arrow";
        private const string TypeClass = "mie-serialize-interface__type";
        private const string ChildrenClass = "mie-serialize-interface__children";
        private const string NoneLabel = "None";

        private static StyleSheet _sheet;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType != SerializedPropertyType.ManagedReference)
            {
                return Decorations.CreateUnsupportedFieldTypeWarning(
                    tracked, "SerializeInterface", "[SerializeReference] fields");
            }

            Type baseType = Unwrap(fieldInfo?.FieldType) ?? ParseType(tracked.managedReferenceFieldTypename);
            if (baseType == null)
            {
                return Decorations.CreateUnsupportedFieldTypeWarning(
                    tracked, "SerializeInterface", "an interface or class");
            }

            Type[] types = CollectTypes(baseType);
            List<string> choices = BuildChoices(types);
            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;

            VisualElement root = new();
            root.AddToClassList(RootClass);
            ApplySheet(root);

            DropdownField dropdown = new(choices, CurrentIndex(tracked, types));
            dropdown.AddToClassList(TypeClass);

            SerializeInterfaceField field = new(label, dropdown);
            field.AddToClassList(FieldClass);
            root.Add(field);

            Toggle arrow = new() { value = true, viewDataKey = ViewDataKey(tracked) };
            arrow.pickingMode = PickingMode.Ignore;
            arrow.AddToClassList(ArrowClass);
            arrow.AddToClassList(Foldout.toggleUssClassName);

            VisualElement children = new();
            children.AddToClassList(ChildrenClass);
            root.Add(children);

            string lastTypeName = tracked.managedReferenceFullTypename;

            void SyncChildren()
            {
                bool hasChildren = HasVisibleChildren(tracked);
                field.EnableInClassList(FoldoutClass, hasChildren);

                if (!hasChildren)
                {
                    arrow.RemoveFromHierarchy();
                    children.Clear();
                    children.style.display = DisplayStyle.None;
                    return;
                }

                if (arrow.parent == null)
                    field.hierarchy.Insert(0, arrow);

                children.Clear();
                AddChildren(children, tracked);
                children.style.display = arrow.value ? DisplayStyle.Flex : DisplayStyle.None;
            }

            void SyncType()
            {
                int index = CurrentIndex(tracked, types);
                if (dropdown.index != index)
                    dropdown.SetValueWithoutNotify(choices[index]);

                lastTypeName = tracked.managedReferenceFullTypename;
                SyncChildren();
            }

            dropdown.RegisterValueChangedCallback(_ =>
            {
                if (!ApplyType(tracked, types, dropdown.index))
                    return;

                lastTypeName = tracked.managedReferenceFullTypename;
                root.schedule.Execute(SyncChildren);
            });
            void ApplyExpanded(bool expanded) =>
                children.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;

            arrow.RegisterValueChangedCallback(evt => ApplyExpanded(evt.newValue));
            arrow.RegisterCallback<AttachToPanelEvent>(_ => ApplyExpanded(arrow.value));
            field.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (!field.ClassListContains(FoldoutClass))
                    return;
                if (evt.position.x > field.labelElement.worldBound.xMax)
                    return;

                arrow.value = !arrow.value;
                evt.StopImmediatePropagation();
            }, TrickleDown.TrickleDown);

            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                root.EnableInClassList(RootFieldsClass, HasFieldsAncestor(root));
                HideHostLabel(root);
            });

            SyncChildren();
            root.TrackPropertyValue(tracked, _ =>
            {
                string typeName = tracked.managedReferenceFullTypename;
                if (typeName == lastTypeName)
                    return;

                lastTypeName = typeName;
                root.schedule.Execute(SyncType);
            });

            return root;
        }

        private static void HideHostLabel(VisualElement root)
        {
            PropertyField host = root.GetFirstAncestorOfType<PropertyField>();
            if (host == null)
                return;

            host.Query<Label>(className: PropertyField.labelUssClassName).ForEach(label =>
            {
                if (IsUnder(label, root))
                    return;

                label.style.display = DisplayStyle.None;
            });
        }

        private static bool IsUnder(VisualElement element, VisualElement ancestor)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current == ancestor)
                    return true;
            }

            return false;
        }

        private static bool ApplyType(SerializedProperty property, Type[] types, int selected)
        {
            if (selected == CurrentIndex(property, types))
                return false;

            if (selected <= 0)
                property.managedReferenceValue = null;
            else
                property.managedReferenceValue = Activator.CreateInstance(types[selected - 1]);

            property.serializedObject.ApplyModifiedProperties();
            return true;
        }

        private static void AddChildren(VisualElement parent, SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            if (!iterator.NextVisible(true))
                return;

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                PropertyField child = new(iterator.Copy())
                {
                    name = "PropertyField:" + iterator.propertyPath
                };
                child.Bind(property.serializedObject);
                parent.Add(child);

                if (!iterator.NextVisible(false))
                    break;
            }
        }

        private static bool HasVisibleChildren(SerializedProperty property)
        {
            if (property.managedReferenceValue == null)
                return false;

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            return iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, end);
        }

        private static int CurrentIndex(SerializedProperty property, Type[] types)
        {
            object value = property.managedReferenceValue;
            if (value == null)
                return 0;

            int index = Array.IndexOf(types, value.GetType());
            return index >= 0 ? index + 1 : 0;
        }

        private static Type[] CollectTypes(Type baseType)
        {
            List<Type> types = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (type.IsAbstract || type.IsGenericType || typeof(UnityEngine.Object).IsAssignableFrom(type))
                    continue;

                types.Add(type);
            }

            return types.ToArray();
        }

        private static List<string> BuildChoices(Type[] types)
        {
            List<string> choices = new(types.Length + 1) { NoneLabel };
            choices.AddRange(types.Select(t => t.Name));

            return choices;
        }

        private static Type Unwrap(Type type)
        {
            if (type == null)
                return null;
            if (type.IsArray)
                return type.GetElementType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private static Type ParseType(string assemblyAndType)
        {
            if (string.IsNullOrEmpty(assemblyAndType))
                return null;

            int split = assemblyAndType.IndexOf(' ');
            if (split < 0)
                return null;

            return Type.GetType($"{assemblyAndType[(split + 1)..]}, {assemblyAndType[..split]}");
        }

        private static string ViewDataKey(SerializedProperty property)
        {
            string typeName = property.serializedObject.targetObject.GetType().FullName;
            return "Mane.SerializeInterface." + typeName + "." + property.propertyPath;
        }

        private static bool HasFieldsAncestor(VisualElement element)
        {
            for (VisualElement current = element.parent; current != null; current = current.parent)
            {
                if (current.ClassListContains(ManeEditorStyles.FieldsClass))
                    return true;
            }

            return false;
        }

        private static void ApplySheet(VisualElement root)
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("SerializeInterfaceDrawer.uss was not found next to SerializeInterfaceDrawer.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(SerializeInterfaceDrawer));

        private sealed class SerializeInterfaceField : BaseField<int>
        {
            public SerializeInterfaceField(string label, VisualElement visualInput) : base(label, visualInput)
            {
                AddToClassList(alignedFieldUssClassName);
            }
        }
    }
}
