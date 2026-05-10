## Summary

Add one new committed rendering test scene to `city` that keeps the existing single-cube scene intact and introduces a second scene containing a `4x4` grid of rotating cubes. Each cube uses the shared generated cube model, rotates slowly, starts with a different orientation, and uses a distinct authored material color. The new scene becomes the temporary startup/export target for fast cross-platform renderer validation.

## Goals

- Preserve the current minimal `Cube Test` scene as the simplest renderer bring-up baseline.
- Add a richer follow-up validation scene that stresses:
  - multiple 3D drawables
  - repeated material usage with distinct authored colors
  - per-entity transforms and rotation updates
  - directional lighting across many visible faces
- Make the new scene available to every platform through the normal `city` scene-generation flow.
- Point the project startup/export selection at the new scene for now so testing does not require controller input.

## Non-Goals

- Removing the existing single-cube scene.
- Introducing textures in this step.
- Adding new menu systems or debug-only scene bootstraps.
- Adding PS2-only scene content that other platforms do not share.

## Scene Structure

### Existing Scene

Keep the current single-cube scene unchanged:

- scene id: `scenes/rendering/cube_test.helen`
- menu/catalog entry remains available
- continues to serve as the smallest fallback renderer check

### New Scene

Add one new scene:

- scene id: `scenes/rendering/colored_cube_grid.helen`
- display name: `Colored Cube Grid`
- purpose: multi-object lit color/material validation scene

Scene contents:

- one camera
- one directional light
- sixteen cube entities arranged in a `4x4` grid in front of the camera

### Layout

The cubes should be arranged in a flat `4x4` grid centered around the world origin so the camera sees all cubes without panning. Spacing should be wide enough to keep silhouettes separated while still fitting comfortably in frame on PS2 and desktop.

The camera remains straightforward and readable:

- faces the grid head-on
- uses fullscreen viewport `0,0,1,1`
- keeps the whole grid visible with some margin

The directional light remains simple:

- one authored directional light entity
- same scene-authored direction convention already used by the renderer
- strong enough to show face variation without blowing out all cubes equally

## Cube Behavior

Each cube should:

- use the generated cube model reference
- use a standard material reference that resolves to one authored colored material
- rotate slowly at runtime
- start with a different initial orientation so the grid does not look uniform

Rotation behavior should stay readable:

- roughly the same slow speed as the existing cube test
- use the current reusable runtime spin path rather than inventing another one

Initial orientation variation should be deterministic and stable. The point is to ensure multiple cubes present different faces to the light and camera even before rotation advances.

## Material Strategy

Use real authored per-cube material color, not renderer diagnostics.

That means:

- each cube gets its own distinct material asset or generated authored material variant
- the color exists at the scene/material level
- every platform sees the same intended cube colors

The PS2 renderer should continue to apply lighting on top of that color rather than reverting to grayscale-only lighting for this scene.

The new scene therefore depends on the standard material path carrying base color through:

- scene/material authoring
- packaging/cooking
- runtime material loading
- backend shading

## Scene Generation

`city` rendering scene generation should now produce both rendering scenes:

- `cube_test.helen`
- `colored_cube_grid.helen`

The generator should remain explicit and committed, not data-driven by ad hoc temporary files. The new scene should follow the same authored-scene pattern already used by the cube test scene factory.

## Catalog and Startup

The demo-disc scene catalog should list both rendering scenes:

- `Cube Test`
- `Colored Cube Grid`

For current debugging convenience, project startup/export selection should point to:

- `scenes/rendering/colored_cube_grid.helen`

This is an intentional temporary default. The single-cube scene remains generated and selectable, but the new grid scene becomes the direct boot target for now.

## Error Handling

The scene generator should fail explicitly if any required generated references or authored material inputs are missing. Do not silently substitute default colors or fallback assets.

Material/color authoring should also remain explicit. If the standard material path cannot carry authored color for one platform, that should fail in a way we can diagnose instead of degrading to hidden renderer-specific defaults.

## Testing

### Source-Level Coverage

Add or update focused tests to prove:

- the rendering scene generator writes both scene ids
- the new colored grid scene contains sixteen cube entities
- each cube receives a distinct authored material/color assignment
- startup/export selection points at `colored_cube_grid.helen`

### Runtime Verification

Rebuild and export PS2 after the scene lands, then verify:

- the project boots directly into `Colored Cube Grid`
- all sixteen cubes are visible
- cubes rotate
- colors are distinct
- lighting still affects faces visibly

Desktop verification should confirm the same authored scene intent.

## Risks

- The current standard material pipeline on PS2 only recently moved beyond grayscale lighting. Carrying real base color may expose another missing link in the material path.
- Sixteen cubes increase draw count and may expose additional PS2 performance or state issues sooner than the single-cube scene.
- If color authoring is attached incorrectly, we could accidentally validate a renderer-side override instead of real scene materials.

## Recommended Implementation Direction

Implement this as a second committed scene factory in `city`, keep the current cube scene intact, and make the new grid scene the temporary startup target. Treat authored per-cube color as part of the real material pipeline, not as a debug shortcut.
