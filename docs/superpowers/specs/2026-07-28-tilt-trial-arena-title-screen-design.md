# Tilt Trial Arena Title Screen

## Goal

Give the Tilt Trial front-door title screen a loud, playful game-show identity inspired by Super Monkey Ball, while preserving the existing title-menu actions and transition into the level selector.

## Scope

This change applies only to the desktop-style Tilt Trial title panel shown before the level list. It does not change the selector, gameplay scenes, menu state machine, action semantics, or input mappings.

## Visual composition

- Replace the flat navy title-panel presentation with a deep navy arena backdrop.
- Add a large off-center radial burst built from teal, purple, and warm-yellow UI shapes, plus subtle concentric rings that imply a rolling arena.
- Present a large, slightly angled `TILT TRIAL` title in warm yellow with a dark-purple offset shadow.
- Add the small subtitle `THE ROLLING CHALLENGE` above the title.
- Add one glossy marble and a small set of floating course-piece silhouettes behind the title. These are decorative UI elements and must not cover readable text or controls.

## Menu layout and focus

- Keep the existing three actions: Play, Options, and Back to Demo Disc.
- Make Play the dominant wide yellow hero button beneath the title.
- Render Options and Back to Demo Disc as compact secondary buttons below the hero action.
- Keep pointer interaction, keyboard navigation, gamepad navigation, and current state transitions unchanged.
- Focused actions use a bright magenta outline and a compact chevron marker. The treatment must remain legible against every background decoration.

## Constraints

- Use existing generated-scene UI primitives and assets where possible; do not introduce generated-output edits.
- Preserve the current 1280x720 reference-canvas behavior and responsive canvas fitting.
- Keep all text and button hit targets within the safe visual area at the reference resolution.
- Avoid visual changes to the existing level selector during this task.

## Validation

- Update focused source-contract tests to assert the title-screen hierarchy, visible labels, primary/secondary button styling, and unchanged action wiring.
- Regenerate only the Tilt Trial front-door scene through the existing editor command.
- Run the focused generated-code test project for the changed source contracts.
- Build the Windows package after the visual change and verify that `helengine_windows.exe` is emitted.
