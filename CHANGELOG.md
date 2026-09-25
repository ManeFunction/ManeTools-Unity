# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]
### Changed
- `UnityRandom` takes one shared lock around its swap of `UnityEngine.Random.state`, so overlapping draws from any instances cannot interleave that global state. The previous global state is restored if a draw throws.

## [2.0.0-preview.1] - 2026-08-29

Initial release of the extracted Unity codebase and Editor tools. Types were moved and refactored out of the legacy Unity-coupled module. Versioning starts at 2.0.0 to mark that split; this is not a new project, it is just a fresh start.

Below, you can find a list of changes compared with legacy ManeTools.

### Added
- Added `SerializableDateTime` and `SerializableDateTimeRange` types for date data, and `Calendar` - an editor date selection control.
- Added the `InterfaceOnly` attribute to declare fields that require a `MonoBehaviour` plus an interface.
- Added a Mane Tools editor Scene View overlay to keep editor features in one place.
- Added 'Copy as C# code' to color fields' context menu.
- Added `EditorButton` to draw method-calling buttons in inspectors.
- Added optional conditions to `InfoBox`.
- `Apply Transform values` to a prefab now works properly with `RectTransform`.
- Added editor `LimitedValueField` as a custom `IntField` with custom labels and limits for non-positive values.
- All `UI Toolkit` styles were aggregated into this package, so they can be reused from one place. You can use them as well if you want to style your inspectors the same way.
- Added the custom style attribute `ManeStyle` that you can apply to a component or Scriptable Object to follow this package's inspector styles. In that case `Space` splits visual blocks, `Header` adds a header to the split, and custom `Foldout("Header")` creates a foldout block, the same as in `ThreeStatesToggle`.
- Added a `Prefix` property decorator, alongside the existing `Postfix` one.
- Added the `ItemNameFromString` attribute, to set collection element names like `ItemNameFromField`, but using a custom format string `{0}`.

### Changed
- Moved .NET-related classes to the separate library [ManeTools-dotNET](https://github.com/ManeFunction/ManeTools-dotNet.git), so they can be used from domain code without Unity references.
- Moved UI (`uGUI` and `TMPro`) related components and tools to the separate library [ManeTools-UnityUI](https://github.com/ManeFunction/ManeTools-UnityUI.git), because `uGUI` is a package that may not be in the project, and `TextMesh Pro` is part of the `uGUI` package now.
- `TextMesh` was renamed to `ManeText` so it is not confused with the legacy Unity component, and moved to the separate module [ManeTools-Text](https://github.com/ManeFunction/ManeTools-Text.git), because honestly it's needed in very rare cases nowadays.
- Reorganized extension classes for more clarity.
- All custom Editor UI now uses `UI Toolkit` instead of legacy `IMGUI`.
- The project has 3 different singletons now: `ManeSingleton` for domain code (non-Unity), `UnitySingleton` based on a Unity component when you need it on a scene, and `ScriptableSingleton` that is data-driven and based on `ScriptableObject`.
- `Children Transform Freezer` is not a component anymore; it is an editor toggle in the `Mane Tools` overlay panel.
- `PositionFollower` now has 2 different implementations.
- `MainThreadDispatcher` is a singleton now.
- All `Mane` menu items that open windows are now under `Window → Mane Tools`.
- Tuned `Color` extensions and `Color Picker` to show and work with different values properly (everything grounded to the `HSL + Luma` system).
- Most custom hotkey-related menus (screenshots, enabling / disabling GameObjects, console clearing, etc.) are under the `Edit` menu now.
- `Screenshoter` is now available from code via `Screenshot.Capture()` with an optional custom path.
- The Enable / Disable GameObject hotkey is F6 instead of F4 now (F4 is used for the Search panel by default).
- `Missing Reference Finder` is now a context menu from `Assets` or `GameObject` menus instead of a separate window.
- `Asset Reference Finder` is now a context menu from the `Assets` menu instead of a separate window.
- `Scene management` hotkeys moved under the `File` menu with improved selected-scene detection.
- `GetRequiredComponent` is `GetOrAddComponent` now, for clarity.
- `ArrayElements` attribute renamed to `ItemNameFromField`.
- `DropdownList` attribute now works not only with public methods, but also with private members and properties.
- `DropdownList` options (when they are loaded dynamically from code) can now be refreshed from the context menu.
- `Layer` attribute renamed to `LayerSelector`.
- `SerializeReferenceInterface` attribute was renamed to `SerializeInterface`.

### Removed
- Intentionally dropped support of the legacy `IMGUI` system, highlighting the advantages of `UI Toolkit`.
- Dropped support of the legacy `Text` component. Everyone has used `TextMesh Pro` for years anyway.
- Some components and tools were deleted. They were too specific for a generic package like this, and some of them duplicated functions that appeared in the standard API over the last years.
