# City DS Boot And Scene Restoration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore the `city` DS boot and scene-generation path so Nintendo DS builds boot through `GeneratedBootScene`, retain `DemoDiscMainMenuDs`, and remap playable scene loads to DS-suffixed scene assets.

**Architecture:** Restore the deleted city-owned DS companion-scene generation path instead of moving DS policy into the builder. Keep `GeneratedBootScene` as the DS startup scene, restore the scene-map resolution calls in gameplay menu components, and reintroduce the menu-scene generation files that materialize `DemoDiscMainMenuDs`.

**Tech Stack:** C#, xUnit source-audit tests, city generated scene assets, Nintendo DS scene-map boot flow

---

### Task 1: Lock The Regression With A Focused Audit Test

**Files:**
- Create: `C:\dev\helworks\helengine-ds\builder.tests\CityNintendoDsSceneSourceAuditTests.cs`
- Test: `C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj`

- [ ] **Step 1: Write the failing audit test**

Add assertions for:
- missing `city` menu generator files that should regenerate `DemoDiscMainMenuDs`
- missing `SceneMapComponent.ResolveSceneId(...)` calls in city menu runtime components
- missing DS companion-scene writer path in `GeneratedAuthoringSceneWriteService`
- missing DS boot-scene mappings and DS menu asset file

- [ ] **Step 2: Run the focused test and confirm failure**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter FullyQualifiedName~CityNintendoDsSceneSourceAuditTests --no-restore -v minimal
```

Expected: `FAIL` because the current city worktree deleted the DS generator files and remap path.

### Task 2: Restore DS Menu Generation And Runtime Remap Wiring

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\menu\MenuComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\menu\DemoDiscReturnToMenuComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\menu\NintendoDsReturnOverlayComponent.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\menu.tools\DemoDiscMainMenuSceneFactory.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\menu.tools\DemoDiscSceneGenerator.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\menu.tools\RegenerateDemoDiscMainMenuCommand.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\GeneratedAuthoringSceneWriteService.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs`

- [ ] **Step 1: Restore the scene-map runtime calls**

Reintroduce `SceneMapComponent.ResolveSceneId(...)` before runtime scene loads in the three city menu components.

- [ ] **Step 2: Restore DS companion-scene writing**

Restore the DS scaffold writer path in `GeneratedAuthoringSceneWriteService` so generated authored scenes can emit `NintendoDsScene` companion assets again.

- [ ] **Step 3: Restore menu scene generation**

Recreate the deleted menu-tool generator files and ensure `DemoDiscMainMenuSceneFactory` emits a DS companion scene definition for `scenes/DemoDiscMainMenuDs.helen`.

### Task 3: Restore DS Assets And Boot Mapping Coverage

**Files:**
- Restore: `C:\dev\helprojs\city\assets\scenes\DemoDiscMainMenuDs.helen`
- Restore or update: `C:\dev\helprojs\city\assets\scenes\GeneratedBootScene.helen`

- [ ] **Step 1: Restore the committed DS menu asset**

Bring back `DemoDiscMainMenuDs.helen` so the current worktree matches the intended DS scene set.

- [ ] **Step 2: Restore the boot-scene mapping coverage**

Ensure `GeneratedBootScene.helen` contains the DS scene-map entries required for the playable rendering scene ids.

### Task 4: Verify The Repair

**Files:**
- Test: `C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj`
- Build: `C:\dev\helprojs\city\city.sln`

- [ ] **Step 1: Re-run the focused audit test**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter FullyQualifiedName~CityNintendoDsSceneSourceAuditTests --no-restore -v minimal
```

Expected: `PASS`

- [ ] **Step 2: Build the city solution**

Run:

```powershell
rtk dotnet build C:\dev\helprojs\city\city.sln --no-restore -v minimal
```

Expected: `Build succeeded.`
