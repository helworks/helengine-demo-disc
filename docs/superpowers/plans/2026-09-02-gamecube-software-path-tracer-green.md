# GameCube Software Path Tracer Green Build Plan

## Goal

Produce a fresh DemoDisc GameCube image containing the shared `software_path_tracer` scene, while preserving the existing dirty GameCube renderer work and keeping target-specific configuration outside HelEngine core.

## Current evidence

- The Windows Release package succeeds and the live CPU renderer displays the 320x240 Cornell box.
- The GameCube builder test assembly compiles, then reports 96 passed and 24 failed.
- Nine failures are caused by `helengine.nativeownership.dll` not being copied into the test output.
- Four source-audit tests read untracked `tmp` build products and fail when those products are absent.
- The remaining failures are stale request, disc-layout, or runtime-source assertions against the current engine/platform contracts.
- The DemoDisc GameCube build exits before codegen because `project.heproj` requires engine version `1.0.0+fb94b93fbfd8c1e895c910a57903970c0e303900`, while the shared `helengine/user_settings/platforms.json` GameCube entry is stamped `1.0.0+13db86b8a91031015e3d0475799b6e6b1a56b309`.
- The configured GameCube builder DLL, player source root, generated-core root, and Release codegen executable all exist.

## Constraints

- Do not modify or stage the five existing dirty files in `C:\dev\helworks\helengine-gc`.
- Implement GameCube repairs in an isolated worktree rooted under `C:\dev\helprojs\.worktrees`.
- Do not add GameCube-specific utility code to HelEngine core.
- Do not patch generated C++ output.
- Keep codegen guarded: a windowed, nonresponsive worker or `WerFault` is a stop condition.
- Keep the live Windows tracer intact until GameCube verification no longer needs it.

## Task 1: Give DemoDisc a version-matched GameCube platform catalog

Files:

- Add `user_settings/gamecube-build-platforms/platforms.json`.
- Extend the existing DemoDisc project integration tests that validate platform packaging configuration.

Steps:

1. Add a failing test that loads the dedicated GameCube catalog and requires exactly one installed `gamecube` descriptor stamped with DemoDisc's required engine version.
2. Add the small catalog using absolute development paths for the existing GameCube builder, player source, generated-core root, and Release codegen executable, following the established Windows validation catalog pattern.
3. Run only the affected DemoDisc integration test project.

## Task 2: Restore hermetic GameCube builder tests

Files:

- Update `builder.tests/helengine.gamecube.builder.tests.csproj`.
- Update only the failing GameCube builder tests whose fixtures or current contracts changed.

Steps:

1. Add a direct project reference to `helengine.nativeownership` so serializer-dependent tests receive the runtime assembly in their output directory.
2. Remove source-audit tests that depend on untracked `tmp/generated-input-gamecube` or `tmp/builder-retail-check` build products; do not recreate or commit generated fixtures.
3. Supply the now-required generated-core root in the default-flow `PlatformBuildRequest` fixture.
4. Update disc-system-area assertions to the maintained raw-byte offset contract and reserved `FirstPayloadOffset` floor.
5. Run the focused dependency, builder-path, and disc-layout tests before the whole suite.

## Task 3: Rebase runtime-source contracts without weakening behavior

Files:

- Update `builder.tests/GameCubePackagedRuntimeSourceTests.cs` only where the current maintained source has replaced the asserted mechanism.
- Change production GameCube runtime code only when a failing assertion exposes behavior required by a real DemoDisc build, not merely historical diagnostic wording.

Steps:

1. For every remaining source assertion, identify the current owner and mechanism in maintained source.
2. Delete obsolete diagnostic-marker assertions that no longer describe a supported contract.
3. Update retained tests to assert behavior rather than stale spelling or removed internal helper names.
4. Preserve the existing dirty ordered-2D and aligned-text work; do not overwrite or absorb it into the repair commit.
5. Run the complete GameCube builder suite and require zero failures.

## Task 4: Build and smoke the actual DemoDisc image

Steps:

1. Rebuild the GameCube builder from the isolated repair worktree.
2. Run `build-platform.ps1` with platform `gamecube`, profile `gamecube-default`, and `HELENGINE_ENGINE_USER_SETTINGS_ROOT` pointing at DemoDisc's dedicated GameCube catalog.
3. Require a succeeded build state and a fresh, non-empty `game.gcm` containing the packaged `software_path_tracer` scene and generated gameplay module.
4. Launch the image through the repository's maintained Dolphin launcher and verify clean boot/scene navigation through logs and live process state.
5. Leave screenshot capture disabled unless Helena explicitly authorizes it for the GameCube/Dolphin smoke.

## Final verification gates

- DemoDisc focused platform-config tests: green.
- Complete GameCube builder suite: green.
- `git diff --check`: green in every changed worktree.
- DemoDisc GameCube build state: `succeeded`, exit code 0.
- Fresh non-empty `game.gcm` newer than the build start.
- No `codegen.exe` error window and no `WerFault` process.
- Dolphin launches the new image without an immediate host crash.
