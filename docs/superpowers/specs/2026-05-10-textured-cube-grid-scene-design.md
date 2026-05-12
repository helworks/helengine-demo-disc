## Summary

Add one new committed rendering test scene to `city` that preserves the existing single-cube and colored-cube scenes and introduces a third scene containing a `4x4` grid of rotating cubes. Each cube uses the shared generated cube model, rotates slowly, starts with a different orientation, and uses a distinct authored textured material. The new scene becomes the temporary startup/export target for fast cross-platform texture validation.

## Goals

- Preserve the current `Cube Test` scene as the smallest renderer bring-up baseline.
- Preserve the current `Colored Cube Grid` scene as the lit material-color baseline.
- Add a third validation scene that stresses:
  - multiple textured 3D drawables
  - distinct authored texture assets and per-cube material bindings
  - per-entity transforms and rotation updates
  - directional lighting across many textured faces
- Make the new scene available to every platform through the normal `city` scene-generation flow.
- Point the project startup/export selection at the new scene for now so testing does not require menu input.

## Non-Goals

- Removing the existing single-cube scene.
- Removing the existing colored-cube scene.
- Reusing production textures from unrelated scene content.
- Adding PS2-only scene content that other platforms do not share.
- Replacing authored material assets with renderer-side debug overrides.

## Scene Structure

### Existing Scenes

Keep the current scenes unchanged:

- scene id: `scenes/rendering/cube_test.helen`
- scene id: `scenes/rendering/colored_cube_grid.helen`

Both remain generated and selectable.

### New Scene

Add one new scene:

- scene id: `scenes/rendering/textured_cube_grid.helen`
- display name: `Textured Cube Grid`
- purpose: multi-object textured material validation scene

Scene contents:

- one camera
- one directional light
- sixteen cube entities arranged in a `4x4` grid in front of the camera

## Layout

The cubes should use the same readable `4x4` centered grid layout as the colored cube scene so the texture test isolates materials and sampling instead of camera framing changes.

The camera remains straightforward:

- faces the grid head-on
- uses fullscreen viewport `0,0,1,1`
- keeps the whole grid visible with some margin

The directional light remains simple:

- one authored directional light entity
- same authored light direction convention already used by the renderer
- strong enough to reveal lit textured faces clearly

## Cube Behavior

Each cube should:

- use the generated cube model reference
- use a standard material reference that resolves to one authored textured material
- rotate slowly at runtime
- start with a different initial orientation so the grid does not look uniform

Rotation behavior should match the current readable baseline:

- roughly the same slow speed as the existing cube test
- use the current reusable runtime spin path rather than inventing another one

## Texture and Material Strategy

Use real authored per-cube textures and materials, not renderer diagnostics.

That means:

- each cube gets its own distinct texture asset
- each cube gets its own distinct material asset
- each material references the standard material path and binds its texture explicitly
- every platform sees the same intended textured result

The textures should be simple diagnostic assets:

- distinct from one another
- easy to differentiate at a glance
- authored on the `city` side rather than borrowed from engine internals

The PS2 renderer should continue to apply lighting on top of the sampled texture rather than falling back to flat color or grayscale-only output for this scene.

The new scene therefore depends on the standard material path carrying texture bindings through:

- scene/material authoring
- packaging/cooking
- runtime material loading
- backend shading

## Scene Generation

`city` rendering scene generation should now produce all three rendering scenes:

- `cube_test.helen`
- `colored_cube_grid.helen`
- `textured_cube_grid.helen`

The generator should remain explicit and committed, not data-driven by temporary files. The new scene should follow the same authored-scene pattern already used by the cube and colored-grid scene factories.

## Catalog and Startup

The demo-disc scene catalog should list all three rendering scenes:

- `Cube Test`
- `Colored Cube Grid`
- `Textured Cube Grid`

For current debugging convenience, project startup/export selection should point to:

- `scenes/rendering/textured_cube_grid.helen`

This is an intentional temporary default. The other scenes remain generated and selectable, but the new textured grid scene becomes the direct boot target for now.

## Error Handling

The scene generator should fail explicitly if any required generated references, authored materials, or texture assets are missing. Do not silently substitute default textures or fallback materials.

Material and texture authoring should remain explicit. If the standard material path cannot carry the texture binding for one platform, that should fail in a diagnosable way instead of degrading to hidden renderer-specific defaults.

## Testing

### Source-Level Coverage

Add or update focused tests to prove:

- the rendering scene generator writes all three scene ids
- the new textured grid scene contains sixteen cube entities
- each cube receives a distinct authored material/texture assignment
- startup/export selection points at `textured_cube_grid.helen`

### Runtime Verification

Rebuild and export PS2 after the scene lands, then verify:

- the project boots directly into `Textured Cube Grid`
- all sixteen cubes are visible
- cubes rotate
- textures are distinct
- lighting still affects faces visibly

Desktop verification should confirm the same authored scene intent.

## Risks

- The PS2 texture path has not yet been validated with a multi-object textured scene in this new material layout.
- Sixteen textured cubes increase bandwidth and state usage, which may expose PS2 renderer issues not visible in the colored-material scene.
- Incorrect material or texture references could accidentally validate cached or shared assets instead of true per-cube bindings.

## Recommended Implementation Direction

Implement this as a third committed scene factory in `city`, keep the current cube and colored-grid scenes intact, and make the new textured-grid scene the temporary startup target. Treat per-cube textures as part of the real authored material pipeline, not as a renderer shortcut.
