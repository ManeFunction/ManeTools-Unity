using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(HeaderAttribute))]
    internal sealed class HeaderDrawer : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            VisualElement marker = new();
            marker.AddToClassList(ManeInspectorLayout.HeaderDecoratorClass);
            marker.style.display = DisplayStyle.None;
            return marker;
        }
    }
}
