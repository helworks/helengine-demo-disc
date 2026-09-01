# DemoDisc Progressive CPU Renderer Execution Plan

> **Worker boundary:** Root owns this plan and acceptance. A Luna xhigh worker implements it with strict red-green-refactor, then separate spec and quality review.

**Goal:** Extend the accepted scalar path tracer with fixed platform resolution, camera-ray generation, one reusable accumulator, deterministic interleaved 8x8 tiles, CPU tone mapping, and one reusable RGBA8 tile buffer. This task does not create textures, call render managers, update components, or author the Cornell scene.

**Scope:** Modify `assets/codebase/rendering/SoftwarePathTracer.cs`; create `assets/codebase/gameplay.tests/SoftwareProgressiveRendererTests.cs` and its adjacent unique `.hmeta`. Do not modify HelenEngine, prior tests, generated projects, scene/controller files, or unrelated DemoDisc code.

## Fixed ownership and memory design

- DS is exactly `256 x 192`. Every other supported target, including monoscopic 3DS, is exactly `320 x 240`.
- Supported platform ids are `ds`, `3ds`, `gamecube`, `ps2`, `psp`, `psvita`, `switch`, `wii`, `wiiu`, and `windows`. Only the exact ordinal string `ds` selects the smaller resolution.
- Each pixel owns one `float3` accumulation value: exactly 12 bytes per pixel. DS accumulator bytes are exactly `589,824`; every 320x240 target is exactly `921,600`.
- There is no per-pixel sample count, RNG state, depth, G-buffer, denoising history, or persistent full-resolution display buffer.
- Persistent progressive buffers are one `float3[width * height]` accumulator and one `byte[8 * 8 * 4]` tile staging array. The tile array is reused for every upload.
- The existing scene triangles, materials, BVH, and traversal stack remain borrowed. Progressive disposal clears only progressive-owned arrays/state and never disposes borrowed state.
- Buffer allocation occurs only after Task 2 scene conversion and Task 4 BVH construction, when the controller calls progressive initialization.
- Initialization preflights checked dimensions/counts/bytes before allocation. Any allocation or allocator-contract failure rolls back to empty progressive state and throws one stable `InvalidOperationException` message.
- Keep the narrow allocation seam in this same runtime file. One small `ISoftwareTraceBufferAllocator` interface is permitted for deterministic failure tests; do not create a separate utility file.

## Required focused types and surface

Equivalent naming is allowed only for an established DemoDisc convention, but behavior and ownership are fixed:

```csharp
public readonly struct SoftwareTraceResolution {
    public int Width { get; }
    public int Height { get; }
    public int PixelCount { get; }
    public long AccumulatorBytes { get; }
    public SoftwareTraceResolution(int width, int height);
    public static SoftwareTraceResolution ForPlatform(string platformId);
}

public readonly struct SoftwareTraceCamera {
    public float3 Origin { get; }
    public float3 Forward { get; }
    public float3 Right { get; }
    public float3 Up { get; }
    public float VerticalFieldOfViewDegrees { get; }
}

public readonly struct SoftwareTraceTile {
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int TileIndex { get; }
}

public interface ISoftwareTraceBufferAllocator {
    float3[] AllocateAccumulator(int pixelCount);
    byte[] AllocateTileRgba8(int byteCount);
}
```

Extend `SoftwarePathTracer` without changing its accepted Task 5 constructor/kernel:

```csharp
public const int TileSize = 8;
public const int TileRgba8Bytes = 8 * 8 * 4;
public bool IsProgressiveInitialized { get; }
public SoftwareTraceResolution Resolution { get; }
public int CompletedPasses { get; }
public int TileRowPitch { get; } // always TileSize * 4
public float3[] Accumulation { get; }
public byte[] TileRgba8 { get; }
public long ProgressiveOwnedBytes { get; }

public void InitializeProgressive(
    SoftwareTraceResolution resolution,
    SoftwareTraceCamera camera,
    float exposure,
    ISoftwareTraceBufferAllocator allocator = null);

public SoftwareTraceTile RenderNextTile();
public void DisposeProgressive();
```

The public arrays expose the owned buffers for controller upload/reference verification; callers must not replace or mutate them. Document that future multithreading requires worker-local traversal/tile scratch and synchronized or partitioned accumulator ownership.

## Camera-ray contract

- Camera origin and basis must be finite. `Forward`, `Right`, and `Up` must be unit length and mutually orthogonal within one documented tolerance; reject a left-handed/degenerate basis and FOV outside `(0, 179)` degrees.
- Pixel jitter uses `SoftwarePathSampler.Sample01(x, y, completedPass, -1, 0/1)` so it is stateless and distinct from bounce dimensions.
- For pixel center `(x + jitterX, y + jitterY)`, compute aspect-correct NDC, use top-left image origin, apply `tan(verticalFov/2)`, and normalize exactly once at this path boundary.
- `TraceSample` continues to receive normalized primary directions and performs no primitive normalization.

## Deterministic tile scheduling

- Tiles are 8x8 and edge-clipped for arbitrary validated test resolutions.
- `tilesX = ceil(width/8)`, `tilesY = ceil(height/8)`, and `tileCount = tilesX * tilesY`, all checked.
- Sequence position `0..tileCount-1` maps through `(position * step + passOffset) % tileCount`.
- Choose one deterministic step near the golden-ratio fraction of `tileCount`, adjust deterministically until it is coprime with `tileCount`, and never choose zero. This distributes early tiles while guaranteeing a permutation.
- Derive a deterministic non-zero pass offset for pass 1 and later; pass 1 must start at a different tile than pass 0.
- `RenderNextTile()` traces every pixel in exactly one tile, accumulates one sample, converts that same tile into the reusable staging buffer, then advances one sequence position.
- Increment `CompletedPasses` only after the final tile of the pass is fully traced/tone-mapped. There is one global completed-pass count and no per-pixel count.
- Guard completed-pass overflow with a stable failure rather than wrapping sampler identity.

## Accumulation and CPU conversion

For the current tile:

1. Generate the jittered primary ray using the current `CompletedPasses` identity.
2. Call the accepted `TraceSample`, add finite radiance component-wise to the pixel accumulator, and reject any unexpected non-finite sum without contaminating stored accumulation.
3. Divide the updated pixel by `CompletedPasses + 1` while the current pass is in progress.
4. Multiply by fixed positive finite exposure.
5. Apply the ACES fitted curve per channel:

```text
mapped = x * (2.51*x + 0.03) / (x * (2.43*x + 0.59) + 0.14)
```

6. Clamp finite output to `[0,1]`, convert linear to sRGB (`12.92*x` below `0.0031308`, otherwise `1.055*x^(1/2.4)-0.055`), and round to nearest byte.
7. Write packed RGBA with alpha `255` into `TileRgba8` at `localY * TileRowPitch + localX * 4`. Do not allocate or clear another display buffer.

Tone mapping must defensively map negative/NaN values to black and positive infinity to white, even though the kernel returns finite non-negative radiance.

## TDD sequence

### 1. RED/GREEN: platform resolution and exact memory

Create `SoftwareProgressiveRendererTests.cs` and sidecar, then capture a focused failure before production changes. Cover all ten platform ids. Assert:

- `ds`: 256x192, 49,152 pixels, 589,824 accumulator bytes;
- every other supported id, including `3ds` and `gamecube`: 320x240, 76,800 pixels, 921,600 bytes;
- exact ordinal behavior (`DS` is not the DS id);
- positive custom resolutions use checked pixel/byte math and reject zero, negative, or overflow;
- the runtime type has no per-pixel count/RNG/display arrays by field inspection and owned-byte accounting.

### 2. RED/GREEN: allocation staging and rollback

- Initialize a small real tracer only after its real BVH exists.
- Assert exact accumulator/tile lengths, row pitch 32, and owned bytes `AccumulatorBytes + 256`.
- Inject accumulator failure and tile failure separately. Assert the stable exception, empty buffers, `IsProgressiveInitialized == false`, and no retained partially allocated array.
- Reject double initialization. Assert `DisposeProgressive()` is idempotent, clears only progressive arrays, preserves borrowed triangle/BVH/stack objects, and permits no further tile work.

### 3. RED/GREEN: camera and one-tile scheduling

- Validate a canonical camera and reject non-finite, non-unit, non-orthogonal, wrong-handed/degenerate bases and invalid FOV/exposure.
- Assert deterministic camera rays for fixed pixel/pass, finite unit directions, center rays near forward, top/bottom vertical orientation, and pass-dependent jitter.
- On a small custom edge-clipped resolution, assert one call processes exactly one tile, returns correct bounds, and reuses the exact same 256-byte staging object.

### 4. RED/GREEN: complete pass permutation

- Track test-only pixel visitation outside production. Across one pass, every pixel is covered exactly once, no tile repeats, and `CompletedPasses` remains zero until the final tile finishes.
- After the final tile, assert `CompletedPasses == 1`; the next call begins pass 1 at a different deterministic tile.
- Assert the chosen step is coprime with tile count and early tiles are not a simple adjacent raster run.
- Repeat a two-pass run twice and compare tile order and accumulator values exactly on the same build.

### 5. RED/GREEN: accumulation and tone mapping

- Use a real BVH scene whose huge primary-visible emissive plane returns constant finite radiance. After one and two completed passes, assert each accumulator equals one/two times the sample and the displayed average is unchanged.
- Assert black converts to `(0,0,0,255)`, a fixed midtone matches a checked expected RGBA value, exposure changes brightness monotonically, very large finite/positive infinity converts to white, and negative/NaN converts to black.
- Assert edge-tile rows use the fixed 32-byte pitch and unused staging bytes are irrelevant/not uploaded by the returned width/height.
- Warm JIT, render repeated tiles on a prepared tracer, and assert zero managed bytes are allocated by `RenderNextTile()`.

### 6. Sensitivity and verification

Temporarily change pass completion to increment one tile early and prove the pass-boundary test fails; restore. Temporarily use a raster tile index instead of the coprime permutation and prove the interleaving test fails; restore.

Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwareProgressiveRendererTests|FullyQualifiedName~SoftwarePathSamplingTests|FullyQualifiedName~SoftwarePathKernelTests|FullyQualifiedName~SoftwareBvhTests|FullyQualifiedName~SoftwareIntersectionTests" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/gameplay/gameplay.csproj --no-restore -v:minimal
rtk rg -n "System\.Linq|Random|new .+\[|List<|RuntimeModel|RenderManager3D|MeshComponent|File\.Write|OpenWrite" assets/codebase/rendering/SoftwarePathTracer.cs
rtk git diff --check
```

The runtime allocation scan may match only progressive initialization/default-allocator array construction. It must not match `RenderNextTile`, camera-ray generation, tile ordering, tone mapping, or helpers reachable from those loops.

Commit only `SoftwarePathTracer.cs`, `SoftwareProgressiveRendererTests.cs`, and `SoftwareProgressiveRendererTests.cs.hmeta` with message `Add progressive CPU path trace scheduling`. Report initial RED, both sensitivity REDs, final counts/build, exact memory assertions, scan result, hash, and files.
