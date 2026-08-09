# HelenUI DemoDisc Navigation Profile

## Goal

Make the Demodisc repository the source of truth for a HelenUI profile that can recognize and navigate every currently authored DemoDisc menu, showcase, Tilt Trial selector, gameplay, pause, result, failure, retry, next-level, and return path.

## Context

The existing `demodisc.json` lives in the separate HelenUI repository and describes an older, partial DemoDisc surface:

- it contains only one Tilt Trial level;
- it does not model the current Tilt Play title/options shell;
- it omits the three current PBR rendering entries;
- it does not describe all current gameplay levels or result actions;
- it is not co-located with the C# menu and gameplay contracts that define the application.

The current authoritative contracts are:

- `assets/codebase/menu.authoring/DemoDiscMenuDefinitionProvider.cs` for the main menu and Options panel;
- `assets/codebase/menu.authoring/DemoDiscSceneCatalog.cs` for rendering, physics, and game entries;
- `assets/codebase/game.tools/GameSceneFactory.cs` for the console Tilt Play shell, level selector, and gameplay presentation;
- `assets/codebase/game.tools/TiltTrialHandheldLevelSelectSceneFactory.cs` for the handheld selector;
- `assets/codebase/game/TiltTrialLevelCatalog.cs` for the five ordered levels;
- `assets/codebase/game/TiltTrialSessionComponent.cs` and its action enum for gameplay pause/result/failure behavior.

## Chosen approach

Add one static schemaVersion 7 project at:

`helenui/demodisc.json`

This file is authored directly from the current Demodisc contracts. It is not generated from C# and is not mirrored back into the separate HelenUI repository. Keeping the JSON beside the project makes profile changes reviewable with the menu/game changes that require them and avoids introducing a generator whose main remaining responsibility would be recognition metadata.

## Surface model

The profile will contain these logical surfaces:

### DemoDisc hub

- Main menu: `Demo Scenes`, `Physics Scenes`, `Games`, `Options`.
- Rendering scenes menu: all ten current rendering entries plus `Back`, including `PBR Material Gallery`, `PBR Textured Showcase`, and `PBR Shadow Theater`.
- Physics scenes menu: `Stacked Boxes`, `Sphere Stack`, `Mixed Stack`, `Static Mesh`, `Static Mesh Simple`, and `Back`.
- Games menu: `Tilt Trial` and `Back`.
- Options: `Display`, `Audio`, `Controls`, and `Back`. The three placeholder entries are modeled as selectable but non-activating controls; `Back` returns to the main menu.
- Generic showcase scene: one read-only surface with a `Back` control returning to the main menu. Each rendering/physics entry activates this surface because the runtime loads the selected showcase scene while the profile uses one shared recognition contract for showcase scenes.

### Tilt Play front door and selectors

- Console Tilt Play title: icon-backed `Play`, `Options`, and `Demo Disc` actions. The surface uses the project-local title background/button assets for recognition because the authored buttons are images rather than OCR text.
- Console Tilt Play options: `OPTIONS`, `Settings coming soon`, and `BACK`; `Back` returns to the title.
- Console combined level selector: five ordered level rows, plus selector `Back` and `Play`. The current generated console selector keeps the list and selected-level details in one view.
- Handheld level list: five ordered rows, with `TILT TRIAL` and `Preview` recognition evidence. Selecting a row opens the handheld details view.
- Handheld level details: selected level metadata (`Level N`, `Limit`, `Targets`), `BACK`, and `PLAY`. `Back` returns to the handheld list and `Play` enters the selected gameplay level.

### Tilt Trial gameplay and overlays

For each of the five catalog levels, add a gameplay surface with level-specific recognition evidence and a hidden `Pause` action bound to the profile’s `pause` input action. The profile does not model analog movement or camera controls.

Add separate result/failure surfaces for the current console and handheld presentations:

- console clear results: `Clear` and `Time`, with hidden ordered actions `Retry`, `Level Select`, and `Next`;
- handheld clear results: `Clear` and `RETRY`, with visible/ordered `RETRY`, `EXIT`, and `NEXT` actions;
- time-up failure: `Time Up` with ordered `Retry` and `Level Select` actions.

Retry routes back to the same level, `Next` routes to the next catalog level and returns to the selector after Level 5, and `Level Select`/`EXIT` routes to the appropriate selector surface. The generated runtime behavior remains authoritative; the profile only describes the controls and recognition states needed to drive it.

## Recognition and routing

- Use `text_must_appear`, `any_of_texts`, and `highlighted_text` for stable menu and HUD labels.
- Use project-relative `image_must_match` clues for the icon-only Tilt Play title background and selected button assets. Paths resolve from the profile’s Demodisc repository location under `assets/images/ui/tilt_trial/title`.
- Use distinct recognition clues for each selector and overlay so a route can confirm the resulting surface instead of relying on a scene-load assumption.
- Give each actionable node stable IDs, unique order values, and explicit default activation interactions.
- Every cross-surface interaction names its `targetSurfaceId`. Local movement interactions use `move_previous`/`move_next` and the shared keyboard action catalog maps them to Up/Down, with `accept`, `back`, and `pause` available for explicit actions.
- Use hidden buttons only for controls that are real runtime actions but are not rendered as textual controls, such as console result actions and gameplay pause.
- Keep `stateTransitions` empty because these flows are UI-owned interactions rather than automatic splash/loading transitions; keep `editor.actionRoutes` as layout metadata only.

## Validation and acceptance

The implementation is accepted when:

1. `helenui/demodisc.json` parses as schemaVersion 7 and has no duplicate IDs, duplicate node orders within a surface, or dangling `targetSurfaceId`/`inputActionId` references.
2. The profile covers all current catalog names and all five `TiltTrialLevelCatalog` entries.
3. A static route audit can reach every surface from the main menu and can return from every menu/overlay surface to its owning parent or selector.
4. HelenUI’s parser/recognition tooling accepts the profile and its project-local image paths.
5. The relevant Demodisc tests and profile validation commands pass.
6. If a runnable Windows build and NavigatorService session are available, a smoke run recognizes the hub, enters each menu branch, reaches at least one rendering scene, one physics scene, the console and handheld selectors, and exercises clear/time-up return actions. The run must verify recognized screens after transitions rather than treating sent keys as proof.

## Non-goals

- No changes to runtime C# menu/game behavior.
- No changes to the separate HelenUI repository’s sample profile.
- No generator or shared-code dependency between the Demodisc C# catalog and HelenUI JSON.
- No profile modeling of analog Tilt Trial gameplay movement, camera orbit, physics, or visual correctness of showcase scenes.
