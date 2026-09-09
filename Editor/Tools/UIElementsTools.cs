using System;
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    public static class UIElementsTools
    {
        public static StyleSheet LoadUSS(Type scriptType, string fileName = null) =>
            LoadNextToScript<StyleSheet>(scriptType, fileName, ".uss");

        public static VisualTreeAsset LoadUXML(Type scriptType, string fileName = null) =>
            LoadNextToScript<VisualTreeAsset>(scriptType, fileName, ".uxml");

        private static T LoadNextToScript<T>(Type scriptType, string fileName, string extension)
            where T : UnityObject
        {
            if (scriptType == null)
                return null;

            if (string.IsNullOrEmpty(fileName))
                fileName = scriptType.Name;

            if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                fileName += extension;

            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {scriptType.Name}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null || script.GetClass() != scriptType)
                    continue;

                string folder = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(folder))
                    return null;

                string assetPath = Path.Combine(folder, fileName).Replace('\\', '/');
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }

            return null;
        }
    }
}
