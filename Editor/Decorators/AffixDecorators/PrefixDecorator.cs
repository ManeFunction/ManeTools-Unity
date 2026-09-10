using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(PrefixAttribute))]
    internal sealed class PrefixDecorator : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            PrefixAttribute info = (PrefixAttribute)attribute;
            return FieldAffix.CreateHook(info.Text, prefix: true);
        }
    }
}
