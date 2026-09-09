using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(DropdownListAttribute))]
    internal sealed class DropdownListPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType is not SerializedPropertyType.String and not SerializedPropertyType.Integer)
            {
                VisualElement root = new();
                root.Add(InfoBoxDrawer.Create(
                    $"[DropdownList] can only be applied to string or int fields.\n'{tracked.displayName}' is {tracked.propertyType}.",
                    InfoBoxType.Warning));
                root.Add(new PropertyField(tracked));
                return root;
            }

            DropdownListAttribute info = (DropdownListAttribute)attribute;
            string[] list = info.GetStrings(property.serializedObject.targetObject);
            if (list == null || list.Length == 0)
                return new PropertyField(tracked);

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;
            bool isString = tracked.propertyType == SerializedPropertyType.String;
            DropdownField dropdown = CreateDropdown(label, list, CurrentIndex());

            dropdown.RegisterValueChangedCallback(evt =>
            {
                if (isString)
                {
                    if (tracked.stringValue == evt.newValue)
                        return;

                    tracked.stringValue = evt.newValue;
                }
                else
                {
                    if (tracked.intValue == dropdown.index)
                        return;

                    tracked.intValue = dropdown.index;
                }

                tracked.serializedObject.ApplyModifiedProperties();
            });
            dropdown.TrackPropertyValue(tracked, _ => ApplyDisplay());

            if (info.CanRefresh)
            {
                dropdown.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    if (evt.menu.MenuItems().Count > 0)
                        evt.menu.AppendSeparator();

                    evt.menu.AppendAction("Refresh Options", _ => Refresh());
                }));
            }

            return dropdown;

            int CurrentIndex() => isString
                ? Mathf.Max(0, Array.IndexOf(list, tracked.stringValue))
                : Mathf.Clamp(tracked.intValue, 0, list.Length - 1);

            void ApplyDisplay()
            {
                string value = list[CurrentIndex()];
                if (dropdown.value != value)
                    dropdown.SetValueWithoutNotify(value);
            }

            void Refresh()
            {
                string[] next = info.GetStrings(tracked.serializedObject.targetObject);
                if (next == null || next.Length == 0)
                    return;

                list = next;
                dropdown.choices = new List<string>(list);
                ApplyDisplay();
            }
        }

        private static DropdownField CreateDropdown(string label, string[] list, int index)
        {
            DropdownField dropdown = new(label, new List<string>(list), index);
            dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            return dropdown;
        }
    }
}
