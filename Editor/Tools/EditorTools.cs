using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mane.Unity.Editor
{
    internal static class EditorTools
    {
        [MenuItem("Edit/Clear Console _F8", false, 900)]
        private static void ClearConsole()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            if (method != null)
                method.Invoke(new object(), null);
        }

        [MenuItem("Edit/Enable \u2044 Disable selected GO _F6", false, 903)]
        private static void ChangeSelectedObjectState()
        {
            bool state = !Selection.activeGameObject.activeSelf;
            foreach (GameObject go in Selection.gameObjects)
                go.SetActive(state);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
        }

        [MenuItem("Edit/Enable \u2044 Disable selected GO _F6", true)]
        private static bool ChangeSelectedObjectStateCheck() => Selection.activeGameObject;
    }
}