using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    public static class SerializedPropertyExtensions
    {
        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static T GetAttribute<T>(this SerializedProperty property) where T : Attribute
        {
            FieldInfo field = GetFieldInfoFromProperty(property);
            return field?.GetCustomAttribute<T>();
        }

        public static T[] GetAttributes<T>(this SerializedProperty property) where T : Attribute
        {
            FieldInfo field = GetFieldInfoFromProperty(property);
            if (field == null)
                return Array.Empty<T>();

            return (T[])field.GetCustomAttributes(typeof(T), true);
        }

        private static FieldInfo GetFieldInfoFromProperty(SerializedProperty property)
        {
            if (property?.serializedObject?.targetObject == null)
                return null;

            object current = property.serializedObject.targetObject;
            FieldInfo field = null;
            string normalized = property.propertyPath.Replace(".Array.data[", "[");
            string[] parts = normalized.Split('.');

            foreach (string part in parts)
            {
                if (current == null)
                    return field;

                if (part.Contains('['))
                {
                    int bracket = part.IndexOf('[');
                    string fieldName = part.Substring(0, bracket);
                    int index = int.Parse(part.Substring(bracket + 1, part.IndexOf(']') - bracket - 1));
                    field = FindField(current.GetType(), fieldName);
                    if (field == null)
                        return null;

                    object list = field.GetValue(current);
                    current = list is IList items && index >= 0 && index < items.Count
                        ? items[index]
                        : null;
                }
                else
                {
                    field = FindField(current.GetType(), part);
                    if (field == null)
                        return null;

                    current = field.GetValue(current);
                }
            }

            return field;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name, FieldFlags);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }

        public static bool IsPropertyDefault(this SerializedProperty property)
        {
            object value;
            try
            {
                value = property.boxedValue;
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return value switch
            {
                null => true,
                string s => string.IsNullOrEmpty(s),
                Quaternion q => q == Quaternion.identity,
                _ => Equals(value, value.GetType().IsValueType ? Activator.CreateInstance(value.GetType()) : null)
            };
        }
    }
}
