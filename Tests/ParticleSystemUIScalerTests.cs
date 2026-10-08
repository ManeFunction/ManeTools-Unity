using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mane.Unity.Tests
{
    public class ParticleSystemUIScalerTests
    {
        private GameObject _go;
        private RectTransform _rect;
        private ParticleSystem _particles;
        private ParticleSystemUIScaler _scaler;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ParticleSystemUIScaler", typeof(RectTransform));
            _rect = (RectTransform)_go.transform;
            _rect.sizeDelta = new Vector2(100f, 50f);

            _particles = _go.AddComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.scale = new Vector3(2f, 1f, 3f);

            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = 10f;
            emission.rateOverDistance = 4f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 5) });

            ParticleSystem.MainModule main = _particles.main;
            main.maxParticles = 100;

            _scaler = _go.AddComponent<ParticleSystemUIScaler>();
            _scaler.ParticleSystem = _particles;
            _scaler.RectTransform = _rect;
            _scaler.CopyReferenceValues();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void CopyReferenceValues_CapturesRectAndParticleState()
        {
            Assert.AreEqual(new Vector2(100f, 50f), _scaler.ReferenceSize);
            Assert.AreEqual(new Vector3(2f, 1f, 3f), _scaler.ReferenceShapeScale);
            Assert.AreEqual(100, _scaler.ReferenceMaxParticles);
        }

        [Test]
        public void Apply_DoubleWidth_DoublesShapeXAndCounts()
        {
            _rect.sizeDelta = new Vector2(200f, 50f);
            _scaler.Apply();

            Assert.AreEqual(new Vector3(4f, 1f, 3f), _particles.shape.scale);
            Assert.AreEqual(20f, _particles.emission.rateOverTime.constant, 1e-4f);
            Assert.AreEqual(8f, _particles.emission.rateOverDistance.constant, 1e-4f);
            Assert.AreEqual(10f, _particles.emission.GetBurst(0).count.constant, 1e-4f);
            Assert.AreEqual(200, _particles.main.maxParticles);
        }

        [Test]
        public void Apply_DoubleBothAxes_ScalesCountsByArea()
        {
            _rect.sizeDelta = new Vector2(200f, 100f);
            _scaler.Apply();

            Assert.AreEqual(new Vector3(4f, 2f, 3f), _particles.shape.scale);
            Assert.AreEqual(40f, _particles.emission.rateOverTime.constant, 1e-4f);
            Assert.AreEqual(400, _particles.main.maxParticles);
        }

        [Test]
        public void Apply_AtReferenceSize_RestoresReferenceValues()
        {
            _rect.sizeDelta = new Vector2(300f, 20f);
            _scaler.Apply();
            _rect.sizeDelta = new Vector2(100f, 50f);
            _scaler.Apply();

            Assert.AreEqual(new Vector3(2f, 1f, 3f), _particles.shape.scale);
            Assert.AreEqual(10f, _particles.emission.rateOverTime.constant, 1e-4f);
            Assert.AreEqual(5f, _particles.emission.GetBurst(0).count.constant, 1e-4f);
            Assert.AreEqual(100, _particles.main.maxParticles);
        }

        [Test]
        public void Apply_TwoConstantsRate_ScalesBothBounds()
        {
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(2f, 6f);
            _scaler.CopyReferenceValues();

            _rect.sizeDelta = new Vector2(200f, 50f);
            _scaler.Apply();

            ParticleSystem.MinMaxCurve rate = _particles.emission.rateOverTime;
            Assert.AreEqual(ParticleSystemCurveMode.TwoConstants, rate.mode);
            Assert.AreEqual(4f, rate.constantMin, 1e-4f);
            Assert.AreEqual(12f, rate.constantMax, 1e-4f);
        }

        [Test]
        public void Apply_ZeroSize_KeepsMaxParticlesPositive()
        {
            _rect.sizeDelta = Vector2.zero;
            _scaler.Apply();

            Assert.AreEqual(Vector3.forward * 3f, _particles.shape.scale);
            Assert.AreEqual(0f, _particles.emission.rateOverTime.constant, 1e-4f);
            Assert.AreEqual(1, _particles.main.maxParticles);
        }

        [Test]
        public void Apply_ZeroReferenceSize_UsesFactorOne()
        {
            _rect.sizeDelta = Vector2.zero;
            _scaler.CopyReferenceValues();

            _rect.sizeDelta = new Vector2(200f, 50f);
            _scaler.Apply();

            Assert.AreEqual(new Vector3(2f, 1f, 3f), _particles.shape.scale);
            Assert.AreEqual(100, _particles.main.maxParticles);
        }

        [Test]
        public void ApplyAndCopy_MissingTargets_DoNotThrow()
        {
            _scaler.ParticleSystem = null;
            _scaler.RectTransform = null;

            Assert.DoesNotThrow(() => _scaler.Apply());
            Assert.DoesNotThrow(() => _scaler.CopyReferenceValues());
        }

        [Test]
        public void CopyReferenceValues_UnrotatedEmitter_MapsWidthXHeightY()
        {
            Assert.AreEqual(ParticleSystemUIScaler.ShapeAxis.X, _scaler.WidthAxis);
            Assert.AreEqual(ParticleSystemUIScaler.ShapeAxis.Y, _scaler.HeightAxis);
        }

        [Test]
        public void CopyReferenceValues_DefaultEmitterRotation_MapsHeightToZ()
        {
            GameObject child = new("Emitter");
            child.transform.SetParent(_go.transform, false);
            child.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            ParticleSystem particles = child.AddComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.scale = new Vector3(2f, 1f, 3f);

            _scaler.ParticleSystem = particles;
            _scaler.CopyReferenceValues();

            Assert.AreEqual(ParticleSystemUIScaler.ShapeAxis.X, _scaler.WidthAxis);
            Assert.AreEqual(ParticleSystemUIScaler.ShapeAxis.Z, _scaler.HeightAxis);

            _rect.sizeDelta = new Vector2(100f, 100f);
            _scaler.Apply();

            Assert.AreEqual(new Vector3(2f, 1f, 6f), particles.shape.scale);
        }

        [Test]
        public void Apply_CurveRate_ScalesCurveMultiplier()
        {
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(5f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
            _scaler.CopyReferenceValues();

            _rect.sizeDelta = new Vector2(200f, 50f);
            _scaler.Apply();

            ParticleSystem.MinMaxCurve rate = _particles.emission.rateOverTime;
            Assert.AreEqual(ParticleSystemCurveMode.Curve, rate.mode);
            Assert.AreEqual(10f, rate.curveMultiplier, 1e-4f);
        }

        [Test]
        public void Apply_TwoConstantsRateOverDistance_ScalesBothBounds()
        {
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverDistance = new ParticleSystem.MinMaxCurve(1f, 3f);
            _scaler.CopyReferenceValues();

            _rect.sizeDelta = new Vector2(200f, 100f);
            _scaler.Apply();

            ParticleSystem.MinMaxCurve rate = _particles.emission.rateOverDistance;
            Assert.AreEqual(4f, rate.constantMin, 1e-4f);
            Assert.AreEqual(12f, rate.constantMax, 1e-4f);
        }

        [Test]
        public void Apply_BurstAddedAfterCopy_KeepsExtraBurstAndWarns()
        {
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 5), new ParticleSystem.Burst(1f, 7) });

            LogAssert.Expect(LogType.Warning, new Regex("bursts changed"));
            _rect.sizeDelta = new Vector2(200f, 50f);
            _scaler.Apply();

            Assert.AreEqual(10f, _particles.emission.GetBurst(0).count.constant, 1e-4f);
            Assert.AreEqual(7f, _particles.emission.GetBurst(1).count.constant, 1e-4f);
        }
    }
}
