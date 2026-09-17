using System;
using System.Collections.Generic;
using System.Linq;
using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for translating, clamping, and testing <see cref="Vector3"/> values.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Adds the given deltas to the components.
        /// </summary>
        public static Vector3 Translate(this Vector3 v, float dX, float dY, float dZ)
        {
            v.x += dX;
            v.y += dY;
            v.z += dZ;
            
            return v;
        }

        /// <summary>
        /// Adds <paramref name="d"/> to the vector.
        /// </summary>
        public static Vector3 Translate(this Vector3 v, Vector3 d)
        {
            v.x += d.x;
            v.y += d.y;
            v.z += d.z;
            
            return v;
        }

        /// <summary>
        /// Adds XY from <paramref name="d"/>, leaving Z unchanged.
        /// </summary>
        public static Vector3 Translate(this Vector3 v, Vector2 d)
        {
            v.x += d.x;
            v.y += d.y;
            
            return v;
        }


        /// <summary>
        /// Adds <paramref name="dX"/> to X.
        /// </summary>
        public static Vector3 TranslateX(this Vector3 v, float dX)
        {
            v.x += dX;

            return v;
        }

        /// <summary>
        /// Adds <paramref name="dY"/> to Y.
        /// </summary>
        public static Vector3 TranslateY(this Vector3 v, float dY)
        {
            v.y += dY;

            return v;
        }

        /// <summary>
        /// Adds <paramref name="dZ"/> to Z.
        /// </summary>
        public static Vector3 TranslateZ(this Vector3 v, float dZ)
        {
            v.z += dZ;

            return v;
        }

        
        /// <summary>
        /// Returns a copy with X set to <paramref name="x"/>.
        /// </summary>
        public static Vector3 SetX(this Vector3 v, float x)
        {
            v.x = x;
            
            return v;
        }

        /// <summary>
        /// Returns a copy with Y set to <paramref name="y"/>.
        /// </summary>
        public static Vector3 SetY(this Vector3 v, float y)
        {
            v.y = y;
            
            return v;
        }

        /// <summary>
        /// Returns a copy with Z set to <paramref name="z"/>.
        /// </summary>
        public static Vector3 SetZ(this Vector3 v, float z)
        {
            v.z = z;
            
            return v;
        }
        

        /// <summary>
        /// Negates X.
        /// </summary>
        public static Vector3 FlipX(this Vector3 v)
        {
            v.x *= -1;
            
            return v;
        }

        /// <summary>
        /// Negates Y.
        /// </summary>
        public static Vector3 FlipY(this Vector3 v)
        {
            v.y *= -1;
            
            return v;
        }

        /// <summary>
        /// Negates Z.
        /// </summary>
        public static Vector3 FlipZ(this Vector3 v)
        {
            v.z *= -1;
            
            return v;
        }


        /// <summary>
        /// Clamps each component between <paramref name="a"/> and <paramref name="b"/>.
        /// </summary>
        public static Vector3 Clamp(this Vector3 v, float a, float b)
        {
            v.x = v.x.Clamp(a, b);
            v.y = v.y.Clamp(a, b);
            v.z = v.z.Clamp(a, b);

            return v;
        }

        /// <summary>
        /// Returns X times Y times Z.
        /// </summary>
        public static float Volume(this Vector3 size) => size.x * size.y * size.z;

        // Thanks to bronxbomber92 (https://forum.unity.com/threads/math-problem.8114/#post-59715)
        /// <summary>
        /// Closest point on the segment from <paramref name="vA"/> to <paramref name="vB"/>.
        /// </summary>
        public static Vector3 ClosestPointOnLine(this Vector3 vPoint, Vector3 vA, Vector3 vB)
        {
            Vector3 vVector1 = vPoint - vA;
            Vector3 vVector2 = (vB - vA).normalized;
 
            float d = Vector3.Distance(vA, vB);
            float t = Vector3.Dot(vVector2, vVector1);
 
            if (t <= 0) return vA;
            if (t >= d) return vB;
 
            Vector3 vVector3 = vVector2 * t;
            Vector3 vClosestPoint = vA + vVector3;
 
            return vClosestPoint;
        }

        // Thanks to Saeed Amiri (https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon)
        /// <summary>
        /// Define polygon with points CW or CCW, works with convex polygons
        /// </summary>
        public static bool IsInPolygon(this Vector3 point, Vector3[] poly)
        {
            float[] coef = new float[poly.Length];
            for (int i = 0; i < poly.Length; i++)
            {
                Vector3 prev = poly[i == 0 ? poly.Length - 1 : i - 1];
                Vector3 cur = poly[i];
                coef[i] = (point.y - cur.y) * (prev.x - cur.x) 
                        - (point.x - cur.x) * (prev.y - cur.y);
            }

            if (coef.Any(p => Math.Abs(p) < ManeConst.FloatTolerance)) return true;

            for (int i = 1; i < coef.Length; i++)
                if (coef[i] * coef[i - 1] < 0) return false;
            
            return true;
        }

        /// <summary>
        /// Component-wise average, or zero when the sequence is empty.
        /// </summary>
        public static Vector3 Average(this IEnumerable<Vector3> values)
        {
            Vector3 sum = Vector3.zero;
            int total = 0;
            foreach (Vector3 v in values)
            {
                sum += v;
                total++;
            }

            return total == 0 ? Vector3.zero : sum / total;
        }

        /// <summary>
        /// Divides each component by the matching component of <paramref name="divisor"/>.
        /// </summary>
        public static Vector3 Divide(this Vector3 dividend, Vector3 divisor) => new()
        {
            x = dividend.x / divisor.x,
            y = dividend.y / divisor.y,
            z = dividend.z / divisor.z
        };
    }
}
