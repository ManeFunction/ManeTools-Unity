using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    internal static class AssetReferenceFinder
    {
        private const string ProjectMenuPath = "Assets/Mane Tools/Search for References (project only)";
        private const string PackagesMenuPath = "Assets/Mane Tools/Search for References (+ packages)";

        [MenuItem(ProjectMenuPath, true)]
        [MenuItem(PackagesMenuPath, true)]
        private static bool Validate() => GetTargets().Count > 0;

        [MenuItem(ProjectMenuPath, false, 48)]
        private static void SearchProjectOnly() => Search(false);

        [MenuItem(PackagesMenuPath, false, 49)]
        private static void SearchWithPackages() => Search(true);

        private static List<Object> GetTargets()
        {
            List<Object> targets = new();
            foreach (Object obj in Selection.objects)
            {
                if (obj && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)))
                    targets.Add(obj);
            }

            return targets;
        }

        private static void Search(bool includePackages)
        {
            List<Object> targets = GetTargets();
            if (targets.Count == 0)
            {
                Debug.LogError("No asset selected.");
                return;
            }

            List<string> allAssets = new();
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.EndsWith(".unity") && !path.EndsWith(".prefab"))
                    continue;

                if (!includePackages && path.StartsWith("Packages/"))
                    continue;

                allAssets.Add(path);
            }

            try
            {
                foreach (Object target in targets)
                {
                    if (FindReferences(target, allAssets))
                        break;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static bool FindReferences(Object target, List<string> allAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            bool found = false;

            for (int i = 0; i < allAssets.Count; i++)
            {
                string path = allAssets[i];
                if (path == assetPath)
                    continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                        "Finding References",
                        $"Scanning {path}",
                        (float)i / allAssets.Count))
                    return true;

                if (!File.Exists(path) || !File.ReadAllText(path).Contains(assetGuid))
                    continue;

                found = true;
                Debug.Log($"Reference found in: {path}", AssetDatabase.LoadAssetAtPath<Object>(path));
            }

            if (!found)
                Debug.Log($"No references found for '{target.name}'.", target);

            return false;
        }
    }
}
