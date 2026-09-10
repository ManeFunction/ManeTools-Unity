using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyDecorator : DecoratorDrawer
    {
        private const string DecoratorContainerClass = "unity-decorator-drawers-container";
        private const string AppliedClass = "mie-read-only";
        private const string PanelFilterClass = "mie-read-only-menu-filter";

        public override VisualElement CreatePropertyGUI()
        {
            VisualElement hook = new()
            {
                style = { display = DisplayStyle.None }
            };
            hook.RegisterCallback<AttachToPanelEvent>(_ => Bind(hook));
            return hook;
        }

        private static void Bind(VisualElement hook)
        {
            PropertyField host = hook.GetFirstAncestorOfType<PropertyField>();
            if (host == null)
                return;

            void TryApply() => Apply(host);

            host.RegisterCallback<GeometryChangedEvent>(_ => TryApply());
            hook.schedule.Execute(TryApply);
            TryApply();
        }

        private static void Apply(PropertyField host)
        {
            if (!host.ClassListContains(AppliedClass))
            {
                host.AddToClassList(AppliedClass);
                host.RegisterCallback<ValidateCommandEvent>(BlockPasteCommand, TrickleDown.TrickleDown);
                host.RegisterCallback<ExecuteCommandEvent>(BlockPasteCommand, TrickleDown.TrickleDown);
            }

            VisualElement root = host.panel?.visualTree;
            if (root != null && !root.ClassListContains(PanelFilterClass))
            {
                root.AddToClassList(PanelFilterClass);
                root.RegisterCallback<ContextualMenuPopulateEvent>(StripPaste);
            }

            foreach (VisualElement child in host.Children())
            {
                if (child.ClassListContains(DecoratorContainerClass))
                    continue;

                child.SetEnabled(false);
            }
        }

        private static void BlockPasteCommand(ValidateCommandEvent evt)
        {
            if (!IsPaste(evt.commandName))
                return;

            evt.StopImmediatePropagation();
        }

        private static void BlockPasteCommand(ExecuteCommandEvent evt)
        {
            if (!IsPaste(evt.commandName))
                return;

            evt.StopImmediatePropagation();
        }

        private static void StripPaste(ContextualMenuPopulateEvent evt)
        {
            if (FindReadOnlyHost(evt.target) == null)
                return;

            DropdownMenu menu = evt.menu;
            for (int i = menu.MenuItems().Count - 1; i >= 0; i--)
            {
                if (menu.MenuItems()[i] is not DropdownMenuAction action)
                    continue;
                if (IsPaste(action.name))
                    menu.RemoveItemAt(i);
            }
        }

        private static PropertyField FindReadOnlyHost(IEventHandler target)
        {
            for (VisualElement ve = target as VisualElement; ve != null; ve = ve.parent)
            {
                if (ve is PropertyField field && field.ClassListContains(AppliedClass))
                    return field;
            }

            return null;
        }

        private static bool IsPaste(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            int slash = name.LastIndexOf('/');
            string leaf = slash >= 0 ? name[(slash + 1)..] : name;
            int tab = leaf.IndexOf('\t');
            if (tab >= 0)
                leaf = leaf[..tab];

            leaf = leaf.Trim();
            return leaf.Equals("Paste", StringComparison.OrdinalIgnoreCase)
                   || leaf.StartsWith("Paste ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
