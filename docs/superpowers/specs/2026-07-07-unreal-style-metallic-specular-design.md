# Unreal-Style Metallic Specular Design

## Goal

Add scalar `metallic` and `specular` support to the shared `standard-shader` material path so Windows and editor-preview materials can use an Unreal-style metallic/roughness workflow with a separate dielectric specular trim control.

This is primarily to improve highlight control on non-metal materials such as the Tilt Trial marble ball without abusing metallic to fake stronger reflections.

## Non-Goals

This change will not:

- add metallic or specular texture maps in the first pass
- add normal mapping
- redesign non-Windows fixed-pipeline shaders to consume the new values
- change the current roughness texture path beyond integrating with the new scalar controls
- introduce broad content migration for existing materials

## Current State

The current built-in standard shader uses:

- `base-color`
- `texture-id`
- `roughness`
- `roughness-texture-id`
- fixed dielectric reflectance with `F0 = 0.04`

That means:

- non-metal materials cannot raise or lower dielectric reflectance directly
- metallic behavior does not exist in the standard shader contract
- roughness can widen or tighten highlights, but cannot change base reflectance

For the Tilt Trial marble ball specifically, using metallic to make the ball less dull would be physically wrong because marble should remain non-metallic.

## Chosen Approach

Add two scalar fields to the standard shader contract:

- `metallic` with default `0.0`
- `specular` with default `0.5`

Implement them as constant-buffer-backed material data on the shared standard shader path.

Use `specular` as an Unreal-like dielectric reflectance trim for non-metals, while `metallic` blends between dielectric reflectance and base-color-driven metal reflectance.

This keeps the first pass small, preserves the current roughness workflow, and leaves the contract in a clean state for future metallic/specular texture support.

## Contract Changes

Add the following fields to the Windows `standard-shader` schema and the corresponding editor-side standard material handling:

- `metallic`
  - scalar
  - default `0.0`
- `specular`
  - scalar
  - default `0.5`

The field names should remain stable so future texture-map support can extend the same contract instead of replacing it.

Expected touched contract areas:

- `helengine-windows/builder/WindowsPlatformDefinitionFactory.cs`
- `helengine.editor/managers/asset/MaterialAssetSettingsService.cs`
- shared shader material asset serialization/runtime handling as needed for new constant buffers

## Runtime Data Flow

### Authoring

Materials authored with `standard-shader` may omit both new fields and still behave correctly through defaults:

- `metallic = 0.0`
- `specular = 0.5`

### Material Serialization

`MaterialAssetSettingsService` should:

- parse the two scalar fields
- clamp them to the supported `0..1` range
- serialize them into stable named constant buffers on `ShaderMaterialAsset`

This should mirror the current base-color and roughness-buffer pattern.

### Runtime Loading

No new runtime texture resolution path is required for these controls.

The values travel as constant-buffer data, so `ShaderRuntimeMaterialLoader` should continue working through the existing shader-backed material path without adding new texture resolution logic.

### Shader Behavior

`ForwardStandardShader.hlsl` should:

- read `metallic` and `specular` from new constant buffers
- derive dielectric `F0` from the Unreal-style `specular` control
- blend `F0` toward base color as `metallic` rises
- reduce diffuse contribution as metallic rises
- keep roughness evaluation unchanged so the roughness texture continues to drive highlight width/shape

The marble ball should remain authored as:

- `metallic = 0.0`
- `specular` tuned upward if the default dielectric response still feels too dull

## Compatibility

No asset migration step is required.

Existing materials that omit the new fields should:

- deserialize successfully
- receive `metallic = 0.0`
- receive `specular = 0.5`
- preserve their current look as closely as possible

This makes the feature additive rather than breaking.

## City Content Usage

Update the Tilt Trial marble material to carry the new scalar fields explicitly once engine support exists.

Target authored values for the marble ball in the first tuning pass:

- `metallic = 0.0`
- `specular` set above default only if needed after visual verification
- keep `roughness-texture-id` active
- keep `roughness` scalar available as a multiplier, not a replacement for the texture

The important behavior requirement is that the ball continues to use its roughness texture while gaining a better dielectric highlight response.

## Testing

Add or extend tests for:

1. material settings hydration
   - `metallic` and `specular` fields round-trip into the material asset
   - default values are applied when fields are omitted
2. shader source/layout
   - standard shader exposes the new constant-buffer bindings
3. Windows builder contract
   - schema exposes the new fields
   - cooked material serialization preserves them
4. runtime/editor material loading
   - standard shader materials rebuild with the new constant buffers intact
5. city authored-material coverage
   - Tilt Trial marble material preserves intended scalar values and existing roughness texture binding

## Validation

Validation should happen in this order:

1. run focused engine/editor/builder tests for the new fields
2. regenerate city authored assets
3. rebuild the Windows package
4. launch Tilt Trial
5. verify:
   - the ball remains non-metallic
   - the roughness texture still influences highlight response
   - highlight intensity is stronger and less dull when tuned through `specular`
   - the ball does not pick up metallic coloration artifacts

## Expected Result

The shared standard shader gains a clean Unreal-style scalar metallic/specular contract for Windows and preview paths.

The Tilt Trial marble ball can stay physically non-metallic while still getting stronger, more controllable dielectric highlights, with the roughness texture continuing to shape the reflection response instead of being bypassed.
