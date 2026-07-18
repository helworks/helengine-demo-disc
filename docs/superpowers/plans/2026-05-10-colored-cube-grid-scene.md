# Colored Cube Grid Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a second committed rendering test scene with a `4x4` grid of sixteen rotating cubes, each using a distinct authored material color, while keeping the existing single-cube scene intact and making the new scene the temporary startup/export target.

**Architecture:** Extend the existing generated standard-material path so authored base color survives packaging and reaches the PS2 runtime as lit color instead of grayscale-only intensity. Then add a new `city` scene factory that generates the colored cube grid alongside the existing single-cube scene and update the catalog and startup selection to boot into the new scene directly.

**Tech Stack:** C#, .NET 9, helengine scene generation and packaging, PS2 C++ renderer path, xUnit, headless editor CLI, PCSX2 verification

---

### File Map

**helengine worktree**
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.core/assets/raw/ps2/Ps2MaterialAsset.cs`
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor/managers/project/EditorWindowsBuildScenePackager.cs`
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs`
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor.tests/managers/project/EditorWindowsBuildScenePackagerTests.cs`

**helengine-ps2 worktree**
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/builder/Ps2MaterialCooker.cs`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RuntimeMaterial.hpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RuntimeMaterial.cpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RenderManager3D.hpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RenderManager3D.cpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/builder.tests/Ps2PlatformAssetBuilderTests.cs`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/builder.tests/Ps2NativeBuildInputsTests.cs`

**city main**
- Create: `C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/RenderingSceneGenerator.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/menu/DemoDiscSceneCatalog.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/CubeTestSceneFactory.cs` only if shared helpers are extracted; otherwise leave unchanged
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/RenderingScriptComponentRecordFactory.cs` only if a shared spin-record helper needs extension; otherwise leave unchanged
- Modify: `C:/dev/helprojs/demodisc/user_settings/build_config.json`
- Create/generated: `C:/dev/helprojs/demodisc/assets/scenes/rendering/colored_cube_grid.helen`
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor.tests/CityCubeTestSceneSourceTests.cs`

### Task 1: Add Base Color To The Generated Standard Material Path

**Files:**
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.core/assets/raw/ps2/Ps2MaterialAsset.cs`
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor/managers/project/EditorWindowsBuildScenePackager.cs`
- Modify: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs`
- Test: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor.tests/managers/project/EditorWindowsBuildScenePackagerTests.cs`

- [ ] **Step 1: Write the failing editor packaging regression**

Add one test that proves the generated PS2 standard material can carry an authored base color instead of only schema lighting flags.

```csharp
[Fact]
public void Package_WhenSceneReferencesGeneratedStandardMaterial_CooksPs2MaterialAssetWithBaseColor() {
    string sceneId = "Scenes/GeneratedStandardMaterialScene.helen";
    WriteSceneAsset(sceneId, CreateGeneratedStandardMaterialReference());

    RecordingMaterialBuilder materialBuilder = new RecordingMaterialBuilder(
        CreatePs2MaterialBuilderDefinition(),
        request => new PlatformMaterialCookResult(
            AssetSerializer.SerializeToBytes(new Ps2MaterialAsset {
                RendererFamilyId = "ps2-standard-forward",
                LightingMode = Ps2MaterialLightingMode.SimpleLit,
                AlphaMode = Ps2MaterialAlphaMode.Opaque,
                RenderClass = Ps2RenderClass.Opaque,
                BaseColorR = 255,
                BaseColorG = 64,
                BaseColorB = 64,
                BaseColorA = 255
            }),
            Array.Empty<string>()));

    EditorPlatformBuildScenePackager packager = new EditorPlatformBuildScenePackager(
        ProjectRootPath,
        Array.Empty<IAssetImporterRegistration>(),
        "ps2",
        materialBuilder,
        "debug",
        "ps2-standard-forward");

    packager.Package(new[] { sceneId }, BuildRootPath);

    Assert.NotNull(materialBuilder.LastMaterialCookRequest);
    Assert.Equal("#FF4040FF", materialBuilder.LastMaterialCookRequest.FieldValues["base-color"]);

    using FileStream stream = File.OpenRead(Path.Combine(BuildRootPath, "cooked", "engine", "materials", "standard.hasset"));
    Ps2MaterialAsset cookedMaterial = Assert.IsType<Ps2MaterialAsset>(AssetSerializer.Deserialize(stream));

    Assert.Equal((byte)255, cookedMaterial.BaseColorR);
    Assert.Equal((byte)64, cookedMaterial.BaseColorG);
    Assert.Equal((byte)64, cookedMaterial.BaseColorB);
    Assert.Equal((byte)255, cookedMaterial.BaseColorA);
}
```

- [ ] **Step 2: Run the focused editor test and verify it fails**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~EditorWindowsBuildScenePackagerTests.Package_WhenSceneReferencesGeneratedStandardMaterial_CooksPs2MaterialAssetWithBaseColor
```

Expected: FAIL because the PS2 generated standard-material path does not populate or persist a base-color field yet.

- [ ] **Step 3: Extend the shared PS2 material asset with explicit base color channels**

Add concrete serialized channels to the cooked PS2 material asset.

```csharp
/// <summary>
/// Gets or sets the cooked base-color red channel used by the PS2 runtime lighting path.
/// </summary>
public byte BaseColorR;

/// <summary>
/// Gets or sets the cooked base-color green channel used by the PS2 runtime lighting path.
/// </summary>
public byte BaseColorG;

/// <summary>
/// Gets or sets the cooked base-color blue channel used by the PS2 runtime lighting path.
/// </summary>
public byte BaseColorB;

/// <summary>
/// Gets or sets the cooked base-color alpha channel used by the PS2 runtime lighting path.
/// </summary>
public byte BaseColorA;
```

- [ ] **Step 4: Teach both generated-standard packager paths to seed PS2 base color**

In both `EditorWindowsBuildScenePackager.cs` and `SceneComponentPackagingTransformService.cs`, add one helper that seeds the generated standard material with an explicit default base color for PS2.

```csharp
static void ApplyGeneratedStandardMaterialDefaults(
    MaterialAssetProcessorSettings materialSettings,
    Dictionary<string, string> fieldValues,
    string targetPlatformId) {
    if (materialSettings == null) {
        throw new ArgumentNullException(nameof(materialSettings));
    } else if (fieldValues == null) {
        throw new ArgumentNullException(nameof(fieldValues));
    }

    if (!string.Equals(targetPlatformId, "ps2", StringComparison.OrdinalIgnoreCase)) {
        return;
    }

    fieldValues["base-color"] = "#FF4040FF";
}
```

And call it immediately after `BuildMaterialCookFieldValues(...)`:

```csharp
Dictionary<string, string> standardMaterialFieldValues = BuildMaterialCookFieldValues(new MaterialAsset(), standardMaterialSettings);
ApplyGeneratedStandardMaterialDefaults(standardMaterialSettings, standardMaterialFieldValues, TargetPlatformId);
standardMaterialFieldValues[VariantFieldId] = StandardShaderVariantName;
```

- [ ] **Step 5: Run the focused editor test and verify it passes**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~EditorWindowsBuildScenePackagerTests.Package_WhenSceneReferencesGeneratedStandardMaterial_CooksPs2MaterialAssetWithBaseColor
```

Expected: PASS

- [ ] **Step 6: Commit the helengine material-packaging change**

```bash
git add engine/helengine.core/assets/raw/ps2/Ps2MaterialAsset.cs engine/helengine.editor/managers/project/EditorWindowsBuildScenePackager.cs engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs engine/helengine.editor.tests/managers/project/EditorWindowsBuildScenePackagerTests.cs
git commit -m "feat: carry base color through ps2 standard materials"
```

### Task 2: Apply Base Color In The PS2 Lit Shading Path

**Files:**
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/builder/Ps2MaterialCooker.cs`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RuntimeMaterial.hpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RuntimeMaterial.cpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RenderManager3D.hpp`
- Modify: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/src/platform/ps2/rendering/Ps2RenderManager3D.cpp`
- Test: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/builder.tests/Ps2PlatformAssetBuilderTests.cs`
- Test: `C:/dev/helworks/helengine-ps2/.worktrees/normalize-camera-viewport-core/builder.tests/Ps2NativeBuildInputsTests.cs`

- [ ] **Step 1: Write the failing cooker regression for PS2 base color**

Add one builder test that proves the PS2 cooker parses a material base color into cooked byte channels.

```csharp
[Fact]
public void CookMaterial_WhenPs2MaterialIncludesBaseColor_PersistsCookedChannels() {
    Ps2MaterialCooker cooker = new Ps2MaterialCooker();
    PlatformMaterialCookRequest request = new PlatformMaterialCookRequest(
        "Engine.Materials.Standard.material",
        "generated/engine/materials/standard.helmat",
        "ps2",
        "debug",
        "ps2-standard-forward",
        "ps2-simple-lit-textured",
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            ["alpha-mode"] = "opaque",
            ["double-sided"] = "false",
            ["cast-shadows"] = "true",
            ["vertex-color-mode"] = "ignore",
            ["base-color"] = "#FF4040FF"
        });

    PlatformMaterialCookResult result = cooker.Cook(request);
    Ps2MaterialAsset materialAsset = Assert.IsType<Ps2MaterialAsset>(AssetSerializer.DeserializeFromBytes(result.CookedMaterialBytes));

    Assert.Equal((byte)255, materialAsset.BaseColorR);
    Assert.Equal((byte)64, materialAsset.BaseColorG);
    Assert.Equal((byte)64, materialAsset.BaseColorB);
    Assert.Equal((byte)255, materialAsset.BaseColorA);
}
```

- [ ] **Step 2: Run the focused PS2 builder test and verify it fails**

Run:

```powershell
$env:HELENGINE_ROOT='C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core'
rtk dotnet test C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core\builder.tests\helengine.ps2.builder.tests.csproj -c Debug --filter FullyQualifiedName~Ps2PlatformAssetBuilderTests.CookMaterial_WhenPs2MaterialIncludesBaseColor_PersistsCookedChannels
```

Expected: FAIL because the cooker and runtime material do not carry base color yet.

- [ ] **Step 3: Add base-color parsing to the PS2 cooker and runtime material**

In `Ps2MaterialCooker.cs`, parse a required or defaulted `base-color` field and write byte channels into the cooked asset.

```csharp
Ps2MaterialAsset cookedAsset = new Ps2MaterialAsset {
    Id = request.MaterialAssetId,
    RendererFamilyId = request.SelectedGraphicsProfileId,
    LightingMode = ResolveLightingMode(request.SchemaId),
    AlphaMode = alphaMode,
    RenderClass = ResolveRenderClass(alphaMode),
    BaseColorR = baseColor.R,
    BaseColorG = baseColor.G,
    BaseColorB = baseColor.B,
    BaseColorA = baseColor.A,
    TextureRelativePath = ReadOptionalField(request.FieldValues, Ps2MaterialSchemaIds.TextureRelativePathFieldId),
    ...
};
```

Add the runtime getters and storage in `Ps2RuntimeMaterial.hpp/.cpp`:

```cpp
std::uint8_t GetBaseColorR() const;
std::uint8_t GetBaseColorG() const;
std::uint8_t GetBaseColorB() const;
std::uint8_t GetBaseColorA() const;
```

```cpp
BaseColorR = materialAsset->BaseColorR;
BaseColorG = materialAsset->BaseColorG;
BaseColorB = materialAsset->BaseColorB;
BaseColorA = materialAsset->BaseColorA;
```

- [ ] **Step 4: Multiply lit intensity by the cooked base color**

Update `ResolveVertexColor(...)` in `Ps2RenderManager3D.cpp` so lit materials shade by modulating the authored base color rather than returning grayscale intensity.

```cpp
const std::uint8_t intensity = static_cast<std::uint8_t>(std::clamp(std::lround(intensityValue), 0l, 255l));
const auto applyIntensity = [intensity](std::uint8_t channel) {
    const double litChannel = (static_cast<double>(channel) * static_cast<double>(intensity)) / 255.0;
    return static_cast<std::uint8_t>(std::clamp(std::lround(litChannel), 0l, 255l));
};

return GS_SETREG_RGBAQ(
    applyIntensity(material.GetBaseColorR()),
    applyIntensity(material.GetBaseColorG()),
    applyIntensity(material.GetBaseColorB()),
    material.GetBaseColorA(),
    0x00);
```

- [ ] **Step 5: Add one PS2 source test that proves the lit path uses base color**

In `Ps2NativeBuildInputsTests.cs`, assert the renderer source now reads base-color channels during lighting.

```csharp
[Fact]
public void Ps2_renderer3d_modulates_lighting_by_cooked_base_color() {
    string source = File.ReadAllText(@"C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core\src\platform\ps2\rendering\Ps2RenderManager3D.cpp");

    Assert.Contains("material.GetBaseColorR()", source, StringComparison.Ordinal);
    Assert.Contains("material.GetBaseColorG()", source, StringComparison.Ordinal);
    Assert.Contains("material.GetBaseColorB()", source, StringComparison.Ordinal);
    Assert.Contains("material.GetBaseColorA()", source, StringComparison.Ordinal);
}
```

- [ ] **Step 6: Run the focused PS2 tests and verify they pass**

Run:

```powershell
$env:HELENGINE_ROOT='C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core'
rtk dotnet test C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core\builder.tests\helengine.ps2.builder.tests.csproj -c Debug --filter FullyQualifiedName~Ps2PlatformAssetBuilderTests.CookMaterial_WhenPs2MaterialIncludesBaseColor_PersistsCookedChannels
```

Run:

```powershell
$env:HELENGINE_ROOT='C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core'
rtk dotnet test C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core\builder.tests\helengine.ps2.builder.tests.csproj -c Debug --filter FullyQualifiedName~Ps2NativeBuildInputsTests
```

Expected: PASS

- [ ] **Step 7: Commit the PS2 base-color shading change**

```bash
git add builder/Ps2MaterialCooker.cs src/platform/ps2/rendering/Ps2RuntimeMaterial.hpp src/platform/ps2/rendering/Ps2RuntimeMaterial.cpp src/platform/ps2/rendering/Ps2RenderManager3D.hpp src/platform/ps2/rendering/Ps2RenderManager3D.cpp builder.tests/Ps2PlatformAssetBuilderTests.cs builder.tests/Ps2NativeBuildInputsTests.cs
git commit -m "feat: shade ps2 materials with authored base color"
```

### Task 3: Add The Colored Cube Grid Scene To City

**Files:**
- Create: `C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/rendering.tools/RenderingSceneGenerator.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/menu/DemoDiscSceneCatalog.cs`
- Modify: `C:/dev/helprojs/demodisc/user_settings/build_config.json`
- Test: `C:/dev/helworks/helengine/.worktrees/normalize-camera-viewport-core/engine/helengine.editor.tests/CityCubeTestSceneSourceTests.cs`

- [ ] **Step 1: Write the failing scene-source regression**

Add one source-level test that proves the generator emits both rendering scenes and the new scene contains sixteen cube entities.

```csharp
[Fact]
public void ColoredCubeGridSceneFactory_creates_sixteen_rotating_cubes_with_distinct_colors() {
    string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\ColoredCubeGridSceneFactory.cs");

    Assert.Contains("public const string SceneId = RenderingSceneGenerator.ColoredCubeGridSceneId;", source, StringComparison.Ordinal);
    Assert.Contains("for (int row = 0; row < 4; row++)", source, StringComparison.Ordinal);
    Assert.Contains("for (int column = 0; column < 4; column++)", source, StringComparison.Ordinal);
    Assert.Contains("CreateCubeEntity(", source, StringComparison.Ordinal);
    Assert.Contains("CreateColoredMaterialReference(", source, StringComparison.Ordinal);
}
```

Add one generator/catalog expectation:

```csharp
[Fact]
public void RenderingSceneGenerator_generates_cube_test_and_colored_cube_grid() {
    string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneGenerator.cs");

    Assert.Contains("public const string CubeTestSceneId = \"scenes/rendering/cube_test.helen\";", source, StringComparison.Ordinal);
    Assert.Contains("public const string ColoredCubeGridSceneId = \"scenes/rendering/colored_cube_grid.helen\";", source, StringComparison.Ordinal);
    Assert.Contains("SceneWriteService.WriteScene(projectRootPath, CubeTestSceneId, cubeTestSceneAsset);", source, StringComparison.Ordinal);
    Assert.Contains("SceneWriteService.WriteScene(projectRootPath, ColoredCubeGridSceneId, coloredCubeGridSceneAsset);", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the focused source test and verify it fails**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~CityCubeTestSceneSourceTests
```

Expected: FAIL because the new scene factory and scene id do not exist yet.

- [ ] **Step 3: Create the new scene factory and generate sixteen cubes**

Create `ColoredCubeGridSceneFactory.cs` following the same authored-scene pattern as `CubeTestSceneFactory.cs`.

Use a grid-centered layout and stable ids:

```csharp
public sealed class ColoredCubeGridSceneFactory {
    public const string SceneId = RenderingSceneGenerator.ColoredCubeGridSceneId;

    static readonly float4[] InitialOrientations = new[] {
        float4.Identity,
        CreateYawPitch(-0.35f, 0f),
        CreateYawPitch(0.35f, 0f),
        CreateYawPitch(0.7f, -0.2f),
        CreateYawPitch(-0.7f, -0.2f),
        CreateYawPitch(0f, 0.35f),
        CreateYawPitch(0f, -0.35f),
        CreateYawPitch(1.05f, 0f),
        CreateYawPitch(-1.05f, 0f),
        CreateYawPitch(0.52f, 0.28f),
        CreateYawPitch(-0.52f, 0.28f),
        CreateYawPitch(0.52f, -0.28f),
        CreateYawPitch(-0.52f, -0.28f),
        CreateYawPitch(1.4f, 0.18f),
        CreateYawPitch(-1.4f, 0.18f),
        CreateYawPitch(3.14f, 0f)
    };
```

```csharp
for (int row = 0; row < 4; row++) {
    for (int column = 0; column < 4; column++) {
        int cubeIndex = (row * 4) + column;
        rootEntities.Add(CreateCubeEntity(
            cubeIndex,
            cubeReference,
            CreateColoredMaterialReference(cubeIndex),
            new float3((column - 1.5f) * 2.75f, (1.5f - row) * 2.75f, 0f),
            InitialOrientations[cubeIndex]));
    }
}
```

Each cube should still use the current slow spin component:

```csharp
RenderingScriptComponentRecordFactory.CreateTowerSpinRecord(1, 0f, (float)(Math.PI / 2.0))
```

- [ ] **Step 4: Generate sixteen distinct authored colors**

In the new factory, create a stable palette and one generated/reference path per cube material.

```csharp
static readonly string[] CubeColors = new[] {
    "#FF4040FF", "#FF8040FF", "#FFC040FF", "#FFFF40FF",
    "#C0FF40FF", "#80FF40FF", "#40FF40FF", "#40FF80FF",
    "#40FFC0FF", "#40FFFFFF", "#40C0FFFF", "#4080FFFF",
    "#4040FFFF", "#8040FFFF", "#C040FFFF", "#FF40FFFF"
};
```

Expose the material references through the scene asset:

```csharp
SceneAssetReference[] materialReferences = new SceneAssetReference[16];
for (int cubeIndex = 0; cubeIndex < materialReferences.Length; cubeIndex++) {
    materialReferences[cubeIndex] = CreateColoredMaterialReference(cubeIndex);
}

return new SceneAsset {
    Id = SceneId,
    AssetReferences = new[] { cubeReference, .. materialReferences },
    RootEntities = rootEntities.ToArray()
};
```

- [ ] **Step 5: Update generator, catalog, and startup selection**

In `RenderingSceneGenerator.cs`:

```csharp
public const string ColoredCubeGridSceneId = "scenes/rendering/colored_cube_grid.helen";
```

```csharp
readonly ColoredCubeGridSceneFactory ColoredCubeGridFactory;

public RenderingSceneGenerator() {
    SceneWriteService = new GeneratedSceneWriteService();
    CubeTestFactory = new CubeTestSceneFactory();
    ColoredCubeGridFactory = new ColoredCubeGridSceneFactory();
}
```

```csharp
SceneAsset coloredCubeGridSceneAsset = ColoredCubeGridFactory.CreateSceneAsset(cubeReference);
SceneWriteService.WriteScene(projectRootPath, CubeTestSceneId, cubeTestSceneAsset);
SceneWriteService.WriteScene(projectRootPath, ColoredCubeGridSceneId, coloredCubeGridSceneAsset);
```

In `DemoDiscSceneCatalog.cs` add the new scene item ahead of `Back`:

```csharp
new MenuItemDefinition("scene-colored-cube-grid", "Colored Cube Grid", "Sixteen rotating cubes with distinct lit material colors.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/rendering/colored_cube_grid.helen")),
```

In `build_config.json`, point startup/export to the new scene:

```json
"startupSceneId": "scenes/rendering/colored_cube_grid.helen"
```

- [ ] **Step 6: Run the focused scene-source test and verify it passes**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~CityCubeTestSceneSourceTests
```

Expected: PASS

- [ ] **Step 7: Commit the city scene-generation change**

```bash
git add assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs assets/codebase/rendering.tools/RenderingSceneGenerator.cs assets/codebase/menu/DemoDiscSceneCatalog.cs user_settings/build_config.json C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs
git commit -m "feat: add colored cube grid scene"
```

### Task 4: Regenerate, Export, And Verify The New Scene

**Files:**
- Generated: `C:/dev/helprojs/demodisc/assets/scenes/rendering/colored_cube_grid.helen`
- Verify: `C:/dev/helprojs/output/ps2-colored-cube-grid/game.iso`

- [ ] **Step 1: Regenerate rendering scenes from the updated city generator**

Run:

```powershell
dotnet C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-rendering-scenes
```

Expected: `cube_test.helen` and `colored_cube_grid.helen` are rewritten under `assets/scenes/rendering`.

- [ ] **Step 2: Verify the generated colored-grid scene exists**

Run:

```powershell
Get-ChildItem C:\dev\helprojs\demodisc\assets\scenes\rendering\colored_cube_grid.helen
```

Expected: one file entry for `colored_cube_grid.helen`

- [ ] **Step 3: Export the PS2 build directly into a fresh output folder**

Run:

```powershell
$env:HELENGINE_ROOT='C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core'
dotnet C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\project.heproj --build ps2 --output C:\dev\helprojs\output\ps2-colored-cube-grid
```

Expected:

```text
Build completed for platform 'ps2': C:\dev\helprojs\output\ps2-colored-cube-grid
```

- [ ] **Step 4: Launch PCSX2 with the fresh ISO**

Run:

```powershell
Start-Process -FilePath 'C:\Program Files\PCSX2\pcsx2-qt.exe' -ArgumentList '"C:\dev\helprojs\output\ps2-colored-cube-grid\game.iso"'
```

Expected: the project boots directly into the colored cube grid scene.

- [ ] **Step 5: Verify runtime behavior manually**

Check:

- sixteen cubes visible
- all cubes rotating
- colors are distinct
- directional lighting affects visible faces
- existing single-cube scene still exists in the catalog

- [ ] **Step 6: Commit regenerated scene assets if they are part of the intended project state**

```bash
git add assets/scenes/rendering/colored_cube_grid.helen assets/scenes/rendering/cube_test.helen
git commit -m "chore: regenerate rendering scene assets"
```

### Spec Coverage Self-Check

- Preserves the existing single-cube scene: Task 3 keeps `cube_test.helen` and adds `colored_cube_grid.helen`
- Adds a second committed rendering scene: Task 3
- Uses a `4x4` grid: Task 3
- Sixteen rotating cubes with different orientations: Task 3
- Distinct authored per-cube colors: Tasks 1, 2, and 3
- Makes the new scene the startup/export target: Task 3
- Rebuilds and verifies on PS2: Task 4

No spec gaps remain.
