# Unreal-Style Metallic Specular Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add scalar `metallic` and Unreal-style `specular` support to the shared `standard-shader` workflow, then author the Tilt Trial marble ball to stay non-metallic while using the roughness texture plus stronger dielectric highlights.

**Architecture:** Follow the existing roughness path instead of inventing a parallel material system. Define shared constant-buffer defaults in `helengine.core`, hydrate them from authored standard-shader field values in editor and builder code, consume them in `ForwardStandardShader.hlsl`, and only then update the city-authored marble material. Keep this first pass scalar-only: no metallic/specular textures, no serializer version bump, and no fixed-pipeline backend shading changes beyond preview defaults.

**Tech Stack:** C#/.NET 9, HLSL, xUnit, helengine editor material pipeline, helengine Windows builder, city generated authored-material workflow.

---

### Task 1: Add Shared Metallic and Specular Buffer Defaults Plus Round-Trip Coverage

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.core\material\StandardMaterialMetallicDefaults.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\material\StandardMaterialSpecularDefaults.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\BinarySerializationTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\BinarySerializationTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Shader_material_binary_serializer_round_trips_metallic_and_specular_constant_buffers() {
    ShaderMaterialAsset asset = CreateMaterialAsset();
    asset.ConstantBuffers = new[] {
        new MaterialConstantBufferAsset {
            Name = StandardMaterialMetallicDefaults.MetallicBufferName,
            Data = StandardMaterialMetallicDefaults.CreateConstantBufferData(0.25f)
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialSpecularDefaults.SpecularBufferName,
            Data = StandardMaterialSpecularDefaults.CreateConstantBufferData(0.75f)
        }
    };

    byte[] data = ShaderMaterialAssetBinarySerializer.SerializeToBytes(asset);

    using MemoryStream stream = new MemoryStream(data, writable: false);
    ShaderMaterialAsset deserialized = ShaderMaterialAssetBinarySerializer.Deserialize(stream);

    MaterialConstantBufferAsset metallicBuffer = Assert.Single(
        deserialized.ConstantBuffers,
        buffer => buffer.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
    MaterialConstantBufferAsset specularBuffer = Assert.Single(
        deserialized.ConstantBuffers,
        buffer => buffer.Name == StandardMaterialSpecularDefaults.SpecularBufferName);

    Assert.Equal(StandardMaterialMetallicDefaults.CreateConstantBufferData(0.25f), metallicBuffer.Data);
    Assert.Equal(StandardMaterialSpecularDefaults.CreateConstantBufferData(0.75f), specularBuffer.Data);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter Shader_material_binary_serializer_round_trips_metallic_and_specular_constant_buffers`

Expected: FAIL because `StandardMaterialMetallicDefaults` and `StandardMaterialSpecularDefaults` do not exist yet.

- [ ] **Step 3: Write minimal implementation**

```csharp
namespace helengine {
    /// <summary>
    /// Stores the shared standard-material metallic constant-buffer contract used by builder, editor, and runtime paths.
    /// </summary>
    public static class StandardMaterialMetallicDefaults {
        /// <summary>
        /// Stable constant-buffer binding name used by the built-in forward standard shader for authored metallic.
        /// </summary>
        public const string MetallicBufferName = "MetallicBuffer";

        /// <summary>
        /// Default authored metallic used when a material omits the field.
        /// </summary>
        public const float DefaultMetallic = 0f;

        /// <summary>
        /// Creates one packed float4 constant-buffer payload from the supplied metallic value.
        /// </summary>
        /// <param name="metallic">Authored metallic value that will be clamped to the supported zero-to-one range.</param>
        /// <returns>Sixteen-byte packed constant-buffer payload.</returns>
        public static byte[] CreateConstantBufferData(float metallic) {
            float normalized = Math.Clamp(metallic, 0f, 1f);
            byte[] data = new byte[16];
            WriteSingle(data, 0, normalized);
            WriteSingle(data, 4, normalized);
            WriteSingle(data, 8, normalized);
            WriteSingle(data, 12, normalized);
            return data;
        }

        /// <summary>
        /// Creates one default metallic constant-buffer payload.
        /// </summary>
        /// <returns>Sixteen-byte packed constant-buffer payload for the default metallic value.</returns>
        public static byte[] CreateDefaultConstantBufferData() {
            return CreateConstantBufferData(DefaultMetallic);
        }

        static void WriteSingle(byte[] data, int offset, float value) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            } else if (offset < 0 || offset > data.Length - 4) {
                throw new ArgumentOutOfRangeException(nameof(offset), "Single-precision values require four writable bytes.");
            }

            int bits = BitConverter.SingleToInt32Bits(value);
            data[offset] = (byte)bits;
            data[offset + 1] = (byte)(bits >> 8);
            data[offset + 2] = (byte)(bits >> 16);
            data[offset + 3] = (byte)(bits >> 24);
        }
    }
}
```

```csharp
namespace helengine {
    /// <summary>
    /// Stores the shared standard-material specular constant-buffer contract used by builder, editor, and runtime paths.
    /// </summary>
    public static class StandardMaterialSpecularDefaults {
        /// <summary>
        /// Stable constant-buffer binding name used by the built-in forward standard shader for authored specular.
        /// </summary>
        public const string SpecularBufferName = "SpecularBuffer";

        /// <summary>
        /// Default authored specular used when a material omits the field.
        /// </summary>
        public const float DefaultSpecular = 0.5f;

        /// <summary>
        /// Creates one packed float4 constant-buffer payload from the supplied specular value.
        /// </summary>
        /// <param name="specular">Authored specular value that will be clamped to the supported zero-to-one range.</param>
        /// <returns>Sixteen-byte packed constant-buffer payload.</returns>
        public static byte[] CreateConstantBufferData(float specular) {
            float normalized = Math.Clamp(specular, 0f, 1f);
            byte[] data = new byte[16];
            WriteSingle(data, 0, normalized);
            WriteSingle(data, 4, normalized);
            WriteSingle(data, 8, normalized);
            WriteSingle(data, 12, normalized);
            return data;
        }

        /// <summary>
        /// Creates one default specular constant-buffer payload.
        /// </summary>
        /// <returns>Sixteen-byte packed constant-buffer payload for the default specular value.</returns>
        public static byte[] CreateDefaultConstantBufferData() {
            return CreateConstantBufferData(DefaultSpecular);
        }

        static void WriteSingle(byte[] data, int offset, float value) {
            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            } else if (offset < 0 || offset > data.Length - 4) {
                throw new ArgumentOutOfRangeException(nameof(offset), "Single-precision values require four writable bytes.");
            }

            int bits = BitConverter.SingleToInt32Bits(value);
            data[offset] = (byte)bits;
            data[offset + 1] = (byte)(bits >> 8);
            data[offset + 2] = (byte)(bits >> 16);
            data[offset + 3] = (byte)(bits >> 24);
        }
    }
}
```

```csharp
[Fact]
public void Shader_material_binary_serializer_round_trips_metallic_and_specular_constant_buffers() {
    ShaderMaterialAsset asset = CreateMaterialAsset();
    asset.ConstantBuffers = new[] {
        new MaterialConstantBufferAsset {
            Name = StandardMaterialMetallicDefaults.MetallicBufferName,
            Data = StandardMaterialMetallicDefaults.CreateConstantBufferData(0.25f)
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialSpecularDefaults.SpecularBufferName,
            Data = StandardMaterialSpecularDefaults.CreateConstantBufferData(0.75f)
        }
    };

    byte[] data = ShaderMaterialAssetBinarySerializer.SerializeToBytes(asset);

    using MemoryStream stream = new MemoryStream(data, writable: false);
    ShaderMaterialAsset deserialized = ShaderMaterialAssetBinarySerializer.Deserialize(stream);

    MaterialConstantBufferAsset metallicBuffer = Assert.Single(
        deserialized.ConstantBuffers,
        buffer => buffer.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
    MaterialConstantBufferAsset specularBuffer = Assert.Single(
        deserialized.ConstantBuffers,
        buffer => buffer.Name == StandardMaterialSpecularDefaults.SpecularBufferName);

    Assert.Equal(StandardMaterialMetallicDefaults.CreateConstantBufferData(0.25f), metallicBuffer.Data);
    Assert.Equal(StandardMaterialSpecularDefaults.CreateConstantBufferData(0.75f), specularBuffer.Data);
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter Shader_material_binary_serializer_round_trips_metallic_and_specular_constant_buffers`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.core/material/StandardMaterialMetallicDefaults.cs engine/helengine.core/material/StandardMaterialSpecularDefaults.cs engine/helengine.editor.tests/BinarySerializationTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "feat: add metallic and specular buffer defaults"
```

### Task 2: Hydrate Metallic and Specular in Editor Material Loading and Preview Fallback

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\MaterialAssetSettingsService.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\serialization\scene\EditorSceneAssetReferenceResolver.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\MaterialAssetSettingsServiceTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\EditorSceneAssetReferenceResolverTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\MaterialAssetSettingsServiceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\EditorSceneAssetReferenceResolverTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void LoadMaterialAsset_WhenWindowsStandardShaderFieldsSpecifyMetallicAndSpecular_HydratesBothBuffers() {
    string tempDirectoryPath = Path.Combine(Path.GetTempPath(), "helengine-material-settings-tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempDirectoryPath);

    try {
        string materialAssetPath = Path.Combine(tempDirectoryPath, "MarbleSphere.hasset");
        MaterialAssetSettingsService service = new MaterialAssetSettingsService();
        MaterialAssetImportSettings settings = CreateSharedTextureSettings("imported-texture-id");
        settings.Processor.Platforms["windows"].FieldValues["metallic"] = "0.25";
        settings.Processor.Platforms["windows"].FieldValues["specular"] = "0.75";

        service.Save(materialAssetPath, settings);

        ShaderMaterialAsset materialAsset = service.LoadMaterialAsset(materialAssetPath, "windows");
        MaterialConstantBufferAsset metallicBuffer = Assert.Single(
            materialAsset.ConstantBuffers,
            constantBuffer => constantBuffer.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
        MaterialConstantBufferAsset specularBuffer = Assert.Single(
            materialAsset.ConstantBuffers,
            constantBuffer => constantBuffer.Name == StandardMaterialSpecularDefaults.SpecularBufferName);

        Assert.Equal(StandardMaterialMetallicDefaults.CreateConstantBufferData(0.25f), metallicBuffer.Data);
        Assert.Equal(StandardMaterialSpecularDefaults.CreateConstantBufferData(0.75f), specularBuffer.Data);
    } finally {
        if (Directory.Exists(tempDirectoryPath)) {
            Directory.Delete(tempDirectoryPath, true);
        }
    }
}
```

```csharp
[Fact]
public void ResolveMaterial_WhenOnlyFixedPipelineMaterialExists_SeedsDefaultMetallicAndSpecularPreviewBuffers() {
    string materialRelativePath = "Materials/rendering/fixed_pipeline/Cube00.hasset";
    WriteMaterialSettingsDocument(materialRelativePath, CreatePs2OnlyMaterialSettings("#336699"));
    new EditorProjectPlatformsService(TempProjectRootPath).Save(new EditorProjectPlatformsDocument {
        SupportedPlatforms = ["ps2"]
    });
    new EditorProjectLocalSettingsService(TempProjectRootPath, ["ps2"]).SaveActivePlatform("ps2");
    ContentManager contentManager = new ContentManager(new HostFileSystemContentStreamSource(TempProjectRootPath));
    EditorContentManagerConfiguration.ConfigureSharedAssetContentManager(contentManager);
    EditorProjectPaths.Initialize(TempProjectRootPath);
    using ShaderModuleManager shaderModuleManager = CreateShaderModuleManager();
    EditorShaderPackageService.Initialize(shaderModuleManager, ShaderCompileTarget.DirectX11, contentManager);
    EditorSceneAssetReferenceResolver resolver = new EditorSceneAssetReferenceResolver(contentManager, TempProjectRootPath);

    RuntimeMaterial material = resolver.ResolveMaterial(global::helengine.editor.tests.SceneAssetReferenceTestFactory.CreateFileSystemMaterial(materialRelativePath));

    TestRenderManager3D renderManager = Assert.IsType<TestRenderManager3D>(Core.Instance.RenderManager3D);
    ShaderMaterialAsset builtMaterialAsset = Assert.Single(renderManager.BuiltMaterialAssets);
    MaterialConstantBufferAsset metallicBuffer = Assert.Single(
        builtMaterialAsset.ConstantBuffers,
        constantBuffer => constantBuffer.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
    MaterialConstantBufferAsset specularBuffer = Assert.Single(
        builtMaterialAsset.ConstantBuffers,
        constantBuffer => constantBuffer.Name == StandardMaterialSpecularDefaults.SpecularBufferName);

    Assert.NotNull(material);
    Assert.Equal(StandardMaterialMetallicDefaults.CreateDefaultConstantBufferData(), metallicBuffer.Data);
    Assert.Equal(StandardMaterialSpecularDefaults.CreateDefaultConstantBufferData(), specularBuffer.Data);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter LoadMaterialAsset_WhenWindowsStandardShaderFieldsSpecifyMetallicAndSpecular_HydratesBothBuffers`

Expected: FAIL because `MaterialAssetSettingsService` does not parse `metallic` or `specular`.

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter ResolveMaterial_WhenOnlyFixedPipelineMaterialExists_SeedsDefaultMetallicAndSpecularPreviewBuffers`

Expected: FAIL because the preview fallback path only seeds base color and roughness buffers.

- [ ] **Step 3: Write minimal implementation**

```csharp
const string MetallicFieldId = "metallic";
const string SpecularFieldId = "specular";
```

```csharp
bool ApplyStandardShaderRuntimeFields(ShaderMaterialAsset shaderMaterialAsset, Dictionary<string, string> fieldValues) {
    if (shaderMaterialAsset == null) {
        throw new ArgumentNullException(nameof(shaderMaterialAsset));
    } else if (fieldValues == null) {
        throw new ArgumentNullException(nameof(fieldValues));
    }

    bool changed = ApplyStandardShaderRenderState(shaderMaterialAsset, fieldValues);
    byte[] baseColorData = ResolveStandardShaderBaseColorBufferData(fieldValues);
    byte[] roughnessData = ResolveStandardShaderRoughnessBufferData(fieldValues);
    byte[] metallicData = ResolveStandardShaderMetallicBufferData(fieldValues);
    byte[] specularData = ResolveStandardShaderSpecularBufferData(fieldValues);
    changed |= UpsertConstantBuffer(shaderMaterialAsset, StandardMaterialBaseColorDefaults.BaseColorBufferName, baseColorData);
    changed |= UpsertConstantBuffer(shaderMaterialAsset, StandardMaterialRoughnessDefaults.RoughnessBufferName, roughnessData);
    changed |= UpsertConstantBuffer(shaderMaterialAsset, StandardMaterialMetallicDefaults.MetallicBufferName, metallicData);
    changed |= UpsertConstantBuffer(shaderMaterialAsset, StandardMaterialSpecularDefaults.SpecularBufferName, specularData);
    changed |= ApplyMirroredField(
        fieldValues,
        RoughnessTextureAssetIdFieldId,
        shaderMaterialAsset.RoughnessTextureAssetId,
        value => shaderMaterialAsset.RoughnessTextureAssetId = value,
        true);
    return changed;
}

byte[] ResolveStandardShaderMetallicBufferData(Dictionary<string, string> fieldValues) {
    if (fieldValues == null) {
        throw new ArgumentNullException(nameof(fieldValues));
    }
    if (!fieldValues.TryGetValue(MetallicFieldId, out string metallicValue) || string.IsNullOrWhiteSpace(metallicValue)) {
        return StandardMaterialMetallicDefaults.CreateDefaultConstantBufferData();
    }
    if (!float.TryParse(metallicValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float metallic)) {
        throw new InvalidOperationException("Standard material metallic must be a floating-point value.");
    }
    return StandardMaterialMetallicDefaults.CreateConstantBufferData(metallic);
}

byte[] ResolveStandardShaderSpecularBufferData(Dictionary<string, string> fieldValues) {
    if (fieldValues == null) {
        throw new ArgumentNullException(nameof(fieldValues));
    }
    if (!fieldValues.TryGetValue(SpecularFieldId, out string specularValue) || string.IsNullOrWhiteSpace(specularValue)) {
        return StandardMaterialSpecularDefaults.CreateDefaultConstantBufferData();
    }
    if (!float.TryParse(specularValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float specular)) {
        throw new InvalidOperationException("Standard material specular must be a floating-point value.");
    }
    return StandardMaterialSpecularDefaults.CreateConstantBufferData(specular);
}
```

```csharp
ShaderMaterialAsset previewMaterialAsset = new ShaderMaterialAsset {
    Id = materialAsset.Id,
    ShaderAssetId = StandardShaderAssetId,
    VertexProgram = StandardVertexProgramName,
    PixelProgram = StandardPixelProgramName,
    Variant = StandardMeshVariantName,
    ConstantBuffers = new[] {
        new MaterialConstantBufferAsset {
            Name = StandardMaterialBaseColorDefaults.BaseColorBufferName,
            Data = StandardMaterialBaseColorDefaults.CreateConstantBufferData(ResolvePreviewBaseColor(platformSettings))
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialRoughnessDefaults.RoughnessBufferName,
            Data = StandardMaterialRoughnessDefaults.CreateDefaultConstantBufferData()
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialMetallicDefaults.MetallicBufferName,
            Data = StandardMaterialMetallicDefaults.CreateDefaultConstantBufferData()
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialSpecularDefaults.SpecularBufferName,
            Data = StandardMaterialSpecularDefaults.CreateDefaultConstantBufferData()
        }
    },
    CastsShadows = materialAsset.CastsShadows,
    ReceivesShadows = materialAsset.ReceivesShadows
};
```

```csharp
MaterialAssetImportSettings CreatePs2OnlyMaterialSettings(string baseColor) {
    if (string.IsNullOrWhiteSpace(baseColor)) {
        throw new ArgumentException("Base color must be provided.", nameof(baseColor));
    }

    MaterialAssetImportSettings settings = new MaterialAssetImportSettings();
    settings.Importer.ImporterId = "helengine.material";
    settings.Importer.AssetId = "Materials/rendering/fixed_pipeline/Cube00.hasset";
    settings.Processor.Platforms["ps2"] = new MaterialAssetProcessorSettings {
        SchemaId = "ps2-simple-lit-textured",
        FieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            ["texture-id"] = string.Empty,
            ["cast-shadows"] = "true",
            ["base-color"] = baseColor
        }
    };
    return settings;
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter LoadMaterialAsset_WhenWindowsStandardShaderFieldsSpecifyMetallicAndSpecular_HydratesBothBuffers`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter ResolveMaterial_WhenOnlyFixedPipelineMaterialExists_SeedsDefaultMetallicAndSpecularPreviewBuffers`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor/managers/asset/MaterialAssetSettingsService.cs engine/helengine.editor/serialization/scene/EditorSceneAssetReferenceResolver.cs engine/helengine.editor.tests/managers/asset/MaterialAssetSettingsServiceTests.cs engine/helengine.editor.tests/serialization/scene/EditorSceneAssetReferenceResolverTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "feat: hydrate metallic and specular material fields"
```

### Task 3: Extend ForwardStandardShader to Use Metallic and Specular Scalars

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\shaders\builtin\ForwardStandardShader.hlsl`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\shaders\ForwardStandardShaderTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\shaders\ForwardStandardShaderTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void ForwardStandardShaderSource_WhenInspected_UsesAuthoredMetallicAndSpecularInputs() {
    string repositoryRootPath = new EditorSourceBuildWorkspaceLocator().ResolveHelEngineRootPath();
    string shaderPath = Path.Combine(repositoryRootPath, "engine", "helengine.editor", "shaders", "builtin", "ForwardStandardShader.hlsl");
    string shaderSource = File.ReadAllText(shaderPath);

    Assert.Contains("cbuffer MetallicBuffer", shaderSource, StringComparison.Ordinal);
    Assert.Contains("cbuffer SpecularBuffer", shaderSource, StringComparison.Ordinal);
    Assert.Contains("float metallic = saturate(metallicValue.x);", shaderSource, StringComparison.Ordinal);
    Assert.Contains("float specular = saturate(specularValue.x);", shaderSource, StringComparison.Ordinal);
    Assert.Contains("float dielectricF0 = saturate(specular) * 0.08f;", shaderSource, StringComparison.Ordinal);
    Assert.Contains("float3 reflectanceAtNormalIncidence = lerp(dielectricReflectance, surfaceColor, metallic);", shaderSource, StringComparison.Ordinal);
}
```

```csharp
Assert.Contains(layout.ConstantBufferBindings, binding => binding.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
Assert.Contains(layout.ConstantBufferBindings, binding => binding.Name == StandardMaterialSpecularDefaults.SpecularBufferName);
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter ForwardStandardShaderSource_WhenInspected_UsesAuthoredMetallicAndSpecularInputs`

Expected: FAIL because the HLSL file does not declare the new buffers yet.

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter 'LoadShaderAsset_WhenCompilingForDirectX11_ExposesExpectedStandardMaterialBindings|LoadShaderAsset_WhenCompilingForVulkan_ExposesExpectedStandardMaterialBindings'`

Expected: FAIL because the compiled material layout does not expose metallic/specular constant-buffer bindings yet.

- [ ] **Step 3: Write minimal implementation**

```hlsl
cbuffer MetallicBuffer : register(b5)
{
    float4 metallicValue;
};

cbuffer SpecularBuffer : register(b6)
{
    float4 specularValue;
};
```

```hlsl
float3 EvaluateForwardLight(
    float4 colorAndType,
    float4 directionAndShadow,
    float4 positionAndRange,
    float4 spotAngles,
    float4 shadowAtlasRect,
    float4 shadowSlotMetadata,
    float4x4 worldToShadowClip,
    float3 surfaceColor,
    float3 worldPos,
    float3 normal,
    float3 viewDirection,
    float roughness,
    float metallic,
    float specular)
{
    // existing light setup stays unchanged above this point

    float diffuse = saturate(dot(normal, lightDirection));
    if (diffuse <= 0.0f)
    {
        return float3(0.0f, 0.0f, 0.0f);
    }

    float3 halfVector = normalize(lightDirection + viewDirection);
    float resolvedRoughness = max(roughness, 0.045f);
    float dielectricF0 = saturate(specular) * 0.08f;
    float3 dielectricReflectance = float3(dielectricF0, dielectricF0, dielectricF0);
    float3 reflectanceAtNormalIncidence = lerp(dielectricReflectance, surfaceColor, metallic);
    float3 fresnel = FresnelSchlick(saturate(dot(halfVector, viewDirection)), reflectanceAtNormalIncidence);
    float distribution = DistributionGgx(normal, halfVector, resolvedRoughness);
    float geometry = GeometrySmith(normal, viewDirection, lightDirection, resolvedRoughness);
    float normalDotView = saturate(dot(normal, viewDirection));
    float specularDenominator = max(4.0f * normalDotView * diffuse, 0.0001f);
    float3 specularColor = (distribution * geometry * fresnel / specularDenominator) * radiance * diffuse * attenuation;
    float3 diffuseWeight = (1.0f - fresnel) * (1.0f - metallic);
    float3 diffuseColor = (surfaceColor / 3.14159265f) * diffuseWeight * radiance * diffuse * attenuation;

    return diffuseColor + specularColor;
}
```

```hlsl
float4 sampledBaseColor = DiffuseTexture.Sample(DiffuseTextureSampler, input.texCoord) * baseColor;
float roughness = saturate(RoughnessTexture.Sample(RoughnessTextureSampler, input.texCoord).r * roughnessValue.x);
float metallic = saturate(metallicValue.x);
float specular = saturate(specularValue.x);
float3 surfaceColor = sampledBaseColor.rgb;
```

```hlsl
color += EvaluateForwardLight(light0ColorAndType, light0DirectionAndShadow, light0PositionAndRange, light0SpotAngles, shadowLight0AtlasRect, shadowLight0Metadata, shadowLight0WorldToShadowClip, surfaceColor, input.worldPos, normal, viewDirection, roughness, metallic, specular);
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter ForwardStandardShaderSource_WhenInspected_UsesAuthoredMetallicAndSpecularInputs`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter 'LoadShaderAsset_WhenCompilingForDirectX11_ExposesExpectedStandardMaterialBindings|LoadShaderAsset_WhenCompilingForVulkan_ExposesExpectedStandardMaterialBindings'`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor/shaders/builtin/ForwardStandardShader.hlsl engine/helengine.editor.tests/shaders/ForwardStandardShaderTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "feat: add metallic and specular shader inputs"
```

### Task 4: Extend the Windows Builder Schema and Material Cook Path

**Files:**
- Modify: `C:\dev\helworks\helengine-windows\builder\WindowsPlatformDefinitionFactory.cs`
- Modify: `C:\dev\helworks\helengine-windows\builder\WindowsPlatformAssetBuilder.cs`
- Modify: `C:\dev\helworks\helengine-windows\builder.tests\WindowsPlatformAssetBuilderTests.cs`
- Test: `C:\dev\helworks\helengine-windows\builder.tests\WindowsPlatformAssetBuilderTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Fact]
public void Descriptor_and_definition_expose_standard_material_metallic_and_specular_fields() {
    WindowsPlatformAssetBuilder builder = new();

    PlatformMaterialSchemaDefinition schema = Assert.Single(builder.Definition.MaterialSchemas, materialSchema => materialSchema.SchemaId == "standard-shader");

    Assert.Contains(schema.Fields, field => field.FieldId == "metallic" && field.FieldKind == PlatformMaterialFieldKind.Text && field.DefaultValue == "0.0" && !field.Required);
    Assert.Contains(schema.Fields, field => field.FieldId == "specular" && field.FieldKind == PlatformMaterialFieldKind.Text && field.DefaultValue == "0.5" && !field.Required);
}

[Fact]
public void CookMaterial_preserves_metallic_and_specular_scalar_fields() {
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
            ["metallic"] = "0.25",
            ["specular"] = "0.75"
        }));

    ShaderMaterialAsset materialAsset = Assert.IsType<ShaderMaterialAsset>(global::helengine.files.AssetSerializer.DeserializeFromBytes(result.CookedMaterialBytes));
    MaterialConstantBufferAsset metallicBuffer = Assert.Single(
        materialAsset.ConstantBuffers,
        constantBuffer => constantBuffer.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
    MaterialConstantBufferAsset specularBuffer = Assert.Single(
        materialAsset.ConstantBuffers,
        constantBuffer => constantBuffer.Name == StandardMaterialSpecularDefaults.SpecularBufferName);

    Assert.Equal(StandardMaterialMetallicDefaults.CreateConstantBufferData(0.25f), metallicBuffer.Data);
    Assert.Equal(StandardMaterialSpecularDefaults.CreateConstantBufferData(0.75f), specularBuffer.Data);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `rtk dotnet test C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj --filter Descriptor_and_definition_expose_standard_material_metallic_and_specular_fields`

Expected: FAIL because the schema does not publish the two fields yet.

Run: `rtk dotnet test C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj --filter CookMaterial_preserves_metallic_and_specular_scalar_fields`

Expected: FAIL because the cook path only writes base color and roughness buffers.

- [ ] **Step 3: Write minimal implementation**

```csharp
new PlatformMaterialFieldDefinition(
    "metallic",
    "Metallic",
    PlatformMaterialFieldKind.Text,
    "0.0",
    false,
    []),
new PlatformMaterialFieldDefinition(
    "specular",
    "Specular",
    PlatformMaterialFieldKind.Text,
    "0.5",
    false,
    []),
```

```csharp
const string MetallicFieldId = "metallic";
const string SpecularFieldId = "specular";
```

```csharp
float metallic = ReadOptionalFloatField(request.FieldValues, MetallicFieldId, StandardMaterialMetallicDefaults.DefaultMetallic);
float specular = ReadOptionalFloatField(request.FieldValues, SpecularFieldId, StandardMaterialSpecularDefaults.DefaultSpecular);

ShaderMaterialAsset materialAsset = new ShaderMaterialAsset {
    Id = request.MaterialAssetId,
    ShaderAssetId = shaderAssetId,
    VertexProgram = vertexProgram,
    PixelProgram = pixelProgram,
    Variant = variant,
    DiffuseTextureAssetId = diffuseTextureAssetId,
    RoughnessTextureAssetId = roughnessTextureAssetId,
    CastsShadows = castsShadows,
    ReceivesShadows = receivesShadows,
    RenderState = new MaterialRenderState(),
    ConstantBuffers = [
        new MaterialConstantBufferAsset {
            Name = BaseColorBufferName,
            Data = CreateFloat4ConstantBufferData(ParseBaseColor(baseColor))
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialRoughnessDefaults.RoughnessBufferName,
            Data = StandardMaterialRoughnessDefaults.CreateConstantBufferData(roughness)
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialMetallicDefaults.MetallicBufferName,
            Data = StandardMaterialMetallicDefaults.CreateConstantBufferData(metallic)
        },
        new MaterialConstantBufferAsset {
            Name = StandardMaterialSpecularDefaults.SpecularBufferName,
            Data = StandardMaterialSpecularDefaults.CreateConstantBufferData(specular)
        }
    ]
};
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `rtk dotnet test C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj --filter Descriptor_and_definition_expose_standard_material_metallic_and_specular_fields`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj --filter CookMaterial_preserves_metallic_and_specular_scalar_fields`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helworks\helengine-windows add builder/WindowsPlatformDefinitionFactory.cs builder/WindowsPlatformAssetBuilder.cs builder.tests/WindowsPlatformAssetBuilderTests.cs
rtk git -C C:\dev\helworks\helengine-windows commit -m "feat: add metallic and specular builder support"
```

### Task 5: Author the Tilt Trial Marble Material for the New Workflow and Validate the Windows Build

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialPlayerSphereMarbleMaterialFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityTiltTrialMarbleMaterialTests.cs`
- Regenerate: `C:\dev\helprojs\demodisc\assets\materials\rendering\tilt_trial\PlayerSphereMarble.hasset`
- Rebuild output: `C:\dev\helprojs\demodisc\windows-build`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityTiltTrialMarbleMaterialTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void Tilt_trial_marble_material_source_preserves_windows_metallic_specular_and_roughness_fields() {
    MaterialAssetSettingsService settingsService = new MaterialAssetSettingsService();

    Assert.True(File.Exists(TiltTrialMarbleMaterialPath), $"Expected Tilt Trial marble material at '{TiltTrialMarbleMaterialPath}'.");
    Assert.True(settingsService.TryLoadPlatformSettings(TiltTrialMarbleMaterialPath, "windows", out MaterialAssetProcessorSettings platformSettings));
    Assert.NotNull(platformSettings);
    Assert.Equal("standard-shader", platformSettings.SchemaId);
    Assert.Equal("1.0", platformSettings.FieldValues["roughness"]);
    Assert.Equal("0.0", platformSettings.FieldValues["metallic"]);
    Assert.Equal("0.5", platformSettings.FieldValues["specular"]);
    Assert.False(string.IsNullOrWhiteSpace(platformSettings.FieldValues["roughness-texture-id"]));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter Tilt_trial_marble_material_source_preserves_windows_metallic_specular_and_roughness_fields`

Expected: FAIL because the generated marble material does not author `metallic` or `specular` yet.

- [ ] **Step 3: Write minimal implementation**

```csharp
const string MetallicFieldId = "metallic";
const string SpecularFieldId = "specular";
```

```csharp
platformDefinition.SchemaId = WindowsMaterialSchemaId;
platformDefinition.SetFieldValue(UseCustomShaderFieldId, "false");
platformDefinition.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
platformDefinition.SetFieldValue(TextureIdFieldId, diffuseTextureAssetId);
platformDefinition.SetFieldValue(RoughnessFieldId, "1.0");
platformDefinition.SetFieldValue(MetallicFieldId, "0.0");
platformDefinition.SetFieldValue(SpecularFieldId, "0.5");
platformDefinition.SetFieldValue(RoughnessTextureIdFieldId, roughnessTextureAssetId);
platformDefinition.SetFieldValue(AlphaModeFieldId, "opaque");
platformDefinition.SetFieldValue(DoubleSidedFieldId, "true");
platformDefinition.SetFieldValue(CastsShadowFieldId, "true");
platformDefinition.SetFieldValue(ReceivesShadowFieldId, "true");
platformDefinition.SetFieldValue(BaseColorFieldId, "#FFFFFFFF");
```

```bash
rtk dotnet build C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools\game.tools.csproj -c Debug
rtk proxy dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-game-scenes
```

- [ ] **Step 4: Run tests and package validation**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter Tilt_trial_marble_material_source_preserves_windows_metallic_specular_and_roughness_fields`

Expected: PASS

Run: `rtk proxy dotnet C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\project.heproj --build windows --output C:\dev\helprojs\demodisc\windows-build`

Expected: build completes and refreshes `C:\dev\helprojs\demodisc\windows-build`

Run: `rtk proxy powershell -NoProfile -Command "Get-Process helengine_windows -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue; Start-Process -FilePath 'C:\dev\helprojs\demodisc\windows-build\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\demodisc\windows-build'"`

Expected: the Windows player launches from the rebuilt package

- [ ] **Step 5: Commit**

```bash
rtk git -C C:\dev\helprojs\demodisc add assets/codebase/rendering.tools/TiltTrialPlayerSphereMarbleMaterialFactory.cs assets/materials/rendering/tilt_trial/PlayerSphereMarble.hasset
rtk git -C C:\dev\helprojs\demodisc commit -m "feat: author marble metallic and specular defaults"
```
