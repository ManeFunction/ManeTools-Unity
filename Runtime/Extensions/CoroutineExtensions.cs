using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Mane.Unity
{
    public static class CoroutineExtensions
    {
        public static IEnumerator ToCoroutine<T>(this Task<T> task, Action<bool, T> callback)
        {
            while (!task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    callback?.Invoke(false, default);

                    yield break;
                }

                yield return null;
            }

            callback?.Invoke(true, task.Result);
        }

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

        public static bool TryKillCoroutine(this MonoBehaviour target, ref Coroutine coroutine)
        {
            if (coroutine == null || !target) return false;

            target.StopCoroutine(coroutine);
            coroutine = null;
            
            return true;
        }
    }
}