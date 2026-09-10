using UnityEngine;

namespace Mane.Unity
{
    public sealed class PrefixAttribute : PropertyAttribute
    {
        public string Text { get; }

        public PrefixAttribute(string text)
        {
            Text = text;
        }
    }
}
