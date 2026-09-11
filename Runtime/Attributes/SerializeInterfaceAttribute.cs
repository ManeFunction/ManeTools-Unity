using System;
using UnityEngine;

namespace Mane.Unity
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeInterfaceAttribute : PropertyAttribute { }
}
