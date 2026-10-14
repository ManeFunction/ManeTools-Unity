using System;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Draws a string (or matching int) as a dropdown of fixed or resolved choices.
    /// </summary>
    public sealed class DropdownListAttribute : PropertyAttribute
    {
        /// <summary>
        /// Static choices used when no source member is set.
        /// </summary>
        public string[] Strings { get; }

        /// <summary>
        /// Type that owns the source member when choices are resolved dynamically.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Field, property or parameterless method that gives a <c>string[]</c>.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Creates a dropdown from a fixed list of strings.
        /// </summary>
        /// <param name="strings">Dropdown choices.</param>
        public DropdownListAttribute(params string[] strings) { Strings = strings; }

        /// <summary>
        /// Creates a dropdown whose choices come from a member on <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type that owns the member. An instance member is read from the object that holds
        /// the field (or the inspected object) when it is of this type; otherwise only static members are used.</param>
        /// <param name="memberName">Field, property or parameterless method giving a <c>string[]</c>.</param>
        public DropdownListAttribute(Type type, string memberName)
        {
            SourceType = type;
            MemberName = memberName;
        }
    }
}
