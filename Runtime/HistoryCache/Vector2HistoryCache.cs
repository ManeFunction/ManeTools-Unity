using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Ring buffer of recent <see cref="Vector2"/> values.
    /// </summary>
    public class Vector2HistoryCache : HistoryCache<Vector2>
    {
        /// <summary>
        /// Creates a cache that stores up to <paramref name="length"/> values.
        /// </summary>
        /// <param name="length">Buffer size. Must be greater than 0.</param>
        public Vector2HistoryCache(int length) : base(length) { }

        /// <inheritdoc />
        public override Vector2 GetAverage()
        {
            if (Count == 0)
                return Vector2.zero;

            Vector2 sum = Vector2.zero;
            for (int i = 0; i < Count; i++)
                sum += History[i];

            return sum / Count;
        }
    }
}
