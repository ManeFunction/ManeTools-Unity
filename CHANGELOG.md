# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [2.0.0-preview.1] - 2026-10-27

Initial release of the extracted Unity codebase and Editor tools. Types were moved and refactored out of the legacy Unity-coupled module. Versioning starts at 2.0.0 to mark that split; this is not a new project, it is just a fresh start.

Below, you can find a list of changes compared with legacy ManeTools.

### Added
- `SerializableDateTime` and `SerializableDateTimeRange` types for date data, and `Calendar` - an editor date selection control.
- `InterfaceOnly` attribute to declare fields that require a `MonoBehaviour` plus an interface.
- `Easing` presets: the standard easing equations as shared `AnimationCurve`s (`SinIn`, `ExpoOut`, `BackInOut`, and the rest). The `EasingPreset` enum names them: `Easing.Get(preset)` returns the curve, `Easing.FindPreset(curve)` recognizes a copy of one.
- `EasingCurve` attribute for `AnimationCurve` fields. The inspector shows a dropdown of the easing presets, and the curve field only for `Custom`, which keeps the current keys (an empty curve starts from a linear one). Picking a preset copies its keys, so the field stays a plain `AnimationCurve` at runtime.
- `ChildFields`, a shared editor layout for child fields of a parent field: a `├` / `└` branch mark in a fixed slot, then the field. `SerializeInterface` and `EasingCurve` use it, and so does `ManeStyle` for plain serializable classes: their fields are drawn like a `SerializeInterface` instance, only without the type dropdown. In a default inspector `SerializeInterface` and `EasingCurve` instead take the layout of Unity's own nested fields (no branch marks).
- Mane Tools editor Scene View overlay to keep editor features in one place.
- 'Copy as C# code' to color fields' context menu.
- `EditorButton` to draw method-calling buttons in inspectors. Buttons go at the end of the inspector, or at the end of a `Header` or `Foldout` block when you pass its text as `block`. Buttons in a foldout hide with it, unless `hideWithFoldout` is false.
- Optional conditions to `InfoBox`.
- `Apply Transform values` to a prefab now works properly with `RectTransform`.
- Editor `LimitedIntField` as a custom `IntegerField` with custom labels and limits for non-positive values, and `LimitedFloatField` as its decimal version based on `FloatField`.
- `LimitedField` attribute to draw a plain `int` or `float` field with `LimitedIntField` / `LimitedFloatField`, optionally with `allowNegatives` and a custom `negativeLabel`.
- All `UI Toolkit` styles were aggregated into this package, so they can be reused from one place. You can use them as well if you want to style your inspectors the same way.
- Custom style attribute `ManeStyle` that you can apply to a component or Scriptable Object to follow this package's inspector styles. In that case `Space` splits visual blocks, `Header` adds a header to the split, and custom `Foldout("Header")` creates a foldout block, the same as in `ThreeStatesToggle`. The `ManeStyle for all` toggle in the `Mane Tools` overlay applies this layout to every inspector drawn by the package's fallback inspector, without the attribute. It is saved per project.
- `FeatureToggle` attribute for a `bool` field. In a default inspector it is a header above the field, like `Header`. The header text is optional: `[FeatureToggle]` takes the field name (or its `Label` text). Inside `ManeStyle` it splits a block and its title becomes a checkbox bound to the field; the fields after it stay in that block and are hidden while it is off. The block is the new `FeatureBlock` editor control (usable in UXML, with `toggle-path` and `header`). `ManeInspectorLayout.Fill` has an overload that starts from a given property, so a custom inspector can build only a part of the fields. `EditorButton` accepts a `FeatureToggle` text as its `block`.
- `NonBreakingSpace` attribute. It adds the same spacing as `Space` (or a custom height) in the default inspector, but inside `ManeStyle` it keeps the fields in one block instead of splitting it.
- `Prefix` property decorator, alongside the existing `Postfix` one. On `MinMaxInt` / `MinMaxFloat` / `MinMaxDouble` both decorators are drawn around each of the Min and Max inputs.
- `Label` attribute to show a custom field name in the inspector without renaming the field. It works as a decorator, so it combines with `Prefix` / `Postfix`, `DropdownList`, and other attributes, and on collections it replaces the collection title.
- `ItemNameFromString` attribute, to set collection element names like `ItemNameFromField`, but using a custom format string `{0}`.
- `UnityRandom.Range01Inc`, the new `IRandom` member from ManeTools-dotNET: a float in `[0, 1]`, where 1 is a possible result too.
- `ParticleSystemUIScaler` that scales a `ParticleSystem` emitter size and particle count (emission rates, bursts, max particles) by a `RectTransform` size, relative to reference values captured with one inspector button. Shape axes that follow the rect width / height are detected from the emitter rotation.
- `UIElementsTools.AddStyleSheet`, `SetVisible`, `ShowWhen`, `Require` and `BindField` for custom inspectors: attach the stylesheet next to a script once, show or hide an element, keep an element shown only while a condition on serialized properties holds, find an element the UXML must have with an error when it is missing, and bind a field by property path with an error when the path is wrong. `InfoBoxDrawer.Create` can add an action button to a standalone info box. `ScriptTemplates.CreateScript` creates a script from a template in a package's `Editor/ScriptTemplates` folder.

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
- Custom hotkey menus (game screenshot, console clearing) are under the `Edit` menu now. The GameObject enable / disable shortcuts were removed.
- `Screenshoter` is now available from code via `Screenshot.Capture()` with an optional custom path.
- `Missing Reference Finder` is now a context menu from `Assets` or `GameObject` menus instead of a separate window.
- `Asset Reference Finder` is now a context menu from the `Assets` menu instead of a separate window. It finds references from every asset type (materials, ScriptableObjects, animator controllers, and so on), not only scenes and prefabs, using Unity's dependency data instead of searching file text, so binary-serialized assets are covered too. Several selected assets are searched in one pass. A `deep` variant also searches the text of every text file for the asset's GUID, to find GUIDs stored as plain strings, such as an Addressables `AssetReference`.
- `Scene management` hotkeys moved under the `File` menu with improved selected-scene detection.
- `GetRequiredComponent` is `GetOrAddComponent` now, for clarity.
- `ArrayElements` attribute renamed to `ItemNameFromField`.
- `DropdownList` attribute now works not only with public methods, but also with private members and properties.
- `DropdownList` options (when they are loaded dynamically from code) can now be refreshed from the context menu.
- `Layer` attribute renamed to `LayerSelector`.
- `SerializeReferenceInterface` attribute was renamed to `SerializeInterface`.
- `UnityRandom` takes one shared lock around its swap of `UnityEngine.Random.state`, so overlapping draws from any instances cannot interleave that global state. The previous global state is restored if a draw throws.
- `AnimationEventInvoker` now shows a `UnityEvent` and can be used from the inspector as well as from code.
- **Create → Scripting → MonoBehaviour Script** now creates an empty class body, without `Start` and `Update`.
- `AvailableIf`, the `InfoBox` show condition and `DropdownList` now find the members they name the same way: on the object that holds the field (a nested class or a list element), then on the inspected object, from the most derived type to the base, as a field, property or parameterless method. Conditions check serialized fields first, where any non-default value counts as true, so `InfoBox` now accepts non-bool serialized conditions too. `DropdownList` also accepts a `string[]` field.
- A member that `AvailableIf`, `InfoBox` or `DropdownList` cannot find is logged as an error once per type and member, instead of on every inspector change. `AvailableIf` then leaves the field available instead of locking it.

### Fixed
- `AvailableIf` with several objects selected used only the first one: a field locked on the others could be edited on all of them. It is now available only when it is on every selected object.
- A `DropdownList` instance member on a nested serializable class or list element was never found.
- Fixed `SerializeInterface` collection Option hold ignoring (fold / unfold all).
- `ToCoroutine` now calls back with `(false, default)` when the task faulted or was canceled, instead of throwing from the coroutine.
- `WaitForSecondsRealtimeWhile` counted scaled time instead of unscaled time.
- `IsInPolygon` returned true for outside points on the line of an edge past its ends, for example `(10, 0)` for the triangle `(0, 0)`, `(4, 0)`, `(0, 4)`. Points on an edge still count as inside. The edge tolerance is now a distance instead of an area, so it no longer depends on the polygon's size, and a repeated corner is ignored.
- `UnityRandom.Range01` could return exactly `1f` (about 3 in 100 million calls), breaking its `[0, 1)` range, because the top values of the integer it scaled round up to 1 as a float. It now draws one of the 2^24 float steps below 1.
- A `UnitySingleton` placed in a scene threw when that scene was loaded again. The new copy now destroys its GameObject.
- Changes made by an `EditorButton` method to the inspected objects were not saved: they were not marked dirty, not kept as prefab overrides, and could not be undone.
- Custom `ManeEditor` inspectors did not draw `EditorButton` methods, and did not re-apply the field layout after a change the way `ManeStyle` default inspectors do, so fields created by the change (list items, for example) could keep Unity's label alignment.
- Children Transform Freeze did not record the children's counter-move as prefab overrides, so prefab instance children could jump back on reload.
- `ReadOnly` together with `AvailableIf` made the field editable whenever the condition was true, because the last decorator to run decided the enabled state. Decorators on one field now apply together: the field is enabled only when it is not read-only and is available.
- `Prefix` and `Postfix` showed nothing on `string` fields.
- `DropdownList` showed the first option for an unset value, and picking that option did not save it. A value that is not an option now shows as empty.

### Removed
- Intentionally dropped support of the legacy `IMGUI` system, highlighting the advantages of `UI Toolkit`.
- The `Color Picker` no longer reads the clipboard every 200 ms. It applied any text that parsed as a color and then cleared the system clipboard. Paste a color into the hex field instead.
- Dropped support of the legacy `Text` component. Everyone has used `TextMesh Pro` for years anyway.
- Some components and tools were deleted. They were too specific for a generic package like this, and some of them duplicated functions that appeared in the standard API over the last years.
