# PSP Viewport Backgrounds Design

## Goal

Remove the one-pixel top and bottom letterbox strips from the Helen of Code splash and loading overlay on the PSP while retaining aspect-correct 1280 by 720 content layout.

## Cause

The reference canvas is 16:9.  PSP output is 480 by 272, so aspect-preserving fit produces a centered 480 by 270 content canvas.  The existing black rectangles are children of that fitted canvas and therefore leave one physical row uncovered at both edges.

## Design

Create each blackout rectangle as a direct child of its overlay camera rather than a child of the fitted reference-canvas root.  Each runtime component updates its rectangle size from `RenderManager3D.MainWindowSize`, so the rectangle is not affected by the 16:9 fit and covers the exact live viewport on every platform.  The splash logo and loading progress UI remain children of the fitted root and retain their current layout.

Both runtime components keep their stable scene references and continue to control only alpha or progress.  No PSP renderer behavior, reference-canvas fitting, or authored content scaling changes.

## Verification

Source-contract tests will assert that both factories create the blackout rectangle under the camera before the fitted root is created, and that both runtime components assign `RenderManager3D.MainWindowSize` to their resolved background rectangle.  A fresh PSP package will then be launched in PPSSPP for visual verification.
