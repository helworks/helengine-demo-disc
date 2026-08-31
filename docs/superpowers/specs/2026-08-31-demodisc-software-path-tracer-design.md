# DemoDisc Software Path Tracer Design

**Date:** 2026-08-31

**Status:** Approved for implementation planning

## Summary

DemoDisc will add one fixed-camera Cornell-box scene rendered by a progressive CPU path tracer. The tracer will run on Windows, DS, 3DS, GameCube, PS2, PSP, PS Vita, Wii, Wii U, and Switch. Ray generation, intersection, shading, sampling, accumulation, tone mapping, and pixel conversion all run on the CPU. Existing graphics backends only upload and present CPU-produced pixels.

The feature is owned by DemoDisc. Its `SoftwareModelComponent`, path tracer, BVH, sampling code, accumulation, scene generation, UI, lifecycle, and tests remain in DemoDisc project code. HelenEngine receives no ray-tracing or general software-renderer framework. Engine changes are limited to the smallest unavoidable primitives for preserving CPU-readable packaged model geometry and updating a presentable texture from CPU pixels.

Rendering continues until the user returns to the menu. All tracer state remains in RAM. The feature never writes samples, checkpoints, images, or caches to NAND, hard disks, memory cards, SD cards, or other runtime storage.

## Goals

- Demonstrate diffuse global illumination and color bleeding in a recognizable Cornell-box scene.
- Run the same scalar CPU tracing algorithm on all ten DemoDisc targets, even where it is extremely slow.
- Reuse ordinary model assets without creating GPU-resident models or geometry buffers.
- Update the displayed image progressively and indefinitely.
- Keep peak and persistent RAM use explicit and small enough for the constrained targets.
- Release every tracer-owned allocation when returning to the DemoDisc menu.
- Establish a small reusable path-tracing core that can accept other model-backed scenes later.

## Non-goals

- Real-time rendering or a target frame rate.
- GPU ray tracing, compute shaders, SIMD kernels, or platform-specific tracing acceleration.
- Multiple CPU workers in v0.
- A movable camera, animation, scene editing, or runtime model changes.
- Textures, reflection, refraction, participating media, denoising, or arbitrary production materials.
- Pause, reset, save, quality, exposure, or resolution controls.
- Runtime persistence of any kind.
- A HelenEngine ray-tracing subsystem or a large collection of engine-side utility functions.
- A prebuilt ray-scene or offline BVH format in v0.

## Ownership Boundary

### DemoDisc-owned code

DemoDisc owns all feature-specific behavior under its rendering runtime and rendering authoring modules:

- `SoftwareModelComponent`, including the model reference and compact per-submesh software material values.
- `SoftwarePathTracerComponent`, which owns scene initialization, progressive rendering, presentation, statistics, input, and disposal.
- The scalar path-tracing kernel, triangle and AABB intersection, deterministic sampler, BVH construction and traversal, accumulation, and tone mapping.
- The Cornell-box scene factory and authored constants.
- Rendering-catalog routing, build-scene inclusion, handheld presentation, and automated tests.

The implementation should use a few focused types with clear ownership rather than fragmenting the tracer into a large utility library. None of these feature-specific types move into HelenEngine.

### Minimal HelenEngine seams

Existing APIs must be reused before adding engine code. Only these missing capabilities may be added:

1. A narrow packaging/loading path that preserves a generic CPU-readable `ModelAsset` companion when a packaged console normally replaces the model with an opaque platform-owned GPU payload. The companion is emitted only for model references used by `SoftwareModelComponent`; ordinary models do not gain duplicate geometry.
2. A narrow `RenderManager2D` operation that updates a rectangular region of an existing `RuntimeTexture` from CPU pixel data. Texture creation continues to use `BuildTextureFromRaw`; the new operation only enables repeated pixel uploads.

No engine code may implement rays, intersections, a BVH, sampling, accumulation, materials, tone mapping, Cornell-scene behavior, statistics, or software-renderer orchestration. If an existing API can satisfy either capability, that engine change is omitted.

## Platform Matrix and Resolution

| Platform | Trace resolution | Presentation |
| --- | ---: | --- |
| DS | 256 x 192 | Top screen image; bottom screen statistics and Return control |
| 3DS | 320 x 240, monoscopic | Centered on the 400 x 240 top screen; bottom screen statistics and Return control |
| GameCube | 320 x 240 | Centered image with translucent overlay |
| PS2 | 320 x 240 | Centered image with translucent overlay |
| PSP | 320 x 240 | Centered image with translucent overlay |
| PS Vita | 320 x 240 | Centered image with translucent overlay |
| Wii | 320 x 240 | Centered image with translucent overlay |
| Wii U | 320 x 240 | Centered image with translucent overlay |
| Switch | 320 x 240 | Centered image with translucent overlay |
| Windows | 320 x 240 | Reference implementation with centered image and translucent overlay |

The final 3DS requirement supersedes the earlier full-width proposal: it renders 320 x 240 and does not render a second eye.

`project.heproj` must add the canonical `gamecube` platform identifier so it matches `settings/platforms.json` and `settings/platform.gamecube.json`. The identifier is `gamecube`, not `gc`.

## Cornell-box Scene

The generated scene appears in the DemoDisc Rendering catalog as the software path-tracing showcase and is selected in every platform build.

One shared cube model asset supplies all geometry:

- Floor, ceiling, back wall, red left wall, and green right wall.
- Two neutral-white interior boxes with different heights and rotations.
- One thin emissive rectangle inset into the ceiling.

The front remains open toward a fixed centered camera. The camera transform, field of view, exposure, light intensity, and tone-mapping constants are authored by the scene factory and cannot change at runtime.

The scene contains no ordinary 3D `MeshComponent` for traced geometry. Each traced entity has a DemoDisc `SoftwareModelComponent` containing:

- Its authored model-asset reference.
- Per-submesh base color.
- Per-submesh emission color and strength.
- The entity transform inherited from the ordinary scene hierarchy.

v0 materials are diffuse RGB or emissive only. Textures and ordinary runtime material assets are not loaded for the tracer.

## CPU Model Asset Flow

`SoftwareModelComponent` retains an asset reference; normal scene materialization must not turn that reference into a `RuntimeModel`. The path-tracer component groups instances by model reference and requests an owned CPU-readable `ModelAsset` for one group at a time.

For each unique model asset:

1. Load positions, normals, indices, submeshes, and bounds without calling `RenderManager3D`.
2. Validate that exactly one 16-bit or 32-bit index stream is active, every triangle index is in range, and every referenced material slot has a software material assignment.
3. Apply each entity's world transform and append its triangles to the compact trace-scene storage.
4. Preserve the winding and geometric normal required by ray intersection; retain authored vertex normals only where the compact v0 representation uses them.
5. Dispose the raw `ModelAsset` and all of its arrays before loading the next unique model.

The Cornell scene loads the cube once and expands its instances into one small world-space triangle array. v0 deliberately flattens instances before building one BVH. A separate instance hierarchy is deferred until a future scene demonstrates a need.

On platforms with opaque platform-owned model cooks, packaging emits a parallel generic `ModelAsset` companion only for these CPU references. The packaged component resolves directly to that companion. Loading the companion never creates or retains GPU geometry.

## Trace-scene Representation and BVH

The compact trace scene contains only the fields required by v0:

- Triangle positions or edges needed by the intersection kernel.
- A geometric or retained authored normal.
- One compact software material index.
- A material table containing diffuse RGB and emission.
- A flat BVH node array and triangle-order array.
- One emissive-rectangle descriptor for explicit light sampling.

The BVH is built in RAM after all raw model assets have been converted and disposed. A deterministic median split along the largest centroid-bounds axis is sufficient for v0. Leaves store contiguous triangle ranges. Traversal uses an explicit fixed-capacity local stack sized from the builder's maximum depth rather than allocating per ray.

The builder must fail initialization if its computed maximum depth cannot fit the traversal stack. It must not silently fall back to brute-force tracing.

## Path-tracing Algorithm

v0 uses one portable scalar worker and the same algorithm on every platform:

- One jittered primary sample per pixel per completed pass.
- Triangle-only intersections through the BVH.
- Cosine-weighted Lambertian diffuse sampling.
- Explicit next-event sampling of the ceiling emitter at diffuse hits.
- At most four diffuse bounces per path.
- A deterministic integer-hash random sequence derived from pixel coordinates, completed-pass index, bounce, and sample dimension.
- A fixed scene-authored exposure followed by tone mapping, linear-to-display conversion, and output quantization on the CPU.
- No denoising.

The light estimator must avoid counting the same emitter contribution twice. Non-finite radiance or throughput values discard the affected sample and increment a diagnostic counter rather than contaminating the accumulator.

The design does not require bit-identical floating-point output across different CPUs. The same input must produce deterministic results on one platform/build, and cross-platform images must agree within statistical and numeric tolerances.

## Progressive Scheduling and Presentation

The image is divided into small fixed-size tiles. Tiles use an interleaved deterministic order so early work is distributed across the image instead of filling it strictly from one corner. A pass is complete only after every pixel receives one sample. The displayed samples-per-pixel value reports completed passes.

After tracing a tile, the CPU tone-maps that tile into one reusable staging buffer and uploads only that rectangle into the presentation texture. There is no second persistent full-resolution CPU display buffer. The GPU or console graphics hardware performs presentation only; it does not trace, shade, accumulate, or tone-map.

The HUD displays:

- Completed samples per pixel.
- Elapsed render time.
- Rays per second.
- The platform-appropriate Return-to-menu control.

On DS and 3DS, the HUD lives on the bottom screen so the top image is unobstructed. On all other targets it is a small translucent overlay. The only runtime action is Return.

## Memory Model

Each pixel stores three 32-bit floating-point accumulation channels. Because every completed pass samples every pixel once, one global completed-pass counter replaces per-pixel sample counts. Stateless random generation removes the need for per-pixel RNG state. There is no depth buffer, G-buffer, denoising history, or saved checkpoint.

Persistent accumulator sizes are:

- DS, 256 x 192 x 12 bytes: 589,824 bytes (576 KiB).
- All 320 x 240 targets, including 3DS: 921,600 bytes (900 KiB).

Additional persistent memory consists of the compact triangle/material arrays, BVH arrays, presentation texture, component state, and HUD state. CPU staging memory is one tile. The implementation must allocate the full accumulator only after all raw model assets have been disposed and the compact trace scene has been finalized, minimizing peak memory.

The implementation must expose measured initialization peak and steady-state tracer-owned bytes in tests or diagnostics. It must not claim the feature fits DS solely from the accumulator calculation; the full packaged runtime must be measured on hardware or an equivalent platform memory report.

## Scene Lifecycle

Selecting the showcase uses the existing single-scene transition and persistent loading overlay. The main menu is not retained additively behind the tracer.

Initialization stages are:

1. Validate scene structure and authored constants.
2. Create the blank presentation texture and HUD.
3. Load and convert unique CPU model assets sequentially.
4. Dispose all raw model assets.
5. Build and validate the compact BVH.
6. Preflight and allocate the accumulator and tile staging memory.
7. Begin progressive tile tracing.

Returning to the menu stops new tracing work, releases the presentation texture through its owning renderer, disposes the accumulator, compact geometry, BVH, staging buffer, and component state, and then requests the standard main-menu scene. Disposal is idempotent and also handles partial initialization.

## Failure Handling

Initialization uses staged ownership with rollback. Any model-load, validation, conversion, BVH, allocation, or presentation failure disposes every resource created by earlier stages before entering a visible error state. The Return control remains usable.

The component rejects:

- A missing fixed camera.
- No `SoftwareModelComponent` instances.
- No emissive ceiling light or more than one configured emitter in v0.
- Missing CPU-readable model companions.
- Null or malformed geometry arrays.
- Both or neither populated index widths where triangles are required.
- Out-of-range or non-triangle index counts.
- Missing per-submesh software materials.
- Invalid camera, light, exposure, or resolution constants.
- A BVH that exceeds the fixed traversal-stack capacity.
- A failed memory preflight or allocation.

There is no GPU-rendering fallback, hard-coded-geometry fallback, lower-resolution fallback, or runtime-storage fallback. Presentation upload failure stops tracing, releases transient work, reports a clear diagnostic, and preserves Return.

## Verification

### DemoDisc unit and source-contract tests

- `SoftwareModelComponent` serialization preserves the authored model reference and per-submesh diffuse/emissive values.
- Runtime model ingestion does not call `RenderManager3D`, create a `RuntimeModel`, or add a `MeshComponent`.
- Repeated references load one raw cube asset and expand every instance transform correctly.
- Raw model arrays are disposed before accumulator allocation.
- Triangle and AABB intersection tests cover hits, misses, parallel rays, edge hits, and nearest-hit selection.
- BVH traversal returns the same nearest hit as brute force across deterministic ray sets.
- The BVH builder is deterministic and respects the fixed traversal-stack bound.
- Sampling is deterministic on one platform/build and produces finite radiance.
- Direct emitter sampling does not double-count emission.
- A small Windows reference render matches a tolerance-based image expectation after a fixed sample count.
- Tile ordering covers every pixel exactly once per pass, and completed samples per pixel advances only after a full pass.
- Resolution tests require DS 256 x 192 and every other platform, including 3DS, 320 x 240.
- Accumulator-size tests require exactly 589,824 bytes for DS and 921,600 bytes elsewhere.
- Partial initialization failures and repeated disposal release every owned object once.
- Menu-to-tracer-to-menu round trips do not retain tracer allocations.

### Engine seam tests

- CPU-readable model companions are emitted only for software-model references on opaque-cook platforms.
- A packaged CPU companion deserializes as `ModelAsset` without renderer-owned model creation.
- The texture-region update contract validates bounds, format, stride, ownership, and disposed textures.
- Each active 2D backend implements the same region-update behavior and retains normal release semantics.

### Project integration tests

- The Rendering catalog contains the software path-tracing entry.
- The Cornell scene contains the fixed camera, expected cube instances, exactly one emitter, the correct red/green walls, and no ordinary traced `MeshComponent`.
- The scene and its CPU model companion are included in Windows, DS, 3DS, GameCube, PS2, PSP, PS Vita, Wii, Wii U, and Switch builds.
- `project.heproj` and `settings/platforms.json` both contain the canonical `gamecube` identifier.
- Handheld builds place the image on the top screen and HUD on the bottom screen.
- No feature code opens a writable runtime content or storage stream.

### Hardware smoke tests

Each console build must launch the scene, display completed tiles, advance samples and statistics, accept Return, and load the menu again. Memory reporting must confirm that peak and steady-state tracer ownership are released after returning. DS and the other most constrained targets receive explicit long-duration smoke tests; slowness is acceptable, but lack of forward progress, memory growth, corruption, or storage writes is not.

## Acceptance Criteria

The feature is accepted when:

- All ten targets include and can enter the scene.
- DS renders at 256 x 192; every other target, including monoscopic 3DS, renders at 320 x 240.
- The progressively refined image visibly shows the Cornell box, its two interior boxes, the ceiling emitter, soft illumination, and red/green color bleeding.
- All tracing and pixel-generation work runs on the CPU; graphics hardware only presents uploaded pixels.
- The image continues accumulating until Return is selected.
- The HUD reports samples per pixel, elapsed time, and rays per second.
- Traced geometry comes from CPU-readable model assets without GPU model creation.
- No runtime storage writes occur.
- Returning to the menu releases every tracer-owned allocation.
- Feature-specific implementation remains in DemoDisc, with only the two proven-minimal engine capabilities added where existing APIs are insufficient.
