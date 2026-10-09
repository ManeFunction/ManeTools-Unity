using System;

namespace Mane.Unity
{
    /// <summary>
    /// Applies Mane inspector layout: fixed label column, framed blocks,
    /// <see cref="UnityEngine.SpaceAttribute"/> splits a block,
    /// <see cref="UnityEngine.HeaderAttribute"/> splits a block and adds a section title, and
    /// <see cref="FoldoutAttribute"/> splits a block into a foldout, and
    /// <see cref="NonBreakingSpaceAttribute"/> adds spacing inside the current block.
    /// Put on a <see cref="UnityEngine.MonoBehaviour"/> or <see cref="UnityEngine.ScriptableObject"/>,
    /// or on a custom inspector type (all <c>ManeEditor</c> subclasses inherit it).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ManeStyleAttribute : Attribute { }
}
