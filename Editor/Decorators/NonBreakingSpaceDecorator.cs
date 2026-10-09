using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(NonBreakingSpaceAttribute))]
    internal sealed class NonBreakingSpaceDecorator : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI() =>
            NonBreakingSpaceDrawer.Create(((NonBreakingSpaceAttribute)attribute).Height);
    }
}
