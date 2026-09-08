// This feature is based on BinaryCats solution, took from this thread:
// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/

using UnityEngine;

namespace Mane.Unity
{
    public class CollectionItemNameAttribute : PropertyAttribute
    {
        public string Prefix { get; }
        public string Postfix { get; }
        public string TitleVariableName { get; }

        public CollectionItemNameAttribute(string titleVariableName, string prefix = "", string postfix = "")
        {
            Prefix = prefix;
            Postfix = postfix;
            TitleVariableName = titleVariableName;
        }
    }
}
