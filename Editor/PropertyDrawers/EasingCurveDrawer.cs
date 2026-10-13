using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Inspector field for <see cref="EasingCurveAttribute"/>: a preset dropdown, and the curve itself for Custom.
    /// An empty curve shows no selection.
    /// </summary>
    [CustomPropertyDrawer(typeof(EasingCurveAttribute))]
    internal sealed class EasingCurveDrawer : PropertyDrawer
    {
        private const string CurveLabel = "Curve";
        // Same indent as foldout content, so the curve reads as part of the dropdown above.
        private const float CurveIndent = 15f;

        /// <summary>
        /// Builds the preset dropdown and the curve field.
        /// </summary>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty tracked = property.Copy();
            if (tracked.propertyType != SerializedPropertyType.AnimationCurve)
            {
                return Decorations.CreateUnsupportedFieldTypeWarning(
                    tracked, "EasingCurve", nameof(AnimationCurve));
            }

            string label = string.IsNullOrEmpty(preferredLabel) ? tracked.displayName : preferredLabel;

            // A dropdown of names rather than an EnumField: an empty curve shows as no selection,
            // so picking Custom on it is a change that gets a starting curve.
            DropdownField presets = new(label, new List<string>(Enum.GetNames(typeof(EasingPreset))), -1);
            presets.AddToClassList(BaseField<string>.alignedFieldUssClassName);

            CurveField curve = new(CurveLabel)
            {
                // Fills the row without relying on the base-field styles, which only ManeStyle supplies.
                style = { marginLeft = 0, flexGrow = 1, flexShrink = 1, minWidth = 0 }
            };
            curve.AddToClassList(BaseField<AnimationCurve>.alignedFieldUssClassName);
            curve.BindProperty(tracked);

            // The branch mark sits in the indent, so the field keeps its position.
            Label branch = new("└")
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    width = CurveIndent,
                    flexShrink = 0,
                    paddingLeft = 0,
                    paddingRight = 0,
                    paddingTop = 3,
                    marginLeft = -2,
                    marginRight = 0,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };

            VisualElement curveRow = new() { style = { flexDirection = FlexDirection.Row } };
            curveRow.Add(branch);
            curveRow.Add(curve);

            VisualElement root = new();
            root.Add(presets);
            root.Add(curveRow);

            // The field stores only keys, so the preset is found by comparing them. Custom stays selected once
            // picked, even while the keys still match a preset, so the curve does not hide under the user.
            bool custom = false;
            ApplyDisplay();

            presets.RegisterValueChangedCallback(evt =>
            {
                if (!Enum.TryParse(evt.newValue, out EasingPreset preset))
                    return;

                custom = preset == EasingPreset.Custom;

                // Custom keeps the current keys; only an empty curve starts from Linear.
                // The setter copies the keys, so the shared preset is never edited through the field.
                AnimationCurve next = !custom ? Easing.Get(preset)
                    : IsEmpty() ? Easing.Get(EasingPreset.Linear)
                    : null;

                if (next != null)
                {
                    tracked.animationCurveValue = next;
                    tracked.serializedObject.ApplyModifiedProperties();
                }

                ApplyDisplay();
            });
            root.TrackPropertyValue(tracked, _ => ApplyDisplay());

            return root;

            bool IsEmpty() => !tracked.hasMultipleDifferentValues && tracked.animationCurveValue is not { length: > 0 };

            void ApplyDisplay()
            {
                bool mixed = tracked.hasMultipleDifferentValues;
                bool empty = IsEmpty();
                EasingPreset found = mixed || empty ? EasingPreset.Custom : Easing.FindPreset(tracked.animationCurveValue);
                if (empty)
                    custom = false;
                else if (!mixed && found == EasingPreset.Custom)
                    custom = true;

                string shown = empty ? null : (custom ? EasingPreset.Custom : found).ToString();
                presets.showMixedValue = mixed && !custom;
                if (presets.value != shown)
                    presets.SetValueWithoutNotify(shown);

                curveRow.style.display = custom ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
