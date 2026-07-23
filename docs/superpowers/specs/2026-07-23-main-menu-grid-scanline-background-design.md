# Main Menu Grid and Scanline Background Design

## Goal

Give the standard Demo Disc main menu a restrained animated background that reinforces the purple-black Helen of Code identity without competing with menu content.

## Visual Design

The generated menu will add two background layers behind the existing logo, menu panels, footer, and platform information.

- A faint purple orthographic grid spans the reference canvas and drifts diagonally at a slow constant rate.
- Fine horizontal scanlines span the same canvas and shimmer vertically at a much smaller rate.
- The existing purple-black camera clear color remains the backdrop beneath both layers.
- Grid and scanline colors use low-alpha variants of the existing accent and muted theme colors.

## Architecture

The standard menu scene factory will author dedicated grid and scanline entities at render orders below existing menu visuals. A single update component on their common background root will hold serialized entity references and update the two local positions. It will use the existing reference-canvas fit hierarchy, so the effect follows the menu viewport on every supported standard platform.

No image asset, shader, clip rectangle, or handheld-menu change is included. The effect remains component-based and uses existing 2D primitives for portability.

## Runtime Behavior

The grid wraps continuously as it drifts, avoiding visible pauses or direction reversals. Scanlines likewise wrap while moving more slowly. If a serialized entity reference cannot be resolved after scene loading, the component fails clearly rather than silently producing a partial background.

## Validation

A focused source-contract test will verify authored background layers, serialized references, render ordering, and motion constants. The standard main menu scene will be regenerated and a Windows build will verify packaging.
