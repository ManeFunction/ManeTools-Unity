using System;
using NUnit.Framework;
using UnityEngine;

namespace Mane.Unity.Tests
{
    public class VectorExtensionsTests
    {
        [Test]
        public void Vector2_SetTranslateFlipAndArea()
        {
            Vector2 v = new(1f, 2f);

            Assert.AreEqual(new Vector2(5f, 2f), v.SetX(5f));
            Assert.AreEqual(new Vector2(1f, 5f), v.SetY(5f));
            Assert.AreEqual(new Vector2(4f, 6f), v.Translate(3f, 4f));
            Assert.AreEqual(new Vector2(-1f, 2f), v.FlipX());
            Assert.AreEqual(new Vector2(1f, -2f), v.FlipY());
            Assert.AreEqual(new Vector3(1f, 2f, 3f), v.AddZ(3f));
            Assert.AreEqual(6f, new Vector2(2f, 3f).Area());
        }

        [Test]
        public void Vector2_ClampAndDivide()
        {
            Assert.AreEqual(new Vector2(0f, 10f), new Vector2(-1f, 20f).Clamp(0f, 10f));
            Assert.AreEqual(new Vector2(2f, 3f), new Vector2(4f, 9f).Divide(new Vector2(2f, 3f)));
        }

        [Test]
        public void Vector2_IsInsideRectangle()
        {
            Rect rect = new(0f, 0f, 10f, 10f);

            Assert.IsTrue(new Vector2(5f, 5f).IsInsideRectangle(rect));
            Assert.IsFalse(new Vector2(-1f, 5f).IsInsideRectangle(rect));
        }

        [Test]
        public void Vector2_Average_EmptyIsZero()
        {
            Assert.AreEqual(Vector2.zero, Array.Empty<Vector2>().Average());
            Assert.AreEqual(new Vector2(2f, 3f), new[] { new Vector2(1f, 2f), new Vector2(3f, 4f) }.Average());
        }

        [Test]
        public void Vector3_SetTranslateFlipAndVolume()
        {
            Vector3 v = new(1f, 2f, 3f);

            Assert.AreEqual(new Vector3(5f, 2f, 3f), v.SetX(5f));
            Assert.AreEqual(new Vector3(1f, 5f, 3f), v.SetY(5f));
            Assert.AreEqual(new Vector3(1f, 2f, 5f), v.SetZ(5f));
            Assert.AreEqual(new Vector3(2f, 4f, 6f), v.Translate(1f, 2f, 3f));
            Assert.AreEqual(new Vector3(-1f, 2f, 3f), v.FlipX());
            Assert.AreEqual(6f, new Vector3(1f, 2f, 3f).Volume());
        }

        [Test]
        public void Vector3_Clamp_UsesEachComponent()
        {
            Vector3 clamped = new Vector3(-10f, 5f, 20f).Clamp(0f, 10f);

            Assert.AreEqual(new Vector3(0f, 5f, 10f), clamped);
        }

        [Test]
        public void Vector3_ClosestPointOnLine()
        {
            Vector3 a = Vector3.zero;
            Vector3 b = new(10f, 0f, 0f);

            Assert.AreEqual(new Vector3(4f, 0f, 0f), new Vector3(4f, 3f, 0f).ClosestPointOnLine(a, b));
            Assert.AreEqual(a, new Vector3(-2f, 1f, 0f).ClosestPointOnLine(a, b));
            Assert.AreEqual(b, new Vector3(12f, 1f, 0f).ClosestPointOnLine(a, b));
        }

        [Test]
        public void Vector3_IsInPolygon_ConvexTriangle()
        {
            Vector3[] triangle =
            {
                new(0f, 0f, 0f),
                new(4f, 0f, 0f),
                new(0f, 4f, 0f),
            };

            Assert.IsTrue(new Vector3(1f, 1f, 0f).IsInPolygon(triangle));
            Assert.IsFalse(new Vector3(3f, 3f, 0f).IsInPolygon(triangle));
        }
    }
}
