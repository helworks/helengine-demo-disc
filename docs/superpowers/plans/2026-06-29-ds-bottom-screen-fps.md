# DS Bottom Screen FPS Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move authored FPS overlays from the top screen to the bottom screen for all generated Nintendo DS rendering companion scenes.

**Architecture:** Keep authored rendering scene factories unchanged and implement the relocation in `NintendoDsRenderingSceneScaffoldFactory`. The scaffold will clone FPS settings onto a bottom-screen entity, remove the authored top-screen FPS component, regenerate the committed DS scene assets, and verify the new bottom-screen component is present.

**Tech Stack:** C#, xUnit source and asset audits, city rendering scene generation, Nintendo DS platform build

---

### Task 1: Lock The Expected Generated Asset Shape

**Files:**
- Modify: `C:\dev\helworks\helengine-ds\builder.tests\CityNintendoDsSceneSourceAuditTests.cs`
- Test: `C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj`

- [ ] **Step 1: Write the failing asset audit**

Add one test that reads `rendering/ds/cube_test_ds.helen` and `rendering/ds/scaled_cube_ds.helen`, finds `DemoDiscBottomScreenFps` under `DemoDiscBottomScreenRoot`, and asserts the entity contains a serialized `helengine.FPSComponent`.

- [ ] **Step 2: Run the targeted test to verify it fails**

Run: `dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter "FullyQualifiedName~Assets_whenGeneratedDsRenderingScenesMoveFpsToBottomScreen_includeBottomScreenFpsComponent" -clp:ErrorsOnly;Summary`

Expected: FAIL because the committed DS rendering scene assets do not yet contain `DemoDiscBottomScreenFps`.

### Task 2: Move Generated DS FPS Into The Bottom Viewport

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs`
- Modify: `C:\dev\helworks\helengine-ds\builder.tests\CityNintendoDsSceneSourceAuditTests.cs`

- [ ] **Step 1: Relocate FPS in the shared scaffold**

Update `NintendoDsRenderingSceneScaffoldFactory` so it creates scaffold-owned bottom-screen FPS entities from authored `FPSComponent` instances before the top-screen roots are finalized.

- [ ] **Step 2: Regenerate the rendering scenes**

Run: `dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-rendering-scenes`

Expected: `Editor command 'menu.generate-rendering-scenes' executed successfully.`

- [ ] **Step 3: Run the targeted test to verify it passes**

Run: `dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter "FullyQualifiedName~Assets_whenGeneratedDsRenderingScenesMoveFpsToBottomScreen_includeBottomScreenFpsComponent" -clp:ErrorsOnly;Summary`

Expected: PASS

### Task 3: Rebuild And Launch DS

**Files:**
- Build output: `C:\dev\helprojs\city\output\ds`

- [ ] **Step 1: Rebuild the DS artifact**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\artifacts\build-platform.ps1 -Project C:\dev\helprojs\city\project.heproj -Platform ds -Output C:\dev\helprojs\city\output\ds`

Expected: `Build completed for platform 'ds': C:\dev\helprojs\city\output\ds`

- [ ] **Step 2: Launch melonDS with the fresh ROM**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine-ds\scripts\launch_in_emulator.ps1 -ArtifactPath C:\dev\helprojs\city\output\ds\helengine_ds.nds`

Expected: output reports the ROM path, last-write time, emulator path, and process id.
