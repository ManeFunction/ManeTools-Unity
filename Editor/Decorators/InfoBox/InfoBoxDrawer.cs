using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Draws <see cref="InfoBoxAttribute"/> and builds standalone info boxes.
    /// </summary>
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public sealed class InfoBoxDrawer : ManeDecorator<InfoBoxDrawer>
    {
        protected override string XmlFileName => "InfoBox";

        /// <summary>
        /// Builds the info box and binds an optional show condition.
        /// </summary>
        public override VisualElement CreatePropertyGUI()
        {
            InfoBoxAttribute info = (InfoBoxAttribute)attribute;
            VisualElement root = Create(info.Message, info.Type);
            BindShowCondition(root, info.ShowCondition, info.InvertCondition);
            return root;
        }

        /// <summary>
        /// Creates a standalone info box of the given type.
        /// </summary>
        public static VisualElement Create(string message, InfoBoxType type = InfoBoxType.Info)
        {
            VisualTreeAsset tree = new InfoBoxDrawer().Xml;
            if (tree == null)
            {
                Debug.LogError("InfoBox UXML is not assigned.");
                return new Label(message);
            }

            TemplateContainer container = tree.CloneTree();
            VisualElement box = container.Q<VisualElement>("infoBox");
            if (box == null)
                return new Label(message);

            box.Q<Label>("label").text = message;
            box.AddToClassList(TypeClass(type));
            box.AddToClassList(PropertyHost.KeepClass);
            box.RemoveFromHierarchy();

            for (int i = 0; i < container.styleSheets.count; i++)
            {
                StyleSheet sheet = container.styleSheets[i];
                if (!box.styleSheets.Contains(sheet))
                    box.styleSheets.Add(sheet);
            }

            return box;
        }

        // A hidden PropertyHost hook inside the box binds to the field and re-evaluates on every change.
        private static void BindShowCondition(VisualElement root, string showCondition, bool invert)
        {
            if (string.IsNullOrEmpty(showCondition))
                return;

            root.Add(PropertyHost.CreateHook(host => host.WhenBound(property =>
                root.SetVisible(ShouldShow(property, showCondition, invert)))));
        }

        // Shown while the condition is false on any inspected object (true, when inverted).
        // A missing member is logged and leaves the box shown.
        private static bool ShouldShow(SerializedProperty property, string showCondition, bool invert)
        {
            MemberLookup.Condition condition = MemberLookup.EvaluateCondition(property, showCondition, "InfoBox");
            return !condition.Found || (invert ? condition.Any : !condition.All);
        }

        private static string TypeClass(InfoBoxType type) => type switch
        {
            InfoBoxType.Warning => "warning",
            InfoBoxType.Error => "error",
            InfoBoxType.None => "none",
            _ => "info"
        };
    }
}
