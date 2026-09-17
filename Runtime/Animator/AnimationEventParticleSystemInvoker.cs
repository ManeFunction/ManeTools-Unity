using UnityEngine;

namespace Mane.Unity.Animator
{
    /// <summary>
    /// Plays a particle system from an animation event named <c>InvokeParticleSystem</c>.
    /// </summary>
    [ManeStyle]
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
