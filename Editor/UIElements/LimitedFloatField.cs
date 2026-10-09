using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Decimal version of <see cref="LimitedIntField"/>: float field with optional labels for zero and for -1.
    /// Negatives are either clamped out, or limited to -1 with an optional display string.
    /// Empty labels show the numeric value.
    /// </summary>
    [UxmlElement]
    public sealed partial class LimitedFloatField : FloatField
    {
        private bool _allowNegatives;
        private string _negativeLabel = string.Empty;
        private string _zeroLabel = string.Empty;
        private bool _dragging;

        /// <summary>
        /// Creates a field with default labels.
        /// </summary>
        public LimitedFloatField()
        {
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                RefreshDisplayedText();
                schedule.Execute(RefreshDisplayedText);
            });
        }

        /// <summary>
        /// When true, -1 is allowed (shown as <see cref="NegativeLabel"/> when set).
        /// </summary>
        [UxmlAttribute("allow-negatives")]
        public bool AllowNegatives
        {
            get => _allowNegatives;
            set
            {
                if (_allowNegatives == value)
                    return;

                _allowNegatives = value;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// Optional display string for -1. Empty shows the number.
        /// </summary>
        [UxmlAttribute("negative-label")]
        public string NegativeLabel
        {
            get => _negativeLabel;
            set
            {
                _negativeLabel = value ?? string.Empty;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// Optional display string for 0. Empty shows the number.
        /// </summary>
        [UxmlAttribute("zero-label")]
        public string ZeroLabel
        {
            get => _zeroLabel;
            set
            {
                _zeroLabel = value ?? string.Empty;
                SetValueWithoutNotify(this.value);
            }
        }

        /// <summary>
        /// The field value, clamped before the change event is sent so listeners and bindings get the limited value.
        /// </summary>
        public override float value
        {
            get => base.value;
            set => base.value = _dragging ? ClampDragged(value) : Clamp(value);
        }

        /// <summary>
        /// Applies a label drag. Drag steps are small decimals, so -1 and 0 are passed one at a time
        /// instead of snapping back to -1, as the integer field does.
        /// </summary>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, float startValue)
        {
            _dragging = true;
            try
            {
                base.ApplyInputDeviceDelta(delta, speed, startValue);
            }
            finally
            {
                _dragging = false;
            }
        }

        /// <summary>
        /// Sets the value without sending a change event, clamping and refreshing labels.
        /// </summary>
        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(Clamp(newValue));
            RefreshDisplayedText();
        }

        private void RefreshDisplayedText()
        {
            string display = ValueToString(value);
            if (text != display)
                text = display;
        }

        protected override string ValueToString(float v)
        {
            if (v < 0f)
                return string.IsNullOrEmpty(_negativeLabel) ? base.ValueToString(v) : _negativeLabel;

            if (v == 0f)
                return string.IsNullOrEmpty(_zeroLabel) ? base.ValueToString(v) : _zeroLabel;

            return base.ValueToString(v);
        }

        protected override float StringToValue(string str)
        {
            if (!string.IsNullOrEmpty(_negativeLabel) &&
                string.Equals(str, _negativeLabel, StringComparison.OrdinalIgnoreCase))
                return _allowNegatives ? -1f : 0f;

            if (!string.IsNullOrEmpty(_zeroLabel) &&
                string.Equals(str, _zeroLabel, StringComparison.OrdinalIgnoreCase))
                return 0f;

            return Clamp(base.StringToValue(str));
        }

        private float ClampDragged(float v)
        {
            if (!_allowNegatives)
                return Clamp(v);

            if (v >= 0f)
                return v;

            // Below zero the value moves between -1 and 0 first, so neither is skipped by a small drag step.
            float current = base.value;
            if (current < 0f)
                return v > current ? 0f : -1f;

            return current > 0f ? 0f : -1f;
        }

        private float Clamp(float v)
        {
            if (_allowNegatives)
                return v < 0f ? -1f : v;

            return v < 0f ? 0f : v;
        }
    }
}
