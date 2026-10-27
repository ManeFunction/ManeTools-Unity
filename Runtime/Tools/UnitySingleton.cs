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
        /// Returns null instead of creating one in edit mode or while the application is quitting
        /// (this includes leaving play mode), so check <see cref="IsReady"/> in <c>OnDisable</c> / <c>OnDestroy</c>.
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

                // Creating during teardown leaks an object; creating in edit mode saves it into the scene
                if (!Application.isPlaying || SingletonLifetime.IsQuitting)
                    return null;

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
        /// Registers this object as the singleton instance. If another instance is already registered
        /// (for example, the scene that holds this object was loaded again), destroys this GameObject instead.
        /// Overrides should call <c>base.Awake()</c> first and return when <c>Instance != this</c>.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

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

    // Not generic: RuntimeInitializeOnLoadMethod is ignored in generic classes
    internal static class SingletonLifetime
    {
        public static bool IsQuitting { get; private set; }

        // SubsystemRegistration also resets the flag when domain reload is disabled
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            IsQuitting = false;
            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;
        }

        private static void OnQuitting() => IsQuitting = true;
    }
}
