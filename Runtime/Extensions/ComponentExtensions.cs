using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for <see cref="Component"/> lookup.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Get component or add it if no one was found.
        /// </summary>
        public static T GetOrAddComponent<T>(this Component component) where T : Component => 
            component.gameObject.GetOrAddComponent<T>();
    }
}