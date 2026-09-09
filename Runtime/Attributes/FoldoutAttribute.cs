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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FoldoutAttribute : PropertyAttribute
    {
        public string Header { get; }

        public FoldoutAttribute(string headerName)
        {
            Header = headerName;
        }
    }
}
