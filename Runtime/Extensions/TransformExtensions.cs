using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for local reset and pivot rotation.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Resets local position (Z kept as <paramref name="z"/>), scale, and rotation.
        /// </summary>
        public static void Reset(this Transform transform, float z = 0f)
        {
            transform.localPosition = new Vector3(0f, 0f, z);
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
