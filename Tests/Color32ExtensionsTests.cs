using NUnit.Framework;
using UnityEngine;

namespace Mane.Unity.Tests
{
    public class Color32ExtensionsTests
    {
        [Test]
        public void ToHex_IncludesRgbAndAlpha()
        {
            Color32 color = new(255, 0, 128, 64);
            Assert.AreEqual("#FF008040", color.ToHex());
        }

        [Test]
        public void ToUInt_ToColor32_RoundTrip()
        {
            Color32 original = new(10, 20, 30, 40);
            Assert.AreEqual(original, original.ToUInt().ToColor32());
        }

        [Test]
        public void SetChannels_ReturnCopyWithNewValues()
        {
            Color32 color = new(1, 2, 3, 4);

            Assert.AreEqual(new Color32(9, 2, 3, 4), color.SetR(9));
            Assert.AreEqual(new Color32(1, 9, 3, 4), color.SetG(9));
            Assert.AreEqual(new Color32(1, 2, 9, 4), color.SetB(9));
            Assert.AreEqual(new Color32(1, 2, 3, 9), color.SetA(9));
            Assert.AreEqual(new Color32(7, 8, 9, 4), color.SetRGB(7, 8, 9));
            Assert.AreEqual(new Color32(7, 8, 9, 4), color.SetRGB(new Color32(7, 8, 9, 255)));
        }

        [Test]
        public void Shift_ClampsChannels()
        {
            Color32 color = new(250, 10, 0, 128);
            Color32 shifted = color.Shift(10);

            Assert.AreEqual(255, shifted.r);
            Assert.AreEqual(20, shifted.g);
            Assert.AreEqual(10, shifted.b);
            Assert.AreEqual(128, shifted.a);
        }
    }
}
