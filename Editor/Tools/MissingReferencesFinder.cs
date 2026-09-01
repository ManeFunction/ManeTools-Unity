using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mane.Unity.Editor
{
    internal static class MissingReferencesFinder
    {
        private const string MenuPath = "GameObject/Mane Tools/Check Missed Components";

        [MenuItem(MenuPath, false, 32)]
        private static void CheckMissedComponents()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length > 0)
            {
                CheckGameObjects(selected);
                return;
            }

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.prefabContentsRoot != null)
            {
                CheckGameObjects(stage.prefabContentsRoot);
                return;
            }

            List<Scene> scenes = GetSelectedScenes();
            if (scenes.Count == 0)
            {
                Scene active = SceneManager.GetActiveScene();
                if (active.IsValid() && active.isLoaded)
                    scenes.Add(active);
            }

            CheckScenes(scenes.ToArray());
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateCheckMissedComponents()
        {
            if (Selection.gameObjects.Length > 0)
                return true;

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return true;

            if (GetSelectedScenes().Count > 0)
                return true;

            Scene active = SceneManager.GetActiveScene();
            return active.IsValid() && active.isLoaded;
        }

        internal static void CheckGameObjects(params GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
                return;

            ScanGameObjects(gameObjects);
            LogDone();
        }

        internal static bool CheckScenes(params Scene[] scenes)
        {
            if (scenes == null || scenes.Length == 0)
                return false;

            bool hadErrors = false;
            foreach (Scene scene in scenes)
                hadErrors |= ScanScene(scene);

            LogDone();
            return hadErrors;
        }

        private static List<Scene> GetSelectedScenes()
        {
            List<Scene> scenes = new();
            foreach (EntityId id in Selection.entityIds)
            {
                if (!TryGetSceneFromEntityId(id, out Scene scene))
                    continue;

                if (!scenes.Contains(scene))
                    scenes.Add(scene);
            }

            return scenes;
        }

        private static bool TryGetSceneFromEntityId(EntityId id, out Scene scene)
        {
            scene = default;
            if (EditorUtility.EntityIdToObject(id) != null)
                return false;

            SceneHandle handle = SceneHandle.FromRawData(EntityId.ToULong(id));
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene candidate = SceneManager.GetSceneAt(i);
                if (candidate.handle != handle)
                    continue;

                scene = candidate;
                return scene.IsValid();
            }

            return false;
        }

        private static bool ScanScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"Cannot scan scene '{scene.name}' because it is not loaded.");
                return false;
            }

            bool hadErrors = false;
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject rootObject in roots)
                hadErrors |= Scan(rootObject);

            return hadErrors;
        }

        private static void ScanGameObjects(GameObject[] selected)
        {
            foreach (var go in selected)
            {
                if (!go || IsChildOfSelected(go, selected))
                    continue;

                Scan(go);
            }
        }

        private static bool IsChildOfSelected(GameObject go, GameObject[] selected)
        {
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                if (selected.Any(t => t == parent.gameObject))
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        private static bool Scan(GameObject obj)
        {
            if (!obj)
                return false;

            bool hadErrors = false;

            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.MissingAsset)
            {
                Debug.LogError($"Missing prefab instance: {FullPath(obj)}", obj);
                hadErrors = true;
            }

            MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                if (!component)
                {
                    Debug.LogError($"Missing Component in GO: {FullPath(obj)}", obj);
                    hadErrors = true;
                    continue;
                }

                SerializedObject serializedObject = new(component);
                SerializedProperty serializedProperty = serializedObject.GetIterator();
                while (serializedProperty.NextVisible(true))
                {
                    if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    if (serializedProperty.objectReferenceValue == null
                        && serializedProperty.objectReferenceEntityIdValue != EntityId.None)
                    {
                        Debug.LogError(
                            $"Missing Ref in: {FullPath(obj)}. Component: {component.GetType().Name}, Property: {ObjectNames.NicifyVariableName(serializedProperty.name)}",
                            obj);
                        hadErrors = true;
                    }
                }
            }

            int childCount = obj.transform.childCount;
            for (int i = 0; i < childCount; i++)
                hadErrors |= Scan(obj.transform.GetChild(i).gameObject);

            return hadErrors;
        }

        private static string FullPath(GameObject go)
        {
            string path = go.name;
            Transform parent = go.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            if (go.scene.IsValid() && !string.IsNullOrEmpty(go.scene.name))
                path = go.scene.name + "/" + path;

            return path;
        }

        private static void LogDone() => Debug.Log("Scan completed!");
    }
}
