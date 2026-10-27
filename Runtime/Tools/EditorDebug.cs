using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
        /// <param name="message">The message to log.</param>
        /// <param name="context">The context to log the message from.</param>
        [Conditional(UnityConst.UnityEditor), HideInCallstack]
        public static void Log(object message, Object context = null) =>
            Debug.Log(message, context);

        /// <summary>
        /// Logs a warning in the editor. Does nothing outside the Unity Editor.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">The context to log the message from.</param>
        [Conditional(UnityConst.UnityEditor), HideInCallstack]
        public static void LogWarning(object message, Object context = null) =>
            Debug.LogWarning(message, context);

        /// <summary>
        /// Logs an error in the editor. Does nothing outside the Unity Editor.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">The context to log the message from.</param>
        [Conditional(UnityConst.UnityEditor), HideInCallstack]
        public static void LogError(object message, Object context = null) =>
            Debug.LogError(message, context);
    }
}
