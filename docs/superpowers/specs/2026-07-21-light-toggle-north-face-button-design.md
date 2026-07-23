# Light Toggle North Face Button

## Goal

Use the controller face button represented by `InputGamepadButton.North` to advance the light toggle on every platform.

## Scope

- Replace the shared console light-toggle component's `RightShoulder` binding with `North`.
- Replace the Nintendo DS light-overlay component's handheld gamepad binding with `North`.
- Preserve keyboard `L` and Nintendo DS touch activation.
- Add focused regression coverage for the new binding.
- Do not modify generated scenes, blueprints, or unrelated working-tree changes.

## Validation

Run the smallest gameplay/rendering test project that covers the light-toggle source and confirm the old shoulder binding is absent from both components.
