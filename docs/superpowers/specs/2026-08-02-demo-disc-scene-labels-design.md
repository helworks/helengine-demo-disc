# Demo Disc Scene Labels

## Goal

Add a small top-right scene identifier to every non-game playable scene surfaced by the demo disc. Each identifier uses the format `$. Scene Name`, where `$` is the global scene number and the description is no more than two words.

## Scope

The label applies to the 12 playable scenes in the Demo Scenes and Physics Scenes menu groups, in this order:

1. `1. Cube Test`
2. `2. Colored Cubes`
3. `3. Textured Cubes`
4. `4. Axis 1`
5. `5. Axis 2`
6. `6. Matrix Render`
7. `7. Shadow Plaza`
8. `8. Stacked Boxes`
9. `9. Sphere Stack`
10. `10. Mixed Stack`
11. `11. Static Mesh`
12. `12. Simple Mesh`

The following remain unchanged and unlabeled:

- GameSceneFactory and all game-owned screens.
- Nintendo DS and Nintendo 3DS generator files and their generated handheld presentation roots.
- Menu, splash, and loading scenes.
- Non-menu rendering or physics validation scenes that are not playable entries in the demo-disc catalog.

## Approved Design

Create one shared `DemoDiscSceneLabelOverlayFactory` in `rendering.tools`. It attaches a child viewport and right-aligned `TextComponent` to an existing scene UI root, using the project demo-disc body font and a fixed 1280x720 reference canvas. The label is positioned 24 pixels from the top and right edges, uses a small white font, and renders above the ordinary scene HUD.

Each targeted rendering or physics scene factory supplies its explicit final label string to the shared helper. The number is explicit metadata rather than inferred from the order in which generators happen to run. The rendering targets are Cube Test, Colored Cubes, Textured Cubes, Axis 1, Axis 2, and Shadow Plaza. The physics targets are Matrix Render, Stacked Boxes, Sphere Stack, Mixed Stack, Static Mesh, and Simple Mesh.

The label is authored under the common scene UI root. The existing `GeneratedAuthoringSceneWriteService` already marks common roots as unavailable to the Nintendo handheld platforms when a handheld augmentation is present, so DS/3DS generator code does not need to change and the label does not appear in those builds.

## Data Flow

1. A targeted scene factory creates its existing UI root.
2. The factory calls `DemoDiscSceneLabelOverlayFactory.AttachToSceneUi` with the approved label text.
3. The helper creates the fixed-size screen-bound viewport and right-aligned text entity, stores the body-font asset reference, and returns control to the scene factory.
4. The normal generated-scene writer serializes the common root for supported non-handheld platforms and applies its existing DS/3DS root exclusions.

## Error Handling

The helper rejects a missing UI root, missing font, or blank label with the same argument-validation style used by the existing overlay factories. Scene factories continue to fail during generation if their required editor font or UI root cannot be created.

## Testing

Add source-level regression coverage that verifies:

- The shared helper uses a screen-bound 1280x720 viewport, top/right margins, right text alignment, and the project body-font reference.
- Every one of the 12 approved scene labels is attached by the intended rendering or physics factory.
- GameSceneFactory and DS/3DS generator sources are not modified by this feature.
- The final label strings stay in the `number + period + one/two-word name` contract.

Run the narrow affected test projects first, then rebuild the rendering, physics, and menu tool projects. Regenerate the affected scenes through the existing editor prebuild/generator path and verify the generated common roots contain the labels while the handheld roots remain unchanged.

## Alternatives Considered

- Direct per-factory text construction was rejected because layout and font-reference behavior would be duplicated across 12 scene paths.
- A post-generation serialized-scene rewrite was rejected because it would need to walk and mutate generated scene assets and could accidentally label non-menu validation scenes.
