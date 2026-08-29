using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    public static class Color32Extensions
    {
        public static uint ToUInt(this Color32 color) =>
            (uint)(color.a << 24
                 | color.r << 16
                 | color.g << 8
                 | color.b);

        public static Color32 ToColor32(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)color;

            return new Color32(r, g, b, a);
        }

        public static string ToHex(this Color32 c) => 
            $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";

        public static float GetHSL_Lightness(this Color32 color)
        {
            Color c = color;
            
            return c.GetHSL_Lightness();
        }

        public static float GetHSL_Hue(this Color32 color)
        {
            Color c = color;
            
            return c.GetHSL_Hue();
        }

        public static float GetHSL_Saturation(this Color32 color)
        {
            Color c = color;
            
            return c.GetHSL_Saturation();
        }

        public static float GetLuma(this Color32 color)
        {
            Color c = color;
            
            return c.GetLuma();
        }

        /// <summary>
        /// Shift RGB color channels
        /// </summary>
        public static Color32 Shift(this Color32 c, byte shift) => new(
            (byte)(c.r + shift).Clamp(0, byte.MaxValue),
            (byte)(c.g + shift).Clamp(0, byte.MaxValue),
            (byte)(c.b + shift).Clamp(0, byte.MaxValue), c.a);

        
        public static Color32 SetR(this Color32 c, byte r)
        {
            c.r = r;

            return c;
        }

        public static Color32 SetG(this Color32 c, byte g)
        {
            c.g = g;

            return c;
        }

        public static Color32 SetB(this Color32 c, byte b)
        {
            c.b = b;

            return c;
        }

        public static Color32 SetA(this Color32 c, byte a)
        {
            c.a = a;

            return c;
        }

        public static Color32 SetRGB(this Color32 c, byte r, byte g, byte b)
        {
            c.r = r;
            c.g = g;
            c.b = b;

            return c;
        }

        public static Color32 SetRGB(this Color32 c, Color32 rgb)
        {
            c.r = rgb.r;
            c.g = rgb.g;
            c.b = rgb.b;

            return c;
        }
    }
}