using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Mane.Unity.Editor.Tests
{
    // Inspected object for MemberLookupTests: serialized values of several kinds, a nested class, a list of it,
    // and members found only by reflection.
    public sealed class LookupFixture : LookupFixtureBase
    {
        public enum Mode
        {
            First,
            Second
        }

        public bool flag;
        public int number;
        public string text;
        public UnityObject reference;
        public Mode mode;
        [Tooltip("tip")] public float tooltipped;
        public Nested nested = new();
        public List<Nested> items = new();

        private bool FlagProperty => flag;
        private bool SameName => true;
        protected override bool Shadowed => true;
        private string[] InstanceOptions => new[] { "component" };

        private bool ComponentMethod() => true;

        [Serializable]
        public sealed class Nested
        {
            public float value;
            // Same name as the top-level field: the sibling wins.
            public bool flag;
            [DropdownList(typeof(Nested), nameof(FieldOptions))] public string choice;

            [NonSerialized] public string[] FieldOptions = { "a", "b" };

            private bool SameName => false;
        }
    }

    // Static options for DropdownList's source type.
    public static class LookupOptionSource
    {
        private static string[] Values => new[] { "static" };
    }
}
