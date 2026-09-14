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
        public const string PropertyName = nameof(_dateTimeString);
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        [SerializeField] private string _dateTimeString;

        public DateTime Value
        {
            get => TryParse(_dateTimeString, out DateTime utc) ? utc : DateTime.UtcNow;
            set => _dateTimeString = FormatRoundTrip(ToUtc(value));
        }

        public SerializableDateTime() => Value = DateTime.UtcNow;

        public SerializableDateTime(DateTime dateTime) => Value = dateTime;

        public static implicit operator DateTime(SerializableDateTime serializable) =>
            serializable?.Value ?? DateTime.UtcNow;

        public static implicit operator SerializableDateTime(DateTime dateTime) =>
            new(dateTime);

        public override string ToString() => FormatDisplay(Value);

        public static DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public static DateTime Normalize(DateTime value)
        {
            DateTime utc = ToUtc(value);
            return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc);
        }

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

        public static string FormatRoundTrip(DateTime utc) =>
            ToUtc(utc).ToString("O", CultureInfo.InvariantCulture);

        public static string FormatDisplay(DateTime value) =>
            new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind)
                .ToString(DateTimeFormat, CultureInfo.InvariantCulture);

        public static string FormatDate(DateTime value) =>
            value.ToString(DateFormat, CultureInfo.InvariantCulture);
    }
}
