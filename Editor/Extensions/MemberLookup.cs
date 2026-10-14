using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Finds what inspector attributes name by string. The <b>declaring object</b> of a property is the object that
    /// holds its field: the inspected object itself, a nested serializable class or a list element. A
    /// <b>condition member</b> is the member that AvailableIf or InfoBox names to decide availability or visibility.
    /// Members are looked up on the declaring object, then on the inspected object, each from the most derived type
    /// to the base; conditions first check serialized fields.
    /// </summary>
    internal static class MemberLookup
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private const string ArrayData = ".Array.data[";
        private const string ArraySize = ".Array.size";

        // Resolved members per (type, name, value type), null when there is none. Types only change on a domain reload.
        private static readonly Dictionary<(Type, string, Type), MemberInfo> Members = new();
        private static readonly HashSet<(Type, string, string)> Reported = new();

        /// <summary>
        /// The object that holds the field at <paramref name="propertyPath"/> on <paramref name="target"/>,
        /// or null when the path does not resolve (an empty list, a null reference on the way).
        /// </summary>
        public static object GetDeclaringObject(UnityObject target, string propertyPath)
        {
            if (target == null || string.IsNullOrEmpty(propertyPath))
                return null;

            string[] parts = SplitPath(propertyPath);
            object current = target;
            for (int i = 0; i < parts.Length - 1 && current != null; i++)
                current = GetPartValue(current, current.GetType(), parts[i], out _);

            return current;
        }

        /// <summary>
        /// The field behind <paramref name="property"/>. For a list element, the list field. Null when it does not resolve.
        /// </summary>
        public static FieldInfo GetField(SerializedProperty property)
        {
            UnityObject target = property?.serializedObject?.targetObject;
            if (target == null)
                return null;

            object current = target;
            Type type = target.GetType();
            FieldInfo field = null;
            foreach (string part in SplitPath(property.propertyPath))
            {
                // A null value on the way still has a declared type, so the walk goes on by type.
                current = GetPartValue(current, current?.GetType() ?? type, part, out field, out type);
                if (field == null)
                    return null;
            }

            return field;
        }

        /// <summary>
        /// Reads the field, property or parameterless method <paramref name="member"/> that gives a
        /// <typeparamref name="T"/>, on the declaring object of <paramref name="property"/> in <paramref name="target"/>,
        /// then on <paramref name="target"/> itself. With <paramref name="sourceType"/>, only objects of that type
        /// are used, and then its static members.
        /// </summary>
        public static bool TryGetValue<T>(SerializedProperty property, UnityObject target, string member, out T value,
            Type sourceType = null)
        {
            value = default;
            if (target == null || string.IsNullOrEmpty(member))
                return false;

            object declaring = GetDeclaringObject(target, property.propertyPath);
            if (TryRead(declaring, sourceType, member, out value))
                return true;
            if (!ReferenceEquals(declaring, target) && TryRead(target, sourceType, member, out value))
                return true;

            return sourceType != null && TryReadMember(sourceType, null, member, out value);
        }

        /// <summary>
        /// Evaluates condition member <paramref name="member"/> on every object being inspected: a serialized sibling
        /// field, then a serialized top-level field (true when it is not the default value), then a bool field,
        /// property or parameterless method. A missing member is logged once, as an error from <paramref name="attributeName"/>.
        /// </summary>
        public static Condition EvaluateCondition(SerializedProperty property, string member, string attributeName)
        {
            SerializedObject serializedObject = property?.serializedObject;
            if (serializedObject == null || string.IsNullOrEmpty(member))
                return Condition.Missing;

            SerializedProperty serialized = FindSerialized(property, member);
            if (serialized != null)
                return EvaluateSerialized(serialized);

            bool all = true;
            bool any = false;
            foreach (UnityObject target in serializedObject.targetObjects)
            {
                if (!TryGetValue(property, target, member, out bool value))
                {
                    ReportMissing(target != null ? target.GetType() : null, member, attributeName,
                        "a serialized field, or a bool field, property or parameterless method");
                    return Condition.Missing;
                }

                all &= value;
                any |= value;
            }

            return new Condition(true, all, any);
        }

        /// <summary>
        /// Logs that <paramref name="member"/> was not found on <paramref name="type"/>, once per type, member and attribute.
        /// </summary>
        public static void ReportMissing(Type type, string member, string attributeName, string expected)
        {
            if (!Reported.Add((type, member, attributeName)))
                return;

            Debug.LogError($"[{attributeName}] '{member}' was not found on {type?.Name ?? "null"}. Expected {expected}.");
        }

        private static SerializedProperty FindSerialized(SerializedProperty property, string member)
        {
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot >= 0)
            {
                SerializedProperty sibling = property.serializedObject.FindProperty($"{path[..lastDot]}.{member}");
                if (sibling != null)
                    return sibling;
            }

            return property.serializedObject.FindProperty(member);
        }

        private static Condition EvaluateSerialized(SerializedProperty serialized)
        {
            if (!serialized.hasMultipleDifferentValues)
            {
                bool value = !serialized.IsPropertyDefault();
                return new Condition(true, value, value);
            }

            // Different values can still all be set (1 and 2), so each object is read on its own.
            bool all = true;
            bool any = false;
            foreach (UnityObject target in serialized.serializedObject.targetObjects)
            {
                using SerializedObject single = new(target);
                SerializedProperty own = single.FindProperty(serialized.propertyPath);
                bool value = own != null && !own.IsPropertyDefault();
                all &= value;
                any |= value;
            }

            return new Condition(true, all, any);
        }

        private static bool TryRead<T>(object owner, Type sourceType, string member, out T value)
        {
            value = default;
            if (owner == null || (sourceType != null && !sourceType.IsInstanceOfType(owner)))
                return false;

            return TryReadMember(owner.GetType(), owner, member, out value);
        }

        // Static members only when instance is null.
        private static bool TryReadMember<T>(Type type, object instance, string member, out T value)
        {
            value = default;
            switch (Find(type, member, typeof(T)))
            {
                case FieldInfo field when instance != null || field.IsStatic:
                    value = (T)field.GetValue(field.IsStatic ? null : instance);
                    return true;
                case PropertyInfo property when instance != null || property.GetGetMethod(true).IsStatic:
                    value = (T)property.GetValue(property.GetGetMethod(true).IsStatic ? null : instance);
                    return true;
                case MethodInfo method when instance != null || method.IsStatic:
                    value = (T)method.Invoke(method.IsStatic ? null : instance, null);
                    return true;
                default:
                    return false;
            }
        }

        private static MemberInfo Find(Type type, string member, Type valueType)
        {
            (Type, string, Type) key = (type, member, valueType);
            if (Members.TryGetValue(key, out MemberInfo found))
                return found;

            for (Type current = type; current != null && found == null; current = current.BaseType)
            {
                // C# does not allow a field, property and method of the same name in one type, so the order is free.
                FieldInfo field = current.GetField(member, MemberFlags);
                if (field != null && valueType.IsAssignableFrom(field.FieldType))
                {
                    found = field;
                    break;
                }

                PropertyInfo property = current.GetProperty(member, MemberFlags);
                if (property != null && property.GetIndexParameters().Length == 0 &&
                    property.GetGetMethod(true) != null && valueType.IsAssignableFrom(property.PropertyType))
                {
                    found = property;
                    break;
                }

                MethodInfo method = current.GetMethod(member, MemberFlags, null, Type.EmptyTypes, null);
                if (method != null && valueType.IsAssignableFrom(method.ReturnType))
                    found = method;
            }

            Members[key] = found;
            return found;
        }

        // "list.Array.data[2].value" -> "list[2]", "value". A list size path stands for the list.
        private static string[] SplitPath(string propertyPath)
        {
            if (propertyPath.EndsWith(ArraySize, StringComparison.Ordinal))
                propertyPath = propertyPath[..^ArraySize.Length];

            return propertyPath.Replace(ArrayData, "[").Split('.');
        }

        private static object GetPartValue(object source, Type type, string part, out FieldInfo field) =>
            GetPartValue(source, type, part, out field, out _);

        // Value of one path part on source (null when it has none), with the field and the declared type of the value.
        private static object GetPartValue(object source, Type type, string part, out FieldInfo field, out Type valueType)
        {
            int bracket = part.IndexOf('[');
            string name = bracket < 0 ? part : part[..bracket];

            field = FindField(type, name);
            valueType = field?.FieldType;
            if (field == null)
                return null;

            object value = source != null ? field.GetValue(source) : null;
            if (bracket < 0)
                return value;

            valueType = GetElementType(field.FieldType);
            int index = int.Parse(part.Substring(bracket + 1, part.IndexOf(']') - bracket - 1));
            return value is IList items && index >= 0 && index < items.Count ? items[index] : null;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, MemberFlags);
                if (field != null)
                    return field;
            }

            return null;
        }

        private static Type GetElementType(Type listType)
        {
            if (listType.IsArray)
                return listType.GetElementType();

            return listType.IsGenericType ? listType.GetGenericArguments()[0] : typeof(object);
        }

        /// <summary>
        /// Result of <see cref="EvaluateCondition"/> over the objects being inspected.
        /// </summary>
        internal readonly struct Condition
        {
            public static readonly Condition Missing = new(false, false, false);

            public Condition(bool found, bool all, bool any)
            {
                Found = found;
                All = all;
                Any = any;
            }

            /// <summary>The member was found.</summary>
            public bool Found { get; }

            /// <summary>True on every inspected object.</summary>
            public bool All { get; }

            /// <summary>True on at least one inspected object.</summary>
            public bool Any { get; }
        }
    }
}
