# HelPhysics Standalone Windows Demo Design

## Goal

Create a small Windows-only HelPhysics showcase that boots directly into a four-box stack, exposes live profiler information, and demonstrates sleep and wake behavior through simple keyboard controls. The showcase exists only in an isolated `demodisc` worktree until Helena explicitly requests integration.

## Isolation

The demo lives on branch `codex/helphysics-standalone-demo` at:

`C:\dev\helprojs\demodisc\.worktrees\helphysics-standalone-demo`

The branch starts from committed `demodisc/main` commit `a085d58`. It must not read implementation files from, write to, regenerate assets inside, or commit changes from the dirty main checkout at `C:\dev\helprojs\demodisc`.

The demo may consume reviewed HelPhysics engine commits from the isolated engine feature branch. It must not merge HelPhysics or demo changes into either main branch as part of this work.

## Entry and Runtime Selection

The Windows demo build boots directly into one dedicated scene. It does not show the splash screen, main menu, level selector, Tilt Trial, or any other demo-disc content.

HelPhysics registration is explicit and local to this demo configuration. BEPU remains the engine and normal demo-disc default. If HelPhysics registration or scene binding is unavailable, startup fails with a clear diagnostic rather than silently selecting BEPU.

The demo uses the generated native Windows C++ runtime through a Profiler build. A managed-only preview does not satisfy final acceptance.

## Scene

The scene contains:

- one static ground box;
- four dynamic unit boxes aligned vertically;
- one fixed three-quarter camera that clearly shows the entire stack after an impulse;
- simple directional lighting and existing solid-color cube materials;
- one unobtrusive profiler overlay.

Existing committed rendering assets may be reused. The branch does not alter shared menu, Tilt Trial, splash, or platform presentation assets.

The physics world advances at exactly 20 Hz using the reviewed HelPhysics defaults of four velocity iterations and one penetration-correction pass. Rendering consumes the latest synchronized entity transforms through the normal scene-binding layer.

## Runtime Ownership and Data Flow

The scene-binding layer owns the HelPhysics world and entity-to-body handles. Scene entities do not access body pools, manifolds, solver constraints, or other HelPhysics internals.

At startup, binding creates the static ground and four dynamic bodies from explicit authored descriptions. Returned pending handles become active at the next successful fixed step. Dynamic snapshots synchronize back to scene transforms after each step.

Input enters the fixed command boundary:

- `Space` queues an impulse on the top cube for the next fixed step;
- `R` queues removal of the current dynamic bodies and reconstruction of the original stack with new generation-safe handles;
- `Esc` exits the application through the existing input/application path.

The demo never mutates active solver structures directly.

## Profiler Overlay

The bottom or top corner overlay displays:

- HelPhysics step time measured outside the allocation-sensitive world;
- total and awake dynamic body counts;
- candidate, manifold, and contact counts;
- active and sleeping island counts;
- current sleep state;
- the fixed rate (`20 Hz`).

The overlay reads immutable HelPhysics step counters and the existing outer profiler timing path. No `Stopwatch` or UI work enters HelPhysics body/contact loops.

## Expected Interaction

On launch, the stack visibly settles and transitions to zero awake dynamic bodies. Persistent support contacts and one sleeping island remain available for wake detection.

Pressing `Space` wakes the connected stack, visibly disturbs the top cube, updates the overlay, and allows the bodies to sleep again naturally. Pressing `R` reconstructs the authored stack rather than teleporting existing bodies or resetting solver state in place.

## Failure Behavior

The demo fails explicitly when:

- the HelPhysics runtime or scene binder is not registered;
- the native build does not contain the HelPhysics module;
- the authored fixed step differs from 20 Hz;
- a required body, shape, command, contact, manifold, or island capacity is unavailable;
- a stale or foreign body handle reaches demo input logic.

There is no fallback backend, forced sleeping, pose snapping, ignored command, or best-effort scene repair.

## Verification

Automated verification covers:

- the isolated build profile selects the dedicated scene and HelPhysics runtime;
- the scene contains exactly one static ground and four dynamic cubes;
- fixed-step configuration is exactly 20 Hz;
- `Space`, `R`, and `Esc` use the intended action/input path;
- reset replaces handles rather than reusing stale identities;
- overlay values come from HelPhysics metrics and outer timing;
- no normal demo-disc startup/menu/Tilt Trial source is changed;
- the settled HelPhysics step path remains allocation-free in engine tests.

Final manual acceptance uses the native Windows Profiler executable:

1. Launch directly into the stack scene in a visible window.
2. Observe all four cubes settle and the overlay reach zero awake dynamic bodies and one sleeping island.
3. Press `Space`; observe a visible impulse, wake metrics, and eventual resleep.
4. Press `R`; observe a clean four-box reconstruction and fresh settling cycle.
5. Press `Esc`; observe a normal exit.

The build output uses a demo-specific output directory and does not overwrite the normal demo-disc Windows output.

## Worktree Baseline Setup

`user_settings/generated_code` is intentionally ignored and absent in a fresh worktree. Before implementation tests run, the normal editor-owned project generation command must regenerate module projects inside this worktree. Generated project files must not be copied from the dirty main checkout.

If the regenerated committed baseline fails, implementation pauses until Helena explicitly approves proceeding with the documented baseline failure.

## Out of Scope

- merging or copying this demo into `demodisc/main`;
- changing the normal demo-disc startup flow or default physics backend;
- adding the demo to the main menu;
- modifying Tilt Trial or its generated scenes and presentation assets;
- supporting PS2, DS, 3DS, Wii, GameCube, or other non-Windows platforms;
- adding joints, vehicles, continuous collision detection, static meshes, or non-box collision shapes;
- turning the showcase into a general benchmark browser or editor tool.
