using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LabelAttribute))]
    internal sealed class LabelDecorator : DecoratorDrawer
    {
        private const string BaseFieldLabelClass = "unity-base-field__label";

        public override VisualElement CreatePropertyGUI()
        {
            LabelAttribute info = (LabelAttribute)attribute;
            VisualElement hook = new()
            {
                style = { display = DisplayStyle.None }
            };
            hook.RegisterCallback<AttachToPanelEvent>(_ => Bind(hook, info.Text));
            return hook;
        }

        private static void Bind(VisualElement hook, string text)
        {
            PropertyField host = hook.GetFirstAncestorOfType<PropertyField>();
            if (host == null || string.IsNullOrEmpty(text))
                return;

            void TryApply() => Apply(host, text);

            host.RegisterCallback<GeometryChangedEvent>(_ => TryApply());
            hook.schedule.Execute(TryApply);
            TryApply();
        }

        private static void Apply(PropertyField host, string text)
        {
            SerializedProperty property = host.GetBoundSerializedProperty();
            if (property is { isArray: true } && property.propertyType != SerializedPropertyType.String)
            {
                FieldAffix.ApplyCollectionTitle(host, property);
                return;
            }

            // Outermost label first: Query walks in document order, so nested fields come later.
            foreach (VisualElement element in host.Query<VisualElement>().ToList())
            {
                if (element.GetFirstAncestorOfType<PropertyField>() != host)
                    continue;

                if (element is Foldout foldout)
                {
                    if (foldout.text != text)
                        foldout.text = text;
                    return;
                }

                if (element is Label label && label.ClassListContains(BaseFieldLabelClass))
                {
                    if (label.text != text)
                        label.text = text;
                    return;
                }
            }
        }
    }
}
