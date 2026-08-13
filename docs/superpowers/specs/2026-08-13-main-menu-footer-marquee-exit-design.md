# Main Menu Footer Marquee Exit Design

## Problem

The main-menu footer marquee restarts while part of its runtime text is still
visible. The component currently treats the serialized `TextWidth` value of
`420` as the complete line width, but it replaces the authored copy with a
longer platform-specific string at runtime.

## Approved behavior

The marquee must restart only after the rendered footer line has completely
left the left edge of the menu strip. Its existing start position and scroll
speed remain unchanged.

## Design

- Remove the serialized `TextWidth` configuration from the marquee and its
  standard-menu factory call site; it does not describe the runtime text.
- After resolving the footer text entity and assigning the platform/version
  copy, measure `FooterTextComponent.Text` with
  `FooterTextComponent.Font.MeasureTight(...)`.
- Multiply the tight metric width by `FooterTextComponent.FontScale` and cache
  that authored-canvas width in the component.
- Do not advance the marquee until its font is available and the runtime width
  has been measured. This preserves a valid first frame if the font resolves
  after the scene entity.
- Continue applying the existing reference-canvas horizontal scale to both the
  cached width and the movement speed. Restart once the scaled measured width
  is entirely left of zero.

## Verification

- Update the focused source-contract test to require runtime tight measurement,
  font scaling, and the measured exit threshold, and to forbid the obsolete
  `TextWidth` configuration.
- Run that focused test before and after the implementation.
- Rebuild and inspect the main menu on the affected desktop/PSP path before
  claiming the visual result is complete.
