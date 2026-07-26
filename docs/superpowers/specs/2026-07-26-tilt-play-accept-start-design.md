# Tilt Play Accept Start Gate

## Goal

Each Tilt Play level waits at a dedicated start state instead of beginning gameplay as soon as the scene loads. The start presentation contains only the text `Press "X" to start`.

## Runtime behavior

`TiltTrialSessionComponent` gains a start state that becomes the initial state after runtime dependencies resolve. While that state is active, the session suppresses gameplay updates: the timer does not advance, coin collection and goal checks do not run, and the player remains frozen through the existing gameplay-update suppression path.

Only the existing Accept input transitions the session from the start state to `Playing`. On PSP this is the X button. No navigation, retry, level-select, or back action is available from the start state.

## Presentation

`GameSceneFactory` adds a generated start overlay to both the console and handheld Tilt Play gameplay UI hierarchies. The overlay has one text element with the exact label `Press "X" to start`. It is visible only during the new start state; existing result and failure overlays remain unchanged.

## Data flow

The session owns the state transition and controls the generated overlay through the same dependency-resolution and presentation-refresh pattern used for result and failure overlays. The scene factory owns only the presentation hierarchy. This keeps input and timer authority in the session rather than in a presentation component.

## Validation

Source and unit tests cover the initial state, Accept-only transition, gameplay suppression before Accept, and generated start-overlay roles/text for console and handheld presentations. A PSP package build verifies the generated native runtime compiles.
