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

### Changed
- Moved .NET related classes to the separate library [ManeTools-dotNET](https://github.com/ManeFunction/ManeTools-dotNet.git), so it can be used with a domain code, without any Unity references.
- `TextMesh` named `ManeText` now to not confused with the legacy Unity component, and also moved to the separate module [ManeTools-Text](https://github.com/ManeFunction/ManeTools-Text.git).
- Reorganized extension classes for more clarity.
- All custom Editor UI now uses `UI Toolkit` instead of legacy `IMGUI`.
- Project has 3 different singletons now: `ManeSingleton` for domain code usage (non-Unity), `UnitySingleton` is based on a Unity component, when you need it on a scene, and `ScriptableSingletone` that is data driven and based on ScriptableObject.
- Childen Transform Freezer is not a component anymore, now it's an editor toggle within new Mane Tools overlay panel.
- `PositionFollower` now has 2 different implementations.
- MainThreadDispatcher is a singleton now.
- All `Mane` menu items are now under `Window -> Mane`.
- Tuned `Color` extensions and `Color Picker` to show and work with different values properly (ground everything to `HSL + Luma` system).

### Removed
- Some components and tools was deleted. I think they were too specific for a generic package like this. Some of them was duplicates of existed functions that appars in the standard API last years.