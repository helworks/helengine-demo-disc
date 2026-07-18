# Physics DS Scene Generation Design

## Summary

`city` already exposes three physics showcase scenes through the demo-disc menu:

- `test_scene_dynamic_stack_boxes`
- `test_scene_dynamic_sphere_stack`
- `test_scene_dynamic_mixed_stack`

Those scenes currently exist only as desktop-authored `.helen` assets under `assets/scenes/physics/`. The DS flow already has generator support for rendering scenes and the main menu, but physics scenes are missing from that DS companion path. The result is that the DS menu can point at physics scenes conceptually, while the project does not generate or include the DS-scaffolded scene assets needed to load them consistently on DS.

This design extends the existing city-owned generator pipeline so the three physics scenes produce DS companion scenes using the same generic scaffold already used for generated rendering DS scenes.

## Problem

The current setup has three gaps:

1. Physics scene ids already exist in the demo-disc menu catalog, but no generated `_ds` companion scenes exist for them.
2. The DS build policy is based on DS-only scene ids, so physics scenes are currently incomplete from the DS content set.
3. The project already has one generic DS scaffold path for generated scenes, but physics scenes are not flowing through it.

The issue is not missing menu wiring. The issue is missing generated DS scene assets for the physics catalog entries.

## Goals

- Generate DS companion scenes for every curated physics demo scene in `DemoDiscSceneCatalog`.
- Reuse the existing generic Nintendo DS scaffold path rather than inventing a second DS scene layout system.
- Keep desktop physics scenes as the authored source of truth.
- Include the generated physics DS scene ids in the DS build and DS startup/menu mapping.
- Leave non-DS platforms unchanged.

## Non-Goals

- This change does not redesign the physics scenes themselves.
- This change does not create a physics-specific DS UI layout.
- This change does not hand-author binary `.helen` DS scene copies.
- This change does not expand the curated physics scene list beyond the three existing menu entries.

## Recommended Approach

Extend the current generated-scene pipeline so authored physics scenes can emit DS companion scenes through the same generic scaffold service already used for generated rendering scenes.

The desktop physics `.helen` assets remain authored and unchanged as the source scenes. A city-owned generation command should enumerate the curated physics scene ids from `DemoDiscSceneCatalog`, load each authored scene, and write one DS companion scene beside it with the `_ds` suffix naming convention expected by the DS menu/build flow.

## Scene Mapping

The generated physics DS scene ids should be:

- `test_scene_dynamic_stack_boxes_ds`
- `test_scene_dynamic_sphere_stack_ds`
- `test_scene_dynamic_mixed_stack_ds`

Their persisted scene asset paths should follow the same project-relative scene-id convention already used elsewhere in `city`, under the physics scene folder.

The DS startup/menu flow should continue to use logical scene ids in desktop code and resolve them to DS ids through the generated boot scene scene-map behavior already in place for DS.

## Generator Ownership

Generator ownership should stay in `city`, not in `helengine` or `helengine-ds`.

Recommended responsibilities:

- `DemoDiscSceneCatalog`
  - remains the curated source for which physics scene ids belong in the demo-disc experience
- new or extended city generator command/service
  - enumerates the curated physics scene ids
  - loads the authored scene assets
  - emits DS companion scenes
- `GeneratedAuthoringSceneWriteService`
  - remains the shared DS scaffold writer used to persist the companion roots

This keeps project policy in the project, while the generic DS scaffolding remains reusable.

## DS Scaffold Behavior

Physics DS companions should use the same generic DS scaffold as rendering DS scenes:

- top screen: staged source scene content
- bottom screen: existing default DS bottom overlay/return scaffold

No physics-specific DS layout rules are needed in this slice.

## Build Integration

The DS build should include the generated physics DS scene ids in the same way it already includes generated rendering DS scene ids.

That means:

- the generated DS physics scene assets must exist before the DS build runs
- the DS-only scene selection must include the new physics DS scene ids
- non-DS scene ids for those curated physics entries should not be included in the DS packaged playable scene set

## File Areas

Expected implementation areas:

- `C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscSceneCatalog.cs`
- `C:\dev\helprojs\demodisc\assets\codebase\scene.tools\...` or `physics.tools\...`
- `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\GeneratedAuthoringSceneWriteService.cs`
- `C:\dev\helprojs\demodisc\user_settings\build_config.json`
- generated outputs under `C:\dev\helprojs\demodisc\assets\scenes\physics\`

The exact generator class placement should follow the existing city generator layout and avoid duplicating DS scaffold logic.

## Verification

Implementation should prove all of the following:

- the curated physics menu ids now have `_ds` companion scene assets
- the DS scene-selection/build configuration includes those `_ds` ids
- the generated boot scene scene-map resolves the physics logical ids to the DS ids
- a DS build packages the physics DS scenes and excludes the non-DS physics playable scenes

## Recommendation

Proceed with a small city-owned physics DS companion generator that reuses `GeneratedAuthoringSceneWriteService` and the existing generic Nintendo DS scaffold path. This is the lowest-risk change because it keeps all project scene policy in `city` and aligns physics scenes with the existing rendering/menu DS generation model.
