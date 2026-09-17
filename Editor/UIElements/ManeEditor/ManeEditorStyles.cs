using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Shared Mane editor USS and inspector field layout.
    /// Applied by <see cref="ManeEditor"/>, or call <see cref="Apply(VisualElement)"/> yourself.
    /// </summary>
    public static class ManeEditorStyles
    {
        /// <summary>
        /// USS class on the inspector root.
        /// </summary>
        public const string RootClass = "mie-root";

        /// <summary>
        /// USS class that enables Mane field layout.
        /// </summary>
        public const string FieldsClass = "mie-fields";

        /// <summary>
        /// What <see cref="Apply(VisualElement, Options)"/> should attach.
        /// </summary>
        [Flags]
        public enum Options
        {
            /// <summary>
            /// Root class only.
            /// </summary>
            None = 0,

            /// <summary>
            /// Shared Mane editor stylesheet.
            /// </summary>
            Sheet = 1,

            /// <summary>
            /// Field layout stylesheet and list footer styling.
            /// </summary>
            FieldLayout = 2,

            /// <summary>
            /// Strip Unity inspector label alignment.
            /// </summary>
            DisableInspectorAlignment = 4,

            /// <summary>
            /// Stylesheet, field layout, and alignment strip.
            /// </summary>
            Inspector = Sheet | FieldLayout | DisableInspectorAlignment
        }

        private const string AlignedFieldClass = "unity-base-field__aligned";
        private const string SheetFileName = "ManeEditor.uss";
        private const string FieldsSheetFileName = "ManeEditorFields.uss";

        private static StyleSheet _sheet;
        private static StyleSheet _fieldsSheet;

        /// <summary>
        /// True when the editor type or every inspected target has <see cref="ManeStyleAttribute"/>.
        /// </summary>
        public static bool HasManeStyle(UnityEditor.Editor editor)
        {
            if (editor == null || editor.targets == null || editor.targets.Length == 0)
                return false;

            if (editor.GetType().GetCustomAttribute<ManeStyleAttribute>(true) != null)
                return true;

            foreach (Object targetObject in editor.targets)
            {
                if (targetObject == null ||
                    targetObject.GetType().GetCustomAttribute<ManeStyleAttribute>(true) == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Applies <see cref="Options.Inspector"/> to <paramref name="root"/>.
        /// </summary>
        public static void Apply(VisualElement root) => Apply(root, Options.Inspector);

        /// <summary>
        /// Applies the selected styles to <paramref name="root"/>.
        /// </summary>
        public static void Apply(VisualElement root, Options options)
        {
            if (root == null)
                return;

            root.AddToClassList(RootClass);

            if ((options & Options.Sheet) != 0)
                AddSheet(root, Sheet, SheetFileName);

            if ((options & Options.FieldLayout) != 0)
            {
                root.AddToClassList(FieldsClass);
                AddSheet(root, FieldsSheet, FieldsSheetFileName);
                root.schedule.Execute(() => StyleListViewFooterButtons(root));
            }

            if ((options & Options.DisableInspectorAlignment) != 0)
            {
                DisableInspectorLabelAlignment(root);
                root.schedule.Execute(() => DisableInspectorLabelAlignment(root));
            }
        }

        /// <summary>
        /// Re-strips inspector alignment and restyles list footer buttons.
        /// </summary>
        public static void RefreshFieldLayout(VisualElement root)
        {
            if (root == null)
                return;

            DisableInspectorLabelAlignment(root);
            StyleListViewFooterButtons(root);
        }

        private static void AddSheet(VisualElement root, StyleSheet sheet, string fileName)
        {
            if (sheet == null)
            {
                Debug.LogError($"{fileName} was not found next to ManeEditorStyles.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(ManeEditorStyles), SheetFileName);

        private static StyleSheet FieldsSheet =>
            _fieldsSheet ??= UIElementsTools.LoadUSS(typeof(ManeEditorStyles), FieldsSheetFileName);

        private static void StyleListViewFooterButtons(VisualElement root)
        {
            root.Query<Button>(name: BaseListView.footerRemoveButtonName).ForEach(button =>
            {
                button.text = "–";
            });
        }

        private static void DisableInspectorLabelAlignment(VisualElement root)
        {
            root.Query(className: AlignedFieldClass).ForEach(element =>
            {
                element.RemoveFromClassList(AlignedFieldClass);
                element.Query<Label>(className: "unity-base-field__label").ForEach(ClearInlineLabelWidth);
                element.Query<Label>(className: "unity-property-field__label").ForEach(ClearInlineLabelWidth);
            });
        }

        private static void ClearInlineLabelWidth(Label label)
        {
            label.style.width = StyleKeyword.Null;
            label.style.minWidth = StyleKeyword.Null;
            label.style.maxWidth = StyleKeyword.Null;
        }
    }
}
