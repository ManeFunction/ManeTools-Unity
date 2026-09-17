using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Enables, disables, or hides a field based on a bool or another member.
    /// </summary>
    public sealed class AvailableIfAttribute : PropertyAttribute
    {
        /// <summary>
        /// Constant availability used when <see cref="PropertyName"/> is not set.
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// Name of a bool field, property, or parameterless method on the target.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// When true, the field is hidden instead of disabled.
        /// </summary>
        public bool Hide { get; }

        /// <summary>
        /// When true, the resolved condition is inverted.
        /// </summary>
        public bool Invert { get; }

        /// <summary>
        /// Makes the field available or not using a constant.
        /// </summary>
        /// <param name="isAvailable">True to enable the field.</param>
        /// <param name="hide">Hide the field instead of disabling it.</param>
        public AvailableIfAttribute(bool isAvailable, bool hide = false)
        {
            IsAvailable = isAvailable;
            Hide = hide;
        }

        /// <summary>
        /// Makes the field available based on another member.
        /// </summary>
        /// <param name="propertyName">Bool field, property, or parameterless method name.</param>
        /// <param name="invert">Invert the resolved value.</param>
        /// <param name="hide">Hide the field instead of disabling it.</param>
        public AvailableIfAttribute(string propertyName, bool invert = false, bool hide = false)
        {
            PropertyName = propertyName;
            Invert = invert;
            Hide = hide;
        }
    }
}
