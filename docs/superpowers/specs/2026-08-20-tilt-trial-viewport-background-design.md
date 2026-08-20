# Tilt Trial Viewport Background

## Goal

Make the Tilt Trial title backdrop fill the live display at 4:3, matching the DemoDisc main menu, while retaining the current 16:9 reference canvas for title text and controls.

## Scope

The change applies only to the non-handheld Tilt Play title screen. It does not alter the level selector, gameplay scenes, DS/3DS presentation, input, or title-menu state transitions.

## Design

- Add a screen-bound, lowest-order background sprite for the existing Tilt Trial title image.
- Place that sprite outside the `1280x720` `ReferenceCanvasFitComponent` hierarchy so it covers the full live viewport and may stretch horizontally on a 4:3 display.
- Keep `TiltPlayShellUi`, the title panel, title text, and all action buttons within the current fitted `1280x720` canvas.
- Remove the duplicate fitted-canvas title-background sprite so the image is drawn exactly once.

## Expected behavior

- At 16:9, the visual composition remains equivalent to the current title screen.
- At 4:3, the backdrop reaches both viewport edges, while controls preserve their current 16:9-safe placement and proportions.
- Handheld scenes remain unaffected.

## Validation

- Add a focused source-contract test covering the screen-bound background root, its full-viewport layout behavior, and the retained fitted UI shell.
- Run that focused test before and after the implementation.
- Regenerate the Tilt Trial scene, build the GameCube package, and boot the resulting disc in Dolphin for visual confirmation.
