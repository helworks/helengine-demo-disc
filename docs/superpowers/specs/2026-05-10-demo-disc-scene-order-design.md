# Demo Disc Scene Order

## Goal
Reorder the demo-disc scene selector so the listed scenes appear in this order:
1. Cube Test
2. Colored Cube Grid
3. Textured Cube Grid

The labels and scene ids stay exactly the same. Only the presentation order changes.

## Scope
- Update the authored scene item list in `DemoDiscSceneCatalog`.
- Preserve the existing menu structure.
- Keep the `Back` item in the same trailing position.

## Behavior
- The `Select Scene` panel should show the three playable scenes in the new order.
- Scene labels remain unchanged.
- Scene ids remain unchanged.

## Verification
- Add or update a test that asserts the catalog returns the new order.
- Confirm the generated main menu scene still loads the same three scene ids, just reordered.
