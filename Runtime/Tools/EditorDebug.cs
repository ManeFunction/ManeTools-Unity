using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Debug logs that compile away outside the Unity Editor.
    /// </summary>
    public static class EditorDebug
    {
        /// <summary>
        /// Logs <paramref name="message"/> in the editor. Does nothing outside the Unity Editor.
        /// </summary>
        public static void Log(object message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#endif
        }
        
        /// <summary>
        /// Logs a warning in the editor. Does nothing outside the Unity Editor.
        /// </summary>
        public static void LogWarning(object message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }
        
        /// <summary>
        /// Logs an error in the editor. Does nothing outside the Unity Editor.
        /// </summary>
        public static void LogError(object message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
        }
    }
}
