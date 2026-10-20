using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Marks a <see cref="bool"/> field as the switch of a feature. In Unity's default inspector it works like
    /// <see cref="HeaderAttribute"/>: a bold title above the field.
    /// With <see cref="ManeStyleAttribute"/>, it starts a new block like <see cref="HeaderAttribute"/> does, but
    /// the title is a checkbox: the following fields stay in that block until the next <see cref="HeaderAttribute"/>,
    /// <see cref="SpaceAttribute"/>, <see cref="FoldoutAttribute"/>, or <see cref="FeatureToggleAttribute"/>,
    /// and are hidden while the field is off.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FeatureToggleAttribute : PropertyAttribute
    {
        /// <summary>
        /// Header text. Null or empty takes the field name (or its <see cref="LabelAttribute"/> text).
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Starts a feature block with the given header, or with the field name when none is given.
        /// </summary>
        /// <param name="header">Optional header text.</param>
        public FeatureToggleAttribute(string header = null)
        {
            Header = header;
        }
    }
}
