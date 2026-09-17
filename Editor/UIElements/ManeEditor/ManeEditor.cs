using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// UI Toolkit inspector that clones the assigned UXML.
    /// <see cref="ManeStyleAttribute"/> on this editor (or on the inspected type) applies
    /// <see cref="ManeEditorStyles"/>.
    /// Override <see cref="BuildInspector"/> to wire controls after the tree is built.
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
            VisualElement root = new();
            if (serializedObject.targetObject != null)
                root.userData = serializedObject.targetObject.GetType();

            if (xml == null)
            {
                Debug.LogError($"{GetType().Name} UXML is not assigned.");
                return root;
            }

            if (ManeEditorStyles.HasManeStyle(this))
                ManeEditorStyles.Apply(root);

            xml.CloneTree(root);
            BuildInspector(root);
            return root;
        }

        protected virtual void BuildInspector(VisualElement root) { }
    }
}
