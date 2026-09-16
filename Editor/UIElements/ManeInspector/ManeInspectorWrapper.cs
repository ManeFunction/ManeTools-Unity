using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Fallback inspector for types that do not have a more specific editor.
    /// Mane block layout is used when the inspected type or the editor has <see cref="ManeStyleAttribute"/>.
    /// </summary>
    internal abstract class ManeInspectorWrapper : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            if (serializedObject.targetObject != null)
                root.userData = serializedObject.targetObject.GetType();

            if (ManeEditorStyles.HasManeStyle(this))
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
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    internal sealed class EditorButtonMonoBehaviourInspector : ManeInspectorWrapper { }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    internal sealed class EditorButtonScriptableObjectInspector : ManeInspectorWrapper { }
}
