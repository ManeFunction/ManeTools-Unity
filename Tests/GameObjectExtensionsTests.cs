using NUnit.Framework;
using UnityEngine;

namespace Mane.Unity.Tests
{
    public class GameObjectExtensionsTests
    {
        [Test]
        public void GetOrAddComponent_AddsOnceThenReuses()
        {
            GameObject go = new("GetOrAdd");
            try
            {
                Camera first = go.GetOrAddComponent<Camera>();
                Camera second = go.GetOrAddComponent<Camera>();

                Assert.AreSame(first, second);
                Assert.AreEqual(1, go.GetComponents<Camera>().Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Duplicate_CreatesSiblingAfterSource()
        {
            GameObject parent = new("Parent");
            GameObject source = new("Source");
            try
            {
                source.transform.SetParent(parent.transform);
                GameObject extra = new("Extra");
                extra.transform.SetParent(parent.transform);

                GameObject clone = source.Duplicate();

                Assert.AreEqual(parent.transform, clone.transform.parent);
                Assert.AreEqual(source.transform.GetSiblingIndex() + 1, clone.transform.GetSiblingIndex());
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [Test]
        public void IsPrefab_SceneObject_IsFalse()
        {
            GameObject go = new("SceneObject");
            try
            {
                Assert.IsFalse(go.IsPrefab());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
