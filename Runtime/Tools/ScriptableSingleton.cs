using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// ScriptableObject singleton. The first access to <see cref="Instance"/> uses a loaded
    /// asset if one exists, otherwise creates a hidden runtime instance, unless
    /// <see cref="SetInstance"/> was called first.
    /// Assign an asset on a serialized field and pass it to <see cref="GetOrCreate"/>
    /// to clone that editor default for play mode instead of mutating the asset.
    /// </summary>
    /// <typeparam name="T">Concrete singleton type.</typeparam>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
    {
        private static T _instance;

        /// <summary>
        /// The current singleton instance. Finds a loaded asset or creates one if none has been set.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                T existing = FindLoaded();
                if (existing != null)
                {
                    SetInstance(existing);
                    return _instance;
                }

                CreateRuntimeInstance();
                return _instance;
            }
        }

        /// <summary>
        /// Returns true if an instance already exists, without creating one.
        /// Destroyed Unity objects count as not ready.
        /// </summary>
        public static bool IsReady() => _instance != null;

        /// <summary>
        /// Replaces the current instance. Use this when the asset is assigned manually.
        /// </summary>
        /// <param name="instance">The instance to store. Must not be null.</param>
        public static void SetInstance(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _instance = instance;
        }

        /// <summary>
        /// Clears the stored instance without destroying the object.
        /// </summary>
        public static void ClearInstance() => _instance = null;

        /// <summary>
        /// Registers this object as the singleton instance if none is set yet.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_instance == null)
                _instance = (T)this;
        }

        /// <summary>
        /// Clears the stored instance when this object is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private static T FindLoaded()
        {
            T[] loaded = Resources.FindObjectsOfTypeAll<T>();
            T runtime = null;
            for (int i = 0; i < loaded.Length; i++)
            {
                T candidate = loaded[i];
                if (candidate == null)
                    continue;

                if ((candidate.hideFlags & HideFlags.HideAndDontSave) != 0)
                {
                    runtime ??= candidate;
                    continue;
                }

                return candidate;
            }

            return runtime;
        }

        private static void CreateRuntimeInstance()
        {
            T instance = CreateInstance<T>();
            instance.name = $"[{typeof(T).Name}]";
            instance.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
