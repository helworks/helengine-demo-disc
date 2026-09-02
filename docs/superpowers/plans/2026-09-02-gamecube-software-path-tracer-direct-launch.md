# GameCube Software Path Tracer Direct-Launch Validation Plan

**Goal:** Produce and launch a GameCube validation disc that boots directly into DemoDisc's `software_path_tracer` scene without changing the normal DemoDisc startup order.

## Constraints

- Keep the authored scene, renderer, and GameCube runtime source unchanged.
- Preserve the normal GameCube build selection in `user_settings/build_config.json` after the validation build.
- Continue monitoring `codegen.exe`; abort the build if it exposes a window, becomes nonresponsive, or launches Windows Error Reporting.
- Do not capture a Dolphin screenshot without explicit authorization.

## Steps

1. Temporarily narrow only the GameCube `selectedSceneIds` entry to `software_path_tracer`.
2. Build into the dedicated `gamecube-path-tracer-build` output directory.
3. Restore `user_settings/build_config.json` exactly and confirm it has no diff.
4. Verify the generated GameCube runtime manifest embeds `software_path_tracer` as its startup scene and the packaged disc contains the cooked scene.
5. Stop only the previous Dolphin validation process, launch the direct-start `game.gcm`, and verify that Dolphin remains responsive while emulation time advances.

## Acceptance

- The dedicated GameCube build succeeds.
- The normal DemoDisc build configuration is unchanged after the build.
- The dedicated disc's embedded startup scene is `software_path_tracer`.
- Dolphin runs the dedicated disc without a host-side crash or codegen MessageBox.
