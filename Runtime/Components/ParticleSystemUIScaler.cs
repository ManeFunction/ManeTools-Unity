using UnityEngine;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace Mane.Unity
{
    /// <summary>
    /// Scales a <see cref="UnityEngine.ParticleSystem"/> emitter size and particle count by a <see cref="UnityEngine.RectTransform"/> size.
    /// Reference values are the anchor state: particle settings tuned for <see cref="ReferenceSize"/>.
    /// Shape scale axes mapped by <see cref="WidthAxis"/> / <see cref="HeightAxis"/> follow the rect width / height;
    /// emission rates, burst counts, and max particles follow the rect area, so particle density stays the same.
    /// </summary>
    [ManeStyle]
    [ExecuteAlways]
    [AddComponentMenu("Mane Tools/Components/Particle System UI Scaler")]
    public sealed class ParticleSystemUIScaler : MonoBehaviour
    {
        /// <summary>
        /// Axis of the particle system shape module.
        /// </summary>
        public enum ShapeAxis
        {
            /// <summary>
            /// Shape X axis.
            /// </summary>
            X,

            /// <summary>
            /// Shape Y axis.
            /// </summary>
            Y,

            /// <summary>
            /// Shape Z axis.
            /// </summary>
            Z
        }

        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private RectTransform _rectTransform;

        [Foldout("Reference Values")]
        [SerializeField, Label("Size"), Tooltip("Rect size the reference values are tuned for.")]
        private Vector2 _referenceSize = new(100f, 100f);
        [SerializeField, Label("Shape Scale"), Tooltip("Shape module scale at the reference size. The axis not mapped to width or height is not scaled.")]
        private Vector3 _referenceShapeScale = Vector3.one;
        [SerializeField, Label("Width Axis"), Tooltip("Shape axis that follows the rect width. Detected from the emitter rotation on copy.")]
        private ShapeAxis _widthAxis = ShapeAxis.X;
        [SerializeField, Label("Height Axis"), Tooltip("Shape axis that follows the rect height. Detected from the emitter rotation on copy.")]
        private ShapeAxis _heightAxis = ShapeAxis.Y;

        [NonBreakingSpace]
        [SerializeField, Label("Rate Over Time"), Tooltip("Emission rate over time at the reference size.")]
        private MinMaxCurve _referenceRateOverTime = new(10f);
        [SerializeField, Label("Rate Over Distance"), Tooltip("Emission rate over distance at the reference size.")]
        private MinMaxCurve _referenceRateOverDistance = new(0f);
        [SerializeField, Label("Bursts"), Tooltip("Burst counts at the reference size, by burst index.")]
        private MinMaxCurve[] _referenceBurstCounts = { };
        [SerializeField, Label("Max Particles"), Tooltip("Max particles at the reference size.")]
        private int _referenceMaxParticles = 1000;

        [SerializeField, HideInInspector] private bool _hasReferences;

        private Vector2 _lastSize = new(float.NaN, float.NaN);
        private bool _burstWarningShown;

        /// <summary>
        /// Particle system to scale.
        /// </summary>
        public ParticleSystem ParticleSystem
        {
            get => _particleSystem;
            set
            {
                _particleSystem = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Rect whose size drives the scale.
        /// </summary>
        public RectTransform RectTransform
        {
            get => _rectTransform;
            set
            {
                _rectTransform = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Rect size the reference values are tuned for.
        /// </summary>
        public Vector2 ReferenceSize => _referenceSize;

        /// <summary>
        /// Shape module scale at <see cref="ReferenceSize"/>.
        /// </summary>
        public Vector3 ReferenceShapeScale => _referenceShapeScale;

        /// <summary>
        /// Shape axis that follows the rect width.
        /// </summary>
        public ShapeAxis WidthAxis
        {
            get => _widthAxis;
            set
            {
                _widthAxis = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Shape axis that follows the rect height.
        /// </summary>
        public ShapeAxis HeightAxis
        {
            get => _heightAxis;
            set
            {
                _heightAxis = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Max particles at <see cref="ReferenceSize"/>.
        /// </summary>
        public int ReferenceMaxParticles => _referenceMaxParticles;

        private void Reset()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _rectTransform = transform as RectTransform;
            if (_rectTransform == null)
                _rectTransform = GetComponentInParent<RectTransform>();

            CaptureReferences();
        }

        private void OnEnable() => Invalidate();

        private void OnValidate() => Invalidate();

        private void LateUpdate()
        {
            if (_particleSystem == null || _rectTransform == null)
                return;

            if (!_hasReferences)
            {
                CaptureReferences();
                MarkDirty();
            }

            if (_rectTransform.rect.size != _lastSize)
                Apply();
        }

        /// <summary>
        /// Captures the current rect size and particle system values as the reference (anchor) state.
        /// </summary>
        [EditorButton("Copy Reference Values")]
        public void CopyReferenceValues()
        {
            if (_particleSystem == null || _rectTransform == null)
            {
                Debug.LogWarning($"[{nameof(ParticleSystemUIScaler)}] Assign a particle system and a rect transform first.", this);
                return;
            }

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Copy Reference Values");
#endif
            CaptureReferences();
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
        }

        /// <summary>
        /// Scales the particle system from the reference values to the current rect size.
        /// Called automatically when the rect size changes.
        /// </summary>
        public void Apply()
        {
            if (_particleSystem == null || _rectTransform == null)
                return;

            Vector2 size = _rectTransform.rect.size;
            _lastSize = size;

            float scaleX = Factor(size.x, _referenceSize.x);
            float scaleY = Factor(size.y, _referenceSize.y);
            float area = scaleX * scaleY;

            Vector3 shapeScale = _referenceShapeScale;
            shapeScale[(int)_widthAxis] *= scaleX;
            shapeScale[(int)_heightAxis] *= scaleY;
            ParticleSystem.ShapeModule shape = _particleSystem.shape;
            shape.scale = shapeScale;

            ParticleSystem.EmissionModule emission = _particleSystem.emission;
            emission.rateOverTime = Scale(_referenceRateOverTime, area);
            emission.rateOverDistance = Scale(_referenceRateOverDistance, area);

            if (_referenceBurstCounts != null)
            {
                if (emission.burstCount != _referenceBurstCounts.Length && !_burstWarningShown)
                {
                    _burstWarningShown = true;
                    Debug.LogWarning(
                        $"[{nameof(ParticleSystemUIScaler)}] Particle system bursts changed after the reference values were copied. " +
                        "Press Copy Reference Values to scale all of them.", this);
                }

                int count = Mathf.Min(emission.burstCount, _referenceBurstCounts.Length);
                for (int i = 0; i < count; i++)
                {
                    ParticleSystem.Burst burst = emission.GetBurst(i);
                    burst.count = Scale(_referenceBurstCounts[i], area);
                    emission.SetBurst(i, burst);
                }
            }

            ParticleSystem.MainModule main = _particleSystem.main;
            main.maxParticles = Mathf.Max(1, Mathf.RoundToInt(_referenceMaxParticles * area));
        }

        private void CaptureReferences()
        {
            if (_particleSystem == null || _rectTransform == null)
                return;

            _referenceSize = _rectTransform.rect.size;
            _referenceShapeScale = _particleSystem.shape.scale;
            DetectShapeAxes();

            ParticleSystem.EmissionModule emission = _particleSystem.emission;
            _referenceRateOverTime = emission.rateOverTime;
            _referenceRateOverDistance = emission.rateOverDistance;

            _referenceBurstCounts = new MinMaxCurve[emission.burstCount];
            for (int i = 0; i < _referenceBurstCounts.Length; i++)
                _referenceBurstCounts[i] = emission.GetBurst(i).count;

            _referenceMaxParticles = _particleSystem.main.maxParticles;
            _hasReferences = true;
            _burstWarningShown = false;
            _lastSize = _referenceSize;
        }

        // Maps the rect right / up directions into shape space and picks the dominant axis for each.
        private void DetectShapeAxes()
        {
            Quaternion toShape = Quaternion.Inverse(
                _particleSystem.transform.rotation * Quaternion.Euler(_particleSystem.shape.rotation));
            ShapeAxis width = DominantAxis(toShape * _rectTransform.right);
            ShapeAxis height = DominantAxis(toShape * _rectTransform.up);

            if (width == height)
            {
                width = ShapeAxis.X;
                height = ShapeAxis.Y;
            }

            _widthAxis = width;
            _heightAxis = height;
        }

        private static ShapeAxis DominantAxis(Vector3 direction)
        {
            float x = Mathf.Abs(direction.x);
            float y = Mathf.Abs(direction.y);
            float z = Mathf.Abs(direction.z);

            if (x >= y && x >= z)
                return ShapeAxis.X;
            return y >= z ? ShapeAxis.Y : ShapeAxis.Z;
        }

        private void Invalidate() => _lastSize = new Vector2(float.NaN, float.NaN);

        private void MarkDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private static float Factor(float size, float reference) =>
            reference > 0f ? Mathf.Max(0f, size) / reference : 1f;

        private static MinMaxCurve Scale(MinMaxCurve curve, float factor)
        {
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    curve.constant *= factor;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    curve.constantMin *= factor;
                    curve.constantMax *= factor;
                    break;
                default:
                    curve.curveMultiplier *= factor;
                    break;
            }

            return curve;
        }
    }
}
