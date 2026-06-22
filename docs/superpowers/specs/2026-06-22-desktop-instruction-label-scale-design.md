# Desktop Instruction Label Scale Design

## Goal

Reduce the size of the desktop and console instruction overlay labels (`Rotate Camera` and `Toggle Light`) by about 40% without changing the instruction panel itself and without affecting the Nintendo DS instruction overlay.

## Scope

This change applies only to the desktop/shared overlay authored by `DemoSceneInstructionOverlayFactory.CreateDesktopInstructionOverlayRoot`.

In scope:

- desktop/shared label font scale
- desktop/shared label position constants
- desktop/shared label bounds constants
- regression coverage for the authored desktop overlay source

Out of scope:

- Nintendo DS instruction overlay constants
- instruction panel size, position, or styling
- icon sizes or icon positions

## Approach

Use the existing desktop overlay constant seam in `assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs`.

Adjust only the desktop label constants so the labels:

- render at roughly 60% of the current size
- sit closer to their icon rows
- remain fully readable inside the existing panel

This keeps the change local, preserves the current overlay structure, and avoids touching DS-specific layout values.

## Implementation Notes

Update the desktop-only constants:

- `DesktopInstructionLabelFontScale`
- `DesktopInstructionTextLeft`
- `DesktopInstructionTextTopAdjustment`
- `DesktopInstructionTextWidth`
- `DesktopInstructionTextHeight`

Do not change:

- `NintendoDsInstructionFontScale`
- Nintendo DS text offsets or sizes
- panel dimensions
- icon dimensions

## Testing

Add one source regression test in `helengine.editor.tests` that asserts the authored desktop overlay source keeps the reduced desktop label scale and updated desktop text positioning while leaving Nintendo DS constants untouched.

Verification:

- run the narrow source test first and watch it fail
- make the constant-only change
- rerun the narrow source test and confirm it passes
- regenerate the affected city rendering scenes
- rebuild Windows output
- launch and verify the overlay uses the smaller labels

## Risks

The main risk is choosing text bounds that clip the labels on some shared platforms. Keeping the panel and icons unchanged limits the blast radius, and the constants can be retuned without structural changes if needed.
