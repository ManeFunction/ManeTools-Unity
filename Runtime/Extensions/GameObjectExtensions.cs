using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mane.Unity
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Get component or add it if no one was found.
        /// </summary>
        public static T GetRequiredComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                component = gameObject.AddComponent<T>();

            return component;
        }

        public static GameObject Duplicate(this GameObject source)
        {
            GameObject clone = Object.Instantiate(source, source.transform.parent);
            clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            return clone;
        }
        
        public static T Duplicate<T>(this T source) where T : Component
        {
            T clone = Object.Instantiate(source, source.transform.parent);
            clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);

            return clone;
        }
        
        public static void SetLayerRecursively(this GameObject go, int newLayer) =>
            go.DoRecursively(current => current.layer = newLayer);

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

        public static void SetActiveStateRecursively(this GameObject go, bool isActive) =>
            go.DoRecursively(current => current.SetActive(isActive));
        
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
        public static bool IsPrefab(this GameObject go) => go.scene.rootCount == 0;
    }
}