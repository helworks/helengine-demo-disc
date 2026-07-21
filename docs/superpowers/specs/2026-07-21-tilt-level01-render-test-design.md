# Tilt Level 1 Render Test Design

## Goal

Create a separate PS2-focused scene that reproduces the visible Level 1 Tilt Trial course geometry, coins, and goal flag for renderer validation.

## Scope

The scene contains the Level 1 course cubes, walls, guards, goal pad, three collectible coin visuals, and the goal flag visual. It excludes the player sphere, gameplay controller, rigid bodies, colliders, trigger observers, coin logic, goal logic, follow camera, menu systems, and gameplay HUD.

The scene uses a fixed inspection camera and the existing FPS component so the PS2 build exposes rendering performance while the geometry is viewed. Existing authored course, coin, and flag material/blueprint assets are reused rather than duplicated.

## Implementation

The physics scene authoring pipeline will add a dedicated render-test scene id and factory path. Its generated scene will use the same Level 1 visible transforms and material references as `tilt_trial_level_01.helen`, but will serialize render-only mesh/visual entities. The scene catalog and PS2 build scene list will include the new scene, with its startup order controlled only for the validation export.

Source tests will verify the new scene id, required visible asset references, FPS component, and absence of gameplay-only components. The editor CLI will package the PS2 ISO, and the resulting ISO will be launched through the existing PCSX2 script.

## Acceptance Criteria

1. The new scene is separate from `tilt_trial_level_01.helen`.
2. It contains the Level 1 cubes, three coins, goal pad, and goal flag with their existing visual materials/assets.
3. It contains an FPS component.
4. It contains no player, physics, gameplay, trigger, or menu components.
5. The PS2 editor CLI build packages and verifies successfully.
6. PCSX2 boots the new render-test scene directly.
