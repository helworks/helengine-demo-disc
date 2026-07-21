# Tilt Level 1 Render Test Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task with review checkpoints.

**Goal:** Add a separate render-only scene containing the visible Tilt Trial Level 1 cubes, coins, goal pad, and flag, with FPS diagnostics, and package it for direct PS2 inspection.

**Architecture:** Extend the existing `GameSceneFactory` with render-only Level 1 entity builders that reuse the authored transforms and visual asset references but do not attach gameplay, physics, or trigger components. `GameSceneGenerator` writes the new scene, while `GameSceneCatalog` and the PS2 build scene list make it exportable.

**Tech Stack:** C#, generated `.helen` scene assets, existing Blueprint/material/model asset generators, xUnit source tests, editor CLI PS2 packaging, PCSX2 launcher script.

---

### Task 1: Add failing source-contract tests

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`

- [ ] **Step 1: Add tests for the new scene contract**

Add tests that read the generator and factory source and assert:

```csharp
Assert.Contains("TiltTrialLevel01RenderTestSceneId", catalogSource, StringComparison.Ordinal);
Assert.Contains("CreateTiltTrialLevel01RenderTestScene", generatorSource, StringComparison.Ordinal);
Assert.Contains("CreateLevel01RenderOnlyCourseBoxEntity", factorySource, StringComparison.Ordinal);
Assert.Contains("GoldenCoinBlueprintRelativePath", factorySource, StringComparison.Ordinal);
Assert.Contains("GoalFlagBlueprintRelativePath", factorySource, StringComparison.Ordinal);
Assert.Contains("new FPSComponent", factorySource, StringComparison.Ordinal);
Assert.DoesNotContain("new RigidBody3DComponent", renderMethodSource, StringComparison.Ordinal);
Assert.DoesNotContain("SceneEntityTriggerObserverComponent", renderMethodSource, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
rtk dotnet test assets/codebase/game.tools.tests/game.tools.tests.csproj --filter FullyQualifiedName~TiltTrialSceneGenerationSourceTests
```

Expected: FAIL because the new catalog id, generator method, and render-only factory path do not exist yet.

### Task 2: Register the render-test scene

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneCatalog.cs`
- Modify: `assets/codebase/game.tools/GameSceneGenerator.cs`
- Modify: `user_settings/build_config.json`

- [ ] **Step 1: Add `TiltTrialLevel01RenderTestSceneId`**

Define the id as `tilt_trial_level_01_render_test` and include it in `GameSceneCatalog.GetSceneIds()`.

- [ ] **Step 2: Write the generated scene**

After the normal Level 1 gameplay scene generation path is available, call `factory.CreateTiltTrialLevel01RenderTestScene()` and write its returned definition with `sceneWriteService.WriteScene(projectRootPath, renderTestScene)`.

- [ ] **Step 3: Add the scene to the PS2 scene list**

Add `tilt_trial_level_01_render_test` to the PS2 `selectedSceneIds` and `sceneOrders` entries in `user_settings/build_config.json`. Do not add it to other platform lists.

- [ ] **Step 4: Run the focused test and verify the registration contract**

Run the Task 1 test command. Expected: the registration assertions pass while the render-only factory assertions remain red until Task 3.

### Task 3: Implement render-only Level 1 scene authoring

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneFactory.cs`

- [ ] **Step 1: Add the render-test scene method**

Create `CreateTiltTrialLevel01RenderTestScene()` returning `GeneratedAuthoringSceneDefinition` with:

```csharp
new[] {
    CreateRenderTestCameraEntity(),
    CreateDirectionalLightEntity(),
    CreateDirectionalFillLightEntity(),
    CreateAmbientLightEntity(),
    CreateRenderTestFpsEntity(),
    CreateTiltTrialLevel01RenderOnlyStageRootEntity()
}
```

Use a fixed camera framing the full Level 1 course and a scene path under `assets/scenes/physics/test_scene_tilt_trial_level_01_render.helen`.

- [ ] **Step 2: Add render-only course entities**

Create cube-only helpers using the exact Level 1 positions, scales, and orientations for StartPad, Ramp, Bridge, both blockers, FinalPlatform, both walls, and both final guards. Each helper adds only a `MeshComponent` using `GeneratedCubeModel` and `TiltTrialCourseMaterial`, plus the existing material reference persistence.

- [ ] **Step 3: Add render-only coin and flag entities**

Create visual-only coin entities at `(0, 1.35, -2.2)`, `(-0.8, 1.9, 4.6)`, and `(1.35, 1.9, 13.8)` with the existing coin blueprint reference and no trigger/collider/collectible components. Create the flag at the existing Level 1 position with its existing flag blueprint reference and no gameplay components.

- [ ] **Step 4: Add FPS-only diagnostics**

Create an entity containing `FPSComponent` with the project UI font and the same font-reference persistence used by the Matrix Render UI. Do not add return-to-menu, gameplay HUD, or instruction overlays.

- [ ] **Step 5: Run the focused source test and verify it passes**

Run the Task 1 test command. Expected: PASS, including the absence assertions for physics and trigger components.

### Task 4: Generate and inspect the authored scene

**Files:**
- Generated: `assets/scenes/physics/test_scene_tilt_trial_level_01_render.helen`

- [ ] **Step 1: Run the editor scene-generation path through the PS2 editor CLI build**

Run:

```powershell
rtk proxy powershell -NoProfile -Command "& 'C:\dev\helworks\helengine\.codex-temp\editor-ps2-current\publish\helengine.editor.app.exe' --project 'C:\dev\helprojs\demodisc' --build ps2 --build-profile ps2-default --output 'C:\dev\helprojs\demodisc\tmp\editor-cli-tilt-level01-render-test' 2>&1 | Select-Object -Last 120"
```

Expected: `native build completed`, `iso packaged`, and `packaged outputs verified` in `ps2-build-phase.txt`.

- [ ] **Step 2: Verify the package contains the generated scene and ISO**

Verify `game.iso`, `disc/HELENGIN.ELF`, and the complete build phase file exist. Confirm the ISO is larger than 10 MB and the scene appears in the packaged scene manifest.

- [ ] **Step 3: Launch the exact ISO in PCSX2**

Run:

```powershell
rtk proxy powershell -NoProfile -ExecutionPolicy Bypass -File 'C:\dev\helworks\helengine-ps2\.worktrees\ps2-onscreen-boot-log\scripts\launch_in_emulator.ps1' -ArtifactPath 'C:\dev\helprojs\demodisc\tmp\editor-cli-tilt-level01-render-test\game.iso'
```

Expected: PCSX2 starts the render-test scene with the course geometry, coins, flag, and FPS overlay visible.

### Task 5: Commit only the feature files

- [ ] **Step 1: Review status and diff**

Run `rtk git status --short` and confirm unrelated pre-existing changes are not staged.

- [ ] **Step 2: Commit the scene feature**

```powershell
rtk git add assets/codebase/game.tools/GameSceneCatalog.cs assets/codebase/game.tools/GameSceneGenerator.cs assets/codebase/game.tools/GameSceneFactory.cs assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs user_settings/build_config.json assets/scenes/physics/test_scene_tilt_trial_level_01_render.helen
rtk git commit -m "Add Tilt Level 1 render test scene"
```
