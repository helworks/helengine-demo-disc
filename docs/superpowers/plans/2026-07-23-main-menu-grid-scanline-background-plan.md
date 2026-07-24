# Main Menu Grid and Scanline Background Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a subtle, continuously animated grid and scanline layer behind the standard Demo Disc menu.

**Architecture:** The standard-menu factory authors a shared background root with repeated rounded-rectangle primitives for a grid and scanlines. `MenuBackgroundMotionComponent` owns serialized references to those roots, resolves them after scene loading, and offsets them continuously with wraparound. The existing reference-canvas fit hierarchy keeps the effect aligned with every standard viewport.

**Tech Stack:** C#, HelEngine 2D `RoundedRectComponent`, `UpdateComponent`, scene entity references, xUnit source-contract tests.

## Global Constraints

- Standard menu only; do not change the handheld factory.
- Use existing 2D primitives; add no image, shader, or clip asset.
- Render below existing logo, panels, footer, and platform information.
- Grid and scanlines must wrap continuously, with no easing or pauses.
- Add XML documentation to every created class and member.
- Regenerate the main menu scene and build Windows after passing the focused test.

---

### Task 1: Define and verify the authored background contract

**Files:**
- Modify: `assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs`
- Modify: `assets/codebase/menu.tools/DemoDiscStandardMainMenuSceneFactory.cs`
- Create: `assets/codebase/menu/MenuBackgroundMotionComponent.cs`

**Interfaces:**
- Produces: `MenuBackgroundMotionComponent` with `GridEntityReference`, `ScanlineEntityReference`, `GridPeriod`, `ScanlinePeriod`, `GridPixelsPerSecond`, and `ScanlinePixelsPerSecond` serialized properties.
- Produces: `CreateAnimatedBackgroundEntity(Entity generatedRootEntity, MenuDefinition definition)` invoked before overlay, footer, and panels.

- [ ] **Step 1: Write the failing test**

Add a `Standard_menu_authors_animated_grid_and_scanline_background` fact that reads the standard factory and motion-component source, asserting these exact fragments:

```csharp
Assert.Contains("CreateAnimatedBackgroundEntity(generatedRootEntity, definition)", standardFactorySource, StringComparison.Ordinal);
Assert.Contains("DemoDiscAnimatedBackgroundGrid", standardFactorySource, StringComparison.Ordinal);
Assert.Contains("DemoDiscAnimatedBackgroundScanlines", standardFactorySource, StringComparison.Ordinal);
Assert.Contains("new MenuBackgroundMotionComponent", standardFactorySource, StringComparison.Ordinal);
Assert.Contains("GridEntityReference = CreateEntityReference(gridEntity)", standardFactorySource, StringComparison.Ordinal);
Assert.Contains("ScanlineEntityReference = CreateEntityReference(scanlineEntity)", standardFactorySource, StringComparison.Ordinal);
Assert.Contains("GridPixelsPerSecond", motionComponentSource, StringComparison.Ordinal);
Assert.Contains("ScanlinePixelsPerSecond", motionComponentSource, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
$env:HELENGINE_TEST_PROJECT_ROOT = 'C:\dev\helprojs\demodisc\.worktrees\main-menu-footer-identity'
dotnet test user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --filter 'FullyQualifiedName~Standard_menu_authors_animated_grid_and_scanline_background' --no-restore
```

Expected: failure because the authored background and motion component do not exist.

- [ ] **Step 3: Implement the authored layer and motion component**

In `CreateMenuRootEntity`, invoke `CreateAnimatedBackgroundEntity` immediately after creating `generatedRootEntity`, before any overlay or panel. Author grid lines as low-alpha `RoundedRectComponent` children under `DemoDiscAnimatedBackgroundGrid`, and scanlines as low-alpha horizontal `RoundedRectComponent` children under `DemoDiscAnimatedBackgroundScanlines`. Give all layers a 2D render order lower than 28, the existing logo order.

Create `MenuBackgroundMotionComponent` as an `UpdateComponent`. On update, resolve each serialized entity reference using `SceneEntityRuntimeIdComponent`; move the grid diagonally and scanlines vertically using `FrameDeltaSeconds`; reset an axis to zero when it reaches the configured period. Throw clear `InvalidOperationException` messages for missing references or an absent object manager.

Attach the component to the shared animated-background root with the exact serialized references established by `CreateEntityReference` and nonzero grid/scanline periods and speeds.

- [ ] **Step 4: Run the focused test and verify it passes**

Run the command from Step 2.

Expected: one passing test. Existing generated-project warnings may remain unchanged.

- [ ] **Step 5: Commit the source and test contract**

```powershell
git add assets/codebase/menu/MenuBackgroundMotionComponent.cs assets/codebase/menu.tools/DemoDiscStandardMainMenuSceneFactory.cs assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs
git commit -m "feat: add animated main menu background"
```

### Task 2: Regenerate and package the standard menu

**Files:**
- Modify: `assets/scenes/DemoDiscMainMenu.helen`

**Interfaces:**
- Consumes: generated animated-background hierarchy and serialized component references from Task 1.
- Produces: a packaged Windows player whose generated main-menu scene includes the animated background.

- [ ] **Step 1: Regenerate the standard menu scene**

Run:

```powershell
dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\.worktrees\main-menu-footer-identity\project.heproj --editor-command menu.regenerate-demo-disc-main-menu
```

Expected: `Editor command 'menu.regenerate-demo-disc-main-menu' executed successfully.`

- [ ] **Step 2: Build the Windows player**

Run:

```powershell
dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\.worktrees\main-menu-footer-identity\project.heproj --build windows --build-profile debug --output C:\dev\helprojs\demodisc\.worktrees\main-menu-footer-identity\output\windows
```

Expected: `Build completed for platform 'windows'` and `output\windows\helengine_windows.exe` exists.

- [ ] **Step 3: Commit only the regenerated scene**

```powershell
git add assets/scenes/DemoDiscMainMenu.helen
git commit -m "build: regenerate animated menu background scene"
```
