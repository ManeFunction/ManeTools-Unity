using System;
using UnityEngine;

namespace Mane.Unity
{
    [AddComponentMenu("Mane Tools/Components/State Change Detector")]
    public sealed class StateChangeDetector : MonoBehaviour
    {
        public enum State : byte
        {
            Enabled = 1 << 0,
            Disabled = 1 << 1,
            Destroyed = 1 << 2,
        }
        
        public event Action<GameObject, State> OnStateChanged;
        
        private void OnEnable() => OnStateChanged?.Invoke(gameObject, State.Enabled);

        private void OnDisable() => OnStateChanged?.Invoke(gameObject, State.Disabled);

        private void OnDestroy() => OnStateChanged?.Invoke(gameObject, State.Destroyed);
    }
}
