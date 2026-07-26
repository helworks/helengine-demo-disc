# Tilt Play Accept Start Gate Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make every Tilt Play level show only `Press "X" to start` and begin gameplay exclusively after Accept/X.

**Architecture:** Extend the existing `TiltTrialSessionComponent` state machine with a `Start` state that suppresses existing gameplay updates until Accept. Generate a matching `TiltTrialStartOverlay` in the console and handheld presentation roots; session presentation logic owns its visibility.

**Tech Stack:** C#, helengine scene components, Demo Disc scene factory, xUnit source/unit tests.

---

### Task 1: Add the session start state

**Files:**
- Modify: `assets/codebase/game/TiltTrialSessionState.cs`
- Modify: `assets/codebase/game/TiltTrialSessionComponent.cs`
- Modify: `assets/codebase/gameplay.tests/TiltTrialSessionComponentTests.cs`

- [ ] **Step 1: Write the failing state-machine test**

```csharp
[Fact]
public void Build_state_machine_starts_waiting_for_accept_and_transitions_to_playing() {
    FiniteStateMachine<TiltTrialSessionState> machine = TiltTrialSessionComponent.CreateStateMachine();
    machine.Initialize(TiltTrialSessionState.Start);
    bool changed = machine.TryChangeState(TiltTrialSessionState.Playing);
    Assert.True(changed);
    Assert.Equal(TiltTrialSessionState.Playing, machine.CurrentState);
}
```

- [ ] **Step 2: Run the focused test**

Run: `dotnet test assets/codebase/gameplay.tests/gameplay.tests.csproj --filter FullyQualifiedName~Build_state_machine_starts_waiting_for_accept_and_transitions_to_playing`

Expected: FAIL because `Start` is unavailable.

- [ ] **Step 3: Add the state and Accept-only transition**

```csharp
Start = 0,
Playing = 1,
Paused = 2,
Results = 3,
Failed = 4
machine.RegisterState(TiltTrialSessionState.Start, new FiniteStateDefinition<TiltTrialSessionState>());
SessionStateMachine.Initialize(TiltTrialSessionState.Start);
SetGameplayUpdatesSuppressed(true);

if (SessionStateMachine.CurrentState == TiltTrialSessionState.Start) {
    if (WasAcceptPressed()) {
        SetGameplayUpdatesSuppressed(false);
        SessionStateMachine.TryChangeState(TiltTrialSessionState.Playing);
    }
    return;
}
```

The start branch must ignore Return, navigation, retry, level-select, and all other actions.

- [ ] **Step 4: Run the session suite**

Run: `dotnet test assets/codebase/gameplay.tests/gameplay.tests.csproj --filter FullyQualifiedName~TiltTrialSessionComponentTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add assets/codebase/game/TiltTrialSessionState.cs assets/codebase/game/TiltTrialSessionComponent.cs assets/codebase/gameplay.tests/TiltTrialSessionComponentTests.cs
git commit -m "Add Tilt Play accept start state"
```

### Task 2: Generate and bind the start prompt

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneFactory.cs`
- Modify: `assets/codebase/game/TiltTrialSessionComponent.cs`
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Write failing source assertions**

```csharp
Assert.Contains("TiltTrialStartOverlay", source, StringComparison.Ordinal);
Assert.Contains("Press \"X\" to start", source, StringComparison.Ordinal);
Assert.Contains("StartOverlayEntity.Enabled = SessionStateMachine.CurrentState == TiltTrialSessionState.Start", sessionSource, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the focused source test**

Run: `dotnet test assets/codebase/game.tools.tests/game.tools.tests.csproj --filter FullyQualifiedName~Game_scene_factory_authors_level_settings_and_session_components`

Expected: FAIL because no start overlay exists.

- [ ] **Step 3: Add the console and handheld overlays**

```csharp
Entity startOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialStartOverlay", new float3(16f, 58f, 0f), new int2(224, 68), 6f, 2f, new byte4(18, 27, 43, 245), new byte4(255, 214, 138, 255), 4);
CreateUiTextEntity(startOverlayEntity, "TiltTrialStartPromptText", new float3(12f, 22f, 0.1f), "Press \"X\" to start", new int2(200, 24), 0.72f, 5, new byte4(255, 236, 196, 255), TextAlignment.Center);

Entity consoleStartOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialStartOverlay", new float3(320f, 260f, 0f), new int2(640, 150), 28f, 3f, new byte4(18, 27, 43, 238), new byte4(255, 214, 138, 255), 4);
CreateUiTextEntity(consoleStartOverlayEntity, "TiltTrialStartPromptText", new float3(36f, 48f, 0.1f), "Press \"X\" to start", new int2(568, 48), 2f, 5, new byte4(255, 236, 196, 255), TextAlignment.Center);
```

Add one start overlay to each existing presentation root. Resolve it as `StartOverlayEntity` in the session and make it visible only in `Start`. Existing results and failure overlays remain unchanged.

- [ ] **Step 4: Run focused tests**

Run: `dotnet test assets/codebase/game.tools.tests/game.tools.tests.csproj --filter FullyQualifiedName~TiltTrialSceneGenerationSourceTests`

Run: `dotnet test assets/codebase/gameplay.tests/gameplay.tests.csproj --filter FullyQualifiedName~TiltTrialSessionComponentTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add assets/codebase/game.tools/GameSceneFactory.cs assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs assets/codebase/game/TiltTrialSessionComponent.cs
git commit -m "Add Tilt Play start prompt overlay"
```

### Task 3: Verify PSP packaging

**Files:**
- Verify only: `assets/codebase/game/**`
- Verify only: `assets/codebase/game.tools/**`

- [ ] **Step 1: Build PSP**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform psp -Output C:\dev\helprojs\output\psp-tilt-update-lifecycle`

Expected: a newly timestamped `PSP\GAME\HELENGINE\EBOOT.PBP`.

- [ ] **Step 2: Launch PPSSPP**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\launch_in_emulator.ps1 -ArtifactPath C:\dev\helprojs\output\psp-tilt-update-lifecycle\PSP\GAME\HELENGINE\EBOOT.PBP`

Expected: the new package opens and each Tilt Play level waits on `Press "X" to start`.
