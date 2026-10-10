using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for wrapping tasks, delayed invokes, and stopping coroutines.
    /// </summary>
    public static class CoroutineExtensions
    {
        /// <summary>
        /// Yields until <paramref name="task"/> completes, then invokes <paramref name="callback"/>
        /// with <c>(true, result)</c>, or <c>(false, default)</c> if the task faulted or was canceled.
        /// </summary>
        public static IEnumerator ToCoroutine<T>(this Task<T> task, Action<bool, T> callback)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.Status == TaskStatus.RanToCompletion)
                callback?.Invoke(true, task.Result);
            else
                callback?.Invoke(false, default);
        }

        /// <summary>
        /// Invokes <paramref name="action"/> after <paramref name="delay"/> seconds, or immediately if delay is 0.
        /// </summary>
        public static Coroutine Delayed(this MonoBehaviour target, Action action, float delay)
        {
            if (action == null) return null;

            if (delay <= 0f)
            {
                action.Invoke();
                
                return null;
            }
            
            return target.StartCoroutine(Coroutine());

            
            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(delay);
                
                action.Invoke();
            }
        }

        /// <summary>
        /// Invokes <paramref name="action"/> after <paramref name="frames"/> frames, or immediately if 0.
        /// </summary>
        public static Coroutine DelayedFrames(this MonoBehaviour target, Action action, int frames)
        {
            if (action == null) return null;

            if (frames <= 0)
            {
                action.Invoke();
                
                return null;
            }
            
            return target.StartCoroutine(Coroutine());

            
            IEnumerator Coroutine()
            {
                while (frames-- > 0)
                {
                    yield return null;
                }

                action.Invoke();
            }
        }

        /// <summary>
        /// Stops <paramref name="coroutine"/> and clears the reference. Returns false if there was nothing to stop.
        /// </summary>
        public static bool TryKillCoroutine(this MonoBehaviour target, ref Coroutine coroutine)
        {
            if (coroutine == null || !target) return false;

            target.StopCoroutine(coroutine);
            coroutine = null;
            
            return true;
        }
    }
}
