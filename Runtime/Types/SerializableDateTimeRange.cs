using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// UTC start–end range stored as round-trip ISO 8601 strings.
    /// </summary>
    [Serializable]
    public sealed class SerializableDateTimeRange
    {
        public const string StartPropertyName = nameof(_startDateTimeString);
        public const string EndPropertyName = nameof(_endDateTimeString);

        [SerializeField] private string _startDateTimeString;
        [SerializeField] private string _endDateTimeString;

        public string StartString => _startDateTimeString ?? string.Empty;
        public string EndString => _endDateTimeString ?? string.Empty;

        public DateTime Start
        {
            get => SerializableDateTime.TryParse(_startDateTimeString, out DateTime utc)
                ? utc
                : DateTime.UtcNow;
            set => _startDateTimeString = SerializableDateTime.FormatRoundTrip(value);
        }

        public DateTime End
        {
            get => SerializableDateTime.TryParse(_endDateTimeString, out DateTime utc)
                ? utc
                : DateTime.UtcNow.AddDays(7);
            set => _endDateTimeString = SerializableDateTime.FormatRoundTrip(value);
        }

        public SerializableDateTimeRange() =>
            SetRange(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        public SerializableDateTimeRange(DateTime start, DateTime end)
        {
            if (start > end)
                (start, end) = (end, start);

            SetRange(start, end);
        }

        public override string ToString() =>
            $"{SerializableDateTime.FormatDisplay(Start)} – {SerializableDateTime.FormatDisplay(End)}";

        private void SetRange(DateTime start, DateTime end)
        {
            _startDateTimeString = SerializableDateTime.FormatRoundTrip(start);
            _endDateTimeString = SerializableDateTime.FormatRoundTrip(end);
        }
    }
}
