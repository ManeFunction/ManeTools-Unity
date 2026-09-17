using System;
using System.Reflection;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Draws a string (or matching int) as a dropdown of fixed or resolved choices.
    /// </summary>
    public sealed class DropdownListAttribute : PropertyAttribute
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        /// <summary>
        /// Static choices used when no source member is set.
        /// </summary>
        public string[] Strings { get; }

        /// <summary>
        /// Type that owns the source member when choices are resolved dynamically.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Parameterless method or property that returns <c>string[]</c>.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Creates a dropdown from a fixed list of strings.
        /// </summary>
        /// <param name="strings">Dropdown choices.</param>
        public DropdownListAttribute(params string[] strings) { Strings = strings; }

        /// <summary>
        /// Creates a dropdown whose choices come from a member on <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type that owns the member.</param>
        /// <param name="memberName">Parameterless method or property returning <c>string[]</c>.</param>
        public DropdownListAttribute(Type type, string memberName)
        {
            SourceType = type;
            MemberName = memberName;
        }

        /// <summary>
        /// True when choices can be resolved from <see cref="SourceType"/> and <see cref="MemberName"/>.
        /// </summary>
        public bool CanRefresh => SourceType != null && !string.IsNullOrEmpty(MemberName);

        /// <summary>
        /// Returns the current dropdown choices for <paramref name="target"/>.
        /// </summary>
        /// <param name="target">Inspected object, used for instance members.</param>
        /// <returns>Choice strings, or null if the member could not be resolved.</returns>
        public string[] GetStrings(object target) =>
            CanRefresh ? TryResolve(SourceType, MemberName, target, true) : Strings;

        private static string[] TryResolve(Type type, string memberName, object target, bool logIfMissing = false)
        {
            if (type == null || string.IsNullOrEmpty(memberName))
                return null;

            for (Type current = type; current != null; current = current.BaseType)
            {
                MethodInfo method = current.GetMethod(memberName, MemberFlags, null, Type.EmptyTypes, null);
                if (method != null && typeof(string[]).IsAssignableFrom(method.ReturnType))
                {
                    if (!TryGetInvokeTarget(method.IsStatic, method.DeclaringType, target, out object methodTarget))
                        continue;

                    return method.Invoke(methodTarget, null) as string[];
                }

                PropertyInfo property = current.GetProperty(memberName, MemberFlags);
                if (property == null || property.PropertyType != typeof(string[]) ||
                    property.GetIndexParameters().Length != 0)
                    continue;

                MethodInfo getter = property.GetGetMethod(true);
                if (getter == null)
                    continue;

                if (!TryGetInvokeTarget(getter.IsStatic, property.DeclaringType, target, out object propertyTarget))
                    continue;

                return property.GetValue(propertyTarget) as string[];
            }

            if (logIfMissing)
                Debug.LogError($"Dropdown List: Can't find method or property {memberName} in {type}");

            return null;
        }

        private static bool TryGetInvokeTarget(bool isStatic, Type declaringType, object target, out object instance)
        {
            instance = null;
            if (isStatic)
                return true;

            if (target == null || !declaringType.IsInstanceOfType(target))
                return false;

            instance = target;
            return true;
        }
    }
}
