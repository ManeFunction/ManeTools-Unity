using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Section-header checkbox: bold title via <see cref="Toggle.text"/>, not the inspector label column.
    /// Bound and unbound toggles stay visually identical; inspector alignment is stripped.
    /// When a sibling <c>contentContainer</c> (or <c>mie-content</c>) exists, it is shown only while checked.
    /// </summary>
    [UxmlElement]
    public partial class HeaderToggle : Toggle
    {
        public const string UssClassName = "mie-header-toggle";
        private const string AlignedFieldClass = "unity-base-field__aligned";
        private const string ContentName = "contentContainer";

        private static StyleSheet _sheet;

        public HeaderToggle()
        {
            AddToClassList(UssClassName);
            ApplySheet();
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                StripInspectorChrome();
                ApplyContentVisibility();
                schedule.Execute(StripInspectorChrome);
            });
            RegisterCallback<GeometryChangedEvent>(_ => StripInspectorChrome());
            RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.target != this)
                    return;

                ApplyContentVisibility();
            });
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            ApplyContentVisibility();
        }

        private void StripInspectorChrome()
        {
            if (ClassListContains(AlignedFieldClass))
                RemoveFromClassList(AlignedFieldClass);

            Label fieldLabel = labelElement;
            if (fieldLabel == null)
                return;

            if (fieldLabel.resolvedStyle.display != DisplayStyle.None)
                fieldLabel.style.display = DisplayStyle.None;
        }

        private void ApplyContentVisibility()
        {
            VisualElement content = FindContent();
            if (content == null)
                return;

            content.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplySheet()
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("HeaderToggle.uss was not found next to HeaderToggle.");
                return;
            }

            if (!styleSheets.Contains(sheet))
                styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(HeaderToggle));

        private VisualElement FindContent()
        {
            if (parent == null)
                return null;

            VisualElement named = parent.Q<VisualElement>(ContentName);
            return named ?? parent.Q(className: "mie-content");
        }
    }
}
