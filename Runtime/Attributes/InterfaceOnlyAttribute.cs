using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Use with <see cref="SerializeField"/> on <see cref="UnityEngine.Object"/> reference fields.
    /// Editor validates that the assigned object implements the given interface (and is assignable to the field type).
    /// </summary>
    public sealed class InterfaceOnlyAttribute : PropertyAttribute
    {
        public Type InterfaceType { get; }

        public bool IsInterface => InterfaceType is { IsInterface: true };

        public InterfaceOnlyAttribute(Type interfaceType) => InterfaceType = interfaceType;
    }
}