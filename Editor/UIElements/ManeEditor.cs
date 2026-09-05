using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// UI Toolkit inspector that applies <see cref="ManeEditorStyles"/> and clones the assigned UXML.
    /// Override <see cref="BuildInspector"/> to wire controls after the tree is built.
    /// </summary>
    public abstract class ManeEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset xml;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            if (xml == null)
            {
                Debug.LogError($"{GetType().Name} UXML is not assigned.");
                return root;
            }

            ManeEditorStyles.Apply(root);
            xml.CloneTree(root);
            BuildInspector(root);
            return root;
        }

        protected virtual void BuildInspector(VisualElement root) { }
    }
}
