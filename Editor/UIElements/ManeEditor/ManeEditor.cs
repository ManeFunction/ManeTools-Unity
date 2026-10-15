using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// UI Toolkit inspector that clones the assigned UXML.
    /// <see cref="ManeStyleAttribute"/> on this editor (or on the inspected type) applies
    /// <see cref="ManeEditorStyles"/>.
    /// Override <see cref="BuildInspector"/> to wire controls after the tree is built.
    /// Methods marked with <see cref="EditorButtonAttribute"/> are drawn as buttons below it.
    /// </summary>
    [ManeStyle]
    public abstract class ManeEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset xml;

        /// <summary>
        /// Builds the inspector from the assigned UXML.
        /// </summary>
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = ManeEditorStyles.CreateInspectorRoot(this);
            if (xml == null)
            {
                Debug.LogError($"{GetType().Name} UXML is not assigned.");
                return root;
            }

            xml.CloneTree(root);
            BuildInspector(root);
            EditorButton.AddTo(root, this);
            return root;
        }

        protected virtual void BuildInspector(VisualElement root) { }
    }
}
