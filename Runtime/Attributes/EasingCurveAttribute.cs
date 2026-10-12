using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Use with <see cref="SerializeField"/> on <see cref="AnimationCurve"/> fields. The inspector shows a dropdown
    /// of the <see cref="Easing"/> presets; picking one copies its keys into the field. <see cref="EasingPreset.Custom"/>
    /// shows the curve field to edit the current keys, starting from a linear curve when the field is empty. The field stays a plain <see cref="AnimationCurve"/> at runtime.
    /// </summary>
    public sealed class EasingCurveAttribute : PropertyAttribute
    {
    }
}
