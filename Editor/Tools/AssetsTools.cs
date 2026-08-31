using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    public static class AssetsTools
    {
        [MenuItem("Assets/Force Reserialise Asset(s)", true)]
        private static bool ValidateSaveAssets() => Selection.objects.Length > 0;

        [MenuItem("Assets/Force Reserialise Asset(s)", false, 45)]
        private static void SaveAssets()
        {
            AssetDatabase.ForceReserializeAssets(Selection.objects
                .Select(AssetDatabase.GetAssetPath));

            foreach (Object obj in Selection.objects)
                if (obj) EditorUtility.SetDirty(obj);
        }
        
        [MenuItem("Assets/Force Reserialize All Assets", false, 46)]
        private static void ForceSaveAssets()
        {
            if (EditorUtility.DisplayDialog(
                "Force Assets Reserialization",
                "This may be long operation despite of the size of your project, so be aware! There is no progress bar so it may looks like that Unity is frozen, but be patient, it's working. Proceed operation?",
                "Yes, go ahead!", "Cancel!"))
            {
                AssetDatabase.ForceReserializeAssets();
                AssetDatabase.Refresh();
            }
        }

        private const string CheckMissedComponentsMenuPath = "Assets/Mane Tools/Check Missed Components";

        [MenuItem(CheckMissedComponentsMenuPath, true)]
        private static bool ValidateCheckMissedComponents() =>
            Selection.GetFiltered<GameObject>(SelectionMode.Assets).Length > 0
            || Selection.GetFiltered<SceneAsset>(SelectionMode.Assets).Length > 0;

        [MenuItem(CheckMissedComponentsMenuPath, false, 47)]
        private static void CheckMissedComponents()
        {
            GameObject[] gameObjects = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            if (gameObjects.Length > 0)
                MissingReferencesFinder.CheckGameObjects(gameObjects);

            SceneAsset[] sceneAssets = Selection.GetFiltered<SceneAsset>(SelectionMode.Assets);
            if (sceneAssets.Length == 0)
                return;

            List<Scene> loadedScenes = new();
            List<string> scenesToOpen = new();
            foreach (var sceneAsset in sceneAssets)
            {
                string path = AssetDatabase.GetAssetPath(sceneAsset);
                Scene scene = SceneManager.GetSceneByPath(path);
                if (scene.IsValid() && scene.isLoaded)
                    loadedScenes.Add(scene);
                else
                    scenesToOpen.Add(path);
            }

            if (scenesToOpen.Count > 0
                && !EditorUtility.DisplayDialog(
                    "Check Missed Components",
                    "Selected scene(s) will be opened to perform a scan. Continue?",
                    "Yes", "No"))
                scenesToOpen.Clear();

            if (loadedScenes.Count > 0)
                MissingReferencesFinder.CheckScenes(loadedScenes.ToArray());

            foreach (var sceneToOpen in scenesToOpen)
            {
                Scene scene = EditorSceneManager.OpenScene(sceneToOpen, OpenSceneMode.Additive);
                bool hadErrors = MissingReferencesFinder.CheckScenes(scene);
                if (!hadErrors)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}