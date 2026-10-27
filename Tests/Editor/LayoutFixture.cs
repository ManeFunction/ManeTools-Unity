using UnityEngine;

namespace Mane.Unity.Editor.Tests
{
    // Inspected object for ManeInspectorLayoutTests: each layout attribute that starts a block, one that does not,
    // a FeatureToggle on a non-bool field, and a Header on a Foldout field.
    public sealed class LayoutFixture : ScriptableObject
    {
        public int first;

        [Header("Stats")] public int health;
        [NonBreakingSpace] public int armor;
        [Space] public int speed;

        [Foldout("Options")] public int volume;
        public int pitch;

        [FeatureToggle("Shield")] public bool shield;
        public float power;

        [FeatureToggle] public bool boost;
        // Counts only on a bool, so the field stays in the Boost block.
        [FeatureToggle("Ignored")] public int notBool;

        // Fill does not draw a header on a foldout field, so the block is not found by it.
        [Foldout("Extra"), Header("Inner")] public int combo;
        public int tail;
    }
}
