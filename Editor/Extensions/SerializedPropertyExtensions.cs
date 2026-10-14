using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Helpers for reading attributes and default values from a <see cref="SerializedProperty"/>.
    /// </summary>
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// First attribute of type <typeparamref name="T"/> on the backing field.
        /// </summary>
        public static T GetAttribute<T>(this SerializedProperty property) where T : Attribute
        {
            FieldInfo field = MemberLookup.GetField(property);
            return field?.GetCustomAttribute<T>();
        }

        /// <summary>
        /// All attributes of type <typeparamref name="T"/> on the backing field.
        /// </summary>
        public static T[] GetAttributes<T>(this SerializedProperty property) where T : Attribute
        {
            FieldInfo field = MemberLookup.GetField(property);
            if (field == null)
                return Array.Empty<T>();

            return (T[])field.GetCustomAttributes(typeof(T), true);
        }

        /// <summary>
        /// True when the boxed value is a type default, empty string, or identity quaternion.
        /// </summary>
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
