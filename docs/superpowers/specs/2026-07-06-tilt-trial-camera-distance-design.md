## Summary

Adjust the authored Tilt Trial follow camera so it sits much closer to the player ball while preserving the current viewing angle and the existing follow-camera runtime behavior.

## Goal

When the Tilt Trial scene loads, the camera should keep the same yaw and pitch it has today, but the ball should appear significantly larger on screen because the orbit radius is shorter.

## Approach

Use the existing authored-camera-driven orbit setup in `assets/codebase/game.tools/GameSceneFactory.cs`.

Reduce only the authored `TiltTrialCamera` local-position offset magnitude in `CreateCameraEntity()`. Do not change:

- `DemoTiltFollowCameraComponent`
- the target offset
- the follow input behavior
- the camera orientation

This keeps the current runtime system intact because `DemoTiltFollowCameraComponent` derives its initial orbit radius from the authored pose on first update.

## Why This Approach

Two options were considered:

1. Move the authored camera closer while keeping the same orientation.
2. Add a new serialized orbit-distance override to `DemoTiltFollowCameraComponent`.

Option 1 is preferred because it is the smallest change, matches the existing authored-scene pattern, and avoids adding new runtime behavior for a one-scene tuning request.

## Affected Files

- `assets/codebase/game.tools/GameSceneFactory.cs`
- potentially regenerated Tilt Trial authored scene output if the project keeps a generated scene artifact for this gameplay scene

## Testing

1. Add or update a source-level test that locks the authored Tilt Trial camera distance while preserving the existing orientation intent.
2. Run the targeted city or helengine editor tests that cover the Tilt Trial scene source.
3. Rebuild the Windows package.
4. Launch the Windows build and verify the Tilt Trial camera is visibly closer to the ball while preserving the same angle.

## Risks

- If the camera is moved too close, the framing may feel cramped during fast steering.
- If the authored scene must be regenerated and that step is skipped, the packaged output may continue using the old camera distance.

## Non-Goals

- Changing camera controls
- Changing pitch or yaw behavior
- Adding zoom controls
- Reworking the follow-camera runtime component
