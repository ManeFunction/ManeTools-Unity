using System;
using NUnit.Framework;

namespace Mane.Unity.Tests
{
    public class SerializableDateTimeRangeTests
    {
        [Test]
        public void DefaultConstructor_SpansAboutAWeek()
        {
            SerializableDateTimeRange range = new();
            TimeSpan duration = range.End - range.Start;

            Assert.AreEqual(DateTimeKind.Utc, range.Start.Kind);
            Assert.AreEqual(DateTimeKind.Utc, range.End.Kind);
            Assert.Greater(duration, TimeSpan.FromDays(6.9));
            Assert.Less(duration, TimeSpan.FromDays(7.1));
            Assert.IsNotEmpty(range.StartString);
            Assert.IsNotEmpty(range.EndString);
        }

        [Test]
        public void Constructor_StoresStartAndEnd()
        {
            DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            SerializableDateTimeRange range = new(start, end);

            Assert.AreEqual(start, range.Start);
            Assert.AreEqual(end, range.End);
        }

        [Test]
        public void Constructor_SwapsWhenStartIsAfterEnd()
        {
            DateTime start = new(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            SerializableDateTimeRange range = new(start, end);

            Assert.AreEqual(end, range.Start);
            Assert.AreEqual(start, range.End);
        }

        [Test]
        public void Setters_RoundTripIso()
        {
            SerializableDateTimeRange range = new();
            DateTime start = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);
            DateTime end = new(2026, 5, 2, 18, 30, 0, DateTimeKind.Utc);

            range.Start = start;
            range.End = end;

            Assert.AreEqual(start, range.Start);
            Assert.AreEqual(end, range.End);
            Assert.IsTrue(SerializableDateTime.TryParse(range.StartString, out DateTime parsedStart));
            Assert.AreEqual(start, parsedStart);
        }

        [Test]
        public void ToString_ContainsDisplayDates()
        {
            DateTime start = new(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);
            DateTime end = new(2026, 9, 2, 11, 15, 0, DateTimeKind.Utc);
            SerializableDateTimeRange range = new(start, end);

            string text = range.ToString();
            Assert.IsTrue(text.Contains("2026-09-01 10:00"));
            Assert.IsTrue(text.Contains("2026-09-02 11:15"));
        }
    }
}
