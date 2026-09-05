using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Shared Mane editor USS and inspector field layout.
    /// Call <see cref="Apply(VisualElement)"/> from CreateInspectorGUI after cloning UXML.
    /// </summary>
    public static class ManeEditorStyles
    {
        public const string RootClass = "mie-root";
        public const string FieldsClass = "mie-fields";
        public const string WideLabelsClass = "mie-labels-150";

        [Flags]
        public enum Options
        {
            None = 0,
            Sheet = 1,
            FieldLayout = 2,
            DisableInspectorAlignment = 4,
            Inspector = Sheet | FieldLayout | DisableInspectorAlignment
        }

        private const string AlignedFieldClass = "unity-base-field__aligned";
        private const string SheetFileName = "ManeEditor.uss";

        private static StyleSheet _sheet;

        public static void Apply(VisualElement root) => Apply(root, Options.Inspector);

        public static void Apply(VisualElement root, Options options)
        {
            if (root == null)
                return;

            root.AddToClassList(RootClass);

            if ((options & Options.Sheet) != 0)
                AddSheet(root);

            if ((options & Options.FieldLayout) != 0)
                root.AddToClassList(FieldsClass);

            if ((options & Options.DisableInspectorAlignment) != 0)
            {
                DisableInspectorLabelAlignment(root);
                root.schedule.Execute(() => DisableInspectorLabelAlignment(root));
            }
        }

        public static void UseWideLabels(VisualElement root)
        {
            root?.AddToClassList(WideLabelsClass);
        }

        public static void RefreshFieldLayout(VisualElement root)
        {
            if (root == null)
                return;

            DisableInspectorLabelAlignment(root);
        }

        private static void AddSheet(VisualElement root)
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("ManeEditor.uss was not found next to ManeEditorStyles.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet
        {
            get
            {
                if (_sheet != null)
                    return _sheet;

                string[] guids = AssetDatabase.FindAssets($"t:MonoScript {nameof(ManeEditorStyles)}");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script == null || script.GetClass() != typeof(ManeEditorStyles))
                        continue;

                    string folder = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(folder))
                        break;

                    _sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                        Path.Combine(folder, SheetFileName).Replace('\\', '/'));
                    break;
                }

                return _sheet;
            }
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
