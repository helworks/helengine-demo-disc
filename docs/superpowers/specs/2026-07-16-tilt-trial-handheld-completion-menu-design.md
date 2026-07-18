# Tilt Trial Handheld Completion Menu

## Goal

When a player reaches a Tilt Trial goal flag on DS or 3DS, the handheld bottom screen must present an actionable completion menu instead of leaving the player without retry, progression, or exit controls.

## Scope

The feature applies to the handheld Tilt Trial gameplay presentation Blueprint. It does not modify authored gameplay `.helen` scene geometry or add runtime scene-generation work. The existing `TiltTrialSessionComponent` remains the authority for completion state, input semantics, and scene transitions.

## Behavior

- Reaching the goal flag enters the existing results state and freezes gameplay.
- The bottom screen replaces the gameplay HUD with a completion panel containing `Retry`, `Exit`, and `Next`.
- `Exit` loads the Tilt Trial level-select scene.
- `Retry` reloads the current level.
- `Next` loads the next catalog level; after the final level it returns to level select.
- D-pad and left-stick navigation move the selected option.
- Accept activates the selected option; Return/Back exits to level select.

## Architecture

The handheld presentation factory will author the completion panel and three large selectable controls during cook-time Blueprint generation. Each control will emit a presentation-independent `TiltTrialSessionAction` through the existing semantic action bridge. The session controller will continue to own selection state and destination resolution, preventing UI hierarchy order from becoming gameplay logic.

The normal bottom-screen HUD remains available while the session is playing. The completion panel is hidden until the session enters results, then the session updates its title, clear time, medal, and selected-option visuals. Failure behavior remains unchanged unless shared control wiring requires a narrowly scoped correction.

## Validation

- Unit-test result destination behavior, including final-level `Next` fallback.
- Source-test handheld generation for the three controls, their semantic actions, and stable presentation names.
- Run the focused demodisc gameplay and game-tools test projects.
- Regenerate only the presentation Blueprint assets through the existing generator if required; do not rewrite authored level scenes.

## Out Of Scope

- A generic engine-wide completion-menu framework.
- Runtime allocation of the menu after the goal is reached.
- Changes to Windows presentation layout.
- Changes to level geometry, goal-flag assets, or authored gameplay scene files.
