# Software Model Owned Return Codegen Fix Plan

> **Worker:** Implement with `superpowers:test-driven-development`; preserve the DemoDisc-only boundary established by the software path tracer plans.

**Goal:** Make the generated gameplay module pass C++ ownership analysis by declaring the existing `ModelAsset` ownership transfer on the software-model source contract.

**Root cause:** Windows build `1c2351c7-deca-4c9b-ba62-50f7e8d0b304` reached `assets-cooked`, then codegen failed with `CPPOWN001` at `ContentSoftwareModelAssetSource.LoadOwned`. The method returns a newly loaded, disposable `ModelAsset`, and `SoftwareTraceScene` disposes it in a `finally`, but neither the interface method nor the concrete implementation carries `NativeOwnedReturnAttribute`. C++ codegen therefore refuses to infer the non-null ownership boundary.

**Architecture:** Express the ownership contract where it already exists: annotate both `ISoftwareModelAssetSource.LoadOwned` and `ContentSoftwareModelAssetSource.LoadOwned` with `[NativeOwnedReturn]`. Do not change `ContentManager`, `ModelAsset`, codegen, the engine, or the load/dispose lifecycle.

**Files:**

- Modify: `assets/codebase/rendering/SoftwareTraceScene.cs`
- Modify: `assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs` (or the narrow existing software-trace-scene test file that owns source-contract tests)

## Task 1: Add the failing ownership contract test

- [ ] Add a reflection-based test requiring `NativeOwnedReturnAttribute` on the interface declaration and the production implementation.
- [ ] Keep the test focused on metadata; do not duplicate codegen implementation logic.
- [ ] Run the focused test and record a meaningful red result before changing production code.

## Task 2: Declare the existing transfer semantics

- [ ] Add `[NativeOwnedReturn]` directly above both `LoadOwned` declarations.
- [ ] Preserve the method signatures, loading path, `try/finally` disposal, and RAM-bounded group-at-a-time behavior.
- [ ] Do not annotate test doubles unless their own compilation requires it.

## Task 3: Verify the real failed boundary

- [ ] Run the focused ownership test.
- [ ] Run the relevant gameplay test project and `rtk git diff --check`.
- [ ] Rerun the retained Windows gameplay codegen command against `build-graph/code/gameplay/_project/gameplay.csproj`; require `CPPOWN001` to disappear and conversion to exit zero.
- [ ] If codegen exposes a different independent diagnostic, stop after recording it; do not expand this fix without a new root plan.

## Task 4: Commit narrowly

- [ ] Stage only the plan-approved production and test files.
- [ ] Commit as `Declare software model load ownership`.
- [ ] Leave the local Windows platform manifest, package output, codegen cache, and unrelated importer churn uncommitted and untouched.
