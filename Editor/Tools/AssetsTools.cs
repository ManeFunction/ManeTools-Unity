using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    public static class AssetsTools
    {
        [MenuItem("Assets/Force Reserialise Asset(s)", true)]
        private static bool ValidateSaveAssets() => Selection.objects.Length > 0;

        [MenuItem("Assets/Force Reserialise Asset(s)", false, 45)]
        private static void SaveAssets()
        {
            AssetDatabase.ForceReserializeAssets(Selection.objects
                .Select(AssetDatabase.GetAssetPath));

            foreach (Object obj in Selection.objects)
                if (obj) EditorUtility.SetDirty(obj);
        }
        
        [MenuItem("Assets/Force Reserialize All Assets", false, 46)]
        private static void ForceSaveAssets()
        {
            if (EditorUtility.DisplayDialog(
                "Force Assets Reserialization",
                "This may be long operation despite of the size of your project, so be aware! There is no progress bar so it may looks like that Unity is frozen, but be patient, it's working. Proceed operation?",
                "Yes, go ahead!", "Cancel!"))
            {
                AssetDatabase.ForceReserializeAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}