using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Moves this transform toward a target each frame.
    /// The curve is sampled by current distance to the target (not time):
    /// X is lag in world units, last key is "far enough" for full weight;
    /// Y is the catch-up factor (0 = stay, 1 = snap this frame).
    /// </summary>
    [AddComponentMenu("Mane Tools/Components/Position Follower (by distance)")]
    public class PositionFollowerByDistance : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField, Tooltip("X is a distance range to the target. Y is catch-up factor (0 = stay, 1 = snap).")]
        private AnimationCurve _animationCurve = AnimationCurve.Linear(10f, 0f, 100f, .3f);

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public AnimationCurve AnimationCurve
        {
            get => _animationCurve;
            set => _animationCurve = value;
        }

        private void Update()
        {
            if (_target == null)
                return;

            Vector3 current = transform.position;
            Vector3 destination = _target.position;
            float sqrLag = (destination - current).sqrMagnitude;
            if (sqrLag <= 0f)
                return;

            float lag = Mathf.Sqrt(sqrLag);
            float factor = Mathf.Clamp01(_animationCurve.Evaluate(SampleTime(_animationCurve, lag)));
            if (factor <= 0f)
                return;

            float t = 1f - Mathf.Pow(1f - factor, Time.deltaTime * 60f);
            transform.position = Vector3.Lerp(current, destination, t);
        }

        private static float SampleTime(AnimationCurve curve, float distance)
        {
            if (curve == null || curve.length == 0)
                return 0f;

            float lastTime = curve[curve.length - 1].time;
            if (lastTime <= 0f)
                return 0f;

            return Mathf.Min(distance, lastTime);
        }
    }
}
