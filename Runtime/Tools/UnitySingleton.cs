using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// MonoBehaviour singleton. The first access to <see cref="Instance"/> uses a scene
    /// component if one exists, otherwise creates a DontDestroyOnLoad object, unless
    /// <see cref="SetInstance"/> was called first.
    /// </summary>
    /// <typeparam name="T">Concrete singleton type.</typeparam>
    public abstract class UnitySingleton<T> : MonoBehaviour where T : UnitySingleton<T>
    {
        private static T _instance;

        /// <summary>
        /// The current singleton instance. Finds or creates one if none has been set.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                T existing = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                if (existing != null)
                {
                    SetInstance(existing);
                    return _instance;
                }

                CreateInstance();
                return _instance;
            }
        }

        /// <summary>
        /// Returns true if an instance already exists, without creating one.
        /// Destroyed Unity objects count as not ready.
        /// </summary>
        public static bool IsReady() => _instance != null;

        /// <summary>
        /// Replaces the current instance. Use this when the component is created manually.
        /// </summary>
        /// <param name="instance">The instance to store. Must not be null.</param>
        public static void SetInstance(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            _instance = instance;
            Persist(instance.gameObject);
        }

        /// <summary>
        /// Clears the stored instance without destroying the GameObject.
        /// </summary>
        public static void ClearInstance() => _instance = null;

        /// <summary>
        /// Registers this object as the singleton instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when another instance is already registered.
        /// </exception>
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
                throw new InvalidOperationException(
                    $"{typeof(T).Name} singleton is already initialized.");

            _instance = (T)this;
            Persist(gameObject);
        }

        /// <summary>
        /// Clears the stored instance when this object is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private static void CreateInstance()
        {
            GameObject gameObject = new($"[{typeof(T).Name}]");
            gameObject.AddComponent<T>();
        }

        private static void Persist(GameObject gameObject)
        {
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
    }
}
