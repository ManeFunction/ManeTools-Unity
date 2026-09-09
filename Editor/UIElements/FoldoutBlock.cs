using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Bordered inspector block with a foldout header.
    /// Expanded/collapsed state is stored per inspected component type and block id
    /// (<see cref="VisualElement.name"/>, or <see cref="Foldout.text"/> if name is empty).
    /// </summary>
    [UxmlElement]
    public partial class FoldoutBlock : Foldout
    {
        public const string UssClassName = "mie-foldout-block";
        private const string PrefPrefix = "Mane.Editor.FoldoutBlock.";

        private bool _collapsedByDefault;
        private bool _appliedStoredState;

        public FoldoutBlock()
        {
            AddToClassList("mie-block");
            AddToClassList(UssClassName);
            RegisterCallback<AttachToPanelEvent>(_ => ApplyStoredState());
            RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.target != this)
                    return;

                ApplyCollapsedStyle(evt.newValue);
                SaveState(evt.newValue);
            });
        }

        [UxmlAttribute("collapsed-by-default")]
        public bool CollapsedByDefault
        {
            get => _collapsedByDefault;
            set => _collapsedByDefault = value;
        }

        private void ApplyStoredState()
        {
            if (_appliedStoredState)
                return;

            _appliedStoredState = true;
            string key = PrefKey();
            bool expanded = key != null && EditorPrefs.HasKey(key)
                ? EditorPrefs.GetBool(key)
                : !_collapsedByDefault;
            SetValueWithoutNotify(expanded);
            ApplyCollapsedStyle(expanded);
        }

        private void ApplyCollapsedStyle(bool expanded) =>
            EnableInClassList(UssClassName + "--collapsed", !expanded);

        private void SaveState(bool expanded)
        {
            string key = PrefKey();
            if (key == null)
                return;

            EditorPrefs.SetBool(key, expanded);
        }

        private string PrefKey()
        {
            Type type = FindInspectedType();
            if (type == null)
                return null;

            string id = !string.IsNullOrEmpty(name) ? name : text;
            if (string.IsNullOrEmpty(id))
                return null;

            return PrefPrefix + type.FullName + "." + id;
        }

        private Type FindInspectedType()
        {
            for (VisualElement element = this; element != null; element = element.parent)
            {
                if (element.userData is Type type)
                    return type;
            }

            return null;
        }
    }
}
