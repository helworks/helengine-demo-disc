# Console Camera/Light UI Blueprint Implementation Plan

> **For agentic workers:** Use `superpowers:subagent-driven-development` or `superpowers:executing-plans` when implementing this plan. Work task-by-task and keep the checklist current.

**Goal:** Generate one reusable `ConsoleCameraLightInstructions.hblueprint` containing the shared console camera/light panel, then attach it to the standard rendering and physics scenes for PS2, GameCube, Wii, Switch, and WiiU. The panel contains the existing background, one platform-native D-pad camera icon, the camera/light labels, and one platform-native light icon. WiiU explicitly uses the Wii icon family.

**Architecture:** Reuse `DemoSceneInstructionOverlayFactory` for viewport, panel, text, sprite, font, asset-reference, and platform-override authoring. Add a console-specific authoring method with one D-pad camera slot and console-only icon specifications. Serialize its temporary root through `BlueprintSaveService`. Add a scene-attachment helper that creates a `BlueprintInstanceComponent` root with entity platform-existence overrides. Targeted scene factories keep their existing overlay path for Windows and handheld platforms while excluding that legacy root on the five console platforms, so consoles receive exactly one Blueprint-backed panel.

**Tech stack:** C#/.NET 9, xUnit, `BlueprintSaveService`, `GeneratedControlIconAssetResolver`, editor scene persistence, and the HelEngine editor command host.

## Task 1: Lock the console Blueprint and WiiU fallback contracts with failing tests

**Files:**

- Create: `assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsSourceTests.cs`
- Create: `assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsBlueprintAssetGenerationTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/GeneratedControlIconAssetResolverTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs`
- Test: the focused console camera/light test slice

- [ ] **Step 1: Add source-level tests for the new catalog, generator, and attachment contract**

  Assert that the implementation will contain:

  - `ConsoleCameraLightInstructionsBlueprintRelativePath` with value `blueprints/ui/ConsoleCameraLightInstructions.hblueprint`.
  - the exact target set `ps2`, `gamecube`, `wii`, `switch`, and `wiiu`.
  - a console-specific factory method that creates one camera D-pad slot and one light row.
  - `BlueprintSaveService` usage.
  - `BlueprintInstanceComponent` attachment with console platform existence rules.
  - no direct console use of the old duplicated overlay root after migration.

- [ ] **Step 2: Add the WiiU family-map test**

  Extend `GeneratedControlIconAssetResolverTests` with assertions that:

  ```csharp
  Assert.Equal("wii", GeneratedControlIconPlatformMap.ResolveFamilyId("wiiu"));
  Assert.Equal(
      "images/instructions/controls/generated/wii/dpad.png",
      catalog.RequireControlPath("wii", "dpad"));
  ```

  The test must make the fallback explicit without requiring a new `wiiu` directory.

- [ ] **Step 3: Add the serialized-asset contract test**

  Load `assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint` with `AssetSerializer` and assert:

  - the Blueprint id is `blueprints/ui/ConsoleCameraLightInstructions.hblueprint`;
  - the root contains the screen-bound viewport and rounded background;
  - the root contains the `Camera` and `Light` text rows;
  - the camera side contains exactly one icon entity/slot;
  - asset references include PS2 D-pad/R1, GameCube D-pad/R, Wii D-pad/B, Switch D-pad/R, and WiiU-resolved Wii D-pad/B paths;
  - the Blueprint contains no `BlueprintInstanceComponent`, `DemoDiscLightToggleComponent`, light-state swatch, FPS, Back, or scene-specific gameplay component.

  Use serialized component type ids and asset-reference records rather than runtime entity lookup so the test verifies the actual cooked input asset.

- [ ] **Step 4: Run the focused tests and verify the expected red state**

  Run:

  ```powershell
  rtk dotnet test city.sln --filter "FullyQualifiedName~ConsoleCameraLightInstructions|FullyQualifiedName~GeneratedControlIconAssetResolverTests" --no-restore
  ```

  Expected: the new tests fail because the catalog, generator, attachment helper, WiiU mapping, and generated Blueprint do not exist yet.

## Task 2: Add the explicit WiiU icon mapping and console-only authoring primitives

**Files:**

- Modify: `assets/codebase/rendering.tools/GeneratedControlIconPlatformMap.cs`
- Modify: `assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs`
- Modify: `assets/codebase/rendering.tools.tests/GeneratedControlIconAssetResolverTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs`

- [ ] **Step 1: Map WiiU to the existing Wii family**

  Add `wiiu` to `FamilyIdsByPlatformId` with the value `wii`. Keep the mapping centralized so both Blueprint authoring and any future WiiU prompt resolution use the same deterministic fallback.

- [ ] **Step 2: Add the console icon specification tables**

  Add console-only specs for:

  | Platform | Camera | Light |
  | --- | --- | --- |
  | `ps2` | `dpad` | `r1` |
  | `gamecube` | `dpad` | `r` |
  | `wii` | `dpad` | `b` |
  | `switch` | `dpad` | `r` |
  | `wiiu` | Wii-family `dpad` | Wii-family `b` |

  Keep the old desktop/handheld tables available for non-console scenes. The new console method must not include the secondary camera slot.

- [ ] **Step 3: Factor a console-specific root method**

  Add a public method such as `CreateConsoleCameraLightInstructionsRoot(string projectRootPath, FontAsset font)` that reuses the existing viewport/background/layout constants and persistence helpers while authoring only:

  - the fixed 1280x720 screen-bound viewport;
  - the existing rounded background panel;
  - one `Camera` row with one D-pad icon;
  - one `Light` row with the platform override icon.

  Use PS2 as the shared serialized baseline and persist overrides for the other four platform ids, including an explicit `wiiu` override that resolves to the Wii family. Do not add FPS, Back, swatch, or light-toggle behavior.

- [ ] **Step 4: Run the focused resolver/source tests**

  ```powershell
  rtk dotnet test city.sln --filter "FullyQualifiedName~GeneratedControlIconAssetResolverTests|FullyQualifiedName~PromptIconOverlaySourceTests" --no-restore
  ```

  Expected: WiiU resolver assertions pass; Blueprint-generation and attachment tests remain red.

## Task 3: Generate and persist the reusable Blueprint

**Files:**

- Create: `assets/codebase/rendering.tools/ConsoleCameraLightInstructionsAssetCatalog.cs`
- Create: `assets/codebase/rendering.tools/ConsoleCameraLightInstructionsBlueprintGenerator.cs`
- Create: `assets/codebase/menu.tools/GenerateConsoleCameraLightInstructionsBlueprintCommand.cs`
- Modify: `assets/codebase/menu.tools/DemoMenuItemProvider.cs` if the project menu exposes standalone generation commands
- Test: `assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsBlueprintAssetGenerationTests.cs`
- Test: `assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsSourceTests.cs`

- [ ] **Step 1: Add the stable asset catalog**

  Define:

  ```csharp
  public const string ConsoleCameraLightInstructionsBlueprintRelativePath =
      "blueprints/ui/ConsoleCameraLightInstructions.hblueprint";
  public static readonly string[] ConsolePlatformIds =
      ["ps2", "gamecube", "wii", "switch", "wiiu"];
  ```

  Keep all paths project-relative and use forward slashes in serialized asset references.

- [ ] **Step 2: Implement the generator**

  Add `ConsoleCameraLightInstructionsBlueprintGenerator.Generate(string projectRootPath, DemoSceneInstructionOverlayFactory factory, FontAsset font)` with these checks:

  - reject an empty project path, null factory, null font, or missing active editor core;
  - call `factory.CreateConsoleCameraLightInstructionsRoot(projectRootPath, font)`;
  - save `assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint` through `BlueprintSaveService` and `GeneratedScenePersistenceRegistryFactory.Create()`;
  - dispose the temporary authoring root in a `finally` block.

- [ ] **Step 3: Add the explicit editor command**

  Add command id `menu.generate-console-camera-light-instructions-blueprint`. Resolve the editor default font, invoke the generator, and expose the command through the project’s menu catalog if required. The command is an explicit regeneration entry point; scene generation will also invoke the same generator so exports cannot depend on a manually run menu command.

- [ ] **Step 4: Run the generator source tests**

  ```powershell
  rtk dotnet test city.sln --filter "FullyQualifiedName~ConsoleCameraLightInstructionsSourceTests" --no-restore
  ```

  Expected: source-contract tests pass; the serialized-asset test remains red until the headless generation command creates the Blueprint.

## Task 4: Attach the Blueprint to console scenes and remove console duplication

**Files:**

- Create: `assets/codebase/rendering.tools/ConsoleCameraLightInstructionsSceneAttachmentService.cs`
- Modify: `assets/codebase/rendering.tools/RenderingSceneGenerator.cs`
- Modify: `assets/codebase/physics.tools/PhysicsSceneGenerator.cs`
- Modify: `assets/codebase/rendering.tools/AxisTestSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools/AxisTest2SceneFactory.cs`
- Modify: `assets/codebase/rendering.tools/CubeTestSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools/ScaledCubeSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs`
- Modify: `assets/codebase/physics.tools/PhysicsSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools.tests/ConsoleCameraLightInstructionsSourceTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs`

- [ ] **Step 1: Implement the attachment helper**

  Add a helper that creates a root entity named `ConsoleCameraLightInstructions` with a serialized `BlueprintInstanceComponent` pointing to the catalog path. Apply entity existence overrides so the root exists only for `ps2`, `gamecube`, `wii`, `switch`, and `wiiu`; it must be absent on Windows, PSP, PS Vita, DS, and 3DS. Reuse `PlatformSceneAuthoringHelperService`/entity existence persistence rather than duplicating raw serialization logic in each factory.

- [ ] **Step 2: Preserve the non-console legacy path**

  Keep the existing desktop/handheld overlay available for Windows and handheld scene paths. Add the corresponding exclusions to the old overlay root on the five console ids. This prevents a console scene from serializing both the old duplicated tree and the Blueprint instance while preserving unrelated platform presentation behavior.

- [ ] **Step 3: Replace direct factory calls with the helper**

  Update every rendering factory currently calling `CreateDesktopInstructionOverlayRoot` and the playable physics showcase path to use the shared attachment helper. Keep each factory’s camera, music, `DemoDiscLightToggleComponent`, indicator swatch, and scene-specific entities unchanged.

- [ ] **Step 4: Generate the Blueprint before scene writes**

  Invoke the generator from both `RenderingSceneGenerator.Generate` and `PhysicsSceneGenerator.Generate` (or their command-level equivalents) before writing scene assets. The operation is idempotent and ensures a standalone rendering or physics generation cannot leave a missing Blueprint reference.

- [ ] **Step 5: Run source and scene-attachment tests**

  ```powershell
  rtk dotnet test city.sln --filter "FullyQualifiedName~ConsoleCameraLightInstructions" --no-restore
  ```

  Expected: all focused source tests pass, including assertions that targeted factories use the attachment helper, the old overlay is excluded on consoles, and handheld/desktop behavior remains represented.

## Task 5: Regenerate authored assets and verify the serialized result

**Files:**

- Generated: `assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint`
- Generated: standard rendering scene files touched by the factory migration
- Generated: standard physics scene files touched by the factory migration

- [ ] **Step 1: Run rendering-scene generation**

  ```powershell
  rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-rendering-scenes
  ```

  Expected: the command ends with `Editor command 'menu.generate-rendering-scenes' executed successfully.` and writes the Blueprint.

- [ ] **Step 2: Run physics-scene generation**

  ```powershell
  rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-physics-scenes
  ```

  Expected: the command ends with `Editor command 'menu.generate-physics-scenes' executed successfully.` and does not create a second instruction hierarchy for console-targeted scenes.

- [ ] **Step 3: Re-run the serialized Blueprint test**

  ```powershell
  rtk dotnet test city.sln --filter "FullyQualifiedName~ConsoleCameraLightInstructionsBlueprintAssetGenerationTests" --no-restore
  ```

  Expected: the Blueprint exists, deserializes, has the required five platform icon overrides, and contains no prohibited systems.

- [ ] **Step 4: Inspect generated scene records**

  Extend the test or add a focused scene-deserialization assertion that checks representative rendering and physics scenes contain one `ConsoleCameraLightInstructions` Blueprint instance root, with the five console existence rules, and no serialized `DemoSceneInstructionViewport` root on those console variants. Verify `DemoDiscLightToggleComponent` remains present independently.

## Task 6: Export and verify the console package set

- [ ] **Step 1: Run the focused and full solution tests**

  ```powershell
  rtk dotnet test city.sln --filter "FullyQualifiedName~ConsoleCameraLightInstructions" --no-restore
  rtk dotnet test city.sln --no-restore
  ```

  Expected: focused tests and the full solution pass; only the repository’s existing warnings remain.

- [ ] **Step 2: Build each target console package**

  Run the project’s corrected build wrapper once per target:

  ```powershell
  rtk proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform ps2 -Output C:\dev\helprojs\demodisc\output\ps2
  rtk proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform gamecube -Output C:\dev\helprojs\demodisc\output\gamecube
  rtk proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform wii -Output C:\dev\helprojs\demodisc\output\wii
  rtk proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform switch -Output C:\dev\helprojs\demodisc\output\switch
  rtk proxy powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform wiiu -Output C:\dev\helprojs\demodisc\output\wiiu
  ```

  Expected: each command exits zero and reports `Build completed for platform '<platform>'`.

- [ ] **Step 3: Verify cooked scene content per package**

  For each output, verify the cooked `ConsoleCameraLightInstructions` Blueprint expansion and the expected platform icon asset family. WiiU must reference Wii-family D-pad/B assets; no `wiiu` icon directory is required. Verify the target scene list includes the full configured demo-disc scene set and no generated boot helper is selected before MainMenu.

- [ ] **Step 4: Perform runtime checks where a host is available**

  Launch the Windows package only for regression of the preserved desktop path. For console outputs, use the available platform runner/emulator or package-level validation and record any hardware-only checks that require the user. Confirm the first scene remains `DemoDiscMainMenu` and the camera/light panel appears once in rendering and physics scenes.

## Task 7: Final review and handoff

- [ ] **Step 1: Inspect the diff and generated asset references**

  ```powershell
  rtk git diff --check
  rtk git status --short
  rtk git diff --stat
  ```

  Confirm unrelated dirty work is preserved and no generated temporary files are accidentally added.

- [ ] **Step 2: Confirm acceptance criteria**

  - one tracked `assets/blueprints/ui/ConsoleCameraLightInstructions.hblueprint`;
  - one D-pad camera icon and one light icon per console platform override;
  - explicit WiiU-to-Wii fallback;
  - background and requested labels only;
  - no FPS, Back, swatch, or light behavior inside the Blueprint;
  - one Blueprint instance in targeted console scenes;
  - Windows/handheld overlay paths and light-toggle behavior preserved;
  - console exports build successfully and contain the expanded panel.

## Self-review

- The plan covers the approved spec at `docs/superpowers/specs/2026-07-14-console-camera-light-ui-blueprint-design.md`.
- Every production change is preceded by a failing focused test or source contract.
- The five console ids are repeated consistently across the catalog, icon specs, existence rules, tests, and export commands.
- WiiU is an explicit `wii` family mapping, not an implicit missing-family fallback.
- The old desktop/handheld path remains available outside the console target set, while console duplication is prevented.
- All instructions are concrete and self-contained; no step depends on an unstated earlier task.
