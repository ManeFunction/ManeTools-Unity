using UnityEngine;

namespace Mane.Unity
{
    public sealed class PostfixAttribute : PropertyAttribute
    {
        public string Text { get; }

        public PostfixAttribute(string text)
        {
            Text = text;
        }
    }
}
