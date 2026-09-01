# Software Path Tracer Registration and Scene Generation Plan

> **Worker:** Implement with `superpowers:test-driven-development` after the platform-presentation task is accepted.

**Goal:** Register the accepted software path tracer factory in DemoDisc's existing rendering generator and commit one validated generated `software_path_tracer.helen` scene.

**Architecture:** `RenderingSceneGenerator` owns one `SoftwarePathTracerSceneFactory`, passes it the engine cube as a raw `SceneAssetReference` (never `assets.GeneratedCubeModel`), passes the editor font used by the existing generator, and writes the resulting definition through `GeneratedAuthoringSceneWriteService`. There is one logical scene and no DS/3DS companion scene because platform overrides live inside the shared asset.

**Files:**

- Modify: `assets/codebase/rendering.tools/RenderingSceneGenerator.cs`
- Create: `assets/codebase/rendering.tools.tests/RenderingSceneGeneratorSoftwarePathTracerTests.cs`
- Create: `assets/codebase/rendering.tools.tests/RenderingSceneGeneratorSoftwarePathTracerTests.cs.hmeta`
- Generate: `assets/scenes/rendering/software_path_tracer.helen`

## Task 1: Add a failing registration contract

- [ ] Add focused tests that locate the active checkout instead of hard-coding `C:\dev\helprojs\demodisc`.
- [ ] Require `public const string SoftwarePathTracerSceneId = "scenes/rendering/software_path_tracer.helen";`.
- [ ] Require one `SoftwarePathTracerSceneFactory` field constructed with the existing `IEditorProjectAuthoringSession` only.
- [ ] Require the generator call to use `EngineSceneAssetReferenceFactory.CreateCubeModel()` and `editorCore.DefaultFontAssetForEditor`.
- [ ] Explicitly reject `assets.GeneratedCubeModel` as the software factory's geometry argument and reject any software-path-tracer DS/3DS companion scene ID.
- [ ] Require one `AuthoringSceneWriteService.WriteScene(softwarePathTracerSceneDefinition);` call after the accepted rendering showcase definitions.
- [ ] Run the focused filter and capture a meaningful red result.

## Task 2: Register the factory minimally

- [ ] Add the stable scene-id constant and readonly factory field to `RenderingSceneGenerator`.
- [ ] Construct the factory as `new SoftwarePathTracerSceneFactory(AssetAuthoringService)`; do not pass/store the generator transaction in the factory.
- [ ] In `Generate`, create the definition with:

```csharp
SoftwarePathTracerFactory.CreateSceneDefinition(
    projectRootPath,
    EngineSceneAssetReferenceFactory.CreateCubeModel(),
    editorCore.DefaultFontAssetForEditor);
```

- [ ] Write it once through the existing scene writer. Do not create a runtime model, material asset, companion logical scene, or new generation utility.
- [ ] Run focused tests and the rendering.tools/gameplay builds.

## Task 3: Generate only the shared scene asset

- [ ] Run the editor command against the isolated worktree project:

```powershell
rtk dotnet run --project C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\project.heproj --editor-command menu.generate-rendering-scenes
```

The editor must run from the accepted engine-seams worktree until that branch is integrated into engine `main`. The generated editor-command projects reference the launched editor's `helengine.core.dll`; launching the current main checkout would omit `CpuReadableModelReferenceAttribute` and fail DemoDisc gameplay compilation before the command handler runs.

- [ ] Require exit code `0` and `assets/scenes/rendering/software_path_tracer.helen` to exist.
- [ ] Inspect `git status` immediately. Existing generated scenes may be rewritten by the command; do not stage unrelated churn. If existing tracked scenes change semantically, diagnose before proceeding rather than silently discarding user changes.
- [ ] Do not create a `.helen.hmeta`; the stable identity comes from `ProjectAuthoringAssetIdentityCatalog` like the other generated rendering scenes.

## Task 4: Validate the generated asset

- [ ] Parse/load the generated scene through the existing scene test support or editor load path.
- [ ] Assert one `city.rendering.SoftwarePathTracerComponent`, eight `city.rendering.SoftwareModelComponent` records, one output sprite, desktop and handheld presentation roots, and stable nonzero entity references.
- [ ] Assert no traced entity has `helengine.MeshComponent`, no `RuntimeModel` payload is embedded for the cube reference, and no separate DS/3DS scene was emitted.
- [ ] Assert the scene's software model references all resolve to the same engine cube identity/reference.
- [ ] Assert the generated platform metadata contains DS `256x192`, 3DS `320x240` at X=`40`, DS/3DS handheld HUD controller-reference overrides, and mutually exclusive HUD roots.
- [ ] Run focused tests, rendering.tools/gameplay builds, and `rtk git diff --check`.

## Task 5: Commit

- [ ] Stage only the generator, new test and sidecar, and `assets/scenes/rendering/software_path_tracer.helen`.
- [ ] Commit as `Generate software path tracer scene`.
