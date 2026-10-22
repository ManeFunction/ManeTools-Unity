using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Scene view overlay hosting Mane Tools toolbar items.
    /// </summary>
    [Icon(IconPath)]
    [Overlay(typeof(SceneView), "ManeTools", "Mane Tools")]
    internal sealed class ManeToolsOverlay : ToolbarOverlay
    {
        internal const string IconPath = "Packages/com.manefunction.tools-unity/Editor/Icons/ManeToolsSig@2x.png";
        internal const string FramedIconPath = "Packages/com.manefunction.tools-unity/Editor/Icons/ManeToolsSig-framed@2x.png";

        private ManeToolsOverlay() : base(ChildrenTransformFreezeToggle.Id, ManeStyleForAllToggle.Id) { }
    }
}
