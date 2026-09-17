using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Raises <see cref="OnStateChanged"/> when this object is enabled, disabled, or destroyed.
    /// </summary>
    [AddComponentMenu("Mane Tools/Components/State Change Detector")]
    public sealed class StateChangeDetector : MonoBehaviour
    {
        /// <summary>
        /// Lifecycle state reported by <see cref="OnStateChanged"/>.
        /// </summary>
        public enum State : byte
        {
            /// <summary>
            /// The object was enabled.
            /// </summary>
            Enabled = 1 << 0,

            /// <summary>
            /// The object was disabled.
            /// </summary>
            Disabled = 1 << 1,

            /// <summary>
            /// The object was destroyed.
            /// </summary>
            Destroyed = 1 << 2,
        }
        
        /// <summary>
        /// Fired with this GameObject and the new <see cref="State"/>.
        /// </summary>
        public event Action<GameObject, State> OnStateChanged;
        
        private void OnEnable() => OnStateChanged?.Invoke(gameObject, State.Enabled);

        private void OnDisable() => OnStateChanged?.Invoke(gameObject, State.Disabled);

        private void OnDestroy() => OnStateChanged?.Invoke(gameObject, State.Destroyed);
    }
}
