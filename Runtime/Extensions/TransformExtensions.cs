using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for local reset and pivot rotation.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Resets local position, scale, and rotation.
        /// </summary>
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
        
        /// <summary>
        /// Rotates this transform around a world-space <paramref name="pivot"/>.
        /// </summary>
        public static void RotateAround(this Transform transform, Vector3 pivot, Quaternion rotation)
        {
            transform.position = rotation * (transform.position - pivot) + pivot;
            transform.rotation = rotation * transform.rotation;
        }
    }
}
