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
            VisualElement root = ManeEditorStyles.CreateInspectorRoot(this);
            if (root.ClassListContains(ManeEditorStyles.RootClass))
                ManeInspectorLayout.Fill(root, serializedObject);
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
