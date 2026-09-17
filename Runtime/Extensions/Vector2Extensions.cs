using System;
using System.Collections.Generic;
using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for translating, clamping, and testing <see cref="Vector2"/> values.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Adds <paramref name="dX"/> and <paramref name="dY"/> to the components.
        /// </summary>
        public static Vector2 Translate(this Vector2 v, float dX, float dY)
        {
            v.x += dX;
            v.y += dY;
            
            return v;
        }

        /// <summary>
        /// Adds <paramref name="d"/> to the vector.
        /// </summary>
        public static Vector2 Translate(this Vector2 v, Vector2 d)
        {
            v.x += d.x;
            v.y += d.y;
            
            return v;
        }

        
        /// <summary>
        /// Adds <paramref name="dX"/> to X.
        /// </summary>
        public static Vector2 TranslateX(this Vector2 v, float dX)
        {
            v.x += dX;

            return v;
        }

        /// <summary>
        /// Adds <paramref name="dY"/> to Y.
        /// </summary>
        public static Vector2 TranslateY(this Vector2 v, float dY)
        {
            v.y += dY;

            return v;
        }

        
        /// <summary>
        /// Returns a <see cref="Vector3"/> with XY translated and Z set to <paramref name="dZ"/>.
        /// </summary>
        public static Vector3 Translate(this Vector2 v, float dX, float dY, float dZ) => 
            new(v.x + dX, v.y + dY, dZ);


        /// <summary>
        /// Returns a copy with X set to <paramref name="x"/>.
        /// </summary>
        public static Vector2 SetX(this Vector2 v, float x)
        {
            v.x = x;
            
            return v;
        }

        /// <summary>
        /// Returns a copy with Y set to <paramref name="y"/>.
        /// </summary>
        public static Vector2 SetY(this Vector2 v, float y)
        {
            v.y = y;
            
            return v;
        }


        /// <summary>
        /// Negates X.
        /// </summary>
        public static Vector2 FlipX(this Vector2 v)
        {
            v.x *= -1;
            
            return v;
        }

        /// <summary>
        /// Negates Y.
        /// </summary>
        public static Vector2 FlipY(this Vector2 v)
        {
            v.y *= -1;
            
            return v;
        }


        /// <summary>
        /// Returns a <see cref="Vector3"/> with this XY and the given Z.
        /// </summary>
        public static Vector3 AddZ(this Vector2 v, float z = 0f) => new(v.x, v.y, z);


        /// <summary>
        /// Clamps each component between <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        public static Vector2 Clamp(this Vector2 v, float a, float b)
        {
            v.x = v.x.Clamp(a, b);
            v.y = v.y.Clamp(a, b);

            return v;
        }
        
        /// <summary>
        /// Projects this vector onto <paramref name="onNormal"/>.
        /// </summary>
        public static Vector2 Project(this Vector2 v, Vector2 onNormal)
        {
            float num1 = Vector2.Dot(onNormal, onNormal);
            if (num1 < Mathf.Epsilon)
                return Vector2.zero;
            
            float num2 = Vector2.Dot(v, onNormal);
            
            return new Vector2(onNormal.x * num2 / num1, onNormal.y * num2 / num1);
        }

        /// <summary>
        /// Returns X times Y.
        /// </summary>
        public static float Area(this Vector2 size) => size.x * size.y;
        
        /// <summary>
        /// True if the point lies inside <paramref name="rect"/>.
        /// </summary>
        public static bool IsInsideRectangle(this Vector2 p, Rect rect) =>
            p.IsInsideRectangle(rect.min, new Vector2(rect.xMin, rect.yMax),
                                rect.max, new Vector2(rect.xMax, rect.yMin));
        
        /// <summary>
        /// True if the point lies inside the 4-corner rectangle.
        /// </summary>
        public static bool IsInsideRectangle(this Vector2 p, params Vector2[] rect)
        {
            if (rect.Length != 4)
                throw new ArgumentOutOfRangeException(nameof(rect), "The rect argument must be a 4 point array!");

            return IsInsideRectangle(p, rect[0], rect[1], rect[2], rect[3]);
        }
    
        /// <summary>
        /// True if the point lies inside the rectangle defined by four corners.
        /// </summary>
        public static bool IsInsideRectangle(this Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float dot1 = Vector2.Dot(p - p1, p2 - p1);
            float dot2 = Vector2.Dot(p - p1, p4 - p1);
            float dot3 = Vector2.Dot(p - p3, p4 - p3);
            float dot4 = Vector2.Dot(p - p3, p2 - p3);

            return dot1 >= 0 && dot1 <= (p2 -p1).sqrMagnitude &&
                   dot2 >= 0 && dot2 <= (p4 -p1).sqrMagnitude &&
                   dot3 >= 0 && dot3 <= (p4 -p3).sqrMagnitude &&
                   dot4 >= 0 && dot4 <= (p2 -p3).sqrMagnitude;
        }

        /// <summary>
        /// Component-wise average, or zero when the sequence is empty.
        /// </summary>
        public static Vector2 Average(this IEnumerable<Vector2> values)
        {
            Vector2 sum = Vector2.zero;
            int total = 0;
            foreach (Vector2 v in values)
            {
                sum += v;
                total++;
            }

            return total == 0 ? Vector2.zero : sum / total;
        }

        /// <summary>
        /// Random float in [<c>x</c>, <c>y</c>].
        /// </summary>
        public static float RandomBetween(this Vector2 value) => 
            UnityEngine.Random.Range(value.x, value.y);

        /// <summary>
        /// Random int in [<c>x</c>, <c>y</c>). Pass <paramref name="inclusiveMax"/> to include <c>y</c>.
        /// </summary>
        public static int RandomBetween(this Vector2Int value, bool inclusiveMax = false) => 
            UnityEngine.Random.Range(value.x, inclusiveMax ? value.y + 1 : value.y);

        /// <summary>
        /// Divides each component by the matching component of <paramref name="divisor"/>.
        /// </summary>
        public static Vector2 Divide(this Vector2 dividend, Vector2 divisor) => new()
        {
            x = dividend.x / divisor.x,
            y = dividend.y / divisor.y
        };
    }
}
