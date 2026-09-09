using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
    internal sealed class LayerSelectorPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType != SerializedPropertyType.Integer)
                return Decorations.CreateUnsupportedFieldTypeWarning(tracked, "LayerSelector", "int");

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;
            LayerField field = new(label);
            field.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            field.BindProperty(tracked);
            return field;
        }
    }
}
