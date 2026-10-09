using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Draws an <c>int</c> or <c>float</c> field with the limited editor field: negatives are clamped out
    /// (or limited to -1), and 0 and -1 can be shown as custom text. On a collection it applies to every element.
    /// </summary>
    public sealed class LimitedFieldAttribute : PropertyAttribute
    {
        /// <summary>
        /// When true, -1 is allowed. Any other negative value becomes -1. Otherwise negatives become 0.
        /// </summary>
        public bool AllowNegatives { get; }

        /// <summary>
        /// Text shown instead of -1. Empty shows the number. Only used when <see cref="AllowNegatives"/> is true.
        /// </summary>
        public string NegativeLabel { get; }

        /// <summary>
        /// Text shown instead of 0. Empty shows the number.
        /// </summary>
        public string ZeroLabel { get; }

        /// <summary>
        /// Creates a limited field.
        /// </summary>
        /// <param name="zeroLabel">Text shown instead of 0. Empty shows the number.</param>
        /// <param name="allowNegatives">Allows -1 as a special value.</param>
        /// <param name="negativeLabel">Text shown instead of -1. Empty shows the number.</param>
        public LimitedFieldAttribute(string zeroLabel = "", bool allowNegatives = false, string negativeLabel = "")
        {
            ZeroLabel = zeroLabel ?? string.Empty;
            AllowNegatives = allowNegatives;
            NegativeLabel = negativeLabel ?? string.Empty;
        }
    }
}
