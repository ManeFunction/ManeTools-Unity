using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    internal sealed class InfoBoxDrawer : ManeDecorator<InfoBoxDrawer>
    {
        protected override string XmlFileName => "InfoBox";

        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private static FieldInfo _serializedPropertyField;
        private static PropertyInfo _inspectorSerializedObject;

        public override VisualElement CreatePropertyGUI()
        {
            InfoBoxAttribute info = (InfoBoxAttribute)attribute;
            VisualTreeAsset tree = Xml;
            VisualElement root;
            if (tree == null)
            {
                Debug.LogError("InfoBox UXML is not assigned.");
                root = new Label(info.Message);
            }
            else
            {
                TemplateContainer container = tree.CloneTree();
                VisualElement box = container.Q<VisualElement>("infoBox");
                box.Q<Label>("label").text = info.Message;
                box.AddToClassList(TypeClass(info.Type));
                root = container;
            }

            BindShowCondition(root, info.ShowCondition, info.InvertCondition);
            return root;
        }

        public override float GetHeight()
        {
            InfoBoxAttribute info = (InfoBoxAttribute)attribute;
            float width = EditorGUIUtility.currentViewWidth;
            if (width < 1f)
                width = 200f;

            return Mathf.Max(
                EditorGUIUtility.singleLineHeight * 2f,
                EditorStyles.helpBox.CalcHeight(new GUIContent(info.Message), width));
        }

        private static void BindShowCondition(VisualElement root, string showCondition, bool invert)
        {
            if (string.IsNullOrEmpty(showCondition))
                return;

            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                bool tracking = false;
                int retries = 0;

                void TryBind()
                {
                    SerializedProperty property = GetBoundProperty(root);
                    if (property?.serializedObject == null)
                    {
                        if (retries++ < 10)
                            root.schedule.Execute(TryBind);
                        return;
                    }

                    SerializedProperty tracked = property.Copy();
                    UpdateDisplay(root, tracked, showCondition, invert);
                    if (tracking)
                        return;

                    tracking = true;
                    void Update() => UpdateDisplay(root, tracked, showCondition, invert);
                    root.TrackSerializedObjectValue(tracked.serializedObject, _ => Update());
                    if (tracked.isArray)
                    {
                        SerializedProperty size = tracked.FindPropertyRelative("Array.size");
                        if (size != null)
                            root.TrackPropertyValue(size, _ => Update());
                    }
                }

                TryBind();
            });
        }

        private static void UpdateDisplay(
            VisualElement root, SerializedProperty property, string showCondition, bool invert)
        {
            root.style.display = ShouldShow(property, showCondition, invert)
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private static bool ShouldShow(SerializedProperty property, string showCondition, bool invert)
        {
            foreach (UnityObject target in property.serializedObject.targetObjects)
            {
                object declaring = GetDeclaringObject(target, property.propertyPath);
                if (!TryEvaluate(declaring, showCondition, out bool value))
                    return true;

                if (invert ? value : !value)
                    return true;
            }

            return false;
        }

        private static bool TryEvaluate(object target, string memberName, out bool value)
        {
            value = false;
            if (target == null)
                return false;

            Type type = target.GetType();
            while (type != null)
            {
                PropertyInfo property = type.GetProperty(memberName, MemberFlags);
                if (property != null && property.PropertyType == typeof(bool) &&
                    property.GetIndexParameters().Length == 0)
                {
                    MethodInfo getter = property.GetGetMethod(true);
                    value = (bool)property.GetValue(getter != null && getter.IsStatic ? null : target);
                    return true;
                }

                MethodInfo method = type.GetMethod(memberName, MemberFlags, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType == typeof(bool))
                {
                    value = (bool)method.Invoke(method.IsStatic ? null : target, null);
                    return true;
                }

                FieldInfo field = type.GetField(memberName, MemberFlags);
                if (field != null && field.FieldType == typeof(bool))
                {
                    value = (bool)field.GetValue(field.IsStatic ? null : target);
                    return true;
                }

                type = type.BaseType;
            }

            Debug.LogWarning(
                $"[InfoBox] showCondition '{memberName}' was not found as a bool property, method, or field on {target.GetType().Name}.");
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

        private static SerializedProperty GetBoundProperty(VisualElement element)
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

                SerializedObject serializedObject = GetInspectorSerializedObject(field);
                SerializedProperty fromPath = serializedObject?.FindProperty(field.bindingPath);
                if (fromPath != null)
                    return fromPath;
            }

            return null;
        }

        private static SerializedObject GetInspectorSerializedObject(VisualElement element)
        {
            InspectorElement inspector = element.GetFirstAncestorOfType<InspectorElement>();
            if (inspector == null)
                return null;

            _inspectorSerializedObject ??= typeof(InspectorElement).GetProperty("serializedObject",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return _inspectorSerializedObject?.GetValue(inspector) as SerializedObject;
        }

        private static string TypeClass(InfoBoxType type) => type switch
        {
            InfoBoxType.Warning => "warning",
            InfoBoxType.Error => "error",
            InfoBoxType.None => "none",
            _ => "info"
        };
    }
}
