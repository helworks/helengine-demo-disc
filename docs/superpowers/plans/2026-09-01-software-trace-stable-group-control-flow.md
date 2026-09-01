# Software Trace Stable Group Control Flow Plan

> **Worker:** Implement with `superpowers:test-driven-development`; use contained-failure codegen `3e2460d` or newer.

**Goal:** Preserve stable first-seen grouping without merging borrowed and newly owned `ModelGroup` references into one local.

**Root cause:** Codegen reports `CPPOWN009` at `SoftwareTraceScene.cs:420`: `matchingGroup` is borrowed when found in `groups`, but owned when the missing branch allocates a new `ModelGroup`. Both paths merge before `matchingGroup.Instances.Add(instance)`.

**Architecture:** Track the matching group by integer index. Append directly through `groups[index]` on the existing path; allocate, populate, and add a new group only inside the missing path. No reference local crosses the ownership join. Preserve first-seen order, stable identity matching, and instance order.

**Files:**

- Modify: `assets/codebase/rendering/SoftwareTraceScene.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs`

## Task 1: Add a failing structural regression

- [ ] Add a narrow source/contract test requiring index-based group selection and rejecting the mixed-lifecycle `ModelGroup matchingGroup` local in `CreateStableGroups`.
- [ ] Keep existing grouping behavior tests as the semantic contract; run the new test red first.

## Task 2: Split existing/new group paths

- [ ] Replace `matchingGroup` with `matchingGroupIndex`, initialized to `-1` and set on the first stable match.
- [ ] Existing path: append through `groups[matchingGroupIndex]`.
- [ ] Missing path: create one branch-local group, append the instance, then add it to `groups`.
- [ ] Preserve matching criteria and ordering exactly.

## Task 3: Verify safely

- [ ] Run focused/full `SoftwareTraceSceneTests` and `rtk git diff --check`.
- [ ] Rerun retained gameplay codegen with the safe executable; require the `matchingGroup` diagnostic gone and stop at the next independent normal stderr diagnostic.
- [ ] Confirm no crash processes remain.

## Task 4: Commit narrowly

- [ ] Commit only the two planned files as `Separate software trace group ownership paths`.

