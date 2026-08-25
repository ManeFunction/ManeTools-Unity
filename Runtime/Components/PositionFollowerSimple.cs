using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Moves this transform toward a target each frame with a constant catch-up factor.
    /// <see cref="Speed"/> is applied as-is (0 = stay, 1 = snap this frame).
    /// </summary>
    [AddComponentMenu("Mane Tools/Components/Position Follower (simple)")]
    public class PositionFollowerSimple : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField, Range(0f, 1f), Tooltip("Catch-up factor (0 = stay, 1 = snap).")]
        private float _speed = .2f;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        private void Update()
        {
            if (_target == null)
                return;

            transform.position = Vector3.Lerp(transform.position, _target.position, _speed);
        }
    }
}
