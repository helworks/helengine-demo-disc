# Software BVH Build Buffer Borrowing Plan

> **Worker:** Implement with `superpowers:test-driven-development`; use only the contained-failure codegen build at csharpcodegen commit `3e2460d` or newer.

**Goal:** Classify the deterministic BVH builder's temporary arrays as caller-owned, nonescaping buffers while recursive build helpers mutate them.

**Root cause:** After the tracer component lookup was classified, gameplay codegen advanced to `CPPOWN001` at `SoftwareBvh.cs:251`, specifically the locally owned `order` array passed into `BuildNode`. `Build` must retain ownership of the newly allocated `order` and `nodes` arrays so it can transfer them into the final `SoftwareBvh`; `BuildNode` and its recursion only borrow and mutate those buffers during the call.

**Architecture:** Mark the `BuildNode` array parameters that it does not retain as `[NativeNoEscape]`. This is parameter-boundary metadata only: no allocation, sorting, recursion, ownership transfer, field storage, or disposal behavior changes. Cover both locally owned build buffers (`order` and `nodes`) because they have the same lifetime and are passed together; retain the source-triangle behavior unchanged unless codegen identifies it separately.

**Files:**

- Modify: `assets/codebase/rendering/SoftwareBvh.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwareBvhTests.cs`

## Task 1: Add the failing helper-contract test

- [ ] Reflect the private static `BuildNode` method and identify its `order` and `nodes` parameters by name.
- [ ] Require `NativeNoEscapeAttribute` on both temporary build-buffer parameters.
- [ ] Run the focused test first and record the red missing-attribute result.

## Task 2: Declare nonescaping build buffers

- [ ] Add `[NativeNoEscape]` to only the `order` and `nodes` parameters of `BuildNode`.
- [ ] Preserve the recursive calls and every sort/build operation unchanged.
- [ ] Do not add ownership annotations to fields, constructor parameters, source triangles, traversal buffers, or unrelated helpers without a separate diagnostic.

## Task 3: Verify safely

- [ ] Run the focused ownership-contract test and the full `SoftwareBvhTests` filter.
- [ ] Run `rtk git diff --check`.
- [ ] Rerun retained Windows gameplay codegen using the rebuilt contained-failure executable. Require the line-251 `order` diagnostic to disappear; require any later failure to remain a normal stderr/exit-code result with no `WerFault` or MessageBox process.
- [ ] Stop and report the next independent diagnostic rather than widening this patch.

## Task 4: Commit narrowly

- [ ] Stage only the two plan-approved files and commit as `Declare BVH build buffer borrowing`.
- [ ] Leave importer churn, local manifests, cache, and package output untouched.
