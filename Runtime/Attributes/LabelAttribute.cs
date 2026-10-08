using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Replaces the field label shown in the inspector without renaming the field itself.
    /// On a list or array it replaces the collection title.
    /// </summary>
    public sealed class LabelAttribute : PropertyAttribute
    {
        /// <summary>
        /// Text shown instead of the default field name.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a label with the given text.
        /// </summary>
        /// <param name="text">Text shown instead of the default field name.</param>
        public LabelAttribute(string text) : base(applyToCollection: true)
        {
            Text = text;
        }
    }
}
