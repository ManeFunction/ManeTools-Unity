using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Random points in the unit square/cube and on line segments.
    /// </summary>
    public static class RandomPoint
    {
        /// <summary>
        /// Returns a point uniformly in the half-open unit square [0, 1)².
        /// </summary>
        public static Vector2 Point2D(IRandom random) =>
            new(random.Range01(), random.Range01());

        /// <summary>
        /// Returns a point uniformly in the half-open unit cube [0, 1)³.
        /// </summary>
        public static Vector3 Point3D(IRandom random) =>
            new(random.Range01(), random.Range01(), random.Range01());

        /// <summary>
        /// Returns a point uniformly on the segment from <paramref name="start"/> toward
        /// <paramref name="end"/> (t in [0, 1), so <paramref name="end"/> itself is excluded).
        /// </summary>
        public static Vector2 OnVector2D(Vector2 start, Vector2 end, IRandom random) =>
            start + (end - start) * random.Range01();

        /// <summary>
        /// Returns a point uniformly on the segment from <paramref name="start"/> toward
        /// <paramref name="end"/> (t in [0, 1), so <paramref name="end"/> itself is excluded).
        /// </summary>
        public static Vector3 OnVector3D(Vector3 start, Vector3 end, IRandom random) =>
            start + (end - start) * random.Range01();
    }
}