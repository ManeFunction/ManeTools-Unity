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

        private static StyleSheet _sheet;

        /// <summary>
        /// Appends buttons for the inspected type to <paramref name="inspectorRoot"/>.
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

            List<(MethodInfo method, string label)> buttons = Collect(first.GetType());
            if (buttons.Count == 0)
                return;

            bool maneStyle = inspectorRoot.ClassListContains(ManeEditorStyles.RootClass);
            VisualElement container = new();
            container.AddToClassList(ContainerClass);
            if (maneStyle)
                AddSheet(inspectorRoot);
            else
                container.style.marginTop = 4;

            inspectorRoot.Add(container);

            UnityObject[] targets = editor.targets;
            SerializedObject serializedObject = editor.serializedObject;

            foreach ((MethodInfo method, string label) in buttons)
            {
                MethodInfo capturedMethod = method;
                string capturedLabel = label;
                Button button = new(() => Invoke(capturedMethod, capturedLabel, targets, serializedObject))
                {
                    text = label
                };
                if (maneStyle)
                    button.AddToClassList("mie-button");
                else
                    button.style.marginRight = -3;

                container.Add(button);
            }
        }

        private static void AddSheet(VisualElement root)
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("EditorButton.uss was not found next to EditorButton.");
                return;
            }

            if (!root.styleSheets.Contains(sheet))
                root.styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet =>
            _sheet ??= UIElementsTools.LoadUSS(typeof(EditorButton));

        private static List<(MethodInfo method, string label)> Collect(Type type)
        {
            List<(MethodInfo method, string label)> buttons = new();

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
                    if (attribute == null)
                        continue;

                    if (method.GetParameters().Length > 0)
                    {
                        Debug.LogWarning(
                            $"[EditorButton] {method.DeclaringType}.{method.Name} has parameters and will be skipped.");
                        continue;
                    }

                    string label = string.IsNullOrEmpty(attribute.Name) ? method.Name : attribute.Name;
                    buttons.Add((method, label));
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
    }
}
