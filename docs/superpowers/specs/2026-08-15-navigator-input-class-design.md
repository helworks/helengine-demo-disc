# Navigator Input Class Design

## Goal

Allow Navigator's authored navigation to choose an explicit physical input transport without changing the semantic route graph.

## Contract

Add an optional `inputClass` field to Navigator's navigation request and MCP `navigate_session` tool. Its values are:

- `auto` (default): preserve the existing composite keyboard plus virtual-controller delivery.
- `keyboard`: deliver resolved navigation controls only as Windows keyboard input.
- `gamepad`: deliver resolved navigation controls only through the virtual Xbox controller.

Unknown input classes are rejected as invalid requests.

## Behavior

Profiles continue to author semantic actions such as `navigate-next`, `accept`, and `back`. Navigator resolves those to its normalized control tokens before selecting a transport. The route planner, recognition loop, retry behavior, and target verification are unchanged.

`gamepad` must never invoke the Windows keyboard emitter. This makes it safe for PPSSPP and other emulator targets where a desktop hotkey can open an emulator-owned menu.

## PSP Route Coverage

The PSP runner uses `navigate_session` with `inputClass: gamepad` for every authored transition. It removes its direct Back-key recovery: if the current state cannot be recognized as the planned route origin, it reports the blocked state without sending input.

## Compatibility

Existing Navigator clients omit `inputClass` and therefore retain `auto`. No existing profile format changes are required.

## Verification

Unit tests prove that each input class selects exactly its intended emitter, default requests remain `auto`, and invalid values fail validation. The PSP route-runner contract proves it requests `gamepad` and contains no raw key recovery. An automated HelenUI PSP sweep remains the end-to-end validation.
