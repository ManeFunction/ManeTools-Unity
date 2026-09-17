using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Helpers for <see cref="Color32"/> packing, HSL, and hex.
    /// </summary>
    public static class Color32Extensions
    {
        /// <summary>
        /// Packs the color as <c>0xAARRGGBB</c>.
        /// </summary>
        public static uint ToUInt(this Color32 color) =>
            (uint)(color.a << 24
                 | color.r << 16
                 | color.g << 8
                 | color.b);

        /// <summary>
        /// Unpacks a <c>0xAARRGGBB</c> value.
        /// </summary>
        public static Color32 ToColor32(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)color;

            return new Color32(r, g, b, a);
        }

        /// <summary>
        /// Returns an <c>#RRGGBBAA</c> hex string.
        /// </summary>
        public static string ToHex(this Color32 c) => 
            $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";

        /// <summary>
        /// Returns HSL lightness in [0, 1].
        /// </summary>
        public static float GetHSL_Lightness(this Color32 color)
        {
            Color c = color;
            
            return c.GetHSL_Lightness();
        }

        /// <summary>
        /// Returns HSL hue in [0, 1].
        /// </summary>
        public static float GetHSL_Hue(this Color32 color)
        {
            Color c = color;
            
            return c.GetHSL_Hue();
        }

        /// <summary>
        /// Returns HSL saturation in [0, 1].
        /// </summary>
        public static float GetHSL_Saturation(this Color32 color)
        {
            Color c = color;
            
            return c.GetHSL_Saturation();
        }

        /// <summary>
        /// Returns a rough luma estimate (0.2R + 0.7G + 0.1B).
        /// </summary>
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

        
        /// <summary>
        /// Returns a copy with red set to <paramref name="r"/>.
        /// </summary>
        public static Color32 SetR(this Color32 c, byte r)
        {
            c.r = r;

            return c;
        }

        /// <summary>
        /// Returns a copy with green set to <paramref name="g"/>.
        /// </summary>
        public static Color32 SetG(this Color32 c, byte g)
        {
            c.g = g;

            return c;
        }

        /// <summary>
        /// Returns a copy with blue set to <paramref name="b"/>.
        /// </summary>
        public static Color32 SetB(this Color32 c, byte b)
        {
            c.b = b;

            return c;
        }

        /// <summary>
        /// Returns a copy with alpha set to <paramref name="a"/>.
        /// </summary>
        public static Color32 SetA(this Color32 c, byte a)
        {
            c.a = a;

            return c;
        }

        /// <summary>
        /// Returns a copy with RGB set from the given channels.
        /// </summary>
        public static Color32 SetRGB(this Color32 c, byte r, byte g, byte b)
        {
            c.r = r;
            c.g = g;
            c.b = b;

            return c;
        }

        /// <summary>
        /// Returns a copy with RGB taken from <paramref name="rgb"/>, keeping this alpha.
        /// </summary>
        public static Color32 SetRGB(this Color32 c, Color32 rgb)
        {
            c.r = rgb.r;
            c.g = rgb.g;
            c.b = rgb.b;

            return c;
        }
    }
}