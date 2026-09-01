# DemoDisc Baseline Test Repair Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Repair the eleven pre-existing DemoDisc test failures that block merging the software path tracer while making every affected source/asset test resolve the active checkout instead of a hard-coded main worktree.

**Architecture:** Repair the three failing test assemblies independently. Each assembly receives a test-local `DemoDiscTestProject` path resolver; stale source/asset expectations are updated to the accepted production behavior, the menu root's unintended single-child fallback is removed, and the splash test is made hermetic by validating the committed authored scene rather than ignored cooked output.

**Tech Stack:** C# 13, .NET 9, xUnit, HelenEngine editor asset serialization, PowerShell, Git.

**Spec:** User-approved failure diagnosis from 2026-09-01; no separate design document.

## Global Constraints

- Work only in `C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core` until the complete suite is green.
- Preserve all unrelated dirty importer/generated assets; never restore, stage, or regenerate `assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint` as part of this repair.
- Do not change the software path tracer, engine, Windows host, codegen, or Vulkan code.
- Every affected test must resolve the active checkout, preferring `HELENGINE_TEST_PROJECT_ROOT` when set and otherwise deriving the repository root from its own compiled source path.
- No affected test file may contain the literal root `C:\dev\helprojs\demodisc` after repair.
- Normalize line endings before any deliberately multiline source assertion; do not encode CRLF or LF as an accidental behavioral requirement.
- Tests must assert accepted current behavior: selected Tilt PNG overlays; `referenceCanvasFitComponent.CalculateScale()`; `LogoBottomMargin => 76`; `new DemoDiscSceneUiKitFactory(AssetAuthoringService)`; DS swatch render order `222`; and face-button mappings PS2 `circle`, GameCube `y`, Wii `2`, Switch `x`.
- DemoDisc unit tests may deserialize tracked authored assets. They must not require ignored `output/` or `windows-build/` artifacts.
- Follow strict red-green TDD. Run and record the failing focused filter before each task's implementation, then the same filter green afterward.
- Each task stages only its listed files and uses `rtk git diff --cached --check` before committing.

---

### Task 1: Repair the Tilt title-selection test

**Files:**
- Create: `assets/codebase/gameplay.tests/DemoDiscTestProject.cs`
- Create: `assets/codebase/gameplay.tests/DemoDiscTestProject.cs.hmeta`
- Modify: `assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs`

**Interfaces:**
- Consumes: the active checkout's `assets/codebase/game/TiltPlayMenuComponent.cs` and optional `HELENGINE_TEST_PROJECT_ROOT` override.
- Produces: `city.testing.DemoDiscTestProject.GetPath(params string[])` inside the gameplay test assembly and selection assertions matching the three accepted overlay assignments.

- [ ] **Step 1: Reproduce the stale color assertion**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~TiltPlayMenuComponentTests.TiltPlayMenuComponent_applies_title_action_selection_presentation" -v:minimal
```

Expected: one failure at the assertion for `new byte4(102, 56, 160, 255)`.

- [ ] **Step 2: Add the gameplay test-project path resolver**

Create `DemoDiscTestProject.cs` with this test-only implementation:

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

Create the sidecar as:

```json
{
  "version": 1,
  "assetId": "3f1a7c9e2b644d58a0f6c3e91b2d7485",
  "formerAssetIds": []
}
```

- [ ] **Step 3: Replace the stale assertion with current overlay behavior**

Read the production source through:

```csharp
global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game", "TiltPlayMenuComponent.cs")
```

Retain the `ApplyTitleActionSelection();` and entity-name assertions. Replace the purple literal with these exact accepted assignments:

```csharp
Assert.Contains("PlayButtonSelectedOverlay.Enabled = isTitleVisible && SelectedTitleActionIndex == 0;", source, StringComparison.Ordinal);
Assert.Contains("OptionsButtonSelectedOverlay.Enabled = isTitleVisible && SelectedTitleActionIndex == 1;", source, StringComparison.Ordinal);
Assert.Contains("DemoDiscButtonSelectedOverlay.Enabled = isTitleVisible && SelectedTitleActionIndex == 2;", source, StringComparison.Ordinal);
```

Use the resolver for the other `TiltPlayMenuComponent.cs` read in the same test class.

- [ ] **Step 4: Verify gameplay GREEN and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~TiltPlayMenuComponentTests" -v:minimal
rtk git diff --check -- assets/codebase/gameplay.tests/DemoDiscTestProject.cs assets/codebase/gameplay.tests/DemoDiscTestProject.cs.hmeta assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs
rtk git add -- assets/codebase/gameplay.tests/DemoDiscTestProject.cs assets/codebase/gameplay.tests/DemoDiscTestProject.cs.hmeta assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs
rtk git diff --cached --check
rtk git commit -m "Repair Tilt menu presentation tests"
```

Expected: every `TiltPlayMenuComponentTests` test passes.

---

### Task 2: Repair menu source contracts and root resolution

**Files:**
- Create: `assets/codebase/menu.tools.tests/DemoDiscTestProject.cs`
- Create: `assets/codebase/menu.tools.tests/DemoDiscTestProject.cs.hmeta`
- Modify: `assets/codebase/menu.tools.tests/SceneLoadingScreenComponentSourceTests.cs`
- Modify: `assets/codebase/menu.tools.tests/MenuComponentGeneratedRootResolutionSourceTests.cs`
- Modify: `assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs`
- Modify: `assets/codebase/menu.tools.tests/DemoDiscMainMenuAudioSourceTests.cs`
- Modify: `assets/codebase/menu/MenuComponent.cs`

**Interfaces:**
- Consumes: accepted menu implementations in `menu`, `menu.authoring`, and `menu.tools` plus optional `HELENGINE_TEST_PROJECT_ROOT`.
- Produces: the same `city.testing.DemoDiscTestProject.GetPath(params string[])` contract inside the menu test assembly and strict panel-subtree menu-root resolution without the legacy single-child fallback.

- [ ] **Step 1: Reproduce the five menu failures owned by this task**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SceneLoadingScreenComponentSourceTests|FullyQualifiedName~MenuComponentGeneratedRootResolutionSourceTests|FullyQualifiedName~DemoDiscMenuButtonTextStyleSourceTests|FullyQualifiedName~DemoDiscMainMenuAudioSourceTests" -v:minimal
```

Expected failures: instance `CalculateScale` spelling, retained single-child fallback, two references to the removed `menu/DemoDiscMenuTheme.cs`, and the LF-only outline fragment.

- [ ] **Step 2: Add the menu test-project path resolver**

Create `DemoDiscTestProject.cs` under `menu.tools.tests`:

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

Create its sidecar as:

```json
{
  "version": 1,
  "assetId": "6b2d8f04c7a145e39f5a1c8d2e709643",
  "formerAssetIds": []
}
```

Replace every absolute DemoDisc path in the four listed test files with `DemoDiscTestProject.GetPath(...)`.

- [ ] **Step 3: Align menu source assertions with accepted behavior**

Apply these exact expectation repairs:

```csharp
Assert.Contains("referenceCanvasFitComponent.CalculateScale()", source, StringComparison.Ordinal);
```

Resolve the theme as:

```csharp
global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "menu.authoring", "DemoDiscMenuTheme.cs")
```

Require `LogoBottomMargin => 76`. Normalize the standard and handheld factory sources with `source.Replace("\r\n", "\n", StringComparison.Ordinal)` before checking:

```csharp
string outlineAssignment = "definition.SurfaceBorderColor,\n                2f";
```

Keep the existing behavioral tokens for outlines, animated background, and silence.

- [ ] **Step 4: Remove the unintended single-child generated-root fallback**

Delete only this block from `MenuComponent.FindGeneratedRootEntity`:

```csharp
if (rootEntity.Children.Count == 1) {
    return rootEntity.Children[0];
}
```

The method must return a child only when its subtree contains `MenuPanelComponent`; otherwise it returns `null`. Make the test line-ending-independent by asserting absence of `rootEntity.Children.Count == 1` instead of a CRLF block.

- [ ] **Step 5: Verify menu GREEN and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SceneLoadingScreenComponentSourceTests|FullyQualifiedName~MenuComponentGeneratedRootResolutionSourceTests|FullyQualifiedName~DemoDiscMenuButtonTextStyleSourceTests|FullyQualifiedName~DemoDiscMainMenuAudioSourceTests" -v:minimal
rtk git diff --check -- assets/codebase/menu.tools.tests/DemoDiscTestProject.cs assets/codebase/menu.tools.tests/DemoDiscTestProject.cs.hmeta assets/codebase/menu.tools.tests/SceneLoadingScreenComponentSourceTests.cs assets/codebase/menu.tools.tests/MenuComponentGeneratedRootResolutionSourceTests.cs assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs assets/codebase/menu.tools.tests/DemoDiscMainMenuAudioSourceTests.cs assets/codebase/menu/MenuComponent.cs
rtk git add -- assets/codebase/menu.tools.tests/DemoDiscTestProject.cs assets/codebase/menu.tools.tests/DemoDiscTestProject.cs.hmeta assets/codebase/menu.tools.tests/SceneLoadingScreenComponentSourceTests.cs assets/codebase/menu.tools.tests/MenuComponentGeneratedRootResolutionSourceTests.cs assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs assets/codebase/menu.tools.tests/DemoDiscMainMenuAudioSourceTests.cs assets/codebase/menu/MenuComponent.cs
rtk git diff --cached --check
rtk git commit -m "Repair DemoDisc menu source contracts"
```

Expected: all tests selected by the filter pass.

---

### Task 3: Make splash-scene coverage hermetic

**Files:**
- Modify: `assets/codebase/menu.tools.tests/HelenOfCodeSplashSceneSourceTests.cs`

**Interfaces:**
- Consumes: Task 2's menu-test `DemoDiscTestProject` and tracked `assets/scenes/HelenOfCodeSplash.helen`.
- Produces: committed authored-scene layer coverage with no dependency on ignored `output/windows` content.

- [ ] **Step 1: Reproduce the stale packaged-format failure**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~HelenOfCodeSplashSceneSourceTests.Packaged_windows_splash_scene_preserves_overlay_layer_on_the_sprite_subtree" -v:minimal
```

Expected: deserialization rejects ignored package version `22` because the current serializer requires `24`.

- [ ] **Step 2: Replace ignored-package coverage with committed-scene coverage**

Rename the test to `Committed_splash_scene_preserves_overlay_layer_on_the_sprite_subtree`. Load only:

```csharp
global::city.testing.DemoDiscTestProject.GetPath("assets", "scenes", "HelenOfCodeSplash.helen")
```

Use the existing authored graph assertions and require layer `2` on the camera, background, splash root, and logo entity:

```csharp
SceneEntityAsset cameraEntity = Assert.Single(scene.RootEntities);
Assert.Equal(2, cameraEntity.Children.Length);
SceneEntityAsset backgroundEntity = Assert.Single(cameraEntity.Children.Where(entity => entity.Components.Any(component => component.ComponentTypeId == "helengine.RoundedRectComponent")));
SceneEntityAsset splashRootEntity = Assert.Single(cameraEntity.Children.Where(entity => entity.Children.Length == 1));
SceneEntityAsset logoEntity = Assert.Single(splashRootEntity.Children);
Assert.Equal((ushort)2, cameraEntity.LayerMask);
Assert.Equal((ushort)2, backgroundEntity.LayerMask);
Assert.Equal((ushort)2, splashRootEntity.LayerMask);
Assert.Equal((ushort)2, logoEntity.LayerMask);
```

Remove all reads from `output/windows/cooked`. Use `DemoDiscTestProject` for every other absolute DemoDisc path in this test file.

- [ ] **Step 3: Verify splash GREEN and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~HelenOfCodeSplashSceneSourceTests" -v:minimal
rtk git diff --check -- assets/codebase/menu.tools.tests/HelenOfCodeSplashSceneSourceTests.cs
rtk git add -- assets/codebase/menu.tools.tests/HelenOfCodeSplashSceneSourceTests.cs
rtk git diff --cached --check
rtk git commit -m "Make splash scene tests hermetic"
```

Expected: every `HelenOfCodeSplashSceneSourceTests` test passes with no package output present.

---

### Task 4: Repair rendering source and Blueprint expectations

**Files:**
- Create: `assets/codebase/rendering.tools.tests/DemoDiscTestProject.cs`
- Create: `assets/codebase/rendering.tools.tests/DemoDiscTestProject.cs.hmeta`
- Modify: `assets/codebase/rendering.tools.tests/ColoredCubeGridProfilingOverlaySourceTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsBlueprintAssetGenerationTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs`

**Interfaces:**
- Consumes: accepted scene UI-kit construction, DS render ordering, tracked Blueprint mappings, and optional `HELENGINE_TEST_PROJECT_ROOT`.
- Produces: worktree-local source/asset checks matching current constructor and control-icon contracts.

- [ ] **Step 1: Reproduce the four rendering failures**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --filter "FullyQualifiedName~ColoredCubeGridProfilingOverlaySourceTests|FullyQualifiedName~ConsoleCameraLightInstructionsBlueprintAssetGenerationTests|FullyQualifiedName~FpsFontScaleSourceTests" -v:minimal
```

Expected failures: old parameterless UI-kit construction, old DS swatch order `211`, and old shoulder-button Blueprint references.

- [ ] **Step 2: Add the rendering test-project path resolver**

Create `DemoDiscTestProject.cs` under `rendering.tools.tests`:

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

Create its sidecar as:

```json
{
  "version": 1,
  "assetId": "9c4e1a73b5d6482fa07c3e915b2d8640",
  "formerAssetIds": []
}
```

Replace absolute DemoDisc source, scene, and Blueprint paths throughout the three listed test files. Convert the two absolute `[InlineData]` scene paths to repository-relative strings and resolve them inside the test method.

- [ ] **Step 3: Align source and asset expectations**

Require this constructor-bearing source fragment wherever the UI kit is checked:

```csharp
new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateStandardSceneUi
```

Require:

```csharp
const byte NintendoDsLightSwatchRenderOrder = 222;
```

Retain the D-pad and stick assertions, but replace the four obsolete Blueprint face-action references with:

```text
images/instructions/controls/generated/ps2/circle.png
images/instructions/controls/generated/gamecube/y.png
images/instructions/controls/generated/wii/2.png
images/instructions/controls/generated/switch/x.png
```

Do not modify or stage the dirty Blueprint.

- [ ] **Step 4: Verify rendering GREEN and commit**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --filter "FullyQualifiedName~ColoredCubeGridProfilingOverlaySourceTests|FullyQualifiedName~ConsoleCameraLightInstructionsBlueprintAssetGenerationTests|FullyQualifiedName~FpsFontScaleSourceTests" -v:minimal
rtk git diff --check -- assets/codebase/rendering.tools.tests/DemoDiscTestProject.cs assets/codebase/rendering.tools.tests/DemoDiscTestProject.cs.hmeta assets/codebase/rendering.tools.tests/ColoredCubeGridProfilingOverlaySourceTests.cs assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsBlueprintAssetGenerationTests.cs assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs
rtk git add -- assets/codebase/rendering.tools.tests/DemoDiscTestProject.cs assets/codebase/rendering.tools.tests/DemoDiscTestProject.cs.hmeta assets/codebase/rendering.tools.tests/ColoredCubeGridProfilingOverlaySourceTests.cs assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsBlueprintAssetGenerationTests.cs assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs
rtk git diff --cached --check
rtk git commit -m "Repair DemoDisc rendering source tests"
```

Expected: every selected rendering test passes.

---

### Task 5: Prove the complete DemoDisc suite is merge-ready

**Files:**
- Verify only; no production or test files should change.

**Interfaces:**
- Consumes: Tasks 1-4 and the existing generated EditorFull test projects.
- Produces: fresh full-suite, tracer-suite, path-isolation, and staged-file evidence for the branch-finishing workflow.

- [ ] **Step 1: Prove affected tests no longer hard-code the main checkout**

```powershell
rtk rg -n -F 'C:\dev\helprojs\demodisc' assets/codebase/gameplay.tests/TiltPlayMenuComponentTests.cs assets/codebase/menu.tools.tests/SceneLoadingScreenComponentSourceTests.cs assets/codebase/menu.tools.tests/MenuComponentGeneratedRootResolutionSourceTests.cs assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs assets/codebase/menu.tools.tests/DemoDiscMainMenuAudioSourceTests.cs assets/codebase/menu.tools.tests/HelenOfCodeSplashSceneSourceTests.cs assets/codebase/rendering.tools.tests/ColoredCubeGridProfilingOverlaySourceTests.cs assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsBlueprintAssetGenerationTests.cs assets/codebase/rendering.tools.tests/FpsFontScaleSourceTests.cs
```

Expected: no matches and exit code `1` from `rg`.

- [ ] **Step 2: Run all four generated DemoDisc test projects**

```powershell
$projects = Get-ChildItem -LiteralPath 'user_settings\generated_code\editor-command\EditorFull\projects' -Recurse -Filter '*.tests.csproj' -File | Sort-Object FullName
$failedProjects = 0
foreach ($project in $projects) {
    rtk dotnet test $project.FullName --no-restore -v:minimal
    if ($LASTEXITCODE -ne 0) {
        $failedProjects++
    }
}
if ($failedProjects -ne 0) {
    throw "$failedProjects DemoDisc test project(s) failed."
}
```

Expected: `game.tools.tests`, `gameplay.tests`, `menu.tools.tests`, and `rendering.tools.tests` all exit `0`; the eleven diagnosed failures are absent.

- [ ] **Step 3: Re-run the complete software-path-tracer matrix**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~Software" -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --filter "FullyQualifiedName~Software" -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathTracer" -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathTracer" -v:minimal
```

Expected: every command exits `0`; the previously observed counted suites remain at least 180 gameplay, 17 rendering, and 2 menu tests.

- [ ] **Step 4: Audit the branch before returning to merge**

```powershell
rtk git diff --check main..HEAD
rtk git status --short
rtk git diff --name-only main..HEAD -- assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint output windows-build
```

Expected: no whitespace errors; the Blueprint and ignored build-output directories are absent from the committed branch diff. Preserve all unrelated dirty files. Once these checks are green, return to `superpowers:finishing-a-development-branch` and perform the already-requested local merges into the DemoDisc, engine, and Windows main branches.
