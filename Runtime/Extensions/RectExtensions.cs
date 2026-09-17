using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for scaling a <see cref="Rect"/> around a pivot.
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// Scales the rect about its center.
        /// </summary>
        public static Rect ScaleSizeBy(this Rect rect, float scale) => 
            rect.ScaleSizeBy(scale, rect.center);


        /// <summary>
        /// Scales the rect about <paramref name="pivotPoint"/>.
        /// </summary>
        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;

            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;

            result.x += pivotPoint.x;
            result.y += pivotPoint.y;

            return result;
        }
    }
}
