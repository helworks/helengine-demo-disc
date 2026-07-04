# Shared DS Bottom-Screen Controls Design

## Summary

Make the Nintendo DS companion-scene scaffold own one canonical handheld bottom-screen control layout for both rendering scenes and physics scenes. Every scaffolded handheld scene should always emit:

- one `FPSComponent` at `1x` scale
- one full-width `Light` button
- one light-state swatch
- one full-width `Back` button

The handheld `Light` control must use the same light cycle already used on the other platforms:

`white -> yellow -> red -> blue -> green -> off -> white`

Pointer input on the bottom-screen `Light` button and handheld `R` input must both advance that cycle.

## Goals

- Standardize the bottom-screen UI contract across all handheld render scenes and physics scenes.
- Stop relying on authored per-scene bottom-screen overlays for handheld controls.
- Keep handheld light behavior aligned with the existing desktop and console demo-disc light cycle.
- Preserve authored scene-specific bottom-screen roots by appending them after the scaffold-owned controls.

## Non-Goals

- Redesigning the desktop scene UI layout.
- Changing the light cycle itself.
- Requiring every scene factory to manually author a handheld light button.
- Rewriting generated `.helen` files directly.

## Current State

`NintendoDsRenderingSceneScaffoldFactory` already owns:

- bottom-screen camera and viewport setup
- relocation of authored `FPSComponent` instances from top-screen roots into the bottom screen
- the scaffold-owned `Back` button
- removal of top-screen `DemoDiscReturnToMenuComponent`
- removal of top-screen `DemoDiscLightToggleComponent`
- removal of the top-screen light indicator overlay

`PhysicsNintendoDsSceneGenerator` already routes physics companion scenes through `GeneratedAuthoringSceneWriteService.WriteNintendoDsCompanionScene(...)`, so physics scenes already share the same DS scaffold path.

The current handheld bottom-screen contract is inconsistent:

- some render scenes still author oversized `FPSComponent` scales
- the scaffold still uses the legacy temporary `useDefaultBottomOverlay` branch
- the handheld bottom screen has no canonical scaffold-owned light control
- handheld scenes currently strip the authored top-screen light toggle and swatch instead of replacing them with handheld equivalents

## Chosen Approach

Use the shared DS scaffold as the only source of truth for the handheld bottom-screen UI contract.

The scaffold will always emit a canonical bottom-screen control stack for both render scenes and physics scenes:

1. relocated or scaffold-owned `FPSComponent` with `FontScale = 1f`
2. full-width `Light` button with adjacent swatch
3. full-width `Back` button

Scene-specific bottom-screen roots remain supported, but they are appended after the scaffold-owned controls instead of defining the primary handheld interaction contract.

## Layout Contract

The handheld layout should be vertically stacked and fully scaffold-owned:

- `FPSComponent` stays at the top of the bottom screen using `1x` scale
- `Light` is a second full-width button stacked above `Back`
- the swatch is rendered inside or immediately beside the `Light` button row as part of the same scaffold-owned control group
- `Back` remains the bottom-most full-width button

This layout is intentionally simpler than the desktop scene UI. The handheld bottom screen is a dedicated control surface, not a mirror of the desktop viewport overlay.

## Behavior Contract

The handheld scaffold-owned light control uses the same fixed demo-disc cycle as the existing `DemoDiscLightToggleComponent`:

- white
- yellow
- red
- blue
- green
- off
- back to white

Two inputs advance the cycle:

- pointer or touch interaction on the bottom-screen `Light` button
- handheld `R` input

The swatch always mirrors the active cycle state, including the dark `off` color.

If a scaffolded scene has no directional lights, the scaffold still emits the `Light` button and swatch so the layout stays consistent. In that case:

- the swatch uses the `off` color
- the button does not mutate scene lighting
- `R` input becomes a no-op for the light cycle

## Ownership Boundaries

### Shared DS Scaffold

`NintendoDsRenderingSceneScaffoldFactory` becomes responsible for:

- always emitting the canonical bottom-screen layout
- forcing handheld `FPSComponent` scale to `1f`
- creating the scaffold-owned `Light` button and swatch
- creating the scaffold-owned handheld light toggle component
- continuing to create the scaffold-owned `Back` button
- continuing to remove desktop-only return and light-toggle UI from the top-screen roots

### Handheld Light Controller

Add a new scaffold-owned handheld light controller component instead of trying to preserve the authored top-screen `DemoDiscLightToggleComponent`.

That new component should:

- capture directional lights after scene initialization
- update the scaffold-owned swatch
- respond to handheld `R`
- respond to button-click input from the scaffold-owned `Light` button
- preserve the exact same cycle semantics as the existing desktop component

This keeps the handheld contract explicit and avoids depending on authored scene UI subtrees that the scaffold already strips.

### Physics Companion Scenes

Physics scenes must receive the same scaffold-owned handheld layout through the existing `WriteNintendoDsCompanionScene(...)` path.

The physics generator should not maintain a separate bottom-screen contract. If physics currently toggles `useDefaultBottomOverlay` based on FPS presence, that temporary branch should be removed or collapsed into the new always-on canonical handheld layout.

## File-Level Impact

Primary city-side changes should stay concentrated in shared seams:

- `assets/codebase/rendering.tools/NintendoDsRenderingSceneScaffoldFactory.cs`
- one new shared handheld light control component under `assets/codebase/rendering/`
- one new shared scaffold helper or factory for the handheld `Light` button and swatch, if needed
- `assets/codebase/physics.tools/PhysicsNintendoDsSceneGenerator.cs`
- tests under `C:\dev\helworks\helengine\engine\helengine.editor.tests\` that lock the shared authored-source and generated-scene contract

Scene factories should only be touched when they conflict with the new shared contract or when a representative regeneration command is needed for validation.

## Migration Rules

- Do not patch generated `.helen` output manually.
- Regenerate representative render and physics scenes through their authoring commands after the scaffold change.
- Preserve existing scene-specific bottom-screen roots by appending them after the scaffold-owned controls.
- Remove the legacy concept of a temporary default bottom overlay and replace it with the permanent canonical handheld control stack.

## Validation

Validation should cover both shared source and generated output.

### Source-Level Tests

Add focused tests that assert:

- the DS scaffold always emits `FPSComponent` with `FontScale = 1f`
- the DS scaffold emits a full-width `Light` button, swatch, and `Back` button
- the handheld scaffold-owned light component listens to `R`
- the handheld scaffold-owned light component uses the same light-cycle ordering as the existing desktop component
- physics companion-scene generation goes through the same canonical bottom-screen layout

### Generated-Scene Validation

Regenerate representative scenes and verify the generated `.helen` output contains:

- bottom-screen `FPSComponent` at `1f`
- one scaffold-owned light-control subtree
- one scaffold-owned back-button subtree

Representative scenes should include at least:

- one render scene with authored FPS and directional light
- one render scene that previously relied on inconsistent handheld layout
- one physics showcase scene

### Runtime Validation

Rebuild and launch 3DS after regeneration and verify:

- all render scenes show the same bottom-screen layout
- physics scenes show the same bottom-screen layout
- touching the `Light` button advances the cycle
- pressing `R` advances the same cycle
- the swatch color stays in sync with the active state
- `Back` still works

## Risks

The main risk is hidden coupling between the existing desktop `DemoDiscLightToggleComponent` assumptions and the new handheld scaffold-owned light controller. The handheld implementation should reuse the cycle contract and color semantics, but it should not depend on the authored desktop overlay subtree surviving into the handheld scene.

Another risk is physics-scene divergence. The fix is to keep the physics path on the same scaffold-owned contract rather than allowing physics-specific bottom-screen exceptions.

## Result

After this change, handheld companion scenes stop being a best-effort adaptation of desktop scene UI and become a deliberate, shared product contract:

- `FPS 1x`
- `Light button + swatch`
- `Back button`

for all render scenes and physics scenes, with `R` mapped to the same light cycle everywhere.
