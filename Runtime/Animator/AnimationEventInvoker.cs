using UnityEngine;
using UnityEngine.Events;

namespace Mane.Unity.Animator
{
    /// <summary>
    /// Raises <see cref="AnimationEventInvoked"/> from an animation event named <c>InvokeEvent</c>.
    /// </summary>
    [ManeStyle]
    [AddComponentMenu("Mane Tools/Animator/Event Invoker")]
    public class AnimationEventInvoker : MonoBehaviour
    {
        [SerializeField] private UnityEvent onAnimationEventInvoked = new();

        /// <summary>
        /// Fired when the animation event is invoked.
        /// </summary>
        public event UnityAction AnimationEventInvoked
        {
            add => onAnimationEventInvoked.AddListener(value);
            remove => onAnimationEventInvoked.RemoveListener(value);
        }

        private void InvokeEvent() => onAnimationEventInvoked.Invoke();
    }
}
