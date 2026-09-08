using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal static class VisualElementExtensions
    {
        private static FieldInfo _serializedPropertyField;
        private static PropertyInfo _inspectorSerializedObject;

        public static SerializedProperty GetBoundSerializedProperty(this VisualElement element)
        {
            _serializedPropertyField ??= typeof(PropertyField).GetField("m_SerializedProperty",
                BindingFlags.Instance | BindingFlags.NonPublic);

            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current is not PropertyField field)
                    continue;

                if (_serializedPropertyField?.GetValue(field) is SerializedProperty bound)
                    return bound;

                if (string.IsNullOrEmpty(field.bindingPath))
                    continue;

                SerializedProperty fromPath = field.GetInspectorSerializedObject()?.FindProperty(field.bindingPath);
                if (fromPath != null)
                    return fromPath;
            }

            return null;
        }

        public static SerializedObject GetInspectorSerializedObject(this VisualElement element)
        {
            InspectorElement inspector = element.GetFirstAncestorOfType<InspectorElement>();
            if (inspector == null)
                return null;

            _inspectorSerializedObject ??= typeof(InspectorElement).GetProperty("serializedObject",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return _inspectorSerializedObject?.GetValue(inspector) as SerializedObject;
        }
    }
}
