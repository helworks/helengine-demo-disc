# GameCube Software Path Tracer Direct-Launch Validation Plan

**Goal:** Produce and launch a GameCube validation disc that boots directly into DemoDisc's `software_path_tracer` scene without changing the normal DemoDisc startup order.

## Constraints

- Keep the authored scene, renderer, and GameCube runtime source unchanged.
- Preserve the normal GameCube build selection in `user_settings/build_config.json` after the validation build.
- Continue monitoring `codegen.exe`; abort the build if it exposes a window, becomes nonresponsive, or launches Windows Error Reporting.
- Do not capture a Dolphin screenshot without explicit authorization.

## Steps

1. Build into the dedicated `gamecube-path-tracer-build` output directory so the complete DemoDisc scene catalog is cooked and packaged.
2. Stage a validation-only copy of the generated GameCube runtime manifest, change only its embedded startup id to `software_path_tracer`, and copy it into the generated build cache.
3. Incrementally rebuild the packaged-mode native DOL, copy it into the dedicated extracted-disc layout, and repackage that layout as `game.gcm`.
4. Restore the original generated-cache manifest and confirm `user_settings/build_config.json` has no diff.
5. Verify the direct-start DOL contains `software_path_tracer` as its startup scene and the packaged disc contains the cooked scene.
6. Stop only the previous Dolphin validation process, launch the direct-start `game.gcm`, and verify that Dolphin remains responsive while emulation time advances.

## Acceptance

- The dedicated GameCube build succeeds.
- The normal DemoDisc build configuration is unchanged after the build.
- The dedicated disc's embedded startup scene is `software_path_tracer`.
- Dolphin runs the dedicated disc without a host-side crash or codegen MessageBox.
