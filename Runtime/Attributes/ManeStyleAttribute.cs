using System;

namespace Mane.Unity
{
    /// <summary>
    /// Applies Mane inspector layout to this <see cref="UnityEngine.MonoBehaviour"/> or
    /// <see cref="UnityEngine.ScriptableObject"/>: fixed label column, framed blocks,
    /// <see cref="UnityEngine.SpaceAttribute"/> splits a block,
    /// <see cref="UnityEngine.HeaderAttribute"/> splits a block and adds a section title, and
    /// <see cref="FoldoutAttribute"/> splits a block into a foldout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ManeStyleAttribute : Attribute { }
}
