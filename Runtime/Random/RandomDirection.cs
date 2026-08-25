using System;
using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Uniform random directions on the unit circle and unit sphere.
    /// </summary>
    public static class RandomDirection
    {
        /// <summary>
        /// Returns a unit <see cref="Vector2"/> with uniform direction (random angle).
        /// </summary>
        public static Vector2 OnCircle(IRandom random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            float angle = random.Range01() * (Mathf.PI * 2f);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        /// <summary>
        /// Returns a unit <see cref="Vector3"/> with uniform direction on the sphere.
        /// </summary>
        public static Vector3 OnSphere(IRandom random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            float z = random.Range01() * 2f - 1f;
            float angle = random.Range01() * (Mathf.PI * 2f);
            float radius = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));
            return new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), z);
        }
    }
}
