using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Prefix and postfix chips around a field input. <see cref="PropertyHost"/> decides when they apply.
    /// </summary>
    internal static class FieldAffix
    {
        private const string PrefixClass = "mie-prefix";
        private const string PostfixClass = "mie-postfix";
        private const string HasPrefixClass = "mie-has-prefix";
        private const string BaseFieldClass = "unity-base-field";
        private const string InputClass = "unity-base-field__input";
        private const string ChipFieldClass = "unity-composite-field__field";
        private const string MinMaxFieldClass = "mie-minmax-field";

        private static StyleSheet _sheet;

        /// <summary>
        /// Adds a prefix or postfix chip next to the input of <paramref name="host"/>, or next to each
        /// endpoint input of a Min / Max drawer. Does nothing if that chip is already there.
        /// </summary>
        public static void AddChips(PropertyField host, string text, bool prefix)
        {
            VisualElement field = FindBaseField(host);
            if (field != null && field.ClassListContains(MinMaxFieldClass))
            {
                // Min / Max drawer: the affix goes around each endpoint input, not around the whole row.
                foreach (VisualElement endpoint in field.Query(className: ChipFieldClass).ToList())
                {
                    if (endpoint.GetFirstAncestorOfType<PropertyField>() == host)
                        AddChip(endpoint, text, prefix);
                }

                return;
            }

            AddChip(field, text, prefix);
        }

        // Chips are siblings of the input. The input must stay a direct child of its field: numeric fields cast
        // the input's parent to the field when dragging the label, and field styles expect that structure.
        private static void AddChip(VisualElement field, string text, bool prefix)
        {
            VisualElement input = FindInput(field);
            if (input == null)
                return;

            string chipClass = prefix ? PrefixClass : PostfixClass;
            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(chipClass))
                    return;
            }

            ApplySheet(field);
            Label chip = new(text);
            chip.AddToClassList(chipClass);
            if (prefix)
                field.AddToClassList(HasPrefixClass);
            field.Insert(field.IndexOf(input) + (prefix ? 0 : 1), chip);
        }

        private static VisualElement FindBaseField(PropertyField host)
        {
            foreach (VisualElement element in host.Query(className: BaseFieldClass).ToList())
            {
                if (element.GetFirstAncestorOfType<PropertyField>() != host)
                    continue;
                if (element.ClassListContains(ChipFieldClass))
                    continue;

                return element;
            }

            return null;
        }

        private static VisualElement FindInput(VisualElement field)
        {
            if (field == null)
                return null;

            foreach (VisualElement child in field.Children())
            {
                if (child.ClassListContains(InputClass))
                    return child;
            }

            return null;
        }

        private static void ApplySheet(VisualElement root)
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("FieldAffix.uss was not found next to FieldAffix.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(FieldAffix));
    }
}
