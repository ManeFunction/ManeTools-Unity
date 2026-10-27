using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor.Tests
{
    public class ManeInspectorLayoutTests
    {
        private LayoutFixture _fixture;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _fixture = ScriptableObject.CreateInstance<LayoutFixture>();
            _serializedObject = new SerializedObject(_fixture);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            UnityObject.DestroyImmediate(_fixture);
        }

        [Test]
        public void FindBlockEnd_EndsAtTheNextBlockStart()
        {
            Assert.AreEqual("armor", ManeInspectorLayout.FindBlockEnd(_serializedObject, "Stats"));
            Assert.AreEqual("pitch", ManeInspectorLayout.FindBlockEnd(_serializedObject, "Options"));
            Assert.AreEqual("power", ManeInspectorLayout.FindBlockEnd(_serializedObject, "Shield"));
        }

        [Test]
        public void FindBlockEnd_LastBlockEndsAtTheLastField()
        {
            Assert.AreEqual("tail", ManeInspectorLayout.FindBlockEnd(_serializedObject, "Extra"));
        }

        [Test]
        public void FindBlockEnd_FeatureToggleCountsOnlyOnABool()
        {
            Assert.AreEqual("notBool", ManeInspectorLayout.FindBlockEnd(_serializedObject, "Boost"));
            Assert.IsNull(ManeInspectorLayout.FindBlockEnd(_serializedObject, "Ignored"));
        }

        [Test]
        public void FindBlockEnd_IgnoresHeadersThatFillDoesNotDraw()
        {
            Assert.IsNull(ManeInspectorLayout.FindBlockEnd(_serializedObject, "Inner"));
        }

        [Test]
        public void FindBlockEnd_UnknownOrEmptyBlockIsNull()
        {
            Assert.IsNull(ManeInspectorLayout.FindBlockEnd(_serializedObject, "Missing"));
            Assert.IsNull(ManeInspectorLayout.FindBlockEnd(_serializedObject, null));
            Assert.IsNull(ManeInspectorLayout.FindBlockEnd(null, "Stats"));
        }

        [Test]
        public void HasFoldout_FindsAFoldoutField()
        {
            Assert.IsTrue(ManeInspectorLayout.HasFoldout(_serializedObject));

            LookupFixture plain = ScriptableObject.CreateInstance<LookupFixture>();
            using SerializedObject plainObject = new(plain);
            Assert.IsFalse(ManeInspectorLayout.HasFoldout(plainObject));
            UnityObject.DestroyImmediate(plain);
        }
    }
}
