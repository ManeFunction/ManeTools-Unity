using System.Linq;
using UnityEngine;
using UnityEditor;


namespace Mane.Unity.Editor
{
    public static class PrefabsTools
    {
        [MenuItem("GameObject/Prefab/Apply Prefab(s) Changes", true, 1500)]
        private static bool ApplyPrefabsChecker() => IsPrefabSelected();

        [MenuItem("GameObject/Prefab/Apply Prefab(s) Changes", false, 1500)]
        private static void ApplyPrefabs(MenuCommand menuCommand)
        {
            GameObject obj = menuCommand.context as GameObject;
            if (!obj) return;
            
            ApplyChanges(obj);
        }

        [MenuItem("GameObject/Prefab/Apply Prefab(s) Transform Changes", true, 1501)]
        private static bool ApplyPrefabsTransformChecker() => IsPrefabSelected();
        
        [MenuItem("GameObject/Prefab/Apply Prefab(s) Transform Changes", false, 1501)]
        private static void ApplyPrefabsTransform(MenuCommand menuCommand)
        {
            GameObject obj = menuCommand.context as GameObject;
            if (!obj) return;
            
            ApplyTransformChanges(obj);
        }

        [MenuItem("GameObject/Prefab/Apply Prefab(s) Changes (+Transform)", true, 1502)]
        private static bool ApplyPrefabsAllChecker() => IsPrefabSelected();
        
        [MenuItem("GameObject/Prefab/Apply Prefab(s) Changes (+Transform)", false, 1502)]
        private static void ApplyPrefabsAll(MenuCommand menuCommand)
        {
            GameObject obj = menuCommand.context as GameObject;
            if (!obj) return;
            
            ApplyChanges(obj);
            ApplyTransformChanges(obj);
        }
        
        private static bool IsPrefabSelected() => 
            Selection.objects.Any(o => o != null && PrefabUtility.IsPartOfPrefabInstance(o));
        

        private static void ApplyChanges(GameObject gameObject)
        {
            if (!gameObject) return;
            
            PrefabUtility.ApplyPrefabInstance(gameObject, InteractionMode.UserAction);
        }

        private static void ApplyTransformChanges(GameObject gameObject)
        {
            if (!gameObject) return;
            
            SerializedObject so = new SerializedObject(gameObject.transform);
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            
            ApplyPropertyOverride(so, path, "m_LocalRotation");
            ApplyPropertyOverride(so, path, "m_LocalPosition");

            if (gameObject.transform is not RectTransform)
                return;

            ApplyPropertyOverride(so, path, "m_AnchorMin");
            ApplyPropertyOverride(so, path, "m_AnchorMax");
            ApplyPropertyOverride(so, path, "m_AnchoredPosition");
            ApplyPropertyOverride(so, path, "m_SizeDelta");
            ApplyPropertyOverride(so, path, "m_Pivot");
        }

        private static void ApplyPropertyOverride(SerializedObject so, string prefabPath, string propertyName)
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null)
                return;

            PrefabUtility.ApplyPropertyOverride(property, prefabPath, InteractionMode.UserAction);
        }
    }
}