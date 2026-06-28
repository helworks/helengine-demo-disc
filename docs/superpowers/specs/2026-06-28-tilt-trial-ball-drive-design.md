# Tilt Trial Ball Drive Design

## Goal

Replace the current Tilt Trial stage-rotation behavior with one fixed-stage, camera-relative ball controller.

The stage must remain visually and physically static. Player input should drive only the dynamic sphere. Motion should feel arcade-like but still respect physics integration:

- input is camera-relative
- only planar `XZ` motion is authored by the controller
- `Y` velocity is always preserved from physics
- diagonal input is normalized
- movement approaches a target velocity using acceleration
- releasing input causes the ball to coast instead of actively braking

## Non-Goals

This change will not:

- keep or improve the existing rotating-stage behavior
- add torque-driven rolling controls
- add explicit braking or drag controls
- add new friction or material systems
- add scene-specific physics special cases in the BEPU runtime

## Current State

`Tilt Trial` is currently assembled in `GameSceneFactory` with:

- one orbit camera using `DemoTiltFollowCameraComponent`
- one dynamic player sphere using `RigidBody3DComponent` and `SphereCollider3DComponent`
- one stage root using `DemoTiltStageComponent`
- several fixed authored course boxes under the stage root
- one reset component on the sphere

The current `DemoTiltStageComponent` reads movement input, computes pitch/roll, rotates kinematic stage pieces, derives their motion velocities, and synchronizes those pieces into BEPU every frame.

That behavior is the wrong control model for the desired game feel. The requested feel is a fixed course with direct ball drive.

## Chosen Approach

Use a fixed-stage, dynamic-ball driver.

`DemoTiltStageComponent` will stop moving stage geometry and will become the controller that:

- resolves the playable sphere at runtime
- resolves the active Tilt Trial follow camera at runtime
- converts left-stick / `WASD` input into one normalized camera-relative planar move vector
- computes one target planar ball velocity
- approaches that target by acceleration while preserving the current `Y` velocity
- writes the updated dynamic-body velocity back into the live physics body

This is preferred over torque or impulse control because it matches the requested Monkey Ball-like feel and avoids control quality depending on contact/friction quirks.

## Architecture

### Component Responsibilities

`DemoTiltFollowCameraComponent`

- remains responsible only for orbit camera behavior
- continues to own yaw/pitch input from right stick / arrows
- continues to resolve the followed sphere through a serialized scene entity reference

`DemoTiltBallResetComponent`

- remains responsible only for out-of-bounds reset
- continues to teleport the sphere back to spawn when it falls below the reset height

`DemoTiltStageComponent`

- no longer rotates or synchronizes stage pieces
- becomes the Tilt Trial movement controller
- resolves the player sphere and follow camera at runtime
- computes and applies camera-relative planar velocity steering

`GameSceneFactory`

- keeps the same scene shape and fixed authored course geometry
- continues to create the stage root and attach `DemoTiltStageComponent`
- continues to assign the follow camera target to the sphere
- does not need stage-piece kinematic motion setup anymore beyond the existing static authored geometry

### Runtime Entity Resolution

To avoid hard-coded scene assumptions and per-frame casts spread across gameplay code, `DemoTiltStageComponent` will resolve:

- the player sphere entity
- the player sphere rigid body
- the orbit camera entity
- the orbit camera follow component

Resolution should happen lazily and then be cached, using the same style of scene-runtime lookup already used by `DemoTiltFollowCameraComponent`.

The controller must fail loudly if the expected scene wiring is missing.

## Control Model

### Input Mapping

Movement input sources:

- keyboard: `WASD`
- gamepad: left stick

Camera input sources remain:

- keyboard: arrow keys
- gamepad: right stick and d-pad

### Camera-Relative Planar Basis

The movement controller will read the orbit camera orientation and derive:

- camera forward
- camera right

Then it will:

- zero the `Y` component on both vectors
- normalize each vector after flattening
- combine them using player input
- normalize the final combined move vector if its length is non-zero

This guarantees:

- looking up or down never creates vertical drive
- diagonal input is not faster than cardinal input
- input always follows the current camera orbit heading

### Velocity Steering

The ball controller will work in world-space velocity, not force or torque.

For each frame with non-zero movement input:

1. Read the current dynamic rigid-body velocity.
2. Extract the current planar velocity from `X` and `Z`.
3. Build one target planar velocity from:
   - normalized move direction
   - `MaximumPlanarSpeed`
4. Move the current planar velocity toward the target by:
   - `PlanarAccelerationUnitsPerSecond * elapsedSeconds`
5. Recombine:
   - updated planar velocity
   - untouched current `Y` velocity
6. Write the resulting velocity back to the rigid body.
7. Synchronize the dynamic body into the active physics runtime.

For each frame with zero movement input:

- preserve the rigid body velocity as-is
- do not inject braking
- allow the ball to coast naturally

## Tuning Parameters

`DemoTiltStageComponent` should expose only the minimum tuning needed for this control pass:

- `MaximumPlanarSpeed`
- `PlanarAccelerationUnitsPerSecond`
- `GamepadDeadzone`

No additional knobs should be added in this change for:

- braking
- drag
- torque
- friction overrides
- air control specialization

Those can be introduced later only if testing shows they are actually needed.

## Failure Behavior

The controller should throw explicit exceptions when required runtime wiring is missing, including:

- no attached parent entity
- no resolved player sphere
- no resolved follow camera
- missing rigid body on the player sphere
- missing compatible physics runtime
- invalid flattened camera basis that cannot produce a planar direction

The intent is to preserve the existing project rule of fixing initialization and wiring correctly rather than masking broken scene setup.

## Testing Strategy

Validation should stay narrow and directly tied to the new behavior.

### Automated Test

Add one focused test for the planar velocity steering math that proves:

- incoming `Y` velocity is preserved
- diagonal input is normalized
- velocity approaches the target by the configured acceleration limit rather than snapping
- zero input leaves the current velocity unchanged so coasting works

This test should target the smallest logic unit possible instead of requiring a full scene integration harness.

### Runtime Validation

After the automated test passes:

- rebuild the Windows player
- boot directly into `tilt_trial`
- verify the course remains fixed
- verify left stick / `WASD` drive the ball relative to the camera
- verify releasing input causes coasting
- verify looking up/down does not change the movement plane

## Expected User-Facing Result

Tilt Trial should now behave like a fixed obstacle course where:

- the camera orbits freely around the ball
- movement follows the camera heading
- the ball is driven only across the ground plane
- gravity, falling, and bounce still come from physics
- the stage no longer visually tilts or rotates under the player

## Implementation Boundaries

This should remain a gameplay-scene change first, not an engine-wide physics redesign.

Expected touched areas:

- `city/assets/codebase/game/DemoTiltStageComponent.cs`
- possibly a small extracted helper for planar velocity steering if needed for clean testing
- `city` gameplay tests if a suitable target exists, otherwise the smallest appropriate engine-side test host that can validate the math unit

This change should not require:

- BEPU runtime contract changes
- renderer changes
- scene catalog changes
- menu changes

