# Tilt Trial Viewport Background Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stretch the Tilt Trial title backdrop over the live 4:3 viewport while preserving the existing fitted 16:9 title controls.

**Architecture:** The title image becomes a sibling screen-viewport root instead of a child of `TiltPlayShellUi`. Its sprite uses the existing camera-viewport `LayoutComponent` with all four edges anchored, so the engine resizes it to the live viewport. `TiltPlayShellUi` retains its existing `1280x720` viewport and reference-canvas fit, so controls stay undistorted.

**Tech Stack:** C#, Helengine scene entities/components, xUnit source-contract tests, DemoDisc generated scene command, GameCube build script, Dolphin.

---

### Task 1: Specify the scene hierarchy and viewport behavior with a failing source-contract test

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`
- Test: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Add a focused failing test**

Add this fact to `TiltTrialSceneGenerationSourceTests`:

```csharp
[Fact]
public void Tilt_play_title_background_uses_a_screen_viewport_outside_the_fitted_shell() {
    string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
    Assert.Contains("CreateTiltPlayViewportBackgroundEntity()", source, StringComparison.Ordinal);
    Assert.Contains("CreateTiltPlayShellUiEntity()", source, StringComparison.Ordinal);
    Assert.Contains("Entity CreateTiltPlayViewportBackgroundEntity()", source, StringComparison.Ordinal);
    Assert.Contains("BindingMode = ViewportComponent.ScreenBindingMode", source, StringComparison.Ordinal);
    Assert.Contains("LayoutSpace = LayoutComponent.CameraViewportLayoutSpace", source, StringComparison.Ordinal);
    Assert.Contains("SetAnchorDistances(left: 0f, top: 0f, right: 0f, bottom: 0f)", source, StringComparison.Ordinal);
    Assert.DoesNotContain("CreateTiltPlayTitleBackgroundSprite(titlePanel);", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the focused test and verify RED**

Run:

```powershell
rtk dotnet test assets\codebase\game.tools.tests\game.tools.tests.csproj --no-restore --filter FullyQualifiedName~TiltTrialSceneGenerationSourceTests.Tilt_play_title_background_uses_a_screen_viewport_outside_the_fitted_shell
```

Expected: FAIL because `CreateTiltPlayViewportBackgroundEntity()` does not yet exist, the new camera-viewport layout is absent, and the old fitted title-background call is still present.

### Task 2: Add the screen-bound background root

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneFactory.cs`
- Test: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Implement `CreateTiltPlayViewportBackgroundEntity`**

Create a `TiltPlayViewportBackground` root with `ViewportComponent.ScreenBindingMode` and a child named `TiltPlayTitleBackground`. Give the child a `SpriteComponent` using `TiltPlayTitleBackgroundTextureRelativePath` at render order `0`, then attach a camera-viewport layout anchored on all four edges:

```csharp
LayoutComponent backgroundLayoutComponent = new LayoutComponent {
    LayoutSpace = LayoutComponent.CameraViewportLayoutSpace
};
backgroundLayoutComponent.SetAnchorDistances(left: 0f, top: 0f, right: 0f, bottom: 0f);
backgroundEntity.AddComponent(backgroundLayoutComponent);
```

Persist the child texture with `TextureAssetScenePersistenceSupport.TextureReferenceName` and the existing file-system texture reference helper. Do not add runtime components or alter `TiltPlayMenuComponent`, input handling, title/menu action state, DS/3DS scene generation, or generic reference-canvas behavior.

### Task 3: Generate the background as a sibling viewport root

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneFactory.cs`
- Test: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Add the sibling root to `CreateTiltTrialScene`**

Change the root order to retain the camera first, draw the background next, and draw the fitted shell last:

```csharp
RootEntities = [
    CreateLevelSelectCameraEntity(),
    CreateTiltPlayViewportBackgroundEntity(),
    CreateTiltPlayShellUiEntity()
]
```

- [ ] **Step 2: Implement `CreateTiltPlayViewportBackgroundEntity`**

Create a `TiltPlayViewportBackground` root with `ViewportComponent.ScreenBindingMode`, a child named `TiltPlayTitleBackground`, and a `SpriteComponent` using `TiltPlayTitleBackgroundTextureRelativePath` at render order `0`. Attach the all-edge camera-viewport `LayoutComponent` to the child so it fills the root's live screen viewport. Persist the child texture through `TextureAssetScenePersistenceSupport.TextureReferenceName` exactly as `CreateTiltPlaySpriteEntity` does.

- [ ] **Step 3: Remove the fitted duplicate**

Delete `CreateTiltPlayTitleBackgroundSprite(titlePanel);` from `CreateTiltPlayShellUiEntity` and remove `CreateTiltPlayTitleBackgroundSprite` if it has no other call sites. Keep all existing title buttons and panels unchanged.

- [ ] **Step 4: Run the focused test and verify GREEN**

Run the Task 1 command again.

Expected: PASS. The contract proves the background is outside the fitted shell, screen-bound, resize-capable, and no duplicate fitted sprite remains.

### Task 4: Regenerate and validate the GameCube package

**Files:**
- Generated: `assets/scenes/games/tilt/tilt_trial.helen`

- [ ] **Step 1: Regenerate the authored Tilt Trial scene**

Run the project’s existing `Generate Tilt Trial Scene` editor command so the generated `.helen` scene receives the new root hierarchy. Do not hand-edit the generated scene file.

- [ ] **Step 2: Run the focused test project**

Run:

```powershell
rtk dotnet test assets\codebase\game.tools.tests\game.tools.tests.csproj --no-restore --filter FullyQualifiedName~TiltTrialSceneGenerationSourceTests
```

Expected: all `TiltTrialSceneGenerationSourceTests` pass.

- [ ] **Step 3: Build the GameCube package**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform gamecube -Output C:\dev\helprojs\demodisc\output\gamecube
```

Expected: exit code `0` and `C:\dev\helprojs\demodisc\output\gamecube\game.gcm` exists.

- [ ] **Step 4: Boot in Dolphin and inspect at 4:3**

Launch `game.gcm` with `C:\dev\helworks\emus\dolphin-2603a-x64\Dolphin-x64\Dolphin.exe`, set the emulated display to 4:3, and open Tilt Trial. Confirm the background fills the viewport edge-to-edge while the title and buttons retain their 16:9-safe composition.

- [ ] **Step 5: Commit implementation files**

```powershell
git add -- assets/codebase/game.tools/GameSceneFactory.cs assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs assets/scenes/games/tilt/tilt_trial.helen
git commit -m "Stretch Tilt Trial title background to viewport"
```
