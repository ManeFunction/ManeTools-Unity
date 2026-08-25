using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Queues work from other threads and runs it on Unity's main thread.
    /// Access <see cref="UnitySingleton{T}.Instance"/> once from the main thread
    /// (or place the component in a scene) before calling <see cref="RunOnMainThread"/>.
    /// </summary>
    public sealed class MainThreadDispatcher : UnitySingleton<MainThreadDispatcher>
    {
        private readonly Queue<Action> _actions = new();

        /// <summary>
        /// Enqueues <paramref name="action"/> to run on the next Update.
        /// Safe to call from a background thread. Does not create the dispatcher.
        /// </summary>
        public static void RunOnMainThread(Action action)
        {
            if (!IsReady())
            {
                Debug.LogError("MainThreadDispatcher is not present in the scene.");
                return;
            }

            Queue<Action> actions = Instance._actions;
            lock (actions)
                actions.Enqueue(action);
        }

        private void Update()
        {
            while (_actions.Count > 0)
            {
                Action action;
                lock (_actions)
                    action = _actions.Dequeue();

                action();
            }
        }
    }
}
