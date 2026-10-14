using UnityEngine;

namespace Mane.Unity.Editor.Tests
{
    // Base of LookupFixture: an inherited private field, an inherited private member, and one the derived type overrides.
    public abstract class LookupFixtureBase : ScriptableObject
    {
        [SerializeField] private float _baseValue;

        private bool BaseOnly => true;

        protected virtual bool Shadowed => false;
    }
}
