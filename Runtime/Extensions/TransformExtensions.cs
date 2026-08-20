using UnityEngine;

namespace Mane.Unity
{
    public static class TransformExtensions
    {
        public static void Reset(this Transform transform, float z = 0f)
        {
            transform.localPosition = new Vector3(0f, 0f, z);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }
        
        public static void RotateAround(this Transform transform, Vector3 pivot, Quaternion rotation)
        {
            transform.position = rotation * (transform.position - pivot) + pivot;
            transform.rotation = rotation * transform.rotation;
        }
    }
}