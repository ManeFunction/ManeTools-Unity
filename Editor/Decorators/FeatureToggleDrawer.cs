using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Draws <see cref="FeatureToggleAttribute"/> in Unity's default inspector: a header like
    /// <see cref="HeaderAttribute"/>'s, then the <see cref="bool"/> field. On any other field type it shows a warning.
    /// A ManeStyle inspector replaces it with a <see cref="FeatureBlock"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(FeatureToggleAttribute))]
    public sealed class FeatureToggleDrawer : PropertyDrawer
    {
        // The class Unity gives the label of its own Header decorator, so both look and align the same.
        private const string UnityHeaderClass = "unity-header-drawer__label";
        // The gap above Unity's Header title and the one under it, measured against a plain [Header].
        private const float TopGap = 11f;
        private const float BottomGap = 2f;
        // Unity's Header sits in the decorator container, which is indented 3px from the field labels' edge.
        private const float LeftGap = 3f;

        /// <summary>
        /// Builds the header and the toggle, or a warning for a field that is not a <see cref="bool"/>.
        /// </summary>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType != SerializedPropertyType.Boolean)
                return Decorations.CreateUnsupportedFieldTypeWarning(tracked, "FeatureToggle", "bool");

            Label header = new(ManeInspectorLayout.FeatureTitle((FeatureToggleAttribute)attribute, tracked));
            header.AddToClassList(UnityHeaderClass);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginTop = TopGap;
            header.style.marginBottom = BottomGap;
            header.style.marginLeft = LeftGap;

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;
            Toggle toggle = new(label) { tooltip = tracked.tooltip };
            toggle.AddToClassList(BaseField<bool>.alignedFieldUssClassName);
            toggle.BindProperty(tracked);

            VisualElement root = new();
            root.Add(header);
            root.Add(toggle);
            return root;
        }
    }
}
