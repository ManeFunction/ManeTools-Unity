using UnityEngine;

namespace Mane.Unity
{
    public static class UnityObjectExtensions
    {
        public static T Instantiate<T>(this string path, Transform parent = null) where T : Object => 
            Object.Instantiate(Resources.Load<T>(path), parent);

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