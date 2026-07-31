# Tilt Trial Platform Action Prompts

## Goal

Replace the desktop Tilt Trial selector's single top hint with two bottom-right platform-action prompts using the project's existing generated control-icon system.

## Behavior

- Place separate Play and Menu prompts at the bottom-right of the selector's details panel.
- Each prompt contains the existing generated button/keyboard sprite and a text action label.
- Windows resolves to the generated keyboard `enter` and `escape` icons.
- Other supported platforms resolve through `GeneratedControlIconPlatformMap` to the corresponding generated face-button family.
- The old single hint text is removed from the generated desktop scene.

## Implementation

The scene generator follows the established `GeneratedControlIconAssetResolver` and `SpriteComponent` platform-override pattern used by `DemoSceneInstructionOverlayFactory`. Each prompt has a stable sprite entity and a stable action-label child. The right detail panel remains the owner of the prompts, and the list panel is unchanged.

## Validation

- Add source tests for the two generated prompt entities, their bottom-right positions, their resolver-backed sprite overrides, and removal of the old hint.
- Add runtime source assertions that the prompt sprites resolve to the platform action controls.
- Regenerate `assets/scenes/games/tilt/tilt_trial.helen`.
- Run the focused generated `game.tools.tests` and `gameplay.tests` filters.
