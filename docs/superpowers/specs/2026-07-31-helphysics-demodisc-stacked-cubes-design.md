# HelPhysics Demo Disc Stacked-Cubes Windows Test Design

## Goal

Build the real demo-disc `test_scene_dynamic_stack_boxes` scene as a native Windows player using HelPhysics, so its rendering, scene deserialization, rigid-body binding, fixed-step scheduling, transform synchronization, and packaging path are exercised together before HelPhysics is considered for migration.

This test is deliberately isolated. It does not modify or build from the writable state of any main checkout.

## Immutable Baseline

The demo-disc test starts from commit:

`d716776b6ec6d719c7448fe57652cf342d0b09d6`

The dedicated demo-disc worktree and branch are:

- `C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test`
- `codex/helphysics-demodisc-stack-test`

The committed scene at `assets/scenes/physics/test_scene_dynamic_stack_boxes.helen` is the source of truth. The test must use that scene's existing camera, UI, meshes, materials, static ground, four dynamic boxes, `RigidBody3DComponent` records, and `BoxCollider3DComponent` records. It must not regenerate the scene or substitute a synthetic approximation.

The supporting repositories use these exact committed baselines:

- HelEngine: `25374214867e8e5deeb3cd9e19893caeb2278b64`;
- `helengine-windows`: `b9590af9bece2c824b3531bdc5cc237853879188`;
- `csharpcodegen`: `87958b3ac4f5b2e6221551874e8bf0a7a5620254`.

Changes required by the test live in dedicated worktrees and branches created from those commits. All four main working copies remain read-only. The selected HelEngine and codegen commits contain the reviewed HelPhysics and generated-C++ support merges.

## Build Shape

The Windows package contains exactly one scene: `test_scene_dynamic_stack_boxes`. That scene is the startup scene, so the player does not enter the splash screen, main menu, level selector, loading screen, or another demo-disc scene.

The build uses the normal Windows generated-C++ player pipeline:

1. Load and cook the committed `.helen` scene.
2. Generate native C++ for the engine, required runtime modules, and used demo-disc code modules.
3. Compile the Windows host and generated code together.
4. Package the scene and its existing referenced assets into a dedicated output directory.

The output directory is unique to this test and cannot overwrite the ordinary demo-disc Windows output.

## Explicit Physics Backend Selection

BEPU remains the default for every ordinary build. This test introduces an explicit build-local backend selection whose value is `helphysics`.

The selected backend is carried through the editor build graph into generated-core regeneration and the Windows native build. For the HelPhysics selection:

- generated core includes the HelPhysics runtime and its scene-binding registration;
- the Windows host registers HelPhysics before loading the startup scene;
- `Physics3DRuntimeComponentRegistration` does not register BEPU;
- startup fails if HelPhysics registration, generated native types, or scene binding are absent;
- there is no fallback from HelPhysics to BEPU.

The built package records the selected backend in `runtime-physics-backend.json`, and generated native configuration exposes the same value to the Windows host at compile time. Automated verification rejects an artifact that identifies BEPU, contains conflicting backend selections, or cannot prove which backend it uses.

## Runtime Integration

A HelPhysics runtime adapter owns one `HelPhysicsSceneBinder3D` and implements the engine's `IPhysicsRuntime` boundary. The adapter receives fixed steps from `Core`, synchronizes authored input before each step, advances the binder-owned world, and writes dynamic snapshots back to the existing scene entities after the step.

Scene-load registration traverses the committed scene hierarchy and binds its supported rigid-body and box-collider pairs. The static ground and four dynamic cubes are created from their serialized component values. Unsupported collider or body configurations fail the scene load with a specific error rather than being ignored.

Scene unload detaches the runtime and invalidates bindings before entity disposal. Backend registration and scene callbacks are owned by the runtime integration layer; demo-disc UI and presentation components do not know which physics backend is active.

## Simulation Configuration

The test runs with:

- fixed physics frequency: exactly `20 Hz`;
- velocity iterations: exactly `4`;
- penetration-correction passes: exactly `1`;
- gravity, mass, friction, restitution, damping, and sleep values sourced through the existing scene components and reviewed HelPhysics translation rules.

The Core fixed-step scheduler and HelPhysics world must agree on `0.05` seconds. A disagreement fails before the first simulation mutation.

## Presentation and Input

The scene retains its existing demo-disc camera, materials, lighting, FPS display, instruction presentation, and orbit/light input behavior where those components are already part of the committed asset.

No new synthetic rendering host or replacement cube visualization is used. The proof is the normal Windows renderer drawing the real packaged scene while its original entities receive HelPhysics snapshots.

Existing return-to-menu behavior must not attempt to load a scene omitted from this one-scene package. The isolated cook removes `DemoDiscReturnToMenuComponent` from the packaged scene without altering the committed source asset, and the Windows host handles Escape as a normal application exit for this test build. Automated verification checks both conditions. This exception is local to the test package.

## Isolation Guarantees

The build and implementation obey these boundaries:

- no edits, staging, commits, generation, or output under `C:\dev\helprojs\demodisc`;
- no edits or generated output in the HelEngine main checkout;
- no edits or generated output in the `helengine-windows` main checkout;
- no edits or generated output in the `csharpcodegen` main checkout;
- no copying generated code from a dirty checkout;
- no output beneath the normal demo-disc output directory;
- all source changes are reviewable on dedicated test branches;
- all transient generation occurs in isolated build workspaces or ignored output directories.

The build script captures all four main checkouts' HEADs and porcelain statuses before work and compares them after the build. Any change caused during the test is a build failure.

## Verification

Automated verification must prove:

1. The demo-disc test worktree starts at `d716776` and the committed stacked-cubes scene hash matches that baseline before test-specific code changes.
2. The package selects exactly `test_scene_dynamic_stack_boxes` and uses it as startup.
3. The package contains one static rigid body and four dynamic rigid bodies with box colliders from the real scene.
4. The selected backend is exactly HelPhysics and no BEPU registration call is emitted for the artifact.
5. The generated C++ conversion report contains zero errors.
6. Native compilation and linking complete successfully.
7. The runtime is configured for 20 Hz, four velocity iterations, and one correction pass.
8. HelPhysics scene binding synchronizes dynamic entity transforms through the normal Core physics step.
9. The final Windows executable and packaged scene assets exist in the test-specific output directory.
10. The four main checkouts have identical HEADs and porcelain status before and after the build.

The initial delivery builds but does not launch the player. Launch and visual acceptance occur only after Helena explicitly requests it.

## Failure Behavior

The build stops with a clear diagnostic when the scene hash is wrong, more than one scene is selected, HelPhysics code generation is absent, BEPU remains registered, the fixed-step values disagree, a required referenced asset is missing, conversion reports errors, native compilation fails, or a main checkout changes.

The test does not catch these failures and continue with a partial package.

## Out of Scope

- changing `demodisc/main`;
- replacing the committed scene with the standalone GDI demo;
- migrating ordinary demo-disc builds to HelPhysics;
- changing BEPU defaults;
- testing spheres, capsules, static meshes, joints, vehicles, or continuous collision detection;
- changing the stacked-cubes scene's art, layout, camera, or gameplay presentation;
- supporting non-Windows platforms;
- performance comparison or Tracy capture in this first integration proof.
