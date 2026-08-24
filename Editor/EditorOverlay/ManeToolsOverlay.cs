using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Mane.Unity.Editor
{
    [Icon("Packages/com.manefunction.tools-unity/Editor/Icons/InkedKettle@2x.png")]
    [Overlay(typeof(SceneView), "ManeTools", "Mane Tools")]
    public sealed class ManeToolsOverlay : ToolbarOverlay
    {
        private ManeToolsOverlay() : base(ChildrenTransformFreezeToggle.Id) { }
    }
}
