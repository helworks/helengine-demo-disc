# Tilt Trial Handheld Completion Menu Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task with verification checkpoints.

**Goal:** Add a cook-time DS/3DS bottom-screen completion menu with Retry, Exit-to-level-select, and Next controls after a Tilt Trial goal is reached.

**Architecture:** Keep `TiltTrialSessionComponent` as the single owner of completion state, selection, and scene destinations. Extend the generated handheld presentation with three stable-role button entities, each carrying an `InteractableComponent` and `TiltTrialPresentationActionComponent`; update their visual state from the session controller while preserving keyboard/gamepad navigation.

**Tech Stack:** C#, xUnit, generated Helengine scene/Blueprint authoring, existing `RoundedRectComponent`, `TextComponent`, `InteractableComponent`, and semantic Tilt Trial action components.

---

## File Map

- Modify `assets/codebase/game.tools/GameSceneFactory.cs`: author the handheld results panel and three semantic action buttons in the handheld presentation Blueprint.
- Modify `assets/codebase/game/TiltTrialSessionComponent.cs`: resolve result buttons, update selected-button visuals, and keep Retry/Next/LevelSelect behavior consistent.
- Modify `assets/codebase/gameplay.tests/TiltTrialSessionComponentTests.cs`: cover final-level progression fallback and completion-menu source wiring.
- Modify `assets/codebase/game.tools.tests/TiltTrialPlatformPresentationSourceTests.cs`: verify generated handheld buttons and action bridges.
- Modify `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`: verify the factory emits the completion controls.
- Regenerate `assets/blueprints/games/tilt/TiltTrialHandheldPresentation.hblueprint` through the existing project generation command only; never hand-edit the serialized Blueprint or authored gameplay `.helen` files.

## Task 1: Add failing tests for handheld completion controls

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialPlatformPresentationSourceTests.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialSceneGenerationSourceTests.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\TiltTrialSessionComponentTests.cs`

- [x] Add source assertions that `GameSceneFactory.cs` contains stable result roles `TiltTrialResultRetryButton`, `TiltTrialResultExitButton`, and `TiltTrialResultNextButton`, creates `InteractableComponent`, and assigns `TiltTrialSessionAction.Retry`, `TiltTrialSessionAction.LevelSelect`, and `TiltTrialSessionAction.Next`.
- [x] Add assertions that `TiltTrialSessionComponent.cs` resolves all three result button roles and applies selected and idle colors.
- [x] Add a test that `ResolveNextSceneId("tilt-trial-05", LevelSelectSceneId)` returns `LevelSelectSceneId`, preserving the final-level `Next` behavior required by the menu.
- [x] Run the focused demodisc tests and verify the new source assertions fail because the factory still creates only the text-only results body.

Run:
```powershell
rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj' --no-restore --nologo --filter 'FullyQualifiedName~TiltTrialPlatformPresentationSourceTests|FullyQualifiedName~TiltTrialSceneGenerationSourceTests'; dotnet test 'C:\dev\helprojs\demodisc\user_settings\generated_code\projects\gameplay.tests\gameplay.tests.csproj' --no-restore --nologo --filter 'FullyQualifiedName~TiltTrialSessionComponentTests'"
```

Expected: the new source assertions fail; existing unrelated tests must remain diagnosable rather than being hidden.

## Task 2: Generate the DS/3DS completion menu

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs`

- [x] Replace the handheld results body-only layout with a compact results panel containing the clear title, a time/medal text area, and three vertically stacked buttons sized for the 256x192 bottom-screen reference canvas.
- [x] Add a focused `CreateTiltTrialResultActionButton` method beside the existing level-select button helper. It must create a rounded rectangle, set its `InteractableComponent.Size`, add `TiltTrialPresentationActionComponent` with the supplied semantic action, add a stable `TiltTrialPresentationRoleComponent` through the existing panel helper, and create a centered text child.
- [x] Use these exact roles and actions: `TiltTrialResultRetryButton` with `Retry`, `TiltTrialResultExitButton` with `LevelSelect`, and `TiltTrialResultNextButton` with `Next`.
- [x] Keep the completion panel disabled at authoring time and leave the normal HUD unchanged while the level is playing.
- [x] Keep the generated Blueprint path and platform attachment flow unchanged; do not add any `.helen` write or runtime UI allocation.

## Task 3: Bind result buttons to session state

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialSessionComponent.cs`

- [x] Add documented fields for the three result button entities and their `RoundedRectComponent` backgrounds.
- [x] During dependency resolution, locate each button with `TryFindNamedEntity(ResultsOverlayEntity, role)` and resolve its rounded rectangle component. Throw the existing-style dependency error after the configured deferral window if the handheld results presentation is present but incomplete.
- [x] Update `RefreshOverlayPresentation` to enable the results overlay only for `Results`, preserve its dynamic title/body data, and apply selected/idle fill, border, and label colors to the three buttons.
- [x] Update `UpdateResultsOverlay` so D-pad, keyboard, and gamepad navigation wraps over exactly three options, and Accept still dispatches the selected semantic behavior. Keep Return/Back mapped to level select.
- [x] Keep `RequestResultsAction` aligned with the same three options so generated pointer/touch presses use the exact destinations as controller navigation.
- [x] Retain the existing final-level fallback through `ResolveNextSceneId`; do not duplicate catalog traversal in presentation code.
- [x] Preserve failure overlay behavior unless the shared button helper requires a compile-only adjustment.

## Task 4: Make the tests pass

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialPlatformPresentationSourceTests.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialSceneGenerationSourceTests.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\TiltTrialSessionComponentTests.cs`

- [x] Run the focused tests from Task 1 and correct only implementation or test assumptions that contradict the approved design.
- [x] Add a source assertion that authored gameplay scene generation remains absent from `GameSceneGenerator.cs` and that the result controls are authored only in the presentation factory.
- [x] Run `git diff --check` for all modified files.

## Task 5: Regenerate and verify the handheld Blueprint

**Files:**
- Regenerated: `C:\dev\helprojs\demodisc\assets\blueprints\games\tilt\TiltTrialHandheldPresentation.hblueprint`

- [x] Run the existing deterministic game-scene generation command from the demodisc project using the editor tooling that already generates presentation Blueprints.
- [x] Verify the generated diff contains only the handheld presentation Blueprint and expected source/test changes; authored `assets/scenes/games/tilt/tilt_trial_level_*.helen` files must remain byte-for-byte untouched.
- [x] Run the focused gameplay and game-tools test projects again after regeneration.
- [ ] DS build was not run in this pass; when attempted, report any existing packaging blocker separately from this feature and do not bypass Blueprint validation or modify the scene to make the build pass.

## Verification Checklist

- [x] Reaching the flag enters the existing results state and freezes the player.
- [x] Bottom screen shows three large controls: Retry, Exit, Next.
- [x] D-pad/left stick changes selection and the selected button is visibly distinct.
- [x] Accept activates Retry, Exit-to-level-select, or Next.
- [x] Back/Return exits to level select.
- [x] Final-level Next returns to level select.
- [x] No authored gameplay `.helen` scene is modified.
