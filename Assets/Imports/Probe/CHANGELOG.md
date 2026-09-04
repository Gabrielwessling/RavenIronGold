# Changelog

All notable changes to Probe are documented here.
This project follows Semantic Versioning.

## [1.0.2] — 2026-07-22

### Fixed
- EventSystem creation now waits until scene initialization is complete, so
  Probe no longer creates a duplicate when the scene already contains one.

### Changed
- Re-exported the package with Unity 2022.3.62f3 for broader Editor compatibility.
- Validated on Unity 2022.3 LTS and Unity 6 with both the legacy Input Manager
  and the new Input System.
- Clarified that Probe uses Unity uGUI and has no third-party dependencies.

## [1.0.1] — 2026-07-12

### Fixed
- Probe now respects an existing scene `EventSystem` even when it initializes
  before that EventSystem has assigned `EventSystem.current`.

## [1.0.0] — 2026-06-04

First public release. Free Asset Store package.

### Added
- **Zero-setup in-game console** — builds its own UI from code, auto-bootstraps
  at startup, toggles with the backquote key or an on-screen `</>` button.
- **`[DebugCommand]` command system** — turn any static or instance method into a
  console command. Type-checked arguments (`string` / `int` / `float` / `double`
  / `bool` / `enum`), optional parameters, and quoted strings.
- **Command autocomplete (Tab) and history (↑/↓).**
- **Log capture with search + severity filters** (Log / Warn / Err).
- **`[DebugButton]` one-tap actions** — run a method with a tap; great for mobile QA.
- **`[DebugTweak]` live sliders** — tune any `float` / `int` / `double` / `long`
  at runtime, no recompile.
- **Tweak presets** — save / load / reset slider values as JSON, from the panel
  buttons or console (`preset_save` / `preset_load` / `preset_list` / `preset_reset`).
- **Debug panel** — FPS, memory, and custom `ProbePanel.Watch` values.
- Works in Built-in / URP / HDRP and with both the new Input System and the
  legacy Input Manager.
- Example `ProbeDemo` component and a full README.

### Notes
- Probe auto-runs only in the Editor and Development builds. Use the
  `PROBE_ENABLE` / `PROBE_DISABLE` scripting defines to override (see README).
- Static registries, event subscribers, UI references, and cached watches are
  reset at play-mode startup for projects that disable Domain Reload.
