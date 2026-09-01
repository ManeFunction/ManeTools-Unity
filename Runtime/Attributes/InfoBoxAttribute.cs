using UnityEngine;

namespace Mane.Unity
{
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoBoxType Type { get; }
        public string ShowCondition { get; }
        public bool InvertCondition { get; }

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

        public InfoBoxAttribute(string message, string showCondition, bool invertCondition = false)
            : this(message, InfoBoxType.Info, showCondition, invertCondition)
        {
        }
    }

    public enum InfoBoxType
    {
        Info,
        Warning,
        Error,
        None
    }
}
