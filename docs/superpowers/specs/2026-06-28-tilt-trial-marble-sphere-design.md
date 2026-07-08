# Tilt Trial Marble Sphere Design

## Goal

Give the playable Tilt Trial sphere a project-owned classic white marble appearance with gray veining.

The change must affect only the Tilt Trial sphere. The rest of the scene should keep using the existing generated scene material flow.

## Non-Goals

This change will not:

- alter Tilt Trial gameplay, physics, or controls
- replace stage or wall materials
- create a shared general-purpose marble material library
- introduce scene catalog, menu, or startup-scene changes

## Current State

`GameSceneFactory` currently creates the playable Tilt Trial sphere in `CreatePlayerSphereEntity()`.

That entity uses:

- the generated sphere model
- `GeneratedStandardMaterial`
- the existing rigid body, sphere collider, and reset component wiring

Because the sphere uses the same generated material path as other generated geometry, it does not have a distinct marble look.

## Chosen Approach

Create one dedicated Tilt Trial sphere texture and one dedicated Tilt Trial sphere material, then wire only the player sphere entity to use that material.

This keeps the scope narrow and makes the marble look project-owned without disturbing the rest of the generated scene pipeline.

## Asset Design

Add one new texture under `assets/textures/rendering/tilt_trial/`:

- white marble base
- soft gray veining
- sized only for this sphere use

Add one new material under `assets/materials/rendering/tilt_trial/`:

- references the new marble texture
- uses the existing standard lit material schema already used by project rendering assets
- remains dedicated to the Tilt Trial sphere rather than being positioned as a shared reusable marble material

## Scene Wiring

Update `CreatePlayerSphereEntity()` in `assets/codebase/game.tools/GameSceneFactory.cs` so the player sphere uses the new marble material instead of `GeneratedStandardMaterial`.

No other entities in `GameSceneFactory` should change material assignment.

Expected touched areas:

- `assets/codebase/game.tools/GameSceneFactory.cs`
- one new texture file and its `.hasset`
- one new material `.hasset`

## Validation

Validation should stay narrow:

1. confirm the Tilt Trial sphere entity now references the new material path
2. rebuild the Windows output
3. launch directly into `tilt_trial`
4. verify the sphere renders with the marble look
5. verify the rest of the course appearance remains unchanged

## Expected Result

Tilt Trial should launch with the playable ball rendered as a white marble sphere with gray veining, while the stage and surrounding generated geometry continue to use their existing materials.
