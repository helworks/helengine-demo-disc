# DemoDisc Software Path Tracer Reference Execution Plan

> **Worker boundary:** Root owns this plan and acceptance. A Luna xhigh worker implements it after the controller/session task is accepted, followed by separate Luna xhigh spec and quality reviews.

**Goal:** Add a deterministic, GPU-independent 16x16 Cornell-box reference render that exercises the complete accepted scalar BVH, path kernel, camera, progressive accumulation, pass scheduling, and cleanup for 64 samples per pixel.

**Scope:** Create `assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs` and its adjacent unique `.hmeta`. Change an accepted runtime file only if the new reference proves a specific defect first; record the failing invariant before such a fix. Do not create image files, golden hashes, test assets, renderer fakes, scene-authoring code, engine helpers, or runtime storage writes.

## Reference-scene contract

Construct the reference entirely from compact in-memory `SoftwareTriangle[]`, `SoftwareMaterialData[]`, one `SoftwareAreaLight`, a real `SoftwareBvh`, a real fixed 64-entry traversal stack, and a real `SoftwarePathTracer`.

Use a camera at `(0, 0, 3)` looking toward `-Z`, with `Right = +X`, `Up = +Y`, vertical FOV 55 degrees, square 16x16 resolution, and exposure 1. This basis obeys the accepted right/up/forward handedness convention and keeps image-left aligned with world `-X`.

The open-front box occupies `x/y/z` in `[-1, 1]`, with the front at `z = +1` omitted:

- floor at `y = -1`, inward normal `+Y`, neutral diffuse `(0.75, 0.75, 0.75)`;
- ceiling at `y = +1`, inward normal `-Y`, neutral diffuse;
- back at `z = -1`, inward normal `+Z`, neutral diffuse;
- left wall at `x = -1`, inward normal `+X`, diffuse `(0.75, 0.05, 0.05)`;
- right wall at `x = +1`, inward normal `-X`, diffuse `(0.05, 0.75, 0.05)`;
- one downward-facing rectangular emitter immediately below the ceiling, spanning `x = [-0.35, 0.35]`, `z = [-0.35, 0.35]`, with black diffuse and finite white emission `(10, 10, 10)`.

Represent every rectangle as exactly two consistently wound triangles. The explicit light corner/edges must match the two emissive triangles, its inward normal is `-Y`, its area is exactly `0.49`, and its two triangle indices identify the actual emitter geometry. Validate fixture normals and area in the test before rendering so a wrongly wound or disconnected light cannot create a misleading black reference.

No inner boxes are required in this core reference. The final authored showcase adds them; this test isolates enclosure illumination and red/green transfer with fewer triangles and a stronger deterministic signal.

## Render contract

- Initialize progressive tracing at exactly 16x16. There are four 8x8 tiles per pass.
- Call `RenderNextTile()` until `CompletedPasses == 64`; assert exactly `64 * 4 == 256` tile calls and never use a fixed call count without also checking the pass boundary.
- Snapshot linear radiance as `Accumulation[pixel] / 64f`. Do not assert the reusable final tile staging buffer as if it were a full display image.
- Render a second independently constructed fixture with the same constants. Compare every final accumulator channel using `BitConverter.SingleToInt32Bits`; same-build results must be bit-for-bit identical.
- Dispose progressive state and BVH for both fixtures in `finally`, then assert their owned arrays are empty and no reference render writes any file.

## Robust invariants instead of a golden hash

Calibrate conservative fixed thresholds from the first correct local run, record the observed values in assertion messages, and keep enough margin for supported float/codegen implementations. The committed test must assert all of the following:

1. Every accumulator and averaged channel is finite and non-negative; `NonFiniteSampleCount == 0`.
2. Every pixel received exactly 64 samples by the global pass/permutation contract; the accumulator contains at least one non-black pixel.
3. The central enclosure region has positive average luminance above a conservative fixed minimum.
4. The projected emitter region is measurably brighter than the average of the four 2x2 image-corner regions.
5. A neutral floor/back sample region adjacent to the image-left red wall has red greater than green by a fixed margin.
6. The mirrored neutral region adjacent to the image-right green wall has green greater than red by a fixed margin.
7. The two neutral transfer regions are not themselves wall pixels: fixture geometry/camera projection or a primary-ray classification helper in the test must prove their primary hits use the neutral material before their accumulated colors are compared.
8. `RayCount` is greater than the number of primary samples, proving secondary/shadow work occurred.
9. Two independent runs have identical tile-order completion, counters, and accumulator bit patterns.

Select small multi-pixel regions rather than one fragile pixel. Derive their exact coordinates from an initial diagnostic run, then remove temporary logging. Keep all region definitions and luminance/channel calculations test-local and allocation-free inside the render loop; allocations after rendering are acceptable for assertions but unnecessary.

## TDD sequence

### 1. Fixture geometry proof

Create the test and sidecar. Build helpers for a consistently wound quad and primary-hit material classification. Before a 64-pass render, assert:

- 12 total triangles: ten enclosure triangles plus two emitter triangles;
- exactly four compact materials;
- all triangle normals point inward as specified;
- the two emitter indices are in range, emissive, coplanar, and reconstruct the explicit 0.49-area rectangle;
- a center primary ray hits the neutral back wall and the selected transfer-region primary rays hit neutral material.

Capture an initial RED from a deliberately incomplete reference invariant or any proven runtime defect before adjusting production code. Do not manufacture a production change solely to create RED; this task's product is the new verification test.

### 2. Deterministic 64-pass render

Render one fixture through the real progressive scheduler. Assert 256 tile calls, 64 completed passes, finite non-negative accumulators, nonzero illumination, zero discarded samples, and secondary/shadow ray activity.

Render a second fresh fixture and compare every accumulator channel and counter exactly. The two fixtures must not share triangles, BVH arrays, traversal stacks, tracers, or accumulation arrays.

### 3. Cornell illumination invariants

Measure the central luminance, emitter-versus-corner contrast, and red/green transfer on primary-neutral multi-pixel regions. Use linear averaged radiance rather than tone-mapped bytes. Print observed values only while calibrating; commit fixed thresholds and descriptive failure messages, not console output or generated images.

If a threshold fails, first distinguish fixture/camera error from tracer error by classifying primary hits and checking direct emitter visibility. Change runtime production only after a minimal focused test demonstrates the same defect outside the reference aggregate.

### 4. Sensitivity checks

Perform and record both temporary mutations:

- Change both colored wall diffuse values to neutral white. Prove at least one red/green transfer assertion fails; restore the red/green materials.
- Give the second run a different pass identity/order (or perturb one accumulator input after rendering). Prove the bitwise repeatability assertion fails; restore identical independent runs.

These checks establish that the reference is sensitive to the two behaviors it claims to protect, rather than merely checking finite brightness.

### 5. Verification and commit

Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter FullyQualifiedName~SoftwarePathTracerReferenceTests -v:minimal
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter FullyQualifiedName~Software -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/gameplay/gameplay.csproj --no-restore -v:minimal
rtk rg -n "RenderManager3D|RuntimeModel|MeshComponent|FileMode\.(Create|Append|OpenOrCreate)|File\.Write|OpenWrite|CreateText" assets/codebase/rendering/Software*.cs assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs
rtk git diff --check
```

Expected: the focused reference passes, the entire software group passes, gameplay builds, ownership/storage scan has no matches, and only the reference test/source sidecar plus any separately proven runtime defect fix are changed.

Commit the normal no-defect case:

```powershell
rtk git add -- assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs assets/codebase/gameplay.tests/SoftwarePathTracerReferenceTests.cs.hmeta
rtk git commit -m "Verify DemoDisc software path tracer core"
```

Report fixture triangle/material counts, tile calls, completed SPP, ray/non-finite counts, observed invariant values and committed thresholds, both sensitivity REDs, exact repeatability result, final test/build counts, scan result, commit hash, and files.
