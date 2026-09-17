using System;
using NUnit.Framework;

namespace Mane.Unity.Tests
{
    public class SerializableDateTimeTests
    {
        [Test]
        public void DefaultConstructor_IsUtcNow()
        {
            DateTime before = DateTime.UtcNow;
            SerializableDateTime serializable = new();
            DateTime after = DateTime.UtcNow;

            Assert.AreEqual(DateTimeKind.Utc, serializable.Value.Kind);
            Assert.GreaterOrEqual(serializable.Value, before.AddSeconds(-1));
            Assert.LessOrEqual(serializable.Value, after.AddSeconds(1));
        }

        [Test]
        public void Constructor_StoresUtc()
        {
            DateTime utc = new(2026, 9, 16, 12, 30, 45, DateTimeKind.Utc);
            SerializableDateTime serializable = new(utc);

            Assert.AreEqual(utc, serializable.Value);
            Assert.AreEqual(DateTimeKind.Utc, serializable.Value.Kind);
        }

        [Test]
        public void ToUtc_Unspecified_TreatedAsUtc()
        {
            DateTime unspecified = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);
            DateTime utc = SerializableDateTime.ToUtc(unspecified);

            Assert.AreEqual(DateTimeKind.Utc, utc.Kind);
            Assert.AreEqual(unspecified.Ticks, utc.Ticks);
        }

        [Test]
        public void ToUtc_Local_ConvertsToUniversalTime()
        {
            DateTime local = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Local);
            Assert.AreEqual(local.ToUniversalTime(), SerializableDateTime.ToUtc(local));
        }

        [Test]
        public void Normalize_DropsSeconds()
        {
            DateTime utc = new(2026, 9, 16, 12, 30, 45, 123, DateTimeKind.Utc);
            DateTime normalized = SerializableDateTime.Normalize(utc);

            Assert.AreEqual(new DateTime(2026, 9, 16, 12, 30, 0, DateTimeKind.Utc), normalized);
        }

        [Test]
        public void FormatRoundTrip_TryParse_RoundTrip()
        {
            DateTime utc = new(2026, 9, 16, 18, 45, 0, DateTimeKind.Utc);
            string iso = SerializableDateTime.FormatRoundTrip(utc);

            Assert.IsTrue(SerializableDateTime.TryParse(iso, out DateTime parsed));
            Assert.AreEqual(utc, parsed);
            Assert.AreEqual(DateTimeKind.Utc, parsed.Kind);
        }

        [Test]
        public void TryParse_Empty_ReturnsFalse()
        {
            Assert.IsFalse(SerializableDateTime.TryParse(null, out _));
            Assert.IsFalse(SerializableDateTime.TryParse(string.Empty, out _));
        }

        [Test]
        public void ImplicitOperators_ConvertBothWays()
        {
            DateTime utc = new(2026, 3, 4, 5, 6, 0, DateTimeKind.Utc);
            SerializableDateTime serializable = utc;
            DateTime back = serializable;

            Assert.AreEqual(utc, serializable.Value);
            Assert.AreEqual(utc, back);
        }

        [Test]
        public void ImplicitFromNull_ReturnsUtcNow()
        {
            DateTime before = DateTime.UtcNow;
            DateTime value = (SerializableDateTime)null;
            DateTime after = DateTime.UtcNow;

            Assert.GreaterOrEqual(value, before.AddSeconds(-1));
            Assert.LessOrEqual(value, after.AddSeconds(1));
        }

        [Test]
        public void FormatDisplay_MatchesToString()
        {
            DateTime utc = new(2026, 9, 16, 12, 30, 45, DateTimeKind.Utc);
            SerializableDateTime serializable = new(utc);

            Assert.AreEqual("2026-09-16 12:30", serializable.ToString());
            Assert.AreEqual("2026-09-16", SerializableDateTime.FormatDate(utc));
        }
    }
}
