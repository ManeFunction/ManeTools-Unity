// This feature is based on BinaryCats solution, took from this thread:
// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/

using System;
using UnityEngine;

namespace Mane.Unity
{
    public abstract class ItemNameAttribute : PropertyAttribute { }

    public sealed class ItemNameFromFieldAttribute : ItemNameAttribute
    {
        public string Prefix { get; }
        public string Postfix { get; }
        public string TitleVariableName { get; }

        public ItemNameFromFieldAttribute(string titleVariableName, string prefix = "", string postfix = "")
        {
            Prefix = prefix;
            Postfix = postfix;
            TitleVariableName = titleVariableName;
        }
    }

    public sealed class ItemNameFromStringAttribute : ItemNameAttribute
    {
        public string Label { get; }
        public int StartingIndex { get; }

        public ItemNameFromStringAttribute(string label, int startingIndex = 0)
        {
            Label = label;
            StartingIndex = startingIndex;
        }

        public static bool HasIndexPlaceholder(string label) =>
            !string.IsNullOrEmpty(label) && label.IndexOf("{0}", StringComparison.Ordinal) >= 0;
    }
}
