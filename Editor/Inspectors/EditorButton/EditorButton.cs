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
    public static class EditorButton
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;

        public static void AddTo(VisualElement inspectorRoot, UnityEditor.Editor editor)
        {
            if (inspectorRoot == null || editor == null || editor.targets == null || editor.targets.Length == 0)
                return;

            UnityObject first = editor.targets[0];
            if (first == null)
                return;

            List<(MethodInfo method, string label)> buttons = Collect(first.GetType());
            if (buttons.Count == 0)
                return;

            VisualElement container = new VisualElement
            {
                style =
                {
                    marginTop = 4
                }
            };
            inspectorRoot.Add(container);

            UnityObject[] targets = editor.targets;
            SerializedObject serializedObject = editor.serializedObject;

            foreach ((MethodInfo method, string label) in buttons)
            {
                MethodInfo capturedMethod = method;
                Button button = new Button(() => Invoke(capturedMethod, targets, serializedObject))
                {
                    text = label,
                    style =
                    {
                        marginRight = -3
                    }
                };
                container.Add(button);
            }
        }

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

        private static void Invoke(MethodInfo method, UnityObject[] targets, SerializedObject serializedObject)
        {
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

                        method.Invoke(target, null);
                    }
                }
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception);
            }

            if (serializedObject != null && serializedObject.targetObject != null)
                serializedObject.Update();
        }
    }
}
