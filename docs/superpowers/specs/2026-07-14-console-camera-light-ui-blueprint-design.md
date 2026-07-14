# Console Camera and Light Instruction Blueprint Design

## Goal

Create one reusable console-platform Blueprint containing only the shared camera/light instruction panel: the platform-native camera D-pad icon, the platform-native light-toggle icon, the `Camera` and `Light` labels, and the shared background panel. Attach it to the standard console rendering and physics scenes so those scenes no longer author duplicate instruction-overlay trees.

## Scope

The Blueprint targets the console platform path for `ps2`, `gamecube`, `wii`, `switch`, and `wiiu`.

- `ps2`, `gamecube`, `wii`, and `switch` use their existing generated control-icon families.
- `wiiu` explicitly falls back to the Wii icon family because no Wii U generated icon family currently exists.
- Windows, PSP, PS Vita, DS, and 3DS remain outside this Blueprint.
- Tilt Trial presentation Blueprints, the demo-disc menu, FPS text, return-to-menu behavior, and the light-state swatch remain separate systems.

## Existing Behavior

`DemoSceneInstructionOverlayFactory.CreateDesktopInstructionOverlayRoot` currently creates a screen-bound 1280x720 viewport, a dark rounded background panel at the lower-left of the reference canvas, a camera row, and a light row. The camera row currently supports two platform-specific icon slots; the new Blueprint preserves the existing console-native icon bindings and layout. The light row preserves each platform’s native binding, including PS2 `R1`.

Rendering scene factories and the playable physics-scene factory call this authoring path independently. The resulting duplicated entity trees are serialized into each scene. The existing `DemoDiscLightToggleComponent` and its indicator-swatch overlay are not part of this Blueprint and continue to own light-cycle behavior separately.

## Design

The existing factory supports multiple camera-icon slots, but this Blueprint uses only the requested platform-native D-pad camera icon. The other slot is not copied into the Blueprint.

### Blueprint asset

Create a generated asset at:

`assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint`

The Blueprint root owns:

1. A screen-bound viewport using the existing 1280x720 reference canvas.
2. The existing lower-left rounded background panel and its current dimensions/colors/render order.
3. The camera instruction row with one native D-pad icon override for each target platform.
4. The light instruction row with native icon overrides for each target platform.

The Blueprint must not contain FPS, Back, light-state swatch, scene-specific instructions, or platform-specific gameplay behavior.

### Platform icon overrides

Each icon component keeps one shared base record and platform-specific sprite overrides. The override table uses these families:

| Target platform | Camera icon family | Light icon family |
| --- | --- | --- |
| `ps2` | PS2 D-pad | PS2 R1 |
| `gamecube` | GameCube D-pad | GameCube R |
| `wii` | Wii D-pad | Wii B |
| `switch` | Switch D-pad | Switch R |
| `wiiu` | Wii D-pad | Wii B |

The Wii U fallback is represented as an explicit `wiiu` platform override pointing at the Wii icon assets, so platform resolution remains deterministic and does not depend on an implicit family lookup.

### Scene attachment

Replace the repeated desktop instruction-root serialization in the standard rendering and playable physics scene generators with one Blueprint instance root referencing `blueprints/ui/ConsoleCameraLightInstructions.hblueprint`. Keep the existing platform pruning behavior so the instance is present only for the console targets listed above. Preserve each scene’s camera, gameplay UI, light-toggle component, music, and scene-specific entities.

The Blueprint instance must expand during packaging into ordinary scene entities and asset references. No runtime Blueprint asset lookup or nested Blueprint instance is required.

### Generation and ownership

Reuse the existing generated-scene persistence and Blueprint save services. Add one focused generator/catalog entry for the Blueprint path and one attachment helper so scene factories do not each duplicate the icon/layout construction. The existing instruction-overlay factory may remain as the source of shared layout constants while generation is migrated; after migration, no targeted console scene should serialize a second copy of the same instruction tree.

## Error handling

- Blueprint generation fails if a required icon asset or font reference cannot be resolved.
- Blueprint validation fails if the root contains a nested Blueprint instance.
- Scene attachment fails if a target scene cannot accept a Blueprint instance or if the expected asset path is missing.
- Wii U generation must fail only if the Wii fallback assets are unavailable; it must not silently fall back to desktop or handheld icons.

## Testing

Add focused source and asset-generation tests that verify:

1. The generator writes the expected `.hblueprint` file.
2. The serialized Blueprint contains the viewport, background, camera row, light row, and no FPS/Back/light-swatch components.
3. The serialized icon records include the PS2, GameCube, Wii, Switch, and explicit Wii U override paths.
4. The rendering and physics scene attachment paths reference the Blueprint instance instead of calling the old per-scene instruction-root authoring path.
5. The existing scene-generation and Blueprint packaging tests continue to pass.

Verification will run the focused generated-code tests first, then the full project solution tests, then a fresh console-capable export to confirm the Blueprint expands into cooked scene content for the target platforms.

## Alternatives considered

### Keep duplicating the generated instruction tree

This requires every scene factory to carry the same layout and platform-icon logic, so future UI changes can drift between scenes. It does not meet the reuse goal.

### Hand-author separate Blueprints per console

This would make platform-specific art easy to inspect but would duplicate the layout and background across five assets. One Blueprint with platform overrides keeps the shared structure centralized and matches the current asset pipeline.

### Put the light behavior inside the Blueprint

This would couple a visual instruction panel to scene-wide directional-light discovery and input behavior. Keeping `DemoDiscLightToggleComponent` outside preserves the current runtime ownership and keeps the Blueprint limited to the requested UI.

## Acceptance criteria

- A single `ConsoleCameraLightInstructions.hblueprint` is the source of the console camera/light instruction panel.
- PS2, GameCube, Wii, Switch, and Wii U console exports show the panel with native icons, with Wii art used only as the explicit Wii U fallback.
- The panel contains only the requested camera/light rows and background.
- Targeted rendering and physics scenes no longer embed independent copies of the same instruction hierarchy.
- Existing light-toggle behavior and all unrelated platform presentation paths remain unchanged.
