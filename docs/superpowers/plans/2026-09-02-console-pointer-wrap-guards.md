# Console Pointer-Wrap Guard Plan

## Failure

The engine intentionally exposes pointer-wrap state only under `DESKTOP_PLATFORM`, because console targets have no desktop client-edge mouse cursor to wrap. Two DemoDisc Zombislayer components call those APIs outside desktop guards, so GameCube code generation emits calls that cannot exist in the native `InputSystem` contract.

## Repair

1. Extend DemoDisc's maintained desktop-input source contract to classify `SetPointerWrapEnabled` and `RequestPointerWrapEnabled` as desktop-only references.
2. Guard the Zombislayer session's explicit pointer-wrap state update with `#if DESKTOP_PLATFORM`.
3. Move the FPS controller's wrap request into the existing desktop mouse-look guard.
4. Run the focused desktop-input contract and affected Zombislayer tests, then regenerate the GameCube game core through the real DemoDisc build.
5. Continue the Docker/devkitPPC build from the next concrete compiler or packaging result.

## Acceptance

- Non-desktop preprocessing removes every DemoDisc pointer-wrap call.
- Desktop mouse-look and pause/resume pointer behavior remain unchanged.
- No engine-side no-op API or console-only utility is introduced.
- GameCube native compilation passes the two prior missing-member errors.
- Existing importer-generated DemoDisc changes remain untouched.
