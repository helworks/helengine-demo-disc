# Tilt Play title-flow design

## Goal

Make Tilt Play feel like a game entry rather than an immediate list of levels. Opening Tilt Play presents a title screen before the existing level selector.

## Scope

- The existing `tilt_trial` entry scene becomes the Tilt Play shell.
- A dedicated FSM-driven menu controller owns three states: `Title`, `Options`, and `LevelSelect`.
- `Title` shows a large bold `TILT PLAY` title centered on screen and three actions at the lower middle: `Play`, `Options`, and `Back to Demo Disc`.
- `Play` reveals the existing level-select UI without changing its selection or level-start behavior.
- `Options` reveals a placeholder panel that clearly says settings are coming soon and provides a Back action.
- `Back to Demo Disc` uses the existing return-to-menu route.
- The current level-select controller remains responsible only for choosing and launching levels.
- The in-level `TiltTrialSessionComponent` FSM remains unchanged.

## Architecture

`TiltPlayMenuComponent` is attached to the generated Tilt Play shell UI root. It owns `FiniteStateMachine<TiltPlayMenuState>` and resolves three generated presentation panels:

- `Title`: title and three action buttons.
- `Options`: placeholder copy and Back action.
- `LevelSelect`: the existing selector hierarchy.

State transitions only change panel visibility, focused action presentation, and input routing. They do not load a scene. Starting a selected level remains the level selector's existing scene transition.

## Input and transitions

| State | Accept | Back | Navigation |
| --- | --- | --- | --- |
| Title | Activates Play, Options, or Back to Demo Disc | Returns to Demo Disc | Moves among the three actions |
| Options | Activates Back | Returns to Title | Focus remains on Back |
| LevelSelect | Existing selector behavior | Returns to Title | Existing selector behavior |

Pointer actions invoke the same state transitions as controller and keyboard input.

## Presentation

The title is large, bold, and centered. The three title actions are visually grouped at the lower middle of the screen. The initial selected action is Play. The level-select panel is hidden at startup, preserving its authored content until Play is chosen.

## Error handling

The menu controller throws a descriptive exception if a required generated panel, action, or presentation component is absent. It does not create fallback UI at runtime.

## Testing

- Source/behavior tests prove the FSM begins in `Title`.
- Tests prove Play enters `LevelSelect`, Options enters `Options`, and both Back routes return to the expected state.
- Tests prove the Demo Disc action uses the established return route.
- Generation tests prove title, options, and level-select panels are emitted in the `tilt_trial` shell.
- Existing level-select and session tests continue to pass unchanged.

## Explicit non-goals

- No real settings persistence or settings controls.
- No changes to level gameplay, lighting, materials, physics, or Tilt Trial session states.
- No generated scene-file edits; the scene generator remains the source of truth.
