# DemoDisc Software Path Tracer Scene Rollout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Author the Cornell-box scene, expose it in DemoDisc, package it for all ten targets, and verify progressive presentation, memory release, and Return behavior on real runtime paths.

**Architecture:** One generated logical scene (`software_path_tracer`) owns a fixed 2D presentation camera, CPU-traced cube instances, one tracer controller, and two HUD layouts selected through platform existence overrides. DS/3DS use a bottom-screen HUD and an unobstructed top-screen image; all other targets use a translucent overlay. The same runtime tracer and scene id are packaged everywhere.

**Tech Stack:** DemoDisc rendering authoring modules, HelenEngine editor CLI, xUnit source/integration tests, JSON build configuration and HelenUI metadata, platform build scripts and console/emulator smoke tests.

**Spec:** `docs/superpowers/specs/2026-08-31-demodisc-software-path-tracer-design.md`

## Global Constraints

- Do not add a `MeshComponent` to any traced Cornell entity and do not create ordinary runtime materials for them.
- Reuse `EngineSceneAssetReferenceFactory.CreateCubeModel()` for every `SoftwareModelComponent`; packaging must emit one CPU companion.
- One logical scene id, `software_path_tracer`, is selected in every platform build.
- The common scene contains both HUD layouts, but platform existence overrides ensure exactly one is present at runtime.
- DS/3DS bottom-screen controls use the established DemoDisc scaffold conventions. The 3DS trace image is 320x240 centered with 40-pixel side margins on the 400x240 top screen and is never duplicated to a second eye.
- The only action is Return. Do not add pause, reset, save, exposure, resolution, or camera controls.
- Generated `.helen` files are committed only after generation and validation; scene identity comes from `ProjectAuthoringAssetIdentityCatalog`, matching existing generated scenes that have no `.helen.hmeta` sidecar.
- Every `Create: ...cs` entry in this plan also creates the adjacent `...cs.hmeta` JSON (`version: 1`, a new lowercase 32-hex `assetId`, empty `formerAssetIds`). Add that exact sidecar beside the source in the same task and stage it in the same commit; existing `.cs` files retain their current identities.

---

### Task 1: Author the fixed Cornell scene definition

**Files:**
- Create: `assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs`
- Create: `assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs`
- Create: `assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactorySourceTests.cs`
- Modify: `assets/codebase/scene.tools/ProjectAuthoringAssetIdentityCatalog.cs`

**Interfaces:**
- Consumes: `SoftwareModelComponent`, `SoftwareMaterial`, `SoftwarePathTracerComponent`, generated cube reference, authoring services.
- Produces: `GeneratedAuthoringSceneDefinition` at `scenes/rendering/software_path_tracer.helen`.
- Consumed by: `RenderingSceneGenerator` in Task 3.

- [ ] **Step 1: Write failing structural tests.** Require the exact scene path/id, one controller, one output sprite, one fixed presentation camera, eight cube-backed software-model entities, exactly one emissive rectangle, red left wall, green right wall, five open-front enclosure surfaces, two rotated white boxes, no traced `MeshComponent`, and immutable camera/exposure/light constants.

Expected instance table:

| Entity | Translation | Scale | Y rotation | Diffuse | Emission |
| --- | --- | --- | ---: | --- | --- |
| Floor | `(0,-1,0)` | `(2,0.05,2)` | 0 | white | none |
| Ceiling | `(0,1,0)` | `(2,0.05,2)` | 0 | white | none |
| Back | `(0,0,1)` | `(2,2,0.05)` | 0 | white | none |
| Left | `(-1,0,0)` | `(0.05,2,2)` | 0 | red | none |
| Right | `(1,0,0)` | `(0.05,2,2)` | 0 | green | none |
| Short box | `(-0.35,-0.55,0.15)` | `(0.6,0.9,0.6)` | `+0.30` rad | white | none |
| Tall box | `(0.38,-0.25,0.35)` | `(0.55,1.45,0.55)` | `-0.28` rad | white | none |
| Ceiling emitter | `(0,0.93,0)` | `(0.55,0.025,0.45)` | 0 | black | white, strength 14 |

Use these as v0 constants unless the 16x16 reference test shows the camera cannot see the full enclosure; any adjustment must update the test and remain authored/fixed.

- [ ] **Step 2: Implement the factory.** Public contract:

```csharp
public sealed class SoftwarePathTracerSceneFactory {
    public const string SceneId = "scenes/rendering/software_path_tracer.helen";

    public GeneratedAuthoringSceneDefinition CreateSceneDefinition(
        string projectRootPath,
        SceneAssetReference cubeReference,
        FontAsset hudFont);
}
```

Create `SoftwareModelComponent` instances with `Materials = [material]` because the generated cube has one submesh. Put ray-camera position/target/FOV, exposure, max bounces, and tile size on the controller. The ordinary `CameraComponent` renders only the output sprite/HUD and sees no `MeshComponent` geometry.

- [ ] **Step 3: Add stable identity.** Add:

```csharp
["scenes/rendering/software_path_tracer.helen"] = "1000000000000000000000000000001f",
```

Do not reuse or renumber existing identities.

- [ ] **Step 4: Run tests.**

```powershell
rtk dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter "FullyQualifiedName~SoftwarePathTracerSceneFactory|FullyQualifiedName~Cornell"
```

Expected: PASS.

- [ ] **Step 5: Commit.**

```powershell
rtk git add -- assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs.hmeta assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs.hmeta assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactorySourceTests.cs assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactorySourceTests.cs.hmeta assets/codebase/scene.tools/ProjectAuthoringAssetIdentityCatalog.cs
rtk git commit -m "Author DemoDisc software Cornell scene"
```

---

### Task 2: Author platform-specific presentation and HUD layouts

**Files:**
- Modify: `assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/NintendoDsRenderingSceneScaffoldSourceTests.cs`

**Interfaces:**
- Consumes: existing `ViewportComponent`, `ComponentPlatformEditingService`, DS bottom-screen camera conventions, Return components.
- Produces: top image plus exactly one runtime HUD layout for each platform.

- [ ] **Step 1: Write failing platform-layout tests.** Materialize platform overrides for `ds`, `3ds`, and `windows`. Assert:
  - DS: output viewport 256x192 on top; SPP/time/rays and touch Return on bottom.
  - 3DS: output is 320x240 at top-screen X=40, monoscopic; SPP/time/rays and touch Return on bottom.
  - Windows/non-handheld: 320x240 centered, translucent overlay with SPP/time/rays and standard Return.
  - exactly one HUD is enabled and no light/camera controls exist.

- [ ] **Step 2: Implement common output presentation.** Author an output `SpriteComponent` with null texture, exact pixel size, stable name `SoftwarePathTracerOutput`, and nearest/unaltered presentation settings. The controller assigns the runtime texture after initialization.
- [ ] **Step 3: Implement handheld-only bottom UI.** Follow `NintendoDsRenderingSceneScaffoldFactory`'s bottom-screen camera/viewport conventions, but create custom stats and Return roots. Apply existence overrides so this root exists only on `ds`/`3ds`. Use `NintendoDsReturnOverlayComponent` for touch Return and the shared runtime controller for button Return polling.
- [ ] **Step 4: Implement non-handheld overlay.** Add three text rows plus Return to a small translucent top-left panel. Apply inverse existence overrides for DS/3DS. Reuse `DemoDiscReturnToMenuComponent` with `AllowKeyboardReturn = false`, `AllowGamepadReturn = false`, and `AllowPointerReturn = true`; the tracer controller owns keyboard/gamepad polling on these targets, so there is exactly one transition request per binding. Do not add new input utilities.
- [ ] **Step 5: Run tests.** Expected PASS.
- [ ] **Step 6: Commit.**

```powershell
rtk git add -- assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs assets/codebase/rendering.tools.tests/NintendoDsRenderingSceneScaffoldSourceTests.cs
rtk git commit -m "Add software path tracer presentation layouts"
```

---

### Task 3: Register and generate the scene

**Files:**
- Modify: `assets/codebase/rendering.tools/RenderingSceneGenerator.cs`
- Create: `assets/codebase/rendering.tools.tests/RenderingSceneGeneratorSoftwarePathTracerTests.cs`
- Create after generation: `assets/scenes/rendering/software_path_tracer.helen`

**Interfaces:**
- Consumes: factory from Tasks 1-2 and rendering-scene generation assets.
- Produces: committed scene asset and prebuild-regenerable registration.
- Consumed by: build packaging in Task 5.

- [ ] **Step 1: Write a failing registration test.** Assert `RenderingSceneGenerator` declares `SoftwarePathTracerSceneId`, constructs the factory, creates the generated cube reference without a `RuntimeModel`, and writes the returned definition.
- [ ] **Step 2: Register the factory.** Add:

```csharp
public const string SoftwarePathTracerSceneId = "scenes/rendering/software_path_tracer.helen";
readonly SoftwarePathTracerSceneFactory SoftwarePathTracerFactory;
```

Construct it with the existing authoring session/transaction and call it from `Generate`. Use `EngineSceneAssetReferenceFactory.CreateCubeModel()` as the software reference; do not pass `assets.GeneratedCubeModel` into the factory.
- [ ] **Step 3: Run tests before generation.** Expected PASS.
- [ ] **Step 4: Generate the scene.** Run:

```powershell
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-rendering-scenes
```

Expected: command exits 0 and writes/updates `assets/scenes/rendering/software_path_tracer.helen`.

- [ ] **Step 5: Inspect the generated asset.** Run source/asset tests and confirm the serialized scene contains `city.rendering.SoftwareModelComponent`, not `helengine.MeshComponent`, on traced entities.
- [ ] **Step 6: Commit.**

```powershell
rtk git add -- assets/codebase/rendering.tools/RenderingSceneGenerator.cs assets/codebase/rendering.tools.tests/RenderingSceneGeneratorSoftwarePathTracerTests.cs assets/codebase/rendering.tools.tests/RenderingSceneGeneratorSoftwarePathTracerTests.cs.hmeta assets/scenes/rendering/software_path_tracer.helen
rtk git commit -m "Generate software path tracer scene"
```

---

### Task 4: Add DemoDisc catalog and HelenUI coverage

**Files:**
- Modify: `assets/codebase/menu.authoring/DemoDiscSceneCatalog.cs`
- Create: `assets/codebase/menu.tools.tests/DemoDiscSoftwarePathTracerCatalogTests.cs`
- Modify: `helenui/demodisc.json`

**Interfaces:**
- Consumes: logical scene id `software_path_tracer`.
- Produces: selectable `Software Path Tracer` entry and automation recognition metadata.

- [ ] **Step 1: Write a failing catalog test.** Require the new item after `PBR Shadow Theater` and before `Back`:

```csharp
new MenuItemDefinition(
    "scene-software-path-tracer",
    "Software Path Tracer",
    true,
    new MenuActionDefinition(MenuActionKind.LoadScene, "software_path_tracer"))
```

- [ ] **Step 2: Add the catalog item.** Keep all current entries and order stable.
- [ ] **Step 3: Update HelenUI.** Add `Software Path Tracer` to the rendering-surface recognition text list and add one menu item node at the next order with previous/next/activate and highlighted-text selected state, mirroring neighboring PBR nodes.
- [ ] **Step 4: Validate JSON and tests.** Run:

```powershell
Get-Content helenui/demodisc.json -Raw | ConvertFrom-Json | Out-Null
rtk dotnet test user_settings/generated_code/projects/menu.tools.tests/menu.tools.tests.csproj --filter "FullyQualifiedName~DemoDiscSceneCatalog|FullyQualifiedName~SoftwarePathTracer"
```

Expected: JSON parses and tests pass.
- [ ] **Step 5: Commit.**

```powershell
rtk git add -- assets/codebase/menu.authoring/DemoDiscSceneCatalog.cs assets/codebase/menu.tools.tests/DemoDiscSoftwarePathTracerCatalogTests.cs assets/codebase/menu.tools.tests/DemoDiscSoftwarePathTracerCatalogTests.cs.hmeta helenui/demodisc.json
rtk git commit -m "Expose software path tracer in DemoDisc"
```

---

### Task 5: Add the scene to every build and restore canonical GameCube support

**Files:**
- Modify: `project.heproj`
- Modify: `user_settings/build_config.json`
- Modify: `assets/codebase/game.tools.tests/DemoDiscBuildConfigTests.cs`
- Create: `assets/codebase/game.tools.tests/SoftwarePathTracerProjectIntegrationTests.cs`

**Interfaces:**
- Consumes: generated scene and selective CPU companion packaging.
- Produces: scene selection/order for Windows, DS, 3DS, GameCube, PS2, PSP, PS Vita, Wii, Wii U, Switch.

- [ ] **Step 1: Write failing configuration tests.** Assert:
  - `project.heproj.supportedPlatforms` contains exactly one `gamecube` and no `gc`;
  - it matches the ten ids in `settings/platforms.json`;
  - every `build_config.json` platform selects/orders `software_path_tracer` exactly once;
  - the scene follows `pbr_shadow_theater` on all targets;
  - all relevant debug/release prebuild profiles regenerate rendering scenes;
  - DS/3DS resolution source contract remains 256x192 and 320x240 respectively.

- [ ] **Step 2: Add `gamecube` to `project.heproj`.** Preserve formatting and all existing ids.
- [ ] **Step 3: Update all ten build blocks.** Append `software_path_tracer` to both `selectedSceneIds` and `sceneOrders`, using the next order number. Do not remove PS2's existing special profile behavior; add the scene to the profile/package that actually ships DemoDisc.
- [ ] **Step 4: Add/verify rendering-scene regeneration prebuild commands** for profiles that need generated asset freshness.
- [ ] **Step 5: Run tests.**

```powershell
rtk dotnet test user_settings/generated_code/projects/game.tools.tests/game.tools.tests.csproj --filter "FullyQualifiedName~DemoDiscBuildConfigTests|FullyQualifiedName~SoftwarePathTracerProjectIntegrationTests"
```

Expected: PASS.
- [ ] **Step 6: Commit.**

```powershell
rtk git add -- project.heproj user_settings/build_config.json assets/codebase/game.tools.tests/DemoDiscBuildConfigTests.cs assets/codebase/game.tools.tests/SoftwarePathTracerProjectIntegrationTests.cs assets/codebase/game.tools.tests/SoftwarePathTracerProjectIntegrationTests.cs.hmeta
rtk git commit -m "Package software path tracer on every target"
```

---

### Task 6: Verify the Windows reference runtime end to end

**Files:**
- Create: `assets/codebase/gameplay.tests/SoftwarePathTracerSceneRoundTripTests.cs`
- Modify only for defects found: software tracer runtime/scene files.

**Interfaces:**
- Consumes: complete Windows package.
- Produces: evidence for CPU companion load, progressive upload, visual GI, Return cleanup.

- [ ] **Step 1: Add a package round-trip integration test.** Package/load the scene with an instrumented renderer and assert no `BuildModelFromRaw`/`BuildModelFromCooked` call, exactly one cube companion load, at least two region uploads on distinct tiles, SPP advances after a full pass, and returning releases the texture and tracer allocations.
- [ ] **Step 2: Build Windows.** Run:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\artifacts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform windows -Output C:\dev\helprojs\demodisc\windows-build
```

Expected: exit 0 and `windows-build\helengine_windows.exe` exists.
- [ ] **Step 3: Inspect package contents.** Assert one `cooked/cpu-models/engine/cube.hasset` exists and deserializes as `ModelAsset`. Assert there is no ordinary model payload referenced by the software components and no tracer-created writable data directory.
- [ ] **Step 4: Run the artifact.** Enter `Software Path Tracer`; verify tiles appear progressively, SPP/time/rays update, image shows red/green bleeding after sufficient samples, rendering continues, Return loads the menu, and re-entry starts cleanly at 0 SPP.
- [ ] **Step 5: Capture diagnostics.** Record initialization peak bytes, steady-state bytes, first-pass time, and post-return tracer-owned bytes (must be zero) in the implementation commit/verification note.
- [ ] **Step 6: Run all DemoDisc tests.** Expected PASS.
- [ ] **Step 7: Commit the integration test and any proven fixes.**

---

### Task 7: Build the complete ten-platform matrix

**Files:**
- No planned source changes. Any build failure requires a new failing test in the owning repository before a fix.

**Interfaces:**
- Consumes: complete engine seams, tracer, scene, and configuration.
- Produces: one launchable artifact per target.

- [ ] **Step 1: Build each target into an explicit directory.** Run one at a time to keep logs attributable and RAM/disk pressure bounded:

```powershell
$platforms = @('windows','ds','3ds','gamecube','ps2','psp','psvita','wii','wiiu','switch')
foreach ($platform in $platforms) {
    rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\artifacts\build-platform.ps1 `
        -Project C:\dev\helprojs\demodisc\project.heproj `
        -Platform $platform `
        -Output ("C:\dev\helprojs\demodisc\output\software-path-tracer-" + $platform)
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $platform" }
}
```

Expected: all commands exit 0.

- [ ] **Step 2: Audit each package.** Confirm the scene manifest contains `software_path_tracer`, the CPU cube companion exists exactly once, and opaque-cook platforms retain their normal model cook behavior for unrelated scenes.
- [ ] **Step 3: Check codegen/runtime size regressions.** Compare each artifact against its pre-feature build. Explain the scene/component/companion increase; investigate any unrelated model-asset duplication.
- [ ] **Step 4: Run platform builder tests.** Run the complete focused matrix from the engine-seams plan. Expected PASS.
- [ ] **Step 5: Commit only if a failing test forced a platform fix.** Keep each fix in its owning repository.

---

### Task 8: Perform console smoke, memory, and longevity verification

**Files:**
- Create: `docs/verification/2026-08-31-software-path-tracer-hardware-results.md`

**Interfaces:**
- Consumes: all ten artifacts.
- Produces: acceptance evidence, including constrained-target memory and no-growth behavior.

- [ ] **Step 1: Use this result table.** Fill every cell with measured values; do not mark an unrun target as pass.

| Platform | Launch | Tiles progress | SPP progress | Resolution/layout | Return/re-entry | Init peak | Steady bytes | 30-minute growth | Storage writes |
| --- | --- | --- | --- | --- | --- | ---: | ---: | --- | --- |
| Windows | | | | 320x240 | | | | | none |
| DS | | | | 256x192 top, HUD bottom | | | | | none |
| 3DS | | | | centered 320x240 mono, HUD bottom | | | | | none |
| GameCube | | | | 320x240 | | | | | none |
| PS2 | | | | 320x240 | | | | | none |
| PSP | | | | 320x240 | | | | | none |
| PS Vita | | | | 320x240 | | | | | none |
| Wii | | | | 320x240 | | | | | none |
| Wii U | | | | 320x240 | | | | | none |
| Switch | | | | 320x240 | | | | | none |

- [ ] **Step 2: Smoke every target.** Enter scene, wait for visible tile and counter progress, invoke Return, re-enter, and confirm accumulation restarts. Slowness is acceptable; lack of forward progress is not.
- [ ] **Step 3: Run constrained longevity tests.** On DS and the next most memory-constrained functioning target, render at least 30 minutes. Record SPP before/after, memory before/after, non-finite count, and confirm no monotonic growth.
- [ ] **Step 4: Verify storage silence.** Observe platform filesystem/NAND/SD/memory-card access logs where available and compare before/after file listings. The scene must create or modify nothing.
- [ ] **Step 5: Verify cleanup.** After Return, diagnostic tracer-owned bytes must be zero and repeated scene entry must not lower available memory beyond normal allocator variance.
- [ ] **Step 6: Commit results.**

```powershell
rtk git add -- docs/verification/2026-08-31-software-path-tracer-hardware-results.md
rtk git commit -m "Verify software path tracer across targets"
```
