using UnityEngine;

namespace Mane.Unity
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// Get component or add it if no one was found.
        /// </summary>
        public static T GetRequiredComponent<T>(this Component component) where T : Component => 
            component.gameObject.GetRequiredComponent<T>();
    }
}