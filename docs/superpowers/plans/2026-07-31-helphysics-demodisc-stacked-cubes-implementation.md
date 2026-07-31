# HelPhysics Demo Disc Stacked-Cubes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax.

**Goal:** Produce a native Windows build of the real demo-disc `test_scene_dynamic_stack_boxes` scene using HelPhysics at 20 Hz with a 4/1 solve schedule, while proving that no main checkout changed and that BEPU was not selected or registered.

**Architecture:** Add a build-local physics-backend selection to the HelEngine code-generation graph, expose HelPhysics through the existing `IPhysicsRuntime` and scene-load lifecycle, and carry the validated selection into a generated Windows runtime manifest. A demo-disc-only editor prebuild command removes return-to-menu behavior from the copied build workspace, while a guarded PowerShell entrypoint builds all supporting tools from dedicated worktrees and verifies the final package.

**Tech Stack:** C#/.NET 9, xUnit, HelEngine editor build graph, csharpcodegen C#-to-C++, C++20, CMake/Ninja, DirectX 11, PowerShell, Git worktrees.

---

## Immutable paths and commits

- Demo worktree: `C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test` at `d716776b6ec6d719c7448fe57652cf342d0b09d6` plus this branch's commits.
- Engine worktree: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test`, branch `codex/helphysics-demodisc-stack-test`, based on `25374214867e8e5deeb3cd9e19893caeb2278b64`.
- Windows worktree: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test`, branch `codex/helphysics-demodisc-stack-test`, based on `b9590af9bece2c824b3531bdc5cc237853879188`.
- Codegen worktree: `C:\dev\helworks\csharpcodegen\.worktrees\helphysics-demodisc-stack-test`, branch `codex/helphysics-demodisc-stack-test`, based on `87958b3ac4f5b2e6221551874e8bf0a7a5620254`.
- Final output: `C:\dev\helprojs\output\windows-helphysics-stack-test`.
- Never edit or generate into the four main checkout roots.

### Task 1: Create and verify the supporting worktrees

**Files:**

- No repository files change in this task.

- [ ] **Step 1: Verify `.worktrees` is ignored in each supporting main repository**

Run:

```powershell
git -C C:\dev\helworks\helengine check-ignore -q .worktrees
git -C C:\dev\helworks\helengine-windows check-ignore -q .worktrees
git -C C:\dev\helworks\csharpcodegen check-ignore -q .worktrees
```

Expected: all three commands exit 0. If any fail, stop before creating a worktree.

- [ ] **Step 2: Capture each main checkout's immutable snapshot**

Run `git rev-parse HEAD` and `git status --porcelain=v1 --untracked-files=all` in all four main roots and save the exact output outside every repository under a new `%LOCALAPPDATA%\Temp\helengine-builds\...\main-snapshots` directory.

- [ ] **Step 3: Create dedicated worktrees at the exact baselines**

Use `git worktree add -b codex/helphysics-demodisc-stack-test <path> <commit>` for engine, Windows, and codegen. If a branch or worktree already exists, verify its path, branch, and merge base instead of recreating or deleting it.

- [ ] **Step 4: Verify all supporting worktrees are clean**

Expected: each new worktree has the required baseline HEAD and empty porcelain status.

### Task 2: Select the physics runtime in generated-core regeneration

**Files:**

- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor\managers\project\EditorPhysicsRuntimeBackendSelectionService.cs`
- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor.tests\managers\project\EditorPhysicsRuntimeBackendSelectionServiceTests.cs`
- Modify: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor\managers\project\EditorPlatformBuildGraphRunner.cs`
- Modify: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor.tests\managers\project\EditorPlatformBuildGraphRunnerTests.cs`

- [ ] **Step 1: Write failing selection tests**

Cover absent setting -> `bepu`, explicit `bepu`, explicit `helphysics`, unknown/blank values throwing with the setting id, and exactly one generated symbol. Use these stable values:

```csharp
public const string SettingId = "physics-runtime-backend";
public const string BepuValue = "bepu";
public const string HelPhysicsValue = "helphysics";
public const string BepuSymbol = "HELENGINE_PHYSICS_RUNTIME_BEPU";
public const string HelPhysicsSymbol = "HELENGINE_PHYSICS_RUNTIME_HELPHYSICS";
```

- [ ] **Step 2: Run the focused tests and confirm RED**

```powershell
dotnet test engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~EditorPhysicsRuntimeBackendSelectionServiceTests" --no-restore
```

Expected: compile failure because the service does not exist.

- [ ] **Step 3: Implement the selection service**

Implement one public sealed service with `Resolve(...)` and `BuildPreprocessorSymbols(...)`. Default only when the key is absent; reject a present blank value. Return one symbol and never both.

- [ ] **Step 4: Add the selected symbol to `RunRegenerateCore`**

Resolve the backend from `selectedCodegenOptionValues` and combine its one symbol with platform, scene-feature, and disabled-feature symbols before `GeneratedCoreRegenerationService.Regenerate(...)`.

- [ ] **Step 5: Run focused tests and confirm GREEN**

Run the selection tests plus `EditorPlatformBuildGraphRunnerTests`.

- [ ] **Step 6: Commit**

```powershell
git add engine/helengine.editor engine/helengine.editor.tests
git commit -m "feat: select generated physics runtime backend"
```

### Task 3: Generate HelPhysics into the native core only when selected

**Files:**

- Modify: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor\managers\project\EditorGeneratedCoreRegenerationService.cs`
- Modify: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor.tests\managers\project\EditorGeneratedCoreRegenerationServiceTests.cs`

- [ ] **Step 1: Write failing regeneration tests**

Test that `HELENGINE_PHYSICS_RUNTIME_HELPHYSICS` causes `engine/helengine.helphysics/helengine.helphysics.csproj` to be regenerated and merged, while absent/BEPU selection leaves the existing generated-core set unchanged. Assert the HelPhysics conversion report is merged and duplicate symbols are not emitted.

- [ ] **Step 2: Confirm RED with the focused test class**

```powershell
dotnet test engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~EditorGeneratedCoreRegenerationServiceTests" --no-restore
```

- [ ] **Step 3: Add conditional HelPhysics generation**

Add an explicit HelPhysics project path and temporary output root. Regenerate and merge it only when the HelPhysics symbol is present. Do not patch generated output and do not remove `helengine.physics3d`: that project still supplies serialized component types used by the scene.

- [ ] **Step 4: Confirm GREEN and inspect one generated output**

Run the focused tests, then invoke one temporary regeneration fixture and assert that `HelPhysicsRuntimeComponentRegistration.hpp` and its transitive dependencies exist.

- [ ] **Step 5: Commit**

```powershell
git add engine/helengine.editor engine/helengine.editor.tests
git commit -m "feat: generate selected HelPhysics runtime"
```

### Task 4: Integrate HelPhysics with Core's runtime and scene lifecycle

**Files:**

- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.helphysics\runtime\HelPhysicsSceneRuntime3D.cs`
- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.helphysics\runtime\HelPhysicsRuntimeComponentRegistration.cs`
- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.helphysics.tests\runtime\HelPhysicsSceneRuntime3DTests.cs`
- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.helphysics.tests\runtime\HelPhysicsRuntimeComponentRegistrationTests.cs`

- [ ] **Step 1: Write failing adapter tests**

Prove that the adapter rejects a step different from `Settings.FixedStepSeconds`, delegates an exact step to `HelPhysicsSceneBinder3D.Step()`, synchronizes dynamic entity transforms, and exposes the world's profiler metrics through `IPhysicsRuntimeProfilerMetricsProvider`.

- [ ] **Step 2: Write failing registration tests**

Create a hierarchy with one static ground and four dynamic boxes. Assert scene load creates exactly five bindings, attaches the adapter as `core.PhysicsRuntime`, reports `AfterHelPhysicsSceneBinding`, rejects unsupported colliders, and detaches before scene entities are disposed.

- [ ] **Step 3: Confirm RED**

```powershell
dotnet test engine\helengine.helphysics.tests\helengine.helphysics.tests.csproj --filter "FullyQualifiedName~HelPhysicsSceneRuntime3DTests|FullyQualifiedName~HelPhysicsRuntimeComponentRegistrationTests" --no-restore
```

- [ ] **Step 4: Implement the runtime adapter**

The adapter owns exactly one binder and implements:

```csharp
public void Step(double fixedStepSeconds) {
    if (fixedStepSeconds != Binder.World.Settings.FixedStepSeconds) {
        throw new InvalidOperationException("Core and HelPhysics fixed-step settings do not match.");
    }

    Binder.Step();
}
```

Expose the binder for lifecycle validation, but do not expose mutable solver storage.

- [ ] **Step 5: Implement registration**

`RegisterConfigured(Core core, HelPhysicsWorldSettings3D settings)` validates both arguments, subscribes to `SceneLoaded` and `SceneUnloading`, creates a fresh binder per compatible loaded scene, binds every root only after whole-hierarchy validation, wraps it in the adapter, and calls `core.AttachPhysicsRuntime(...)`. There is no BEPU call or fallback.

- [ ] **Step 6: Confirm GREEN and run the whole HelPhysics test project**

```powershell
dotnet test engine\helengine.helphysics.tests\helengine.helphysics.tests.csproj --no-restore
```

- [ ] **Step 7: Commit**

```powershell
git add engine/helengine.helphysics engine/helengine.helphysics.tests
git commit -m "feat: attach HelPhysics to packaged scenes"
```

### Task 5: Allow a build to use an isolated platform-installation manifest

**Files:**

- Modify: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor\managers\project\EditorSourceBuildWorkspaceLocator.cs`
- Modify: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor.tests\managers\project\EditorSourceBuildWorkspaceLocatorTests.cs`

- [ ] **Step 1: Write failing environment-override tests**

Test `HELENGINE_SHARED_USER_SETTINGS_ROOT` with a valid directory, missing directory, and no override. The no-override behavior must continue resolving the main checkout's shared `user_settings` when the editor runs from a worktree.

- [ ] **Step 2: Confirm RED, implement, and confirm GREEN**

Resolve the override only in `ResolveSharedEngineUserSettingsRootPath()`, normalize it to an absolute path, and throw when it is configured but missing. Do not alter `HELENGINE_SOURCE_ROOT` behavior.

- [ ] **Step 3: Commit**

```powershell
git add engine/helengine.editor engine/helengine.editor.tests
git commit -m "feat: isolate source-build platform settings"
```

### Task 6: Define and validate the Windows HelPhysics build settings

**Files:**

- Create: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder\WindowsPhysicsRuntimeConfiguration.cs`
- Create: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder.tests\WindowsPhysicsRuntimeConfigurationTests.cs`
- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder\WindowsPlatformDefinitionFactory.cs`
- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder.tests\WindowsPlatformAssetBuilderTests.cs`

- [ ] **Step 1: Write failing configuration tests**

Use stable setting ids `physics-runtime-backend`, `physics-fixed-step-hertz`, `physics-velocity-iterations`, and `physics-penetration-correction-passes`. Assert absent backend resolves to BEPU without requiring HelPhysics numbers; HelPhysics requires exactly positive numeric values; this test's values resolve to `helphysics`, `20`, `4`, `1`; unknown backend and malformed numbers throw.

- [ ] **Step 2: Add codegen-profile settings**

Add a backend selection with choices `bepu` and `helphysics`, defaulting to `bepu`, plus the three numeric text settings. Keep every existing build profile's default effective backend as BEPU.

- [ ] **Step 3: Confirm GREEN**

```powershell
dotnet test builder.tests\helengine.windows.builder.tests.csproj --filter "FullyQualifiedName~WindowsPhysicsRuntimeConfigurationTests|FullyQualifiedName~WindowsPlatformAssetBuilderTests" --no-restore
```

- [ ] **Step 4: Commit**

```powershell
git add builder builder.tests
git commit -m "feat: define Windows physics runtime selection"
```

### Task 7: Emit and package a single authoritative backend manifest

**Files:**

- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder\WindowsRuntimeNativeManifestWriter.cs`
- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder\WindowsBuildWorkspace.cs`
- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder.tests\WindowsRuntimeNativeManifestWriterTests.cs`
- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder.tests\WindowsProfilerArtifactPackagingTests.cs`

- [ ] **Step 1: Write failing writer and packaging tests**

Assert HelPhysics writes `runtime/runtime_physics_backend_manifest.hpp` containing only `HELENGINE_RUNTIME_PHYSICS_BACKEND_HELPHYSICS`, fixed-step constants `20/4/1`, and backend string `helphysics`. Assert JSON package artifact `runtime/runtime-physics-backend.json` has the same values. Assert BEPU emits only its own selection and ordinary release packaging remains valid.

- [ ] **Step 2: Extend the writer input**

Pass `request.SelectedCodegenOptionValues` into `WindowsRuntimeNativeManifestWriter.Write(...)`, resolve once through `WindowsPhysicsRuntimeConfiguration`, then emit both native and JSON forms from that same object.

- [ ] **Step 3: Package JSON after native build succeeds**

Copy the generated JSON to the final output and add it to the Windows build manifest. Missing or conflicting manifest state is fatal.

- [ ] **Step 4: Confirm GREEN and commit**

```powershell
dotnet test builder.tests\helengine.windows.builder.tests.csproj --filter "FullyQualifiedName~WindowsRuntimeNativeManifestWriterTests|FullyQualifiedName~WindowsProfilerArtifactPackagingTests" --no-restore
git add builder builder.tests
git commit -m "feat: package Windows physics backend manifest"
```

### Task 8: Select HelPhysics in the real Windows host

**Files:**

- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\src\platform\windows\win32\win32_application.cpp`
- Modify: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\src\platform\windows\win32\win32_window.cpp`
- Create: `C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder.tests\WindowsPhysicsRuntimeNativeSourceTests.cs`

- [ ] **Step 1: Write failing source-contract tests**

Assert the host includes the generated backend manifest, compiles exactly one registration branch, constructs `HelPhysicsWorldSettings3D` from generated `20/4/1` constants, sets `CoreInitializationOptions.PhysicsFixedStepSeconds` from the same frequency, calls `HelPhysicsRuntimeComponentRegistration::RegisterConfigured`, and leaves the BEPU default branch intact. Assert Escape closes the window only for the HelPhysics test backend.

- [ ] **Step 2: Implement compile-time backend branches**

Use the generated manifest macro, `__has_include` checks for every required HelPhysics header, and `#error` when a selected backend's generated types are absent. The HelPhysics branch must not compile or call `Physics3DRuntimeComponentRegistration`.

- [ ] **Step 3: Implement Escape exit**

Handle `VK_ESCAPE` in the Win32 window procedure under `HELENGINE_RUNTIME_PHYSICS_BACKEND_HELPHYSICS` by destroying the application window. Leave ordinary Windows input behavior unchanged for BEPU builds.

- [ ] **Step 4: Confirm GREEN and compile the unmodified default host**

Run `WindowsPhysicsRuntimeNativeSourceTests`, then the existing native CMake configure/build smoke test to prove the BEPU default path still compiles.

- [ ] **Step 5: Commit**

```powershell
git add src builder.tests
git commit -m "feat: boot Windows player with selected physics runtime"
```

### Task 9: Strip return-to-menu only in the copied demo build workspace

**Files:**

- Create: `C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test\assets\codebase\physics.tools\PrepareHelPhysicsStackTestSceneCommand.cs`
- Create: `C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor.tests\CityHelPhysicsStackTestScenePreparationTests.cs`
- Modify: `C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test\user_settings\build_config.json`

- [ ] **Step 1: Write failing project-source and command tests**

Load the committed scene and record its SHA-256. Copy it to a temporary project, execute `physics.prepare-helphysics-stack-test-scene`, and assert only `city.menu.DemoDiscReturnToMenuComponent` is removed. Assert one static and four dynamic `RigidBody3DComponent` records still pair with box colliders. Assert the source scene hash is unchanged.

- [ ] **Step 2: Implement the editor command**

Deserialize the exact `assets/scenes/physics/test_scene_dynamic_stack_boxes.helen`, recursively filter the return component record by its stable automatic persistence type id, require exactly one removal, and serialize back to the command context's project path. Throw when the scene or component is missing.

- [ ] **Step 3: Configure only the Windows test build**

Set Windows `selectedSceneIds` to only `test_scene_dynamic_stack_boxes`, order it first, select release/DirectX 11, set output to the dedicated test output, set codegen options to `helphysics`, `20`, `4`, `1`, and add the prebuild command under release. Do not change any other platform entry.

- [ ] **Step 4: Confirm GREEN and commit both repositories**

```powershell
dotnet test engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityHelPhysicsStackTestScenePreparationTests" --no-restore
git -C C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test add assets/codebase/physics.tools user_settings/build_config.json
git -C C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test commit -m "test: prepare isolated HelPhysics stack scene"
git -C C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test add engine/helengine.editor.tests
git -C C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test commit -m "test: verify demo HelPhysics scene preparation"
```

### Task 10: Add the guarded end-to-end PowerShell build

**Files:**

- Create: `C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test\build-helphysics-stack-windows.ps1`
- Create: `C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test\tests\build-helphysics-stack-windows.tests.ps1`

- [ ] **Step 1: Write failing script contract tests**

Assert exact worktree/baseline validation, four-main snapshot comparison, temporary platform manifest usage, isolated codegen/editor/builder paths, unique output path, conversion-report validation, backend JSON validation, one-scene manifest validation, executable existence, and no `Start-Process`/launch behavior.

- [ ] **Step 2: Implement the build script**

The script must:

1. snapshot all four main HEAD/status pairs;
2. verify every worktree branch and baseline ancestry;
3. verify the source scene hash against `d716776`;
4. build codegen Release and Windows builder Debug from their isolated worktrees;
5. create a temporary `platforms.json` whose Windows paths point only to those isolated artifacts;
6. set `HELENGINE_SHARED_USER_SETTINGS_ROOT` to that temporary directory;
7. invoke the isolated engine's `scripts/build-platform.ps1` with the demo worktree project, `windows`, release profile, and unique output;
8. reject conversion errors, backend values other than HelPhysics, extra scenes, a missing executable, or BEPU registration evidence;
9. compare all four main snapshots in `finally` and fail if any differ;
10. print the absolute executable path but never launch it.

Any temporary recursive cleanup must first resolve and validate that the target is beneath `%LOCALAPPDATA%\Temp\helengine-builds`.

- [ ] **Step 3: Run script contract tests and commit**

```powershell
pwsh -NoProfile -File .\tests\build-helphysics-stack-windows.tests.ps1
git add build-helphysics-stack-windows.ps1 tests/build-helphysics-stack-windows.tests.ps1
git commit -m "build: add isolated HelPhysics stack proof"
```

### Task 11: Run the smallest complete verification and build

**Files:**

- No planned source changes. Fix failures at their owning source or generator and recommit before rerunning.

- [ ] **Step 1: Run focused managed suites**

```powershell
dotnet test C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.helphysics.tests\helengine.helphysics.tests.csproj --no-restore
dotnet test C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~EditorPhysicsRuntimeBackendSelectionServiceTests|FullyQualifiedName~EditorGeneratedCoreRegenerationServiceTests|FullyQualifiedName~EditorSourceBuildWorkspaceLocatorTests|FullyQualifiedName~CityHelPhysicsStackTestScenePreparationTests" --no-restore
dotnet test C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test\builder.tests\helengine.windows.builder.tests.csproj --no-restore
```

- [ ] **Step 2: Execute the guarded build without launching**

```powershell
& C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test\build-helphysics-stack-windows.ps1
```

Expected: command exits 0 and reports the built executable in `C:\dev\helprojs\output\windows-helphysics-stack-test`.

- [ ] **Step 3: Verify package evidence independently**

Confirm:

- `runtime/runtime-physics-backend.json` says backend `helphysics`, frequency `20`, velocity iterations `4`, correction passes `1`;
- final build manifest lists exactly `test_scene_dynamic_stack_boxes` as startup and only scene;
- conversion report exists and contains zero errors;
- generated native source contains the HelPhysics registration call and no active BEPU branch for this artifact;
- packaged scene contains one static/four dynamic box bodies and no return-to-menu component;
- executable exists and has a fresh timestamp;
- all four main HEAD/status snapshots match their pre-build values.

- [ ] **Step 4: Review branch status and commits**

Run `git status --short` and `git log -5 --oneline` in all four worktrees. All tracked implementation changes must be committed; only ignored build outputs may remain.

- [ ] **Step 5: Stop without launching**

Report the absolute executable and manifest paths, exact verification commands/results, and supporting branch HEADs. Wait for Helena's explicit launch request.

## Self-review against the design

- The real committed `.helen` is cooked; no synthetic cube scene is introduced.
- The authored scene is never regenerated. The only mutation occurs in the build wrapper's copied project and removes one test-incompatible component.
- BEPU remains the absent-setting/default backend and is not registered by the HelPhysics artifact.
- Core and HelPhysics derive their step from one generated 20 Hz configuration and fail on disagreement.
- The regular DirectX 11 host, renderer, asset cook, generated C++, and native linker are all exercised.
- Every supporting source change is isolated to a dedicated worktree and branch.
- The final script proves main-checkout immutability and builds without launching.
