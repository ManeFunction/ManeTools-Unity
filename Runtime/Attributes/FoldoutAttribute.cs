using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// With <see cref="ManeStyleAttribute"/>, starts a <c>FoldoutBlock</c>.
    /// In Unity's default inspector, starts a standard <c>Foldout</c>.
    /// Following fields stay in that foldout until the next <see cref="HeaderAttribute"/>,
    /// <see cref="SpaceAttribute"/>, or <see cref="FoldoutAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FoldoutAttribute : PropertyAttribute
    {
        /// <summary>
        /// Foldout header text.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Starts a foldout with the given header.
        /// </summary>
        /// <param name="headerName">Foldout header text.</param>
        public FoldoutAttribute(string headerName)
        {
            Header = headerName;
        }
    }
}
