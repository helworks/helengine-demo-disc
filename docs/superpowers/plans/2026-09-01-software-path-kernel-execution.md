# DemoDisc Deterministic Sampling and Path-Kernel Execution Plan

> **Worker boundary:** Root owns this plan and acceptance. A Luna xhigh worker implements it with strict red-green-refactor, followed by separate spec and quality review.

**Goal:** Add the reusable scalar sampling and radiance kernel that traces one finite CPU sample through the compact DemoDisc scene and the accepted BVH, without accumulation, tiling, presentation, or per-path allocation.

**Scope:** Create `assets/codebase/rendering/SoftwarePathTracer.cs` and its adjacent `.hmeta`; create `SoftwarePathSamplingTests.cs`/`.hmeta` and `SoftwarePathKernelTests.cs`/`.hmeta` under `assets/codebase/gameplay.tests`. Do not modify HelenEngine, `SoftwareTraceScene.cs`, `SoftwareBvh.cs`, generated projects, scene authoring, or unrelated code.

## Fixed design

- All implementation remains in the single planned DemoDisc runtime file. Small sampler/kernel types in that file are acceptable; do not create a utility library.
- `MaximumDiffuseBounces` is exactly `4` and `RayEpsilon` is one shared positive constant.
- The tracer consumes the exact `SoftwareTriangle[]` used to build the BVH, the compact material array, one `SoftwareAreaLight`, the BVH, and one caller-owned traversal stack of at least `SoftwareBvh.TraversalStackCapacity`.
- The tracer retains references but does not own or dispose scene arrays, BVH, or traversal scratch. Task 7 owns staged cleanup.
- v0 has one scalar worker. The traversal scratch and tracer counters must become worker-local before any future multithreading.
- The public sample boundary accepts a normalized, finite, non-zero primary direction. Reject invalid pixel/pass identity or ray inputs before tracing; never normalize inside primitive intersection.
- No LINQ, delegates, closures, per-ray arrays, per-bounce collections, mutable RNG state, or allocation inside sampling/path loops.
- `RayCount` includes each primary/bounce BVH ray and each shadow BVH ray actually launched. `NonFiniteSampleCount` increments once for each discarded sample.

## Required surface and ownership

The implementation may choose equivalent names only when existing DemoDisc conventions require it, but must provide these behaviors:

```csharp
public static class SoftwarePathSampler {
    public static float Sample01(int pixelX, int pixelY, int completedPass, int bounce, int dimension);
    public static float3 SampleCosineHemisphere(float3 normal, float firstSample, float secondSample);
    public static float3 SampleAreaLight(ref SoftwareAreaLight light, float firstSample, float secondSample);
}

public sealed class SoftwarePathTracer {
    public const int MaximumDiffuseBounces = 4;
    public const float RayEpsilon = 0.0001f;
    public long RayCount { get; }
    public long NonFiniteSampleCount { get; }

    public SoftwarePathTracer(
        SoftwareTriangle[] triangles,
        SoftwareMaterialData[] materials,
        SoftwareAreaLight areaLight,
        SoftwareBvh bvh,
        int[] traversalStack);

    public float3 TraceSample(
        ref SoftwareRay primaryRay,
        int pixelX,
        int pixelY,
        int completedPass);
}
```

Constructor validation is narrow and cheap: reject null/empty arrays, invalid material indices, null/disposed BVH use when first traversed, undersized/null scratch, invalid light indices/area/vectors/emission, and non-finite compact values that could contaminate sampling. Do not duplicate raw-model validation.

## Deterministic sampler

- Hash all five signed integer keys into `uint` using fixed checked-in constants and `unchecked` arithmetic.
- Apply a final avalanche and convert the upper 24 bits with `1 / 16777216f`, yielding exactly `[0, 1)`.
- Do not retain RNG state. Repeated calls with identical keys are bit-identical on one build.
- Reserve dimensions `0,1` for the rectangular light and `2,3` for the cosine bounce at each bounce. Task 6 may use separate dimensions for primary jitter.
- Cosine sampling uses `r = sqrt(u)`, `phi = 2*pi*v`, `z = sqrt(1-u)`, and a deterministic orthonormal basis selected from the supplied unit normal. The returned direction must be finite, unit length within tolerance, and in the normal hemisphere.
- Area-light sampling is `Corner + u * Edge1 + v * Edge2`, including the half-open edge behavior inherited from `[0,1)`.

## Four-bounce radiance kernel

At each bounce from zero through three:

1. Increment `RayCount` and intersect the BVH in `[RayEpsilon, +infinity)` using the retained scratch.
2. On miss, terminate with current radiance.
3. Fetch the hit triangle/material and orient its geometric normal against the incoming direction.
4. If the material emits, add `throughput * emission` only when `bounce == 0`, then terminate. Emission reached after any diffuse bounce contributes zero; this is the explicit no-double-count rule for v0 next-event estimation.
5. For a diffuse hit, sample the rectangle with dimensions `0,1`. Compute:

```text
toLight = sampledPoint - hitPosition
lightDirection = normalize(toLight)
cosSurface = max(0, dot(orientedNormal, lightDirection))
cosLight = max(0, dot(light.InwardNormal, -lightDirection))
geometry = cosSurface * cosLight / distanceSquared
direct = throughput * diffuse * emission * (Area / pi) * geometry
```

6. Launch a shadow ray only when the geometry term is positive. Offset its origin by `orientedNormal * RayEpsilon`, then recompute the vector, normalized direction, and distance from that offset origin to the sampled light point. Increment `RayCount` and test through the BVH up to this recomputed `shadowDistance - RayEpsilon`. This keeps the sampled emitter outside the BVH's inclusive maximum bound; using the pre-offset direction/distance can make the emitter self-block. Any earlier hit blocks the contribution; there is no special GPU/renderer visibility path.
7. Add visible direct light, sample a cosine direction with dimensions `2,3`, multiply throughput component-wise by diffuse color (the Lambertian BRDF/pdf cancellation), offset the next origin by the oriented normal, and continue.

Use scalar component-wise color helpers local to this file if HelenEngine operators are insufficient. Do not add engine utilities.

After every radiance/throughput/contribution update, reject non-finite values. A rejected sample returns `float3.Zero`, increments `NonFiniteSampleCount` once, and never exposes partial radiance.

## TDD sequence

### 1. RED/GREEN: stateless sampling

Create `SoftwarePathSamplingTests.cs` and sidecar, then capture a focused failure before production exists. Cover:

- identical five-key inputs produce exact identical bits;
- changing each key, including `dimension`, changes the checked sample for fixed fixtures;
- at least 4,096 deterministic key tuples stay in `[0,1)` and finite;
- no retained per-pixel or RNG-state fields exist;
- cosine samples for axis-aligned and tilted unit normals are finite, unit length within tolerance, and never below the hemisphere;
- cosine samples are not all identical and include near-center and near-horizon cases;
- area-light samples at fixed `u,v` equal the exact affine point and remain inside the rectangle;
- invalid normal or samples outside `[0,1)` are rejected at this public helper boundary.

### 2. RED/GREEN: primary emission and direct light

Create `SoftwarePathKernelTests.cs` and sidecar. Use small hand-authored compact arrays, build the real `SoftwareBvh`, and pass the exact same triangle array to the tracer. Do not use mocked intersections.

Cover:

- a primary ray seeing emissive geometry returns its material emission and terminates;
- a diffuse surface with an unobstructed sampled light returns positive finite radiance;
- inserting a blocker between surface and every possible sampled light point removes the direct contribution;
- red and green diffuse fixtures transfer only their corresponding channel under neutral emission;
- counters include primary/bounce/shadow rays actually launched.

### 3. RED/GREEN: bounce and double-count rules

- Build a large diffuse plane and large emissive plane so the deterministic cosine bounce is forced to hit the emitter.
- Compare it with an otherwise identical fixture in which the bounce escapes while the same explicit light descriptor remains visible to the first diffuse hit.
- Assert both samples have the same direct contribution: the diffuse-to-emitter hit must not add emission after bounce zero.
- Assert the bounce ray was actually launched/hit so the comparison cannot pass vacuously.
- Use a closed diffuse fixture to prove no path performs more than four diffuse intersections. Assert the corresponding hard upper bound on primary/bounce plus shadow rays.

### 4. RED/GREEN: finite boundary and allocation

- Reject NaN/infinite/zero or materially non-unit primary directions before counters change.
- Construct finite-but-overflowing radiance/throughput inputs and assert the result is zero and `NonFiniteSampleCount` increases exactly once.
- Warm the JIT, then trace a deterministic batch while measuring `GC.GetAllocatedBytesForCurrentThread`; assert zero bytes are allocated by the sample loop. Keep assertions and fixture creation outside the measured interval.
- Verify consecutive samples reuse the supplied traversal array and retain deterministic outputs.

### 5. Sensitivity checks

Before finalizing, temporarily enable emission on diffuse-bounce emitter hits and show the double-count regression fails. Temporarily bypass shadow occlusion and show the blocker regression fails. Restore production before commit and report both RED results.

### 6. Verification and commit

Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathSamplingTests|FullyQualifiedName~SoftwarePathKernelTests|FullyQualifiedName~SoftwareBvhTests|FullyQualifiedName~SoftwareIntersectionTests" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/gameplay/gameplay.csproj --no-restore -v:minimal
rtk rg -n "System\.Linq|Random|new .+\[|List<|RuntimeModel|RenderManager3D|MeshComponent|File\.Write|OpenWrite" assets/codebase/rendering/SoftwarePathTracer.cs
rtk git diff --check
```

The runtime scan must have no matches. Commit only the six scoped files with message `Implement scalar DemoDisc path sampling` and report initial RED, both sensitivity REDs, final test/build results, scan result, hash, and exact files.
