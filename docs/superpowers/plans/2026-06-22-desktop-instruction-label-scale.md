# Desktop Instruction Label Scale Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reduce the desktop/shared `Rotate Camera` and `Toggle Light` instruction labels by about 40% and reposition them inside the existing panel without changing Nintendo DS overlay behavior.

**Architecture:** Keep the change local to `DemoSceneInstructionOverlayFactory` by retuning the desktop-only label constants used by `CreateDesktopInstructionRow`. Guard the authored source shape with one regression test in `helengine.editor.tests`, then regenerate the affected city scenes and rebuild Windows to verify the updated overlay in a running build.

**Tech Stack:** C#, xUnit, headless helengine editor CLI, city generated scene pipeline, Windows runtime build.

---

### Task 1: Add the Failing Desktop Overlay Source Test

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityMenuSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Write the failing test**

```csharp
    /// <summary>
    /// Ensures the desktop instruction overlay uses the reduced desktop-only label scale and updated text placement without changing Nintendo DS instruction sizing.
    /// </summary>
    [Fact]
    public void City_demo_scene_instruction_overlay_source_uses_smaller_desktop_only_labels() {
        string sourcePath = @"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("const float DesktopInstructionLabelFontScale = 1.73f;", source, StringComparison.Ordinal);
        Assert.Contains("const float DesktopInstructionTextLeft = 126f;", source, StringComparison.Ordinal);
        Assert.Contains("const float DesktopInstructionTextTopAdjustment = 6f;", source, StringComparison.Ordinal);
        Assert.Contains("const int DesktopInstructionTextWidth = 300;", source, StringComparison.Ordinal);
        Assert.Contains("const int DesktopInstructionTextHeight = 28;", source, StringComparison.Ordinal);
        Assert.Contains("const float NintendoDsInstructionFontScale = 1.6f;", source, StringComparison.Ordinal);
    }
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter City_demo_scene_instruction_overlay_source_uses_smaller_desktop_only_labels
```

Expected: `FAIL` because the current desktop overlay source still contains the larger desktop label constants.

- [ ] **Step 3: Commit**

```bash
rtk git -C C:\dev\helworks\helengine add -- engine/helengine.editor.tests/CityMenuSourceTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "Add desktop instruction overlay source regression test"
```

### Task 2: Retune the Desktop Instruction Label Constants

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityMenuSourceTests.cs`

- [ ] **Step 1: Write the minimal constant-only implementation**

Update only the desktop/shared constants in `DemoSceneInstructionOverlayFactory`:

```csharp
        const float DesktopInstructionLabelFontScale = 1.73f;
        const float DesktopInstructionTextLeft = 126f;
        const float DesktopInstructionTextTopAdjustment = 6f;
        const int DesktopInstructionTextWidth = 300;
        const int DesktopInstructionTextHeight = 28;
```

Leave these values unchanged:

```csharp
        const float NintendoDsInstructionFontScale = 1.6f;
        const float NintendoDsInstructionTextLeft = 60f;
        const float NintendoDsInstructionTextTopAdjustment = -2f;
        const int NintendoDsInstructionTextWidth = 168;
        const int NintendoDsInstructionTextHeight = 22;
```

- [ ] **Step 2: Run the narrow test to verify it passes**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter City_demo_scene_instruction_overlay_source_uses_smaller_desktop_only_labels
```

Expected: `PASS`.

- [ ] **Step 3: Run the full menu/city source slice**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter CityMenuSourceTests
```

Expected: `PASS`.

- [ ] **Step 4: Commit**

```bash
rtk git -C C:\dev\helprojs\city add -- assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs
rtk git -C C:\dev\helprojs\city commit -m "Reduce desktop instruction overlay label size"
```

### Task 3: Regenerate Scenes, Rebuild Windows, and Verify Runtime Output

**Files:**
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\cube_test.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\scaled_cube.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\colored_cube_grid.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\textured_cube_grid.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\axis_test.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\axis_test2.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\directional_shadow_plaza.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\ground_cube_probe.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\scene_memory_probe.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\spotlight_street_slice.helen`

- [ ] **Step 1: Regenerate the rendering scenes that consume the shared desktop overlay**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-rendering-scenes
```

Expected: `Editor command 'menu.generate-rendering-scenes' executed successfully.`

- [ ] **Step 2: Rebuild the Windows output**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --build windows --output C:\dev\helprojs\city\output\windows
```

Expected: `Build completed for platform 'windows': C:\dev\helprojs\city\output\windows`

- [ ] **Step 3: Launch the rebuilt Windows output**

Run:

```bash
rtk powershell -NoProfile -Command "Start-Process -FilePath 'C:\dev\helprojs\city\output\windows\helengine_windows.exe'"
```

Expected: the application starts normally and loads the configured startup scene.

- [ ] **Step 4: Verify startup and render logs**

Run:

```bash
rtk powershell -NoProfile -Command "Start-Sleep -Seconds 8; Get-Content -Path 'C:\dev\helprojs\city\output\windows\helengine_windows.startup.log' -Tail 20; Get-Content -Path 'C:\dev\helprojs\city\output\windows\helengine_windows.render.log' -Tail 40"
```

Expected:

- startup log still loads the expected startup scene
- render log still shows `2d.render_camera`
- no startup/runtime errors are introduced by the overlay retune

- [ ] **Step 5: Commit regenerated scene outputs**

```bash
rtk git -C C:\dev\helprojs\city add -- assets/scenes/rendering
rtk git -C C:\dev\helprojs\city commit -m "Regenerate rendering scenes with smaller instruction labels"
```
