using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Fallback inspector for types that do not have a more specific editor.
    /// Mane block layout is used only when the type has <see cref="ManeStyleAttribute"/>.
    /// </summary>
    internal abstract class ManeInspectorWrapper : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            if (serializedObject.targetObject != null)
                root.userData = serializedObject.targetObject.GetType();

            if (UseManeStyle())
            {
                ManeEditorStyles.Apply(root);
                ManeInspectorLayout.Fill(root, serializedObject);
                root.TrackSerializedObjectValue(serializedObject, _ => ManeEditorStyles.RefreshFieldLayout(root));
            }
            else if (ManeInspectorLayout.HasFoldout(serializedObject))
                ManeInspectorLayout.FillDefault(root, serializedObject);
            else
                InspectorElement.FillDefaultInspector(root, serializedObject, this);

            EditorButton.AddTo(root, this);
            return root;
        }

        private bool UseManeStyle()
        {
            foreach (Object targetObject in targets)
            {
                if (targetObject == null ||
                    targetObject.GetType().GetCustomAttribute<ManeStyleAttribute>(true) == null)
                    return false;
            }

            return targets.Length > 0;
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    internal sealed class EditorButtonMonoBehaviourInspector : ManeInspectorWrapper { }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    internal sealed class EditorButtonScriptableObjectInspector : ManeInspectorWrapper { }
}
