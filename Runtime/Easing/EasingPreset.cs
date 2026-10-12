namespace Mane.Unity
{
    /// <summary>
    /// Names of the <see cref="Easing"/> presets, plus <see cref="Custom"/> for any other curve.
    /// <see cref="Easing.Get"/> returns the curve of a preset, <see cref="Easing.FindPreset"/> the preset of a curve.
    /// </summary>
    public enum EasingPreset
    {
        /// <summary><see cref="Easing.Linear"/>.</summary>
        Linear,

        /// <summary><see cref="Easing.QuadIn"/>.</summary>
        QuadIn,

        /// <summary><see cref="Easing.QuadOut"/>.</summary>
        QuadOut,

        /// <summary><see cref="Easing.QuadInOut"/>.</summary>
        QuadInOut,

        /// <summary><see cref="Easing.CubicIn"/>.</summary>
        CubicIn,

        /// <summary><see cref="Easing.CubicOut"/>.</summary>
        CubicOut,

        /// <summary><see cref="Easing.CubicInOut"/>.</summary>
        CubicInOut,

        /// <summary><see cref="Easing.QuartIn"/>.</summary>
        QuartIn,

        /// <summary><see cref="Easing.QuartOut"/>.</summary>
        QuartOut,

        /// <summary><see cref="Easing.QuartInOut"/>.</summary>
        QuartInOut,

        /// <summary><see cref="Easing.QuintIn"/>.</summary>
        QuintIn,

        /// <summary><see cref="Easing.QuintOut"/>.</summary>
        QuintOut,

        /// <summary><see cref="Easing.QuintInOut"/>.</summary>
        QuintInOut,

        /// <summary><see cref="Easing.SinIn"/>.</summary>
        SinIn,

        /// <summary><see cref="Easing.SinOut"/>.</summary>
        SinOut,

        /// <summary><see cref="Easing.SinInOut"/>.</summary>
        SinInOut,

        /// <summary><see cref="Easing.ExpoIn"/>.</summary>
        ExpoIn,

        /// <summary><see cref="Easing.ExpoOut"/>.</summary>
        ExpoOut,

        /// <summary><see cref="Easing.ExpoInOut"/>.</summary>
        ExpoInOut,

        /// <summary><see cref="Easing.CircIn"/>.</summary>
        CircIn,

        /// <summary><see cref="Easing.CircOut"/>.</summary>
        CircOut,

        /// <summary><see cref="Easing.CircInOut"/>.</summary>
        CircInOut,

        /// <summary><see cref="Easing.BackIn"/>.</summary>
        BackIn,

        /// <summary><see cref="Easing.BackOut"/>.</summary>
        BackOut,

        /// <summary><see cref="Easing.BackInOut"/>.</summary>
        BackInOut,

        /// <summary><see cref="Easing.BounceIn"/>.</summary>
        BounceIn,

        /// <summary><see cref="Easing.BounceOut"/>.</summary>
        BounceOut,

        /// <summary><see cref="Easing.BounceInOut"/>.</summary>
        BounceInOut,

        /// <summary><see cref="Easing.ElasticIn"/>.</summary>
        ElasticIn,

        /// <summary><see cref="Easing.ElasticOut"/>.</summary>
        ElasticOut,

        /// <summary><see cref="Easing.ElasticInOut"/>.</summary>
        ElasticInOut,

        /// <summary><see cref="Easing.Spring"/>.</summary>
        Spring,

        /// <summary>Not a preset: a curve with its own keys.</summary>
        Custom
    }
}
