using System;
using System.Reflection;
using UnityEditor;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// While enabled, every inspector drawn by this package's fallback inspector uses the ManeStyle layout,
    /// as if its type had <see cref="ManeStyleAttribute"/>. Saved per project in the user settings.
    /// </summary>
    internal static class ManeStyleForAll
    {
        private const string ConfigKey = "ManeTools.ManeStyleForAll";

        internal static event Action<bool> EnabledChanged;

        internal static bool Enabled
        {
            get => EditorUserSettings.GetConfigValue(ConfigKey) == "1";
            set
            {
                if (Enabled == value)
                    return;

                EditorUserSettings.SetConfigValue(ConfigKey, value ? "1" : null);
                RebuildInspectors();
                EnabledChanged?.Invoke(value);
            }
        }

        // Inspectors build their UI once, so the open ones are rebuilt to pick the change up.
        // ForceRebuildInspectors covers locked inspectors too, but it is internal; the shared tracker is the fallback.
        private static void RebuildInspectors()
        {
            MethodInfo rebuildAll = typeof(EditorUtility).GetMethod("ForceRebuildInspectors",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (rebuildAll != null)
                rebuildAll.Invoke(null, null);
            else
                ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
    }
}
