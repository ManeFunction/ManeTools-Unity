using System;
using UnityEngine;

namespace Mane.Unity.Animator
{
    [AddComponentMenu("Mane Tools/Animator/Event Invoker")]
    public class AnimationEventInvoker : MonoBehaviour
    {
        public event Action AnimationEventInvoked;
        
        private void InvokeEvent() => AnimationEventInvoked?.Invoke();
    }
}
