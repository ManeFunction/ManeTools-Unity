using System;
using System.Globalization;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// UTC <see cref="DateTime"/> stored as a round-trip ISO 8601 string.
    /// </summary>
    [Serializable]
    public sealed class SerializableDateTime
    {
        /// <summary>
        /// Serialized field name of the stored ISO string.
        /// </summary>
        public const string PropertyName = nameof(_dateTimeString);

        /// <summary>
        /// Display format for a date without time.
        /// </summary>
        public const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Display format for a date with hours and minutes.
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        [SerializeField] private string _dateTimeString;

        /// <summary>
        /// Stored UTC value. Invalid strings read as <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public DateTime Value
        {
            get => TryParse(_dateTimeString, out DateTime utc) ? utc : DateTime.UtcNow;
            set => _dateTimeString = FormatRoundTrip(ToUtc(value));
        }

        /// <summary>
        /// Creates an instance set to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public SerializableDateTime() => Value = DateTime.UtcNow;

        /// <summary>
        /// Creates an instance from <paramref name="dateTime"/>, stored as UTC.
        /// </summary>
        public SerializableDateTime(DateTime dateTime) => Value = dateTime;

        /// <summary>
        /// Converts to the stored UTC <see cref="DateTime"/>.
        /// </summary>
        public static implicit operator DateTime(SerializableDateTime serializable) =>
            serializable?.Value ?? DateTime.UtcNow;

        /// <summary>
        /// Wraps a <see cref="DateTime"/> as UTC.
        /// </summary>
        public static implicit operator SerializableDateTime(DateTime dateTime) =>
            new(dateTime);

        /// <summary>
        /// Returns the display string for <see cref="Value"/>.
        /// </summary>
        public override string ToString() => FormatDisplay(Value);

        /// <summary>
        /// Converts <paramref name="value"/> to UTC. Unspecified kind is treated as UTC.
        /// </summary>
        public static DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <summary>
        /// Converts to UTC and drops seconds and smaller units.
        /// </summary>
        public static DateTime Normalize(DateTime value)
        {
            DateTime utc = ToUtc(value);
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Parses a round-trip string to UTC.
        /// </summary>
        /// <returns>True when <paramref name="value"/> is a valid date-time.</returns>
        public static bool TryParse(string value, out DateTime utc)
        {
            utc = default;
            if (string.IsNullOrEmpty(value))
                return false;

            if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                    out DateTime parsed))
                return false;

            utc = ToUtc(parsed);
            return true;
        }

        /// <summary>
        /// Formats <paramref name="utc"/> as a round-trip ISO 8601 string.
        /// </summary>
        public static string FormatRoundTrip(DateTime utc) =>
            ToUtc(utc).ToString("O", CultureInfo.InvariantCulture);

        /// <summary>
        /// Formats <paramref name="value"/> with <see cref="DateTimeFormat"/>.
        /// </summary>
        public static string FormatDisplay(DateTime value) =>
            new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind)
                .ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        /// <summary>
        /// Formats <paramref name="value"/> with <see cref="DateFormat"/>.
        /// </summary>
        public static string FormatDate(DateTime value) =>
            value.ToString(DateFormat, CultureInfo.InvariantCulture);
    }
}
