using UnityEditor.UIElements;
namespace Mane.Unity.Editor
{
    internal static class SpaceDrawer
    {
        public static void HideUnityDecorator(PropertyField field) =>
            UnityDecoratorHider.HideImgui(field);
    }
}
