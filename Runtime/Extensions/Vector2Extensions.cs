using System;
using System.Collections.Generic;
using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    public static class Vector2Extensions
    {
        public static Vector2 Translate(this Vector2 v, float dX, float dY)
        {
            v.x += dX;
            v.y += dY;
            
            return v;
        }

        public static Vector2 Translate(this Vector2 v, Vector2 d)
        {
            v.x += d.x;
            v.y += d.y;
            
            return v;
        }

        
        public static Vector2 TranslateX(this Vector2 v, float dX)
        {
            v.x += dX;

            return v;
        }

        public static Vector2 TranslateY(this Vector2 v, float dY)
        {
            v.y += dY;

            return v;
        }

        
        public static Vector3 Translate(this Vector2 v, float dX, float dY, float dZ) => 
            new(v.x + dX, v.y + dY, dZ);


        public static Vector2 SetX(this Vector2 v, float x)
        {
            v.x = x;
            
            return v;
        }

        public static Vector2 SetY(this Vector2 v, float y)
        {
            v.y = y;
            
            return v;
        }


        public static Vector2 FlipX(this Vector2 v)
        {
            v.x *= -1;
            
            return v;
        }

        public static Vector2 FlipY(this Vector2 v)
        {
            v.y *= -1;
            
            return v;
        }


        public static Vector3 AddZ(this Vector2 v, float z = 0f) => new(v.x, v.y, z);


        public static Vector2 Clamp(this Vector2 v, float a, float b)
        {
            v.x = v.x.Clamp(a, b);
            v.y = v.y.Clamp(a, b);

            return v;
        }
        
        public static Vector2 Project(this Vector2 v, Vector2 onNormal)
        {
            float num1 = Vector2.Dot(onNormal, onNormal);
            if (num1 < Mathf.Epsilon)
                return Vector2.zero;
            
            float num2 = Vector2.Dot(v, onNormal);
            
            return new Vector2(onNormal.x * num2 / num1, onNormal.y * num2 / num1);
        }

        public static float Area(this Vector2 size) => size.x * size.y;
        
        public static bool IsInsideRectangle(this Vector2 p, Rect rect) =>
            p.IsInsideRectangle(rect.min, new Vector2(rect.xMin, rect.yMax),
                                rect.max, new Vector2(rect.xMax, rect.yMin));
        
        public static bool IsInsideRectangle(this Vector2 p, params Vector2[] rect)
        {
            if (rect.Length != 4)
                throw new ArgumentOutOfRangeException(nameof(rect), "The rect argument must be a 4 point array!");

            return IsInsideRectangle(p, rect[0], rect[1], rect[2], rect[3]);
        }
    
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

        public static float RandomBetween(this Vector2 value) => 
            UnityEngine.Random.Range(value.x, value.y);

        public static int RandomBetween(this Vector2Int value, bool inclusiveMax = false) => 
            UnityEngine.Random.Range(value.x, inclusiveMax ? value.y + 1 : value.y);

        public static Vector2 Divide(this Vector2 dividend, Vector2 divisor) => new()
        {
            x = dividend.x / divisor.x,
            y = dividend.y / divisor.y
        };
    }
}