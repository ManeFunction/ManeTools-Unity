using NUnit.Framework;

namespace Mane.Unity.Tests
{
    public class UnityRandomTests
    {
        [Test]
        public void Range01_TopStepStaysBelowOne()
        {
            Assert.AreEqual(0f, UnityRandom.ToRange01(0));
            Assert.Less(UnityRandom.ToRange01(UnityRandom.FloatSteps - 1), 1f);
            Assert.AreEqual(1f - 1f / UnityRandom.FloatSteps, UnityRandom.ToRange01(UnityRandom.FloatSteps - 1));
        }

        [Test]
        public void Range01Inc_TopStepIsOne() =>
            Assert.AreEqual(1f, UnityRandom.ToRange01(UnityRandom.FloatSteps));

        [Test]
        public void Range01Inc_IsInClosedUnitInterval()
        {
            UnityRandom random = new(1);
            for (int i = 0; i < 50; i++)
            {
                float f = random.Range01Inc();
                Assert.GreaterOrEqual(f, 0f);
                Assert.LessOrEqual(f, 1f);
            }
        }

        [Test]
        public void Range01_IsInUnitInterval()
        {
            UnityRandom random = new(1);
            for (int i = 0; i < 50; i++)
            {
                float f = random.Range01();
                double d = random.Range01Double();
                Assert.GreaterOrEqual(f, 0f);
                Assert.Less(f, 1f);
                Assert.GreaterOrEqual(d, 0d);
                Assert.Less(d, 1d);
            }
        }
    }
}
