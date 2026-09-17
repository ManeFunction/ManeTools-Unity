using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Shows text immediately after the field value in the inspector.
    /// </summary>
    public sealed class PostfixAttribute : PropertyAttribute
    {
        /// <summary>
        /// Affix shown after the value.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a postfix with the given text.
        /// </summary>
        /// <param name="text">Affix shown after the value.</param>
        public PostfixAttribute(string text) : base(applyToCollection: true)
        {
            Text = text;
        }
    }
}
