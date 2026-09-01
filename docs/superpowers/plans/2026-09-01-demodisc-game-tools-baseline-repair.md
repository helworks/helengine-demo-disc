# DemoDisc Game-Tools Baseline Repair Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the committed DemoDisc branch pass the clean full test gate by repairing fourteen stale game-tools test contracts, restoring the five serialized Tilt Trial platform overrides, and clearing ten branch-owned whitespace errors.

**Architecture:** Keep every repair inside DemoDisc. Add one test-assembly-local checkout resolver so source and asset tests inspect the active checkout, align stale source assertions with accepted authoring APIs, improve the desktop-guard test parser, and use the existing presentation attachment editor command to repair serialized assets. Do not change accepted runtime or engine behavior to satisfy stale string assertions.

**Tech Stack:** C# 13, xUnit, HelEngine editor authoring transactions, native `.helen`/`.hblueprint` serialization, PowerShell, Git

**Spec:** `docs/superpowers/plans/2026-09-01-demodisc-baseline-test-repair.md`

## Global Constraints

- Work only in `C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core`.
- Preserve every pre-existing dirty or untracked file; never clean, restore, or broad-stage the worktree.
- Stage only the exact files listed by the current task.
- Keep path helpers inside `game.tools.tests`; do not add engine utilities.
- Do not modify accepted `GameSceneFactory`, input runtime, renderer, or path-tracer behavior to satisfy stale source-text tests.
- Do not patch binary assets by hand. Regenerate the six presentation-owned assets through `menu.attach-tilt-trial-presentation-blueprints`.
- If `codegen.exe` displays a MessageBox, terminate only the process launched by the current command and stop that task without retrying.
- The four Assimp failures observed in the copied clean scaffold are validation artifacts: the canonical generated project references a cache directory containing `AssimpNetter.dll`. Do not add a DemoDisc package workaround.

---

### Task 1: Clear committed plan whitespace errors

**Files:**
- Modify: `docs/superpowers/plans/2026-09-01-software-bvh-build-buffer-borrowing.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-bvh-constructor-ownership-transfer.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-model-owned-return-codegen-fix.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-path-tracer-borrowed-component-codegen-fix.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-path-tracer-catalog-helenui.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-path-tracer-platform-presentation.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-path-tracer-text-only-hud.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-progressive-default-allocation-flow.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-trace-instance-collection-borrowing.md`
- Modify: `docs/superpowers/plans/2026-09-01-software-trace-stable-group-control-flow.md`

**Interfaces:**
- Consumes: Git's whitespace audit for `main..HEAD`.
- Produces: the same document contents with exactly one terminal newline and no blank line at EOF.

- [ ] **Step 1: Reproduce the branch-owned whitespace failure**

```powershell
git diff --check main..HEAD
```

Expected: ten `new blank line at EOF` errors, one for each listed plan.

- [ ] **Step 2: Remove only the extra terminal blank line**

For every listed file, delete its final empty line while retaining the normal newline that terminates the last content line. Do not reflow or rewrite any plan text.

- [ ] **Step 3: Verify and commit**

```powershell
git diff --check main..HEAD
rtk git diff --stat -- docs/superpowers/plans
rtk git add -- docs/superpowers/plans/2026-09-01-software-bvh-build-buffer-borrowing.md docs/superpowers/plans/2026-09-01-software-bvh-constructor-ownership-transfer.md docs/superpowers/plans/2026-09-01-software-model-owned-return-codegen-fix.md docs/superpowers/plans/2026-09-01-software-path-tracer-borrowed-component-codegen-fix.md docs/superpowers/plans/2026-09-01-software-path-tracer-catalog-helenui.md docs/superpowers/plans/2026-09-01-software-path-tracer-platform-presentation.md docs/superpowers/plans/2026-09-01-software-path-tracer-text-only-hud.md docs/superpowers/plans/2026-09-01-software-progressive-default-allocation-flow.md docs/superpowers/plans/2026-09-01-software-trace-instance-collection-borrowing.md docs/superpowers/plans/2026-09-01-software-trace-stable-group-control-flow.md
rtk git diff --cached --check
rtk git commit -m "Repair software tracer plan whitespace"
```

Expected: both whitespace checks exit `0`; the staged diff removes one blank line from each file and nothing else.

---

### Task 2: Repair game-scene and level-selector source contracts

**Files:**
- Create: `assets/codebase/game.tools.tests/DemoDiscTestProject.cs`
- Create: `assets/codebase/game.tools.tests/DemoDiscTestProject.cs.hmeta`
- Modify: `assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs`
- Modify: `assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs`

**Interfaces:**
- Consumes: optional `HELENGINE_TEST_PROJECT_ROOT` and the accepted transaction/file-reference authoring APIs.
- Produces: `city.testing.DemoDiscTestProject.GetPath(params string[])` for later game-tools test tasks and eight repaired source-contract tests.

- [ ] **Step 1: Reproduce the eight failures**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialSceneGenerationSourceTests.Game_scene_factory_authors_level_settings_and_session_components|FullyQualifiedName~TiltTrialSceneGenerationSourceTests.Tilt_trial_front_door_uses_authored_png_sprites_for_title_chrome|FullyQualifiedName~TiltTrialSceneGenerationSourceTests.Level_01_render_test_scene_uses_one_cube_light_camera_and_fps_only|FullyQualifiedName~TiltTrialLevelSelectLayoutSourceTests.Game_scene_factory_uses_one_shared_non_handheld_selector|FullyQualifiedName~TiltTrialLevelSelectLayoutSourceTests.Game_scene_factory_enables_details_stage_only_for_handheld_selector|FullyQualifiedName~TiltTrialLevelSelectLayoutSourceTests.Game_scene_factory_expands_desktop_level_list_without_title|FullyQualifiedName~TiltTrialLevelSelectLayoutSourceTests.Game_scene_factory_uses_desktop_selector_detail_action_buttons|FullyQualifiedName~TiltTrialLevelSelectLayoutSourceTests.Level_select_controller_uses_generated_platform_action_prompts" -v:minimal
```

Expected: eight failures matching the stale API strings diagnosed in the clean snapshot.

- [ ] **Step 2: Add the game-tools test checkout resolver**

Create `DemoDiscTestProject.cs`:

```csharp
using System.Runtime.CompilerServices;

namespace city.testing {
    public static class DemoDiscTestProject {
        public static readonly string RootPath = ResolveRootPath();

        public static string GetPath(params string[] relativeParts) {
            string path = RootPath;
            for (int partIndex = 0; partIndex < relativeParts.Length; partIndex++) {
                path = Path.Combine(path, relativeParts[partIndex]);
            }
            return path;
        }

        static string ResolveRootPath([CallerFilePath] string sourceFilePath = "") {
            string configuredRoot = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT");
            if (!string.IsNullOrWhiteSpace(configuredRoot)) {
                return Path.GetFullPath(configuredRoot);
            }
            string sourceDirectory = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrWhiteSpace(sourceDirectory)) {
                throw new InvalidOperationException("DemoDisc test source directory could not be resolved.");
            }
            return Path.GetFullPath(Path.Combine(sourceDirectory, "..", "..", ".."));
        }
    }
}
```

Create `DemoDiscTestProject.cs.hmeta`:

```json
{
  "version": 1,
  "assetId": "d84f1c2e7b9a4d60a3f58e1c2b749306",
  "formerAssetIds": []
}
```

- [ ] **Step 3: Make both test classes checkout-local**

Replace every hard-coded `C:\dev\helprojs\demodisc` source or asset path in the two listed test files with `global::city.testing.DemoDiscTestProject.GetPath(...)`. Convert directories and multiple-source tests the same way; leave no absolute DemoDisc path in either file.

- [ ] **Step 4: Align scene-generation assertions with accepted APIs**

Use these exact current fragments:

```csharp
Assert.Contains("RequireIcon(ProjectRootPath, \"windows\", \"enter\", AssetAuthoringService, Transaction)", source, StringComparison.Ordinal);
Assert.Contains("AssetAuthoringService.CreateFileReference(textureRelativePath, AssetEntryKind.Image)", source, StringComparison.Ordinal);
Assert.Contains("AssetAuthoringService.CreateFileReference(TiltTrialClippingProbeModelFactory.ModelRelativePath, AssetEntryKind.Model)", factorySource, StringComparison.Ordinal);
Assert.Contains("AssetAuthoringService.CreateFileReference(TiltTrialClippingProbeMaterialFactory.MaterialRelativePath, AssetEntryKind.Material)", factorySource, StringComparison.Ordinal);
```

Retain the existing behavior assertions around the start prompt, authored title PNGs, render-only scene, probe model/material, camera, and FPS component.

- [ ] **Step 5: Align level-selector assertions with accepted APIs**

Apply these exact repairs:

```csharp
Assert.DoesNotContain("CreatePs2LevelSelectUiEntity", source, StringComparison.Ordinal);
int desktopMethodStart = source.IndexOf("EditorEntity CreateLevelSelectUiEntity(bool useOwnViewport)", StringComparison.Ordinal);
Assert.Contains("AssetAuthoringService.CreateFileReference(relativePath, AssetEntryKind.Image)", source, StringComparison.Ordinal);
Assert.Contains("ControlIconResolver.RequireIcon(ProjectRootPath, \"windows\", controlId, AssetAuthoringService, Transaction)", factorySource, StringComparison.Ordinal);
```

Use the bool-signature marker in both tests that slice the standard selector. Preserve `UseDetailsStage = true` for handheld and `false` for standard, and preserve the existing Back/Play, viewport, list, preview, and platform-override assertions.

- [ ] **Step 6: Verify and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialSceneGenerationSourceTests|FullyQualifiedName~TiltTrialLevelSelectLayoutSourceTests" -p:UseSharedCompilation=false -v:minimal
rtk rg -n -F 'C:\dev\helprojs\demodisc' assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs
rtk git diff --check -- assets/codebase/game.tools.tests/DemoDiscTestProject.cs assets/codebase/game.tools.tests/DemoDiscTestProject.cs.hmeta assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs
rtk git add -- assets/codebase/game.tools.tests/DemoDiscTestProject.cs assets/codebase/game.tools.tests/DemoDiscTestProject.cs.hmeta assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs
rtk git diff --cached --check
rtk git commit -m "Repair Tilt Trial source contract tests"
```

Expected: both full test classes pass; `rg` exits `1` with no matches.

---

### Task 3: Repair tessellation and desktop-guard contracts

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs`
- Modify: `assets/codebase/game.tools.tests/DesktopKeyboardSourceContractTests.cs`

**Interfaces:**
- Consumes: `DemoDiscTestProject.GetPath`, `GeneratedFileTransactionWriter.WriteTexture`, `IEditorProjectAuthoringSession.WriteGeneratedTexture`, and current authored clipping-probe references.
- Produces: current texture-authoring assertions and a source-filter that evaluates both direct and inverse desktop guards.

- [ ] **Step 1: Reproduce the three failures and add an inverse-guard regression test**

Run the two existing failing classes, then add:

```csharp
[Fact]
public void RemoveDesktopOnlySource_keeps_the_non_desktop_branch_of_an_inverse_guard() {
    string source = "#if !DESKTOP_PLATFORM\nreturn false;\n#else\nreturn Keys.Enter;\n#endif";

    string nonDesktopSource = RemoveDesktopOnlySource(source);

    Assert.Contains("return false;", nonDesktopSource, StringComparison.Ordinal);
    Assert.DoesNotContain("Keys.", nonDesktopSource, StringComparison.Ordinal);
}
```

Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialLevel01SceneSourceTests|FullyQualifiedName~DesktopKeyboardSourceContractTests" -v:minimal
```

Expected: the two stale Level 01 contracts, runtime keyboard contract, and new inverse-guard regression are red before implementation.

- [ ] **Step 2: Make both files checkout-local**

Replace every hard-coded DemoDisc path in both files with `global::city.testing.DemoDiscTestProject.GetPath(...)`. Set the runtime source root through:

```csharp
static readonly string RuntimeSourceRootPath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase");
```

- [ ] **Step 3: Align texture and clipping-probe expectations**

In `Tessellation_authoring_sources_use_current_modifier_stack_api`, retain the modifier-stack assertions and require `GeneratedFileTransactionWriter.WriteTexture` in the textured-cube, physics, course, and clipping-probe sources. Require `WriteGeneratedTexture` in the icon, PBR, walnut, and marble sources. Continue rejecting direct serializer/import-settings construction.

Rename `Game_scene_factory_creates_one_tessellated_clipping_probe_cube` to `Game_scene_factory_uses_one_authored_clipping_probe_cube` and assert:

```csharp
Assert.Contains("CreateLevel01RenderOnlyCourseBoxEntity(\"ClipProbeCube\", float3.Zero, new float3(5f, 1f, 5f), float4.Identity)", source, StringComparison.Ordinal);
Assert.DoesNotContain("CreateLevel01RenderOnlyCourseBoxEntity(\"ClipProbeCube\", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true)", source, StringComparison.Ordinal);
Assert.Contains("AssetAuthoringService.CreateFileReference(TiltTrialClippingProbeModelFactory.ModelRelativePath, AssetEntryKind.Model)", source, StringComparison.Ordinal);
Assert.Contains("AssetAuthoringService.CreateFileReference(TiltTrialClippingProbeMaterialFactory.MaterialRelativePath, AssetEntryKind.Material)", source, StringComparison.Ordinal);
```

- [ ] **Step 4: Teach the test filter both desktop guard forms**

Replace the single depth counter with this branch-aware filter:

```csharp
static string RemoveDesktopOnlySource(string source) {
    if (source == null) {
        throw new ArgumentNullException(nameof(source));
    }

    StringReader reader = new StringReader(source);
    StringWriter writer = new StringWriter();
    Stack<(bool IsDesktopConditional, bool IncludeBranch)> conditionalStack = new Stack<(bool, bool)>();
    string line;
    while ((line = reader.ReadLine()) != null) {
        string trimmedLine = line.Trim();
        if (trimmedLine.StartsWith("#if ", StringComparison.Ordinal)) {
            bool isDesktopConditional = string.Equals(trimmedLine, "#if DESKTOP_PLATFORM", StringComparison.Ordinal);
            bool isInverseDesktopConditional = string.Equals(trimmedLine, "#if !DESKTOP_PLATFORM", StringComparison.Ordinal);
            conditionalStack.Push((
                isDesktopConditional || isInverseDesktopConditional,
                isInverseDesktopConditional || (!isDesktopConditional && !isInverseDesktopConditional)));
            continue;
        }
        if (string.Equals(trimmedLine, "#else", StringComparison.Ordinal)) {
            if (conditionalStack.Count == 0) {
                throw new InvalidOperationException("Desktop platform source guard has an unmatched #else.");
            }
            (bool isDesktopConditional, bool includeBranch) = conditionalStack.Pop();
            conditionalStack.Push((isDesktopConditional, isDesktopConditional ? !includeBranch : true));
            continue;
        }
        if (string.Equals(trimmedLine, "#endif", StringComparison.Ordinal)) {
            if (conditionalStack.Count == 0) {
                throw new InvalidOperationException("Desktop platform source guard has an unmatched #endif.");
            }
            conditionalStack.Pop();
            continue;
        }

        bool includeLine = true;
        foreach ((bool IsDesktopConditional, bool IncludeBranch) frame in conditionalStack) {
            if (!frame.IncludeBranch) {
                includeLine = false;
                break;
            }
        }
        if (includeLine) {
            writer.WriteLine(line);
        }
    }

    if (conditionalStack.Count != 0) {
        throw new InvalidOperationException("Desktop platform source guard is not balanced.");
    }
    return writer.ToString();
}
```

- [ ] **Step 5: Verify and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialLevel01SceneSourceTests|FullyQualifiedName~DesktopKeyboardSourceContractTests" -p:UseSharedCompilation=false -v:minimal
rtk rg -n -F 'C:\dev\helprojs\demodisc' assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs assets/codebase/game.tools.tests/DesktopKeyboardSourceContractTests.cs
rtk git diff --check -- assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs assets/codebase/game.tools.tests/DesktopKeyboardSourceContractTests.cs
rtk git add -- assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs assets/codebase/game.tools.tests/DesktopKeyboardSourceContractTests.cs
rtk git diff --cached --check
rtk git commit -m "Repair Tilt Trial authoring contract tests"
```

Expected: both test classes pass and the path scan has no matches.

---

### Task 4: Repair camera and lighting expectations

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialCameraAuthoringTests.cs`
- Modify: `assets/codebase/game.tools.tests/TiltTrialLightingAuthoringTests.cs`

**Interfaces:**
- Consumes: checkout-local paths, `OwningCore` authoring ownership, and the canonical serialized `fonts/Fredoka.ttf` reference.
- Produces: three green tests without changing camera, font, or light behavior.

- [ ] **Step 1: Reproduce the three failures**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialCameraAuthoringTests.Tilt_trial_handheld_selector_binds_title_to_the_top_camera|FullyQualifiedName~TiltTrialCameraAuthoringTests.Tilt_trial_handheld_selector_scene_asset_keeps_fredoka_dependency|FullyQualifiedName~TiltTrialLightingAuthoringTests.Tilt_trial_scene_source_authors_stronger_key_and_shadowless_fill_light" -v:minimal
```

Expected: stale `Core.Instance` and uppercase `Fonts` assertions fail.

- [ ] **Step 2: Make both classes checkout-local and update exact contracts**

Replace every hard-coded DemoDisc path in both files with `DemoDiscTestProject.GetPath(...)`. Change only these expectations:

```csharp
Assert.Contains("OwningCore.EntityFactory.CreateChild(parent, \"TiltTrialHandheldLevelSelectTopInfo\")", source, StringComparison.Ordinal);
string.Equals(reference.RelativePath, "fonts/Fredoka.ttf", StringComparison.Ordinal)
Assert.Contains("Entity entity = OwningCore.EntityFactory.Create(\"TiltTrialSun\");", source, StringComparison.Ordinal);
Assert.Contains("Entity entity = OwningCore.EntityFactory.Create(\"TiltTrialFill\");", source, StringComparison.Ordinal);
```

Retain all camera-binding, camera-pose, font-layout, intensity, and shadow assertions.

- [ ] **Step 3: Verify and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialCameraAuthoringTests|FullyQualifiedName~TiltTrialLightingAuthoringTests" -p:UseSharedCompilation=false -v:minimal
rtk rg -n -F 'C:\dev\helprojs\demodisc' assets/codebase/game.tools.tests/TiltTrialCameraAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialLightingAuthoringTests.cs
rtk git diff --check -- assets/codebase/game.tools.tests/TiltTrialCameraAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialLightingAuthoringTests.cs
rtk git add -- assets/codebase/game.tools.tests/TiltTrialCameraAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialLightingAuthoringTests.cs
rtk git diff --cached --check
rtk git commit -m "Repair Tilt Trial camera contract tests"
```

Expected: both classes pass and contain no absolute DemoDisc path.

---

### Task 5: Restore authored Tilt Trial presentation overrides

**Files:**
- Modify: `assets/codebase/game.tools.tests/TiltTrialPlatformPresentationSourceTests.cs`
- Regenerate: `assets/blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint`
- Regenerate: `assets/scenes/games/tilt/tilt_trial_level_01.helen`
- Regenerate: `assets/scenes/games/tilt/tilt_trial_level_02.helen`
- Regenerate: `assets/scenes/games/tilt/tilt_trial_level_03.helen`
- Regenerate: `assets/scenes/games/tilt/tilt_trial_level_04.helen`
- Regenerate: `assets/scenes/games/tilt/tilt_trial_level_05.helen`

**Interfaces:**
- Consumes: `menu.attach-tilt-trial-presentation-blueprints` and the existing transaction-backed `TiltTrialGameplayPresentationAttachmentService`.
- Produces: five scenes with DS, 3DS, and Windows Release exclusions on `TiltTrialPhysicsBoundsDebug`, plus current console/handheld presentation roots.

- [ ] **Step 1: Prove the target assets are clean and reproduce RED**

```powershell
rtk git status --short -- assets/blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint assets/scenes/games/tilt/tilt_trial_level_01.helen assets/scenes/games/tilt/tilt_trial_level_02.helen assets/scenes/games/tilt/tilt_trial_level_03.helen assets/scenes/games/tilt/tilt_trial_level_04.helen assets/scenes/games/tilt/tilt_trial_level_05.helen
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialPlatformPresentationSourceTests.Authored_gameplay_scenes_scope_windows_only_debug_root_to_windows" -v:minimal
```

Expected: the status command has no entries; the test fails because DS/3DS exclusions are missing. If any target asset is dirty before regeneration, stop without overwriting it.

- [ ] **Step 2: Make the presentation test checkout-local**

Replace every hard-coded DemoDisc path in `TiltTrialPlatformPresentationSourceTests.cs` with `DemoDiscTestProject.GetPath(...)`, including the scene directory and both Blueprint paths. Keep the serialized debug-root assertions unchanged.

- [ ] **Step 3: Run only the existing attachment transaction**

```powershell
rtk dotnet run --no-build --project C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\project.heproj --editor-command menu.attach-tilt-trial-presentation-blueprints
```

Expected: exit `0` with no MessageBox. The command transaction rewrites exactly the six listed native assets.

- [ ] **Step 4: Audit regenerated scope and verify GREEN**

```powershell
rtk git status --short -- assets/blueprints/games/tilt assets/scenes/games/tilt
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~TiltTrialPlatformPresentationSourceTests" -p:UseSharedCompilation=false -v:minimal
rtk rg -n -F 'C:\dev\helprojs\demodisc' assets/codebase/game.tools.tests/TiltTrialPlatformPresentationSourceTests.cs
```

Expected: only the six owned native assets are modified under those directories; the full presentation test class passes; the path scan has no matches.

- [ ] **Step 5: Stage exact files and commit**

```powershell
rtk git add -- assets/codebase/game.tools.tests/TiltTrialPlatformPresentationSourceTests.cs assets/blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint assets/scenes/games/tilt/tilt_trial_level_01.helen assets/scenes/games/tilt/tilt_trial_level_02.helen assets/scenes/games/tilt/tilt_trial_level_03.helen assets/scenes/games/tilt/tilt_trial_level_04.helen assets/scenes/games/tilt/tilt_trial_level_05.helen
rtk git diff --cached --check
rtk git diff --cached --name-only
rtk git commit -m "Restore Tilt Trial platform presentation assets"
```

Expected: the cached name list is exactly the seven task files.

---

### Task 6: Prove the clean full suite and tracer matrix

**Files:**
- Verify only; no production, test, plan, or asset files should change.

**Interfaces:**
- Consumes: Tasks 1-5 and the canonical generated EditorFull projects.
- Produces: clean-HEAD full-suite, tracer-matrix, path-isolation, process, whitespace, and branch-scope evidence for the finishing workflow.

- [ ] **Step 1: Prove all repaired game-tools tests are checkout-local**

```powershell
rtk rg -n -F 'C:\dev\helprojs\demodisc' assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs assets/codebase/game.tools.tests/TiltTrialLevelSelectLayoutSourceTests.cs assets/codebase/game.tools.tests/TiltTrialLevel01SceneSourceTests.cs assets/codebase/game.tools.tests/DesktopKeyboardSourceContractTests.cs assets/codebase/game.tools.tests/TiltTrialCameraAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialLightingAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialPlatformPresentationSourceTests.cs
```

Expected: no matches and exit `1`.

- [ ] **Step 2: Build and run the complete game-tools suite against the active checkout**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore -p:UseSharedCompilation=false -v:minimal
```

Expected: every game-tools test passes. The canonical generated project continues to resolve `helengine.editor.dll` and `AssimpNetter.dll` from the same engine cache; do not copy or rewrite its project references.

- [ ] **Step 3: Create a read-only clean snapshot of exact HEAD**

```powershell
$head = (git rev-parse --short=12 HEAD).Trim()
$verificationRoot = "C:\dev\helprojs\_demodisc-final-$head"
if (Test-Path -LiteralPath $verificationRoot) { throw "Verification root already exists: $verificationRoot" }
New-Item -ItemType Directory -Path $verificationRoot | Out-Null
$archivePath = "$verificationRoot.tar"
git archive --format=tar HEAD -o $archivePath
tar -xf $archivePath -C $verificationRoot
$env:HELENGINE_TEST_PROJECT_ROOT = $verificationRoot
```

Expected: the snapshot contains committed assets and sources only. Do not copy generated project files into it and do not rewrite their engine references.

- [ ] **Step 4: Run all four full projects without rebuilding against dirty assets**

```powershell
$projects = Get-ChildItem -LiteralPath 'user_settings\generated_code\editor-command\EditorFull\projects' -Recurse -Filter '*.tests.csproj' -File | Sort-Object FullName
$failedProjects = 0
foreach ($project in $projects) {
    rtk dotnet test $project.FullName --no-restore --no-build -v:minimal
    if ($LASTEXITCODE -ne 0) { $failedProjects++ }
}
if ($failedProjects -ne 0) { throw "$failedProjects DemoDisc test project(s) failed." }
```

Expected: game, gameplay, menu, and rendering test projects all exit `0`. This run reads clean committed source/assets through `HELENGINE_TEST_PROJECT_ROOT` while using the already-built canonical test assemblies.

- [ ] **Step 5: Re-run the complete software-path-tracer matrix**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --no-build --filter "FullyQualifiedName~Software" -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --no-build --filter "FullyQualifiedName~Software" -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --no-build --filter "FullyQualifiedName~SoftwarePathTracer" -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --no-build --filter "FullyQualifiedName~SoftwarePathTracer" -v:minimal
Remove-Item Env:\HELENGINE_TEST_PROJECT_ROOT
```

Expected minimum passing counts: 180 gameplay, 17 rendering, 4 game integration, and 2 menu.

- [ ] **Step 6: Audit branch scope and process cleanup**

```powershell
git diff --check main..HEAD
rtk git diff --name-only main..HEAD -- assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint output windows-build
rtk git status --short
Get-Process codegen -ErrorAction SilentlyContinue
```

Expected: no whitespace errors; no committed UI Blueprint/output/build artifacts; all pre-existing unrelated dirt remains unmodified; no `codegen.exe` process remains. Record the clean snapshot path for later deliberate cleanup. Then return to `superpowers:finishing-a-development-branch` and the already-requested three-repository local merge.
