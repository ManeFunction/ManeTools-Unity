using System;
using System.Reflection;
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

        [Test]
        public void Get_ReturnsThePropertyOfTheSameName()
        {
            foreach (EasingPreset preset in Enum.GetValues(typeof(EasingPreset)))
            {
                if (preset == EasingPreset.Custom)
                    continue;

                PropertyInfo property = typeof(Easing).GetProperty(preset.ToString(), BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(property, preset.ToString());
                Assert.AreSame(property.GetValue(null), Easing.Get(preset), preset.ToString());
            }
        }

        [Test]
        public void Get_EveryPresetPropertyHasAnEnumValue()
        {
            foreach (PropertyInfo property in typeof(Easing).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (property.PropertyType == typeof(AnimationCurve))
                    Assert.IsTrue(Enum.IsDefined(typeof(EasingPreset), property.Name), property.Name);
            }
        }

        [Test]
        public void Get_CustomIsNull_UndefinedThrows()
        {
            Assert.IsNull(Easing.Get(EasingPreset.Custom));
            Assert.Throws<ArgumentOutOfRangeException>(() => Easing.Get((EasingPreset)999));
        }

        [Test]
        public void FindPreset_RecognizesCopies()
        {
            foreach (EasingPreset preset in Enum.GetValues(typeof(EasingPreset)))
            {
                if (preset == EasingPreset.Custom)
                    continue;

                Assert.AreEqual(preset, Easing.FindPreset(Easing.Get(preset)), preset.ToString());
                Assert.AreEqual(preset, Easing.FindPreset(Easing.Clone(Easing.Get(preset))), preset.ToString());
                // Wrap modes are not part of the preset.
                AnimationCurve looped = new(Easing.Get(preset).keys) { postWrapMode = WrapMode.Loop };
                Assert.AreEqual(preset, Easing.FindPreset(looped), preset.ToString());
            }
        }

        [Test]
        public void FindPreset_OtherCurves_AreCustom()
        {
            Assert.AreEqual(EasingPreset.Custom, Easing.FindPreset(null));
            Assert.AreEqual(EasingPreset.Custom, Easing.FindPreset(new AnimationCurve()));
            Assert.AreEqual(EasingPreset.Custom, Easing.FindPreset(AnimationCurve.EaseInOut(0f, 0f, 1f, 2f)));

            AnimationCurve moved = Easing.Clone(Easing.CubicOut);
            Keyframe key = moved.keys[1];
            key.value = .9f;
            moved.MoveKey(1, key);
            Assert.AreEqual(EasingPreset.Custom, Easing.FindPreset(moved));

            AnimationCurve extra = Easing.Clone(Easing.Linear);
            extra.AddKey(.5f, .5f);
            Assert.AreEqual(EasingPreset.Custom, Easing.FindPreset(extra));
        }
    }
}
