# Software Path Tracer Borrowed Component Codegen Fix Plan

> **Worker:** Implement with `superpowers:test-driven-development`; continue from the retained Windows gameplay codegen repro only.

**Goal:** Let C++ ownership analysis recognize that `SoftwarePathTracerComponent.FindRequiredComponent<T>` returns an entity-owned component borrowed by the tracer.

**Root cause:** After the owned `ModelAsset` return was correctly classified, the same retained Windows codegen command advanced to `CPPOWN001` at `SoftwarePathTracerComponent.cs:595`. `FindRequiredComponent<T>` searches `candidate.Components`, returns that existing hierarchy-owned instance, and never allocates or transfers it, but its non-null generic return boundary is unclassified.

**Architecture:** Add `[NativeBorrowedReturn]` to this one lookup helper, matching established DemoDisc component lookup helpers such as `MenuComponent.FindRequiredComponent<TComponent>`. Preserve its exact search, duplicate detection, exception behavior, and callers. Do not change engine/codegen ownership inference or annotate unrelated methods speculatively.

**Files:**

- Modify: `assets/codebase/rendering/SoftwarePathTracerComponent.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwarePathTracerComponentTests.cs`

## Task 1: Add the failing borrowed-return contract test

- [ ] Add a reflection-based test that resolves the non-public generic `FindRequiredComponent` method and requires `NativeBorrowedReturnAttribute`.
- [ ] Assert enough method identity (name and generic method definition) that the test cannot silently select a different helper.
- [ ] Run only that test first and record the red missing-attribute result.

## Task 2: Classify the hierarchy-owned return

- [ ] Add `[NativeBorrowedReturn]` directly above `FindRequiredComponent<T>`.
- [ ] Preserve the signature, out entity result, lookup loops, duplicate validation, and four initialization call sites.
- [ ] Do not modify ownership of the containing `Entity`, its component list, or the tracer fields.

## Task 3: Verify the real converter boundary

- [ ] Run the new focused test and the relevant `SoftwarePathTracerComponentTests` filter.
- [ ] Run `rtk git diff --check`.
- [ ] Rerun the exact retained Windows gameplay codegen command. Require the line-595 `FindRequiredComponent` diagnostic to disappear.
- [ ] If a new independent diagnostic appears, record it and stop without adding another speculative annotation.

## Task 4: Commit narrowly

- [ ] Stage only the two plan-approved files.
- [ ] Commit as `Declare tracer component lookup borrowing` once the focused contract and original boundary are green.
- [ ] Leave importer churn, local Windows manifest, build output, and cache uncommitted and untouched.

