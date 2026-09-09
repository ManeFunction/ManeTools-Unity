using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal static class HeaderDrawer
    {
        public static VisualElement Create(string text)
        {
            Label label = new(text);
            label.AddToClassList("mie-header");
            return label;
        }

        public static void HideUnityDecorator(PropertyField field) =>
            UnityDecoratorHider.HideImgui(field);
    }
}
