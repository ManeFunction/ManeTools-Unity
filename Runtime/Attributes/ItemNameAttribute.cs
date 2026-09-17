// This feature is based on BinaryCats solution, took from this thread:
// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/

using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Renames a list element in the inspector.
    /// </summary>
    public abstract class ItemNameAttribute : PropertyAttribute { }

    /// <summary>
    /// Uses a nested field value as the list element name.
    /// </summary>
    public sealed class ItemNameFromFieldAttribute : ItemNameAttribute
    {
        /// <summary>
        /// Text prepended to the nested field value.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Text appended to the nested field value.
        /// </summary>
        public string Postfix { get; }

        /// <summary>
        /// Nested field used as the element title.
        /// </summary>
        public string TitleVariableName { get; }

        /// <summary>
        /// Creates a name from a nested field, with optional affixes.
        /// </summary>
        /// <param name="titleVariableName">Nested field name.</param>
        /// <param name="prefix">Optional prefix.</param>
        /// <param name="postfix">Optional postfix.</param>
        public ItemNameFromFieldAttribute(string titleVariableName, string prefix = "", string postfix = "")
        {
            Prefix = prefix;
            Postfix = postfix;
            TitleVariableName = titleVariableName;
        }
    }

    /// <summary>
    /// Uses a format string as the list element name.
    /// Include <c>{0}</c> to insert the element index.
    /// </summary>
    public sealed class ItemNameFromStringAttribute : ItemNameAttribute
    {
        /// <summary>
        /// Format string. <c>{0}</c> is replaced with the element index.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Index used for the first element when formatting <see cref="Label"/>.
        /// </summary>
        public int StartingIndex { get; }

        /// <summary>
        /// Creates a name from a format string.
        /// </summary>
        /// <param name="label">Format string; include <c>{0}</c> for the index.</param>
        /// <param name="startingIndex">Index of the first element.</param>
        public ItemNameFromStringAttribute(string label, int startingIndex = 0)
        {
            Label = label;
            StartingIndex = startingIndex;
        }

        /// <summary>
        /// Returns true if <paramref name="label"/> contains a <c>{0}</c> index placeholder.
        /// </summary>
        public static bool HasIndexPlaceholder(string label) =>
            !string.IsNullOrEmpty(label) && label.IndexOf("{0}", StringComparison.Ordinal) >= 0;
    }
}
