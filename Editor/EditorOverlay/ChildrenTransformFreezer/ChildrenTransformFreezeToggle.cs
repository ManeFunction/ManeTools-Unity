using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [EditorToolbarElement(Id, typeof(SceneView))]
    public sealed class ChildrenTransformFreezeToggle : EditorToolbarToggle
    {
        public const string Id = "ManeTools/FreezeChildren";

        private const string IconsRoot = "Packages/com.manefunction.tools-unity/Editor/Icons/";

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
