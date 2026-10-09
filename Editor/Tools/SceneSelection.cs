using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
#if UNITY_6000_3_OR_NEWER
using ObjectId = UnityEngine.EntityId;
using SceneId = UnityEngine.SceneManagement.SceneHandle;
#else
using ObjectId = System.Int32;
using SceneId = System.Int32;
#endif

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Scenes selected in the Hierarchy window. A selected scene header shows up in the
    /// selection as an id that is not an object but a scene handle; its type differs per Unity version.
    /// </summary>
    internal static class SceneSelection
    {
        /// <summary>
        /// Loaded scenes whose headers are selected in the Hierarchy, in selection order, without duplicates.
        /// </summary>
        internal static List<Scene> GetSelectedScenes()
        {
            List<Scene> scenes = new();
            foreach (ObjectId id in GetSelectedIds())
            {
                if (!TryGetScene(id, out Scene scene))
                    continue;

                if (!scenes.Contains(scene))
                    scenes.Add(scene);
            }

            return scenes;
        }

        private static bool TryGetScene(ObjectId id, out Scene scene)
        {
            scene = default;
            if (IdToObject(id) != null)
                return false;

            SceneId handle = ToSceneId(id);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene candidate = SceneManager.GetSceneAt(i);
                if (candidate.handle != handle)
                    continue;

                scene = candidate;
                return scene.IsValid();
            }

            return false;
        }

#if UNITY_6000_3_OR_NEWER
        private static ObjectId[] GetSelectedIds() => Selection.entityIds;

        private static UnityEngine.Object IdToObject(ObjectId id) => EditorUtility.EntityIdToObject(id);
#else
        private static ObjectId[] GetSelectedIds() => Selection.instanceIDs;

        private static UnityEngine.Object IdToObject(ObjectId id) => EditorUtility.InstanceIDToObject(id);
#endif

#if UNITY_6000_4_OR_NEWER
        private static SceneId ToSceneId(ObjectId id) => SceneId.FromRawData(ObjectId.ToULong(id));
#elif UNITY_6000_3_OR_NEWER
        // 6.3 only has the backward-compatible int conversions: EntityId -> int -> SceneHandle.
        private static SceneId ToSceneId(ObjectId id) => (int)id;
#else
        private static SceneId ToSceneId(ObjectId id) => id;
#endif
    }
}
