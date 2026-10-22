# Mane Tools for Unity

Editor enhancements and class extensions for Unity.

The package is supported starting from **Unity 6.0.81** (6000.0.81f1) and depends on [ManeTools-dotNET](https://github.com/ManeFunction/ManeTools-dotNet.git).

Add components from **Add Component → Mane Tools**.

## Features

- A few ways to **serialize interfaces**: `SerializeInterface` for data structures and `InterfaceOnly` for `MonoBehaviours`.
- `SerializableDateTime` and `SerializableDateTimeRange` with a calendar popup in the inspector.
- `Screenshot.Capture()`, Edit-menu shortcuts to take a game screenshot (F10) and clear the Console (F8), and File-menu shortcuts to unload, remove, and reopen scenes.
- Missing-reference and asset-reference finders as Assets / GameObject context menus.
- `UnitySingleton` and `ScriptableSingleton` implementations (in addition to generic `ManeSingleton` from [ManeTools for .Net](https://github.com/ManeFunction/ManeTools-dotNet.git)).
- Easing presets (`Easing.SinIn`, `Easing.ExpoOut`, and the rest of the standard set) as shared `AnimationCurve`s, and `[EasingCurve]` to pick one in the inspector.
- **ManeStyle** inspector layout: framed blocks, `Space` / `Header` splits, `NonBreakingSpace` for spacing inside a block, `Foldout` groups, and `FeatureToggle`, a header with a checkbox that hides its block while off. Put `[ManeStyle]` on a `MonoBehaviour`, `ScriptableObject`, or a custom inspector, or turn on **ManeStyle for all** in the Scene View overlay to use it everywhere.
- Inspector attributes: `InfoBox`, `ReadOnly`, `Label`, `Prefix` / `Postfix`, `DropdownList`, `LimitedField`, `EasingCurve`, `ItemNameFromField` / `ItemNameFromString`, `AvailableIf`, and others to simplify work with Unity components.
- `EditorButton`s to call component methods from the inspector.
- `Color picker` and `Color` / `Color32` helpers (HSL, luma, hex, channel setters).
- Scene View overlay for editor tools, including children-transform freeze and ManeStyle for all inspectors.
- Extensions for `GameObject`, `Transform`, `RectTransform`, `Rect`, `Vector2` / `Vector3`, `Scene`, and coroutines.
- Custom yield instructions (`WaitForSecondsUntil` / `While`, realtime variants).
- Unity-backed random helpers: `UnityRandom`, `RandomColor`, `RandomPoint`, `RandomDirection` (in addition to system-based `ManeRandom` from [ManeTools for .Net](https://github.com/ManeFunction/ManeTools-dotNet.git)).
- `ParticleSystemUIScaler` to fit a particle emitter size and particle count to a `RectTransform` area with consistent emission intensity.
- Animator helpers, such as state randomizers.
- Script creation menu for `MonoBehaviour` starting point without `Start()` and `Update()` placeholders.
- NUnit coverage for date/time types, color helpers, and some extensions.

For detailed info and usage examples of everything in this package, welcome to [projects Wiki](https://github.com/ManeFunction/ManeTools-Unity/wiki)!

## Showcase

![Mane Tools inspector attributes in a ManeStyle inspector](https://raw.githubusercontent.com/wiki/ManeFunction/ManeTools-Unity/Images/showcase.png)

<details>
<summary>Show the code behind this inspector</summary>

```csharp
using System;
using Mane.DotNet;
using Mane.Unity;
using Mane.Unity.UI;
using UnityEngine;

[ManeStyle]
public class TestComponentBase : MonoBehaviour
{
    [Header("Stats")]
    [Label("Max Health"), Postfix(" HP")]
    [SerializeField] private float _health = 100f;

    [LimitedField("Unlimited")]
    [SerializeField] private int _maxTargets;

    [LimitedField("Instant", allowNegatives: true, negativeLabel: "Default"), Postfix(" (s.)")]
    [SerializeField] private float _delay = -1f;

    [Postfix(" dmg")]
    [SerializeField] private MinMaxFloat _damage = new(5f, 12f);

    [SerializeField] private MinMaxInt _levelRange = new(1, 10);

    [NonBreakingSpace]
    [SerializeField, ReadOnly] private float _computedDps = 42.5f;

    [Header("Setup")]
    [InfoBox("Assign a prefab or this will spawn nothing.", InfoBoxType.Warning, nameof(IsPrefabAssigned))]
    [SerializeField] private GameObject _prefab;

    [DropdownList("Easy", "Normal", "Hard")]
    [SerializeField] private string _difficulty = "Normal";

    [DropdownList(typeof(TestComponentBase), nameof(Levels))]
    [SerializeField] private int _levelId;

    [LayerSelector]
    [SerializeField] private int _groundLayer;

    [EasingCurve]
    [SerializeField] private AnimationCurve _easing = Easing.CubicOut.Clone();

    [SerializeField] private bool _useCustomRadius;

    [InfoBox("Used only when the custom radius is on.", InfoBoxType.Info)]
    [AvailableIf(nameof(_useCustomRadius))]
    [Prefix("r = "), Postfix(" m")]
    [SerializeField] private float _customRadius = 2.5f;

    private const string ShieldBlock = "Shield";

    [FeatureToggle(ShieldBlock)]
    [SerializeField] private bool _shieldEnabled = true;
    [SerializeField] private float _shieldPower = 50f;
    [SerializeField] private Color _shieldColor = new(.3f, .7f, 1f);

    [Foldout("Schedule")]
    [SerializeField] private SerializableDateTime _opens;
    [SerializeField] private SerializableDateTimeRange _active;

    [Foldout("Interfaces")]
    [SerializeReference, SerializeInterface]
    private IWeapon _weapon = new Sword();

    [SerializeField, InterfaceOnly(typeof(IDamageable))]
    private MonoBehaviour _target;

    [Foldout("Collections")]
    [ItemNameFromField(nameof(Wave.name))]
    [SerializeField] private Wave[] _waves =
    {
        new() { name = "Warm-up", count = 5 },
        new() { name = "Assault", count = 20 },
        new() { name = "Boss", count = 1 },
    };

    [ItemNameFromString("Slot {0}", startingIndex: 1)]
    [Postfix(" (coins)")]
    [SerializeField] private int[] _prices = { 10, 25, 50 };

    [Foldout("Effects")]
    [SerializeField] private ParticleSystem.MinMaxCurve _emissionRate = new(10f, 30f);

    private bool IsPrefabAssigned => _prefab != null;

    private static string[] Levels => new[] { "Forest", "Desert", "Castle", "Volcano" };

    [EditorButton("Recharge Shield", ShieldBlock)]
    private void RechargeShield() => _shieldPower = 100f;

    [EditorButton("Rebuild Cache")]
    private void RebuildCache() => Debug.Log("Cache rebuilt");

    [EditorButton()]
    private void Validate() => Debug.Log("Validated");

    
    #region Showcase types

    public interface IWeapon { }

    public interface IDamageable { }

    [Serializable]
    public class Bow : IWeapon
    {
        public int Damage = 12;
        public float Range = 30f;
    }

    [Serializable]
    public class Sword : IWeapon
    {
        public int Damage = 20;
        public float Speed = 1.5f;
    }

    [Serializable]
    public class Wave
    {
        public string name;
        public int count;
    }

    #endregion
}
```

</details>

## Installation

I recommend installing this package with the `OpenUPM` CLI. It keeps dependencies and updates easy to manage. If you cannot use `OpenUPM`, download the package and place it anywhere in your Unity project.

Setting up `OpenUPM` for the first time takes a few minutes, but it is worth it. `OpenUPM` is the usual registry for open-source Unity packages and works with Unity’s Package Manager (dependency resolution and updates included).

On Windows, I recommend `Git Bash` (`MINGW`) for CLI work: it is a Unix-like shell, and it is often already installed.

1. Install `OpenUPM` (skip this if you already have it):
   - If you do not have `npm`, install [Node.js](https://nodejs.org) (or on macOS: `brew install node`).
   - In a terminal, run: `npm install -g openupm-cli`.
   - You can then install packages from the `OpenUPM` registry with no extra Unity setup.
1. Install this package:
   - Open a terminal in your Unity project folder: `cd /path/to/your/project`.
   - Run: `openupm add com.manefunction.tools-unity`.
   - Switch back to Unity and wait for the package to finish importing.

## Compatibility

This package may conflict with other inspector extensions that replace the whole inspector, such as [Odin Inspector](https://odininspector.com) and [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes). Using them together has not been tested yet.

- **One fallback inspector wins.** To draw `EditorButton`s, `Foldout` groups and the `ManeStyle` layout, this package registers a fallback inspector for every `MonoBehaviour` and `ScriptableObject` that has no custom editor of its own. Odin and NaughtyAttributes register fallback inspectors too, and Unity uses only one per type. Whichever one loses stops working for whole-inspector features: NaughtyAttributes' `Button`, `BoxGroup` / `Foldout` and `ShowNonSerializedField` / `ShowNativeProperty`, Odin's own inspector drawing, or this package's `EditorButton`, `Foldout` and `ManeStyle`. There is no error, the features just do nothing.
- **Field-level attributes keep working.** Attributes drawn on a single field, like `ReadOnly`, `InfoBox`, `Prefix` / `Postfix` or NaughtyAttributes' `MinMaxSlider`, work in any inspector, whichever fallback wins.
- **Same attribute names.** `ReadOnly` and `InfoBox` exist in all three packages, and NaughtyAttributes also has `Label` and `Foldout`. A script that has `using` for two of their namespaces gets an "ambiguous reference" compile error. Use only one namespace per script, or write the full name, like `[Mane.Unity.ReadOnly]`.

If you like my project, but miss a feature from another package that you can't use because of mine, open an Issue and I will try to resolve the conflict.

## Why Preview?

Despite the fact that the code itself is not new, splitting one package into a few - plus a pile of refactoring and migration to the `UI Toolkit` - is a great way to invent fresh bugs.

Overall it should still be safe for commercial work (the original ManeTools already ships in a few dozen live projects), but let's give this split a little time to surface whatever I missed. Use at your own risk, I suppose.

## Repository info

This repo follows the [Conventional Commits](https://www.conventionalcommits.org/) specification.

[![GitHub Sponsors](https://img.shields.io/github/sponsors/ManeFunction?label=Sponsor&logo=GitHubSponsors&style=flat)](https://github.com/sponsors/ManeFunction)
[![openupm](https://img.shields.io/npm/v/com.manefunction.tools-unity?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.manefunction.tools-unity/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.manefunction.tools-unity)](https://openupm.com/packages/com.manefunction.tools-unity/)
