using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Tests
{
    public class StateSyncComponentTests
    {
        [Test]
        public void EnableDisable_NullBindList_DoesNotThrow()
        {
            GameObject go = new("StateSync");
            try
            {
                Assert.DoesNotThrow(() => go.AddComponent<StateSyncComponent>());
                Assert.DoesNotThrow(() => go.SetActive(false));
                Assert.DoesNotThrow(() => go.SetActive(true));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EnableDisable_NullBindElement_DoesNotThrow()
        {
            GameObject go = new("StateSync");
            try
            {
                StateSyncComponent sync = go.AddComponent<StateSyncComponent>();
                SerializedObject so = new(sync);
                SerializedProperty bind = so.FindProperty("_bind");
                bind.arraySize = 1;
                bind.GetArrayElementAtIndex(0).objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();

                Assert.DoesNotThrow(() => go.SetActive(false));
                Assert.DoesNotThrow(() => go.SetActive(true));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
