using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for <see cref="RectTransform"/> offsets and world bounds.
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// Sets the left offset (<see cref="RectTransform.offsetMin"/>.x).
        /// </summary>
        public static void SetLeftOffset(this RectTransform rt, float left) => 
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);

        /// <summary>
        /// Sets the right offset (negated into <see cref="RectTransform.offsetMax"/>.x).
        /// </summary>
        public static void SetRightOffset(this RectTransform rt, float right) => 
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);

        /// <summary>
        /// Sets the top offset (negated into <see cref="RectTransform.offsetMax"/>.y).
        /// </summary>
        public static void SetTopOffset(this RectTransform rt, float top) => 
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);

        /// <summary>
        /// Sets the bottom offset (<see cref="RectTransform.offsetMin"/>.y).
        /// </summary>
        public static void SetBottomOffset(this RectTransform rt, float bottom) => 
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        
        
        /// <summary>
        /// Returns axis-aligned world bounds from the four world corners.
        /// </summary>
        public static Rect GetWorldCoordinates(this RectTransform uiElement)
        {
            if (uiElement == null)
                return Rect.zero;
            
            var worldCorners = new Vector3[4];
            uiElement.GetWorldCorners(worldCorners);
            
            return new Rect(
                worldCorners[0].x,
                worldCorners[0].y,
                worldCorners[2].x - worldCorners[0].x,
                worldCorners[2].y - worldCorners[0].y);
        }
    }
}
