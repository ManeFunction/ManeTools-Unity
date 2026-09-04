using System;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Integer field with optional labels for zero and for -1.
    /// Negatives are either clamped out, or limited to -1 with an optional display string.
    /// Empty labels show the numeric value.
    /// </summary>
    [UxmlElement]
    public partial class LimitedValueField : IntegerField
    {
        private bool _allowNegatives;
        private string _negativeLabel = string.Empty;
        private string _zeroLabel = string.Empty;

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

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(Clamp(newValue));
        }

        protected override string ValueToString(int v)
        {
            if (v < 0)
                return string.IsNullOrEmpty(_negativeLabel) ? base.ValueToString(v) : _negativeLabel;

            if (v == 0)
                return string.IsNullOrEmpty(_zeroLabel) ? base.ValueToString(v) : _zeroLabel;

            return base.ValueToString(v);
        }

        protected override int StringToValue(string str)
        {
            if (!string.IsNullOrEmpty(_negativeLabel) &&
                string.Equals(str, _negativeLabel, StringComparison.OrdinalIgnoreCase))
                return _allowNegatives ? -1 : 0;

            if (!string.IsNullOrEmpty(_zeroLabel) &&
                string.Equals(str, _zeroLabel, StringComparison.OrdinalIgnoreCase))
                return 0;

            return Clamp(base.StringToValue(str));
        }

        private int Clamp(int v)
        {
            if (_allowNegatives)
                return v < -1 ? -1 : v;

            return v < 0 ? 0 : v;
        }
    }
}
