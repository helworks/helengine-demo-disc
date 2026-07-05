# DS Bottom Screen Lilac Clear Design

## Goal

Use the exact demo-disc main-menu lilac background color on DS-authored bottom screens, including 3DS playback, so the bottom-screen presentation matches the menu styling instead of the current alternate clear color.

## Scope

This change applies only to the shared DS bottom-screen scaffold camera path used by DS-authored scenes. It does not change top-screen clear colors, scene-specific 3D camera clear settings, or menu theme behavior outside the shared DS bottom-screen scaffold.

## Design

The shared DS bottom-screen camera created by `NintendoDsRenderingSceneScaffoldFactory` will clear with the exact demo-disc main-menu background color `byte4(30, 17, 41, 255)`.

The color will be expressed directly in the scaffold factory as one stable scaffold-owned constant. The scaffold already owns the DS bottom-screen control strip and camera creation path, so this is the correct seam for a global bottom-screen presentation color that should apply across rendering and physics DS scenes.

The scaffold camera will continue to use the existing `CameraClearSettings` flow. Only the clear-color payload changes.

## Testing

Add or update a source-audit test that proves the shared DS scaffold bottom-screen camera uses the exact RGBA lilac value. Validation after implementation should be limited to the narrow source-audit test plus one 3DS rebuild and relaunch.

## Risks

If any DS-authored scene expected a different bottom-screen clear color for readability, this change will normalize it to the menu lilac. That is intentional for this change.
