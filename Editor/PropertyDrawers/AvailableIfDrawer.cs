using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(AvailableIfAttribute))]
    internal sealed class AvailableIfDrawer : DecoratorDrawer
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public override VisualElement CreatePropertyGUI()
        {
            AvailableIfAttribute info = (AvailableIfAttribute)attribute;
            return PropertyHost.CreateHook(host =>
            {
                if (string.IsNullOrEmpty(info.PropertyName))
                {
                    host.SetAvailable(info.IsAvailable, info.Hide);
                    return;
                }

                host.WhenBound(property => host.SetAvailable(Evaluate(property, info), info.Hide));
            });
        }

        private static bool Evaluate(SerializedProperty property, AvailableIfAttribute info)
        {
            bool available = info.IsAvailable;

            if (!string.IsNullOrEmpty(info.PropertyName))
            {
                SerializedProperty attached = FindAttached(property, info.PropertyName);
                if (attached != null)
                {
                    available = !attached.IsPropertyDefault();
                }
                else
                {
                    object declaring = GetDeclaringObject(property.serializedObject.targetObject, property.propertyPath);
                    if (!TryGetBool(declaring, info.PropertyName, out available) &&
                        !TryGetBool(property.serializedObject.targetObject, info.PropertyName, out available))
                    {
                        Debug.LogError(
                            $"AvailableIf: Can't find {info.PropertyName} in {property.serializedObject.targetObject.GetType()}");
                    }
                }
            }

            return info.Invert ? !available : available;
        }

        private static SerializedProperty FindAttached(SerializedProperty property, string name)
        {
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot >= 0)
            {
                SerializedProperty sibling = property.serializedObject.FindProperty($"{path[..lastDot]}.{name}");
                if (sibling != null)
                    return sibling;
            }

            return property.serializedObject.FindProperty(name);
        }

        private static bool TryGetBool(object target, string name, out bool value)
        {
            value = false;
            if (target == null)
                return false;

            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                MethodInfo method = type.GetMethod(name, MemberFlags, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType == typeof(bool))
                {
                    value = (bool)method.Invoke(method.IsStatic ? null : target, null);
                    return true;
                }

                PropertyInfo property = type.GetProperty(name, MemberFlags);
                if (property != null && property.PropertyType == typeof(bool) &&
                    property.GetIndexParameters().Length == 0)
                {
                    MethodInfo getter = property.GetGetMethod(true);
                    value = (bool)property.GetValue(getter != null && getter.IsStatic ? null : target);
                    return true;
                }

                FieldInfo field = type.GetField(name, MemberFlags);
                if (field != null && field.FieldType == typeof(bool))
                {
                    value = (bool)field.GetValue(field.IsStatic ? null : target);
                    return true;
                }
            }

            return false;
        }

        private static object GetDeclaringObject(UnityObject root, string propertyPath)
        {
            if (propertyPath.EndsWith(".Array.size", StringComparison.Ordinal))
                propertyPath = propertyPath.Substring(0, propertyPath.Length - ".Array.size".Length);

            object current = root;
            string normalized = propertyPath.Replace(".Array.data[", "[");
            string[] parts = normalized.Split('.');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                current = GetMemberValue(current, parts[i]);
                if (current == null)
                    return null;
            }

            return current;
        }

        private static object GetMemberValue(object source, string part)
        {
            if (part.Contains('['))
            {
                int bracket = part.IndexOf('[');
                string fieldName = part.Substring(0, bracket);
                int index = int.Parse(part.Substring(bracket + 1, part.IndexOf(']') - bracket - 1));
                FieldInfo field = FindField(source.GetType(), fieldName);
                object list = field?.GetValue(source);
                return list is IList items && index >= 0 && index < items.Count ? items[index] : null;
            }

            return FindField(source.GetType(), part)?.GetValue(source);
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name, MemberFlags);
                if (field != null)
                    return field;

                type = type.BaseType;
            }

            return null;
        }
    }
}
