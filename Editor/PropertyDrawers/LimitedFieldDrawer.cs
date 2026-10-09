using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LimitedFieldAttribute))]
    internal sealed class LimitedFieldDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            bool isInt = tracked.propertyType == SerializedPropertyType.Integer && tracked.type == "int";
            bool isFloat = tracked.propertyType == SerializedPropertyType.Float && tracked.type == "float";
            if (!isInt && !isFloat)
                return Decorations.CreateUnsupportedFieldTypeWarning(tracked, "LimitedField", "int or float");

            LimitedFieldAttribute info = (LimitedFieldAttribute)attribute;
            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;

            if (isInt)
            {
                LimitedIntField field = new()
                {
                    label = label,
                    AllowNegatives = info.AllowNegatives,
                    NegativeLabel = info.NegativeLabel,
                    ZeroLabel = info.ZeroLabel
                };
                field.AddToClassList(BaseField<int>.alignedFieldUssClassName);
                field.BindProperty(tracked);
                return field;
            }

            LimitedFloatField floatField = new()
            {
                label = label,
                AllowNegatives = info.AllowNegatives,
                NegativeLabel = info.NegativeLabel,
                ZeroLabel = info.ZeroLabel
            };
            floatField.AddToClassList(BaseField<float>.alignedFieldUssClassName);
            floatField.BindProperty(tracked);
            return floatField;
        }
    }
}
