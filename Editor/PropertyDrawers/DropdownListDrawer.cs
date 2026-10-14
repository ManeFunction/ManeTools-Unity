using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(DropdownListAttribute))]
    internal sealed class DropdownListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType is not SerializedPropertyType.String and not SerializedPropertyType.Integer)
                return Decorations.CreateUnsupportedFieldTypeWarning(tracked, "DropdownList", "string or int");

            DropdownListAttribute info = (DropdownListAttribute)attribute;
            string[] list = GetOptions(tracked, info);
            if (list == null || list.Length == 0)
                return new PropertyField(tracked);

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;
            bool isString = tracked.propertyType == SerializedPropertyType.String;
            DropdownField dropdown = CreateDropdown(label, list);
            ApplyDisplay();

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

            if (IsDynamic(info))
            {
                dropdown.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    if (evt.menu.MenuItems().Count > 0)
                        evt.menu.AppendSeparator();

                    evt.menu.AppendAction("Refresh Options", _ => Refresh());
                }));
            }

            return dropdown;

            // -1 when the stored value is not one of the options (unset, removed, out of range).
            int CurrentIndex() => isString
                ? Array.IndexOf(list, tracked.stringValue)
                : tracked.intValue >= 0 && tracked.intValue < list.Length ? tracked.intValue : -1;

            // A value that is not an option shows as empty, so picking any option, the first one included,
            // is a change and gets written.
            void ApplyDisplay()
            {
                int index = CurrentIndex();
                string value = index >= 0 ? list[index] : null;
                if (dropdown.value != value)
                    dropdown.SetValueWithoutNotify(value);
            }

            void Refresh()
            {
                string[] next = GetOptions(tracked, info);
                if (next == null || next.Length == 0)
                    return;

                list = next;
                dropdown.choices = new List<string>(list);
                ApplyDisplay();
            }
        }

        private static bool IsDynamic(DropdownListAttribute info) =>
            info.SourceType != null && !string.IsNullOrEmpty(info.MemberName);

        // Fixed strings, or the member's value. A missing member is logged once and gives no options.
        private static string[] GetOptions(SerializedProperty property, DropdownListAttribute info)
        {
            if (!IsDynamic(info))
                return info.Strings;

            UnityEngine.Object target = property.serializedObject.targetObject;
            if (MemberLookup.TryGetValue(property, target, info.MemberName, out string[] options, info.SourceType))
                return options;

            MemberLookup.ReportMissing(info.SourceType, info.MemberName, "DropdownList",
                "a string[] field, property or parameterless method");
            return null;
        }

        private static DropdownField CreateDropdown(string label, string[] list)
        {
            DropdownField dropdown = new(label, new List<string>(list), 0);
            dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            return dropdown;
        }
    }
}
