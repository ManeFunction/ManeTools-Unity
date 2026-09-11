using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(InterfaceOnlyAttribute))]
    public sealed class InterfaceOnlyDrawer : PropertyDrawer
    {
        private const string DisplayLabelClass = "unity-object-field-display__label";
        private const string AcceptDropClass = "unity-object-field-display--accept-drop";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType != SerializedPropertyType.ObjectReference)
            {
                return Decorations.CreateUnsupportedFieldTypeWarning(
                    tracked, nameof(InterfaceOnlyAttribute), "UnityEngine.Object reference");
            }

            InterfaceOnlyAttribute info = (InterfaceOnlyAttribute)attribute;
            if (!info.IsInterface)
            {
                string typeName = info.InterfaceType == null ? "null" : info.InterfaceType.Name;
                return Decorations.CreateWarningField(tracked,
                    $"[InterfaceOnly] requires an interface type, got '{typeName}'.");
            }

            Type fieldType = GetReferencedType();
            string noneLabel = FormatNoneLabel(fieldType, info.InterfaceType);
            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;

            ObjectField field = new(label)
            {
                objectType = fieldType,
                allowSceneObjects = true
            };
            field.AddToClassList(BaseField<UnityObject>.alignedFieldUssClassName);
            field.BindProperty(tracked);

            VisualElement warning = InfoBoxDrawer.Create(string.Empty, InfoBoxType.Warning);
            Label warningLabel = warning.Q<Label>("label");
            warning.style.display = DisplayStyle.None;

            bool applying = false;

            SerializedProperty LiveProperty()
            {
                string path = field.bindingPath;
                if (string.IsNullOrEmpty(path))
                    return tracked;

                return tracked.serializedObject.FindProperty(path) ?? tracked;
            }

            void Refresh()
            {
                SerializedProperty live = LiveProperty();
                if (live == null)
                    return;

                UpdateWarning(live, info.InterfaceType, fieldType, warning, warningLabel);
                UpdateNoneLabel(field, noneLabel);
            }

            void ApplyAssignment(UnityObject assigned)
            {
                if (applying)
                    return;

                SerializedProperty live = LiveProperty();
                if (live == null)
                    return;

                if (!TryResolve(assigned, info.InterfaceType, fieldType, out UnityObject resolved))
                {
                    applying = true;
                    live.objectReferenceValue = null;
                    live.serializedObject.ApplyModifiedProperties();
                    field.SetValueWithoutNotify(null);
                    applying = false;
                    Refresh();
                    return;
                }

                if (resolved != live.objectReferenceValue)
                {
                    applying = true;
                    live.objectReferenceValue = resolved;
                    live.serializedObject.ApplyModifiedProperties();
                    field.SetValueWithoutNotify(resolved);
                    applying = false;
                }

                Refresh();
            }

            field.RegisterValueChangedCallback(evt => ApplyAssignment(evt.newValue));
            field.TrackSerializedObjectValue(tracked.serializedObject, _ => Refresh());
            field.RegisterCallback<GeometryChangedEvent>(_ => UpdateNoneLabel(field, noneLabel));
            field.RegisterCallback<AttachToPanelEvent>(_ => Refresh());
            field.schedule.Execute(Refresh);
            RegisterDragFilter(field, info.InterfaceType, fieldType);

            VisualElement root = new();
            root.Add(field);
            root.Add(warning);
            return root;
        }

        private Type GetReferencedType()
        {
            Type type = fieldInfo.FieldType;
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private static void UpdateWarning(
            SerializedProperty property,
            Type interfaceType,
            Type fieldType,
            VisualElement warning,
            Label warningLabel)
        {
            if (!TryGetWarning(property, interfaceType, fieldType, out string message))
            {
                warning.style.display = DisplayStyle.None;
                return;
            }

            if (warningLabel != null)
                warningLabel.text = message;

            warning.style.display = DisplayStyle.Flex;
        }

        private static void RegisterDragFilter(ObjectField field, Type interfaceType, Type fieldType)
        {
            void RejectIfInvalid(EventBase evt)
            {
                UnityObject[] refs = DragAndDrop.objectReferences;
                if (refs == null || refs.Length == 0)
                    return;
                if (CanAcceptDrag(interfaceType, fieldType, refs))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.None;
                ClearAcceptDrop(field);
                evt.StopImmediatePropagation();
            }

            field.RegisterCallback<DragEnterEvent>(RejectIfInvalid, TrickleDown.TrickleDown);
            field.RegisterCallback<DragUpdatedEvent>(RejectIfInvalid, TrickleDown.TrickleDown);
            field.RegisterCallback<DragPerformEvent>(RejectIfInvalid, TrickleDown.TrickleDown);
        }

        private static bool CanAcceptDrag(Type interfaceType, Type fieldType, UnityObject[] refs)
        {
            foreach (UnityObject obj in refs)
            {
                if (obj == null)
                    continue;
                if (TryResolve(obj, interfaceType, fieldType, out UnityObject resolved) && resolved != null)
                    return true;
            }

            return false;
        }

        private static void ClearAcceptDrop(VisualElement field)
        {
            foreach (VisualElement element in field.Query(className: AcceptDropClass).ToList())
                element.RemoveFromClassList(AcceptDropClass);
        }

        private static string FormatNoneLabel(Type fieldType, Type interfaceType) =>
            $"None ({fieldType.Name}:{interfaceType.Name})";

        private static void UpdateNoneLabel(ObjectField field, string noneLabel)
        {
            if (field.value != null || field.showMixedValue)
                return;

            Label display = field.Q<Label>(className: DisplayLabelClass);
            if (display == null)
                return;

            if (display.text != noneLabel)
                display.text = noneLabel;
        }

        private static bool TryGetWarning(
            SerializedProperty property, Type interfaceType, Type fieldType, out string message)
        {
            message = null;
            UnityObject obj = property.objectReferenceValue;
            if (obj == null)
                return false;

            if (!interfaceType.IsAssignableFrom(obj.GetType()))
            {
                message = $"Reference must implement {interfaceType.Name}.";
                return true;
            }

            if (!fieldType.IsAssignableFrom(obj.GetType()))
            {
                message = $"Reference must be assignable to {fieldType.Name}.";
                return true;
            }

            return false;
        }

        private static bool TryResolve(
            UnityObject obj, Type interfaceType, Type fieldType, out UnityObject resolved)
        {
            resolved = null;
            if (obj == null)
                return true;

            if (Matches(obj, interfaceType, fieldType))
            {
                resolved = obj;
                return true;
            }

            if (obj is GameObject go)
            {
                foreach (Component component in go.GetComponents<Component>())
                {
                    if (component != null && Matches(component, interfaceType, fieldType))
                    {
                        resolved = component;
                        return true;
                    }
                }

                return false;
            }

            if (obj is Component source)
            {
                foreach (Component component in source.gameObject.GetComponents<Component>())
                {
                    if (component != null && Matches(component, interfaceType, fieldType))
                    {
                        resolved = component;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool Matches(UnityObject obj, Type interfaceType, Type fieldType) =>
            interfaceType.IsAssignableFrom(obj.GetType()) && fieldType.IsAssignableFrom(obj.GetType());
    }
}
