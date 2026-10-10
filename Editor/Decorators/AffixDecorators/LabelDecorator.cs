using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LabelAttribute))]
    internal sealed class LabelDecorator : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            LabelAttribute info = (LabelAttribute)attribute;
            return PropertyHost.CreateHook(host => host.Label = info.Text);
        }
    }
}
