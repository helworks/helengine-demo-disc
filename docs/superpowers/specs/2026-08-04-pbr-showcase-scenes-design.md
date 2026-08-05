# PBR Showcase Scenes

## Goal

Add three new rendering showcase scenes to the end of the curated rendering scene list, demonstrating the engine's PBR (metallic/roughness/specular) material support and multi-light cast shadows, using both procedural materials and downloaded free-license textures.

## Scope

Three new scenes, registered after `spotlight_street_slice` (the current last entry in `RenderingSceneGenerator.Generate()`), authored for every currently-supported platform (windows, ps2, psp, gamecube, ds) following the same per-platform material convention used by every existing rendering showcase scene and by `TiltTrialPlayerSphereMarbleMaterialFactory`.

| # | Scene id | Label | Purpose |
|---|---|---|---|
| 13 | `scenes/rendering/pbr_material_gallery.helen` | `13. PBR Gallery` | 5x5 grid of spheres sweeping metallic (rows, 0 to 1) x roughness (columns, 0 to 1). Solid base colors, no textures. Pure material-parameter reference chart. |
| 14 | `scenes/rendering/pbr_textured_showcase.helen` | `14. PBR Textures` | A few hero props wearing downloaded CC0 albedo+roughness texture maps: one scuffed-metal material (metallic) and one wood-plank material (dielectric). |
| 15 | `scenes/rendering/pbr_shadow_theater.helen` | `15. PBR Shadow Theater` | A directional sun and a spotlight, both shadow-casting, lighting a small cluster of metallic/rough props on a pedestal. Demonstrates specular response and shadow casting from two independent lights simultaneously. |

Out of scope: normal maps and metallic/AO texture maps (the engine's `standard-shader` material schema does not expose fields for either today - metallic is scalar-only, and only diffuse/roughness accept texture bindings). Point-light shadows are also out of scope unless a `PointLightComponent` is confirmed to exist in the `helengine` engine assembly during implementation; `PbrShadowTheaterSceneFactory` falls back to a second `SpotLightComponent` if not.

## Platform Behavior

- **windows / psp** (`standard-shader` -> `ForwardStandardShader`): real metallic/roughness/specular Cook-Torrance shading, texture-bound albedo and roughness maps where applicable.
- **ps2** (`ps2-simple-lit-textured`), **gamecube** (`gamecube-standard-textured`), **ds** (`ds-standard-textured`): fixed-function fallback. Metallic gallery spheres get a mid-gray `base-color` proportional to their intended metallic value (darker = less metallic reflectivity implied) since these schemas have no metallic/roughness fields. Textured props use `texture-id`/`texture-relative-path` for the albedo map only (no roughness map support on these schemas).

## Approved Design

### Scene 13 - PBR Material Gallery

New `PbrMaterialGalleryMaterialFactory` writes 25 `.hasset` materials (`materials/rendering/pbr_gallery/M{metallicIndex}R{roughnessIndex}.hasset`) in a nested loop over 5 metallic steps (0, 0.25, 0.5, 0.75, 1.0) x 5 roughness steps (0, 0.25, 0.5, 0.75, 1.0), mirroring `ColoredCubeGridFactory.WriteMaterialAssets`/`CreateRuntimeMaterials`. All 25 share one flat mid-gray base color so metallic/roughness differences are the only visual variable. `specular` stays at the existing default (0.5) for every material.

New `PbrMaterialGallerySceneFactory.CreateSceneDefinition(planeModel, sphereModel, galleryMaterials[25])` places 25 spheres in a 5x5 grid on a ground plane, reusing the sun+fill+ambient three-light rig pattern from `GameSceneFactory.CreateDirectionalLightEntity`/`CreateDirectionalFillLightEntity`/`CreateAmbientLightEntity` (Tilt Trial Level 1 render-test scene) so all 25 materials are lit identically and shadows fall across the grid. Camera is a static front-on framing shot (no orbit needed - the grid itself is the subject).

### Scene 14 - PBR Textured Showcase

New `PbrTexturedShowcaseMaterialFactory` writes 2 materials:
- `materials/rendering/pbr_textured_showcase/ScuffedMetal.hasset`: `metallic=1.0`, `roughness-texture-id` bound to the downloaded roughness map, `texture-id` bound to the downloaded albedo map.
- `materials/rendering/pbr_textured_showcase/WoodPlanks.hasset`: `metallic=0.0`, same texture-binding pattern.

Both follow `TiltTrialPlayerSphereMarbleMaterialFactory`'s exact structure (`ConfigureWindowsPlatform`/`ConfigurePs2Platform`/`ConfigureGameCubePlatform`/`ConfigureDsPlatform`, `AssetImportManager` texture resolution).

New `PbrTexturedShowcaseSceneFactory.CreateSceneDefinition(cubeModel, planeModel, metalMaterial, woodMaterial)` places 2-3 simple cube/slab props (one per material) on a ground plane under a single shadow-casting directional light, framed by a static camera.

### Scene 15 - PBR Shadow Theater

New `PbrShadowTheaterSceneFactory.CreateSceneDefinition(sphereModel, planeModel, galleryMaterials)` reuses a handful of the gallery's metallic materials (e.g. full-metallic-low-roughness, full-metallic-high-roughness, non-metallic-low-roughness) on a small pedestal cluster of 3-4 spheres. Two lights: a `DirectionalLightComponent` sun (`ShadowsEnabled=true`) and a `SpotLightComponent` (`ShadowsEnabled=true`) angled from a different direction, both hitting the cluster, following `DirectionalShadowPlazaSceneFactory`/`SpotlightStreetSliceSceneFactory`'s light-setup patterns. Reuses `DemoDiscOrbitCameraComponent` (as in Shadow Plaza) so the viewer sees specular highlights move across the cluster.

## Texture Sourcing

Two CC0 (public domain, no attribution required) texture sets from ambientCG.com, verified to exist:

- **Metal032** - scuffed/worn metal - `https://ambientcg.com/get?file=Metal032_1K-JPG.zip`
- **WoodFloor041** - parquet wood planks - `https://ambientcg.com/get?file=WoodFloor041_1K-JPG.zip`

Each zip is downloaded, and only the Color and Roughness JPGs are extracted (the engine's `standard-shader` schema has no use for Normal/AO/Displacement/Metalness maps today). Files are placed at:

```
assets/textures/rendering/pbr_textured_showcase/Metal032Albedo.jpg
assets/textures/rendering/pbr_textured_showcase/Metal032Roughness.jpg
assets/textures/rendering/pbr_textured_showcase/WoodFloor041Albedo.jpg
assets/textures/rendering/pbr_textured_showcase/WoodFloor041Roughness.jpg
```

matching the `PlayerSphereMarble.jpg`/`PlayerSphereMarbleRoughness.jpg` naming convention.

## Wiring / Registration

1. `RenderingSceneGenerationAssets.cs`: add `PbrGalleryMaterials` (`RuntimeMaterial[25]`), `PbrTexturedShowcaseMetalMaterial`, `PbrTexturedShowcaseWoodMaterial` properties.
2. `RenderingSceneAssetPreparationService.cs`: add `WriteMaterialAsset(s)` calls for the three new material factories, and `LoadRuntimeMaterial`/loop calls to populate the new asset bundle properties.
3. `RenderingSceneGenerator.cs`: add 3 scene-id consts (+ any DS companion ids only if a handheld variant is authored - not required per this design since these are windows/psp-focused PBR showcases), 3 factory fields + constructor instantiation, 3 `CreateSceneDefinition(...)` calls, and 3 `AuthoringSceneWriteService.WriteScene(...)` calls appended after the existing `spotlightStreetSliceSceneDefinition` line.
4. `DemoDiscSceneLabelOverlaySourceTests.Curated_rendering_factories_contain_the_approved_labels`: add the 3 new `(FileName, Label)` entries.
5. `user_settings/build_config.json`: add the 3 new scene ids to the `windows` platform's `selectedSceneIds`/`sceneOrders` arrays, continuing the existing order numbering.

## Error Handling

Follows existing conventions throughout: constructor/method argument-null and blank-string guards matching every other factory in `rendering.tools`, and `ResolveTextureAssetId`-style failure if a downloaded texture's import settings do not yield a persisted asset id.

## Testing

New `PbrShowcaseSceneSourceTests.cs` in `rendering.tools.tests/`, following the repo's `*SourceTests.cs` source-grep convention (no engine boot required):

- Each of the 3 new factory files exists and contains its expected scene id constant.
- Each factory file references `DemoDiscSceneLabelOverlayFactory` with its approved numbered label.
- `PbrMaterialGalleryMaterialFactory`'s source contains the full metallic x roughness sweep (all 25 `SetFieldValue(MetallicFieldId, ...)`/`SetFieldValue(RoughnessFieldId, ...)` combinations, or the loop bounds that produce them).
- `PbrTexturedShowcaseMaterialFactory`'s source references both downloaded texture relative paths.

Extend `DemoDiscSceneLabelOverlaySourceTests.Curated_rendering_factories_contain_the_approved_labels` with the 3 new entries.

Re-run `dotnet test` on `rendering.tools.tests` and `game.tools.tests` (the two projects touched by the prior background-stretch fix in this session) to confirm no regressions, matching the stash/pop baseline-comparison approach already used in this session.

## Alternatives Considered

- **One combined scene** covering metallic/roughness/textures/shadows in a single scene was rejected (per user direction) in favor of three focused scenes, matching the existing pattern of one scene per rendering technique (Shadow Plaza for shadows, Spotlight Street Slice for spotlights, Textured Cube Grid for texturing).
- **Windows/PSP-only authoring** (skipping PS2/GameCube/DS platform blocks) was rejected per user direction in favor of authoring every platform, consistent with how every other rendering showcase scene behaves - PBR-incapable platforms simply get a flat-shaded fallback rather than being absent from their scene catalog.
- **Full PBR texture sets (normal/AO/metalness maps)** were rejected as out of scope because the engine's `standard-shader` schema does not currently expose fields for them - only diffuse and roughness texture bindings exist.
