using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;
using Button = UnityEngine.UIElements.Button;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Adds inspector buttons for methods marked with <see cref="EditorButtonAttribute"/>.
    /// Changes the method makes to the inspected objects are recorded for Undo, mark them dirty,
    /// and are kept as prefab overrides. Changes to other objects are up to the method.
    /// </summary>
    public static class EditorButton
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        private const string ContainerClass = "mie-editor-buttons";

        private static readonly HashSet<string> MissingBlockWarnings = new();

        /// <summary>
        /// Appends buttons for the inspected type to <paramref name="inspectorRoot"/>.
        /// A button with <see cref="EditorButtonAttribute.Block"/> goes after the last field of that block instead.
        /// Does nothing if the buttons were already added to that root.
        /// </summary>
        public static void AddTo(VisualElement inspectorRoot, UnityEditor.Editor editor)
        {
            if (inspectorRoot == null || editor == null || editor.targets == null || editor.targets.Length == 0)
                return;

            // ManeEditor adds them itself; an editor that also calls AddTo must not get a second row.
            if (inspectorRoot.Q(className: ContainerClass) != null)
                return;

            UnityObject first = editor.targets[0];
            if (first == null)
                return;

            List<ButtonInfo> buttons = Collect(first.GetType());
            if (buttons.Count == 0)
                return;

            bool maneStyle = inspectorRoot.ClassListContains(ManeEditorStyles.RootClass);
            if (maneStyle)
                UIElementsTools.AddStyleSheet(inspectorRoot, typeof(EditorButton));

            UnityObject[] targets = editor.targets;
            SerializedObject serializedObject = editor.serializedObject;

            // One row per block, placed after its last field, and one more after the content of a foldout for buttons
            // that stay visible when it is collapsed. Buttons without a found block share the end row.
            Dictionary<VisualElement, VisualElement> blockRows = new();
            VisualElement endRow = null;

            foreach (ButtonInfo info in buttons)
            {
                VisualElement anchor = FindAnchor(inspectorRoot, serializedObject, info);
                VisualElement row;
                if (anchor == null)
                {
                    if (endRow == null)
                    {
                        endRow = CreateRow(maneStyle);
                        inspectorRoot.Add(endRow);
                    }

                    row = endRow;
                }
                else
                {
                    // Inside a foldout, the hierarchy parent of a field is the content element, which is hidden
                    // on collapse. anchor.parent is the logical parent, the Foldout itself.
                    Foldout foldout = info.HideWithFoldout ? null : anchor.GetFirstAncestorOfType<Foldout>();
                    VisualElement after = foldout != null ? foldout.contentContainer : anchor;
                    if (!blockRows.TryGetValue(after, out row))
                    {
                        row = CreateRow(maneStyle);
                        // Line up with the stock foldout content (15px indent); FoldoutBlock content has none.
                        if (foldout != null && !maneStyle)
                            row.style.marginLeft = 15;

                        VisualElement parent = after.hierarchy.parent;
                        parent.hierarchy.Insert(parent.hierarchy.IndexOf(after) + 1, row);
                        blockRows[after] = row;
                    }
                }

                MethodInfo method = info.Method;
                string label = info.Label;
                Button button = new(() => Invoke(method, label, targets, serializedObject))
                {
                    text = label
                };
                if (maneStyle)
                    button.AddToClassList("mie-button");
                else
                    button.style.marginRight = -3;

                row.Add(button);
            }
        }

        private static VisualElement CreateRow(bool maneStyle)
        {
            VisualElement row = new();
            row.AddToClassList(ContainerClass);
            if (!maneStyle)
                row.style.marginTop = 4;
            return row;
        }

        private static VisualElement FindAnchor(VisualElement root, SerializedObject serializedObject, ButtonInfo info)
        {
            if (string.IsNullOrEmpty(info.Block))
                return null;

            string path = ManeInspectorLayout.FindBlockEnd(serializedObject, info.Block);
            VisualElement anchor = path != null ? root.Q(ManeInspectorLayout.FieldName(path)) : null;
            if (anchor?.hierarchy.parent != null)
                return anchor;

            if (MissingBlockWarnings.Add(info.Method.DeclaringType + "." + info.Method.Name))
                Debug.LogWarning($"[EditorButton] {info.Method.DeclaringType}.{info.Method.Name}: block " +
                                 $"'{info.Block}' was not found in the inspector, the button is drawn at the end.");
            return null;
        }

        private static List<ButtonInfo> Collect(Type type)
        {
            List<ButtonInfo> buttons = new();
            // An override inherits the attribute, so the walk would meet it again at the base declaration.
            // Keyed by the base definition, the most derived declaration is the one that gets the button.
            HashSet<RuntimeMethodHandle> added = new();

            for (Type current = type;
                 current != null && current != typeof(MonoBehaviour) && current != typeof(ScriptableObject) &&
                 current != typeof(object);
                 current = current.BaseType)
            {
                MethodInfo[] methods = current.GetMethods(MethodFlags);
                Array.Sort(methods, (a, b) => a.MetadataToken.CompareTo(b.MetadataToken));

                foreach (MethodInfo method in methods)
                {
                    EditorButtonAttribute attribute = method.GetCustomAttribute<EditorButtonAttribute>(true);
                    if (attribute == null || !added.Add(method.GetBaseDefinition().MethodHandle))
                        continue;

                    if (method.GetParameters().Length > 0)
                    {
                        Debug.LogWarning(
                            $"[EditorButton] {method.DeclaringType}.{method.Name} has parameters and will be skipped.");
                        continue;
                    }

                    string label = string.IsNullOrEmpty(attribute.Name) ? method.Name : attribute.Name;
                    buttons.Add(new ButtonInfo(method, label, attribute.Block, attribute.HideWithFoldout));
                }
            }

            return buttons;
        }

        private static void Invoke(MethodInfo method, string label, UnityObject[] targets, SerializedObject serializedObject)
        {
            List<UnityObject> invoked = new();
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    foreach (UnityObject target in targets)
                    {
                        if (target == null || (method.DeclaringType != null && !method.DeclaringType.IsInstanceOfType(target)))
                            continue;

                        invoked.Add(target);
                    }

                    // A field changed directly from C# is invisible to the editor. Undo compares this snapshot
                    // after the call, and only when something changed records a step and marks the object dirty.
                    Undo.RecordObjects(invoked.ToArray(), label);
                    foreach (UnityObject target in invoked)
                        method.Invoke(target, null);
                }
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception);
            }

            foreach (UnityObject target in invoked)
                if (target != null)
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);

            if (serializedObject != null && serializedObject.targetObject != null)
                serializedObject.Update();
        }

        private readonly struct ButtonInfo
        {
            public readonly MethodInfo Method;
            public readonly string Label;
            public readonly string Block;
            public readonly bool HideWithFoldout;

            public ButtonInfo(MethodInfo method, string label, string block, bool hideWithFoldout)
            {
                Method = method;
                Label = label;
                Block = block;
                HideWithFoldout = hideWithFoldout;
            }
        }
    }
}
