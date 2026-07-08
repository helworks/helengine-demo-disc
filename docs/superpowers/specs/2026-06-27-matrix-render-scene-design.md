# Matrix Render Scene Design

**Goal:** Add one dedicated render-only probe scene for Windows that makes transform-combination bugs obvious by animating a single hero cube through every transform combination while a few static reference cubes remain visible for comparison.

**Why a new scene**

The current static probe scene proved the issue reproduces without BEPU, but it is still hard to read how the transform fails over time. A dedicated motion probe scene gives one stable environment for checking move, rotate, scale, and each pair/triple combination without mixing in gameplay or physics behavior.

**Scene contents**

- One static ground plane using the existing physics demo ground material.
- Four static reference cubes:
  - gray: flat control
  - blue: rotated-only reference
  - yellow: scaled-only reference
  - magenta: rotated+scaled reference
- One animated hero cube:
  - red material
  - generated engine cube model
  - no rigid body, no collider, no BEPU components

**Animated hero behavior**

The hero cube runs in a looping phase sequence so each transform state is isolated before the full combination:

1. move
2. rotate
3. scale
4. move+rotate
5. move+scale
6. rotate+scale
7. move+rotate+scale

Each phase should be visually distinct, slow enough to inspect, and deterministic. The movement path should stay inside one small inspection zone near the reference cubes so the fixed camera can see all cases at once.

**Implementation direction**

- Add one new authored physics-lab scene id, but keep the scene render-only.
- Reuse existing scene-authoring helpers for camera, cube meshes, and materials.
- Add one small runtime animation component dedicated to this probe scene rather than overloading physics or menu code.
- Boot the Windows build directly into this new scene for iteration speed.

**Camera**

- Fixed camera only.
- Wide enough to keep the hero cube and all static references visible in one frame.
- No follow or orbit behavior.

**Verification**

- Add one focused source test that proves the new scene is exposed by the physics scene catalog and factory.
- Assert the scene is authored with mesh-only cubes and does not use physics-backed scene helpers.
- Build Windows and launch directly into the new probe scene.
- Use the scene as the visual harness for diagnosing the actual transform/render bug next.
