# Tilt Trial Platform Action Prompts Implementation Plan

## Goal

Use the existing generated control-icon/action-button system for the Tilt Trial level selector's bottom-right Play and Menu prompts.

## Files

- Modify `assets/codebase/game.tools/GameSceneFactory.cs` to author two prompt sprites and action labels using `GeneratedControlIconAssetResolver` and platform overrides.
- Modify `assets/codebase/game/TiltTrialLevelSelectComponent.cs` to bind the prompt entities without retaining the obsolete combined hint text.
- Modify `assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs` with focused generator/runtime source assertions.
- Regenerate `assets/scenes/games/tilt/tilt_trial.helen` through the existing scene generator.

## Steps

1. Add failing focused source assertions for two bottom-right prompt entities, generated control icon resolution, platform overrides, and removal of `Enter Play   Esc Menu`.
2. Add a generator helper that creates one prompt sprite plus its action label, resolves Windows `enter`/`escape` icons, and persists the existing platform-specific icon overrides for supported platforms.
3. Replace the runtime's combined hint binding with bindings for the two prompt sprite entities; keep action labels fixed as `PLAY` and `MENU`.
4. Run the focused source test and fix only failures caused by this change.
5. Regenerate the Tilt Trial scene and run the focused generated-code test filter.

## Validation

```powershell
dotnet test "C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj" --no-restore --filter "FullyQualifiedName~TiltTrialLevelSelect"
```

The existing full filter may contain unrelated stale assertions; report those separately if they remain.
