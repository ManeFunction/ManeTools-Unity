using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal static class NonBreakingSpaceDrawer
    {
        public static VisualElement Create(float height)
        {
            VisualElement spacer = new()
            {
                style =
                {
                    height = height,
                    flexShrink = 0
                }
            };
            return spacer;
        }

        public static void HideUnityDecorator(PropertyField field) =>
            UnityDecoratorHider.HideImgui(field);
    }
}
