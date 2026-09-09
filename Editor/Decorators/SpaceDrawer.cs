using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(SpaceAttribute))]
    internal sealed class SpaceDrawer : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            VisualElement marker = new();
            marker.AddToClassList(ManeInspectorLayout.SpaceDecoratorClass);
            marker.style.display = DisplayStyle.None;
            return marker;
        }
    }
}
