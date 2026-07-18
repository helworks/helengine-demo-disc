# Physics DS Scene Generation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate Nintendo DS companion scenes for the curated `city` physics demo scenes, include them in the DS build flow, and keep the DS menu/startup mapping aligned with those new `_ds` scene ids.

**Architecture:** Keep the desktop physics `.helen` scenes as the authored source of truth and add a small city-owned generation path that emits `_ds` companion scenes through the existing generic Nintendo DS scaffold writer. Extend the existing city DS source audit and DS build selection so the new physics companion scenes are generated, packaged, and resolved through the existing DS scene-map startup flow.

**Tech Stack:** C#, xUnit source-audit tests, city editor command/generator code, generated `.helen` scene assets, Nintendo DS build configuration

---

### Task 1: Lock The Expected Physics DS Coverage With A Focused Audit

**Files:**
- Modify: `C:\dev\helworks\helengine-ds\builder.tests\CityNintendoDsSceneSourceAuditTests.cs`
- Test: `C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj`

- [ ] **Step 1: Write the failing audit assertions**

Add checks for:
- physics scene ids in `DemoDiscSceneCatalog`
- a city generator command/service that covers those physics scene ids
- DS build selection containing `test_scene_dynamic_stack_boxes_ds`, `test_scene_dynamic_sphere_stack_ds`, and `test_scene_dynamic_mixed_stack_ds`

- [ ] **Step 2: Run the focused audit and confirm failure**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter FullyQualifiedName~CityNintendoDsSceneSourceAuditTests --no-restore -v minimal
```

Expected: `FAIL` because the physics DS generation path and DS build selection are not present yet.

### Task 2: Add A City-Owned Physics DS Companion Generator

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscSceneCatalog.cs`
- Create or modify: `C:\dev\helprojs\demodisc\assets\codebase\scene.tools\...`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\GeneratedAuthoringSceneWriteService.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoMenuItemProvider.cs`

- [ ] **Step 1: Expose the curated physics scene ids from the catalog**

Add one focused catalog surface that returns the physics scene ids and their expected DS companion ids without duplicating the menu item definitions in multiple places.

- [ ] **Step 2: Write the failing generator test or audit seam**

If there is an existing city generator test seam, add the smallest failing test there. If there is not, extend the DS source audit to assert the new generator command/service file and the expected physics scene ids it targets.

- [ ] **Step 3: Add the physics DS generator service/command**

Implement one city-owned generator that:
- enumerates the curated physics scene ids
- loads each authored physics scene asset
- emits a `_ds` companion scene through the existing generic Nintendo DS scaffold path

- [ ] **Step 4: Surface the command in the editor menu**

Add a dedicated editor menu command or a clearly named existing menu entry update so the physics DS scenes can be regenerated intentionally, without piggybacking on unrelated rendering generation.

- [ ] **Step 5: Run the focused test or audit and verify it passes**

Run the smallest relevant test command from Steps 1-2 and confirm the new generator path is covered.

### Task 3: Generate The Physics DS Scene Assets

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\scenes\physics\test_scene_dynamic_stack_boxes_ds.helen`
- Create: `C:\dev\helprojs\demodisc\assets\scenes\physics\test_scene_dynamic_sphere_stack_ds.helen`
- Create: `C:\dev\helprojs\demodisc\assets\scenes\physics\test_scene_dynamic_mixed_stack_ds.helen`

- [ ] **Step 1: Run the city generator command**

Use the project’s headless editor-command flow or the normal editor command path to generate the three physics DS companion scene assets from the authored physics sources.

- [ ] **Step 2: Confirm the generated assets exist**

Verify the three `_ds` `.helen` files are present under `assets/scenes/physics/`.

- [ ] **Step 3: Spot-check the DS scaffold shape**

Confirm the generated assets use the generic DS scaffold rather than a duplicated or physics-specific layout path.

### Task 4: Include The Physics DS Scenes In The DS Build Flow

**Files:**
- Modify: `C:\dev\helprojs\demodisc\user_settings\build_config.json`
- Update generated output if needed: `C:\dev\helprojs\demodisc\assets\scenes\GeneratedBootScene.helen`

- [ ] **Step 1: Add the physics `_ds` scene ids to the DS-only build selection**

Update the DS build configuration so the packaged DS scene set includes the three physics companion ids and continues to exclude the non-DS playable physics scenes.

- [ ] **Step 2: Refresh the generated boot-scene mapping if required**

If the current DS boot scene maps only the rendering logical ids, regenerate or update it so the physics logical ids also resolve to their `_ds` companions.

- [ ] **Step 3: Re-run the DS source audit**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter FullyQualifiedName~CityNintendoDsSceneSourceAuditTests --no-restore -v minimal
```

Expected: `PASS`

### Task 5: Build Verification

**Files:**
- Verify outputs under: `C:\dev\helprojs\output\ds`

- [ ] **Step 1: Build the city solution**

Run:

```powershell
rtk dotnet build C:\dev\helprojs\demodisc\city.sln --no-restore -v minimal
```

Expected: build succeeds with only existing known warnings.

- [ ] **Step 2: Build the DS ROM**

Run:

```powershell
dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc --build ds --output C:\dev\helprojs\output\ds
```

Expected: `Build completed for platform 'ds'`

- [ ] **Step 3: Verify packaged scene coverage**

Confirm the DS package includes:
- `test_scene_dynamic_stack_boxes_ds`
- `test_scene_dynamic_sphere_stack_ds`
- `test_scene_dynamic_mixed_stack_ds`

Confirm the DS packaged playable scene set does not rely on the non-DS physics scene ids.

- [ ] **Step 4: Commit**

```powershell
git -C C:\dev\helprojs\demodisc add assets/codebase assets/scenes user_settings/build_config.json docs/superpowers/plans/2026-05-25-physics-ds-scene-generation.md
git -C C:\dev\helworks\helengine-ds add builder.tests/CityNintendoDsSceneSourceAuditTests.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: generate DS physics companion scenes"
git -C C:\dev\helworks\helengine-ds commit -m "test: audit city physics DS scene coverage"
```
