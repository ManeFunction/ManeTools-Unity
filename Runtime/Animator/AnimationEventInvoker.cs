using System;
using UnityEngine;

namespace Mane.Unity.Animator
{
    /// <summary>
    /// Raises <see cref="AnimationEventInvoked"/> from an animation event named <c>InvokeEvent</c>.
    /// </summary>
    [AddComponentMenu("Mane Tools/Animator/Event Invoker")]
    public class AnimationEventInvoker : MonoBehaviour
    {
        /// <summary>
        /// Fired when the animation event is invoked.
        /// </summary>
        public event Action AnimationEventInvoked;
        
        private void InvokeEvent() => AnimationEventInvoked?.Invoke();
    }
}
