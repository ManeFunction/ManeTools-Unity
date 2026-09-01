# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [2.0.0-preview.1] - 2026-08-29

Initial release of the extracted Unity codebase and Editor tools. Were moved and refactored out of the legacy Unity-coupled module. Versioning starts at 2.0.0 to mark that split; this is not a new project, it's just a fresh start.

Below, you can find a list of changes, comparing with legacy ManeTools.

### Added
- Added Mane Tools editor scene view overlay to keep all the editor features in one place.
- Added 'Copy as a C# code' to color fields context menu.
- Added `EditorButton` feature to draw methods calling buttons within your editors.
- Added optional conditions to the `InfoBox`.
- `Apply Transform values` to a prefab now works with `RectTransform`.

### Changed
- Moved .NET related classes to the separate library [ManeTools-dotNET](https://github.com/ManeFunction/ManeTools-dotNet.git), so it can be used with a domain code, without any Unity references.
- `TextMesh` named `ManeText` now to not confused with the legacy Unity component, and also moved to the separate module [ManeTools-Text](https://github.com/ManeFunction/ManeTools-Text.git).
- Reorganized extension classes for more clarity.
- All custom Editor UI now uses `UI Toolkit` instead of legacy `IMGUI`.
- Project has 3 different singletons now: `ManeSingleton` for domain code usage (non-Unity), `UnitySingleton` is based on a Unity component, when you need it on a scene, and `ScriptableSingletone` that is data driven and based on ScriptableObject.
- Childen Transform Freezer is not a component anymore, now it's an editor toggle within new Mane Tools overlay panel.
- `PositionFollower` now has 2 different implementations.
- MainThreadDispatcher is a singleton now.
- All `Mane` menu items that opens different windows are now under `Window -> Mane Tools`.
- Tuned `Color` extensions and `Color Picker` to show and work with different values properly (ground everything to `HSL + Luma` system).
- Most of the custom hotkeys related menues (screenshoting, enabling / disabling GO, console clearing, ect.) are under `Edit` menu now.
- `Screenshoter` now available from the code via `Screenshot.Capture()` with optional custom path.
- `Enable / Disable GO hotkey` is F6 instead of F4 now (F4 is used for the Search panel by default).
- `Missing Reference Finder` now a context menue from `Assets` or `GameObject` menues instead of the separate window.
- `Scene management` hotkeys moved under the `File` menu with improved selected scene detection.

### Removed
- Some components and tools was deleted. I think they were too specific for a generic package like this. Some of them was duplicates of existed functions that appars in the standard API last years.