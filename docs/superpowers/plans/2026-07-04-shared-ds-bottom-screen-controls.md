# Shared DS Bottom-Screen Controls Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make every scaffolded handheld render scene and physics scene emit the same bottom-screen control contract: `FPS 1x`, full-width `Light` button with swatch, and full-width `Back` button, with both touch and `R` advancing the shared light cycle.

**Architecture:** Keep ownership in `NintendoDsRenderingSceneScaffoldFactory` and route both rendering and physics DS companion scenes through that one canonical layout. Add a scaffold-owned handheld light controller component that uses the same cycle semantics as `DemoDiscLightToggleComponent`, bind it to a scaffold-owned `Light` button plus swatch, and remove the legacy temporary default overlay path so handheld bottom UI no longer depends on scene-authored variation.

**Tech Stack:** C#, city generated-scene authoring pipeline, xUnit source audits, editor-command scene regeneration, Nintendo 3DS runtime verification

---

### Task 1: Lock The Shared Handheld Contract In Tests

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityNintendoDsBottomScreenControlsSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Write the failing scaffold source audit**

Create `CityNintendoDsBottomScreenControlsSourceTests.cs` with source-audit tests that read the current city source files and assert the shared handheld contract.

```csharp
namespace helengine.editor.tests;

/// <summary>
/// Verifies the shared DS companion-scene scaffold owns the canonical handheld bottom-screen controls contract.
/// </summary>
public sealed class CityNintendoDsBottomScreenControlsSourceTests {
    /// <summary>
    /// Ensures the shared DS scaffold authors a full-width light button, swatch, back button, and one-times FPS scale.
    /// </summary>
    [Fact]
    public void City_ds_scaffold_source_authors_canonical_bottom_screen_controls() {
        string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("FontScale = 1f", source, StringComparison.Ordinal);
        Assert.Contains("CreateBottomScreenLightButton(", source, StringComparison.Ordinal);
        Assert.Contains("CreateBottomScreenBackButton(", source, StringComparison.Ordinal);
        Assert.Contains("DemoDiscBottomScreenLightButton", source, StringComparison.Ordinal);
        Assert.Contains("DemoDiscBottomScreenLightButtonLabel", source, StringComparison.Ordinal);
        Assert.Contains("DemoDiscBottomScreenLightSwatch", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateDefaultBottomOverlay(", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// Ensures the handheld light controller responds to both bottom-screen pointer presses and handheld R input.
    /// </summary>
    [Fact]
    public void City_handheld_light_controller_source_responds_to_pointer_and_r_input() {
        string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("BoundInteractable.CursorEvent += HandleCursorEvent;", source, StringComparison.Ordinal);
        Assert.Contains("PointerInteraction.Press", source, StringComparison.Ordinal);
        Assert.Contains("PointerInteraction.Release", source, StringComparison.Ordinal);
        Assert.Contains("InputGamepadButton.RightShoulder", source, StringComparison.Ordinal);
        Assert.Contains("AdvanceLightState();", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// Ensures the handheld light controller uses the same fixed cycle ordering as the desktop light toggle.
    /// </summary>
    [Fact]
    public void City_handheld_light_controller_source_uses_demo_disc_cycle_order() {
        string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("WhiteLightColor", source, StringComparison.Ordinal);
        Assert.Contains("YellowLightColor", source, StringComparison.Ordinal);
        Assert.Contains("RedLightColor", source, StringComparison.Ordinal);
        Assert.Contains("BlueLightColor", source, StringComparison.Ordinal);
        Assert.Contains("GreenLightColor", source, StringComparison.Ordinal);
        Assert.Contains("OffSwatchColor", source, StringComparison.Ordinal);
    }
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_ds_scaffold_source_authors_canonical_bottom_screen_controls|City_handheld_light_controller_source_responds_to_pointer_and_r_input|City_handheld_light_controller_source_uses_demo_disc_cycle_order" -v q`

Expected: FAIL because the scaffold still contains `CreateDefaultBottomOverlay`, the canonical light-button subtree does not exist yet, and `NintendoDsLightToggleOverlayComponent.cs` does not exist yet.

- [ ] **Step 3: Write the failing physics generator source audit**

Extend the same test file with one more test that locks the physics path onto the same canonical scaffold-owned contract.

```csharp
    /// <summary>
    /// Ensures physics companion-scene generation no longer keeps a separate temporary bottom-overlay branch.
    /// </summary>
    [Fact]
    public void City_physics_ds_generator_source_uses_canonical_scaffold_bottom_controls() {
        string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsNintendoDsSceneGenerator.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.DoesNotContain("useDefaultBottomOverlay", source, StringComparison.Ordinal);
        Assert.Contains("WriteNintendoDsCompanionScene(", source, StringComparison.Ordinal);
        Assert.Contains("Array.Empty<Entity>()", source, StringComparison.Ordinal);
    }
```

- [ ] **Step 4: Run the targeted physics source test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_physics_ds_generator_source_uses_canonical_scaffold_bottom_controls" -v q`

Expected: FAIL because `PhysicsNintendoDsSceneGenerator.cs` still contains the `useDefaultBottomOverlay` branch.

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityNintendoDsBottomScreenControlsSourceTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "test: define shared DS bottom-screen controls contract"
```

### Task 2: Implement The Shared Handheld Light Controller

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering\DemoDiscLightToggleComponent.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityNintendoDsBottomScreenControlsSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Add the new handheld light controller component**

Create `NintendoDsLightToggleOverlayComponent.cs` and model its pointer-binding pattern after `NintendoDsReturnOverlayComponent`, but keep the light-cycle responsibilities local to the handheld control.

```csharp
namespace city.rendering {
    /// <summary>
    /// Owns handheld bottom-screen light-cycle behavior from the scaffold-owned light button and R input.
    /// </summary>
    public sealed class NintendoDsLightToggleOverlayComponent : UpdateComponent {
        const int OffLightStateIndex = 5;

        static readonly float4 WhiteLightColor = new float4(1f, 1f, 1f, 1f);
        static readonly float4 YellowLightColor = new float4(1f, 1f, 0f, 1f);
        static readonly float4 RedLightColor = new float4(1f, 0f, 0f, 1f);
        static readonly float4 BlueLightColor = new float4(0f, 0f, 1f, 1f);
        static readonly float4 GreenLightColor = new float4(0f, 1f, 0f, 1f);

        static readonly byte4 WhiteSwatchColor = new byte4(255, 255, 255, 255);
        static readonly byte4 YellowSwatchColor = new byte4(255, 230, 0, 255);
        static readonly byte4 RedSwatchColor = new byte4(255, 0, 0, 255);
        static readonly byte4 BlueSwatchColor = new byte4(0, 120, 255, 255);
        static readonly byte4 GreenSwatchColor = new byte4(0, 220, 80, 255);
        static readonly byte4 OffSwatchColor = new byte4(0, 0, 0, 255);

        readonly List<DemoDiscDirectionalLightToggleState> LightStates;

        InteractableComponent BoundInteractable;
        RoundedRectComponent IndicatorSwatch;
        bool PointerPressStartedInside;
        int CurrentLightStateIndex;

        public NintendoDsLightToggleOverlayComponent() {
            LightStates = new List<DemoDiscDirectionalLightToggleState>();
            CurrentLightStateIndex = 0;
        }

        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            TryBindInteractable();
            CaptureDirectionalLightStates();
            CaptureIndicatorSwatch();
            ApplyCurrentLightState();
        }

        public override void Update() {
            TryBindInteractable();
            if (WasToggleRequestedFromInput()) {
                AdvanceLightState();
            }
        }

        void TryBindInteractable() {
            if (BoundInteractable != null || Parent == null || Parent.Components == null) {
                return;
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is InteractableComponent interactable) {
                    BoundInteractable = interactable;
                    BoundInteractable.CursorEvent += HandleCursorEvent;
                    return;
                }
            }

            throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent requires a sibling InteractableComponent.");
        }

        void HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction) {
            if (interaction == PointerInteraction.Press) {
                PointerPressStartedInside = true;
                return;
            }
            if (interaction == PointerInteraction.Release) {
                bool shouldAdvance = PointerPressStartedInside;
                PointerPressStartedInside = false;
                if (shouldAdvance) {
                    AdvanceLightState();
                }
                return;
            }
            if (interaction == PointerInteraction.Leave) {
                PointerPressStartedInside = false;
            }
        }

        void CaptureIndicatorSwatch() {
            IndicatorSwatch = FindRequiredIndicatorSwatch(Parent);
        }
    }
}
```

- [ ] **Step 2: Reuse the existing demo-disc cycle semantics instead of drifting**

Refactor `DemoDiscLightToggleComponent.cs` only as much as needed so the handheld controller and the desktop controller share one coherent cycle contract. Keep the cycle order, light colors, swatch colors, and `RightShoulder` mapping identical.

```csharp
bool WasToggleRequestedFromInput() {
    InputSystem inputSystem = Core.Instance.Input;
    if (inputSystem == null) {
        throw new InvalidOperationException("Light toggle component requires an initialized input system.");
    }

    return inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.RightShoulder);
}

void AdvanceLightState() {
    CurrentLightStateIndex++;
    if (CurrentLightStateIndex > OffLightStateIndex) {
        CurrentLightStateIndex = 0;
    }

    ApplyCurrentLightState();
}
```

- [ ] **Step 3: Run the targeted source tests to verify they pass**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_handheld_light_controller_source_responds_to_pointer_and_r_input|City_handheld_light_controller_source_uses_demo_disc_cycle_order" -v q`

Expected: PASS

- [ ] **Step 4: Commit**

```bash
rtk git -C C:\dev\helprojs\demodisc add assets/codebase/rendering/NintendoDsLightToggleOverlayComponent.cs assets/codebase/rendering/DemoDiscLightToggleComponent.cs
rtk git -C C:\dev\helprojs\demodisc commit -m "feat: add handheld DS light toggle controller"
```

### Task 3: Replace The Legacy Scaffold Overlay With Canonical Controls

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\DemoDiscSceneComponentRecordFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityNintendoDsBottomScreenControlsSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Remove the temporary overlay branch and force handheld FPS to `1x`**

Update `NintendoDsRenderingSceneScaffoldFactory.cs` so the shared scaffold always builds the canonical control stack, removes `CreateDefaultBottomOverlay(...)`, and forces relocated bottom-screen FPS overlays to `FontScale = 1f`.

```csharp
public Entity[] CreateSceneRoots(Entity[] topScreenRoots, bool useDefaultBottomOverlay, Entity[] bottomScreenRoots, FontAsset bottomOverlayFont) {
    if (topScreenRoots == null) {
        throw new ArgumentNullException(nameof(topScreenRoots));
    } else if (bottomScreenRoots == null) {
        throw new ArgumentNullException(nameof(bottomScreenRoots));
    } else if (bottomOverlayFont == null) {
        throw new ArgumentNullException(nameof(bottomOverlayFont));
    }

    Entity[] filteredTopScreenRoots = FilterTopScreenRoots(topScreenRoots);
    Entity bottomScreenCameraEntity = CreateBottomScreenCameraEntity();
    Entity bottomScreenViewportRoot = Core.Instance.EntityFactory.CreateChild(bottomScreenCameraEntity, "DemoDiscBottomScreenRoot");
    bottomScreenViewportRoot.LayerMask = PersistedSceneLayerMask;
    bottomScreenViewportRoot.AddComponent(new ViewportComponent {
        BindingMode = ViewportComponent.AncestorCameraBindingMode,
        FixedSize = new int2(ScreenWidth, ScreenHeight),
        ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
        ReferenceWidth = ScreenWidth,
        ReferenceHeight = ScreenHeight
    });
    RelocateFpsComponentsToBottomScreen(filteredTopScreenRoots, bottomScreenViewportRoot, bottomOverlayFont, false);
    CreateBottomScreenLightButton(bottomScreenViewportRoot, bottomOverlayFont);
    CreateBottomScreenBackButton(bottomScreenViewportRoot, bottomOverlayFont);
    AttachBottomScreenRoots(bottomScreenViewportRoot, bottomScreenRoots);
    Entity topScreenCameraEntity = ConfigureTopScreenRoots(filteredTopScreenRoots);
    Entity[] adjustedTopScreenRoots = MoveTopScreen2DRootsUnderViewport(filteredTopScreenRoots, topScreenCameraEntity);
    return CombineSceneRoots(adjustedTopScreenRoots, bottomScreenCameraEntity);
}
```

- [ ] **Step 2: Add the scaffold-owned light-button subtree**

Extend `NintendoDsRenderingSceneScaffoldFactory.cs` with one full-width `Light` button plus swatch, following the existing back-button entity creation pattern and using stable entity names.

```csharp
void CreateBottomScreenLightButton(Entity bottomScreenViewportRoot, FontAsset bottomOverlayFont) {
    Entity lightButtonEntity = Core.Instance.EntityFactory.CreateChild(bottomScreenViewportRoot, "DemoDiscBottomScreenLightButton");
    lightButtonEntity.LocalPosition = new float3(NintendoDsBackButtonLeft, NintendoDsLightButtonTop, 0f);
    lightButtonEntity.LayerMask = PersistedSceneLayerMask;
    lightButtonEntity.Static = true;

    SpriteComponent spriteComponent = new SpriteComponent {
        Size = new int2(NintendoDsBackButtonWidth, NintendoDsBackButtonHeight),
        RenderOrder2D = NintendoDsBackButtonSpriteRenderOrder,
        LayerMask = RuntimeLayerMask
    };
    lightButtonEntity.AddComponent(spriteComponent);
    ApplyTextureReference(lightButtonEntity, spriteComponent, NintendoDsBackButtonTexturePath);

    InteractableComponent interactableComponent = new InteractableComponent {
        Size = new int2(NintendoDsBackButtonWidth, NintendoDsBackButtonHeight)
    };
    lightButtonEntity.AddComponent(interactableComponent);
    lightButtonEntity.AddComponent(new NintendoDsLightToggleOverlayComponent());
}
```

- [ ] **Step 3: Add the label and swatch font/reference wiring**

Keep the label and swatch serialization explicit through `DemoDiscSceneComponentRecordFactory` and the existing save-state asset reference helpers.

```csharp
Entity lightButtonLabelEntity = Core.Instance.EntityFactory.CreateChild(lightButtonEntity, "DemoDiscBottomScreenLightButtonLabel");
lightButtonLabelEntity.LocalPosition = new float3(NintendoDsLightButtonLabelLeft, NintendoDsLightButtonLabelTop, 0f);
lightButtonLabelEntity.LayerMask = PersistedSceneLayerMask;
lightButtonLabelEntity.Static = true;

TextComponent labelComponent = new TextComponent {
    Text = "LIGHT",
    Font = bottomOverlayFont,
    FontScale = NintendoDsBottomOverlayFontScale,
    Color = new byte4(255, 255, 255, 255),
    Size = new int2(NintendoDsLightButtonLabelWidth, NintendoDsLightButtonLabelHeight),
    RenderOrder2D = NintendoDsBackButtonLabelRenderOrder,
    LayerMask = RuntimeLayerMask
};
lightButtonLabelEntity.AddComponent(labelComponent);
ApplyFontReference(lightButtonLabelEntity, labelComponent, DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());

Entity lightSwatchEntity = Core.Instance.EntityFactory.CreateChild(lightButtonEntity, "DemoDiscBottomScreenLightSwatch");
lightSwatchEntity.LocalPosition = new float3(NintendoDsLightSwatchLeft, NintendoDsLightSwatchTop, 0.1f);
lightSwatchEntity.LayerMask = PersistedSceneLayerMask;
lightSwatchEntity.Static = true;
lightSwatchEntity.AddComponent(new RoundedRectComponent {
    Size = new int2(NintendoDsLightSwatchSize, NintendoDsLightSwatchSize),
    Radius = 2f,
    BorderThickness = 1f,
    FillColor = new byte4(255, 255, 255, 255),
    BorderColor = new byte4(30, 30, 30, 255),
    RenderOrder2D = NintendoDsBackButtonLabelRenderOrder,
    LayerMask = RuntimeLayerMask
});
```

- [ ] **Step 4: Run the scaffold source test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_ds_scaffold_source_authors_canonical_bottom_screen_controls" -v q`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helprojs\demodisc add assets/codebase/rendering.tools/NintendoDsRenderingSceneScaffoldFactory.cs assets/codebase/rendering.tools/DemoDiscSceneComponentRecordFactory.cs
rtk git -C C:\dev\helprojs\demodisc commit -m "feat: standardize DS bottom-screen controls"
```

### Task 4: Put Physics On The Same Canonical Scaffold Contract

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsNintendoDsSceneGenerator.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityNintendoDsBottomScreenControlsSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Remove the physics-specific temporary bottom-overlay branch**

Delete the `ContainsFpsComponent(...)`-driven `useDefaultBottomOverlay` behavior and always route physics companion scenes through the canonical scaffold-owned bottom controls.

```csharp
LoadedEditorSceneDocument loadedScene = sceneLoadService.Load(authoredScenePath);

try {
    SceneWriteService.WriteNintendoDsCompanionScene(
        fullProjectRootPath,
        BuildNintendoDsSceneAssetId(sceneEntry.NintendoDsSceneId),
        loadedScene.SceneSettings,
        loadedScene.RootEntities,
        false,
        Array.Empty<Entity>());
} finally {
    DisposeRoots(loadedScene.RootEntities);
}
```

- [ ] **Step 2: Delete the obsolete helper methods if they become unused**

Remove the old FPS-presence detection helpers so the physics generator no longer suggests a second handheld UI contract.

```csharp
// Delete:
// static bool ContainsFpsComponent(Entity[] roots)
// static bool ContainsFpsComponent(Entity entity)
```

- [ ] **Step 3: Run the targeted physics source test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "City_physics_ds_generator_source_uses_canonical_scaffold_bottom_controls" -v q`

Expected: PASS

- [ ] **Step 4: Commit**

```bash
rtk git -C C:\dev\helprojs\demodisc add assets/codebase/physics.tools/PhysicsNintendoDsSceneGenerator.cs
rtk git -C C:\dev\helprojs\demodisc commit -m "refactor: share DS bottom controls with physics scenes"
```

### Task 5: Regenerate Representative Scenes And Audit Generated Output

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityNintendo3DsCubeTestPackagedSceneRuntimeTests.cs`
- Generated output: `C:\dev\helprojs\demodisc\assets\scenes\rendering\ds\cube_test_ds.helen`
- Generated output: `C:\dev\helprojs\demodisc\assets\scenes\rendering\ds\colored_cube_grid_ds.helen`
- Generated output: `C:\dev\helprojs\demodisc\assets\scenes\physics\test_scene_dynamic_stack_boxes_ds.helen`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Add one generated-scene audit for the canonical handheld controls**

Extend `CityNintendo3DsCubeTestPackagedSceneRuntimeTests.cs` with a raw-scene audit that verifies representative generated DS scenes contain the scaffold-owned `Light` button, swatch, `Back` button, and bottom-screen `FPSComponent` at `1f`.

```csharp
[Fact]
public void Nintendo3Ds_generated_ds_scenes_include_canonical_bottom_screen_controls() {
    string[] scenePaths = {
        @"C:\dev\helprojs\demodisc\assets\scenes\rendering\ds\cube_test_ds.helen",
        @"C:\dev\helprojs\demodisc\assets\scenes\rendering\ds\colored_cube_grid_ds.helen",
        @"C:\dev\helprojs\demodisc\assets\scenes\physics\test_scene_dynamic_stack_boxes_ds.helen"
    };

    for (int index = 0; index < scenePaths.Length; index++) {
        string sceneText = System.Text.Encoding.Unicode.GetString(File.ReadAllBytes(scenePaths[index]));
        Assert.Contains("DemoDiscBottomScreenLightButton", sceneText, StringComparison.Ordinal);
        Assert.Contains("DemoDiscBottomScreenLightSwatch", sceneText, StringComparison.Ordinal);
        Assert.Contains("DemoDiscBottomScreenBackButton", sceneText, StringComparison.Ordinal);
        Assert.Contains("FontScale", sceneText, StringComparison.Ordinal);
    }
}
```

- [ ] **Step 2: Regenerate the rendering scenes**

Run: `rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-rendering-scenes`

Expected: `Editor command 'menu.generate-rendering-scenes' executed successfully.`

- [ ] **Step 3: Regenerate the physics DS scenes**

Run: `rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-physics-nintendo-ds-scenes`

Expected: `Editor command 'menu.generate-physics-nintendo-ds-scenes' executed successfully.`

- [ ] **Step 4: Run the generated-scene audit to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "Nintendo3Ds_generated_ds_scenes_include_canonical_bottom_screen_controls" -v q`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityNintendo3DsCubeTestPackagedSceneRuntimeTests.cs
rtk git -C C:\dev\helprojs\demodisc add assets/scenes/rendering/ds/cube_test_ds.helen assets/scenes/rendering/ds/colored_cube_grid_ds.helen assets/scenes/physics/test_scene_dynamic_stack_boxes_ds.helen
rtk git -C C:\dev\helworks\helengine commit -m "test: audit generated DS bottom controls"
rtk git -C C:\dev\helprojs\demodisc commit -m "chore: regenerate DS scenes with shared bottom controls"
```

### Task 6: Rebuild And Verify On 3DS

**Files:**
- Build output: `C:\dev\helprojs\demodisc\3ds-build\helengine_3ds.3dsx`
- Runtime verification: `C:\dev\helworks\helengine-3ds\scripts\launch_in_emulator.ps1`

- [ ] **Step 1: Rebuild the 3DS artifact**

Run: `rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\artifacts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform 3ds -Output C:\dev\helprojs\demodisc\3ds-build`

Expected: `Build completed for platform '3ds': C:\dev\helprojs\demodisc\3ds-build`

- [ ] **Step 2: Launch Azahar with the fresh artifact**

Run: `rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine-3ds\scripts\launch_in_emulator.ps1 -ArtifactPath C:\dev\helprojs\demodisc\3ds-build\helengine_3ds.3dsx`

Expected: output reports `ARTIFACT=...helengine_3ds.3dsx`, `EMULATOR=...azahar.exe`, and a new `PROCESS_ID=...`.

- [ ] **Step 3: Verify the runtime contract manually**

Check these scenes in the running 3DS build:

```text
Cube Test
Colored Cube Grid
Dynamic Stack Boxes
```

Expected:

```text
Bottom screen shows FPS at 1x scale.
Bottom screen shows a full-width LIGHT button with swatch.
Bottom screen shows a full-width BACK button.
Touching LIGHT advances the same cycle as desktop.
Pressing R advances the same cycle as desktop.
Swatch color matches the active state.
BACK still returns to the menu.
```

- [ ] **Step 4: Commit**

```bash
rtk git -C C:\dev\helprojs\demodisc add assets/codebase/rendering.tools/NintendoDsRenderingSceneScaffoldFactory.cs assets/codebase/physics.tools/PhysicsNintendoDsSceneGenerator.cs assets/codebase/rendering/NintendoDsLightToggleOverlayComponent.cs assets/scenes/rendering/ds assets/scenes/physics
rtk git -C C:\dev\helprojs\demodisc commit -m "feat: share DS bottom-screen controls across scenes"
```
