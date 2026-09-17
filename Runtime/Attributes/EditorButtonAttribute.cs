using System;

namespace Mane.Unity
{
    /// <summary>
    /// Adds an inspector button that invokes this parameterless method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EditorButtonAttribute : Attribute
    {
        /// <summary>
        /// Button label. The method name is used when this is null or empty.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a button. Pass <paramref name="name"/> to override the method name.
        /// </summary>
        /// <param name="name">Optional button label.</param>
        public EditorButtonAttribute(string name = null)
        {
            Name = name;
        }
    }
}
