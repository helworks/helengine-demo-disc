# Tilt Trial Arena Title Screen Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the flat Tilt Trial title panel with a loud Super Monkey Ball-inspired arena screen while preserving all existing menu actions and transitions.

**Architecture:** Keep the change in `GameSceneFactory`, which owns the generated Tilt Trial front-door hierarchy. Introduce a small title-screen backdrop builder and extend the title-button factory with explicit primary and secondary visual styles; `TiltPlayMenuComponent` keeps owning focus and action routing, with only its colours updated to match the new palette.

**Tech Stack:** C#, HelEngine generated authoring scenes, `RoundedRectComponent`, `TextComponent`, xUnit source-contract tests, editor scene generation command, PowerShell Windows build script.

---

## File structure

- Modify: `assets/codebase/game.tools/GameSceneFactory.cs` — create the arena backdrop, title typography, decorative marble/course silhouettes, and primary/secondary title-menu button layout.
- Modify: `assets/codebase/game/TiltPlayMenuComponent.cs` — retain action wiring while changing selected and unselected button colours to the arena palette.
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs` — add source contracts for the arena title hierarchy and unchanged actions.
- Modify: `assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs` — no change expected; run it to prove title work has not altered the selector contract.

### Task 1: Lock down the arena title-screen contract

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Write the failing source-contract test**

Add this test beside `Tilt_trial_front_door_generates_title_options_and_level_select_panels`:

```csharp
/// <summary>
/// Ensures the Tilt Trial front door emits the approved game-show arena title treatment without changing menu actions.
/// </summary>
[Fact]
public void Tilt_trial_front_door_generates_the_arena_title_treatment() {
    string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

    Assert.Contains("CreateTiltPlayArenaBackdrop(titlePanel);", source, StringComparison.Ordinal);
    Assert.Contains("\"THE ROLLING CHALLENGE\"", source, StringComparison.Ordinal);
    Assert.Contains("\"TiltPlayArenaMarble\"", source, StringComparison.Ordinal);
    Assert.Contains("\"TiltPlayPlayButton\"", source, StringComparison.Ordinal);
    Assert.Contains("new int2(520, 72)", source, StringComparison.Ordinal);
    Assert.Contains("TitleButtonStyle.Primary", source, StringComparison.Ordinal);
    Assert.Contains("TitleButtonStyle.Secondary", source, StringComparison.Ordinal);
    Assert.Contains("city.game.TiltPlayMenuAction.Play", source, StringComparison.Ordinal);
    Assert.Contains("city.game.TiltPlayMenuAction.Options", source, StringComparison.Ordinal);
    Assert.Contains("city.game.TiltPlayMenuAction.BackToDemoDisc", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
dotnet test "C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj" --no-restore --filter "FullyQualifiedName~Tilt_trial_front_door_generates_the_arena_title_treatment" --verbosity quiet
```

Expected: failure because the arena helper, subtitle, marble role, button size, and style enum do not yet exist.

- [ ] **Step 3: Commit the red test**

```powershell
git -C "C:\dev\helprojs\demodisc" add -- "assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs"
git -C "C:\dev\helprojs\demodisc" commit -m "test: define Tilt Trial arena title contract"
```

### Task 2: Generate the Tilt Trial arena title screen

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneFactory.cs:256-286`

- [ ] **Step 1: Add explicit title-button visual styles**

Add this nested enum near the other `GameSceneFactory` private declarations:

```csharp
/// <summary>
/// Identifies the visual emphasis applied to one Tilt Play title-menu action.
/// </summary>
enum TitleButtonStyle {
    /// <summary>
    /// Draws the dominant Play call to action.
    /// </summary>
    Primary,

    /// <summary>
    /// Draws one supporting title-menu action.
    /// </summary>
    Secondary
}
```

- [ ] **Step 2: Replace the flat title-panel content with the approved composition**

In `CreateTiltPlayShellUiEntity`, keep the existing `TiltPlayTitlePanel` name and full-screen dimensions, then use this layout:

```csharp
Entity titlePanel = CreateRoundedPanelEntity(shell, "TiltPlayTitlePanel", new float3(0f, 0f, 0f), new int2(1280, 720), 0f, 0f, new byte4(15, 19, 53, 255), new byte4(15, 19, 53, 255), 1);
CreateTiltPlayArenaBackdrop(titlePanel);
CreateUiTextEntity(titlePanel, "TiltPlaySubtitle", new float3(244f, 118f, 0.2f), "THE ROLLING CHALLENGE", new int2(560, 34), 1.05f, 3, new byte4(84, 244, 224, 255), TextAlignment.Center);
CreateUiTextEntity(titlePanel, "TiltPlayTitleShadow", new float3(184f, 154f, 0.2f), "TILT TRIAL", new int2(820, 138), 5.2f, 3, new byte4(71, 27, 111, 255), TextAlignment.Center);
CreateUiTextEntity(titlePanel, "TiltPlayTitle", new float3(172f, 144f, 0.3f), "TILT TRIAL", new int2(820, 138), 5.2f, 3, new byte4(255, 220, 62, 255), TextAlignment.Center);
CreateTiltPlayActionButton(titlePanel, "TiltPlayPlayButton", new float3(380f, 398f, 0.3f), new int2(520, 72), "PLAY", city.game.TiltPlayMenuAction.Play, TitleButtonStyle.Primary);
CreateTiltPlayActionButton(titlePanel, "TiltPlayOptionsButton", new float3(380f, 486f, 0.3f), new int2(250, 52), "OPTIONS", city.game.TiltPlayMenuAction.Options, TitleButtonStyle.Secondary);
CreateTiltPlayActionButton(titlePanel, "TiltPlayDemoDiscButton", new float3(650f, 486f, 0.3f), new int2(250, 52), "BACK TO DEMO DISC", city.game.TiltPlayMenuAction.BackToDemoDisc, TitleButtonStyle.Secondary);
```

- [ ] **Step 3: Add the backdrop builder**

Add `CreateTiltPlayArenaBackdrop(Entity parent)` beside the other UI factory helpers. It must generate named children for `TiltPlayArenaBurstTeal`, `TiltPlayArenaBurstPurple`, `TiltPlayArenaRingOuter`, `TiltPlayArenaRingInner`, `TiltPlayArenaMarble`, and `TiltPlayArenaCourseBlock`. Use `CreateRoundedPanelEntity` for burst blocks/rings and the existing circle or sprite-capable UI primitive already used by the factory for the marble. Position every decoration behind the title (`Z` below `0.2f`) and outside the 380–900 x 398–538 button footprint.

- [ ] **Step 4: Extend the button factory without changing behavior**

Change the signature to:

```csharp
Entity CreateTiltPlayActionButton(Entity parent, string name, float3 position, int2 size, string label, city.game.TiltPlayMenuAction action, TitleButtonStyle style)
```

Use the following palette selection before calling `CreateRoundedPanelEntity`:

```csharp
byte4 fillColor = style == TitleButtonStyle.Primary ? new byte4(255, 205, 44, 255) : new byte4(45, 36, 103, 255);
byte4 borderColor = style == TitleButtonStyle.Primary ? new byte4(255, 243, 151, 255) : new byte4(92, 239, 222, 255);
byte4 labelColor = style == TitleButtonStyle.Primary ? new byte4(30, 22, 63, 255) : new byte4(247, 248, 252, 255);
```

Keep `InteractableComponent` and `TiltPlayMenuActionComponent` unchanged. Use `1.55f` label scale for `Primary` and `1.0f` for `Secondary`.

- [ ] **Step 5: Regenerate only the Tilt Trial scene**

Run:

```powershell
dotnet run --no-build --project "C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj" -- --project "C:\dev\helprojs\demodisc\project.heproj" --editor-command menu.generate-tilt-trial-scene
```

Expected: `Editor command 'menu.generate-tilt-trial-scene' executed successfully.`

- [ ] **Step 6: Run the focused source contracts**

Run:

```powershell
dotnet test "C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj" --no-restore --filter "FullyQualifiedName~Tilt_trial_front_door_generates_the_arena_title_treatment|FullyQualifiedName~Tilt_trial_front_door_generates_title_options_and_level_select_panels|FullyQualifiedName~Game_scene_factory_expands_desktop_level_list_without_title" --verbosity quiet
```

Expected: all selected tests pass.

- [ ] **Step 7: Commit the generated title-screen composition**

```powershell
git -C "C:\dev\helprojs\demodisc" add -- "assets/codebase/game.tools/GameSceneFactory.cs" "assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs" "assets/scenes/games/tilt/tilt_trial.helen"
git -C "C:\dev\helprojs\demodisc" commit -m "feat: add Tilt Trial arena title screen"
```

### Task 3: Match keyboard and gamepad focus styling to the arena palette

**Files:**
- Modify: `assets/codebase/game/TiltPlayMenuComponent.cs:249-266`
- Test: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Add the failing focus-palette contract**

Add this assertion to `Tilt_trial_front_door_generates_the_arena_title_treatment`:

```csharp
string menuSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltPlayMenuComponent.cs");
Assert.Contains("new byte4(255, 53, 177, 255)", menuSource, StringComparison.Ordinal);
Assert.Contains("new byte4(45, 36, 103, 255)", menuSource, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the test and verify it fails**

Run the same focused `dotnet test` command from Task 2, Step 6.

Expected: failure because the existing purple focus values remain.

- [ ] **Step 3: Update only the visual colours in `ApplyTitleActionButtonStyle`**

Keep the method’s selection branching and null validation intact. Replace only its colour literals:

```csharp
if (isSelected) {
    background.FillColor = new byte4(255, 53, 177, 255);
    background.BorderColor = new byte4(255, 226, 255, 255);
} else {
    background.FillColor = new byte4(45, 36, 103, 255);
    background.BorderColor = new byte4(92, 239, 222, 255);
}
```

- [ ] **Step 4: Run the focused source contracts again**

Run the Task 2, Step 6 command.

Expected: all selected tests pass; action routing remains unmodified.

- [ ] **Step 5: Commit the focus treatment**

```powershell
git -C "C:\dev\helprojs\demodisc" add -- "assets/codebase/game/TiltPlayMenuComponent.cs" "assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs"
git -C "C:\dev\helprojs\demodisc" commit -m "style: match Tilt Trial menu focus to arena"
```

### Task 4: Package the validated Windows build

**Files:**
- No source changes.

- [ ] **Step 1: Build the Windows package with the short temporary root**

Run:

```powershell
$env:TEMP='C:\t'
$env:TMP='C:\t'
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\dev\helworks\helengine-windows\build-demodisc-windows.ps1"
```

Expected: `Build complete: C:\dev\helprojs\demodisc\output\windows-manual` and `Built native Windows player at 'C:\dev\helprojs\demodisc\output\windows-manual\helengine_windows.exe'`.

- [ ] **Step 2: Verify the built executable exists**

Run:

```powershell
Get-Item "C:\dev\helprojs\demodisc\output\windows-manual\helengine_windows.exe" | Select-Object FullName, Length, LastWriteTime
```

Expected: non-zero `Length` and a timestamp from the completed build.

## Plan self-review

- Spec coverage: Task 2 covers the navy arena, burst/rings, title/subtitle, marble/course silhouettes, primary/secondary layout, and preserves the current actions. Task 3 covers the magenta focus treatment. Task 4 covers the required Windows package validation.
- Placeholder scan: no TODO/TBD markers or unspecified implementation steps remain.
- Type consistency: `TitleButtonStyle` is defined in Task 2 before it is used by the extended button factory and title-panel calls; existing action types stay unchanged.
