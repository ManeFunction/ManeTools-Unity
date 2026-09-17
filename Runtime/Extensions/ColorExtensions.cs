using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for <see cref="Color"/> channels, HSL, and hex.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Returns HSL lightness in [0, 1].
        /// </summary>
        public static float GetHSL_Lightness(this Color color)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;
            float l = r;
            float m = r;
            
            if (g > l) l = g;
            if (b > l) l = b;
            if (g < m) m = g;
            if (b < m) m = b;

            return (l + m) * 0.5f;
        }

        /// <summary>
        /// Returns HSL hue in [0, 1].
        /// </summary>
        public static float GetHSL_Hue(this Color color)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;
            
            float max = Mathf.Max(r, Mathf.Max(g, b));
            float min = Mathf.Min(r, Mathf.Min(g, b));
            float delta = max - min;

            float hue = 0f;

            if (delta != 0f)
            {
                if (Math.Abs(max - r) < float.Epsilon)
                    hue = (g - b) / delta;
                else if (Math.Abs(max - g) < float.Epsilon)
                    hue = 2f + (b - r) / delta;
                else
                    hue = 4f + (r - g) / delta;

                hue /= 6f;

                if (hue < 0f)
                    hue += 1f;
            }

            return hue;
        }

        /// <summary>
        /// Returns HSL saturation in [0, 1].
        /// </summary>
        public static float GetHSL_Saturation(this Color color)
        {
            float result;

            float r = color.r;
            float g = color.g;
            float b = color.b;
            float l = r;
            float m = r;
            
            if (g > l) l = g;
            if (b > l) l = b;
            if (g < m) m = g;
            if (b < m) m = b;

            if (Mathf.Approximately(l, m))
                result = 0f;
            else
            {
                float n = (l + m) * 0.5f;
                if (n <= 0.5f)
                    result = (l - m) / (l + m);
                else
                    result = (l - m) / ((2f - l) - m);
            }

            return result;
        }

        /// <summary>
        /// Returns a rough luma estimate (0.2R + 0.7G + 0.1B).
        /// </summary>
        public static float GetLuma(this Color color) => color.r * .2f + color.g * .7f + color.b * .1f;

        /// <summary>
        /// Shift RGB color channels
        /// </summary>
        public static Color Shift(this Color c, float shift) => new(
            Mathf.Clamp01(c.r + shift), 
            Mathf.Clamp01(c.g + shift), 
            Mathf.Clamp01(c.b + shift), c.a);

        /// <summary>
        /// Returns an <c>#RRGGBBAA</c> hex string.
        /// </summary>
        public static string ToHex(this Color c)
        {
            Color32 c32 = c;
            
            return c32.ToHex();
        }
        

        /// <summary>
        /// Returns a copy with red set to <paramref name="r"/>.
        /// </summary>
        public static Color SetR(this Color c, float r)
        {
            c.r = r;

            return c;
        }

        /// <summary>
        /// Returns a copy with green set to <paramref name="g"/>.
        /// </summary>
        public static Color SetG(this Color c, float g)
        {
            c.g = g;

            return c;
        }

        /// <summary>
        /// Returns a copy with blue set to <paramref name="b"/>.
        /// </summary>
        public static Color SetB(this Color c, float b)
        {
            c.b = b;

            return c;
        }

        /// <summary>
        /// Returns a copy with alpha set to <paramref name="a"/>.
        /// </summary>
        public static Color SetA(this Color c, float a)
        {
            c.a = a;

            return c;
        }
        
        /// <summary>
        /// Returns a copy with RGB set from the given channels.
        /// </summary>
        public static Color SetRGB(this Color c, float r, float g, float b)
        {
            c.r = r;
            c.g = g;
            c.b = b;

            return c;
        }
        
        /// <summary>
        /// Returns a copy with RGB taken from <paramref name="rgb"/>, keeping this alpha.
        /// </summary>
        public static Color SetRGB(this Color c, Color rgb)
        {
            c.r = rgb.r;
            c.g = rgb.g;
            c.b = rgb.b;

            return c;
        }
    }
}