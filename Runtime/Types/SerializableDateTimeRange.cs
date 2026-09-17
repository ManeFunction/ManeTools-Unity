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
        /// <summary>
        /// Serialized field name of the start ISO string.
        /// </summary>
        public const string StartPropertyName = nameof(_startDateTimeString);

        /// <summary>
        /// Serialized field name of the end ISO string.
        /// </summary>
        public const string EndPropertyName = nameof(_endDateTimeString);

        [SerializeField] private string _startDateTimeString;
        [SerializeField] private string _endDateTimeString;

        /// <summary>
        /// Raw start string, or empty if unset.
        /// </summary>
        public string StartString => _startDateTimeString ?? string.Empty;

        /// <summary>
        /// Raw end string, or empty if unset.
        /// </summary>
        public string EndString => _endDateTimeString ?? string.Empty;

        /// <summary>
        /// Range start as UTC. Invalid strings read as <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public DateTime Start
        {
            get => SerializableDateTime.TryParse(_startDateTimeString, out DateTime utc)
                ? utc
                : DateTime.UtcNow;
            set => _startDateTimeString = SerializableDateTime.FormatRoundTrip(value);
        }

        /// <summary>
        /// Range end as UTC. Invalid strings read as a week after now.
        /// </summary>
        public DateTime End
        {
            get => SerializableDateTime.TryParse(_endDateTimeString, out DateTime utc)
                ? utc
                : DateTime.UtcNow.AddDays(7);
            set => _endDateTimeString = SerializableDateTime.FormatRoundTrip(value);
        }

        /// <summary>
        /// Creates a range from now to a week later.
        /// </summary>
        public SerializableDateTimeRange() =>
            SetRange(DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        /// <summary>
        /// Creates a UTC range. Swaps the bounds if <paramref name="start"/> is after <paramref name="end"/>.
        /// </summary>
        public SerializableDateTimeRange(DateTime start, DateTime end)
        {
            if (start > end)
                (start, end) = (end, start);

            SetRange(start, end);
        }

        /// <summary>
        /// Returns the display strings for start and end, separated by an en dash.
        /// </summary>
        public override string ToString() =>
            $"{SerializableDateTime.FormatDisplay(Start)} – {SerializableDateTime.FormatDisplay(End)}";

        private void SetRange(DateTime start, DateTime end)
        {
            _startDateTimeString = SerializableDateTime.FormatRoundTrip(start);
            _endDateTimeString = SerializableDateTime.FormatRoundTrip(end);
        }
    }
}
