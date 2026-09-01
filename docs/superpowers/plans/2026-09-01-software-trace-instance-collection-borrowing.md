# Software Trace Instance Collection Borrowing Plan

> **Worker:** Implement with `superpowers:test-driven-development`; use contained-failure codegen `3e2460d` or newer.

**Goal:** Keep the constructor-owned `instances` scratch list owned by its caller while recursive entity scanning appends to it.

**Root cause:** Codegen now reports `CPPOWN001` at `SoftwareTraceScene.cs:290`: owned local `instances` crosses the `CollectInstances` parameter without an ownership contract. `CollectInstances` recursively mutates that list but never stores, returns, or transfers it.

**Architecture:** Mark only `CollectInstances`'s destination-list parameter `[NativeNoEscape]`. Preserve recursion and component discovery unchanged; do not pre-classify later grouping/material/triangle boundaries.

**Files:**

- Modify: `assets/codebase/rendering/SoftwareTraceScene.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs`

## Task 1: Add the failing contract test

- [ ] Reflect private static `CollectInstances` and require `NativeNoEscapeAttribute` on parameter `instances`.
- [ ] Run the focused test first and record red.

## Task 2: Declare the recursive borrow

- [ ] Add `[NativeNoEscape]` to only `CollectInstances`'s `instances` parameter.
- [ ] Preserve all recursion, validation, and append logic.

## Task 3: Verify safely

- [ ] Run focused/full `SoftwareTraceSceneTests` and `rtk git diff --check`.
- [ ] Rerun retained gameplay codegen with the safe executable; require the line-290 diagnostic gone and stop at the next independent normal stderr diagnostic.
- [ ] Confirm no crash processes remain.

## Task 4: Commit narrowly

- [ ] Commit only the two planned files as `Declare trace instance collection borrowing`.

