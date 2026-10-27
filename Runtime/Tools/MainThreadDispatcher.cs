using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Queues work from other threads and runs it on Unity's main thread.
    /// Access <see cref="UnitySingleton{T}.Instance"/> once from the main thread
    /// (or place the component in a scene) before calling <see cref="RunOnMainThread"/>.
    /// </summary>
    [AddComponentMenu("Mane Tools/Singletons/Main Thread Dispatcher")]
    public sealed class MainThreadDispatcher : UnitySingleton<MainThreadDispatcher>
    {
        // Static, so callers on other threads never go through Instance / IsReady():
        // UnityEngine.Object comparisons and lookups are main-thread only
        private static readonly ConcurrentQueue<Action> Actions = new();
        // Compared with `is null`, never with Unity's overloaded ==
        private static volatile MainThreadDispatcher _active;

        /// <summary>
        /// Enqueues <paramref name="action"/> to run on the next Update.
        /// Safe to call from a background thread. Does not create the dispatcher.
        /// </summary>
        public static void RunOnMainThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (_active is null)
            {
                Debug.LogError("MainThreadDispatcher is not present in the scene.");
                return;
            }

            Actions.Enqueue(action);
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                _active = this;
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            if (ReferenceEquals(_active, this))
                _active = null;

            base.OnDestroy();
        }

        private void Update()
        {
            // Only what was queued before this frame, so an action that queues another cannot stall the frame
            int count = Actions.Count;
            while (count-- > 0 && Actions.TryDequeue(out Action action))
                action();
        }

        // With domain reload disabled, statics survive between play sessions
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _active = null;
            while (Actions.TryDequeue(out _)) { }
        }
    }
}
