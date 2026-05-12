# Axis Test 2 Design

## Goal

Add a second rendering validation scene named `axis_test2` that reuses the existing `axis_test` geometry and materials while changing only the camera placement, the lighting receiver wall placement, and the directional-light rotation axis. The purpose of the scene is to validate directional-light rendering behavior as the light direction sweeps around the camera forward axis while the world Z axis remains unchanged.

## Scope

`axis_test2` keeps the following authored content exactly aligned with `axis_test`:

- origin marker
- X, Y, and Z shafts
- floor and ground meshes
- center gizmo position
- shared cube model and shared generated materials

`axis_test2` changes only the following:

- camera transform
- wall placement that acts as the side receiver for the lighting test
- directional-light animation axis
- scene id, generated scene file, and catalog wiring needed to expose the new scene

## Scene Layout

The new camera is placed at the position that corresponds to the current right-wall test viewpoint and is oriented to look back toward the center gizmo at the origin-facing composition used by `axis_test`.

The wall receiver is moved so it sits on the new camera-right side. The intent is that, from the camera view, the wall remains a clear receiver surface while the rest of the world-space axes still appear in their original authored positions.

The center directional-light arrow remains visible near the center of the frame so the light direction can be inspected while the scene runs.

## Directional-Light Behavior

`axis_test2` uses a dedicated runtime spin component or equivalent dedicated scene behavior that rotates the directional light around the camera forward axis instead of the screen-plane sweep used by `axis_test`.

The authored world axes do not rotate. Only the light rig orientation changes over time.

The light-direction sweep should be slow, continuous, and deterministic, using the same style of absolute-time-driven motion already used by the existing rendering validation scenes.

## Implementation Approach

The scene should be implemented as a separate `AxisTest2SceneFactory`.

`AxisTest2SceneFactory` should:

- reuse the same generated arrow model asset and axis-test material scheme as `axis_test`
- author a separate scene id for `axis_test2`
- create its own camera entity with the new transform
- create its own wall receiver placement
- create its own light rig using a rotation component aligned to the camera forward axis

The existing `AxisTestSceneFactory` should remain behaviorally unchanged.

## Catalog And Generation

The rendering scene generator should emit both `axis_test` and `axis_test2`.

The generated scene file should be written to:

- `assets/scenes/rendering/axis_test2.helen`

If the demo-disc scene catalog or Windows selected-scene list is updated later, that should be done explicitly as a separate follow-up unless required to make the scene buildable and testable.

## Testing

Validation should cover:

- generation of the new authored `axis_test2` scene
- presence of one directional-light component in the new scene
- presence of the dedicated light-rotation runtime component for `axis_test2`
- expected camera placement and scene id wiring
- successful Windows packaging with the new scene included when requested

## Risks

The main risk is axis confusion between camera-local forward rotation and world-axis rotation. The implementation should therefore keep the light rotation logic explicit and scene-specific instead of trying to generalize it prematurely.

Another risk is unintentionally drifting `axis_test` while introducing the shared helpers needed by `axis_test2`. Any helper extraction should preserve the existing authored `axis_test` output.
