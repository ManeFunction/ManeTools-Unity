using UnityEngine;

namespace Mane.Unity.Animator
{
    [AddComponentMenu("Mane Tools/Animator/Event Particle System Invoker")]
    public class AnimationEventParticleSystemInvoker : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        
        private void InvokeParticleSystem()
        {
            if (_particleSystem)
                _particleSystem.Play();
        }
    }
}
