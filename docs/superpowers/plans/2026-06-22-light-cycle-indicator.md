# Light Cycle Indicator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a live top-left `Light` indicator with a color preview square, and change `L` from binary toggle to a six-state light cycle across the supported desktop and console rendering demo scenes.

**Architecture:** Keep `DemoDiscLightToggleComponent` as the single owner of runtime light state and UI state. Author the new indicator row through one shared rendering-tools helper so the seven supported scene factories stay consistent, then strengthen engine-side source audits to lock the behavior and scene wiring in place.

**Tech Stack:** C#, xUnit, city generated authoring scenes, helengine 2D UI components, Windows player build verification

---

## File Map

### New files

- `C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoDiscLightIndicatorOverlayFactory.cs`
  Creates the top-left screen-bound indicator row, including the `Light` label and the preview square, and stores the required font asset reference on the generated text entity.

- `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityDemoDiscLightIndicatorSourceTests.cs`
  Locks the intended runtime light-cycle behavior and verifies every supported rendering scene factory authors the indicator helper call.

### Modified files

- `C:\dev\helprojs\city\assets\codebase\rendering\DemoDiscLightToggleComponent.cs`
  Replace the boolean on/off model with a fixed six-state cycle, normalize startup to white, cache the preview-square component, and update the square when state changes.

- `C:\dev\helprojs\city\assets\codebase\rendering\DemoDiscDirectionalLightToggleState.cs`
  Keep the per-light restore state explicit and clear for non-off states.

- `C:\dev\helprojs\city\assets\codebase\rendering.tools\AxisTestSceneFactory.cs`
- `C:\dev\helprojs\city\assets\codebase\rendering.tools\AxisTest2SceneFactory.cs`
- `C:\dev\helprojs\city\assets\codebase\rendering.tools\ColoredCubeGridSceneFactory.cs`
- `C:\dev\helprojs\city\assets\codebase\rendering.tools\CubeTestSceneFactory.cs`
- `C:\dev\helprojs\city\assets\codebase\rendering.tools\DirectionalShadowPlazaSceneFactory.cs`
- `C:\dev\helprojs\city\assets\codebase\rendering.tools\ScaledCubeSceneFactory.cs`
- `C:\dev\helprojs\city\assets\codebase\rendering.tools\TexturedCubeGridSceneFactory.cs`
  Call the shared indicator-overlay helper from each scene UI root that already hosts `FPSComponent`, `DemoDiscReturnToMenuComponent`, and `DemoDiscLightToggleComponent`.

- `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`
  Keep the cube-test-specific source checks aligned with the new helper-based UI authoring.

## Constraints And Notes

- There is no existing city-local gameplay test project. The practical automated coverage for this feature is engine-side source auditing in `helengine.editor.tests`, plus Windows build verification after regeneration.
- Do not touch Nintendo DS paths for this feature.
- Do not change the existing bottom-left `Toggle Light` instruction overlay.
- Use the exact supported scene list already wired to `DemoDiscLightToggleComponent`:
  - `AxisTestSceneFactory`
  - `AxisTest2SceneFactory`
  - `ColoredCubeGridSceneFactory`
  - `CubeTestSceneFactory`
  - `DirectionalShadowPlazaSceneFactory`
  - `ScaledCubeSceneFactory`
  - `TexturedCubeGridSceneFactory`

### Task 1: Lock The Behavior In Source Tests

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityDemoDiscLightIndicatorSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Write the failing source-audit tests**

Add one new source-audit file with tests that pin:

- the helper file exists and authors `Light`
- the helper authors a `RoundedRectComponent`
- the runtime component contains the six-state cycle colors and the white startup normalization
- every supported scene factory calls the helper

Use code like:

```csharp
namespace helengine.editor.tests;

/// <summary>
/// Verifies the shared city demo-disc light indicator source stays wired to the intended runtime and scene-authoring shape.
/// </summary>
public sealed class CityDemoDiscLightIndicatorSourceTests {
    /// <summary>
    /// Ensures the shared indicator overlay factory authors the Light label and preview square.
    /// </summary>
    [Fact]
    public void City_light_indicator_overlay_factory_authors_label_and_preview_square() {
        string sourcePath = @"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoDiscLightIndicatorOverlayFactory.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("Text = \"Light\"", source, StringComparison.Ordinal);
        Assert.Contains("new RoundedRectComponent", source, StringComparison.Ordinal);
        Assert.Contains("DemoDiscLightIndicatorSwatch", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// Ensures the demo-disc light toggle uses the requested six-state color cycle and white startup normalization.
    /// </summary>
    [Fact]
    public void City_light_toggle_source_uses_requested_cycle_order() {
        string sourcePath = @"C:\dev\helprojs\city\assets\codebase\rendering\DemoDiscLightToggleComponent.cs";
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("CurrentLightStateIndex = 0", source, StringComparison.Ordinal);
        Assert.Contains("new float4(1f, 1f, 1f, 1f)", source, StringComparison.Ordinal);
        Assert.Contains("new float4(1f, 1f, 0f, 1f)", source, StringComparison.Ordinal);
        Assert.Contains("new float4(1f, 0f, 0f, 1f)", source, StringComparison.Ordinal);
        Assert.Contains("new float4(0f, 0f, 1f, 1f)", source, StringComparison.Ordinal);
        Assert.Contains("new float4(0f, 1f, 0f, 1f)", source, StringComparison.Ordinal);
        Assert.Contains("ApplyCurrentLightState();", source, StringComparison.Ordinal);
    }
}
```

Update the cube-test file with one assertion that the cube-test UI uses the new helper instead of inlining the indicator:

```csharp
Assert.Contains("DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory();", source, StringComparison.Ordinal);
Assert.Contains("lightIndicatorOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont());", source, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityDemoDiscLightIndicatorSourceTests|FullyQualifiedName~City_cube_test_scene_source_uses_one_rotating_cube_with_shared_instruction_ui"
```

Expected:

- `CityDemoDiscLightIndicatorSourceTests` fails because the helper file does not exist yet
- the cube-test source test fails because the factory does not call the helper yet

- [ ] **Step 3: Commit the red test change**

```bash
git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/CityCubeTestSceneSourceTests.cs engine/helengine.editor.tests/CityDemoDiscLightIndicatorSourceTests.cs
git -C C:\dev\helworks\helengine commit -m "test: lock city light indicator source behavior"
```

### Task 2: Add The Shared Top-Left Indicator Authoring Helper

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoDiscLightIndicatorOverlayFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\AxisTestSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\AxisTest2SceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\ColoredCubeGridSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\CubeTestSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\DirectionalShadowPlazaSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\ScaledCubeSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\TexturedCubeGridSceneFactory.cs`

- [ ] **Step 1: Create the helper with stable entity names and font-reference persistence**

Write the new helper file with one public method that attaches a screen-bound viewport root under the supplied scene UI entity, then adds:

- `DemoDiscLightIndicatorLabel`
- `DemoDiscLightIndicatorSwatch`

Use code like:

```csharp
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Authors the shared top-left light-state indicator used by desktop and console demo-disc rendering scenes.
    /// </summary>
    public sealed class DemoDiscLightIndicatorOverlayFactory {
        public const string IndicatorViewportEntityName = "DemoDiscLightIndicatorViewport";
        public const string IndicatorLabelEntityName = "DemoDiscLightIndicatorLabel";
        public const string IndicatorSwatchEntityName = "DemoDiscLightIndicatorSwatch";

        const ushort OverlayLayerMask = EditorLayerMasks.SceneObjects;
        const byte OverlayDrawableLayerMask = 0b00000001;
        const int ViewportWidth = 1280;
        const int ViewportHeight = 720;
        const float LabelLeft = 8f;
        const float LabelTop = 58f;
        const float SwatchLeft = 78f;
        const float SwatchTop = 60f;

        /// <summary>
        /// Attaches the shared top-left light indicator beneath the supplied scene UI root.
        /// </summary>
        public void AttachToSceneUi(Entity sceneUiEntity, FontAsset font) {
            if (sceneUiEntity == null) {
                throw new ArgumentNullException(nameof(sceneUiEntity));
            } else if (font == null) {
                throw new ArgumentNullException(nameof(font));
            }

            Entity viewportEntity = Core.Instance.EntityFactory.CreateChild(sceneUiEntity, IndicatorViewportEntityName);
            viewportEntity.LayerMask = OverlayLayerMask;
            viewportEntity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(ViewportWidth, ViewportHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = ViewportWidth,
                ReferenceHeight = ViewportHeight
            });

            Entity labelEntity = Core.Instance.EntityFactory.CreateChild(viewportEntity, IndicatorLabelEntityName);
            labelEntity.LocalPosition = new float3(LabelLeft, LabelTop, 0f);
            labelEntity.LayerMask = OverlayLayerMask;
            TextComponent labelText = new TextComponent {
                Text = "Light",
                Font = font,
                FontScale = 1.5f,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(72, 22),
                RenderOrder2D = 252,
                LayerMask = OverlayDrawableLayerMask
            };
            labelEntity.AddComponent(labelText);
            ApplyEditorFontReference(labelEntity, labelText);

            Entity swatchEntity = Core.Instance.EntityFactory.CreateChild(viewportEntity, IndicatorSwatchEntityName);
            swatchEntity.LocalPosition = new float3(SwatchLeft, SwatchTop, 0f);
            swatchEntity.LayerMask = OverlayLayerMask;
            swatchEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(16, 16),
                Radius = 2f,
                BorderThickness = 1f,
                FillColor = new byte4(255, 255, 255, 255),
                BorderColor = new byte4(180, 190, 200, 255),
                RenderOrder2D = 252,
                LayerMask = OverlayDrawableLayerMask
            });
        }
    }
}
```

- [ ] **Step 2: Update every supported scene UI factory to attach the helper**

In each `CreateUiEntity()` method, instantiate the helper once and attach it after the FPS component is added:

```csharp
Entity CreateUiEntity() {
    Entity entity = Core.Instance.EntityFactory.Create("CubeTestUi");
    entity.LayerMask = EditorLayerMasks.SceneObjects;
    entity.LocalPosition = float3.Zero;
    entity.LocalScale = float3.One;
    entity.LocalOrientation = float4.Identity;

    entity.AddComponent(new FPSComponent {
        Font = ResolveRequiredEditorFont(),
        FontScale = 2f
    });

    DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory();
    lightIndicatorOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont());

    entity.AddComponent(new DemoDiscReturnToMenuComponent());
    entity.AddComponent(new DemoDiscLightToggleComponent());
    return entity;
}
```

Apply the same pattern to:

- `AxisTestSceneFactory`
- `AxisTest2SceneFactory`
- `ColoredCubeGridSceneFactory`
- `CubeTestSceneFactory`
- `DirectionalShadowPlazaSceneFactory`
- `ScaledCubeSceneFactory`
- `TexturedCubeGridSceneFactory`

- [ ] **Step 3: Run the source-audit tests to verify the helper wiring passes**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityDemoDiscLightIndicatorSourceTests|FullyQualifiedName~CityCubeTestSceneSourceTests"
```

Expected:

- helper-authoring tests now pass
- cycle-state tests still fail because the runtime component is still boolean

- [ ] **Step 4: Commit the helper and scene-wiring change**

```bash
git -C C:\dev\helprojs\city add assets/codebase/rendering.tools/DemoDiscLightIndicatorOverlayFactory.cs assets/codebase/rendering.tools/AxisTestSceneFactory.cs assets/codebase/rendering.tools/AxisTest2SceneFactory.cs assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs assets/codebase/rendering.tools/CubeTestSceneFactory.cs assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs assets/codebase/rendering.tools/ScaledCubeSceneFactory.cs assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs
git -C C:\dev\helprojs\city commit -m "feat: author demo-disc light indicator overlay"
```

### Task 3: Replace The Boolean Toggle With The Requested Six-State Cycle

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering\DemoDiscLightToggleComponent.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering\DemoDiscDirectionalLightToggleState.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Extend the per-light restore state to stay explicit**

Tighten the state class comments so the stored values clearly represent the restore values for non-off states:

```csharp
namespace city.rendering {
    /// <summary>
    /// Stores one controlled directional-light restore state used by the demo-disc light cycle.
    /// </summary>
    public sealed class DemoDiscDirectionalLightToggleState {
        /// <summary>
        /// Gets or sets the controlled light instance.
        /// </summary>
        public DirectionalLightComponent Light { get; set; }

        /// <summary>
        /// Gets or sets the authored intensity restored for every non-off light state.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets whether shadows should be restored for every non-off light state.
        /// </summary>
        public bool ShadowsEnabled { get; set; }
    }
}
```

- [ ] **Step 2: Replace the boolean runtime model with a cycle index, fixed colors, and swatch updates**

Update `DemoDiscLightToggleComponent` to:

- remove `LightsEnabled`
- add `CurrentLightStateIndex`
- cache the preview `RoundedRectComponent`
- apply the canonical white startup state from `ComponentInitialized`
- cycle on `L`

Use code like:

```csharp
namespace city.rendering {
    public sealed class DemoDiscLightToggleComponent : UpdateComponent {
        static readonly float4 WhiteLightColor = new float4(1f, 1f, 1f, 1f);
        static readonly float4 YellowLightColor = new float4(1f, 1f, 0f, 1f);
        static readonly float4 RedLightColor = new float4(1f, 0f, 0f, 1f);
        static readonly float4 BlueLightColor = new float4(0f, 0f, 1f, 1f);
        static readonly float4 GreenLightColor = new float4(0f, 1f, 0f, 1f);

        static readonly byte4 WhiteIndicatorColor = new byte4(255, 255, 255, 255);
        static readonly byte4 YellowIndicatorColor = new byte4(255, 255, 0, 255);
        static readonly byte4 RedIndicatorColor = new byte4(255, 0, 0, 255);
        static readonly byte4 BlueIndicatorColor = new byte4(0, 0, 255, 255);
        static readonly byte4 GreenIndicatorColor = new byte4(0, 255, 0, 255);
        static readonly byte4 OffIndicatorColor = new byte4(36, 36, 36, 255);

        readonly List<DemoDiscDirectionalLightToggleState> LightStates;
        RoundedRectComponent IndicatorSwatch;
        int CurrentLightStateIndex;

        public DemoDiscLightToggleComponent() {
            LightStates = new List<DemoDiscDirectionalLightToggleState>();
            CurrentLightStateIndex = 0;
        }

        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            CaptureDirectionalLightStates();
            CaptureIndicatorComponents();
            ApplyCurrentLightState();
        }

        public override void Update() {
            if (!WasToggleRequested()) {
                return;
            }

            CurrentLightStateIndex = (CurrentLightStateIndex + 1) % 6;
            ApplyCurrentLightState();
        }
    }
}
```

Implement the helper methods with one `switch`:

```csharp
void ApplyCurrentLightState() {
    for (int lightIndex = 0; lightIndex < LightStates.Count; lightIndex++) {
        DemoDiscDirectionalLightToggleState lightState = LightStates[lightIndex];
        DirectionalLightComponent directionalLightComponent = lightState.Light;
        if (CurrentLightStateIndex == 5) {
            directionalLightComponent.Intensity = 0f;
            directionalLightComponent.ShadowsEnabled = false;
            continue;
        }

        directionalLightComponent.Color = ResolveActiveLightColor();
        directionalLightComponent.Intensity = lightState.Intensity;
        directionalLightComponent.ShadowsEnabled = lightState.ShadowsEnabled;
    }

    if (IndicatorSwatch != null) {
        IndicatorSwatch.FillColor = ResolveIndicatorColor();
    }
}

float4 ResolveActiveLightColor() {
    return CurrentLightStateIndex switch {
        0 => WhiteLightColor,
        1 => YellowLightColor,
        2 => RedLightColor,
        3 => BlueLightColor,
        4 => GreenLightColor,
        _ => WhiteLightColor
    };
}
```

Also add one lookup method that fails fast if the swatch is missing:

```csharp
void CaptureIndicatorComponents() {
    IndicatorSwatch = FindRequiredIndicatorSwatch();
}
```

- [ ] **Step 3: Run the targeted tests to verify the cycle behavior audits pass**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityDemoDiscLightIndicatorSourceTests|FullyQualifiedName~CityCubeTestSceneSourceTests"
```

Expected:

- all targeted source tests pass

- [ ] **Step 4: Commit the runtime light-cycle change**

```bash
git -C C:\dev\helprojs\city add assets/codebase/rendering/DemoDiscLightToggleComponent.cs assets/codebase/rendering/DemoDiscDirectionalLightToggleState.cs
git -C C:\dev\helprojs\city commit -m "feat: add demo-disc light color cycle"
```

### Task 4: Rebuild, Verify In Windows, And Commit The Regenerated Assets

**Files:**
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\axis_test.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\axis_test2.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\colored_cube_grid.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\cube_test.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\directional_shadow_plaza.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\scaled_cube.helen`
- Modify: `C:\dev\helprojs\city\assets\scenes\rendering\textured_cube_grid.helen`
- Modify: generated/cooked output under `C:\dev\helprojs\city\output\windows`

- [ ] **Step 1: Regenerate the rendering scenes so the authored assets capture the new UI row**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-rendering-scenes
```

Expected:

- `Editor command 'menu.generate-rendering-scenes' executed successfully.`

- [ ] **Step 2: Rebuild the Windows output**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --build windows --output C:\dev\helprojs\city\output\windows
```

Expected:

- `Build completed for platform 'windows': C:\dev\helprojs\city\output\windows`

- [ ] **Step 3: Launch the build and manually verify the cycle**

Run:

```bash
rtk powershell -NoProfile -Command "Start-Process -FilePath 'C:\dev\helprojs\city\output\windows\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\city\output\windows'"
```

Manual verification checklist:

- `cube_test` shows `Light` below the FPS overlay
- the preview square starts white
- the bottom-left `Toggle Light` instruction row is unchanged
- repeated `L` presses cycle:
  - white
  - yellow
  - red
  - blue
  - green
  - off
- `off` visibly darkens the scene and shows a dark preview square

- [ ] **Step 4: Commit the authored scene regeneration and final feature**

```bash
git -C C:\dev\helprojs\city add assets/codebase/rendering.tools/DemoDiscLightIndicatorOverlayFactory.cs assets/codebase/rendering/DemoDiscLightToggleComponent.cs assets/codebase/rendering/DemoDiscDirectionalLightToggleState.cs assets/codebase/rendering.tools/AxisTestSceneFactory.cs assets/codebase/rendering.tools/AxisTest2SceneFactory.cs assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs assets/codebase/rendering.tools/CubeTestSceneFactory.cs assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs assets/codebase/rendering.tools/ScaledCubeSceneFactory.cs assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs assets/scenes/rendering/axis_test.helen assets/scenes/rendering/axis_test2.helen assets/scenes/rendering/colored_cube_grid.helen assets/scenes/rendering/cube_test.helen assets/scenes/rendering/directional_shadow_plaza.helen assets/scenes/rendering/scaled_cube.helen assets/scenes/rendering/textured_cube_grid.helen
git -C C:\dev\helprojs\city commit -m "feat: add demo-disc light cycle indicator"
```

## Self-Review

- Spec coverage: the plan covers the top-left UI row, the exact six-state light cycle, white startup normalization, leaving the bottom-left instruction panel unchanged, automated source audits, and Windows build verification.
- Placeholder scan: no `TBD`, `TODO`, or vague “implement later” wording remains.
- Type consistency: the plan consistently uses `DemoDiscLightIndicatorOverlayFactory`, `IndicatorSwatch`, `CurrentLightStateIndex`, `ApplyCurrentLightState()`, and the same seven rendering scene factories throughout.
