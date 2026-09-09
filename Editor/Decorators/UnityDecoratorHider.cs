using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal static class UnityDecoratorHider
    {
        private const string DecoratorContainerClass = "unity-decorator-drawers-container";
        private const string HiddenClass = "mie-unity-imgui-hidden";
        private const string AvailableIfName = "mie-available-if";

        public static void HideImgui(PropertyField field)
        {
            if (field.ClassListContains(HiddenClass))
                return;

            field.AddToClassList(HiddenClass);
            field.RegisterCallback<AttachToPanelEvent>(_ => Hide(field));
            field.RegisterCallback<GeometryChangedEvent>(_ => Hide(field));
            field.schedule.Execute(() => Hide(field));
        }

        private static void Hide(PropertyField field)
        {
            foreach (IMGUIContainer imgui in field.Query<IMGUIContainer>().ToList())
            {
                if (imgui.GetFirstAncestorOfType<PropertyField>() != field)
                    continue;

                SetHidden(imgui);
            }

            VisualElement container = FindDecoratorContainer(field);
            if (container == null)
                return;

            bool anyVisible = false;
            foreach (VisualElement child in container.Children())
            {
                if (ShouldKeep(child))
                {
                    anyVisible = true;
                    continue;
                }

                SetHidden(child);
            }

            if (!anyVisible)
                SetHidden(container);
        }

        private static bool ShouldKeep(VisualElement child) =>
            child.name == AvailableIfName || child.ClassListContains("message-box");

        private static void SetHidden(VisualElement element)
        {
            if (element.resolvedStyle.display != DisplayStyle.None)
                element.style.display = DisplayStyle.None;
        }

        private static VisualElement FindDecoratorContainer(PropertyField field)
        {
            foreach (VisualElement element in field.Query(className: DecoratorContainerClass).ToList())
            {
                if (element.GetFirstAncestorOfType<PropertyField>() == field)
                    return element;
            }

            return null;
        }
    }
}
