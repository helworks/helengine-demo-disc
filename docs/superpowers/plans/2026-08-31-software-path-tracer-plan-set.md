# DemoDisc Software Path Tracer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship one indefinitely progressive, all-CPU Cornell-box path-tracing scene on every DemoDisc target while keeping feature code in DemoDisc and engine changes limited to CPU-model packaging and texture-region upload primitives.

**Architecture:** This is a dependency-ordered plan set. First add and verify the two narrow engine capabilities. Then implement the pure DemoDisc trace core against those contracts. Finally author the scene, connect menu/build packaging, and verify each platform. No tracing or Cornell-box logic belongs in HelenEngine.

**Tech Stack:** C#/.NET 9, xUnit, HelenEngine automatic scene persistence, generated C++ console runtimes, Direct3D 11, Vulkan, native console texture APIs, JSON scene/build configuration, PowerShell build scripts.

**Spec:** `docs/superpowers/specs/2026-08-31-demodisc-software-path-tracer-design.md`

## Global Constraints

- Execute the focused plans in order; a later plan may consume only interfaces explicitly produced by an earlier one.
- Keep `SoftwareModelComponent`, all rays/intersections/BVH/sampling/accumulation/tone mapping, lifecycle logic, scene constants, HUD, and tests in DemoDisc.
- The engine may add only `[CpuReadableModelReference]` packaging behavior and `RenderManager2D.UpdateTextureRegion`; it must not gain software-rendering helpers.
- Use `gamecube` as the platform id. Never add `gc` to `project.heproj` or build configuration.
- DS is 256x192. Windows, 3DS, GameCube, PS2, PSP, PS Vita, Wii, Wii U, and Switch are 320x240. 3DS is monoscopic and centers the image on its 400x240 top screen.
- No runtime storage writes are permitted: runtime code must not open writable streams or write samples, images, caches, or checkpoints to persistent storage.
- Use TDD for every behavior change. Make one focused commit after each task and stage only files named by that task.
- Preserve unrelated dirty-worktree changes. Engine and platform repositories are separate repositories and receive separate commits.

---

## Plan Dependency Map

| Order | Plan | Consumes | Produces |
| --- | --- | --- | --- |
| 1 | [Engine seams](2026-08-31-software-path-tracer-engine-seams.md) | Existing scene packager, content manager, 2D backends | Marked CPU-model companion packaging and validated rectangular RGBA upload on every backend |
| 2 | [DemoDisc trace core](2026-08-31-software-path-tracer-demodisc-core.md) | Engine seam contracts | Reusable scalar trace core, `SoftwareModelComponent`, progressive controller, lifecycle tests |
| 3 | [Scene and platform rollout](2026-08-31-software-path-tracer-scene-rollout.md) | Engine seams and trace core | Authored Cornell scene, menu/build integration, ten target builds, hardware checklist |

## Execution Checkpoints

- [ ] **Checkpoint 1: Finish the engine-seams plan.** Run its complete test matrix and record the engine/platform commit hashes before changing DemoDisc runtime code.
- [ ] **Checkpoint 2: Finish the DemoDisc-core plan.** Run all `gameplay.tests` trace tests, the generated-code build, and the 16x16 deterministic Windows reference render before authoring the final scene.
- [ ] **Checkpoint 3: Finish the scene-rollout plan.** Regenerate authored assets, run project integration tests, build all ten targets, and perform the documented hardware/emulator smoke tests.
- [ ] **Checkpoint 4: Run final cross-plan verification.** From `C:\dev\helprojs\demodisc`, run:

```powershell
rtk dotnet test user_settings/generated_code/projects/gameplay.tests/gameplay.tests.csproj --filter "FullyQualifiedName~Software"
rtk dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter "FullyQualifiedName~SoftwarePathTracer|FullyQualifiedName~Cornell"
rtk dotnet test user_settings/generated_code/projects/game.tools.tests/game.tools.tests.csproj --filter "FullyQualifiedName~DemoDiscBuildConfigTests|FullyQualifiedName~SoftwarePathTracer"
```

Expected: all selected tests pass with no skipped software-path-tracer cases.

- [ ] **Checkpoint 5: Audit the ownership boundary and storage behavior.** Run:

```powershell
$engineRoots = Get-ChildItem -LiteralPath C:\dev\helworks -Directory | Where-Object Name -Like 'helengine*'
foreach ($engineRoot in $engineRoots) {
    rtk rg -n "Ray|Bvh|PathTracer|Cornell|ToneMap" $engineRoot.FullName -g '*.cs' -g '*.cpp' -g '*.hpp'
}
rtk rg -n "FileMode\.(Create|Append|OpenOrCreate)|File\.Write|OpenWrite|CreateText" assets/codebase/rendering -g '*.cs'
```

Expected: the engine search finds no feature implementation (test names and the two generic seam docs/contracts are acceptable); the DemoDisc runtime-storage search finds no matches.

- [ ] **Checkpoint 6: Commit the final DemoDisc integration.** Stage only the files named by the rollout plan and commit:

```powershell
rtk git add -- project.heproj user_settings/build_config.json helenui/demodisc.json assets/codebase assets/scenes
rtk git commit -m "Add DemoDisc CPU path tracing showcase"
```
