using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDateTimeRange))]
    internal sealed class DateTimeRangeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            SerializedProperty start = tracked.FindPropertyRelative(SerializableDateTimeRange.StartPropertyName);
            SerializedProperty end = tracked.FindPropertyRelative(SerializableDateTimeRange.EndPropertyName);
            if (start == null || end == null)
            {
                return Decorations.CreateWarningField(tracked,
                    "[SerializableDateTimeRange] backing strings were not found.");
            }

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;
            return DateTimeDrawer.CreatePickerField(label, tracked.serializedObject, () =>
            {
                SerializedProperty live = tracked.serializedObject.FindProperty(tracked.propertyPath);
                SerializedProperty liveStart = live?.FindPropertyRelative(SerializableDateTimeRange.StartPropertyName);
                SerializedProperty liveEnd = live?.FindPropertyRelative(SerializableDateTimeRange.EndPropertyName);
                return liveStart == null || liveEnd == null ? string.Empty : Display(liveStart, liveEnd);
            }, () =>
            {
                SerializedProperty live = tracked.serializedObject.FindProperty(tracked.propertyPath);
                SerializedProperty liveStart = live?.FindPropertyRelative(SerializableDateTimeRange.StartPropertyName);
                SerializedProperty liveEnd = live?.FindPropertyRelative(SerializableDateTimeRange.EndPropertyName);
                if (liveStart == null || liveEnd == null)
                    return;

                SerializableDateTimeRange current = ReadRange(liveStart, liveEnd);
                CalendarPopup.ShowRange(current, selected =>
                {
                    SerializedProperty target = tracked.serializedObject.FindProperty(tracked.propertyPath);
                    SerializedProperty targetStart =
                        target?.FindPropertyRelative(SerializableDateTimeRange.StartPropertyName);
                    SerializedProperty targetEnd =
                        target?.FindPropertyRelative(SerializableDateTimeRange.EndPropertyName);
                    if (targetStart == null || targetEnd == null)
                        return;

                    targetStart.stringValue = selected.StartString;
                    targetEnd.stringValue = selected.EndString;
                    target.serializedObject.ApplyModifiedProperties();
                });
            });
        }

        private static SerializableDateTimeRange ReadRange(SerializedProperty start, SerializedProperty end)
        {
            DateTime startValue = SerializableDateTime.TryParse(start.stringValue, out DateTime parsedStart)
                ? parsedStart
                : DateTime.UtcNow;
            DateTime endValue = SerializableDateTime.TryParse(end.stringValue, out DateTime parsedEnd)
                ? parsedEnd
                : DateTime.UtcNow.AddDays(7);
            return new SerializableDateTimeRange(startValue, endValue);
        }

        private static string Display(SerializedProperty start, SerializedProperty end) =>
            ReadRange(start, end).ToString();
    }
}
