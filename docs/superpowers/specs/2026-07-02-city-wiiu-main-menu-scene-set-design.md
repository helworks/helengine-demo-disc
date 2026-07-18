# City Wii U Main Menu Scene Set Design

## Goal

Change the default `wiiu` platform build configuration for the `city` project so Wii U builds package the demo-disc main menu and every scene currently reachable from that menu.

## Current State

The `wiiu` entry in `user_settings/build_config.json` currently packages only `cube_test`.

The playable scene menu is owned by `assets/codebase/menu/DemoDiscSceneCatalog.cs`. That catalog currently exposes:

- Rendering scenes: `cube_test`, `colored_cube_grid`, `textured_cube_grid`, `axis_test`, `axis_test2`, `test_scene_matrix_render`, `directional_shadow_plaza`
- Physics scenes: `test_scene_dynamic_stack_boxes`, `test_scene_dynamic_sphere_stack`, `test_scene_dynamic_mixed_stack`, `test_scene_static_mesh_showcase`, `test_scene_static_mesh_minimal`
- Game scenes: `tilt_trial`

The main menu runtime scene id is `DemoDiscMainMenu`.

## Design

Keep this as a configuration-only change in `user_settings/build_config.json`.

Do not change:

- `DemoDiscSceneCatalog`
- generated scene assets
- builder code
- Wii U runtime code

Update only the `wiiu` platform entry so `selectedSceneIds` and `sceneOrders` include the menu scene plus every currently reachable playable scene in a deterministic order.

The new default Wii U scene set will be:

1. `DemoDiscMainMenu`
2. `cube_test`
3. `colored_cube_grid`
4. `textured_cube_grid`
5. `axis_test`
6. `axis_test2`
7. `test_scene_matrix_render`
8. `directional_shadow_plaza`
9. `test_scene_dynamic_stack_boxes`
10. `test_scene_dynamic_sphere_stack`
11. `test_scene_dynamic_mixed_stack`
12. `test_scene_static_mesh_showcase`
13. `test_scene_static_mesh_minimal`
14. `tilt_trial`

`sceneOrders` will mirror the same order numbers.

## Rationale

This keeps Wii U aligned with the current city menu contract without introducing new build-pipeline behavior.

It is the smallest safe change because:

- other platforms already use explicit curated scene lists in `build_config.json`
- the Wii U builder already consumes the platform-selected scene list
- no code change is required to restore the intended content set

This design intentionally duplicates the menu scene list instead of deriving it automatically. Automatic derivation would be a separate build-system feature and is outside the scope of this task.

## Verification

Use the smallest validation that proves the configuration is correct:

1. Inspect the edited `wiiu` entry in `user_settings/build_config.json`.
2. Run the shared wrapper build for `-Platform wiiu` against `C:\dev\helprojs\demodisc\project.heproj`.
3. Inspect the staged Wii U output and confirm it contains cooked scene artifacts for `DemoDiscMainMenu` and the referenced playable scenes.

## Out Of Scope

- automatic scene-set derivation from `DemoDiscSceneCatalog`
- menu reordering
- adding or removing playable scenes
- runtime startup behavior changes beyond what the existing config already controls
