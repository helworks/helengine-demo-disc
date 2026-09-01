# Software BVH Constructor Ownership Transfer Plan

> **Worker:** Implement with `superpowers:test-driven-development`; use the contained-failure codegen installed from csharpcodegen `3e2460d`.

**Goal:** Transfer cleanup responsibility for the newly allocated BVH node and triangle-order arrays into the constructed `SoftwareBvh`.

**Root cause:** Once `BuildNode` correctly borrowed its temporary buffers, codegen advanced to `CPPOWN002` at `SoftwareBvh.cs:256`: owned local `nodes` escapes into the `SoftwareBvh` constructor through an unclassified parameter. Both `nodes` and `order` are allocated by `Build`, stored by the constructor, and owned for the BVH lifetime.

**Architecture:** Mark the constructor's `nodes` and `triangleOrder` parameters `[NativeTakesOwnership]`, matching the existing documented lifetime and DemoDisc constructor-transfer patterns. Do not change allocation, fields, source-triangle borrowing, disposal, or public accessors in this slice.

**Files:**

- Modify: `assets/codebase/rendering/SoftwareBvh.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwareBvhTests.cs`

## Task 1: Add the failing constructor-contract test

- [ ] Reflect the non-public four-parameter `SoftwareBvh` constructor.
- [ ] Require `NativeTakesOwnershipAttribute` on parameters `nodes` and `triangleOrder` only.
- [ ] Run the focused test first and record the red missing-attribute result.

## Task 2: Declare the transfer

- [ ] Add `[NativeTakesOwnership]` to the two owned-array constructor parameters.
- [ ] Preserve all assignments, `Build`, and `Dispose` code unchanged.
- [ ] Do not classify `sourceTriangles` or unrelated boundaries without a converter diagnostic.

## Task 3: Verify safely

- [ ] Run the focused test and full `SoftwareBvhTests` filter, then `rtk git diff --check`.
- [ ] Rerun retained Windows gameplay codegen using the safe installed executable. Require the line-256 `nodes` diagnostic to disappear and any later failure to remain a normal stderr/exit-code result.
- [ ] Confirm no `codegen`, `WerFault`, or Application Error process remains; stop on the next independent diagnostic.

## Task 4: Commit narrowly

- [ ] Stage only the two planned files and commit as `Transfer BVH array ownership`.
- [ ] Leave unrelated worktree churn and build artifacts untouched.

