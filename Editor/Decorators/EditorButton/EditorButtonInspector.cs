using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    internal abstract class EditorButtonInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            EditorButton.AddTo(root, this);
            return root;
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    internal sealed class EditorButtonMonoBehaviourInspector : EditorButtonInspector { }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    internal sealed class EditorButtonScriptableObjectInspector : EditorButtonInspector { }
}
