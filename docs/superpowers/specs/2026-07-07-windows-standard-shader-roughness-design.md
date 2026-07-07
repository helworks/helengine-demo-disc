# Windows Standard Shader Roughness Support

## Goal

Add roughness support to the built-in Windows `standard-shader` so authored materials can provide:

- a scalar `roughness` value with default `1.0`
- an optional `roughness-texture-id`

Then switch Tilt Trial to use the marble ball material and feed it the user-supplied roughness texture at `C:\Users\Helena\Downloads\WhatsApp Image 2026-07-06 at 17.59.58.jpeg`.

## Scope

This feature includes:

- Windows standard-shader authored field support for roughness
- runtime material hydration for roughness scalar and roughness texture
- DirectX11 material texture binding support for the additional material texture
- built-in Windows forward shader updates to consume roughness
- city-authored marble material generation and Tilt Trial scene switch
- regression tests covering schema, runtime hydration, material generation, and scene wiring

This feature does not include:

- full metallic/roughness PBR
- image-based lighting
- cross-platform PBR parity on PS2, PSP, GameCube, or DS
- normal-map or metallic-map rollout as part of the same change

## Requirements

### Authoring

The Windows `standard-shader` schema must expose:

- `roughness`
- `roughness-texture-id`

Behavior rules:

- `roughness` defaults to `1.0`
- `roughness-texture-id` is optional
- final roughness is `saturate(roughness * sampledRoughnessTexture)`
- if the roughness texture is missing, roughness falls back to the scalar only

### Runtime

Windows standard-shader materials must:

- preserve existing diffuse/base-color behavior
- preserve existing shadow participation and render-state behavior
- bind both diffuse and roughness textures deterministically
- upload roughness scalar data through a standard-material constant buffer

### City

Tilt Trial must stop using the walnut player sphere material and instead use a marble material that:

- references the marble base-color texture
- references the imported roughness texture copied from the user-provided JPEG
- uses the Windows standard-shader roughness fields

Non-Windows platforms may keep their simpler platform material schema behavior and do not need to emulate Windows roughness.

## Architecture

### 1. Standard Material Authoring Model

The existing Windows `standard-shader` remains the only built-in standard Windows material path. Roughness is added as an extension of that model rather than as a new shader family.

The Windows schema gains:

- `roughness` as a scalar authored value
- `roughness-texture-id` as an optional texture asset reference

`MaterialAssetSettingsService` remains the place that mirrors authored Windows standard-shader fields into runtime-facing `ShaderMaterialAsset` data. It will continue to hydrate:

- render state
- fixed shader program selection
- diffuse texture id
- standard material constant buffers

It will additionally hydrate:

- roughness scalar constant-buffer data
- roughness texture asset id on the runtime-facing shader material payload

### 2. Shader Asset Model

The current `ShaderMaterialAsset` only exposes:

- diffuse texture
- normal texture
- emissive texture

To support roughness cleanly through the existing material system, the shader-owned material payload must gain one additional optional texture asset id for roughness.

That asset id must round-trip through:

- raw asset model
- shader material binary serialization
- runtime material loading
- DirectX11 material build path

### 3. Runtime Texture Binding

The current DirectX11 3D path binds the first ordinary material texture only. That is sufficient for diffuse-only materials but not for roughness-aware standard materials.

The DirectX11 material application path must be extended so that:

- every texture binding in the material layout is considered
- texture slot binding matches shader-declared binding names and slots
- diffuse and roughness both bind reliably for standard materials

This should be implemented generically enough to avoid special-casing the marble material, but the acceptance target is the Windows standard-shader path.

### 4. Windows Standard Shader

The built-in `ForwardStandardShader.hlsl` currently uses:

- diffuse lighting from base color
- a hardcoded Blinn-Phong specular exponent
- a hardcoded specular strength

The shader will be upgraded to use roughness-aware highlight shaping while preserving:

- current forward light types
- current shadow logic
- current base-color workflow

This is a physically-inspired improvement of the existing shader, not a full PBR rewrite.

## Rendering Model

### Supported Shading Behavior

The Windows standard shader will remain a forward-lit material that combines:

- diffuse base color
- light accumulation
- shadow attenuation
- roughness-controlled specular response

Roughness affects:

- highlight width
- highlight intensity

Expected visual result:

- lower roughness yields tighter, brighter highlights
- higher roughness yields broader, dimmer highlights

### Explicit Non-Goals

This change does not attempt to add:

- metallic workflow
- Fresnel tuning UI
- ambient occlusion input
- BRDF lookup textures
- environment reflections

Those would require a larger material-model and lighting redesign and are intentionally out of scope for this feature.

## City Content Plan

### Roughness Texture Source

The user-provided roughness image:

- `C:\Users\Helena\Downloads\WhatsApp Image 2026-07-06 at 17.59.58.jpeg`

will be copied into the city project under the Tilt Trial texture source area and imported through the normal texture pipeline.

### Marble Material Authoring

A generated marble material factory will be added parallel to the walnut factory. It will author:

- material id
- diffuse texture id
- Windows roughness scalar
- Windows roughness texture id
- existing per-platform fallback fields

The authored Windows material must use the built-in standard shader, not a custom shader override.

### Tilt Trial Scene Switch

Tilt Trial scene generation and asset preparation will switch from the walnut material to the marble material as the active player sphere material.

The acceptance target is that the packaged Windows Tilt Trial build renders the ball using the marble material and the new roughness response.

## Testing Strategy

### Engine Tests

Add or extend tests to cover:

- Windows `standard-shader` schema includes `roughness` and `roughness-texture-id`
- `MaterialAssetSettingsService` hydrates roughness scalar and roughness texture correctly
- shader material serialization round-trips the roughness texture asset id
- material layout/runtime binding supports the additional texture binding
- DirectX11 standard material binding assigns both diffuse and roughness textures to the expected slots

### City Tests

Add or extend tests to cover:

- authored marble material contains the Windows roughness fields
- Tilt Trial authored material generation references the marble material
- Tilt Trial scene source uses the marble player sphere material reference instead of walnut

### Runtime Verification

The runtime verification bar is:

- the Windows build can package successfully
- the marble ball is active in Tilt Trial
- the ball shows visibly different highlight behavior under lighting

## Risks

### Material Binding Risk

The highest engine risk is expanding the current single-texture binding assumption in the DirectX11 3D material path. The implementation should preserve current diffuse-only behavior while adding predictable support for the roughness texture binding.

### Content Tuning Risk

The supplied JPEG is a real-world image that may need tuning once sampled as roughness. The engine feature should support the map as-authored, while the scalar roughness multiplier provides a built-in tuning control.

### Scope Risk

The work must stay roughness-only. Adding metallic, normals, or broader PBR cleanup during this feature would increase risk and blur validation.

## Recommended Implementation Order

1. Add failing engine tests for schema, material hydration, and roughness texture serialization.
2. Extend the shader material asset model and serialization for roughness texture support.
3. Extend Windows standard-shader schema and material-settings hydration for `roughness` and `roughness-texture-id`.
4. Add standard-material roughness constant-buffer data and update the built-in Windows shader.
5. Extend DirectX11 runtime material texture binding to support both diffuse and roughness textures.
6. Add city marble material generation and import the roughness texture.
7. Switch Tilt Trial from walnut to marble.
8. Rebuild and verify the Windows package visually.

## Acceptance Criteria

- Windows `standard-shader` supports authored `roughness` and optional `roughness-texture-id`.
- Runtime material loading preserves and binds roughness data correctly.
- The built-in Windows standard shader uses roughness to shape specular response.
- City contains an authored marble ball material that references the imported roughness texture.
- Tilt Trial uses the marble ball material instead of walnut.
- Automated engine and city source tests cover the new behavior.
