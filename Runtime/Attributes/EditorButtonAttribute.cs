using System;

namespace Mane.Unity
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EditorButtonAttribute : Attribute
    {
        public string Name { get; }

        public EditorButtonAttribute(string name = null)
        {
            Name = name;
        }
    }
}
