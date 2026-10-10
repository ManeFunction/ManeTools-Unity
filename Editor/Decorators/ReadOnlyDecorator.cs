using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyDecorator : DecoratorDrawer
    {
        private const string AppliedClass = "mie-read-only";
        private const string PanelFilterClass = "mie-read-only-menu-filter";

        public override VisualElement CreatePropertyGUI() =>
            PropertyHost.CreateHook(host =>
            {
                host.ReadOnly = true;
                BlockPaste(host.Field);
            });

        // Disabled fields still accept Paste from the keyboard and the context menu.
        private static void BlockPaste(PropertyField field)
        {
            if (!field.ClassListContains(AppliedClass))
            {
                field.AddToClassList(AppliedClass);
                field.RegisterCallback<ValidateCommandEvent>(BlockPasteCommand, TrickleDown.TrickleDown);
                field.RegisterCallback<ExecuteCommandEvent>(BlockPasteCommand, TrickleDown.TrickleDown);
            }

            VisualElement root = field.panel?.visualTree;
            if (root != null && !root.ClassListContains(PanelFilterClass))
            {
                root.AddToClassList(PanelFilterClass);
                root.RegisterCallback<ContextualMenuPopulateEvent>(StripPaste);
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
