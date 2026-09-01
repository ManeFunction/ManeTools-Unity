using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mane.Unity.Editor
{
    internal static class SceneManagementTools
    {
        private static Scene _lastClosed;

        private const string UnloadSceneMenuPath = "File/Unload scene _%w";

        [MenuItem(UnloadSceneMenuPath, false, 160)]
        private static void UnloadSelectedScene() => SaveSelectedSceneAndClose(false);

        [MenuItem(UnloadSceneMenuPath, true)]
        private static bool ValidateUnloadSelectedScene() => CanCloseSelectedScene();


        private const string RemoveSceneMenuPath = "File/Remove scene _%#w";

        [MenuItem(RemoveSceneMenuPath, false, 161)]
        private static void RemoveScene() => SaveSelectedSceneAndClose(true);

        [MenuItem(RemoveSceneMenuPath, true)]
        private static bool ValidateRemoveScene() => CanCloseSelectedScene();

        private static bool CanCloseSelectedScene()
        {
            if (!TryGetSelectedScene(out Scene scene))
                return false;

            if (!scene.IsValid() || !scene.isLoaded)
                return false;

            return GetLoadedSceneCount() > 1;
        }

        private static bool TryGetSelectedScene(out Scene scene)
        {
            foreach (EntityId id in Selection.entityIds)
            {
                if (TryGetSceneFromEntityId(id, out scene))
                    return true;
            }

            GameObject selection = Selection.activeGameObject;
            if (selection)
            {
                scene = selection.scene;
                return scene.IsValid();
            }

            scene = default;
            return false;
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

        private static int GetLoadedSceneCount()
        {
            int count = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).isLoaded)
                    count++;
            }

            return count;
        }

        private static void SaveSelectedSceneAndClose(bool unload)
        {
            if (!TryGetSelectedScene(out Scene scene))
                return;

            if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { scene }))
            {
                _lastClosed = scene;
                EditorSceneManager.CloseScene(scene, unload);
            }
        }


        private const string ReopenSceneMenuPath = "File/Reopen scene _%t";

        [MenuItem(ReopenSceneMenuPath, false, 162)]
        private static void LoadLastUnloadedScene() => EditorSceneManager.OpenScene(_lastClosed.path, OpenSceneMode.Additive);

        [MenuItem(ReopenSceneMenuPath, true)]
        private static bool LoadLastUnloadedSceneCheck() => _lastClosed.IsValid();
    }
}