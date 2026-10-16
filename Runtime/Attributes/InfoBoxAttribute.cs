using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Draws a message box above the field in the inspector.
    /// </summary>
    public sealed class InfoBoxAttribute : PropertyAttribute
    {
        /// <summary>
        /// Message shown in the box.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Visual severity of the box.
        /// </summary>
        public InfoBoxType Type { get; }

        /// <summary>
        /// Optional member that controls whether the box is shown. The box is shown while the condition is
        /// <b>false</b>, which suits a warning that goes away once the condition is met; set
        /// <see cref="InvertCondition"/> to show it while the condition is true instead.
        /// The member is a serialized field (any non-default value counts as true), or a bool field, property,
        /// or parameterless method, on the object that holds the field or on the inspected object.
        /// With several objects selected, the box is shown if any of them needs it.
        /// A member that cannot be found is logged once and leaves the box shown.
        /// </summary>
        public string ShowCondition { get; }

        /// <summary>
        /// When true, the box is shown while <see cref="ShowCondition"/> is true instead of false.
        /// </summary>
        public bool InvertCondition { get; }

        /// <summary>
        /// Creates an info box with an optional type and show condition.
        /// </summary>
        /// <param name="message">Message shown in the box.</param>
        /// <param name="type">Visual severity.</param>
        /// <param name="showCondition">
        /// Optional member name. The box is shown while it is false (see <see cref="ShowCondition"/>).
        /// </param>
        /// <param name="invertCondition">Show the box while the condition is true instead.</param>
        public InfoBoxAttribute(
            string message,
            InfoBoxType type = InfoBoxType.Info,
            string showCondition = null,
            bool invertCondition = false) : base(applyToCollection: true)
        {
            Message = message;
            Type = type;
            ShowCondition = showCondition;
            InvertCondition = invertCondition;
        }

        /// <summary>
        /// Creates an info-severity box with a show condition.
        /// </summary>
        /// <param name="message">Message shown in the box.</param>
        /// <param name="showCondition">
        /// Member name. The box is shown while it is false (see <see cref="ShowCondition"/>).
        /// </param>
        /// <param name="invertCondition">Show the box while the condition is true instead.</param>
        public InfoBoxAttribute(string message, string showCondition, bool invertCondition = false)
            : this(message, InfoBoxType.Info, showCondition, invertCondition)
        {
        }
    }

    /// <summary>
    /// Visual severity for <see cref="InfoBoxAttribute"/>.
    /// </summary>
    public enum InfoBoxType
    {
        /// <summary>
        /// Neutral information.
        /// </summary>
        Info,

        /// <summary>
        /// Warning highlight.
        /// </summary>
        Warning,

        /// <summary>
        /// Error highlight.
        /// </summary>
        Error,

        /// <summary>
        /// Message with no severity styling.
        /// </summary>
        None
    }
}
