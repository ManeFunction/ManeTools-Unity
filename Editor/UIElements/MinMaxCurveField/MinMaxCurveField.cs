using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Inspector row for a <see cref="ParticleSystem.MinMaxCurve"/> property: standard label, one or two evenly sized
    /// values (constants or curves), and a mode dropdown arrow pinned to the right. The curve multiplier is not shown;
    /// switching from a constant mode to a curve mode sets it to 1, so curves show real values. Used by ManeStyle inspectors for MinMaxCurve fields and lists.
    /// Call <see cref="BindProperty"/> to attach it to a property; <see cref="BaseField{T}.value"/> mirrors the curve mode.
    /// </summary>
    public sealed class MinMaxCurveField : BaseField<ParticleSystemCurveMode>
    {
        /// <summary>
        /// USS class on the field root.
        /// </summary>
        public const string UssClassName = "mie-mmc-field";

        private const string TypeName = "MinMaxCurve";
        private const string InputClass = UssClassName + "__input";
        private const string SlotClass = UssClassName + "__slot";
        private const string ValueClass = UssClassName + "__value";
        private const string HiddenClass = SlotClass + "--hidden";
        private const string ModeClass = UssClassName + "__mode";
        private const string ModeArrowClass = UssClassName + "__mode-arrow";
        private const string PopupArrowClass = "unity-base-popup-field__arrow";

        private static readonly string[] ModeNames =
        {
            "Constant",
            "Curve",
            "Random Between Two Curves",
            "Random Between Two Constants"
        };

        private static StyleSheet _sheet;

        private readonly FloatField _constantMin;
        private readonly FloatField _constantMax;
        private readonly CurveField _curveMin;
        private readonly CurveField _curveMax;
        private readonly VisualElement _constantMinSlot;
        private readonly VisualElement _constantMaxSlot;
        private readonly VisualElement _curveMinSlot;
        private readonly VisualElement _curveMaxSlot;
        private readonly Button _modeButton;

        private SerializedProperty _property;
        private SerializedProperty _mode;
        private VisualElement _modeTracker;
        private bool _mixedMode;

        /// <summary>
        /// Creates a field without a label, for list items.
        /// </summary>
        public MinMaxCurveField() : this(null) { }

        /// <summary>
        /// Creates a field with a label.
        /// </summary>
        public MinMaxCurveField(string label) : this(label, new VisualElement()) { }

        private MinMaxCurveField(string label, VisualElement input) : base(label, input)
        {
            AddToClassList(UssClassName);
            AddToClassList(alignedFieldUssClassName);
            input.AddToClassList(InputClass);

            _constantMin = new FloatField();
            _constantMax = new FloatField();
            _curveMin = new CurveField();
            _curveMax = new CurveField();
            _constantMinSlot = CreateSlot(_constantMin, "Min");
            _constantMaxSlot = CreateSlot(_constantMax, "Max");
            _curveMinSlot = CreateSlot(_curveMin, "Min curve");
            _curveMaxSlot = CreateSlot(_curveMax, "Max curve");

            _modeButton = new Button(ShowModeMenu) { tooltip = "Mode" };
            _modeButton.AddToClassList(ModeClass);
            VisualElement arrow = new();
            arrow.AddToClassList(PopupArrowClass);
            arrow.AddToClassList(ModeArrowClass);
            _modeButton.Add(arrow);

            input.Add(_constantMinSlot);
            input.Add(_constantMaxSlot);
            input.Add(_curveMinSlot);
            input.Add(_curveMaxSlot);
            input.Add(_modeButton);

            ApplySheet();
            Refresh();
        }

        /// <summary>
        /// Bound MinMaxCurve property, or null.
        /// </summary>
        public SerializedProperty Property => _property;

        /// <summary>
        /// True when <paramref name="property"/> is a serialized <see cref="ParticleSystem.MinMaxCurve"/>.
        /// </summary>
        public static bool IsMinMaxCurve(SerializedProperty property) =>
            property != null &&
            property.propertyType == SerializedPropertyType.Generic &&
            property.type == TypeName &&
            property.FindPropertyRelative("m_Mode") != null;

        /// <summary>
        /// True when <paramref name="property"/> is an array or list of <see cref="ParticleSystem.MinMaxCurve"/>.
        /// </summary>
        public static bool IsMinMaxCurveArray(SerializedProperty property) =>
            property != null &&
            property.isArray &&
            property.propertyType == SerializedPropertyType.Generic &&
            property.arrayElementType == TypeName;

        /// <summary>
        /// Binds the value slots and the mode to <paramref name="property"/>. Safe to call again to rebind (list items).
        /// </summary>
        public void BindProperty(SerializedProperty property)
        {
            _property = property?.Copy();
            _mode = _property?.FindPropertyRelative("m_Mode");

            _modeTracker?.RemoveFromHierarchy();
            _modeTracker = null;

            if (_mode == null)
            {
                Refresh();
                return;
            }

            _constantMin.BindProperty(_property.FindPropertyRelative("m_ConstantMin"));
            _constantMax.BindProperty(_property.FindPropertyRelative("m_ConstantMax"));
            _curveMin.BindProperty(_property.FindPropertyRelative("m_CurveMin"));
            _curveMax.BindProperty(_property.FindPropertyRelative("m_CurveMax"));

            _modeTracker = new VisualElement();
            _modeTracker.AddToClassList(HiddenClass);
            hierarchy.Add(_modeTracker);
            _modeTracker.TrackPropertyValue(_mode, _ => Refresh());

            Refresh();
        }

        // Plain slot wrapper: list views force flex-grow 0 on every BaseField, so the wrapper owns the width.
        private static VisualElement CreateSlot(VisualElement field, string slotTooltip)
        {
            field.AddToClassList(ValueClass);
            field.tooltip = slotTooltip;

            VisualElement slot = new();
            slot.AddToClassList(SlotClass);
            slot.Add(field);
            return slot;
        }

        private ParticleSystemCurveMode Mode =>
            _mode != null ? (ParticleSystemCurveMode)_mode.intValue : ParticleSystemCurveMode.Constant;

        private void Refresh()
        {
            ParticleSystemCurveMode mode = Mode;
            bool curve = IsCurve(mode);

            SetShown(_constantMinSlot, mode == ParticleSystemCurveMode.TwoConstants);
            SetShown(_constantMaxSlot, !curve);
            SetShown(_curveMinSlot, mode == ParticleSystemCurveMode.TwoCurves);
            SetShown(_curveMaxSlot, curve);

            _modeButton.SetEnabled(_mode != null);
            _modeButton.tooltip = ModeNames[(int)mode];
            _mixedMode = _mode != null && _mode.hasMultipleDifferentValues;
            SetValueWithoutNotify(mode);
        }

        private static void SetShown(VisualElement element, bool shown) =>
            element.EnableInClassList(HiddenClass, !shown);

        private void ShowModeMenu()
        {
            if (_mode == null)
                return;

            ParticleSystemCurveMode current = Mode;
            GenericMenu menu = new();
            for (int i = 0; i < ModeNames.Length; i++)
            {
                ParticleSystemCurveMode mode = (ParticleSystemCurveMode)i;
                menu.AddItem(new GUIContent(ModeNames[i]), mode == current && !_mixedMode, () => SetMode(mode));
            }

            menu.DropDown(_modeButton.worldBound);
        }

        private void SetMode(ParticleSystemCurveMode mode)
        {
            SerializedObject serializedObject = _property.serializedObject;
            serializedObject.Update();

            ParticleSystemCurveMode previous = Mode;
            if (previous == mode && !_mode.hasMultipleDifferentValues)
                return;

            float constantMin = _property.FindPropertyRelative("m_ConstantMin").floatValue;
            float constantMax = _property.FindPropertyRelative("m_ConstantMax").floatValue;

            // The multiplier is hidden, so curves entered from a constant mode hold real values at multiplier 1.
            if (IsCurve(mode) && !IsCurve(previous))
                _property.FindPropertyRelative("m_CurveMultiplier").floatValue = 1f;

            if (IsCurve(mode))
                EnsureCurve(_property.FindPropertyRelative("m_CurveMax"), constantMax);
            if (mode == ParticleSystemCurveMode.TwoCurves)
                EnsureCurve(_property.FindPropertyRelative("m_CurveMin"), constantMin);

            _mode.intValue = (int)mode;
            serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        private static bool IsCurve(ParticleSystemCurveMode mode) =>
            mode is ParticleSystemCurveMode.Curve or ParticleSystemCurveMode.TwoCurves;

        private static void EnsureCurve(SerializedProperty curve, float level)
        {
            AnimationCurve value = curve.animationCurveValue;
            if (value == null || value.length == 0)
                curve.animationCurveValue = AnimationCurve.Linear(0f, level, 1f, level);
        }

        private void ApplySheet()
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("MinMaxCurveField.uss was not found next to MinMaxCurveField.");
                return;
            }

            if (!styleSheets.Contains(sheet))
                styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(MinMaxCurveField));
    }
}
