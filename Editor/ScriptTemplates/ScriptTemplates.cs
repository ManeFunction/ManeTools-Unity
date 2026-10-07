using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Mane.Unity.Editor
{
    [InitializeOnLoad]
    internal static class ScriptTemplates
    {
        private const string MonoBehaviourMenuPath = "Assets/Create/Scripting/MonoBehaviour Script";

        // Built-in scripting templates use priorities 1-3 (MonoBehaviour, ScriptableObject, Empty C#).
        // Stay in that group, ahead of Assembly Definition at 20.
        private const int MonoBehaviourPriority = 1;

        private const BindingFlags MenuFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        static ScriptTemplates() => EditorApplication.delayCall += ReplaceMonoBehaviourMenu;

        private static void CreateMonoBehaviour() =>
            CreateScript("MonoBehaviour.cs.txt", "NewMonoBehaviourScript.cs");

        private static void ReplaceMonoBehaviourMenu()
        {
            Type menuType = typeof(Menu);
            MethodInfo remove = menuType.GetMethod("RemoveMenuItem", MenuFlags, null, new[] { typeof(string) }, null);
            MethodInfo add = menuType.GetMethod(
                "AddMenuItem",
                MenuFlags,
                null,
                new[] { typeof(string), typeof(string), typeof(bool), typeof(int), typeof(Action), typeof(Func<bool>) },
                null);

            if (remove == null || add == null)
            {
                Debug.LogError("Could not replace the MonoBehaviour script menu. Unity's menu API has changed. Please update the Mane Tools package and open a new GitHub issue if the problem persists.");
                return;
            }

            // Unity builds the built-in item once per Editor session. A domain reload drops our
            // previous item and does not bring the built-in one back, so add ours either way.
            remove.Invoke(null, new object[] { MonoBehaviourMenuPath });
            add.Invoke(null, new object[]
            {
                MonoBehaviourMenuPath,
                string.Empty,
                false,
                MonoBehaviourPriority,
                (Action)CreateMonoBehaviour,
                null
            });
        }

        private static void CreateScript(string templateFileName, string defaultFileName)
        {
            string templatePath = TemplatePath(templateFileName);
            if (templatePath == null)
                return;

            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultFileName);
        }

        private static string TemplatePath(string templateFileName)
        {
            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(ScriptTemplates).Assembly);
            if (packageInfo == null)
            {
                Debug.LogError("Could not resolve the Mane Tools package path for script templates.");
                return null;
            }

            return Path.Combine(packageInfo.resolvedPath, "Editor", "ScriptTemplates", templateFileName);
        }
    }
}
