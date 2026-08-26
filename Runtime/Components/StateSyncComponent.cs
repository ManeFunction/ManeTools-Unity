using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    [AddComponentMenu("Mane Tools/Components/State Sync Component")]
    public sealed class StateSyncComponent : MonoBehaviour
    {
        [SerializeField] private GameObject[] _bind;

        private void OnEnable() => _bind.ForEach(b => b.SetActive(true));

        private void OnDisable() => _bind.ForEach(b => b.SetActive(false));
    }
}