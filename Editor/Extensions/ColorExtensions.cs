using System.Globalization;
using UnityEngine;

namespace Mane.Unity.Editor
{
    internal static class ColorExtensions
    {
        public static string ToCode(this Color color) =>
            string.Format(CultureInfo.InvariantCulture,
                "new Color({0:n3}f, {1:n3}f, {2:n3}f, {3:n3}f)",
                color.r, color.g, color.b, color.a);
    }
}