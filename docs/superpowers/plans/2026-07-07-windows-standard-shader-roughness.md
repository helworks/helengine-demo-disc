# Windows Standard Shader Roughness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add scalar-plus-texture roughness support to the built-in Windows `standard-shader`, switch Tilt Trial to a marble ball material, and wire the user-provided marble albedo and roughness images into the authored city content pipeline.

**Architecture:** Extend the existing Windows `standard-shader` contract instead of introducing a parallel material family. The engine work is split into schema/builder updates, shader-material serialization and hydration updates, generic runtime texture binding updates, and a roughness-aware shader path that preserves the current forward-light and shadow model. The city work stays isolated to one new marble material factory, two imported source textures, and the existing Tilt Trial asset-preparation and scene-generation references.

**Tech Stack:** C#/.NET 9, xUnit, DirectX11 shader HLSL, helengine material serialization, helengine-windows builder metadata, city generated authored assets.

---

## File Structure

### Engine and Builder Files

- Modify: `C:\dev\helworks\helengine-windows\builder\WindowsPlatformDefinitionFactory.cs`
  - Add Windows `standard-shader` schema fields for `roughness` and `roughness-texture-id`.
- Modify: `C:\dev\helworks\helengine-windows\builder\WindowsPlatformAssetBuilder.cs`
  - Read authored roughness fields, write roughness constant-buffer data, and carry the roughness texture asset id into cooked `ShaderMaterialAsset` payloads.
- Modify: `C:\dev\helworks\helengine-windows\builder.tests\WindowsPlatformAssetBuilderTests.cs`
  - Cover schema field exposure and cooked material preservation of roughness data.
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\assets\raw\material\ShaderMaterialAsset.cs`
  - Add one optional roughness texture asset id to the shader-owned material payload.
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\content\ShaderMaterialAssetBinarySerializer.cs`
  - Round-trip the new roughness texture asset id.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\BinarySerializationTests.cs`
  - Assert the new roughness texture asset id survives material serialization.
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\StandardMaterialRoughnessDefaults.cs`
  - Centralize the standard-shader roughness buffer contract and buffer packing.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\MaterialAssetSettingsService.cs`
  - Hydrate roughness scalar constant-buffer data and mirror `roughness-texture-id` onto runtime-facing shader materials.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\serialization\scene\EditorSceneAssetReferenceResolver.cs`
  - Seed preview runtime materials with a default roughness buffer so editor preview builds remain valid.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\MaterialAssetSettingsServiceTests.cs`
  - Cover Windows roughness field hydration.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\EditorSceneAssetReferenceResolverTests.cs`
  - Cover preview roughness buffer hydration.
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\assets\material\StandardMaterialTextureBindingDefaults.cs`
  - Add a named standard roughness texture binding and keep safe defaults.
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\assets\ShaderRuntimeMaterialLoader.cs`
  - Load both diffuse and roughness imported textures into shader runtime materials.
- Modify: `C:\dev\helworks\helengine\engine\helengine.directx11\DirectX11Renderer3D.cs`
  - Bind all material texture slots instead of only the first one.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\shaders\builtin\ForwardStandardShader.hlsl`
  - Add roughness buffer and texture bindings, then replace the hardcoded specular shaping with roughness-aware logic.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\ShaderRuntimeMaterialLoaderTests.cs`
  - Cover packaged roughness texture rebinding.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\rendering\DirectX11MaterialFeatureBindingTests.cs`
  - Cover preservation of both standard texture bindings on the runtime material layout.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\shaders\ForwardStandardShaderTests.cs`
  - Cover the new standard-material bindings and roughness shader source contract.

### City Files

- Create: `C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarble.jpg`
  - Project-owned marble albedo copied from `C:\Users\Helena\Downloads\WhatsApp Image 2026-07-06 at 17.59.09.jpeg`.
- Create: `C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarbleRoughness.jpg`
  - Project-owned roughness source copied from `C:\Users\Helena\Downloads\WhatsApp Image 2026-07-06 at 17.59.58.jpeg`.
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\TiltTrialPlayerSphereWalnutMaterialFactory.cs`
  - No new behavior beyond staying as the reference pattern; only touch if shared helpers are extracted.
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\TiltTrialPlayerSphereMarbleMaterialFactory.cs`
  - Author the marble material, resolve both imported texture asset ids, and emit Windows roughness fields.
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneGenerator.cs`
  - Generate the marble material asset before preparing runtime assets.
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingSceneGenerationAssets.cs`
  - Rename the Tilt Trial player sphere material slot from walnut-specific to marble-specific.
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingSceneAssetPreparationService.cs`
  - Load the authored marble material instead of the walnut material.
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs`
  - Switch the player sphere material asset id/path/runtime material reference from walnut to marble.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityGameSceneSourceTests.cs`
  - Assert Tilt Trial now references the marble material path and generator.
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityTiltTrialMarbleMaterialTests.cs`
  - Assert the authored marble material contains the expected Windows roughness fields and imported texture references.

---

### Task 1: Add Windows Builder Roughness Schema Coverage

**Files:**
- Modify: `C:\dev\helworks\helengine-windows\builder.tests\WindowsPlatformAssetBuilderTests.cs`
- Modify: `C:\dev\helworks\helengine-windows\builder\WindowsPlatformDefinitionFactory.cs`

- [ ] **Step 1: Write the failing builder schema test**

```csharp
[Fact]
public void Descriptor_and_definition_expose_standard_material_roughness_fields() {
    WindowsPlatformAssetBuilder builder = new();

    PlatformMaterialSchemaDefinition schema = Assert.Single(
        builder.Definition.MaterialSchemas,
        materialSchema => materialSchema.SchemaId == "standard-shader");

    Assert.Contains(schema.Fields, field =>
        field.FieldId == "roughness" &&
        field.FieldKind == PlatformMaterialFieldKind.Text &&
        field.DefaultValue == "1.0");
    Assert.Contains(schema.Fields, field =>
        field.FieldId == "roughness-texture-id" &&
        field.FieldKind == PlatformMaterialFieldKind.AssetReference &&
        field.DefaultValue == string.Empty);
}
```

- [ ] **Step 2: Run the focused builder test and verify it fails**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj' --filter 'Descriptor_and_definition_expose_standard_material_roughness_fields'"`

Expected: FAIL because the schema does not yet publish `roughness` or `roughness-texture-id`.

- [ ] **Step 3: Add the Windows schema fields**

```csharp
new PlatformMaterialFieldDefinition(
    "roughness",
    "Roughness",
    PlatformMaterialFieldKind.Text,
    "1.0",
    false,
    []),
new PlatformMaterialFieldDefinition(
    "roughness-texture-id",
    "Roughness Texture",
    PlatformMaterialFieldKind.AssetReference,
    string.Empty,
    false,
    [])
```

- [ ] **Step 4: Re-run the focused builder test and verify it passes**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj' --filter 'Descriptor_and_definition_expose_standard_material_roughness_fields'"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git -C C:\dev\helworks\helengine-windows add builder\WindowsPlatformDefinitionFactory.cs builder.tests\WindowsPlatformAssetBuilderTests.cs
git -C C:\dev\helworks\helengine-windows commit -m "feat: add windows standard shader roughness fields"
```

### Task 2: Cook Roughness Data into Shader Material Assets

**Files:**
- Modify: `C:\dev\helworks\helengine-windows\builder.tests\WindowsPlatformAssetBuilderTests.cs`
- Modify: `C:\dev\helworks\helengine-windows\builder\WindowsPlatformAssetBuilder.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\StandardMaterialRoughnessDefaults.cs`

- [ ] **Step 1: Write the failing builder cook test**

```csharp
[Fact]
public void CookMaterial_preserves_standard_shader_roughness_fields() {
    WindowsPlatformAssetBuilder builder = new();

    PlatformMaterialCookResult result = builder.CookMaterial(new PlatformMaterialCookRequest(
        "Materials/Test.helmat",
        "Materials/Test.helmat",
        "windows",
        "debug",
        "directx11",
        "standard-shader",
        new Dictionary<string, string> {
            ["use-custom-shader"] = "false",
            ["shader-asset-id"] = "ForwardStandardShader",
            ["vertex-program"] = "ForwardStandardShader.vs",
            ["pixel-program"] = "ForwardStandardShader.ps",
            ["variant"] = "Mesh",
            ["roughness"] = "0.35",
            ["roughness-texture-id"] = "Textures/MarbleRoughness"
        }));

    ShaderMaterialAsset materialAsset = Assert.IsType<ShaderMaterialAsset>(
        global::helengine.files.AssetSerializer.DeserializeFromBytes(result.CookedMaterialBytes));
    MaterialConstantBufferAsset roughnessBuffer = Assert.Single(
        materialAsset.ConstantBuffers,
        buffer => buffer.Name == StandardMaterialRoughnessDefaults.RoughnessBufferName);

    Assert.Equal("Textures/MarbleRoughness", materialAsset.RoughnessTextureAssetId);
    Assert.Equal(StandardMaterialRoughnessDefaults.CreateConstantBufferData(0.35f), roughnessBuffer.Data);
}
```

- [ ] **Step 2: Run the focused builder cook test and verify it fails**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj' --filter 'CookMaterial_preserves_standard_shader_roughness_fields'"`

Expected: FAIL because cooked materials do not yet include roughness texture data or a roughness constant buffer.

- [ ] **Step 3: Add the shared roughness constant-buffer helper**

```csharp
namespace helengine.editor {
    public static class StandardMaterialRoughnessDefaults {
        public const string RoughnessBufferName = "RoughnessBuffer";

        public static byte[] CreateConstantBufferData(float roughness) {
            float normalized = Math.Clamp(roughness, 0f, 1f);
            return StandardMaterialBaseColorDefaults.CreateConstantBufferData(
                new float4(normalized, normalized, normalized, normalized));
        }
    }
}
```

- [ ] **Step 4: Update the Windows builder to cook roughness**

```csharp
const string RoughnessFieldId = "roughness";
const string RoughnessTextureFieldId = "roughness-texture-id";

string roughnessTextureAssetId =
    request.FieldValues.TryGetValue(RoughnessTextureFieldId, out string authoredRoughnessTextureAssetId) &&
    !string.IsNullOrWhiteSpace(authoredRoughnessTextureAssetId)
        ? authoredRoughnessTextureAssetId
        : string.Empty;
float roughness = ReadOptionalFloatField(request.FieldValues, RoughnessFieldId, 1.0f);

ShaderMaterialAsset materialAsset = new ShaderMaterialAsset {
    // existing fields...
    RoughnessTextureAssetId = roughnessTextureAssetId,
    ConstantBuffers = [
        new MaterialConstantBufferAsset {
            Name = BaseColorBufferName,
            Data = CreateFloat4ConstantBufferData(ParseBaseColor(baseColor))
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialRoughnessDefaults.RoughnessBufferName,
            Data = StandardMaterialRoughnessDefaults.CreateConstantBufferData(roughness)
        }
    ]
};
```

- [ ] **Step 5: Re-run the focused builder cook test and verify it passes**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj' --filter 'CookMaterial_preserves_standard_shader_roughness_fields'"`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git -C C:\dev\helworks\helengine-windows add builder\WindowsPlatformAssetBuilder.cs builder.tests\WindowsPlatformAssetBuilderTests.cs
git -C C:\dev\helworks\helengine add engine\helengine.editor\managers\asset\StandardMaterialRoughnessDefaults.cs
git -C C:\dev\helworks\helengine commit -m "feat: add standard material roughness buffer defaults"
git -C C:\dev\helworks\helengine-windows commit -m "feat: cook windows standard shader roughness data"
```

### Task 3: Add Roughness Texture Serialization to ShaderMaterialAsset

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\BinarySerializationTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\assets\raw\material\ShaderMaterialAsset.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\content\ShaderMaterialAssetBinarySerializer.cs`

- [ ] **Step 1: Write the failing serializer assertion**

```csharp
Assert.Equal(asset.RoughnessTextureAssetId, deserialized.RoughnessTextureAssetId);
```

And extend the representative material asset:

```csharp
RoughnessTextureAssetId = "textures/roughness",
```

- [ ] **Step 2: Run the serializer test and verify it fails**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'AssetSerializer_MaterialAsset_RoundTripsValues'"`

Expected: FAIL because `ShaderMaterialAsset` does not yet expose or serialize `RoughnessTextureAssetId`.

- [ ] **Step 3: Add the new asset property**

```csharp
public string RoughnessTextureAssetId;
```

and initialize it in the constructor:

```csharp
RoughnessTextureAssetId = string.Empty;
```

- [ ] **Step 4: Serialize and deserialize the new field**

```csharp
writer.WriteString(asset.RoughnessTextureAssetId ?? string.Empty);
```

and:

```csharp
asset.RoughnessTextureAssetId = reader.ReadString();
```

Place the field directly after `DiffuseTextureAssetId` so texture fields stay grouped.

- [ ] **Step 5: Re-run the serializer test and verify it passes**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'AssetSerializer_MaterialAsset_RoundTripsValues'"`

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git -C C:\dev\helworks\helengine add engine\helengine.shader\assets\raw\material\ShaderMaterialAsset.cs engine\helengine.shader\content\ShaderMaterialAssetBinarySerializer.cs engine\helengine.editor.tests\BinarySerializationTests.cs
git -C C:\dev\helworks\helengine commit -m "feat: serialize shader material roughness textures"
```

### Task 4: Hydrate Roughness in Editor Material Loading and Preview Paths

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\MaterialAssetSettingsServiceTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\EditorSceneAssetReferenceResolverTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\MaterialAssetSettingsService.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\serialization\scene\EditorSceneAssetReferenceResolver.cs`

- [ ] **Step 1: Write the failing material-settings hydration test**

```csharp
[Fact]
public void LoadMaterialAsset_WhenWindowsStandardShaderFieldsSpecifyRoughnessAndTexture_HydratesRoughnessData() {
    string tempDirectoryPath = Path.Combine(Path.GetTempPath(), "helengine-material-settings-tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempDirectoryPath);

    try {
        string materialAssetPath = Path.Combine(tempDirectoryPath, "RoughPanel.hasset");
        MaterialAssetSettingsService service = new MaterialAssetSettingsService();
        MaterialAssetImportSettings settings = CreateSharedTextureSettings("imported-texture-id");
        settings.Processor.Platforms["windows"].FieldValues["roughness"] = "0.35";
        settings.Processor.Platforms["windows"].FieldValues["roughness-texture-id"] = "roughness-texture-id";

        service.Save(materialAssetPath, settings);

        ShaderMaterialAsset materialAsset = service.LoadMaterialAsset(materialAssetPath, "windows");
        MaterialConstantBufferAsset roughnessBuffer = Assert.Single(
            materialAsset.ConstantBuffers,
            buffer => buffer.Name == StandardMaterialRoughnessDefaults.RoughnessBufferName);

        Assert.Equal("roughness-texture-id", materialAsset.RoughnessTextureAssetId);
        Assert.Equal(StandardMaterialRoughnessDefaults.CreateConstantBufferData(0.35f), roughnessBuffer.Data);
    } finally {
        Directory.Delete(tempDirectoryPath, true);
    }
}
```

- [ ] **Step 2: Write the failing preview resolver test**

```csharp
MaterialConstantBufferAsset roughnessBuffer = Assert.Single(
    builtMaterialAsset.ConstantBuffers,
    buffer => buffer.Name == StandardMaterialRoughnessDefaults.RoughnessBufferName);
Assert.Equal(StandardMaterialRoughnessDefaults.CreateConstantBufferData(1.0f), roughnessBuffer.Data);
```

Add that assertion to `ResolveMaterial_WhenFileSystemMaterialHasStandardShaderBaseColorSettings_AppliesBaseColorBuffer`.

- [ ] **Step 3: Run both focused tests and verify they fail**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'LoadMaterialAsset_WhenWindowsStandardShaderFieldsSpecifyRoughnessAndTexture_HydratesRoughnessData|ResolveMaterial_WhenFileSystemMaterialHasStandardShaderBaseColorSettings_AppliesBaseColorBuffer'"`

Expected: FAIL because the loader and preview resolver do not yet create roughness data.

- [ ] **Step 4: Hydrate roughness in MaterialAssetSettingsService**

```csharp
const string RoughnessFieldId = "roughness";
const string RoughnessTextureAssetIdFieldId = "roughness-texture-id";

changed |= ApplyMirroredField(
    platformSettings.FieldValues,
    RoughnessTextureAssetIdFieldId,
    shaderMaterialAsset.RoughnessTextureAssetId,
    value => shaderMaterialAsset.RoughnessTextureAssetId = value,
    true);

byte[] roughnessData = ResolveStandardShaderRoughnessBufferData(fieldValues);
changed |= UpsertConstantBuffer(
    shaderMaterialAsset,
    StandardMaterialRoughnessDefaults.RoughnessBufferName,
    roughnessData);
```

and:

```csharp
byte[] ResolveStandardShaderRoughnessBufferData(Dictionary<string, string> fieldValues) {
    if (!fieldValues.TryGetValue(RoughnessFieldId, out string serializedRoughness) ||
        string.IsNullOrWhiteSpace(serializedRoughness)) {
        return StandardMaterialRoughnessDefaults.CreateConstantBufferData(1.0f);
    }

    if (!float.TryParse(serializedRoughness, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedRoughness)) {
        throw new InvalidOperationException("Standard material roughness must be a floating-point value.");
    }

    return StandardMaterialRoughnessDefaults.CreateConstantBufferData(parsedRoughness);
}
```

- [ ] **Step 5: Seed preview materials with a default roughness buffer**

```csharp
ConstantBuffers = new[] {
    new MaterialConstantBufferAsset {
        Name = StandardMaterialBaseColorDefaults.BaseColorBufferName,
        Data = StandardMaterialBaseColorDefaults.CreateConstantBufferData(ResolvePreviewBaseColor(platformSettings))
    },
    new MaterialConstantBufferAsset {
        Name = StandardMaterialRoughnessDefaults.RoughnessBufferName,
        Data = StandardMaterialRoughnessDefaults.CreateConstantBufferData(1.0f)
    }
},
```

- [ ] **Step 6: Re-run the focused tests and verify they pass**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'LoadMaterialAsset_WhenWindowsStandardShaderFieldsSpecifyRoughnessAndTexture_HydratesRoughnessData|ResolveMaterial_WhenFileSystemMaterialHasStandardShaderBaseColorSettings_AppliesBaseColorBuffer'"`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git -C C:\dev\helworks\helengine add engine\helengine.editor\managers\asset\MaterialAssetSettingsService.cs engine\helengine.editor\serialization\scene\EditorSceneAssetReferenceResolver.cs engine\helengine.editor.tests\managers\asset\MaterialAssetSettingsServiceTests.cs engine\helengine.editor.tests\serialization\scene\EditorSceneAssetReferenceResolverTests.cs
git -C C:\dev\helworks\helengine commit -m "feat: hydrate standard shader roughness data"
```

### Task 5: Support Standard Roughness Texture Binding End-to-End

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\ShaderRuntimeMaterialLoaderTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\rendering\DirectX11MaterialFeatureBindingTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\shaders\ForwardStandardShaderTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\assets\material\StandardMaterialTextureBindingDefaults.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.shader\assets\ShaderRuntimeMaterialLoader.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.directx11\DirectX11Renderer3D.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\shaders\builtin\ForwardStandardShader.hlsl`

- [ ] **Step 1: Write the failing shader layout and runtime-loader tests**

Add a new binding expectation:

```csharp
Assert.Contains(layout.TextureBindings, binding => binding.Name == StandardMaterialTextureBindingDefaults.RoughnessTextureBindingName);
```

Add a runtime-loader roughness assertion:

```csharp
RuntimeTexture roughnessTexture = ShaderRuntimeMaterialAccess.Require(runtimeMaterial)
    .Properties.GetTexture(StandardMaterialTextureBindingDefaults.RoughnessTextureBindingName);

Assert.NotNull(roughnessTexture);
```

Update the packaged shader fixture to expose both texture bindings:

```csharp
CreateBinding(StandardMaterialTextureBindingDefaults.DiffuseTextureBindingName, ShaderResourceType.Texture2D, 0, 0, 0),
CreateBinding(StandardMaterialTextureBindingDefaults.RoughnessTextureBindingName, ShaderResourceType.Texture2D, 0, 6, 0)
```

- [ ] **Step 2: Write the failing DirectX11 texture-binding preservation test**

```csharp
[Fact]
public void BuildMaterialFromRaw_WhenMaterialExposesDiffuseAndRoughnessTextureBindings_PreservesBothBindings() {
    ShaderMaterialAsset materialAsset = new ShaderMaterialAsset {
        Id = "materials/test",
        ShaderAssetId = "shader/test",
        VertexProgram = "VS",
        PixelProgram = "PS",
        Variant = "default"
    };
    ShaderAsset shaderAsset = new ShaderAsset {
        Id = "shader/test",
        Programs = new[] {
            CreateProgram("VS", ShaderStage.Vertex),
            CreateProgram(
                "PS",
                ShaderStage.Pixel,
                CreateBinding(StandardMaterialTextureBindingDefaults.DiffuseTextureBindingName, ShaderResourceType.Texture2D, 0, 0, 0),
                CreateBinding(StandardMaterialTextureBindingDefaults.RoughnessTextureBindingName, ShaderResourceType.Texture2D, 0, 6, 0))
        },
        Binaries = Array.Empty<ShaderBinaryAsset>()
    };
    TestDirectX11RenderManager3D renderer = TestDirectX11RenderManager3D.Create();

    ShaderRuntimeMaterial material = Assert.IsAssignableFrom<ShaderRuntimeMaterial>(renderer.BuildMaterialFromRaw(materialAsset, shaderAsset));

    Assert.Contains(material.Layout.TextureBindings, binding => binding.Name == StandardMaterialTextureBindingDefaults.DiffuseTextureBindingName);
    Assert.Contains(material.Layout.TextureBindings, binding => binding.Name == StandardMaterialTextureBindingDefaults.RoughnessTextureBindingName);
}
```

- [ ] **Step 3: Run the three focused tests and verify they fail**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'LoadShaderAsset_WhenCompilingForDirectX11_ExposesExpectedStandardMaterialBindings|BuildMaterialFromRawAsset_WhenMaterialReferencesImportedDiffuseTexture_BindsRuntimeTexture|BuildMaterialFromRaw_WhenMaterialExposesDiffuseAndRoughnessTextureBindings_PreservesBothBindings'"`

Expected: FAIL because the shader does not yet declare roughness bindings and the runtime loader only applies diffuse textures.

- [ ] **Step 4: Add the standard roughness texture binding contract**

```csharp
public const string RoughnessTextureBindingName = "RoughnessTexture";
```

and keep the default apply logic limited to diffuse:

```csharp
int roughnessTextureBindingIndex = material.Layout.FindTextureBindingIndex(RoughnessTextureBindingName);
if (roughnessTextureBindingIndex >= 0 && material.Properties.GetTexture(roughnessTextureBindingIndex) == null) {
    material.Properties.SetTexture(roughnessTextureBindingIndex, TextureUtils.PixelTexture);
}
```

- [ ] **Step 5: Load the packaged roughness texture**

```csharp
ApplyImportedTexture(assetContentManager, materialAsset.DiffuseTextureAssetId, runtimeMaterial, StandardMaterialTextureBindingDefaults.DiffuseTextureBindingName);
ApplyImportedTexture(assetContentManager, materialAsset.RoughnessTextureAssetId, runtimeMaterial, StandardMaterialTextureBindingDefaults.RoughnessTextureBindingName);
```

Use one shared helper:

```csharp
static void ApplyImportedTexture(ContentManager assetContentManager, string textureAssetId, RuntimeMaterial runtimeMaterial, string bindingName) {
    if (string.IsNullOrWhiteSpace(textureAssetId)) {
        return;
    }

    ShaderRuntimeMaterial shaderRuntimeMaterial = ShaderRuntimeMaterialAccess.Require(runtimeMaterial);
    int bindingIndex = shaderRuntimeMaterial.Layout.FindTextureBindingIndex(bindingName);
    if (bindingIndex < 0) {
        return;
    }

    // existing packaged texture rebuild path
    shaderRuntimeMaterial.Properties.SetTexture(bindingIndex, runtimeTexture);
}
```

- [ ] **Step 6: Bind all material textures in DirectX11**

```csharp
for (int textureIndex = 0; textureIndex < material.Layout.TextureBindings.Length; textureIndex++) {
    RuntimeTexture runtimeTexture = material.Properties.GetTexture(textureIndex);
    MaterialLayoutBinding binding = material.Layout.TextureBindings[textureIndex];
    ShaderResourceView resourceView = ResolveRuntimeTextureResourceView(runtimeTexture);
    context.PixelShader.SetShaderResource(binding.Slot, resourceView);
    context.PixelShader.SetSampler(binding.Slot, materialTextureSampler);
}
```

Clear any missing bindings explicitly:

```csharp
if (runtimeTexture == null) {
    context.PixelShader.SetShaderResource(binding.Slot, null);
    context.PixelShader.SetSampler(binding.Slot, null);
    continue;
}
```

- [ ] **Step 7: Update the built-in standard shader**

```hlsl
cbuffer RoughnessBuffer : register(b4)
{
    float4 roughnessValue;
};

Texture2D RoughnessTexture : register(t6);
SamplerState RoughnessTextureSampler : register(s6);
```

and replace the hardcoded specular shaping:

```hlsl
float resolvedRoughness = saturate(roughnessValue.x * RoughnessTexture.Sample(RoughnessTextureSampler, input.texCoord).r);
float shininess = lerp(128.0f, 4.0f, resolvedRoughness);
float specularStrength = lerp(0.45f, 0.05f, resolvedRoughness);
float specular = pow(saturate(dot(normal, halfVector)), shininess);
float3 specularColor = radiance * specular * specularStrength * attenuation;
```

- [ ] **Step 8: Re-run the three focused tests and verify they pass**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'LoadShaderAsset_WhenCompilingForDirectX11_ExposesExpectedStandardMaterialBindings|BuildMaterialFromRawAsset_WhenMaterialReferencesImportedDiffuseTexture_BindsRuntimeTexture|BuildMaterialFromRaw_WhenMaterialExposesDiffuseAndRoughnessTextureBindings_PreservesBothBindings'"`

Expected: PASS.

- [ ] **Step 9: Commit**

```bash
git -C C:\dev\helworks\helengine add engine\helengine.shader\assets\material\StandardMaterialTextureBindingDefaults.cs engine\helengine.shader\assets\ShaderRuntimeMaterialLoader.cs engine\helengine.directx11\DirectX11Renderer3D.cs engine\helengine.editor\shaders\builtin\ForwardStandardShader.hlsl engine\helengine.editor.tests\ShaderRuntimeMaterialLoaderTests.cs engine\helengine.editor.tests\rendering\DirectX11MaterialFeatureBindingTests.cs engine\helengine.editor.tests\shaders\ForwardStandardShaderTests.cs
git -C C:\dev\helworks\helengine commit -m "feat: bind roughness textures in windows standard shader"
```

### Task 6: Author the Marble Material Inputs and Material Factory

**Files:**
- Create: `C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarble.jpg`
- Create: `C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarbleRoughness.jpg`
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\TiltTrialPlayerSphereMarbleMaterialFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityTiltTrialMarbleMaterialTests.cs`

- [ ] **Step 1: Copy the marble albedo source into the project**

Run:

```powershell
Copy-Item `
  -LiteralPath 'C:\Users\Helena\Downloads\WhatsApp Image 2026-07-06 at 17.59.09.jpeg' `
  -Destination 'C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarble.jpg'
```

Expected: `C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarble.jpg` exists.

- [ ] **Step 2: Copy the marble roughness source into the project**

Run:

```powershell
Copy-Item `
  -LiteralPath 'C:\Users\Helena\Downloads\WhatsApp Image 2026-07-06 at 17.59.58.jpeg' `
  -Destination 'C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarbleRoughness.jpg'
```

Expected: `C:\dev\helprojs\city\assets\Textures\rendering\tilt_trial\PlayerSphereMarbleRoughness.jpg` exists.

- [ ] **Step 3: Write the failing authored marble material test**

```csharp
public sealed class CityTiltTrialMarbleMaterialTests {
    const string TiltTrialMarbleMaterialPath = @"C:\dev\helprojs\city\assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset";

    [Fact]
    public void Tilt_trial_marble_material_source_preserves_windows_roughness_fields() {
        MaterialAssetSettingsService settingsService = new MaterialAssetSettingsService();

        Assert.True(File.Exists(TiltTrialMarbleMaterialPath));
        Assert.True(settingsService.TryLoadPlatformSettings(
            TiltTrialMarbleMaterialPath,
            "windows",
            out MaterialAssetProcessorSettings platformSettings));

        Assert.Equal("standard-shader", platformSettings.SchemaId);
        Assert.Equal("1.0", platformSettings.FieldValues["roughness"]);
        Assert.False(string.IsNullOrWhiteSpace(platformSettings.FieldValues["roughness-texture-id"]));
    }
}
```

- [ ] **Step 4: Run the marble material test and verify it fails**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'Tilt_trial_marble_material_source_preserves_windows_roughness_fields'"`

Expected: FAIL because the marble material factory and authored material do not exist yet.

- [ ] **Step 5: Create the marble material factory**

```csharp
public sealed class TiltTrialPlayerSphereMarbleMaterialFactory {
    public const string MaterialRelativePath = "materials/rendering/tilt_trial/PlayerSphereMarble.hasset";
    public const string MaterialAssetId = "Materials.rendering.tilt_trial.PlayerSphereMarble";
    public const string DiffuseTextureRelativePath = "Textures/rendering/tilt_trial/PlayerSphereMarble.jpg";
    public const string RoughnessTextureRelativePath = "Textures/rendering/tilt_trial/PlayerSphereMarbleRoughness.jpg";

    void ConfigureWindowsPlatform(GeneratedMaterialPlatformDefinition platformDefinition, string diffuseTextureAssetId, string roughnessTextureAssetId) {
        platformDefinition.SchemaId = "standard-shader";
        platformDefinition.SetFieldValue("use-custom-shader", "false");
        platformDefinition.SetFieldValue("shader-asset-id", "ForwardStandardShader");
        platformDefinition.SetFieldValue("texture-id", diffuseTextureAssetId);
        platformDefinition.SetFieldValue("roughness", "1.0");
        platformDefinition.SetFieldValue("roughness-texture-id", roughnessTextureAssetId);
        platformDefinition.SetFieldValue("casts-shadow", "true");
        platformDefinition.SetFieldValue("receives-shadow", "true");
        platformDefinition.SetFieldValue("base-color", "#FFFFFFFF");
    }
}
```

Mirror the walnut factory pattern for non-Windows fallback platforms, but leave roughness fields Windows-only.

- [ ] **Step 6: Generate the authored marble material and re-run the test**

Run: `rtk powershell -NoProfile -Command "dotnet build 'C:\dev\helprojs\city\user_settings\generated_code\projects\game.tools\game.tools.csproj' -c Debug"`

Then run:

`rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'Tilt_trial_marble_material_source_preserves_windows_roughness_fields'"`

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git -C C:\dev\helprojs\city add assets\Textures\rendering\tilt_trial\PlayerSphereMarble.jpg assets\Textures\rendering\tilt_trial\PlayerSphereMarbleRoughness.jpg assets\codebase\rendering.tools\TiltTrialPlayerSphereMarbleMaterialFactory.cs assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset
git -C C:\dev\helworks\helengine add engine\helengine.editor.tests\CityTiltTrialMarbleMaterialTests.cs
git -C C:\dev\helprojs\city commit -m "feat: add tilt trial marble roughness material"
git -C C:\dev\helworks\helengine commit -m "test: cover tilt trial marble material fields"
```

### Task 7: Switch Tilt Trial from Walnut to Marble

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneGenerator.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingSceneGenerationAssets.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingSceneAssetPreparationService.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityGameSceneSourceTests.cs`

- [ ] **Step 1: Write the failing Tilt Trial source test updates**

Replace the walnut assertions with marble assertions:

```csharp
Assert.Contains("Materials.rendering.tilt_trial.PlayerSphereMarble", gameSceneFactorySource, StringComparison.Ordinal);
Assert.Contains("CreateFileSystemMaterial(TiltTrialPlayerSphereMarbleMaterialRelativePath)", gameSceneFactorySource, StringComparison.Ordinal);
Assert.Contains("TiltTrialPlayerSphereMarbleMaterialFactory", gameSceneGeneratorSource, StringComparison.Ordinal);
Assert.Contains("materials/rendering/tilt_trial/PlayerSphereMarble.hasset", preparationSource, StringComparison.Ordinal);
```

- [ ] **Step 2: Run the focused source test and verify it fails**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'City_game_scene_source_uses_dedicated_tilt_trial_player_sphere_material'"`

Expected: FAIL because the generated gameplay scene still uses walnut.

- [ ] **Step 3: Switch the generated material references**

In `GameSceneGenerator.cs`:

```csharp
TiltTrialPlayerSphereMarbleMaterialFactory materialFactory = new TiltTrialPlayerSphereMarbleMaterialFactory();
materialFactory.WriteMaterialAsset(projectRootPath);
```

In `RenderingSceneGenerationAssets.cs`:

```csharp
public RuntimeMaterial TiltTrialPlayerSphereMarbleMaterial { get; set; }
```

In `RenderingSceneAssetPreparationService.cs`:

```csharp
RuntimeMaterial tiltTrialPlayerSphereMarbleMaterial = LoadRuntimeMaterial(
    bootstrap,
    projectRootPath,
    "materials/rendering/tilt_trial/PlayerSphereMarble.hasset");
```

In `GameSceneFactory.cs`:

```csharp
const string TiltTrialPlayerSphereMarbleMaterialAssetId = "Materials.rendering.tilt_trial.PlayerSphereMarble";
const string TiltTrialPlayerSphereMarbleMaterialRelativePath = "materials/rendering/tilt_trial/PlayerSphereMarble.hasset";
readonly RuntimeMaterial TiltTrialPlayerSphereMarbleMaterial;
Materials = new[] { TiltTrialPlayerSphereMarbleMaterial };
CreateFileSystemMaterial(TiltTrialPlayerSphereMarbleMaterialRelativePath)
```

- [ ] **Step 4: Re-run the focused source test and verify it passes**

Run: `rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj' --filter 'City_game_scene_source_uses_dedicated_tilt_trial_player_sphere_material'"`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git -C C:\dev\helprojs\city add assets\codebase\game.tools\GameSceneGenerator.cs assets\codebase\rendering.tools\RenderingSceneGenerationAssets.cs assets\codebase\rendering.tools\RenderingSceneAssetPreparationService.cs assets\codebase\game.tools\GameSceneFactory.cs
git -C C:\dev\helworks\helengine add engine\helengine.editor.tests\CityGameSceneSourceTests.cs
git -C C:\dev\helprojs\city commit -m "feat: switch tilt trial sphere to marble material"
git -C C:\dev\helworks\helengine commit -m "test: cover tilt trial marble scene wiring"
```

### Task 8: Rebuild and Verify the Windows Package

**Files:**
- Modify: `C:\dev\helprojs\city\assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset`
- Modify: `C:\dev\helprojs\city\assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset.windows.hasset`
- Verify: `C:\dev\helprojs\city\windows-build`

- [ ] **Step 1: Rebuild the editor app and city gameplay tools**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet build 'C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj' -c Debug --no-restore"
rtk powershell -NoProfile -Command "dotnet build 'C:\dev\helprojs\city\user_settings\generated_code\projects\game.tools\game.tools.csproj' -c Debug --no-restore"
```

Expected: both builds succeed.

- [ ] **Step 2: Stop stale Windows editor/player processes**

Run:

```powershell
rtk powershell -NoProfile -Command "Get-Process helengine_windows -ErrorAction SilentlyContinue | Stop-Process -Force; Get-Process 'helengine.editor.app' -ErrorAction SilentlyContinue | Stop-Process -Force"
```

Expected: command exits without a locking error.

- [ ] **Step 3: Build the Windows package**

Run:

```powershell
rtk powershell -NoProfile -Command "& 'C:\Program Files\dotnet\dotnet.exe' 'C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll' --project 'C:\dev\helprojs\city\project.heproj' --build windows --output 'C:\dev\helprojs\city\windows-build'"
```

Expected: log ends with `Build completed for platform 'windows': C:\dev\helprojs\city\windows-build`.

- [ ] **Step 4: Launch the Windows build**

Run:

```powershell
rtk powershell -NoProfile -Command "Start-Process -FilePath 'C:\dev\helprojs\city\windows-build\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\city\windows-build'"
```

Expected: the Windows build launches into the current city startup flow.

- [ ] **Step 5: Verify Tilt Trial visually**

Check:

```text
- Tilt Trial uses the marble ball instead of the walnut ball.
- The ball surface response changes with view/light angle.
- Increasing or decreasing authored roughness changes highlight width and intensity.
- No missing-texture fallback or transparent-ball regression appears.
```

- [ ] **Step 6: Commit final authored assets**

```bash
git -C C:\dev\helprojs\city add assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset.windows.hasset windows-build
git -C C:\dev\helprojs\city commit -m "feat: add windows standard shader roughness to tilt trial marble"
```
