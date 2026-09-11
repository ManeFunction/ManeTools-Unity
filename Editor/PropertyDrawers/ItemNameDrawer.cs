// This feature is based on BinaryCats solution, took from this thread:
// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/

using System;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(ItemNameAttribute), true)]
    internal sealed class ItemNameDrawer : PropertyDrawer
    {
        private const string InvalidLabelMessage =
            "[ItemNameFromString] label must contain \"{0}\" as the item index placeholder.";
        private const string BaseLabelClass = "unity-base-field__label";

        private static readonly FieldInfo LabelField = typeof(PropertyField)
            .GetField("m_Label", BindingFlags.Instance | BindingFlags.NonPublic);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty element = property.Copy();
            return attribute switch
            {
                ItemNameFromStringAttribute indexed => CreateIndexedField(element, indexed),
                ItemNameFromFieldAttribute named => CreateTitleField(element, named),
                _ => new PropertyField(element)
            };
        }

        private static VisualElement CreateIndexedField(
            SerializedProperty element, ItemNameFromStringAttribute info)
        {
            if (!ItemNameFromStringAttribute.HasIndexPlaceholder(info.Label))
            {
                PropertyField field = new(element);
                if (!IsFirstElement(element))
                    return field;

                VisualElement root = new();
                root.Add(InfoBoxDrawer.Create(InvalidLabelMessage, InfoBoxType.Warning));
                root.Add(field);
                return root;
            }

            PropertyField indexed = new(element);
            SetItemLabel(indexed, FormatIndexLabel(element, info));
            WatchElement(indexed, element.serializedObject, live => FormatIndexLabel(live, info));
            return indexed;
        }

        private static VisualElement CreateTitleField(
            SerializedProperty element, ItemNameFromFieldAttribute info)
        {
            PropertyField field = new(element);
            SetItemLabel(field, BuildTitleLabel(element, info));
            WatchElement(field, element.serializedObject, live => BuildTitleLabel(live, info));
            return field;
        }

        private static void WatchElement(
            PropertyField field, SerializedObject serializedObject, Func<SerializedProperty, string> makeLabel)
        {
            void Refresh()
            {
                SerializedProperty live = FindLiveElement(field, serializedObject);
                if (live == null)
                    return;

                SetItemLabel(field, makeLabel(live));
            }

            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                Refresh();
                field.schedule.Execute(Refresh);
            });
            field.TrackSerializedObjectValue(serializedObject, _ => Refresh());
        }

        private static SerializedProperty FindLiveElement(PropertyField field, SerializedObject serializedObject)
        {
            string path = field.bindingPath;
            if (string.IsNullOrEmpty(path))
                return null;

            SerializedObject so = serializedObject ?? field.GetInspectorSerializedObject();
            return so?.FindProperty(path);
        }

        private static void SetItemLabel(PropertyField field, string title)
        {
            // PropertyField.label's setter calls Rebind() and destroys the inner TextField
            // mid-edit, so only the first typed character survives until a full rebuild.
            if (LabelField != null && !Equals(LabelField.GetValue(field), title))
                LabelField.SetValue(field, title);

            Foldout foldout = FindFoldout(field);
            if (foldout != null)
            {
                if (foldout.text != title)
                    foldout.text = title;

                Label caption = foldout.Q<Label>(className: Toggle.textUssClassName);
                if (caption != null && caption.text != title)
                    caption.text = title;
                return;
            }

            foreach (Label label in field.Query<Label>(className: BaseLabelClass).ToList())
            {
                if (label.GetFirstAncestorOfType<PropertyField>() != field)
                    continue;
                if (label.text != title)
                    label.text = title;
                return;
            }
        }

        private static Foldout FindFoldout(PropertyField host)
        {
            foreach (VisualElement child in host.hierarchy.Children())
            {
                if (child is Foldout foldout)
                    return foldout;
            }

            return null;
        }

        private static string FormatIndexLabel(SerializedProperty element, ItemNameFromStringAttribute info)
        {
            if (!TryGetElementIndex(element, out int index))
                return element.displayName;

            try
            {
                return string.Format(CultureInfo.InvariantCulture, info.Label, info.StartingIndex + index);
            }
            catch (FormatException)
            {
                return element.displayName;
            }
        }

        private static string BuildTitleLabel(SerializedProperty element, ItemNameFromFieldAttribute info)
        {
            string text = $"{info.Prefix}{GetTitle(FindTitle(element, info.TitleVariableName))}{info.Postfix}";
            return string.IsNullOrEmpty(text) ? element.displayName : text;
        }

        private static bool IsFirstElement(SerializedProperty element) =>
            TryGetElementIndex(element, out int index) && index == 0;

        private static bool TryGetElementIndex(SerializedProperty element, out int index)
        {
            string path = element.propertyPath;
            int close = path.LastIndexOf(']');
            int open = path.LastIndexOf('[');
            if (open < 0 || close <= open)
            {
                index = 0;
                return false;
            }

            return int.TryParse(path.Substring(open + 1, close - open - 1), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out index);
        }

        private static SerializedProperty FindTitle(SerializedProperty element, string titleVariableName)
        {
            if (element == null || string.IsNullOrEmpty(titleVariableName))
                return null;

            return element.FindPropertyRelative(titleVariableName);
        }

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
