using System;
using System.Reflection;
using UnityEngine;

namespace Mane.Unity
{
    public sealed class DropdownListAttribute : PropertyAttribute
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        public string[] Strings { get; }
        public Type SourceType { get; }
        public string MemberName { get; }

        public DropdownListAttribute(params string[] strings) { Strings = strings; }

        public DropdownListAttribute(Type type, string memberName)
        {
            SourceType = type;
            MemberName = memberName;
        }

        public bool CanRefresh => SourceType != null && !string.IsNullOrEmpty(MemberName);

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
