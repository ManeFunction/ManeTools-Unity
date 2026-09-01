using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    internal sealed class InfoBoxDrawer : DecoratorDrawer
    {
        [SerializeField] private VisualTreeAsset xml;

        private static VisualTreeAsset _assignedXml;

        public override VisualElement CreatePropertyGUI()
        {
            InfoBoxAttribute info = (InfoBoxAttribute)attribute;
            VisualTreeAsset tree = xml != null ? xml : AssignedXml;
            if (tree == null)
            {
                Debug.LogError("InfoBox UXML is not assigned.");
                return new Label(info.Message);
            }

            TemplateContainer root = tree.CloneTree();
            VisualElement box = root.Q<VisualElement>("infoBox");
            box.Q<Label>("label").text = info.Message;
            box.AddToClassList(TypeClass(info.Type));
            return root;
        }

        public override float GetHeight()
        {
            InfoBoxAttribute info = (InfoBoxAttribute)attribute;
            float width = EditorGUIUtility.currentViewWidth;
            if (width < 1f)
                width = 200f;

            return Mathf.Max(
                EditorGUIUtility.singleLineHeight * 2f,
                EditorStyles.helpBox.CalcHeight(new GUIContent(info.Message), width));
        }

        private static VisualTreeAsset AssignedXml
        {
            get
            {
                if (_assignedXml != null)
                    return _assignedXml;

                string[] guids = AssetDatabase.FindAssets("t:MonoScript InfoBoxDrawer");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script == null || script.GetClass() != typeof(InfoBoxDrawer))
                        continue;

                    MonoImporter importer = AssetImporter.GetAtPath(path) as MonoImporter;
                    _assignedXml = importer?.GetDefaultReference(nameof(xml)) as VisualTreeAsset;
                    break;
                }

                return _assignedXml;
            }
        }

        private static string TypeClass(InfoBoxType type) => type switch
        {
            InfoBoxType.Warning => "warning",
            InfoBoxType.Error => "error",
            InfoBoxType.None => "none",
            _ => "info"
        };
    }
}
