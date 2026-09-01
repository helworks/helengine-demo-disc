# Software Path Tracer Text-Only HUD Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the oversized Windows/desktop software-path-tracer HUD card with four tiny, left-aligned text rows that leave the 320x240 progressive render visible.

**Architecture:** Keep the existing mutually exclusive desktop and handheld HUD trees. Change only the desktop authoring graph: the three diagnostics become direct children of the desktop HUD root, the visible panel and Return-button rectangles disappear, and a small invisible pointer target owns a text-only `RETURN` label. Preserve the DS/3DS bottom-screen presentation unchanged.

**Tech Stack:** C#, xUnit, HelenEngine editor authoring APIs, DemoDisc scene generation, Windows DirectX 11 packaging.

**Spec:** `docs/superpowers/specs/2026-08-31-demodisc-software-path-tracer-design.md`

## Global Constraints

- DemoDisc owns the path tracer, HUD authoring, scene generation, and tests; do not add an engine utility.
- The desktop output remains one 320x240 CPU-rendered texture presented through DirectX 11; do not select Vulkan.
- Desktop HUD visuals are text only: no `RoundedRectComponent` anywhere beneath `SoftwarePathTracerDesktopHudRoot`.
- Desktop diagnostic text uses `FontScale = 0.35f`, `TextAlignment.Left`, and `Size = new int2(128, 12)`.
- Desktop rows are positioned at `(4,4)`, `(4,16)`, and `(4,28)` in the 320x240 reference canvas.
- The text-only `RETURN` label is positioned at `(4,40)`, uses the same scale/alignment with `Size = new int2(64, 12)`, and retains an invisible `InteractableComponent` target of `64x12` for pointer return.
- DS and 3DS handheld HUD dimensions, font scale, backgrounds, placement, controller overrides, and Return behavior remain unchanged.
- Follow strict red-green TDD: the behavioral test must fail for the oversized/card-backed current layout before production code changes.
- Preserve unrelated dirty generated/importer files and stage only this task's plan, source, tests, and regenerated `software_path_tracer.helen`.

---

### Task 1: Author, regenerate, package, and show the text-only desktop HUD

**Files:**
- Modify: `assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs`
- Modify: `assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs`
- Generate: `assets/scenes/rendering/software_path_tracer.helen`
- Preserve uncommitted: `user_settings/windows-dx11-build-platforms/platforms.json`
- Produce uncommitted: `windows-build/helengine_windows.exe`

**Interfaces:**
- Consumes: `SoftwarePathTracerSceneFactory.CreateSceneDefinition(string, SceneAssetReference, FontAsset)` and the existing desktop/handheld entity names and return components.
- Produces: a desktop HUD tree whose only renderable components are four compact `TextComponent` instances, plus an invisible 64x12 return interaction target; handheld serialization remains byte-for-byte semantically equivalent.

- [ ] **Step 1: Add a behavioral test that catches the visible desktop obstruction**

Extend `Creates_the_presentation_camera_hud_and_stably_wired_controller` or add a focused fact that builds a real scene definition and asserts the following literals without inspecting source text:

```csharp
Entity desktopRoot = entities.Single(entity => EntityName(entity) == "SoftwarePathTracerDesktopHudRoot");
Entity[] desktopTree = FlattenEntities(new[] { desktopRoot }).ToArray();
Assert.Empty(desktopTree.SelectMany(entity => entity.Components.OfType<RoundedRectComponent>()));

var expectedRows = new[] {
    (Name: "SoftwarePathTracerSppText", Position: new float3(4f, 4f, 0.1f)),
    (Name: "SoftwarePathTracerElapsedText", Position: new float3(4f, 16f, 0.1f)),
    (Name: "SoftwarePathTracerRaysPerSecondText", Position: new float3(4f, 28f, 0.1f))
};
foreach (var expected in expectedRows) {
    Entity row = desktopTree.Single(entity => EntityName(entity) == expected.Name);
    TextComponent text = Component<TextComponent>(row);
    AssertVector(row.LocalPosition, expected.Position);
    Assert.Equal(0.35f, text.FontScale);
    Assert.Equal(TextAlignment.Left, text.Alignment);
    Assert.Equal(new int2(128, 12), text.Size);
}

Entity returnTarget = desktopTree.Single(entity => EntityName(entity) == "SoftwarePathTracerDesktopReturnTarget");
Assert.Equal(new float3(4f, 40f, 0.1f), returnTarget.LocalPosition);
Assert.Equal(new int2(64, 12), Component<InteractableComponent>(returnTarget).Size);
Assert.Single(returnTarget.Components.OfType<DemoDiscReturnToMenuComponent>());
Entity returnLabel = desktopTree.Single(entity => EntityName(entity) == "SoftwarePathTracerDesktopReturnLabel");
TextComponent returnText = Component<TextComponent>(returnLabel);
Assert.Equal(0.35f, returnText.FontScale);
Assert.Equal(TextAlignment.Left, returnText.Alignment);
Assert.Equal(new int2(64, 12), returnText.Size);
```

Also update the existing nested-presentation and Return-label tests so they require diagnostics directly under `SoftwarePathTracerDesktopHudRoot`, require no desktop `RoundedRectComponent`, require the invisible desktop interaction target, and continue to require the handheld Return label above its unchanged handheld rounded background.

- [ ] **Step 2: Run the focused test and verify RED**

Run outside the sandbox if MSBuild cannot rewrite the existing generated `obj` files:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathTracerSceneFactoryTests" -v:minimal
```

Expected: FAIL because the current desktop subtree contains `SoftwarePathTracerDesktopHudPanel` and two desktop `RoundedRectComponent` instances, uses `FontScale = 1f`, gives rows `320x24` bounds, and lacks `SoftwarePathTracerDesktopReturnTarget`.

- [ ] **Step 3: Implement the minimal text-only desktop authoring graph**

In `CreateDesktopHud`, delete the desktop rounded panel and rounded Return button creation. Create the three diagnostics directly under `SoftwarePathTracerDesktopHudRoot` with the exact row positions and text settings from Global Constraints. Create a plain child named `SoftwarePathTracerDesktopReturnTarget` at `(4,40,0.1)`, attach `InteractableComponent { Size = new int2(64, 12) }`, preserve the existing pointer-only `DemoDiscReturnToMenuComponent`, and create the `RETURN` label at local `(0,0,0.1)` with the exact text settings from Global Constraints.

Add the smallest `CreateHudTextEntity` overload or parameters needed to author desktop scale, alignment, and bounds while keeping the existing handheld calls at `FontScale = 1f`, `TextAlignment.Left`, and `320x24`. Do not change tracer execution, output sprite layout, resolution selection, or engine code.

- [ ] **Step 4: Run focused and adjacent tests and verify GREEN**

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathTracerSceneFactoryTests|FullyQualifiedName~RenderingSceneGeneratorSoftwarePathTracerTests" -v:minimal
```

Expected: all selected tests pass. Then run:

```powershell
rtk git diff --check -- assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs
```

Expected: exit code 0 with no whitespace errors.

- [ ] **Step 5: Regenerate and validate only the shared tracer scene**

```powershell
rtk dotnet run --project C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\project.heproj --editor-command menu.generate-rendering-scenes
```

Require exit code 0. Inspect `git status` immediately; retain the intended `assets/scenes/rendering/software_path_tracer.helen` result and do not stage unrelated regenerated/importer churn. Re-run `SoftwarePathTracerSceneFactoryTests` and `RenderingSceneGeneratorSoftwarePathTracerTests` against the regenerated graph.

- [ ] **Step 6: Commit only the HUD source, tests, generated scene, and this plan**

```powershell
rtk git add -- docs/superpowers/plans/2026-09-01-software-path-tracer-text-only-hud.md assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs assets/scenes/rendering/software_path_tracer.helen
rtk git diff --cached --check
rtk git commit -m "Compact software tracer HUD"
```

- [ ] **Step 7: Rebuild the existing Windows DirectX 11 DemoDisc artifact**

```powershell
$env:HELENGINE_ENGINE_USER_SETTINGS_ROOT = "C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\user_settings\windows-dx11-build-platforms"
rtk dotnet run --project C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\tools\build-waiter\helengine.buildwaiter.csproj -- --output C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\windows-build --require helengine_windows.exe -- powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\project.heproj -Platform windows -Configuration Release -BuildProfile release -Output C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\windows-build
```

Require exit code 0, a fresh nonempty `windows-build/helengine_windows.exe`, DirectX 11 selection, and no `codegen.exe` dialog or Application Error event.

- [ ] **Step 8: Launch and visually verify the corrected scene**

Close only the prior `helengine_windows` process if it is still running, launch the rebuilt executable visibly, navigate `Rendering -> Software Path Tracer`, and verify the progressive Cornell render is unobstructed except for four tiny left-aligned text rows at the upper-left. Verify no desktop rectangle is visible, diagnostics update, progressive rendering continues, and Return still works. Leave the corrected DemoDisc window open for Helena.
