using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(AvailableIfAttribute))]
    internal sealed class AvailableIfDrawer : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            AvailableIfAttribute info = (AvailableIfAttribute)attribute;
            return PropertyHost.CreateHook(host =>
            {
                if (string.IsNullOrEmpty(info.PropertyName))
                {
                    host.SetAvailable(info.IsAvailable, info.Hide);
                    return;
                }

                host.WhenBound(property => host.SetAvailable(IsAvailable(property, info), info.Hide));
            });
        }

        // Available only when it is on every inspected object. A missing member is logged and leaves the field available.
        private static bool IsAvailable(SerializedProperty property, AvailableIfAttribute info)
        {
            MemberLookup.Condition condition =
                MemberLookup.EvaluateCondition(property, info.PropertyName, "AvailableIf");
            return !condition.Found || (info.Invert ? !condition.Any : condition.All);
        }
    }
}
