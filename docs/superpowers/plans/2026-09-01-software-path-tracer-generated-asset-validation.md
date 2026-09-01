# Software Path Tracer Generated-Asset Validation Plan

> **Worker:** Implement with `superpowers:test-driven-development` after Task 3 commit `de77e400`.

**Goal:** Close the accepted Task 3 review gaps by proving that the committed scene's serialized viewport overrides and controller entity references are valid.

**Architecture:** Extend the existing generated-asset test only. Deserialize base and platform override payloads through `ComponentPlatformOverridePayloadService`; compare controller references directly with `SceneEntityAsset.Id` values for the named text entities. Do not invoke the full rendering generator from a unit test because it intentionally rewrites the rendering asset catalog; the source registration contract plus the artifact produced by the real editor command remains the generation boundary.

**Files:**

- Modify: `assets/codebase/rendering.tools.tests/RenderingSceneGeneratorSoftwarePathTracerTests.cs`

## Task 1: Validate serialized viewport metadata

- [ ] Locate `SoftwarePathTracerPresentationViewport` and deserialize its base `ViewportComponent` as `320x240` with a `320x240` reference canvas.
- [ ] Deserialize its DS override and require `256x192` fixed/reference dimensions.
- [ ] Deserialize its 3DS override and require a `400x240` top-screen canvas; retain the existing output-sprite `320x240` and X=`40` assertions.
- [ ] Locate `SoftwarePathTracerBottomScreenViewport`; require the base DS `256x192` fixed/reference dimensions and the 3DS override's `320x240` fixed size with a `256x192` reference canvas.

## Task 2: Validate controller reference targets

- [ ] Match the common controller SPP, elapsed, and rays-per-second IDs exactly to `SoftwarePathTracerSppText`, `SoftwarePathTracerElapsedText`, and `SoftwarePathTracerRaysPerSecondText` serialized entity IDs.
- [ ] For both DS and 3DS override payloads, match the three controller IDs exactly to the corresponding `SoftwarePathTracerHandheld*Text` serialized entity IDs.
- [ ] Require each named target entity to contain one serialized `TextComponent` record.

## Task 3: Preserve the correct runtime-model boundary

- [ ] Keep the existing no-`MeshComponent` assertion and exact engine-cube `SceneAssetReference` equality checks. `RuntimeModel` is not a serializable component; those two assertions are the meaningful proof that the scene contains raw CPU-readable cube references rather than embedded GPU runtime models.
- [ ] Do not add string scans or a fictitious `RuntimeModel` component-type assertion.

## Task 4: Verify and commit

- [ ] Run the focused `RenderingSceneGeneratorSoftwarePathTracerTests` filter and require all tests to pass.
- [ ] Run `rtk git diff --check`.
- [ ] Do not modify, restore, delete, or stage the unrelated importer churn already present in the worktree.
- [ ] Stage only this test file and commit as `Strengthen software tracer scene validation`.
