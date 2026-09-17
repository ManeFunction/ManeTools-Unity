using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for <see cref="GameObject"/> duplication, layers, and hierarchy walks.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Get component or add it if no one was found.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();

            return component;
        }

        /// <summary>
        /// Instantiates a sibling copy immediately after <paramref name="source"/>.
        /// </summary>
        public static GameObject Duplicate(this GameObject source)
        {
            GameObject clone = Object.Instantiate(source, source.transform.parent);
            clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            return clone;
        }
        
        /// <summary>
        /// Instantiates a sibling copy of the component's GameObject immediately after it.
        /// </summary>
        public static T Duplicate<T>(this T source) where T : Component
        {
            T clone = Object.Instantiate(source, source.transform.parent);
            clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            return clone;
        }
        
        /// <summary>
        /// Sets <see cref="GameObject.layer"/> on this object and all children.
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int newLayer) =>
            go.DoRecursively(current => current.layer = newLayer);

        /// <summary>
        /// Sets sorting layer (and optional order) on renderers and canvases in the hierarchy.
        /// </summary>
        public static void SetSortingLayerRecursively(this GameObject go, int newLayer, int? newOrder = null)
        {
            go.DoRecursively(current =>
            {
                Renderer renderer = current.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sortingLayerID = newLayer;
                    if (newOrder.HasValue)
                        renderer.sortingOrder = newOrder.Value;
                }

                Canvas canvas = current.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingLayerID = newLayer;
                    if (newOrder.HasValue)
                        canvas.sortingOrder = newOrder.Value;
                }
            });
        }

        /// <summary>
        /// Calls <see cref="GameObject.SetActive"/> on this object and all children.
        /// </summary>
        public static void SetActiveStateRecursively(this GameObject go, bool isActive) =>
            go.DoRecursively(current => current.SetActive(isActive));
        
        /// <summary>
        /// Invokes <paramref name="action"/> on this object and every descendant.
        /// </summary>
        public static void DoRecursively(this GameObject go, Action<GameObject> action)
        {
            if (go == null || action == null) return;

            action(go);
            foreach (Transform child in go.transform)
            {
                if (child != null)
                    DoRecursively(child.gameObject, action);
            }
        }

        // There is no "legit" way to know is GameObject prefab
        // or not besides PrefabUtility, but it's not available
        // in a runtime, so this is the most obvious workaround.
        /// <summary>
        /// True when the object is not in a loaded scene (typical of prefab assets).
        /// </summary>
        public static bool IsPrefab(this GameObject go) => go.scene.rootCount == 0;
    }
}