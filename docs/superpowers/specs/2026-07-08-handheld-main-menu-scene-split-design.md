# Handheld Main Menu Scene Split Design

## Summary

`DS` and `3DS` should stop sharing the same main-menu scene structure used by the rest of the platforms. The handheld presentation is structurally different enough that continuing to branch inside one shared scene generator creates the wrong abstraction boundary.

The solution is to generate two main-menu scenes from the same logical menu definition:

- `MainMenu` for non-handheld platforms
- `MainMenuHandheld` for `DS` and `3DS`

This keeps menu content, panel ids, item ids, and navigation targets shared while allowing each presentation to own its own scene graph, camera layout, animation policy, and helper entities.

## Goals

- Remove the assumption that one menu scene shape can serve every platform.
- Preserve one shared menu-definition source for entries, targets, labels, and panel structure.
- Let `DS` and `3DS` use a dedicated handheld scene layout without leaking handheld-only concerns into the standard menu scene.
- Keep boot/runtime selection explicit so each platform loads the correct generated main-menu scene.

## Non-Goals

- Creating different menu content for handheld platforms.
- Reworking runtime menu navigation semantics.
- Solving unrelated scene-generation or renderer issues outside the main-menu split.
- Introducing a third presentation family unless a later platform actually needs one.

## Current Problem

The current menu tooling already contains a dedicated Nintendo DS presentation path inside the main menu factory. That path owns different cameras, different screen composition, different layout rules, and handheld-specific presentation helpers. This means the system is already acknowledging that handhelds do not share the same structure as the other platforms, but it still hides that reality behind a single scene-generation concept.

That causes three problems:

1. The standard menu scene generator accumulates handheld-specific branches.
2. Handheld requirements appear as exceptions instead of as a first-class presentation model.
3. `3DS` cannot cleanly follow the same scene strategy as `DS` without pushing even more divergence into the shared path.

## Proposed Architecture

### Shared Logical Menu Definition

The menu definition remains the single source of truth for:

- panels
- items
- labels
- scene targets
- provider bindings
- shared metadata used by runtime navigation

No duplication of menu catalog data is introduced.

### Two Generated Scene Outputs

The menu generation flow emits two scene outputs:

- `MainMenu`
  Used by all non-handheld platforms.
- `MainMenuHandheld`
  Used by `DS` and `3DS`.

Both scenes represent the same logical menu. They differ only in presentation structure.

### Two Presentation Builders

Scene generation is split into two builders behind one shared orchestration layer:

- `StandardMainMenuSceneBuilder`
- `HandheldMainMenuSceneBuilder`

The orchestration layer reads the logical menu definition once, then delegates scene construction to the appropriate builder.

The handheld builder owns:

- top-screen branding scene graph
- bottom-screen menu scene graph
- handheld-specific cameras and viewport roots
- handheld-specific layout constants
- handheld-only helper entities/components
- handheld-specific animation policy

The standard builder owns:

- the current non-handheld presentation structure
- the current shared-camera/shared-layout assumptions used by desktop and console platforms

## Platform Mapping

Platform-to-scene selection becomes explicit:

- `DS` loads `MainMenuHandheld`
- `3DS` loads `MainMenuHandheld`
- all other supported platforms load `MainMenu`

This selection should happen in boot/build plumbing, not by post-generation scene surgery.

## Scene Structure Expectations

### Standard Scene

The standard scene remains the presentation used by existing non-handheld platforms. Its structure should stop carrying DS/3DS-specific camera and layout responsibilities.

### Handheld Scene

The handheld scene is allowed to differ completely in structure as long as it preserves shared runtime meaning. It can use:

- dual-screen camera composition
- handheld-specific overlay arrangement
- a handheld-specific branding hierarchy
- handheld-specific animation rules for the logo and related presentation elements
- handheld-friendly menu button sizing and spacing

This scene should not be treated as a variant squeezed into the standard layout. It is its own presentation.

## Boot And Build Flow

The build pipeline should cook both generated menu scenes.

Boot/runtime selection should choose the correct scene per platform instead of expecting a single generated menu scene to work everywhere. Any existing generated boot-scene logic that assumes one menu scene output should be updated to resolve the correct main-menu scene id for the active platform family.

The target behavior is:

- standard platforms enter `MainMenu`
- `DS`/`3DS` enter `MainMenuHandheld`

## Migration Plan

### Phase 1

Extract the existing DS-specific scene-building path into a dedicated handheld scene builder without changing behavior.

### Phase 2

Make the orchestration layer emit both scene outputs from the shared logical menu definition.

### Phase 3

Switch `DS` boot/build selection to `MainMenuHandheld`.

### Phase 4

Switch `3DS` boot/build selection to `MainMenuHandheld`.

### Phase 5

Remove DS/3DS presentation branches from the standard main-menu scene builder.

### Phase 6

Rename any ambiguous generator/build concepts so the two-scene model is obvious in code and generated outputs.

## Risks

### Scene Id Drift

If menu-scene ids are changed carelessly, boot logic and scene launch targets may stop resolving. Scene naming should be explicit and verified end-to-end.

### Shared Metadata Drift

If the handheld scene builder copies menu-definition logic instead of consuming the same shared data model, handheld and standard scenes will diverge over time. The builders must share logical menu input and differ only in presentation output.

### Hidden Runtime Assumptions

Any runtime component that assumes the main menu always has one specific scene graph shape may fail when handhelds switch to a different structure. Those assumptions must be identified and localized.

## Validation

### Functional Validation

- `DS` boots into `MainMenuHandheld`.
- `3DS` boots into `MainMenuHandheld`.
- a non-handheld platform boots into `MainMenu`.
- shared menu entries and navigation targets match across both outputs.
- selecting the same logical menu item launches the same target scene on both outputs.

### Presentation Validation

- handheld main menu preserves the intended handheld layout and animation behavior.
- standard main menu no longer carries handheld-specific scene-graph branches.
- `3DS` can evolve with the handheld presentation without requiring new branches in the standard menu builder.

### Regression Validation

- changing menu definition content updates both generated scene outputs.
- existing non-handheld platforms keep their current main-menu behavior.
- `DS` and `3DS` do not depend on runtime-generated scene mutation to reach the handheld structure.

## Recommendation

Implement the two-scene split using one shared logical menu definition and two dedicated presentation builders. This gives `DS` and `3DS` a correct first-class scene boundary while preserving one source of truth for menu content and navigation.
