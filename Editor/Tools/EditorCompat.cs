using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_6000_3_OR_NEWER
using SelectionId = UnityEngine.EntityId;
using SceneId = UnityEngine.SceneManagement.SceneHandle;
#else
using SelectionId = System.Int32;
using SceneId = System.Int32;
#endif

namespace Mane.Unity.Editor
{
    /// <summary>
    /// Editor APIs that differ between Unity versions or are reachable only through reflection, kept in one place
    /// so a Unity update breaks this file instead of its callers.
    /// Object ids: 6.0–6.2 use int instance IDs; 6.3 adds <c>EntityId</c> to the selection, with int conversions only;
    /// 6.4 adds <c>GetEntityId</c>, <c>objectReferenceEntityIdValue</c> and the <c>ulong</c> conversions.
    /// </summary>
    internal static class EditorCompat
    {
        // PropertyField.label's setter calls Rebind(), which rebuilds the field
        private static readonly FieldInfo PropertyFieldLabel = typeof(PropertyField)
            .GetField("m_Label", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Key of an object that stays the same while the editor runs, also across undo and redo.
        /// </summary>
        internal static ulong GetObjectKey(Object obj) =>
#if UNITY_6000_4_OR_NEWER
            EntityId.ToULong(obj.GetEntityId());
#else
            unchecked((ulong)obj.GetInstanceID());
#endif

        /// <summary>
        /// True when an object reference is null but still holds an id: the object it points to is missing.
        /// </summary>
        internal static bool IsMissingReference(SerializedProperty property) =>
            property.objectReferenceValue == null &&
#if UNITY_6000_4_OR_NEWER
            property.objectReferenceEntityIdValue != EntityId.None;
#else
            property.objectReferenceInstanceIDValue != 0;
#endif

        /// <summary>
        /// Sets the label <paramref name="field"/> keeps for its next rebuild, without the rebuild that the public
        /// <see cref="PropertyField.label"/> setter starts. The label on screen is left to the caller.
        /// </summary>
        internal static void SetLabelWithoutRebind(PropertyField field, string label)
        {
            if (PropertyFieldLabel != null && !Equals(PropertyFieldLabel.GetValue(field), label))
                PropertyFieldLabel.SetValue(field, label);
        }

        /// <summary>
        /// Loaded scenes whose headers are selected in the Hierarchy, in selection order, without duplicates.
        /// A selected scene header shows up in the selection as an id that is not an object but a scene handle.
        /// </summary>
        internal static List<Scene> GetSelectedScenes()
        {
            List<Scene> scenes = new();
            foreach (SelectionId id in GetSelectedIds())
            {
                if (!TryGetScene(id, out Scene scene))
                    continue;

                if (!scenes.Contains(scene))
                    scenes.Add(scene);
            }

            return scenes;
        }

        private static bool TryGetScene(SelectionId id, out Scene scene)
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
        private static SelectionId[] GetSelectedIds() => Selection.entityIds;

        private static Object IdToObject(SelectionId id) => EditorUtility.EntityIdToObject(id);
#else
        private static SelectionId[] GetSelectedIds() => Selection.instanceIDs;

        private static Object IdToObject(SelectionId id) => EditorUtility.InstanceIDToObject(id);
#endif

#if UNITY_6000_4_OR_NEWER
        private static SceneId ToSceneId(SelectionId id) => SceneId.FromRawData(SelectionId.ToULong(id));
#elif UNITY_6000_3_OR_NEWER
        // 6.3 only has the backward-compatible int conversions: EntityId -> int -> SceneHandle.
        private static SceneId ToSceneId(SelectionId id) => (int)id;
#else
        private static SceneId ToSceneId(SelectionId id) => id;
#endif
    }
}
