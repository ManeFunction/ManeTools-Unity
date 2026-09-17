using NUnit.Framework;
using UnityEngine;

namespace Mane.Unity.Tests
{
    public class ColorExtensionsTests
    {
        [Test]
        public void ToHex_UsesColor32Bytes()
        {
            Assert.AreEqual("#FF0000FF", Color.red.ToHex());
        }

        [Test]
        public void GetLuma_WhiteIsOne_BlackIsZero()
        {
            Assert.AreEqual(1f, Color.white.GetLuma());
            Assert.AreEqual(0f, Color.black.GetLuma());
            Assert.AreEqual(.2f, Color.red.GetLuma());
        }

        [Test]
        public void GetHSL_Lightness_BlackWhiteGray()
        {
            Assert.AreEqual(0f, Color.black.GetHSL_Lightness());
            Assert.AreEqual(1f, Color.white.GetHSL_Lightness());
            Assert.AreEqual(.5f, Color.gray.GetHSL_Lightness());
        }

        [Test]
        public void GetHSL_Saturation_GrayIsZero_RedIsOne()
        {
            Assert.AreEqual(0f, Color.gray.GetHSL_Saturation());
            Assert.AreEqual(1f, Color.red.GetHSL_Saturation());
        }

        [Test]
        public void GetHSL_Hue_PrimaryColors()
        {
            Assert.AreEqual(0f, Color.red.GetHSL_Hue(), .001f);
            Assert.AreEqual(1f / 3f, Color.green.GetHSL_Hue(), .001f);
            Assert.AreEqual(2f / 3f, Color.blue.GetHSL_Hue(), .001f);
        }

        [Test]
        public void SetChannels_ReturnCopyWithNewValues()
        {
            Color color = new(.1f, .2f, .3f, .4f);

            Assert.AreEqual(new Color(.9f, .2f, .3f, .4f), color.SetR(.9f));
            Assert.AreEqual(new Color(.1f, .9f, .3f, .4f), color.SetG(.9f));
            Assert.AreEqual(new Color(.1f, .2f, .9f, .4f), color.SetB(.9f));
            Assert.AreEqual(new Color(.1f, .2f, .3f, .9f), color.SetA(.9f));
            Assert.AreEqual(new Color(.7f, .8f, .9f, .4f), color.SetRGB(.7f, .8f, .9f));
            Assert.AreEqual(new Color(.7f, .8f, .9f, .4f), color.SetRGB(new Color(.7f, .8f, .9f)));
        }

        [Test]
        public void Shift_ClampsTo01()
        {
            Color shifted = new Color(.9f, .05f, 0f, .5f).Shift(.2f);

            Assert.AreEqual(1f, shifted.r);
            Assert.AreEqual(.25f, shifted.g, .001f);
            Assert.AreEqual(.2f, shifted.b, .001f);
            Assert.AreEqual(.5f, shifted.a);
        }
    }
}
