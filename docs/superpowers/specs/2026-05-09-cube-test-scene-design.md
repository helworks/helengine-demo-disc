# Cube Test Scene Design

## Goal

Add one committed reusable cube-test scene to `city` that boots directly as the project startup scene and replaces the current multi-scene rendering showcase export for this debugging phase.

The scene must contain only:

- one cube
- one camera
- one light

No menu, no ground, no extra environment, and no additional showcase scenes should be exported from the current rendering scene generator while this diagnostic setup is active.

## Problem

Current PS2 renderer debugging is being obscured by too many moving parts:

- demo-disc menu boot flow
- multiple showcase scenes
- large scene composition
- many proxies and transforms

Even after stripping materials down to flat-color diagnostics, the exported rendering scenes still contain enough geometry and motion to make debugging slow and ambiguous. The fastest way to isolate the remaining PS2 geometry issue is to remove scene complexity entirely and boot directly into one minimal authored runtime scene that is also usable on other platforms.

## Scope

This change is a real committed `city` content change, not a PS2-only override.

It includes:

- a new reusable cube-test scene factory in `city.rendering.tools`
- rendering scene generation updated to emit only that cube scene for now
- project startup changed to that cube scene

It does not include:

- menu integration work
- preserving the current rendering showcase scene export during this debug phase
- restoring the old startup scene as part of this change

## Chosen Approach

Follow the same generated-scene pattern already used by the rendering showcase scenes.

Add one new factory alongside:

- [DirectionalShadowPlazaSceneFactory.cs](/C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs)
- [SpotlightStreetSliceSceneFactory.cs](/C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/SpotlightStreetSliceSceneFactory.cs)

Then update:

- [RenderingSceneGenerator.cs](/C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/RenderingSceneGenerator.cs)

so it writes only the cube-test scene for now.

This keeps the scene authored through the same committed generation path as the existing rendering content while making the actual runtime export as small and deterministic as possible.

## Scene Composition

The cube-test scene contains exactly three entities:

1. camera entity
2. directional light entity
3. cube entity

### Camera

The camera should be static and simple:

- no orbit script
- no sweep script
- no runtime animation
- fullscreen normalized viewport `0,0,1,1`

The camera should be positioned so the cube is fully visible without relying on motion for readability.

### Light

Use one simple directional light so the scene remains valid for lighting-related debugging on other platforms, while still keeping the scene minimal.

No animated light behavior should be attached.

### Cube

Use the same generated engine cube model reference already used by the rendering showcase generator, along with the same standard material reference path already used by the existing rendering scenes.

The cube should be authored with:

- identity orientation
- simple visible scale
- simple centered placement

No script behavior should be attached.

## Export and Startup Behavior

For this debugging phase:

- the rendering scene generator should emit only the cube-test scene
- the project startup should point directly to the cube-test scene
- the old rendering showcase scenes should not be written by the active generation step

This is intentionally easy to revert later by restoring the previous scene generation list and startup selection.

## Cross-Platform Benefit

The cube-test scene is not just for PS2.

Because it is committed in `city` and uses the normal generated-scene flow, it becomes a stable minimal rendering validation scene for:

- Windows
- Vulkan
- PS2
- future renderer bring-up work

That gives the project one canonical “smallest useful 3D scene” for debugging and regression verification across platforms.

## Testing Strategy

Follow red-green before changing generation code.

### Generator Verification

Add or update tests around the rendering scene generator so they prove:

- the cube-test scene id is generated
- the old rendering showcase scenes are not written in this temporary configuration

### Scene Content Verification

Add or update a focused scene-content test that proves the cube-test scene contains only:

- one camera entity
- one light entity
- one cube entity

and does not include orbit/sweep showcase scripts.

### Runtime Verification

After generation and export:

1. build the PS2 ISO
2. boot directly into the cube-test scene
3. verify no menu navigation is required
4. inspect whether the single cube renders correctly

## Non-Goals

This change does not try to:

- fix the PS2 renderer by itself
- preserve the showcase scene export during this debug pass
- create a special platform-specific bootstrap path
- restore the original city startup scene automatically later

## Exit Criteria

This change is successful when:

1. `city` contains a committed reusable cube-test scene
2. the active rendering scene generator emits only that cube scene for now
3. the project boots directly into the cube scene
4. PS2 export no longer requires menu navigation before renderer debugging begins
