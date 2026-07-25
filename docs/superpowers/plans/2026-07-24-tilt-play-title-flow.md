# Tilt Play Title Flow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an FSM-driven Tilt Play title screen that leads into the current level selector without changing gameplay session flow.

**Architecture:** The generated `tilt_trial` shell owns `TiltPlayMenuComponent` and three panel roots. Its `FiniteStateMachine<TiltPlayMenuState>` switches title, placeholder options, and the existing level selector. The selector continues to choose and launch levels; the in-level session FSM is untouched.

**Tech Stack:** C#, HelEngine components/entities, generated authoring scenes, xUnit.

---

## File structure

- Create `assets/codebase/game/TiltPlayMenuState.cs`: title-shell state enum.
- Create `assets/codebase/game/TiltPlayMenuAction.cs`: shared title-shell actions.
- Create `assets/codebase/game/TiltPlayMenuComponent.cs`: state, panel visibility, focus, and input routing.
- Create `assets/codebase/game/TiltPlayMenuActionComponent.cs`: pointer action adapter.
- Create `assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs`: FSM behavior tests.
- Modify `assets/codebase/game.tools/GameSceneFactory.cs`: emits the shell, title, options, and existing selector panels.
- Modify `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`: generator contract.
- Modify `assets/codebase/game/TiltTrialLevelSelectComponent.cs`: `AcceptsInput` gate.

### Task 1: Define the title-shell FSM contract

**Files:**

- Create: `assets/codebase/game/TiltPlayMenuState.cs`
- Create: `assets/codebase/game/TiltPlayMenuAction.cs`
- Create: `assets/codebase/game/TiltPlayMenuComponent.cs`
- Create: `assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs`

- [ ] Write failing tests for the initial state and title actions.

```csharp
[Fact]
public void CreateStateMachine_starts_at_title_when_initialized() {
    FiniteStateMachine<TiltPlayMenuState> machine = TiltPlayMenuComponent.CreateStateMachine();
    machine.Initialize(TiltPlayMenuState.Title);
    Assert.Equal(TiltPlayMenuState.Title, machine.CurrentState);
}

[Fact]
public void ResolveActionState_routes_play_and_options_to_their_panels() {
    Assert.Equal(TiltPlayMenuState.LevelSelect, TiltPlayMenuComponent.ResolveActionState(TiltPlayMenuAction.Play));
    Assert.Equal(TiltPlayMenuState.Options, TiltPlayMenuComponent.ResolveActionState(TiltPlayMenuAction.Options));
}
```

- [ ] Run `dotnet test C:\dev\helprojs\demodisc\city.sln --no-restore --filter FullyQualifiedName~TiltPlayMenuComponentTests` and confirm compilation fails because the types do not exist.
- [ ] Add the two enum files with `Title`, `Options`, `LevelSelect` states and `Play`, `Options`, `BackToDemoDisc`, `Back` actions. Add `CreateStateMachine` and `ResolveActionState` to the component, with substantive XML documentation for every member.
- [ ] Rerun the focused test and confirm it passes.
- [ ] Commit only these four files as `feat: add Tilt Play menu state contract`.

### Task 2: Route input and pointer actions without leaking into level select

**Files:**

- Modify: `assets/codebase/game/TiltPlayMenuComponent.cs`
- Create: `assets/codebase/game/TiltPlayMenuActionComponent.cs`
- Modify: `assets/codebase/game/TiltTrialLevelSelectComponent.cs`
- Test: `assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs`

- [ ] Write failing tests for returning to Title from Options and LevelSelect and for accepting selector input only in LevelSelect.

```csharp
[Fact]
public void ResolveBackState_returns_title_from_submenus() {
    Assert.Equal(TiltPlayMenuState.Title, TiltPlayMenuComponent.ResolveBackState(TiltPlayMenuState.Options));
    Assert.Equal(TiltPlayMenuState.Title, TiltPlayMenuComponent.ResolveBackState(TiltPlayMenuState.LevelSelect));
}

[Fact]
public void ShouldLevelSelectorProcessInput_is_only_true_in_level_select_state() {
    Assert.False(TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(TiltPlayMenuState.Title));
    Assert.False(TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(TiltPlayMenuState.Options));
    Assert.True(TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(TiltPlayMenuState.LevelSelect));
}
```

- [ ] Run the focused test and confirm it fails for missing methods.
- [ ] Implement named required-panel resolution, visible-panel selection, selected-action feedback, and keyboard/gamepad navigation in `TiltPlayMenuComponent`. Give `TiltTrialLevelSelectComponent` an `AcceptsInput` property and make `Update` return before polling input when it is false.
- [ ] Implement `TiltPlayMenuActionComponent` using the existing press/release-inside `InteractableComponent` subscription pattern; it calls `HandleAction` on the nearest menu component.
- [ ] For Back to Demo Disc, call `DemoDiscMainMenuSceneResolver.ResolveRuntimeSceneId()` and `SceneManager.RequestSceneTransition`. Do not leave `DemoDiscReturnToMenuComponent` on the selector root, because it would consume Back before the title FSM.
- [ ] Rerun the focused test and confirm it passes.
- [ ] Commit these interaction files as `feat: route Tilt Play title menu input`.

### Task 3: Generate the Tilt Play shell from source

**Files:**

- Modify: `assets/codebase/game.tools/GameSceneFactory.cs`
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`
- Regenerate: `assets/scenes/games/tilt/tilt_trial.helen`

- [ ] Add a failing source-contract test proving the factory emits `CreateTiltPlayShellUiEntity()`, `TiltPlayTitlePanel`, `TILT PLAY`, `TiltPlayOptionsPanel`, `Settings coming soon`, and `TiltPlayLevelSelectPanel`.
- [ ] Run `dotnet test C:\dev\helprojs\demodisc\city.sln --no-restore --filter FullyQualifiedName~TiltTrialSceneGenerationSourceTests` and confirm the new assertion fails.
- [ ] Change `CreateTiltTrialScene()` to generate the shell root. Attach `TiltPlayMenuComponent` to it and create:

```csharp
Entity titlePanel = CreateRoundedPanelEntity(shell, "TiltPlayTitlePanel", new float3(0f, 0f, 0f), new int2(1280, 720), 0f, 0f, new byte4(18, 29, 45, 255), new byte4(18, 29, 45, 255), 1);
CreateUiTextEntity(titlePanel, "TiltPlayTitle", new float3(240f, 220f, 0.1f), "TILT PLAY", new int2(800, 110), 4.5f, 3, new byte4(247, 248, 252, 255), TextAlignment.Center);
Entity optionsPanel = CreateRoundedPanelEntity(shell, "TiltPlayOptionsPanel", new float3(0f, 0f, 0f), new int2(1280, 720), 0f, 0f, new byte4(18, 29, 45, 255), new byte4(18, 29, 45, 255), 1);
Entity levelSelectPanel = CreateLevelSelectUiEntity();
```

- [ ] Add Play, Options, and Back to Demo Disc action hosts as a lower-middle title group. Add a `Settings coming soon` label and a Back host in Options. Each host receives `InteractableComponent` and `TiltPlayMenuActionComponent`. Options and level-select begin disabled.
- [ ] Preserve the current selector hierarchy nested under `TiltPlayLevelSelectPanel`; remove only its direct `DemoDiscReturnToMenuComponent` attachment. Do not directly edit the generated `.helen` file.
- [ ] Rerun the generator source test and confirm it passes.
- [ ] Regenerate with `dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\project.heproj --command menu.generate-game-scenes`.
- [ ] Commit the factory, generator test, and regenerated scene as `feat: add Tilt Play title shell`.

### Task 4: Verify the packaged Windows flow

**Files:**

- Verify: `assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs`
- Verify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`
- Build: `output/windows/helengine_windows.exe`

- [ ] Run `dotnet test C:\dev\helprojs\demodisc\city.sln --no-restore --filter "FullyQualifiedName~TiltPlayMenuComponentTests|FullyQualifiedName~TiltTrialSceneGenerationSourceTests"` and confirm all selected tests pass.
- [ ] Build with `scripts\build-platform.ps1 -Platform windows -ProjectRoot C:\dev\helprojs\demodisc -Profile debug -OutputRoot C:\dev\helprojs\demodisc\output\windows` and confirm `helengine_windows.exe` exists.
- [ ] Launch the player and verify: title starts focused on Play; Play exposes unchanged level selection; Back from selection returns to title; Options shows the placeholder and returns to title; Back to Demo Disc returns to the main menu.
- [ ] If manual verification reveals a defect, write a focused failing test first, make the minimum source-generator/component change, rerun this task, then commit only that follow-up.
