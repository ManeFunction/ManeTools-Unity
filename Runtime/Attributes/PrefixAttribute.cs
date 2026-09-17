using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Shows text immediately before the field value in the inspector.
    /// </summary>
    public sealed class PrefixAttribute : PropertyAttribute
    {
        /// <summary>
        /// Affix shown before the value.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a prefix with the given text.
        /// </summary>
        /// <param name="text">Affix shown before the value.</param>
        public PrefixAttribute(string text) : base(applyToCollection: true)
        {
            Text = text;
        }
    }
}
