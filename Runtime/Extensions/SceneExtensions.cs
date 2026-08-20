using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mane.Unity
{
    public static class SceneExtensions
    {
        /// <summary>
        /// Gets the first component of type <typeparamref name="T"/> under this scene's root objects.
        /// Active objects are preferred; inactive ones are used only if none are active.
        /// </summary>
        public static T GetRootComponent<T>(this Scene scene) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            GameObject[] roots = scene.GetRootGameObjects();

            foreach (GameObject root in roots)
            {
                T component = root.GetComponentInChildren<T>(false);
                if (component != null)
                    return component;
            }

            foreach (GameObject root in roots)
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }
    }
}
