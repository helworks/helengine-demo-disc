# PS2 Tilt Play Right-Stick Camera Control

## Goal

Make the right analog stick control the orbit camera in the Tilt Play scenes on PS2.

## Existing behavior

`DemoTiltFollowCameraComponent` already consumes shared `RightStickX` and `RightStickY` values for orbit yaw and pitch. The PS2 input backend currently publishes only the left analog stick, so the camera receives zero right-stick input on PS2.

## Design

Extend the PS2 controller snapshot with `RightStickX` and `RightStickY`. When the connected pad exposes analog mode, normalize `padButtonStatus.rjoy_h` and `rjoy_v` using the same center, deadzone, and scale as the left stick. Publish those values through `InputGamepadState` without changing the existing camera component or movement controls.

The right-stick horizontal axis will continue to drive orbit yaw, and the vertical axis will continue to drive orbit pitch with the camera component's existing inversion and pitch limits.

## Verification

Add source-level PS2 native-input coverage asserting that the mapper contains right-stick fields, reads both DualShock right-stick fields, and publishes both values to `InputGamepadState`. Run that focused test only; do not build the PS2 target.
