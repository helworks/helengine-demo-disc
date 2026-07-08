# Tilt Trial Level Select Design

**Date:** 2026-07-08

## Goal

Expand Tilt Trial from a single gameplay scene into a level-based flow with a dedicated level-select scene, reusable per-level configuration, and consistent finish/fail progression behavior.

## Scope

This design covers the first gameplay foundation pass for Tilt Trial:

- a dedicated Tilt Trial level-select scene shown before gameplay
- five immediately selectable Tilt Trial levels
- a reusable per-level component that stores timing and presentation metadata
- a shared ordered level catalog used by the selector and by in-game `Next` progression
- finish and fail overlays with deterministic actions

This design does not cover the authored geometry or gameplay layout of the five final levels. Those levels will be designed separately on top of this foundation.

## Requirements

### Level Select

Tilt Trial must have a dedicated level-select scene before gameplay. The selector must expose five levels immediately, with no unlock gating in the first version.

The selector UI should present:

- a vertical list of the five levels
- a large square preview area for the currently focused level
- the selected level display name
- the selected level start time
- the selected level medal thresholds

If a level has no preview image yet, the selector must render a styled placeholder square instead of failing or leaving the area empty.

### Per-Level Foundation

Every Tilt Trial gameplay level must carry a reusable configuration component named `TiltTrialLevelSettingsComponent`.

Each level must expose:

- stable level id
- display name
- gameplay scene id
- start time in seconds
- target medal times
- optional square preview image reference

The component is the canonical per-scene gameplay metadata container. Menu code and results code may read the same data shape through the shared catalog, but the scene itself must still carry the component so each level remains self-describing.

### Runtime Flow

The dedicated level-select scene becomes the front door for Tilt Trial. Selecting a level loads that gameplay scene directly.

Inside a Tilt Trial gameplay scene, a session controller reads the level settings and starts the countdown from that level's configured start time.

On finish:

- normal gameplay input stops
- a results overlay appears
- the result screen evaluates the completion against the configured medal thresholds
- the overlay exposes `Retry`, `Next`, and `Level Select`

On timeout:

- normal gameplay input stops
- a fail overlay appears
- the overlay exposes `Retry` and `Level Select`

`Next` must resolve through the ordered level catalog. If the current level is already the final entry, `Next` should resolve to `Level Select`.

### UI State Handling

Tilt Trial should use the new reusable finite state machine utility for session and overlay flow instead of scattered booleans.

The first pass should model explicit high-level states such as:

- level select
- active gameplay
- results
- fail

The exact enum names may vary, but the flow should be driven by a small explicit state model rather than implicit UI toggles.

## Architecture

### Ordered Level Catalog

Add a Tilt Trial level catalog in the city gameplay authoring/runtime layer. It should define the ordered five-level list and provide the canonical order used by:

- the level-select scene
- `Next` progression
- validation that all expected levels are present

The selector should not infer order by scanning scenes. It should use the explicit catalog so content remains deterministic.

### Per-Level Settings Component

Add a reusable Tilt Trial settings component in project gameplay code. This component lives on every authored Tilt Trial gameplay scene and stores the level's timer, naming, medal, and preview metadata.

The component should validate obvious invalid input early, such as:

- missing level id
- missing display name
- missing scene id
- non-positive start time
- missing or invalid medal definitions
- medal thresholds in the wrong order

### Level Select Scene

Generate a dedicated Tilt Trial level-select scene through the existing city scene-generation path rather than hand-authoring a one-off runtime-only menu.

The scene should contain:

- a root UI entity
- a list area for the five level entries
- a preview panel area
- detail text fields for the currently focused level
- a selector controller component that binds the visible UI to the ordered level catalog

This keeps the level selector aligned with the existing generated menu/game scene approach already used by the city project.

### Gameplay Session Controller

Add a Tilt Trial session controller for gameplay scenes. It is responsible for:

- reading the level settings component
- starting and resetting the countdown
- determining finish vs timeout outcome
- freezing gameplay during overlays
- exposing `Retry`, `Next`, and `Level Select` behavior

This controller should own the timer lifecycle so resetting a level or returning from fail state always restores the configured start value for that scene.

## Authoring Boundaries

Keep responsibilities narrow:

- the level catalog owns ordered cross-level progression data
- the per-level settings component owns per-scene metadata
- the level-select scene/controller owns level browsing and launch behavior
- the gameplay session controller owns timer and post-run overlays

The per-level settings component should not directly own selector layout or overlay presentation. It is data, not the UI controller.

## Testing Strategy

The first pass should focus on deterministic authoring and flow validation.

Required coverage:

- the Tilt Trial level catalog contains exactly five ordered entries
- each entry exposes scene id, display name, start time, medal thresholds, and preview metadata shape
- the generated level-select scene includes the expected selector structure and five selectable entries
- each authored Tilt Trial gameplay scene includes the per-level settings component
- invalid settings fail fast
- finish routes to the results state with `Retry`, `Next`, and `Level Select`
- timeout routes to the fail state with `Retry` and `Level Select`

This provides confidence in the new foundation before the five final authored course layouts exist.

## Implementation Notes

- Follow the existing city project pattern of scene generation through `game.tools` and `menu.tools`.
- Reuse the current HUD/timer direction where possible, but move ownership into a shared Tilt Trial session flow.
- Prefer one canonical ordered data source for level progression. Avoid duplicating level order in multiple UI and gameplay classes.
- Preview image support should be optional from day one so the system works before final art is ready.
