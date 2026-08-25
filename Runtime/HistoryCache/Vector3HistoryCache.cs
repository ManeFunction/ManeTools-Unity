using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Ring buffer of recent <see cref="Vector3"/> values.
    /// </summary>
    public class Vector3HistoryCache : HistoryCache<Vector3>
    {
        /// <summary>
        /// Creates a cache that stores up to <paramref name="length"/> values.
        /// </summary>
        /// <param name="length">Buffer size. Must be greater than 0.</param>
        public Vector3HistoryCache(int length) : base(length) { }

        /// <inheritdoc />
        public override Vector3 GetAverage()
        {
            if (Count == 0)
                return Vector3.zero;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < Count; i++)
                sum += History[i];

            return sum / Count;
        }
    }
}
