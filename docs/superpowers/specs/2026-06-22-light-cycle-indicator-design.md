# Light Cycle Indicator Design

## Goal

Add one live light-state indicator to the scene UI used by the desktop and console rendering demo scenes.

The indicator must:

- appear at the top left, directly below the FPS overlay
- display the label `Light`
- display one filled preview square beside the label
- show the currently active directional-light color in that square
- keep the existing bottom-left `Toggle Light` instruction row unchanged

Pressing `L` must cycle the shared demo-disc light state through:

1. white
2. yellow
3. red
4. blue
5. green
6. off

All supported scenes should start in the `white` state regardless of their currently authored directional-light tint.

## Problem

The current demo-disc light toggle is binary:

- on
- off

That is enough to prove input works, but it does not communicate the current light state clearly and it does not help with quick visual lighting checks across platforms. The UI also gives no feedback about the active light color, so the player has to infer state from the rendered scene.

The feature needs one stable, always-visible state readout without disturbing the shared bottom-left instruction panel that already explains the controls.

## Scope

This change includes:

- extending the demo-disc light toggle behavior from binary on/off to a fixed color cycle plus off
- adding one live top-left `Light` indicator row under the FPS overlay
- updating the indicator square whenever the light state changes
- normalizing all controlled directional lights to the `white` startup state on scene initialization

This change does not include:

- changing the bottom-left instruction overlay layout
- adding platform-specific variants of the new indicator
- preserving each scene's authored light color as an initial runtime state
- extending the feature to Nintendo DS

## Chosen Approach

Use the existing `DemoDiscLightToggleComponent` as the single owner of both the runtime light state and the live indicator state.

Add the indicator entities to each scene UI root that already contains:

- `FPSComponent`
- `DemoDiscReturnToMenuComponent`
- `DemoDiscLightToggleComponent`

The light-toggle component should discover and cache:

- controlled directional lights
- the label text component for the indicator
- the preview square rounded-rectangle component for the indicator

This keeps the feature aligned with the current architecture:

- scene factories remain responsible for authoring UI entities
- the runtime component remains responsible for gameplay state and input response

## Alternatives Considered

### 1. Recommended: runtime-owned indicator under the scene UI root

Add a small authored UI row in each scene factory and let `DemoDiscLightToggleComponent` update it.

Pros:

- minimal change to existing structure
- indicator remains scene-local and easy to reason about
- no dependency on the bottom-left shared instruction overlay
- one runtime component owns both light state and indicator state

Cons:

- several scene factories need the same small UI addition

### 2. Put the indicator into `DemoSceneInstructionOverlayFactory`

Attach the live indicator to the shared bottom-left instruction panel.

Pros:

- one central overlay factory

Cons:

- wrong placement for the requested design
- mixes static control instructions with live state display
- makes the shared panel responsible for runtime state it does not currently own

### 3. Add a separate runtime indicator component

Create one dedicated component just to manage the UI preview while `DemoDiscLightToggleComponent` keeps the light cycle.

Pros:

- slightly narrower responsibility per component

Cons:

- introduces coordination state between two runtime components
- unnecessary split for a small feature
- increases scene wiring complexity without a real payoff

## Runtime Behavior

`DemoDiscLightToggleComponent` should stop behaving like a boolean toggle and instead behave like a fixed cycle state machine.

The cycle order is:

1. white
2. yellow
3. red
4. blue
5. green
6. off

On initialization:

- the component captures the controlled directional lights after full scene initialization
- the component captures the authored indicator UI components
- the component immediately applies the `white` state

The `white` startup state is intentional even for scenes that currently author warm lights such as:

- `DirectionalShadowPlazaSceneFactory`
- `SpotlightStreetSliceSceneFactory`

That means this feature standardizes initial runtime behavior across scenes instead of preserving scene-authored tint as the first visible state.

When `L` is pressed:

- advance to the next state
- apply that state to every controlled directional light
- update the preview square color

For the `off` state:

- directional-light intensity becomes `0f`
- directional-light shadowing becomes disabled
- the preview square should still remain visible as a dark neutral square so the user can identify the current state

For all colored states:

- the same color is applied to every controlled directional light
- the original authored intensity should be preserved for non-off states

## UI Design

The new indicator belongs to the top-left scene UI root, not the shared instruction panel.

The row should contain:

- one text label with `Light`
- one small filled square to the right of the label

Placement:

- directly below the FPS overlay
- aligned to the same top-left region already used by the scene UI entity

Visual style:

- label uses the same editor font already assigned to the scene UI
- preview square uses a subtle border so blue and dark states still read cleanly against the cornflower background
- no animation is required

The existing bottom-left `Toggle Light` instruction row remains unchanged and continues to describe the control.

## Affected Areas

### Runtime component

Update:

- `assets/codebase/rendering/DemoDiscLightToggleComponent.cs`

Responsibilities added:

- fixed light-state cycle
- startup normalization to white
- UI indicator discovery
- preview square updates

### Scene UI authoring

Update the scene UI creation paths in the rendering demo factories that currently add:

- `FPSComponent`
- `DemoDiscReturnToMenuComponent`
- `DemoDiscLightToggleComponent`

These factories should also author:

- one `TextComponent` for the `Light` label
- one `RoundedRectComponent` for the color preview square

The factories already using the shared demo-disc toggle should all get the same indicator row so behavior stays consistent across scenes.

## Data Flow

1. Scene factory authors the UI indicator entities under the scene UI root.
2. Runtime scene loading materializes those components normally.
3. `DemoDiscLightToggleComponent.ComponentInitialized` captures:
   - directional lights
   - indicator UI components
4. The component applies the canonical `white` startup state.
5. The user presses `L`.
6. The component advances to the next state and updates:
   - light color
   - light intensity and shadow-enabled flag for `off`
   - preview square fill color

## Error Handling

The implementation should keep current failure semantics:

- if the input system is missing, throw
- if the object manager is missing, throw

For the indicator UI:

- do not silently create fallback UI at runtime
- if required indicator entities or components are missing in a scene that is supposed to support the feature, fail fast with a clear exception

That follows the existing project rule against runtime best-effort patching.

## Testing Strategy

Follow red-green.

### Source tests

Add or update focused source tests that prove:

- the light-toggle component uses a multi-state cycle instead of a boolean on/off toggle
- the scene UI factories author the new `Light` label and preview square
- the startup state is normalized to white

### Runtime behavior tests

Add targeted tests around the component behavior that prove:

- initialization applies white immediately
- repeated `L` presses cycle through the exact requested order
- `off` sets intensity to zero and disables shadows
- leaving `off` restores the active non-off intensity and re-enables shadows
- the preview square color matches the current cycle state

### Build verification

After implementation:

1. rebuild the Windows city output
2. launch the build
3. load `cube_test`
4. confirm the `Light` indicator appears below the FPS overlay
5. press `L` repeatedly and verify:
   - white
   - yellow
   - red
   - blue
   - green
   - off
6. confirm the bottom-left `Toggle Light` instruction row remains unchanged

## Exit Criteria

This change is successful when:

1. every supported demo-disc rendering scene starts with a white light state
2. the top-left scene UI shows `Light` plus a live preview square below the FPS overlay
3. `L` cycles exactly through white, yellow, red, blue, green, off
4. the preview square always matches the active light state
5. the existing bottom-left `Toggle Light` instruction row remains intact
