# Software Path Tracer Build Configuration Plan

> **Worker:** Implement with `superpowers:test-driven-development` after the scene/catalog assets are accepted.

**Goal:** Package the one shared `software_path_tracer` scene on all ten configured targets, restore canonical GameCube declaration, and make the active build profile regenerate both rendering scenes and the DemoDisc menu before packaging.

**Files:**

- Modify: `project.heproj`
- Modify: `user_settings/build_config.json`
- Modify: `assets/codebase/game.tools.tests/DemoDiscBuildConfigTests.cs`
- Create: `assets/codebase/game.tools.tests/SoftwarePathTracerProjectIntegrationTests.cs`
- Create: `assets/codebase/game.tools.tests/SoftwarePathTracerProjectIntegrationTests.cs.hmeta`

## Task 1: Make configuration tests worktree-correct and red

- [ ] Replace hard-coded `C:\dev\helprojs\demodisc` reads in the touched build-config tests with one checkout-root resolver that walks upward from the test assembly until it finds both `project.heproj` and `user_settings/build_config.json`.
- [ ] Extend the expected common scene package with `software_path_tracer` immediately after `pbr_shadow_theater`.
- [ ] Add focused integration tests requiring `project.heproj.supportedPlatforms` to match `settings/platforms.json.supportedPlatforms` as an ordinal set of exactly these ten IDs: `windows`, `ps2`, `psp`, `psvita`, `ds`, `3ds`, `gamecube`, `wii`, `switch`, `wiiu`.
- [ ] Assert `gamecube` appears exactly once and `gc` never appears.
- [ ] For each of the ten `build_config.json` platform objects, assert `software_path_tracer` appears exactly once in `selectedSceneIds` and exactly once in `sceneOrders`, immediately after `pbr_shadow_theater`, with the next consecutive order number.
- [ ] Assert each platform's currently selected build profile has both `menu.generate-rendering-scenes` and `menu.regenerate-demo-disc-main-menu` in `editorPrebuildCommandIdsByBuildProfileId` exactly once and in that order.
- [ ] Preserve the existing PS2 `colored-cube-grid` special profile and all unrelated profile command lists.
- [ ] Assert source contracts still pin DS to `256x192`, 3DS to monoscopic `320x240`, and every other trace resolution to `320x240`.
- [ ] Run the focused game.tools tests and confirm failures are from the missing platform/scene/prebuild entries.

## Task 2: Restore canonical GameCube support

- [ ] Add `"gamecube"` once to `project.heproj.supportedPlatforms`; preserve all nine existing IDs and formatting style.
- [ ] Do not introduce `gc`, rename settings files, or change `settings/platforms.json` (it is already canonical).

## Task 3: Add the scene to all ten packages

- [ ] Append `software_path_tracer` after `pbr_shadow_theater` in every platform's `selectedSceneIds`.
- [ ] Add its `sceneOrders` record after `pbr_shadow_theater`, using `23` on the current 22-scene non-handheld packages and `21` on the current 20-scene DS/3DS packages.
- [ ] Do not substitute a handheld companion ID; all targets package the same logical `software_path_tracer` scene.
- [ ] Do not change graphics profiles. Windows remains `directx11`; the preserved experimental Vulkan work is not selected.

## Task 4: Make selected builds regenerate current assets

- [ ] For each platform, ensure the map entry keyed by `selectedBuildProfileId` contains, in order:

```json
[
  "menu.generate-rendering-scenes",
  "menu.regenerate-demo-disc-main-menu"
]
```

- [ ] If an active profile already contains other commands, retain them and insert the two required generation commands without duplicates.
- [ ] Keep PS2's `ps2-default`, `debug`, `release`, and empty `colored-cube-grid` profile semantics; only the selected `ps2-default` entry is mandatory for this task, while existing debug/release generation remains intact.
- [ ] Keep PSP's existing debug/release regeneration intact.

## Task 5: Verify and commit

- [ ] Parse both JSON files through `ConvertFrom-Json`.
- [ ] Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/game.tools.tests/game.tools.tests.csproj --no-restore --filter "FullyQualifiedName~DemoDiscBuildConfigTests|FullyQualifiedName~SoftwarePathTracerProjectIntegrationTests" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/game.tools/game.tools.csproj --no-restore -v:minimal
rtk git diff --check
```

- [ ] Inspect all ten platform blocks, especially DS/3DS order numbers and PS2 special profiles.
- [ ] Commit as `Package software path tracer on every target`.

