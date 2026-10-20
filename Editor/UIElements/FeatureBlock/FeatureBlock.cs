using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Bordered inspector block whose title is a <see cref="HeaderToggle"/>. The content is shown only while
    /// the toggle is on. It is what <see cref="FeatureToggleAttribute"/> builds in a ManeStyle inspector,
    /// and it can be used in UXML: children go to the content, and <c>toggle-path</c> binds the toggle.
    /// </summary>
    [UxmlElement]
    public sealed partial class FeatureBlock : VisualElement
    {
        /// <summary>
        /// USS class on the block root.
        /// </summary>
        public const string UssClassName = "mie-feature-block";
        private const string ToggleName = "isEnableToggle";
        private const string ContentName = "contentContainer";

        private readonly HeaderToggle _toggle;
        private readonly VisualElement _content;

        /// <summary>
        /// Creates an empty block with an unbound toggle.
        /// </summary>
        public FeatureBlock()
        {
            AddToClassList("mie-block");
            AddToClassList(UssClassName);

            _toggle = new HeaderToggle { name = ToggleName };
            _content = new VisualElement { name = ContentName };
            _content.AddToClassList("mie-content");

            hierarchy.Add(_toggle);
            hierarchy.Add(_content);
        }

        /// <summary>
        /// Where children are added: the part of the block that the toggle shows and hides.
        /// </summary>
        public override VisualElement contentContainer => _content;

        /// <summary>
        /// The title checkbox. Hides the content while it is off.
        /// </summary>
        public HeaderToggle Toggle => _toggle;

        /// <summary>
        /// Title text.
        /// </summary>
        [UxmlAttribute("header")]
        public string Header
        {
            get => _toggle.text;
            set => _toggle.text = value;
        }

        /// <summary>
        /// Binding path of the toggle, relative to the binding root above the block.
        /// </summary>
        [UxmlAttribute("toggle-path")]
        public string TogglePath
        {
            get => _toggle.bindingPath;
            set => _toggle.bindingPath = value;
        }

        /// <summary>
        /// Binds the toggle to a <see cref="bool"/> property.
        /// </summary>
        public void BindToggle(UnityEditor.SerializedProperty property) => _toggle.BindProperty(property);
    }
}
