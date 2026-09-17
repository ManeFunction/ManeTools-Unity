using Mane.DotNet;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Mirrors this object's enabled state onto bound GameObjects.
    /// </summary>
    [ManeStyle]
    [AddComponentMenu("Mane Tools/Components/State Sync Component")]
    public sealed class StateSyncComponent : MonoBehaviour
    {
        [SerializeField] private GameObject[] _bind;

        private void OnEnable() => _bind?.ForEach(b => { if (b) b.SetActive(true); });

        private void OnDisable() => _bind?.ForEach(b => { if (b) b.SetActive(false); });
    }
}
