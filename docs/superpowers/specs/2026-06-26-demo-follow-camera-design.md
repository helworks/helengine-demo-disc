## Goal

Add a scene-local `DemoFollowCameraComponent` for the static-mesh physics showcase so the player sphere uses a Super Monkey Ball style third-person camera:

- the camera follows the ball's position
- the player can orbit freely around the ball
- the camera ignores the ball's physical rotation
- other demo scenes remain unchanged

## Why A New Component

The existing `DemoDiscOrbitCameraComponent` is shared by multiple scenes and is built around a mostly static authored orbit center. Extending it would increase risk across the wider demo-disc scene set.

`DemoFollowCameraComponent` keeps this behavior isolated to the static-mesh showcase and makes the intended camera style explicit in scene authoring.

## Scope

In scope:

- add `DemoFollowCameraComponent` under `city` gameplay code
- author the static-mesh showcase camera to use the new component
- target the existing player sphere entity
- preserve free orbit input around the followed target
- add tests/source audits for the new scene wiring

Out of scope:

- replacing `DemoDiscOrbitCameraComponent`
- changing camera behavior in other rendering or physics scenes
- adding a generalized engine-level camera framework
- changing player movement or sphere physics

## Behavior

`DemoFollowCameraComponent` will:

- resolve one followed target entity, intended to be the player sphere
- compute an orbit center from the target world position plus one authored vertical offset
- preserve free manual yaw/pitch orbit around that moving center
- keep an authored orbit radius
- clamp pitch so the camera cannot flip below or above useful play angles
- update the attached camera entity transform every frame

It will not:

- inherit the sphere's local orientation
- snap behind velocity automatically
- perform collision avoidance in this pass

## Authored Data

The component should expose the minimum data needed to make the scene tunable:

- target entity reference
- center height offset
- orbit radius
- manual yaw speed
- manual pitch speed
- minimum pitch
- maximum pitch
- optional default yaw
- optional default pitch

The static-mesh showcase scene should author these values directly on its camera entity.

## Scene Integration

The static-mesh showcase currently creates a fixed camera via `CreatePhysicsShowcaseCameraEntity(...)`.

That scene should instead:

- keep the same camera entity slot in the final scene asset
- attach `DemoFollowCameraComponent`
- bind the component to the player sphere entity created in the showcase
- remove dependence on the old fixed orbit setup for that scene

The player sphere remains the gameplay focus and no new playable actor type is introduced.

## Testing

Add focused tests that prove:

- `DemoFollowCameraComponent` source exists and exposes the expected authored role
- the static-mesh showcase scene authors `DemoFollowCameraComponent`
- the static-mesh showcase camera targets the player sphere entity
- the old orbit camera component is not the active authored camera behavior for that scene

Prefer source or serialization-level tests over trying to validate camera feel numerically in a physics runtime test.

## Risks

- scene entity targeting may be brittle if the current scene authoring helpers do not already support a stable entity reference path
- if the camera target resolution relies on runtime entity ids, authoring and runtime serialization must agree exactly
- without collision avoidance, the camera may clip through geometry in some corners of the showcase; this is acceptable for the first pass unless it becomes severe

## Recommended Implementation

1. Add `DemoFollowCameraComponent` in `city` gameplay code.
2. Use the same input conventions already used by `DemoDiscOrbitCameraComponent` where practical.
3. Add or reuse one stable scene-entity targeting mechanism for the followed sphere.
4. Re-author the static-mesh showcase camera entity to use the new component.
5. Add source/tests to lock the scene wiring.

## Self-Review

Checked:

- scope is limited to the static-mesh showcase
- no other scene camera behavior is changed
- no generated files are specified for direct edits
- collision avoidance is intentionally deferred and called out explicitly
- the main open implementation detail is target-entity reference wiring, which should be resolved from existing authoring/runtime patterns before coding
