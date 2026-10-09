using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Adds vertical spacing before the field, like <see cref="SpaceAttribute"/>,
    /// but without splitting a <see cref="ManeStyleAttribute"/> block: the fields stay in the same block.
    /// In Unity's default inspector it looks the same as <see cref="SpaceAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class NonBreakingSpaceAttribute : PropertyAttribute
    {
        /// <summary>
        /// Default spacing height, the same as <see cref="SpaceAttribute"/>.
        /// </summary>
        public const float DefaultHeight = 8f;

        /// <summary>
        /// Spacing height in pixels.
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Adds the default spacing before the field.
        /// </summary>
        public NonBreakingSpaceAttribute() : this(DefaultHeight) { }

        /// <summary>
        /// Adds spacing of the given height before the field.
        /// </summary>
        /// <param name="height">Spacing height in pixels.</param>
        public NonBreakingSpaceAttribute(float height) : base(applyToCollection: true)
        {
            Height = height;
        }
    }
}
