# Main Menu Footer Identity Design

## Scope

Add one static identity line to the standard Demo Disc main-menu scene. Nintendo DS and Nintendo 3DS menus are excluded.

## Presentation

The footer reads `MADE BY HELENA / HELEN OF CODE` inside a narrow full-width strip anchored eight pixels above the bottom of the reference canvas. The strip is approximately 36 pixels high: only large enough to contain the text. It uses a translucent dark-purple surface with one-pixel muted-lilac top and bottom borders, no side borders, glow, or animation.

The text begins two pixels higher than its current strip position and is clipped to the strip bounds. It moves steadily left at 35 reference-canvas pixels per second. It begins just outside the right edge, leaves fully through the left edge, and immediately restarts just outside the right edge. It uses no easing, bounce, or fade.

The existing platform/version overlay remains at the top-right and the rotating engine logo remains at the bottom-right. No animation, input behavior, asset, or runtime component is added.

## Validation

Add a focused source contract for the footer text and its standard-menu factory placement. Regenerate the standard menu, build Windows, and confirm the package starts without an immediate host failure.
