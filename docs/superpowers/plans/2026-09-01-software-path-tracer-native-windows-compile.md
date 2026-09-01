# Software Path Tracer Native Windows Compile Plan

> **Worker:** Implement with `superpowers:systematic-debugging` and `superpowers:test-driven-development`. Use `gpt-5.6-luna` at `xhigh`. Keep the contained-failure codegen executable `3e2460d` or newer installed throughout verification.

**Goal:** Turn the retained, successfully generated DemoDisc package into a compiling DirectX 11 Windows executable without moving tracer utilities into the engine or enabling Vulkan.

**Observed failure:** Build `205179c8-7b10-44a5-8400-14d6cb6ac3e5` completed code generation, asset cooking, gameplay compilation, container writing, and packaging. Native MSVC compilation then failed in 3 of 20 objects. The failures are reproducible in `native-build.log`; Windows event logs contain no post-fix `codegen.exe` crash event.

**Architecture:** Keep software tracing and its compatibility rewrites entirely in DemoDisc. The Windows owner gets only the DirectX 11 implementation of the engine seam it already consumes: a `RenderManager2D.UpdateTextureRegionCore` override backed by the existing uploaded D3D11 texture, plus current per-renderer fallback-texture calls. Preserve the dormant Vulkan implementation and flags, but do not select or run Vulkan.

**Repositories:**

- DemoDisc: `C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core`
- Windows host: create/use an isolated worktree under `C:\dev\helprojs\.worktrees`; do not implement against an unisolated dirty checkout.
- Engine seam input: `C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams`

## Task 1: Pin the native compiler failures as regressions

- [ ] Add narrow DemoDisc source/codegen compatibility tests for each unsupported construct actually present in the failed generated C++: unsupported exception payload/catch-variable access, three-argument `ArgumentOutOfRangeException`, `ObjectDisposedException`, `Math.Pow`, null-conditional/coalescing chains, abstract `IReadOnlyList<Entity>` materialization, nested member/type name collision, and inline struct-array initializers.
- [ ] Add or extend Windows host source tests for the required `UpdateTextureRegionCore` override and for renderer-instance-owned fallback textures.
- [ ] Run the new focused tests red before implementation. Do not loosen existing behavioral assertions.

## Task 2: Make the DemoDisc tracer codegen-portable

- [ ] Replace only unsupported exception forms with supported equivalents while preserving validation behavior. Do not surface exception `.Message` from generated gameplay; use stable local failure text.
- [ ] Replace `Math.Pow` in display encoding with a deterministic scalar helper that compiles through the native backend and remains within the existing reference-test tolerance.
- [ ] Expand tracer status null-propagation into explicit null checks with primitive return values; do not let `Nullable<T>` reach generated accessors.
- [ ] Keep scene-root storage concrete across the native boundary (array or `List<Entity>` as supported by codegen) so generated code never instantiates `IReadOnlyList<Entity>`.
- [ ] Rename the nested model-instance entity member so it cannot shadow the generated `Entity` type.
- [ ] Replace inline `float3[]` member-expression initializers with explicit allocation/assignment or equivalent scalar logic.
- [ ] Preserve all current ownership annotations, bounded allocations, progressive behavior, resolution rules, and CPU-only rendering semantics.
- [ ] Run the focused software tracer test suites and retained gameplay codegen. Require normal exit and no `codegen.exe` application crash event.

## Task 3: Align the existing Windows DX11 host with the accepted engine seam

- [ ] In the isolated Windows worktree, implement `Win32RenderManager2D::UpdateTextureRegionCore` with the existing DirectX 11 texture resource cache and `ID3D11DeviceContext::UpdateSubresource`/`D3D11_BOX`. Validate resource presence and preserve `sourceRowPitch`; do not rebuild the texture or allocate a full-frame staging copy.
- [ ] Give `Win32RenderManager3D` access to the active `Win32RenderManager2D` instance through construction/initialization, and pass it to `StandardMaterialTextureBindingDefaults::Apply`.
- [ ] Replace stale static `TextureUtils::get_PixelTexture()` fallback checks with that same active renderer's `get_PixelTexture()` so resources remain session-owned.
- [ ] Keep DirectX 11 as the selected build profile. Do not touch the experimental Vulkan gate or choose Vulkan.
- [ ] Run focused Windows source/unit tests, configure the retained native build, and compile until all host-owned errors are gone.

## Task 4: Iterate on the retained native build before another full package build

- [ ] Regenerate/copy gameplay output only when DemoDisc source changes require it; avoid repeating the 15-minute asset cook merely to inspect one compiler error.
- [ ] Re-run the existing CMake/Ninja native build directory and diagnose the first remaining error from its log. Fix only owner-local, reproducible issues and add a regression for each distinct source pattern.
- [ ] Require all 20 native objects and the final `helengine_windows.exe` link to succeed.
- [ ] Confirm no `codegen.exe`, `WerFault`, or error-dialog process remains; query Application events specifically for `codegen.exe` rather than attributing unrelated Blender failures to codegen.

## Task 5: Produce and smoke a fresh DX11 DemoDisc artifact

- [ ] Point the local uncommitted Windows platform manifest at the verified isolated Windows builder/player source and the accepted engine-seam worktree.
- [ ] Run the full bounded Windows package build and require state `success`, exit code `0`, and a fresh nonempty `windows-build\helengine_windows.exe`.
- [ ] Audit the packaged software scene/model companion invariants already listed in the Windows DX11 build plan.
- [ ] Launch only the DirectX 11 executable, navigate to Rendering -> Software Path Tracer, verify progressive 320x240 output continues rendering, then Return/re-enter and verify a clean reset.
- [ ] Run final focused tests and `rtk git diff --check` in each changed repository.

## Task 6: Commit narrowly

- [ ] DemoDisc: commit only planned tracer/test files; preserve unrelated importer churn and keep `windows-build` plus the local platform manifest uncommitted.
- [ ] Windows host: commit only DX11 compatibility source/tests in its isolated worktree.
- [ ] Do not commit generated cache/package artifacts or alter the Vulkan selection defaults.
