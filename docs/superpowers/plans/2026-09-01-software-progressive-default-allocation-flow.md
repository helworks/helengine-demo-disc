# Software Progressive Default Allocation Flow Plan

> **Worker:** Implement with `superpowers:test-driven-development`; use the contained-failure codegen installed from csharpcodegen `3e2460d`.

**Goal:** Remove the mixed borrowed/owned allocator local from progressive initialization while preserving the injectable failure-test seam.

**Root cause:** Gameplay codegen now reaches `CPPOWN009` at `SoftwarePathTracer.cs:726`: `selectedAllocator` is borrowed when the caller supplies `allocator`, but owned when the null branch constructs `DefaultSoftwareTraceBufferAllocator`. Those control-flow paths merge incompatible cleanup responsibilities. The default allocator is stateless and only wraps two array allocations, so its heap object is unnecessary—especially on RAM-constrained targets.

**Architecture:** Keep `ISoftwareTraceBufferAllocator` for injected tests. When it is null, allocate the accumulator and tile arrays directly; when it is non-null, call the injected allocator. Remove the default allocator adapter class and the mixed-lifecycle `selectedAllocator` local. Preserve validation, rollback, field publication order, exception wrapping, and allocation sizes.

**Files:**

- Modify: `assets/codebase/rendering/SoftwarePathTracer.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwareProgressiveRendererTests.cs`

## Task 1: Add the failing default-path contract

- [ ] Add a focused regression test requiring that the production assembly no longer contains `city.rendering.DefaultSoftwareTraceBufferAllocator`.
- [ ] Keep existing default-path and injected-failure tests as the behavioral contract.
- [ ] Run the new test first and record the red result against the current adapter class.

## Task 2: Separate allocation branches

- [ ] Remove `DefaultSoftwareTraceBufferAllocator` and `selectedAllocator`.
- [ ] Allocate `new float3[resolution.PixelCount]` and `new byte[TileRgba8Bytes]` directly only when `allocator == null`.
- [ ] Otherwise call the injected allocator for each buffer.
- [ ] Preserve exact post-allocation length/null checks, catch/reset/wrap behavior, and delayed field publication.

## Task 3: Verify safely

- [ ] Run the new focused test and full `SoftwareProgressiveRendererTests` filter, then `rtk git diff --check`.
- [ ] Rerun retained Windows gameplay codegen with the safe installed executable. Require `CPPOWN009 selectedAllocator` to disappear and any next diagnostic to be a normal stderr/exit-code result.
- [ ] Confirm no `codegen`, `WerFault`, or Application Error process remains; stop on the next independent diagnostic.

## Task 4: Commit narrowly

- [ ] Stage only the two planned files and commit as `Separate progressive allocation ownership paths`.
- [ ] Leave unrelated worktree churn and build artifacts untouched.
