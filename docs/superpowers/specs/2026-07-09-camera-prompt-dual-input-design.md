# Camera Prompt Dual-Input Design

## Summary

Update the shared rendering and physics scene camera prompt so one `Camera` row can show either:

- one icon for single-input platforms
- two icons for platforms that support both D-pad and left-stick camera movement

The runtime camera behavior should continue to accept D-pad and left-stick input for playable rendering and physics showcase cameras. The shared prompt overlay should be brought in line with that behavior using the generated raw control icons that already exist in the project.

## Goal

Make the shared `Camera` prompt accurate across rendering scenes and physics scenes without forking scene content per platform.

## Non-Goals

- Redesigning the prompt panel layout beyond the `Camera` row
- Introducing semantic action mapping on top of raw control IDs
- Adding prompt fallback behavior for unknown platforms
- Changing Nintendo DS to show a non-existent analog control
- Replacing the existing shared-scene and per-platform override authoring model

## Current State

`DemoDiscOrbitCameraComponent` and `DemoFollowCameraComponent` already accept:

- keyboard `WASD`
- gamepad D-pad
- gamepad left-stick input

The current shared prompt overlay in `DemoSceneInstructionOverlayFactory` does not reflect that. The `Camera` row is authored as one icon per platform, currently showing:

- `wasd` on Windows
- `dpad` on consoles and handhelds

This is now inaccurate for platforms that expose both D-pad and left-stick camera movement.

The generated control icon pack already contains the raw assets needed for this feature, including:

- `left_stick`
- `analog`
- `circle_pad`
- `dpad`

depending on the platform family.

## Chosen Approach

Keep one shared prompt overlay scene and extend the overlay authoring model from:

- one row maps to one icon spec per platform

to:

- one row maps to one or more icon specs per platform

The `Camera` row will remain a single row with the short text label `Camera`.

For platforms that support two relevant inputs, the row will display both icons on the same row with a small fixed gap. For platforms that only support one relevant input, the row will continue to display a single icon.

This preserves the existing raw-icon, strict-resolution, per-platform override architecture instead of introducing a second semantic prompt system.

## Platform Contract

The `Camera` row should use these raw controls:

- `windows` and `win32`: `wasd`
- `ds`: `dpad`
- `3ds`: `dpad` and `circle_pad`
- `psp`: `dpad` and `analog`
- `dreamcast`: `dpad` and `analog`
- `xbox360`: `dpad` and `left_stick`
- `switch`: `dpad` and `left_stick`
- `gamecube`: `dpad` and `control_stick`
- `wii`: `dpad` and `stick`
- `ps2`: `dpad` and `left_stick`
- `psvita`: `dpad` and `left_stick`
- `ps1`: `dpad` and `left_stick`
- `ps3`: `dpad` and `left_stick`
- `xbox`: `dpad` and `left_stick`
- `steamdeck`: `dpad` and `left_stick`
- `n64`: `dpad` and `control_stick`

`gamecube`, `wii`, and `n64` should use their platform-authentic stick-style raw controls instead of a literal `left_stick` id. That still satisfies the user requirement because those icons are the functional left-stick approximants exported by the generated pack.

If any listed raw control is missing from the generated manifest for its platform family, scene generation must fail hard.

## Authoring Model

`DemoSceneInstructionOverlayFactory` should gain a multi-icon row model for desktop and console overlays.

Instead of one `DesktopInstructionPlatformIconSpec` per platform for the `Camera` row, the overlay should author:

- one ordered list of one or two raw icon specs per platform
- one shared icon host subtree for the row
- per-platform sprite overrides for each authored icon slot

The row should continue to use one shared scene and editor-authored per-platform overrides rather than duplicating scenes per platform.

### Baseline Authoring

Use the Windows prompt as the common authored baseline for the `Camera` row, because it remains a single `wasd` icon and matches the current shared-scene authoring convention.

Additional icon slots required by other platforms should still exist in the shared authored row so overrides can populate them. The baseline can keep secondary slots hidden or empty until overridden by platforms that need them.

### Layout

The `Camera` row should render:

- a single icon centered in the existing icon region when only one icon is present
- two icons laid out left-to-right with a fixed gap when both are present

The text label stays unchanged:

- `Camera`

The row must preserve current prompt-panel readability and avoid expanding the panel size unless measurement during implementation proves it necessary.

## Runtime Behavior

The preferred implementation is to keep runtime camera input behavior unchanged unless a real platform gap is discovered during verification.

Reasoning:

- `DemoDiscOrbitCameraComponent` already reads D-pad plus left-stick input
- `DemoFollowCameraComponent` already reads D-pad plus left-stick input
- the user request is primarily a prompt-accuracy change

If verification finds one showcase camera path that does not actually use the shared orbit or follow component behavior, then that path should be aligned during implementation. The intended contract after the change is:

- rendering showcase cameras support D-pad or left-stick where the platform exposes both
- physics showcase cameras support D-pad or left-stick where the platform exposes both
- DS remains D-pad only in the prompt

## Nintendo DS Behavior

Nintendo DS should remain a handheld-specific special case.

For DS:

- show only `dpad`
- do not show any second analog icon
- do not change the existing handheld bottom-screen prompt model beyond keeping the `Camera` row accurate

This matches the explicit requirement that platforms without a left stick must not show one.

## File Impact

Expected primary code changes:

- `assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs`
- `assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs`
- additional rendering-tools tests for multi-icon prompt authoring and per-platform override persistence

Possible runtime changes only if verification proves necessary:

- `assets/codebase/rendering/DemoDiscOrbitCameraComponent.cs`
- `assets/codebase/rendering/DemoFollowCameraComponent.cs`

Physics and rendering scene generators should not need separate prompt logic if they continue to consume the shared overlay factory.

## Testing Strategy

Add tests before implementation that prove the new prompt contract.

### Overlay Source Tests

Verify that the shared overlay factory source now authors a multi-icon `Camera` row and persists platform-specific sprite overrides for all required icon slots.

At minimum:

- Windows resolves to one `wasd` icon
- DS resolves to one `dpad` icon
- 3DS resolves to `dpad` plus `circle_pad`
- PSP resolves to `dpad` plus `analog`
- one standard console resolves to `dpad` plus `left_stick`

### Resolver and Strictness Tests

Verify that the raw icon resolver still fails hard when a requested control is absent.

This is important because the new prompt contract increases the number of required raw control IDs per platform.

### Scene Regeneration Verification

After implementation, regenerate:

- rendering scenes
- physics scenes

Then verify the generated scene assets changed as expected and still serialize the shared prompt rows correctly.

## Regeneration Plan

After the implementation is complete and tests pass:

1. Regenerate rendering scenes through the editor command.
2. Regenerate physics scenes through the editor command.
3. Verify the resulting scene assets changed consistently.

The generated `.helen` files should not be hand-edited.

## Risks

### Shared Row Complexity

The current prompt row model assumes one icon entity per row. Moving to one-or-two icons per row adds layout and override complexity. The implementation should keep the row contract narrow and not generalize beyond what this feature needs.

### Platform Coverage Drift

If a future icon-pack regeneration renames `analog`, `circle_pad`, or `left_stick`, the row generation will fail. That is the correct behavior, but tests need to make failures obvious.

### Baseline Slot Authoring

If the common authored baseline only expects one sprite and later rows need two, the shared row subtree must still serialize cleanly. The implementation should prefer a stable two-slot row structure with optional visibility rather than trying to create or destroy row children per platform.

## Recommendation

Implement the `Camera` prompt as one shared row with one or two raw icons per platform, keep the short `Camera` label, leave DS single-input only, and preserve the current shared-scene plus per-platform override architecture.
