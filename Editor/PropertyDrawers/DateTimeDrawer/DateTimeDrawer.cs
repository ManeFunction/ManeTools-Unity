using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    internal sealed class DateTimeDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            SerializedProperty value = tracked.FindPropertyRelative(SerializableDateTime.PropertyName);
            if (value == null)
            {
                return Decorations.CreateWarningField(tracked,
                    "[SerializableDateTime] backing string was not found.");
            }

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;
            return CreatePickerField(label, tracked.serializedObject, () =>
            {
                SerializedProperty live = Live(tracked, SerializableDateTime.PropertyName);
                return live == null ? string.Empty : Display(live);
            }, () =>
            {
                SerializedProperty live = Live(tracked, SerializableDateTime.PropertyName);
                DateTime current = SerializableDateTime.TryParse(live?.stringValue, out DateTime utc)
                    ? utc
                    : DateTime.UtcNow;
                CalendarPopup.Show(current, selected =>
                {
                    SerializedProperty target = Live(tracked, SerializableDateTime.PropertyName);
                    if (target == null)
                        return;

                    target.stringValue = SerializableDateTime.FormatRoundTrip(selected);
                    target.serializedObject.ApplyModifiedProperties();
                });
            });
        }

        internal static VisualElement CreatePickerField(
            string label, SerializedObject serializedObject, Func<string> text, Action onPick)
        {
            VisualElement input = new();
            input.AddToClassList(BaseField<string>.inputUssClassName);
            input.AddToClassList("mie-datetime-field__input");

            Label value = new(text());
            value.AddToClassList("mie-datetime-field__value");
            input.Add(value);

            Button pick = new();
            pick.AddToClassList("mie-datetime-field__button");
            pick.clicked += onPick;

            Label icon = new("📅");
            icon.AddToClassList("mie-datetime-field__button-icon");
            icon.pickingMode = PickingMode.Ignore;
            icon.style.color = StyleKeyword.Null;
            pick.Add(icon);
            input.Add(pick);

            DateTimeRow field = new(label, input);
            UIElementsTools.AddStyleSheet(field, typeof(DateTimeDrawer));
            field.TrackSerializedObjectValue(serializedObject, _ =>
            {
                string next = text();
                if (value.text != next)
                    value.text = next;
            });
            return field;
        }

        private static SerializedProperty Live(SerializedProperty root, string relative)
        {
            SerializedProperty current = root.serializedObject.FindProperty(root.propertyPath);
            return current?.FindPropertyRelative(relative);
        }

        private static string Display(SerializedProperty value)
        {
            DateTime utc = SerializableDateTime.TryParse(value.stringValue, out DateTime parsed)
                ? parsed
                : DateTime.UtcNow;
            return SerializableDateTime.FormatDisplay(utc);
        }

        private sealed class DateTimeRow : BaseField<string>
        {
            public DateTimeRow(string label, VisualElement visualInput) : base(label, visualInput)
            {
                AddToClassList(alignedFieldUssClassName);
                AddToClassList("mie-datetime-field");
            }
        }
    }
}
