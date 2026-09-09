using System;
using Mane.DotNet;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxInt))]
    [CustomPropertyDrawer(typeof(MinMaxFloat))]
    [CustomPropertyDrawer(typeof(MinMaxDouble))]
    internal sealed class MinMaxDrawer : PropertyDrawer
    {
        private static StyleSheet _sheet;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty a = property.FindPropertyRelative(nameof(MinMaxInt.A));
            SerializedProperty b = property.FindPropertyRelative(nameof(MinMaxInt.B));
            string label = string.IsNullOrEmpty(preferredLabel) ? property.displayName : preferredLabel;

            VisualElement min = CreateEndpoint("unity-x-input", "Min", a);
            VisualElement max = CreateEndpoint("unity-y-input", "Max", b);

            bool normalizing = false;
            void TryNormalize()
            {
                if (normalizing)
                    return;

                normalizing = true;
                try
                {
                    Normalize(a, b);
                }
                finally
                {
                    normalizing = false;
                }
            }

            RegisterCommit(min, TryNormalize);
            RegisterCommit(max, TryNormalize);

            return CreateComposite(label, min, max);
        }

        private static VisualElement CreateEndpoint(string name, string label, SerializedProperty property)
        {
            VisualElement field = property.numericType switch
            {
                SerializedPropertyNumericType.Int32 => Bind(new IntegerField(label), property),
                SerializedPropertyNumericType.Double => Bind(new DoubleField(label), property),
                _ => Bind(new FloatField(label), property)
            };
            field.name = name;
            field.AddToClassList("unity-composite-field__field");
            if (name == "unity-x-input")
                field.AddToClassList("unity-composite-field__field--first");
            return field;
        }

        private static T Bind<T>(T field, SerializedProperty property) where T : VisualElement, IBindable
        {
            SetDelayed(field);
            field.BindProperty(property);
            return field;
        }

        private static void SetDelayed(VisualElement field)
        {
            switch (field)
            {
                case IntegerField integerField:
                    integerField.isDelayed = true;
                    break;
                case FloatField floatField:
                    floatField.isDelayed = true;
                    break;
                case DoubleField doubleField:
                    doubleField.isDelayed = true;
                    break;
            }
        }

        private static void RegisterCommit(VisualElement field, Action onCommit)
        {
            field.RegisterCallback<FocusOutEvent>(_ => onCommit());
            switch (field)
            {
                case IntegerField integerField:
                    integerField.RegisterValueChangedCallback(_ => onCommit());
                    break;
                case FloatField floatField:
                    floatField.RegisterValueChangedCallback(_ => onCommit());
                    break;
                case DoubleField doubleField:
                    doubleField.RegisterValueChangedCallback(_ => onCommit());
                    break;
            }
        }

        private static void Normalize(SerializedProperty min, SerializedProperty max)
        {
            switch (min.numericType)
            {
                case SerializedPropertyNumericType.Int32:
                    if (min.intValue <= max.intValue)
                        return;
                    (min.intValue, max.intValue) = (max.intValue, min.intValue);
                    break;
                case SerializedPropertyNumericType.Double:
                    if (min.doubleValue <= max.doubleValue)
                        return;
                    (min.doubleValue, max.doubleValue) = (max.doubleValue, min.doubleValue);
                    break;
                default:
                    if (min.floatValue <= max.floatValue)
                        return;
                    (min.floatValue, max.floatValue) = (max.floatValue, min.floatValue);
                    break;
            }

            min.serializedObject.ApplyModifiedProperties();
        }

        private static VisualElement CreateComposite(string label, VisualElement min, VisualElement max)
        {
            VisualElement root = new();
            root.AddToClassList(BaseField<float>.ussClassName);
            root.AddToClassList(BaseField<float>.alignedFieldUssClassName);
            root.AddToClassList("unity-composite-field");
            root.AddToClassList(Vector2Field.ussClassName);
            root.AddToClassList("mie-minmax-field");
            ApplySheet(root);

            Label propertyLabel = new(label);
            propertyLabel.AddToClassList(BaseField<float>.labelUssClassName);
            propertyLabel.AddToClassList("unity-composite-field__label");
            propertyLabel.AddToClassList(Vector2Field.labelUssClassName);

            VisualElement input = new();
            input.AddToClassList(BaseField<float>.inputUssClassName);
            input.AddToClassList("unity-composite-field__input");
            input.AddToClassList(Vector2Field.inputUssClassName);
            input.Add(min);
            input.Add(max);

            root.Add(propertyLabel);
            root.Add(input);
            return root;
        }

        private static void ApplySheet(VisualElement root)
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("MinMaxDrawer.uss was not found next to MinMaxDrawer.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(MinMaxDrawer));
    }
}
