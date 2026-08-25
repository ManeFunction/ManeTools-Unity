using UnityEngine;
using Mane.DotNet;

namespace Mane.Unity
{
    /// <summary>
    /// Random opaque colors in RGB.
    /// </summary>
    public static class RandomColor
    {
        /// <summary>
        /// Returns an opaque <see cref="UnityEngine.Color"/> with each channel uniform in [0, 1).
        /// </summary>
        public static Color Color(IRandom random) =>
            new(random.Range01(), random.Range01(), random.Range01());

        /// <summary>
        /// Returns an opaque <see cref="UnityEngine.Color32"/> with each channel uniform in [0, 255].
        /// </summary>
        public static Color32 Color32(IRandom random) =>
            new(
                (byte)random.Next(0, 256), 
                (byte)random.Next(0, 256), 
                (byte)random.Next(0, 256), 255);
    }
}