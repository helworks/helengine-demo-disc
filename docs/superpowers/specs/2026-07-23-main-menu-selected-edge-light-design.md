# Main Menu Selected Edge-Light Design

## Scope

Improve the standard Demo Disc main-menu selection state with one small visual change. Nintendo DS and Nintendo 3DS menu scenes are excluded.

## Behavior

The currently selected menu button will use the existing teal `AccentSecondaryColor` for its border. Its purple surface, text color, text outline, layout, and input behavior remain unchanged. Every unselected button continues to use the existing purple border color.

## Implementation Boundary

The selection controller will own the runtime border-color switch. The menu theme remains the source of both the existing purple border and teal accent colors. No new scene entities, animations, shaders, or assets will be added.

## Validation

Add or update a focused source or behavior test proving the selected state assigns the teal border and deselection restores the themed purple border. Regenerate the standard menu scene, build Windows, and verify the changed executable starts without an immediate host failure.
