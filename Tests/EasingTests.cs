using NUnit.Framework;
using UnityEngine;

namespace Mane.Unity.Tests
{
    public class EasingTests
    {
        [Test]
        public void Endpoints_AreZeroAndOne()
        {
            AnimationCurve[] curves =
            {
                Easing.Linear,
                Easing.QuadIn, Easing.QuadOut, Easing.QuadInOut,
                Easing.CubicIn, Easing.CubicOut, Easing.CubicInOut,
                Easing.QuartIn, Easing.QuartOut, Easing.QuartInOut,
                Easing.QuintIn, Easing.QuintOut, Easing.QuintInOut,
                Easing.SinIn, Easing.SinOut, Easing.SinInOut,
                Easing.ExpoIn, Easing.ExpoOut, Easing.ExpoInOut,
                Easing.CircIn, Easing.CircOut, Easing.CircInOut,
                Easing.BackIn, Easing.BackOut, Easing.BackInOut,
                Easing.BounceIn, Easing.BounceOut, Easing.BounceInOut,
                Easing.ElasticIn, Easing.ElasticOut, Easing.ElasticInOut,
                Easing.Spring
            };

            for (int i = 0; i < curves.Length; i++)
            {
                Assert.AreEqual(0f, curves[i].Evaluate(0f), .0001f);
                Assert.AreEqual(1f, curves[i].Evaluate(1f), .0001f);
            }
        }

        [Test]
        public void Samples_MatchKnownValues()
        {
            Assert.AreEqual(.25f, Easing.QuadIn.Evaluate(.5f), .0001f);
            Assert.AreEqual(.75f, Easing.QuadOut.Evaluate(.5f), .0001f);
            Assert.AreEqual(.125f, Easing.QuadInOut.Evaluate(.25f), .0001f);
            Assert.AreEqual(.125f, Easing.CubicIn.Evaluate(.5f), .0001f);
            Assert.AreEqual(.0625f, Easing.CubicInOut.Evaluate(.25f), .0001f);
            Assert.AreEqual(-.0877f, Easing.BackIn.Evaluate(.5f), .0001f);
            Assert.AreEqual(1.0877f, Easing.BackOut.Evaluate(.5f), .0001f);
            Assert.AreEqual(.3025f, Easing.BounceOut.Evaluate(.2f), .0001f);
            Assert.AreEqual(.765625f, Easing.BounceOut.Evaluate(.5f), .0001f);

            Assert.AreEqual(.0625f, Easing.QuartIn.Evaluate(.5f), .002f);
            Assert.AreEqual(.03125f, Easing.QuintIn.Evaluate(.5f), .002f);
            Assert.AreEqual(.2929f, Easing.SinIn.Evaluate(.5f), .002f);
            Assert.AreEqual(.7071f, Easing.SinOut.Evaluate(.5f), .002f);
            Assert.AreEqual(.03125f, Easing.ExpoIn.Evaluate(.5f), .002f);
            Assert.AreEqual(.134f, Easing.CircIn.Evaluate(.5f), .002f);
            Assert.Greater(Easing.ElasticOut.Evaluate(.15f), 1f);
            Assert.Greater(Easing.Spring.Evaluate(.5f), 1f);
        }

        [Test]
        public void Clone_DoesNotChangeThePreset()
        {
            AnimationCurve copy = Easing.Clone(Easing.Linear);
            Assert.AreNotSame(Easing.Linear, copy);

            copy.AddKey(.2f, 0f);
            Assert.AreEqual(.2f, Easing.Linear.Evaluate(.2f), .0001f);
        }
    }
}
