using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Type picker for a <see cref="SerializeReference"/> field of an interface or base class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeInterfaceAttribute : PropertyAttribute { }
}
