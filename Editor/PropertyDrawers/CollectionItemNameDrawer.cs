// This feature is based on BinaryCats solution, took from this thread:
// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/

using System.Globalization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(CollectionItemNameAttribute), true)]
    internal sealed class CollectionItemNameDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            CollectionItemNameAttribute info = (CollectionItemNameAttribute)attribute;
            SerializedProperty element = property.Copy();
            PropertyField field = new(element) { label = BuildLabel(element, info) };

            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                field.label = BuildLabel(element, info);
                SerializedProperty title = FindTitle(element, info.TitleVariableName);
                if (title != null)
                    field.TrackPropertyValue(title, _ => field.label = BuildLabel(element, info));
            });

            return field;
        }

        private static string BuildLabel(SerializedProperty element, CollectionItemNameAttribute info)
        {
            string text = $"{info.Prefix}{GetTitle(FindTitle(element, info.TitleVariableName))}{info.Postfix}";
            return string.IsNullOrEmpty(text) ? element.displayName : text;
        }

        private static SerializedProperty FindTitle(SerializedProperty element, string titleVariableName) =>
            element.serializedObject.FindProperty($"{element.propertyPath}.{titleVariableName}");

        private static string GetTitle(SerializedProperty title)
        {
            if (title == null)
                return string.Empty;

            switch (title.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return title.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return title.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return title.floatValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return title.stringValue;
                case SerializedPropertyType.Color:
                    return title.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return title.objectReferenceValue
                        ? title.objectReferenceValue.ToString()
                        : "None (Empty Object Ref)";
                case SerializedPropertyType.Enum:
                    return title.enumValueIndex < 0
                        ? string.Empty
                        : title.enumDisplayNames[title.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return title.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return title.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return title.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    return title.rectValue.ToString();
                case SerializedPropertyType.Quaternion:
                    return title.quaternionValue.ToString();
                default:
                    return string.Empty;
            }
        }
    }
}
