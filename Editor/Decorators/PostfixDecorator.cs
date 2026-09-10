using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(PostfixAttribute))]
    internal sealed class PostfixDecorator : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            PostfixAttribute info = (PostfixAttribute)attribute;
            return FieldAffix.CreateHook(info.Text, prefix: false);
        }
    }
}
