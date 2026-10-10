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
            return PropertyHost.CreateHook(host => host.Postfix = info.Text);
        }
    }
}
