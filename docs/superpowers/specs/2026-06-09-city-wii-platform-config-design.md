# City Wii Platform Config Design

## Goal

Add full Wii support to the City project configuration so the editor-owned build graph can produce a packaged Wii export that includes the same demo-disc scenes the GameCube build already ships.

## Current Problem

The packaged Wii ISO is missing menu-linked scenes because the City project does not currently declare Wii as a supported project platform and does not persist any Wii build settings. The downstream Wii packager only consumes the manifest it is given, so the missing scenes must be fixed at the City project build-configuration layer.

## Source Of Truth

The City project configuration should own Wii support through the same editor files already used for the other platforms:

- `project.heproj`
- `settings/platforms.json`
- `settings/platform.wii.json`
- `user_settings/build_config.json`

The Wii scene list should mirror the existing GameCube demo-disc scene set exactly for this slice.

## Required Behavior

### Supported platform declarations

The project must declare `wii` as a supported platform in both the project file and shared editor platform settings so the editor and headless build flow can treat Wii as a first-class build target.

### Shared platform profile settings

The project must own a `settings/platform.wii.json` file with the standard platform profile structure. For this slice, the important authored behavior is the standard menu action mapping:

- `accept` -> gamepad `South`
- `return` -> gamepad `East`

The rest of the per-platform profile ids may remain blank so the installed Wii platform metadata can resolve defaults.

### Local build settings

The project must own one `wii` entry in `user_settings/build_config.json` that:

- uses the same selected scene ids as the current GameCube demo-disc build
- preserves the same scene ordering
- writes to a Wii-specific output directory
- keeps debug build enabled for local verification

Profile ids may remain blank when the editor build graph already resolves sane platform defaults from installed Wii metadata.

## Verification

Use the editor-owned headless build path, not the downstream Wii repo staging helpers:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ..\helengine\artifacts\build-platform.ps1 `
  -Project ..\helprojs\city\project.heproj `
  -Platform wii `
  -Output ..\helprojs\city\wii-build
```

Success means:

- the headless Wii build no longer fails for missing project platform/build config
- the cooked/staged Wii output contains the full demo-disc scene set, not only `DemoDiscMainMenu`
- the packaged ISO can be launched and menu-selected scenes are present in the build output
