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
        /// Text of a <see cref="UnityEngine.HeaderAttribute"/>, <see cref="FoldoutAttribute"/>, or
        /// <see cref="FeatureToggleAttribute"/> to draw the button in,
        /// after the last field of that block. The button goes to the end of the inspector when this is null or empty,
        /// or when no such block is found.
        /// </summary>
        public string Block { get; }

        /// <summary>
        /// When the button is in a <see cref="FoldoutAttribute"/> block: true hides it with the foldout content,
        /// false keeps it drawn at the bottom of the foldout while it is collapsed. Has no effect elsewhere.
        /// </summary>
        public bool HideWithFoldout { get; }

        /// <summary>
        /// Creates a button. Pass <paramref name="name"/> to override the method name.
        /// </summary>
        /// <param name="name">Optional button label.</param>
        /// <param name="block">Optional <c>Header</c> or <c>Foldout</c> text: the button is drawn at the end of that block.</param>
        /// <param name="hideWithFoldout">False to keep the button visible while its foldout is collapsed.</param>
        public EditorButtonAttribute(string name = null, string block = null, bool hideWithFoldout = true)
        {
            Name = name;
            Block = block;
            HideWithFoldout = hideWithFoldout;
        }
    }
}
