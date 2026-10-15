using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// UI Toolkit helpers for inspectors: USS/UXML assets next to a script, visibility and binding.
    /// </summary>
    public static class UIElementsTools
    {
        private static readonly Dictionary<(Type, string), StyleSheet> Sheets = new();
        private static readonly HashSet<(Type, string)> MissingSheets = new();

        /// <summary>
        /// Adds the stylesheet next to <paramref name="scriptType"/> (<c>TypeName.uss</c> by default) to
        /// <paramref name="owner"/>, unless it is already there. The sheet is loaded once and shared; a missing
        /// file is logged once.
        /// </summary>
        public static void AddStyleSheet(VisualElement owner, Type scriptType, string fileName = null)
        {
            if (owner == null || scriptType == null)
                return;

            StyleSheet sheet = GetStyleSheet(scriptType, fileName);
            if (sheet != null && !owner.styleSheets.Contains(sheet))
                owner.styleSheets.Add(sheet);
        }

        /// <summary>
        /// Loads a stylesheet next to <paramref name="scriptType"/>. Defaults to <c>TypeName.uss</c>.
        /// </summary>
        public static StyleSheet LoadUSS(Type scriptType, string fileName = null) =>
            LoadNextToScript<StyleSheet>(scriptType, fileName, ".uss");

        /// <summary>
        /// Loads a visual tree next to <paramref name="scriptType"/>. Defaults to <c>TypeName.uxml</c>.
        /// </summary>
        public static VisualTreeAsset LoadUXML(Type scriptType, string fileName = null) =>
            LoadNextToScript<VisualTreeAsset>(scriptType, fileName, ".uxml");

        /// <summary>
        /// Shows or hides <paramref name="element"/> through its display style, so a hidden element takes no space.
        /// </summary>
        public static void SetVisible(this VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        /// <summary>
        /// Binds <paramref name="field"/> to the property at <paramref name="path"/>.
        /// Logs an error and returns false when the property does not exist.
        /// </summary>
        public static bool BindField(this IBindable field, SerializedObject serializedObject, string path)
        {
            SerializedProperty property = serializedObject?.FindProperty(path);
            if (property == null)
            {
                Debug.LogError($"'{path}' was not found on {serializedObject?.targetObject?.GetType().Name ?? "null"}.");
                return false;
            }

            field.BindProperty(property);
            return true;
        }

        // A missing sheet is looked up again next time: it may still be importing.
        private static StyleSheet GetStyleSheet(Type scriptType, string fileName)
        {
            (Type, string) key = (scriptType, fileName);
            if (Sheets.TryGetValue(key, out StyleSheet sheet) && sheet != null)
                return sheet;

            sheet = LoadUSS(scriptType, fileName);
            if (sheet != null)
            {
                Sheets[key] = sheet;
                return sheet;
            }

            if (MissingSheets.Add(key))
                Debug.LogError($"{fileName ?? scriptType.Name + ".uss"} was not found next to {scriptType.Name}.");

            return null;
        }

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
