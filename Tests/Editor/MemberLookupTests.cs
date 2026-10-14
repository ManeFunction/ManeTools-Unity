using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor.Tests
{
    public class MemberLookupTests
    {
        private LookupFixture _fixture;
        private LookupFixture _other;

        [SetUp]
        public void SetUp()
        {
            _fixture = Create();
            _other = Create();
        }

        [TearDown]
        public void TearDown()
        {
            UnityObject.DestroyImmediate(_fixture);
            UnityObject.DestroyImmediate(_other);
        }

        [Test]
        public void GetDeclaringObject_FollowsNestedClassesAndListElements()
        {
            Assert.AreSame(_fixture, MemberLookup.GetDeclaringObject(_fixture, "flag"));
            Assert.AreSame(_fixture.nested, MemberLookup.GetDeclaringObject(_fixture, "nested.value"));
            Assert.AreSame(_fixture.items[1], MemberLookup.GetDeclaringObject(_fixture, "items.Array.data[1].value"));
            Assert.AreSame(_fixture, MemberLookup.GetDeclaringObject(_fixture, "items.Array.data[0]"));
            Assert.AreSame(_fixture, MemberLookup.GetDeclaringObject(_fixture, "items.Array.size"));
            Assert.IsNull(MemberLookup.GetDeclaringObject(_fixture, "items.Array.data[5].value"));
        }

        [Test]
        public void GetField_FindsTheBackingField()
        {
            AssertField("nested.value", "value", typeof(LookupFixture.Nested));
            AssertField("items.Array.data[1].value", "value", typeof(LookupFixture.Nested));
            AssertField("items.Array.data[1]", "items", typeof(LookupFixture));
            AssertField("_baseValue", "_baseValue", typeof(LookupFixtureBase));
        }

        [Test]
        public void GetAttribute_ReadsTheBackingField()
        {
            Assert.AreEqual("tip", Find(_fixture, "tooltipped").GetAttribute<TooltipAttribute>().tooltip);
            Assert.IsNotNull(Find(_fixture, "items.Array.data[0].choice").GetAttribute<DropdownListAttribute>());
            Assert.IsNull(Find(_fixture, "flag").GetAttribute<TooltipAttribute>());
        }

        [Test]
        public void TryGetValue_DeclaringObjectBeforeInspectedObject()
        {
            Assert.IsTrue(MemberLookup.TryGetValue(Find(_fixture, "nested.value"), _fixture, "SameName", out bool nested));
            Assert.IsFalse(nested);

            Assert.IsTrue(MemberLookup.TryGetValue(Find(_fixture, "flag"), _fixture, "SameName", out bool component));
            Assert.IsTrue(component);
        }

        [Test]
        public void TryGetValue_FallsBackToInspectedObject()
        {
            Assert.IsTrue(MemberLookup.TryGetValue(Find(_fixture, "nested.value"), _fixture, "ComponentMethod", out bool value));
            Assert.IsTrue(value);
        }

        [Test]
        public void TryGetValue_DerivedBeforeBase_AndInheritedPrivate()
        {
            SerializedProperty property = Find(_fixture, "flag");

            Assert.IsTrue(MemberLookup.TryGetValue(property, _fixture, "Shadowed", out bool shadowed));
            Assert.IsTrue(shadowed);

            Assert.IsTrue(MemberLookup.TryGetValue(property, _fixture, "BaseOnly", out bool baseOnly));
            Assert.IsTrue(baseOnly);
        }

        [Test]
        public void TryGetValue_OtherValueType_IsNotFound()
        {
            Assert.IsFalse(MemberLookup.TryGetValue(Find(_fixture, "flag"), _fixture, "number", out bool _));
            Assert.IsFalse(MemberLookup.TryGetValue(Find(_fixture, "flag"), _fixture, "Nothing", out bool _));
        }

        [Test]
        public void TryGetValue_SourceType()
        {
            SerializedProperty choice = Find(_fixture, "items.Array.data[0].choice");

            // An instance field on the nested class that holds the dropdown.
            Assert.IsTrue(MemberLookup.TryGetValue(choice, _fixture, "FieldOptions", out string[] nested,
                typeof(LookupFixture.Nested)));
            CollectionAssert.AreEqual(new[] { "a", "b" }, nested);

            // The holder is not of the source type: the inspected object is.
            Assert.IsTrue(MemberLookup.TryGetValue(choice, _fixture, "InstanceOptions", out string[] component,
                typeof(LookupFixture)));
            CollectionAssert.AreEqual(new[] { "component" }, component);

            // Neither is: static members of the source type only.
            Assert.IsTrue(MemberLookup.TryGetValue(choice, _fixture, "Values", out string[] statics,
                typeof(LookupOptionSource)));
            CollectionAssert.AreEqual(new[] { "static" }, statics);
            Assert.IsFalse(MemberLookup.TryGetValue(choice, _fixture, "InstanceOptions", out string[] _,
                typeof(LookupOptionSource)));
        }

        [TestCase("flag")]
        [TestCase("number")]
        [TestCase("text")]
        [TestCase("reference")]
        [TestCase("mode")]
        public void EvaluateCondition_SerializedField_TrueWhenNotDefault(string member)
        {
            AssertCondition(Evaluate(_fixture, "tooltipped", member), all: false, any: false);

            _fixture.flag = true;
            _fixture.number = 3;
            _fixture.text = "x";
            _fixture.reference = _other;
            _fixture.mode = LookupFixture.Mode.Second;
            AssertCondition(Evaluate(_fixture, "tooltipped", member), all: true, any: true);
        }

        [Test]
        public void EvaluateCondition_SiblingFieldBeforeTopLevel()
        {
            _fixture.flag = true;
            AssertCondition(Evaluate(_fixture, "nested.value", "flag"), all: false, any: false);

            _fixture.flag = false;
            _fixture.nested.flag = true;
            AssertCondition(Evaluate(_fixture, "nested.value", "flag"), all: true, any: true);
        }

        [Test]
        public void EvaluateCondition_ReflectionMember()
        {
            AssertCondition(Evaluate(_fixture, "number", "FlagProperty"), all: false, any: false);

            _fixture.flag = true;
            AssertCondition(Evaluate(_fixture, "number", "FlagProperty"), all: true, any: true);
        }

        [TestCase("flag")]
        [TestCase("FlagProperty")]
        public void EvaluateCondition_SeveralObjects(string member)
        {
            _fixture.flag = true;
            AssertCondition(EvaluateBoth("number", member), all: false, any: true);

            _other.flag = true;
            AssertCondition(EvaluateBoth("number", member), all: true, any: true);
        }

        [Test]
        public void EvaluateCondition_SeveralObjects_DifferentButSetValues()
        {
            _fixture.number = 1;
            _other.number = 2;
            AssertCondition(EvaluateBoth("flag", "number"), all: true, any: true);
        }

        [Test]
        public void EvaluateCondition_Missing_LogsOncePerAttribute()
        {
            // Reports are kept until a domain reload, so each run uses a new name.
            string member = "Missing" + System.Guid.NewGuid().ToString("N");
            SerializedProperty property = Find(_fixture, "flag");

            LogAssert.Expect(LogType.Error, new Regex($@"\[AvailableIf\] '{member}' was not found on LookupFixture"));
            Assert.IsFalse(MemberLookup.EvaluateCondition(property, member, "AvailableIf").Found);
            Assert.IsFalse(MemberLookup.EvaluateCondition(property, member, "AvailableIf").Found);

            LogAssert.Expect(LogType.Error, new Regex($@"\[InfoBox\] '{member}'"));
            Assert.IsFalse(MemberLookup.EvaluateCondition(property, member, "InfoBox").Found);
            LogAssert.NoUnexpectedReceived();
        }

        private static LookupFixture Create()
        {
            LookupFixture fixture = ScriptableObject.CreateInstance<LookupFixture>();
            fixture.items.Add(new LookupFixture.Nested { value = 1f });
            fixture.items.Add(new LookupFixture.Nested { value = 2f });
            return fixture;
        }

        private static SerializedProperty Find(UnityObject target, string path) =>
            new SerializedObject(target).FindProperty(path);

        private static MemberLookup.Condition Evaluate(UnityObject target, string path, string member) =>
            MemberLookup.EvaluateCondition(Find(target, path), member, "Test");

        private MemberLookup.Condition EvaluateBoth(string path, string member) =>
            MemberLookup.EvaluateCondition(new SerializedObject(new UnityObject[] { _fixture, _other }).FindProperty(path),
                member, "Test");

        private void AssertField(string path, string name, System.Type declaringType)
        {
            FieldInfo field = MemberLookup.GetField(Find(_fixture, path));
            Assert.IsNotNull(field, path);
            Assert.AreEqual(name, field.Name, path);
            Assert.AreEqual(declaringType, field.DeclaringType, path);
        }

        private static void AssertCondition(MemberLookup.Condition condition, bool all, bool any)
        {
            Assert.IsTrue(condition.Found);
            Assert.AreEqual(all, condition.All, "All");
            Assert.AreEqual(any, condition.Any, "Any");
        }
    }
}
