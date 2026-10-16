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
        private const string ProjectDeepMenuPath = "Assets/Mane Tools/Search for References (project only, deep)";
        private const string PackagesDeepMenuPath = "Assets/Mane Tools/Search for References (+ packages, deep)";
        private const int ProgressStep = 100;
        private const int BinaryCheckLength = 8000;

        [MenuItem(ProjectMenuPath, true)]
        [MenuItem(PackagesMenuPath, true)]
        [MenuItem(ProjectDeepMenuPath, true)]
        [MenuItem(PackagesDeepMenuPath, true)]
        private static bool Validate() => GetTargets().Count > 0;

        [MenuItem(ProjectMenuPath, false, 48)]
        private static void SearchProjectOnly() => Search(false, false);

        [MenuItem(PackagesMenuPath, false, 49)]
        private static void SearchWithPackages() => Search(true, false);

        [MenuItem(ProjectDeepMenuPath, false, 50)]
        private static void SearchProjectOnlyDeep() => Search(false, true);

        [MenuItem(PackagesDeepMenuPath, false, 51)]
        private static void SearchWithPackagesDeep() => Search(true, true);

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

        // Unity's dependency graph knows the object references of every asset type (scenes, prefabs, materials,
        // ScriptableObjects, animator controllers, importer settings, ...), binary-serialized ones included.
        // The deep search also looks for the GUID in the text of every text file, which finds GUIDs stored
        // as plain strings (Addressables' AssetReference, for example). Text alone would miss binary assets
        // and importer settings, so it adds to the dependency search instead of replacing it.
        private static void Search(bool includePackages, bool deep)
        {
            List<Object> targets = GetTargets();
            if (targets.Count == 0)
            {
                Debug.LogError("No asset selected.");
                return;
            }

            // A sub-asset (a sprite in a texture, for example) is referenced through its file's path and GUID.
            Dictionary<string, List<Object>> targetsByPath = new();
            Dictionary<Object, Found> foundByTarget = new();
            foreach (Object target in targets)
            {
                string targetPath = AssetDatabase.GetAssetPath(target);
                if (!targetsByPath.TryGetValue(targetPath, out List<Object> sameFile))
                    targetsByPath[targetPath] = sameFile = new List<Object>();

                sameFile.Add(target);
                foundByTarget[target] = new Found();
            }

            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            try
            {
                for (int i = 0; i < allPaths.Length; i++)
                {
                    string path = allPaths[i];
                    if (!includePackages && path.StartsWith("Packages/"))
                        continue;
                    if (AssetDatabase.IsValidFolder(path))
                        continue;

                    if (i % ProgressStep == 0 && EditorUtility.DisplayCancelableProgressBar(
                            deep ? "Finding References (deep)" : "Finding References",
                            $"Scanning {path}", (float)i / allPaths.Length))
                        return;

                    foreach (string dependency in AssetDatabase.GetDependencies(path, false))
                    {
                        if (dependency == path || !targetsByPath.TryGetValue(dependency, out List<Object> referenced))
                            continue;

                        foreach (Object target in referenced)
                            foundByTarget[target].Add(path, false);
                    }

                    if (deep)
                        SearchText(path, targetsByPath, foundByTarget);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            foreach (Object target in targets)
            {
                Found found = foundByTarget[target];
                if (found.Paths.Count == 0)
                    Debug.Log($"No references found for '{target.name}'.", target);

                foreach (string path in found.Paths)
                {
                    string asText = found.TextOnly.Contains(path) ? " (as a GUID in text)" : string.Empty;
                    Debug.Log($"'{target.name}' is referenced in: {path}{asText}",
                        AssetDatabase.LoadAssetAtPath<Object>(path));
                }
            }
        }

        private static void SearchText(string path, Dictionary<string, List<Object>> targetsByPath,
            Dictionary<Object, Found> foundByTarget)
        {
            string text = ReadText(path);
            if (text == null)
                return;

            foreach (KeyValuePair<string, List<Object>> pair in targetsByPath)
            {
                if (pair.Key == path || !text.Contains(AssetDatabase.AssetPathToGUID(pair.Key)))
                    continue;

                foreach (Object target in pair.Value)
                    foundByTarget[target].Add(path, true);
            }
        }

        // Null for a binary file: a zero byte near the start marks it, as git does.
        private static string ReadText(string path)
        {
            string fullPath = FileUtil.GetPhysicalPath(path);
            if (!File.Exists(fullPath))
                return null;

            using (FileStream stream = File.OpenRead(fullPath))
            {
                byte[] head = new byte[BinaryCheckLength];
                int read = stream.Read(head, 0, head.Length);
                for (int i = 0; i < read; i++)
                {
                    if (head[i] == 0)
                        return null;
                }
            }

            return File.ReadAllText(fullPath);
        }

        // The referencing paths in scan order. TextOnly holds those that only the text search found.
        private sealed class Found
        {
            public readonly List<string> Paths = new();
            public readonly HashSet<string> TextOnly = new();
            private readonly HashSet<string> _known = new();

            public void Add(string path, bool fromText)
            {
                if (_known.Add(path))
                {
                    Paths.Add(path);
                    if (fromText)
                        TextOnly.Add(path);
                }
                else if (!fromText)
                {
                    TextOnly.Remove(path);
                }
            }
        }
    }
}
