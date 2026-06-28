# Render Matrix Probe Scene Design

**Goal:** Add one render-only physics-lab scene that isolates rotated and scaled cube rendering on Windows without any BEPU components, and boot directly into it for fast diagnosis.

**Scope**

- Add one new authored scene id for a render-only matrix probe.
- Populate the scene with one ground plane plus four cube cases:
  - gray: flat control cube
  - blue: rotated-only cube
  - yellow: scaled-only cube
  - red: rotated+scaled cube
- Use only `CreateCubeMeshEntity(...)` so the scene stays completely outside the physics path.
- Point the Windows build config directly at the new scene for immediate runtime validation.

**Scene Layout**

- Ground: centered floor using the existing neutral ground material.
- Cubes arranged left-to-right for visual comparison:
  - `flat_control`
  - `rotated_only`
  - `scaled_only`
  - `rotated_scaled`
- Camera placed high and back on the positive Z axis so all four cubes stay visible in one frame.

**Verification**

- Add one source test that proves the scene is exposed by the catalog and factory.
- Assert the authored scene uses `CreateCubeMeshEntity(...)` entries for all probe cubes.
- Assert the probe scene does not call any physics-backed scene helpers for those cubes.
- Regenerate physics scenes, build Windows, and launch directly into the probe scene.
