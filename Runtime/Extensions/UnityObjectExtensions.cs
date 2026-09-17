using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for instantiating from Resources and destroying in edit or play mode.
    /// </summary>
    public static class UnityObjectExtensions
    {
        /// <summary>
        /// Loads a Resources asset at <paramref name="path"/> and instantiates it.
        /// </summary>
        public static T Instantiate<T>(this string path, Transform parent = null) where T : Object => 
            Object.Instantiate(Resources.Load<T>(path), parent);

        /// <summary>
        /// Destroys <paramref name="o"/> immediately in edit mode, otherwise with <see cref="Object.Destroy"/>.
        /// </summary>
        public static void SafeDestroy(this Object o)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o);
#else
            Object.Destroy(o);
#endif
        }
    }
}
