# DemoDisc Software Path Tracer Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a small reusable DemoDisc-owned scalar CPU path tracer, its model-reference component, progressive scheduler, presentation controller, and deterministic tests without depending on GPU model creation.

**Architecture:** Five focused runtime files separate authored data, raw-model flattening, geometric acceleration, path sampling, and component lifecycle. The tracer receives compact world-space triangles, builds one flat BVH, accumulates `float3` per pixel, tone-maps one tile into a reusable RGBA8 buffer, and asks `RenderManager2D` to upload that rectangle. An injectable DemoDisc model source makes load/dispose ordering testable while production uses `Core.ContentManager` directly.

**Tech Stack:** C#/.NET 9, xUnit, HelenEngine math/entity/content APIs, scalar deterministic sampling.

**Spec:** `docs/superpowers/specs/2026-08-31-demodisc-software-path-tracer-design.md`

## Global Constraints

- All files in this plan live in DemoDisc. Do not add trace utilities to HelenEngine.
- Keep runtime implementation to these five main files: `SoftwareModelComponent.cs`, `SoftwareTraceScene.cs`, `SoftwareBvh.cs`, `SoftwarePathTracer.cs`, and `SoftwarePathTracerComponent.cs`. Test-only fakes may use separate files.
- Avoid LINQ, closures, per-ray arrays, per-pixel RNG state, and allocation inside tile/pixel/bounce loops.
- One scalar worker performs at most one tile of work per `Update()` call. Rendering continues indefinitely.
- `SoftwareTraceScene` owns compact geometry/material/BVH inputs; raw `ModelAsset` arrays must be disposed before accumulator allocation.
- Use a fixed-capacity traversal stack and reject a BVH deeper than the capacity.
- Use one global completed-pass count. Never allocate per-pixel sample counts or a full-resolution display buffer.
- Every public and private member follows the existing DemoDisc XML documentation convention.
- Every `Create: ...cs` entry in this plan also creates the adjacent `...cs.hmeta` JSON (`version: 1`, a new lowercase 32-hex `assetId`, empty `formerAssetIds`). Add that exact sidecar beside the source in the same task and stage it in the same commit; existing `.cs` files retain their current identities.

---

### Task 1: Define authored software-model data and persistence contract

**Files:**
- Create: `assets/codebase/rendering/SoftwareModelComponent.cs`
- Create: `assets/codebase/gameplay.tests/SoftwareModelComponentTests.cs`
- Create: `assets/codebase/rendering.tools.tests/SoftwareModelComponentPersistenceSourceTests.cs`

**Interfaces:**
- Consumes: engine `[CpuReadableModelReference]` from the engine-seams plan.
- Produces: `SoftwareModelComponent.ModelReference` and `SoftwareMaterial[] Materials`.
- Consumed by: model ingestion in Task 2 and Cornell authoring in the rollout plan.

- [ ] **Step 1: Write failing reflection/source tests.** Assert the exact name `SoftwareModelComponent`, the marked `SceneAssetReference ModelReference`, no `RuntimeModel` member, no `MeshComponent` inheritance/use, and software materials with diffuse/emissive values.

```csharp
[Fact]
public void Software_model_component_exposes_cpu_reference_and_materials_only() {
    Type type = typeof(city.rendering.SoftwareModelComponent);
    Assert.Equal(typeof(SceneAssetReference), type.GetProperty("ModelReference").PropertyType);
    Assert.Equal(typeof(city.rendering.SoftwareMaterial[]), type.GetProperty("Materials").PropertyType);
    Assert.Null(type.GetProperty("Model"));
}
```

Run:

```powershell
rtk dotnet test user_settings/generated_code/projects/gameplay.tests/gameplay.tests.csproj --filter FullyQualifiedName~SoftwareModelComponentTests
```

Expected: FAIL because the types do not exist.

- [ ] **Step 2: Implement the compact authored types.** Use reference types for automatic persistence:

```csharp
namespace city.rendering {
    public sealed class SoftwareMaterial {
        public float3 DiffuseColor { get; set; } = float3.One;
        public float3 EmissionColor { get; set; } = float3.Zero;
        public float EmissionStrength { get; set; }
    }

    public sealed class SoftwareModelComponent : Component {
        [CpuReadableModelReference]
        public SceneAssetReference ModelReference { get; set; }
        public SoftwareMaterial[] Materials { get; set; } = Array.Empty<SoftwareMaterial>();
    }
}
```

Add validation helpers only when they are specific to this component (finite, non-negative colors and emission); do not add engine validators.

- [ ] **Step 3: Add an automatic-persistence round trip.** In `rendering.tools.tests`, construct a component with the generated cube reference and two materials, serialize/deserialize through the existing automatic component descriptor, and compare every reference identity and scalar. Assert no resolver method for `RuntimeModel` is called.
- [ ] **Step 4: Run tests.** Run both new test classes; expected PASS.
- [ ] **Step 5: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwareModelComponent.cs assets/codebase/rendering/SoftwareModelComponent.cs.hmeta assets/codebase/gameplay.tests/SoftwareModelComponentTests.cs assets/codebase/gameplay.tests/SoftwareModelComponentTests.cs.hmeta assets/codebase/rendering.tools.tests/SoftwareModelComponentPersistenceSourceTests.cs assets/codebase/rendering.tools.tests/SoftwareModelComponentPersistenceSourceTests.cs.hmeta
rtk git commit -m "Add DemoDisc software model component"
```

---

### Task 2: Load, validate, flatten, and release raw model assets

**Files:**
- Create: `assets/codebase/rendering/SoftwareTraceScene.cs`
- Create: `assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs`
- Create: `assets/codebase/gameplay.tests/FakeSoftwareModelAssetSource.cs`

**Interfaces:**
- Consumes: `SoftwareModelComponent` instances, entity `WorldTransformMatrix`, `ModelAsset`, `ModelAssetIndexData`.
- Produces: compact `SoftwareTriangle[]`, `SoftwareMaterialData[]`, one `SoftwareAreaLight`, memory counters.
- Consumed by: BVH Task 4 and controller Task 7.

- [ ] **Step 1: Write failing ingestion tests.** Cover one load for repeated cube references, 16-bit and 32-bit indices, every instance transform, submesh-to-material mapping, null arrays, both/neither index stream, index count not divisible by three, out-of-range indices, missing materials, zero/multiple emitters, and disposal before the allocation callback.

```csharp
var source = new FakeSoftwareModelAssetSource(cubeReference, cubeAsset);
SoftwareTraceScene scene = SoftwareTraceScene.Build(instances, source);
Assert.Equal(1, source.LoadCount);
Assert.Equal(1, source.DisposeCount);
Assert.Equal(cubeTriangleCount * instances.Length, scene.Triangles.Length);
```

Run the class; expected FAIL.

- [ ] **Step 2: Define compact data and source ownership.** Keep the interface DemoDisc-owned:

```csharp
public interface ISoftwareModelAssetSource {
    ModelAsset LoadOwned(SceneAssetReference reference);
}

public sealed class ContentSoftwareModelAssetSource : ISoftwareModelAssetSource {
    readonly ContentManager Content;
    public ModelAsset LoadOwned(SceneAssetReference reference) {
        return Content.Load<ModelAsset>(reference.RelativePath, RuntimeContentProcessorIds.ModelAsset);
    }
}
```

`SoftwareTriangle` stores `P0`, `Edge1`, `Edge2`, geometric normal, material index, centroid, and bounds. `SoftwareMaterialData` stores only diffuse and pre-multiplied emission. Track `InitializationPeakOwnedBytes` and `SteadyStateOwnedBytes` using exact array element-size constants declared beside each compact type.

- [ ] **Step 3: Implement grouped sequential ingestion.** Build stable groups by reference identity (`SourceKind`, `ProviderId`, `AssetId`, `RelativePath`). For each group: load one owned `ModelAsset`, validate, transform vertices with `Entity.WorldTransformMatrix`, append triangles for all instances, then dispose/delete the raw asset in `finally` before opening the next group.

Use explicit transform math local to `SoftwareTraceScene`; transform points with the full affine matrix and compute the final geometric normal from transformed edges so non-uniform cube scales remain correct.

- [ ] **Step 4: Detect the single rectangular emitter.** Require exactly one emissive `SoftwareModelComponent`. Because the shared cube has one submesh, select the two largest coplanar triangles on the face oriented into the box and derive the sampling rectangle from their four corners; reject ambiguous faces or non-rectangular geometry. Store corner, edge vectors, inward normal, area, emission, and the selected emitter triangle indices for double-count rules. The thin cube's other faces may be intersected as emissive geometry but are not separate configured lights.
- [ ] **Step 5: Run tests and memory-order assertions.** Expected PASS and no `RenderManager3D` symbol in `SoftwareTraceScene.cs`.
- [ ] **Step 6: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwareTraceScene.cs assets/codebase/rendering/SoftwareTraceScene.cs.hmeta assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs.hmeta assets/codebase/gameplay.tests/FakeSoftwareModelAssetSource.cs assets/codebase/gameplay.tests/FakeSoftwareModelAssetSource.cs.hmeta
rtk git commit -m "Build compact CPU trace scenes from model assets"
```

---

### Task 3: Implement scalar ray, triangle, and AABB intersection

**Files:**
- Create: `assets/codebase/rendering/SoftwareBvh.cs`
- Create: `assets/codebase/gameplay.tests/SoftwareIntersectionTests.cs`

**Interfaces:**
- Consumes: `SoftwareTriangle`.
- Produces: `SoftwareRay`, `SoftwareHit`, `SoftwareBounds`, allocation-free intersection routines.
- Consumed by: BVH traversal and path kernel.

- [ ] **Step 1: Write failing table tests.** Cover front/back triangle hits, misses, barycentric edge hit, nearest positive hit, parallel ray, AABB inside origin, zero direction component, grazing slab, and finite epsilon behavior.
- [ ] **Step 2: Implement Möller-Trumbore triangle intersection and robust slab AABB intersection.** Use scalar operations and caller-supplied `minimumDistance`/`maximumDistance`; never normalize inside intersection.

```csharp
public static bool IntersectTriangle(
    ref SoftwareRay ray,
    ref SoftwareTriangle triangle,
    float minimumDistance,
    float maximumDistance,
    out SoftwareHit hit);

public static bool IntersectBounds(
    ref SoftwareRay ray,
    ref SoftwareBounds bounds,
    float maximumDistance);
```

Reject non-finite ray inputs at the path boundary rather than branching in every primitive test.
- [ ] **Step 3: Run tests.** Expected PASS.
- [ ] **Step 4: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwareBvh.cs assets/codebase/rendering/SoftwareBvh.cs.hmeta assets/codebase/gameplay.tests/SoftwareIntersectionTests.cs assets/codebase/gameplay.tests/SoftwareIntersectionTests.cs.hmeta
rtk git commit -m "Add scalar software trace intersections"
```

---

### Task 4: Build and traverse a deterministic bounded BVH

**Files:**
- Modify: `assets/codebase/rendering/SoftwareBvh.cs`
- Create: `assets/codebase/gameplay.tests/SoftwareBvhTests.cs`

**Interfaces:**
- Consumes: compact triangle bounds/centroids.
- Produces: `SoftwareBvhNode[]`, `int[] TriangleOrder`, `MaximumDepth`, nearest-hit traversal.
- Consumed by: `SoftwarePathTracer`.

- [ ] **Step 1: Write failing tests.** Assert deterministic node/order arrays, largest centroid-axis median split, leaf contiguity, each triangle occurs exactly once, traversal equals brute force for at least 1,024 hash-generated rays, nearest hit wins, and a deliberately pathological build exceeding the stack limit fails.
- [ ] **Step 2: Implement the builder.** Use in-place sorting of `TriangleOrder` for each range, a fixed leaf threshold of four, and deterministic tie-breaking by original triangle index. Record maximum depth during recursion. Builder allocations are allowed only during initialization.
- [ ] **Step 3: Implement traversal with a fixed local stack.** Set `TraversalStackCapacity = 64`; reject builds with `MaximumDepth >= TraversalStackCapacity`. Push far child then near child based on ray direction/child entry estimate. No brute-force fallback.

```csharp
int[] stack = TraversalStackScratch; // one tracer-owned reusable array, never per ray
int stackCount = 1;
stack[0] = 0;
```

Because v0 is one worker, the tracer owns one reusable stack. Document that this must become worker-local before multithreading.
- [ ] **Step 4: Run BVH and intersection tests.** Expected PASS.
- [ ] **Step 5: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwareBvh.cs assets/codebase/gameplay.tests/SoftwareBvhTests.cs assets/codebase/gameplay.tests/SoftwareBvhTests.cs.hmeta
rtk git commit -m "Add deterministic bounded software BVH"
```

---

### Task 5: Implement deterministic sampling and the four-bounce path kernel

**Files:**
- Create: `assets/codebase/rendering/SoftwarePathTracer.cs`
- Create: `assets/codebase/gameplay.tests/SoftwarePathSamplingTests.cs`
- Create: `assets/codebase/gameplay.tests/SoftwarePathKernelTests.cs`

**Interfaces:**
- Consumes: `SoftwareTraceScene`, BVH, camera ray, pixel/pass identity.
- Produces: finite linear RGB sample and ray/non-finite counters.
- Consumed by: progressive scheduling in Task 6.

- [ ] **Step 1: Write failing sampler tests.** Assert exact repeatability for identical `(x,y,pass,bounce,dimension)`, differing dimensions produce differing values, outputs lie in `[0,1)`, cosine hemisphere samples remain above the surface, and area-light samples stay inside the rectangle.
- [ ] **Step 2: Implement a stateless integer-hash sampler.** Combine all keys with fixed 32-bit constants and convert the upper 24 bits to float. Do not store RNG state.
- [ ] **Step 3: Write failing kernel tests.** Cover emitted light seen by a primary ray, one diffuse bounce, visibility/shadow ray, red/green color transfer, finite output, four-bounce termination, and direct-light double counting. The double-count test compares a forced diffuse-to-emitter path with direct sampling enabled and asserts emitter-hit emission is not added after a diffuse bounce.
- [ ] **Step 4: Implement the kernel.** At each of at most four diffuse hits:
  - intersect BVH and orient the geometric normal against the incoming ray;
  - add emitter radiance only for a directly visible primary emitter (`bounce == 0`);
  - sample the rectangular light, test visibility, and add Lambertian next-event contribution;
  - sample a cosine-weighted outgoing direction and multiply throughput by diffuse color;
  - offset new origins by one shared ray epsilon.

Discard a sample and increment `NonFiniteSampleCount` if radiance or throughput becomes non-finite. Increment `RayCount` for primary, bounce, and shadow rays.
- [ ] **Step 5: Run tests.** Expected PASS.
- [ ] **Step 6: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwarePathTracer.cs assets/codebase/rendering/SoftwarePathTracer.cs.hmeta assets/codebase/gameplay.tests/SoftwarePathSamplingTests.cs assets/codebase/gameplay.tests/SoftwarePathSamplingTests.cs.hmeta assets/codebase/gameplay.tests/SoftwarePathKernelTests.cs assets/codebase/gameplay.tests/SoftwarePathKernelTests.cs.hmeta
rtk git commit -m "Implement scalar DemoDisc path sampling"
```

---

### Task 6: Add resolution, progressive tiles, accumulation, and CPU tone mapping

**Files:**
- Modify: `assets/codebase/rendering/SoftwarePathTracer.cs`
- Create: `assets/codebase/gameplay.tests/SoftwareProgressiveRendererTests.cs`

**Interfaces:**
- Consumes: one trace scene and fixed camera constants.
- Produces: one sample per pixel per pass, completed SPP, one reusable RGBA8 tile, exact memory totals.
- Consumed by: controller/presenter Task 7.

- [ ] **Step 1: Write failing resolution and memory tests.** Assert:

```csharp
[Theory]
[InlineData("ds", 256, 192, 589_824)]
[InlineData("3ds", 320, 240, 921_600)]
[InlineData("windows", 320, 240, 921_600)]
[InlineData("gamecube", 320, 240, 921_600)]
public void Resolution_and_accumulator_bytes_match_contract(string platform, int width, int height, int bytes) {
    SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform(platform);
    Assert.Equal(width, resolution.Width);
    Assert.Equal(height, resolution.Height);
    Assert.Equal(bytes, resolution.AccumulatorBytes);
}
```

Include the remaining six platform ids. Assert no per-pixel count/RNG/display arrays via owned-byte accounting.
- [ ] **Step 2: Implement platform resolution and accumulator preflight.** Use `Core.PlatformInfo.Name`; only exact `ds` selects 256x192. Allocate `float3[width * height]` after scene/BVH completion. Catch allocation failure, release prior state, and expose a stable error.
- [ ] **Step 3: Implement interleaved tiles.** Use 8x8 tiles (edge-clipped), a deterministic coprime permutation of tile indices, and one `byte[8 * 8 * 4]` staging buffer. Tests assert every pixel exactly once per pass, SPP changes only after the last tile, and pass 1 uses a different deterministic offset than pass 0.
- [ ] **Step 4: Implement CPU presentation conversion.** For the completed tile, divide accumulated radiance by `completedPasses + 1` while the current pass is in progress, apply fixed exposure, ACES-style fitted tone mapping, linear-to-sRGB conversion, finite clamp, and RGBA8 quantization. Do not retain a full output image.
- [ ] **Step 5: Run tests.** Expected PASS and exact accumulator byte counts.
- [ ] **Step 6: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwarePathTracer.cs assets/codebase/gameplay.tests/SoftwareProgressiveRendererTests.cs assets/codebase/gameplay.tests/SoftwareProgressiveRendererTests.cs.hmeta
rtk git commit -m "Add progressive CPU path trace scheduling"
```

---

### Task 7: Orchestrate initialization, upload, HUD, Return, and disposal

**Files:**
- Create: `assets/codebase/rendering/SoftwarePathTracerComponent.cs`
- Create: `assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs`
- Create: `assets/codebase/gameplay.tests/SoftwarePathTracerTestRenderManager2D.cs`

**Interfaces:**
- Consumes: Tasks 1-6, `RenderManager2D.BuildTextureFromRaw`, `RenderManager2D.UpdateTextureRegion`, existing Return input/scene transition.
- Produces: staged scene lifecycle, progressive updates, HUD strings, idempotent cleanup.
- Consumed by: Cornell scene factory in the rollout plan.

- [ ] **Step 1: Write failing staged-lifecycle tests.** Cover success order, one failure at every stage, upload failure, repeated `Dispose`, dispose during partial initialization, Return during tracing, no work after Return, and exact release-once behavior for raw assets, presentation texture, scene arrays, BVH, accumulator, traversal stack, and tile staging.

Required order:

```text
validate -> create presentation/HUD -> load/flatten/dispose raw assets
-> build/validate BVH -> preflight/allocate accumulator -> trace tiles
```

- [ ] **Step 2: Implement the component state machine.** Use an enum (`Uninitialized`, `LoadingModels`, `BuildingBvh`, `Allocating`, `Tracing`, `Failed`, `Disposed`) and explicit owned fields. `ComponentAdded` begins initialization; `Update` traces at most one tile, uploads it, refreshes HUD text at a throttled interval, and polls the existing `DemoDiscReturnInputUtils` path on non-handheld platforms. DS/3DS leave button and touch Return to the existing handheld Return component so one input press cannot request two transitions.
- [ ] **Step 3: Create and update the presentation texture.** Build one blank opaque RGBA8 `TextureAsset`, call `BuildTextureFromRaw`, immediately dispose its raw color array after creation, assign the result to the authored output `SpriteComponent`, and call:

```csharp
renderManager2D.UpdateTextureRegion(
    PresentationTexture,
    tile.X,
    tile.Y,
    tile.Width,
    tile.Height,
    tracer.TileRgba8,
    tracer.TileRowPitch);
```

The component locates the output sprite and three HUD `TextComponent`s by stable child entity names authored by the scene factory. Missing/duplicate required entities fail initialization.
- [ ] **Step 4: Implement diagnostics.** Expose completed SPP, elapsed seconds, rays/second, non-finite sample count, initialization peak bytes, and steady-state bytes. HUD shows only `SPP`, elapsed, `rays/s`, and Return; memory/non-finite counters remain diagnostics/test properties.
- [ ] **Step 5: Implement failure and cleanup.** On any exception, stop tracing, release transient work, set the visible error text, and keep Return available. `Dispose` is idempotent and releases the runtime texture through its owning `RenderManager2D` before clearing managed/native-owned arrays.
- [ ] **Step 6: Run tests.** Expected PASS.
- [ ] **Step 7: Commit.**

```powershell
rtk git add -- assets/codebase/rendering/SoftwarePathTracerComponent.cs assets/codebase/rendering/SoftwarePathTracerComponent.cs.hmeta assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs.hmeta assets/codebase/gameplay.tests/SoftwarePathTracerTestRenderManager2D.cs assets/codebase/gameplay.tests/SoftwarePathTracerTestRenderManager2D.cs.hmeta
rtk git commit -m "Orchestrate progressive software path tracing"
```

---

### Task 8: Add deterministic end-to-end core verification

**Files:**
- Create: `assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs`
- Potential defect-fix scope: the five runtime files created above; change them only after the new reference test demonstrates the defect.

**Interfaces:**
- Consumes: complete core.
- Produces: a stable correctness gate independent of final scene authoring/GPU readback.

- [ ] **Step 1: Construct a tiny in-memory Cornell scene from triangles in the test.** Render 16x16 for 64 completed passes with a fixed camera and constants.
- [ ] **Step 2: Assert tolerance-based invariants, not a platform-specific byte hash.** Require all pixels finite, center luminance above a fixed minimum, corners darker than the emitter area, left neutral surface has measurably more red than green, right neutral surface has more green than red, and two runs on the same build are exactly equal.
- [ ] **Step 3: Run the full software test group.**

```powershell
rtk dotnet test user_settings/generated_code/projects/gameplay.tests/gameplay.tests.csproj --filter FullyQualifiedName~Software
rtk dotnet build user_settings/generated_code/projects/gameplay/gameplay.csproj
```

Expected: PASS/build succeeds.

- [ ] **Step 4: Run the ownership scans.**

```powershell
rtk rg -n "RenderManager3D|RuntimeModel|MeshComponent" assets/codebase/rendering/Software*.cs
rtk rg -n "FileMode\.(Create|Append|OpenOrCreate)|File\.Write|OpenWrite" assets/codebase/rendering/Software*.cs
```

Expected: no matches.

- [ ] **Step 5: Commit.**

```powershell
rtk git add -- assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs.hmeta assets/codebase/rendering/SoftwareModelComponent.cs assets/codebase/rendering/SoftwareTraceScene.cs assets/codebase/rendering/SoftwareBvh.cs assets/codebase/rendering/SoftwarePathTracer.cs assets/codebase/rendering/SoftwarePathTracerComponent.cs
rtk git commit -m "Verify DemoDisc software path tracer core"
```
