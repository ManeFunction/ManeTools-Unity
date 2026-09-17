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
        /// <summary>
        /// Required interface type.
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// True when <see cref="InterfaceType"/> is an interface.
        /// </summary>
        public bool IsInterface => InterfaceType is { IsInterface: true };

        /// <summary>
        /// Restricts the field to objects that implement <paramref name="interfaceType"/>.
        /// </summary>
        /// <param name="interfaceType">Required interface.</param>
        public InterfaceOnlyAttribute(Type interfaceType) => InterfaceType = interfaceType;
    }
}