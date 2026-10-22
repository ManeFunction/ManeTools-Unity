using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Scene view toggle that applies the ManeStyle layout to all inspectors, not only the marked ones.
    /// </summary>
    [EditorToolbarElement(Id, typeof(SceneView))]
    internal sealed class ManeStyleForAllToggle : EditorToolbarToggle
    {
        /// <summary>
        /// Overlay toolbar element id.
        /// </summary>
        public const string Id = "ManeTools/ManeStyleForAll";

        /// <summary>
        /// Creates the toolbar toggle and binds it to the ManeStyle for all state.
        /// </summary>
        public ManeStyleForAllToggle()
        {
            tooltip = "ManeStyle for all: use the ManeStyle layout in every inspector, not only in [ManeStyle] ones";
            offIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ManeToolsOverlay.IconPath);
            onIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ManeToolsOverlay.FramedIconPath);
            SetValueWithoutNotify(ManeStyleForAll.Enabled);
            this.RegisterValueChangedCallback(evt => ManeStyleForAll.Enabled = evt.newValue);
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                ManeStyleForAll.EnabledChanged += OnEnabledChanged;
                SetValueWithoutNotify(ManeStyleForAll.Enabled);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
                ManeStyleForAll.EnabledChanged -= OnEnabledChanged);
        }

        private void OnEnabledChanged(bool enabled) => SetValueWithoutNotify(enabled);
    }
}
