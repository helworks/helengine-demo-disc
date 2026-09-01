# DemoDisc Software Path Tracer Component Execution Plan

> **Worker boundary:** Root owns this plan and acceptance. A Luna xhigh worker implements it with strict red-green-refactor, then separate Luna xhigh spec and quality reviews run in sequence.

**Goal:** Add the DemoDisc-owned lifecycle component that creates one software-trace presentation texture, converts authored CPU-readable models into compact scene state, builds the BVH, initializes the accepted progressive tracer, renders and uploads at most one 8x8 tile per update forever, updates lightweight diagnostics, handles Return, and releases every owned resource exactly once.

**Scope:** Create `assets/codebase/rendering/SoftwarePathTracerComponent.cs`; create `assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs` and `SoftwarePathTracerTestRenderManager2D.cs`, with adjacent unique `.hmeta` files. A small reusable `SoftwarePathTraceSession` and its state enum may live in the component runtime file. Do not modify HelenEngine, generated projects, prior trace-core files, scene factories, menus, build configuration, or unrelated DemoDisc code in this task.

**Engine contract:** Compile against the accepted engine-seam worktree referenced by the generated projects. Use only `RenderManager2D.BuildTextureFromRaw`, validated `RenderManager2D.UpdateTextureRegion`, and `RenderManager2D.ReleaseTexture`. Do not add another upload abstraction to HelenEngine and do not use Vulkan-specific APIs.

## Corrected runtime ownership and reference design

- Runtime entities have no stable name property. The authored controller therefore persists four `SceneEntityReference` properties: output sprite, SPP text, elapsed text, and rays-per-second text. Resolve them through `SceneEntityRuntimeIdComponent` only after the hierarchy is initialized.
- Begin synchronous staged initialization in `ComponentInitialized`, not `ComponentAdded`. `ComponentAdded` may only reset local state. The initialized callback is the first point where all authored entity references and the complete model hierarchy are guaranteed to exist.
- Keep engine-facing component code thin. Put resource ordering and upload ownership in one small reusable `SoftwarePathTraceSession` in the same runtime file; this is DemoDisc feature code, not a general engine utility.
- `SoftwareTraceScene` owns managed compact arrays by reachability and has no disposal method. Cleanup drops the session reference to it after disposing BVH/progressive state. Do not pretend to dispose managed arrays individually.
- `SoftwareBvh` owns and disposes its node/order arrays. `SoftwarePathTracer.DisposeProgressive()` releases only its accumulator/tile arrays. The traversal stack is session-owned and is released by clearing the session reference after the tracer can no longer run.
- The runtime texture is owned by the session and must be released through the exact `RenderManager2D` instance that created it. Never call `RuntimeTexture.Dispose()` directly.
- No runtime file writes, save/checkpoint APIs, `RenderManager3D`, `RuntimeModel`, or `MeshComponent` are permitted.

## Required focused surface

Equivalent private naming is allowed, but the public authored/session contract and behavior are fixed:

```csharp
public enum SoftwarePathTraceSessionState {
    Uninitialized,
    CreatingPresentation,
    LoadingModels,
    BuildingBvh,
    AllocatingProgressiveBuffers,
    Tracing,
    Failed,
    Disposed
}

public sealed class SoftwarePathTraceSession : IDisposable {
    public SoftwarePathTraceSession(RenderManager2D renderManager2D);
    public SoftwarePathTraceSessionState State { get; }
    public RuntimeTexture PresentationTexture { get; }
    public SoftwareTraceScene Scene { get; }
    public SoftwareBvh Bvh { get; }
    public SoftwarePathTracer Tracer { get; }
    public string FailureMessage { get; }
    public long InitializationPeakOwnedBytes { get; }
    public long SteadyStateOwnedBytes { get; }

    public void Initialize(
        IReadOnlyList<Entity> sceneRoots,
        ISoftwareModelAssetSource modelSource,
        SoftwareTraceResolution resolution,
        SoftwareTraceCamera camera,
        float exposure,
        ISoftwareTraceBufferAllocator allocator = null);

    public SoftwareTraceTile RenderAndUploadNextTile();
    public void Dispose();
}

public sealed class SoftwarePathTracerComponent : UpdateComponent {
    public SceneEntityReference OutputSpriteEntityReference { get; set; }
    public SceneEntityReference SppTextEntityReference { get; set; }
    public SceneEntityReference ElapsedTextEntityReference { get; set; }
    public SceneEntityReference RaysPerSecondTextEntityReference { get; set; }
    public float3 TraceCameraOrigin { get; set; }
    public float3 TraceCameraForward { get; set; }
    public float3 TraceCameraRight { get; set; }
    public float3 TraceCameraUp { get; set; }
    public float VerticalFieldOfViewDegrees { get; set; }
    public float Exposure { get; set; }
}
```

The component exposes read-only diagnostics for current session state, completed SPP, elapsed trace seconds, total rays, rays/second, non-finite sample count, initialization peak bytes, steady-state bytes, and whether Return was requested. Authored settings must have deterministic safe defaults, but the rollout scene factory will set the final Cornell values.

## Staged initialization contract

The exact observable ownership order is:

```text
validate component settings/references
-> resolve output sprite and three text components
-> create blank opaque RGBA8 TextureAsset and BuildTextureFromRaw
-> dispose the raw TextureAsset immediately
-> load/flatten models; SoftwareTraceScene disposes every owned ModelAsset
-> build/validate SoftwareBvh
-> allocate one traversal stack
-> construct SoftwarePathTracer
-> allocate progressive accumulator and tile buffer
-> assign the live RuntimeTexture to the output SpriteComponent
-> Tracing
```

- The blank texture is exactly the selected trace resolution. Fill alpha bytes with 255, build synchronously, and dispose/delete the raw `TextureAsset` in `finally`. Do not retain its full-resolution CPU pixels.
- `sceneRoots` for production is exactly the controller's authored root (`new[] { Parent }`); `SoftwareTraceScene` recursively finds its `SoftwareModelComponent`s.
- Use `SoftwareTraceResolution.ForPlatform(OwnerCore.PlatformInfo.Name)`. Exact `ds` is 256x192; 3DS and every other accepted platform are 320x240.
- Construct the camera from the six authored values and use the accepted Task 6 validation. Do not normalize or silently repair authored bases.
- Every initialization-stage exception must roll back all completed later-owned resources, retain one stable non-empty failure message, enter `Failed`, and leave the session disposable. The component writes a concise `Trace error` message to the SPP text and leaves authored Return UI alive.
- Double initialization is rejected. `RenderAndUploadNextTile` is permitted only in `Tracing`.

## One-tile update and upload contract

Each component `Update()` while tracing:

1. On non-handheld platforms only, poll the existing `DemoDiscReturnInputUtils.WasReturnPressed(OwnerCore.Input)` path before tracing. Exact `ds` and `3ds` do not poll here; their existing handheld Return overlay owns button/touch handling.
2. If Return is pressed, mark the request once, clear `OutputSprite.Texture`, dispose the session, request the resolved DemoDisc main menu once, and return without tracing or uploading.
3. Otherwise call `session.RenderAndUploadNextTile()` exactly once.
4. The session calls accepted `SoftwarePathTracer.RenderNextTile()`, then calls:

```csharp
renderManager2D.UpdateTextureRegion(
    PresentationTexture,
    tile.X,
    tile.Y,
    tile.Width,
    tile.Height,
    Tracer.TileRgba8,
    Tracer.TileRowPitch);
```

5. A region-upload exception transitions the session to `Failed`, releases owned resources exactly once, prevents future tile work, and surfaces the stable error through the component HUD.

No call may upload the whole texture after initialization. Edge tiles must pass clipped width/height while retaining the fixed 32-byte source row pitch. The exact same runtime texture and tile array are reused across updates.

## Diagnostics and HUD contract

- Start elapsed timing when the session enters `Tracing`; use `OwnerCore.TotalElapsedSeconds`, not `Stopwatch` or wall-clock APIs.
- `CompletedSpp` delegates to `Tracer.CompletedPasses`; `TotalRays` and non-finite count delegate to the tracer.
- Rays/second is total rays divided by finite positive elapsed trace seconds, otherwise zero.
- Refresh HUD at most four times per trace second, and immediately when completed SPP changes or a failure occurs. HUD allocations are allowed only on these throttled refreshes, never inside the tile/pixel/bounce loops.
- Exact visible prefixes are `SPP: `, `Time: `, and `Rays/s: `. Keep memory and non-finite counters as diagnostic properties, not extra HUD rows.
- Compute documented byte diagnostics from explicit owned buffers only:
  - display texture estimate: `width * height * 4`;
  - scene: its existing exact counters;
  - BVH nodes: 32 bytes each; triangle order and traversal stack: 4 bytes each;
  - progressive: `Tracer.ProgressiveOwnedBytes`.
- `SteadyStateOwnedBytes` is the sum retained while tracing. `InitializationPeakOwnedBytes` is the maximum of presentation creation (runtime texture estimate plus raw blank RGBA), model conversion (`display + Scene.InitializationPeakOwnedBytes`), BVH construction, and final steady state. Document that runtime/backend allocator overhead is excluded.

## Cleanup contract

- Cleanup order is: stop further work, clear `OutputSprite.Texture`, `Tracer.DisposeProgressive()`, dispose BVH, release presentation texture through its creator, then clear tracer/stack/scene/UI references.
- `Dispose`, `ComponentRemoved`, failure rollback, and Return may converge on the same idempotent cleanup path. Release the runtime texture at most once even when upload throws and component disposal follows.
- Call `base.Dispose()` exactly once from the component's first disposal. Do not issue a scene transition from `Dispose` or `ComponentRemoved`.
- After cleanup: no upload/trace work is allowed; all diagnostic owned-byte totals are zero; repeated cleanup is harmless.

## TDD sequence

### 1. RED/GREEN: session presentation and initialization order

Create the two test files and sidecars. `SoftwarePathTracerTestRenderManager2D` records build/update/release calls, returns a fake `RuntimeTexture`, copies only requested upload rectangles for assertions, and can throw independently from build/update/release. It must implement the accepted protected region-update core, not duplicate public validation.

Use the existing `FakeSoftwareModelAssetSource` with a small real cube/rectangle fixture. Capture the initial compile failure before production code. Then assert:

- one blank opaque texture is built at the exact DS and Windows sizes;
- the raw blank `TextureAsset` is no longer retained after build;
- each owned raw model is disposed before progressive allocator callbacks run;
- real compact scene, BVH, stack, tracer, and buffers exist only in `Tracing`;
- exact steady/peak byte formulas and 256-byte reusable tile staging;
- a second `Initialize` is rejected.

### 2. RED/GREEN: one tile per update and exact rectangular upload

- One session call produces exactly one upload with returned tile coordinates, clipped dimensions, the exact tracer tile-array reference, and source pitch 32.
- A 9x9 fixture eventually uploads both full and clipped edge tiles without copying stale unused tile bytes.
- Two calls reuse the same runtime texture and tile buffer.
- Completed SPP advances only after the final tile.
- Warm the path, then assert the session render/upload method adds no managed allocation when the fake renderer's recording capacity is preallocated and recording is disabled for the allocation probe.

### 3. RED/GREEN: failure rollback and idempotent cleanup

Cover texture-build failure, invalid/raw-model failure, progressive accumulator failure, progressive tile failure, upload failure, explicit disposal during each reachable partial stage, repeated disposal, and component disposal after session failure. Assert stable state/message, no further uploads, raw asset disposal, BVH/progressive empty arrays, release-through-creator exactly once, null/cleared presentation reference, and zero owned-byte diagnostics.

If a failure cannot be injected without adding a production abstraction used nowhere else, exercise the nearest real boundary rather than adding a generic factory layer. Do not add interfaces for BVH construction, clocks, strings, entity lookup, or scene transitions.

### 4. RED/GREEN: component references, lifecycle, HUD, and Return policy

- Reflection/persistence tests require the four `SceneEntityReference` properties and camera/exposure properties; forbid public `RuntimeTexture`, `RuntimeModel`, or model-loading authoring properties.
- Resolve each reference by exact non-zero runtime scene entity id; reject missing, duplicate, wrong-component, and zero-id targets.
- Prove `ComponentAdded` does not initialize and `ComponentInitialized` initializes exactly once.
- Prove one `Update()` performs at most one tile/upload.
- Prove throttled HUD prefixes and SPP-immediate refresh with deterministic elapsed values through small pure formatting/refresh-decision methods; do not introduce a clock interface.
- Prove exact `ds`/`3ds` return policy skips controller polling; Windows/non-handheld Return disposes before transition and suppresses all later work. Where constructing a full input/scene manager would obscure the behavior, test a small static platform-policy method plus a source assertion that `Update` uses the existing DemoDisc return and scene-resolver paths.

### 5. Sensitivity checks

Temporarily perform two uploads per session tile and prove the exact one-upload test fails; restore. Temporarily release the runtime texture directly or omit creator release and prove the release-once ownership test fails; restore.

### 6. Final verification and commit

Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathTracerComponentTests|FullyQualifiedName~SoftwareProgressiveRendererTests|FullyQualifiedName~SoftwarePathSamplingTests|FullyQualifiedName~SoftwarePathKernelTests|FullyQualifiedName~SoftwareBvhTests|FullyQualifiedName~SoftwareIntersectionTests|FullyQualifiedName~SoftwareTraceSceneTests|FullyQualifiedName~SoftwareModelComponentTests" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/gameplay/gameplay.csproj --no-restore -v:minimal
rtk rg -n "RenderManager3D|RuntimeModel|MeshComponent|FileMode\.(Create|Append|OpenOrCreate)|File\.Write|OpenWrite|CreateText|Stopwatch" assets/codebase/rendering/SoftwarePathTracerComponent.cs
rtk git diff --check
```

Expected: all selected tests pass, gameplay builds, ownership/storage scan has no matches, and only the six planned source/sidecar files are changed.

Commit:

```powershell
rtk git add -- assets/codebase/rendering/SoftwarePathTracerComponent.cs assets/codebase/rendering/SoftwarePathTracerComponent.cs.hmeta assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs.hmeta assets/codebase/gameplay.tests/SoftwarePathTracerTestRenderManager2D.cs assets/codebase/gameplay.tests/SoftwarePathTracerTestRenderManager2D.cs.hmeta
rtk git commit -m "Orchestrate progressive software path tracing"
```

Report initial RED, both sensitivity REDs, final test/build counts, exact DS/Windows texture and memory assertions, upload/release counts, scan result, commit hash, and file list.
