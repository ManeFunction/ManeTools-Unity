using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Scene view toggle that freezes child transforms while moving the selection.
    /// </summary>
    [EditorToolbarElement(Id, typeof(SceneView))]
    public sealed class ChildrenTransformFreezeToggle : EditorToolbarToggle
    {
        /// <summary>
        /// Overlay toolbar element id.
        /// </summary>
        public const string Id = "ManeTools/FreezeChildren";

        private const string IconsRoot = "Packages/com.manefunction.tools-unity/Editor/Icons/";

        /// <summary>
        /// Creates the toolbar toggle and binds it to the freezer enabled state.
        /// </summary>
        public ChildrenTransformFreezeToggle()
        {
            tooltip = "Freeze children: transform the selection without moving its active children";
            offIcon = LoadIcon("TransformFreezer-unlock@2x.png");
            onIcon = LoadIcon("TransformFreezer-lock@2x.png");
            SetValueWithoutNotify(ChildrenTransformFreezer.Enabled);
            this.RegisterValueChangedCallback(evt => ChildrenTransformFreezer.Enabled = evt.newValue);
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                ChildrenTransformFreezer.EnabledChanged += OnEnabledChanged;
                SetValueWithoutNotify(ChildrenTransformFreezer.Enabled);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
                ChildrenTransformFreezer.EnabledChanged -= OnEnabledChanged);
        }

        private void OnEnabledChanged(bool enabled) => SetValueWithoutNotify(enabled);

        private static Texture2D LoadIcon(string fileName) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>(IconsRoot + fileName);
    }
}
