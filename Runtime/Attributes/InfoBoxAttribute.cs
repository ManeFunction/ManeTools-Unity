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
        /// Optional bool member that controls whether the box is shown.
        /// </summary>
        public string ShowCondition { get; }

        /// <summary>
        /// When true, the show condition is inverted.
        /// </summary>
        public bool InvertCondition { get; }

        /// <summary>
        /// Creates an info box with an optional type and show condition.
        /// </summary>
        /// <param name="message">Message shown in the box.</param>
        /// <param name="type">Visual severity.</param>
        /// <param name="showCondition">Optional bool member name.</param>
        /// <param name="invertCondition">Invert the show condition.</param>
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
        /// <param name="showCondition">Bool member name.</param>
        /// <param name="invertCondition">Invert the show condition.</param>
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
