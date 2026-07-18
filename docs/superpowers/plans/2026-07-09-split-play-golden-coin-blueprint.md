# Split Play Golden Coin Blueprint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Generate one reusable `GoldenCoin` blueprint asset for Split Play, backed by a rounded common cylinder mesh, a lower-step Nintendo DS cylinder mesh, and one shared gold material with a `ds` mesh-component model override.

**Architecture:** Add a small generated asset pipeline under `city.game.tools` that owns the coin asset catalog, raw model serialization, blueprint serialization, and the coin-specific asset factory. Hook that generator into `GameSceneGenerator` so gameplay asset regeneration writes the coin support assets alongside existing Tilt Trial assets. Validate the result with one real asset-deserialization test plus one source-level integration test that proves the generator is wired in.

**Tech Stack:** C#/.NET 9, city code modules under `assets/codebase`, Helengine raw asset serialization (`AssetSerializer`, `ModelAsset`, `BlueprintAsset`), xUnit.

---

### Task 1: Lock the public asset contract with failing tests

**Files:**
- Create: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs`
- Create: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinSourceTests.cs`
- Modify: `assets/codebase/game.tools/GameSceneGenerator.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinSourceTests.cs`

- [ ] **Step 1: Write the failing asset-generation test**

```csharp
using helengine;
using Xunit;

namespace city.tests {
    /// <summary>
    /// Verifies Split Play support asset generation writes one common coin model, one DS override model, one shared material, and one blueprint with the expected model override.
    /// </summary>
    public sealed class SplitPlayGoldenCoinAssetGenerationTests : IDisposable {
        readonly string ProjectRootPath;

        public SplitPlayGoldenCoinAssetGenerationTests() {
            ProjectRootPath = Path.Combine(Path.GetTempPath(), "city-split-play-coin-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(ProjectRootPath, "assets"));
        }

        public void Dispose() {
            if (Directory.Exists(ProjectRootPath)) {
                Directory.Delete(ProjectRootPath, true);
            }
        }

        [Fact]
        public void Generate_writes_coin_models_material_and_blueprint_with_ds_model_override() {
            SplitPlayGoldenCoinAssetGenerator generator = new SplitPlayGoldenCoinAssetGenerator();

            generator.Generate(ProjectRootPath);

            string commonModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "split_play", "golden_coin.hasset");
            string dsModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "split_play", "golden_coin_ds.hasset");
            string materialPath = Path.Combine(ProjectRootPath, "assets", "materials", "games", "split_play", "GoldenCoin.hasset");
            string blueprintPath = Path.Combine(ProjectRootPath, "assets", "blueprints", "games", "split_play", "GoldenCoin.hblueprint");

            Assert.True(File.Exists(commonModelPath));
            Assert.True(File.Exists(dsModelPath));
            Assert.True(File.Exists(materialPath));
            Assert.True(File.Exists(blueprintPath));

            ModelAsset commonModel;
            using (FileStream stream = File.OpenRead(commonModelPath)) {
                commonModel = Assert.IsType<ModelAsset>(AssetSerializer.Deserialize(stream));
            }

            ModelAsset dsModel;
            using (FileStream stream = File.OpenRead(dsModelPath)) {
                dsModel = Assert.IsType<ModelAsset>(AssetSerializer.Deserialize(stream));
            }

            BlueprintAsset blueprint;
            using (FileStream stream = File.OpenRead(blueprintPath)) {
                blueprint = Assert.IsType<BlueprintAsset>(AssetSerializer.Deserialize(stream));
            }

            Assert.True(commonModel.Positions.Length > dsModel.Positions.Length);
            Assert.Equal("blueprints/games/split_play/GoldenCoin.hblueprint", blueprint.Id);
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/split_play/golden_coin.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/split_play/golden_coin_ds.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/split_play/GoldenCoin.hasset");

            SceneEntityAsset meshRoot = Assert.NotNull(blueprint.RootEntity);
            SceneComponentAssetRecord meshComponent = Assert.Single(meshRoot.Components);
            Assert.Contains(meshComponent.Fields, field => field.Name == "ModelReference");
            Assert.Contains(meshComponent.PlatformOverrides, overrideState => overrideState.PlatformId == "ds");
        }
    }
}
```

- [ ] **Step 2: Write the failing source-wiring test**

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies gameplay generation now invokes the Split Play coin asset generator.
    /// </summary>
    public sealed class SplitPlayGoldenCoinSourceTests {
        [Fact]
        public void Game_scene_generator_invokes_split_play_coin_generation() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("SplitPlayGoldenCoinAssetGenerator", source, StringComparison.Ordinal);
            Assert.Contains("splitPlayGoldenCoinAssetGenerator.Generate(projectRootPath);", source, StringComparison.Ordinal);
        }
    }
}
```

- [ ] **Step 3: Run the focused tests to verify they fail**

Run:

```powershell
dotnet test C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests --filter "FullyQualifiedName~SplitPlayGoldenCoin"
```

Expected:

- `SplitPlayGoldenCoinAssetGenerationTests.Generate_writes_coin_models_material_and_blueprint_with_ds_model_override` fails because `SplitPlayGoldenCoinAssetGenerator` does not exist yet
- `SplitPlayGoldenCoinSourceTests.Game_scene_generator_invokes_split_play_coin_generation` fails because `GameSceneGenerator.cs` does not contain the new generator call

- [ ] **Step 4: Commit the failing tests**

```bash
git -C C:\dev\helprojs\demodisc add ^
  assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs ^
  assets/codebase/game.tools.tests/SplitPlayGoldenCoinSourceTests.cs
git -C C:\dev\helprojs\demodisc commit -m "test: define Split Play coin asset generation contract"
```

### Task 2: Add generated model and blueprint write services

**Files:**
- Create: `assets/codebase/game.tools/SplitPlayGeneratedModelAssetWriteService.cs`
- Create: `assets/codebase/game.tools/SplitPlayGeneratedBlueprintAssetWriteService.cs`
- Create: `assets/codebase/game.tools/SplitPlayAssetCatalog.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs`

- [ ] **Step 1: Write the minimal shared asset catalog**

```csharp
namespace city.game.tools {
    /// <summary>
    /// Centralizes Split Play support asset ids and project-relative output paths.
    /// </summary>
    public static class SplitPlayAssetCatalog {
        public const string GoldenCoinCommonModelRelativePath = "models/games/split_play/golden_coin.hasset";
        public const string GoldenCoinDsModelRelativePath = "models/games/split_play/golden_coin_ds.hasset";
        public const string GoldenCoinMaterialRelativePath = "materials/games/split_play/GoldenCoin.hasset";
        public const string GoldenCoinBlueprintRelativePath = "blueprints/games/split_play/GoldenCoin.hblueprint";

        public const string GoldenCoinCommonModelAssetId = "Models.games.split_play.golden_coin";
        public const string GoldenCoinDsModelAssetId = "Models.games.split_play.golden_coin_ds";
        public const string GoldenCoinMaterialAssetId = "Materials.games.split_play.GoldenCoin";
    }
}
```

- [ ] **Step 2: Write the minimal raw model writer**

```csharp
namespace city.game.tools {
    /// <summary>
    /// Writes one generated raw model asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedModelAssetWriteService {
        public void WriteModel(string projectRootPath, string relativePath, ModelAsset modelAsset) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative model path must be provided.", nameof(relativePath));
            } else if (modelAsset == null) {
                throw new ArgumentNullException(nameof(modelAsset));
            }

            string fullPath = Path.Combine(
                Path.GetFullPath(projectRootPath),
                "assets",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Model directory could not be resolved."));

            using FileStream stream = File.Create(fullPath);
            AssetSerializer.Serialize(stream, modelAsset);
        }
    }
}
```

- [ ] **Step 3: Write the minimal blueprint writer**

```csharp
namespace city.game.tools {
    /// <summary>
    /// Writes one generated blueprint asset beneath the city project assets tree.
    /// </summary>
    public sealed class SplitPlayGeneratedBlueprintAssetWriteService {
        public void WriteBlueprint(string projectRootPath, string relativePath, BlueprintAsset blueprintAsset) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative blueprint path must be provided.", nameof(relativePath));
            } else if (blueprintAsset == null) {
                throw new ArgumentNullException(nameof(blueprintAsset));
            }

            string fullPath = Path.Combine(
                Path.GetFullPath(projectRootPath),
                "assets",
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Blueprint directory could not be resolved."));

            using FileStream stream = File.Create(fullPath);
            AssetSerializer.Serialize(stream, blueprintAsset);
        }
    }
}
```

- [ ] **Step 4: Run the focused tests to verify they still fail for the expected next reason**

Run:

```powershell
dotnet test C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests --filter "FullyQualifiedName~SplitPlayGoldenCoin"
```

Expected:

- the asset-generation test still fails because `SplitPlayGoldenCoinAssetGenerator` has not been implemented
- the source-wiring test still fails because `GameSceneGenerator` is not calling the generator yet

- [ ] **Step 5: Commit the shared writers**

```bash
git -C C:\dev\helprojs\demodisc add ^
  assets/codebase/game.tools/SplitPlayAssetCatalog.cs ^
  assets/codebase/game.tools/SplitPlayGeneratedModelAssetWriteService.cs ^
  assets/codebase/game.tools/SplitPlayGeneratedBlueprintAssetWriteService.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: add Split Play generated asset writers"
```

### Task 3: Implement the gold material definition and coin geometry factory

**Files:**
- Create: `assets/codebase/game.tools/SplitPlayGoldenCoinAssetGenerator.cs`
- Modify: `assets/codebase/game.tools/SplitPlayGoldenCoinAssetGenerator.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs`

- [ ] **Step 1: Write the failing geometry-specific assertion inside the existing asset test**

```csharp
Assert.NotNull(commonModel.Indices16);
Assert.NotNull(dsModel.Indices16);
Assert.True(commonModel.Indices16.Length > dsModel.Indices16.Length);
Assert.Equal(commonModel.BoundsMin.Z, dsModel.BoundsMin.Z);
Assert.Equal(commonModel.BoundsMax.Z, dsModel.BoundsMax.Z);
```

- [ ] **Step 2: Implement the generator shell and material writer hook**

```csharp
using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Generates the reusable Split Play coin support assets.
    /// </summary>
    public sealed class SplitPlayGoldenCoinAssetGenerator {
        readonly SplitPlayGeneratedModelAssetWriteService ModelWriteService;
        readonly SplitPlayGeneratedBlueprintAssetWriteService BlueprintWriteService;
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        public SplitPlayGoldenCoinAssetGenerator() {
            ModelWriteService = new SplitPlayGeneratedModelAssetWriteService();
            BlueprintWriteService = new SplitPlayGeneratedBlueprintAssetWriteService();
            MaterialWriteService = new GeneratedMaterialAssetWriteService();
        }

        public void Generate(string projectRootPath) {
            ModelWriteService.WriteModel(projectRootPath, SplitPlayAssetCatalog.GoldenCoinCommonModelRelativePath, CreateCylinderModel(SplitPlayAssetCatalog.GoldenCoinCommonModelAssetId, 20));
            ModelWriteService.WriteModel(projectRootPath, SplitPlayAssetCatalog.GoldenCoinDsModelRelativePath, CreateCylinderModel(SplitPlayAssetCatalog.GoldenCoinDsModelAssetId, 10));
            MaterialWriteService.WriteMaterial(projectRootPath, SplitPlayAssetCatalog.GoldenCoinMaterialRelativePath, CreateMaterialDefinition());
            BlueprintWriteService.WriteBlueprint(projectRootPath, SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath, CreateBlueprintAsset());
        }
    }
}
```

- [ ] **Step 3: Implement the procedural cylinder model builder**

```csharp
ModelAsset CreateCylinderModel(string assetId, int radialSteps) {
    const float radius = 0.5f;
    const float halfDepth = 0.08f;

    List<float3> positions = new List<float3>();
    List<float3> normals = new List<float3>();
    List<float2> texCoords = new List<float2>();
    List<ushort> indices = new List<ushort>();

    AppendCap(+halfDepth, +1f);
    AppendCap(-halfDepth, -1f);
    AppendSideBand();

    return new ModelAsset {
        Id = assetId,
        Positions = positions.ToArray(),
        Normals = normals.ToArray(),
        TexCoords = texCoords.ToArray(),
        Indices16 = indices.ToArray(),
        BoundsMin = new float3(-radius, -radius, -halfDepth),
        BoundsMax = new float3(radius, radius, halfDepth),
        Submeshes = [
            new ModelSubmeshAsset {
                MaterialSlotName = "DefaultMaterial",
                StartIndex = 0,
                IndexCount = indices.Count
            }
        ]
    };

    void AppendCap(float z, float normalZ) {
        ushort centerIndex = (ushort)positions.Count;
        positions.Add(new float3(0f, 0f, z));
        normals.Add(new float3(0f, 0f, normalZ));
        texCoords.Add(new float2(0.5f, 0.5f));

        for (int step = 0; step < radialSteps; step++) {
            float angle = (MathF.PI * 2f * step) / radialSteps;
            float x = MathF.Cos(angle) * radius;
            float y = MathF.Sin(angle) * radius;
            positions.Add(new float3(x, y, z));
            normals.Add(new float3(0f, 0f, normalZ));
            texCoords.Add(new float2((x / radius + 1f) * 0.5f, (y / radius + 1f) * 0.5f));
        }

        for (int step = 0; step < radialSteps; step++) {
            ushort ringA = (ushort)(centerIndex + 1 + step);
            ushort ringB = (ushort)(centerIndex + 1 + ((step + 1) % radialSteps));
            if (normalZ > 0f) {
                indices.Add(centerIndex);
                indices.Add(ringA);
                indices.Add(ringB);
            } else {
                indices.Add(centerIndex);
                indices.Add(ringB);
                indices.Add(ringA);
            }
        }
    }
}
```

- [ ] **Step 4: Implement the shared gold material definition**

```csharp
GeneratedMaterialAssetDefinition CreateMaterialDefinition() {
    GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition {
        MaterialAsset = new MaterialAsset {
            Id = SplitPlayAssetCatalog.GoldenCoinMaterialAssetId
        }
    };

    string[] supportedPlatforms = ["windows", "ps2", "psp", "gamecube", "ds"];
    for (int index = 0; index < supportedPlatforms.Length; index++) {
        GeneratedMaterialPlatformDefinition platform = definition.GetOrCreatePlatform(supportedPlatforms[index]);
        platform.SchemaId = "standard.material";
        platform.FieldValues["baseColor"] = "0.95,0.78,0.18,1.0";
        platform.FieldValues["roughness"] = supportedPlatforms[index] == "ds" ? "0.55" : "0.38";
        platform.FieldValues["metallic"] = "0.75";
    }

    return definition;
}
```

- [ ] **Step 5: Run the focused tests to verify the remaining failure is blueprint wiring**

Run:

```powershell
dotnet test C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests --filter "FullyQualifiedName~SplitPlayGoldenCoinAssetGenerationTests"
```

Expected:

- the test now gets past file existence and mesh density checks
- it still fails because `CreateBlueprintAsset()` has not been implemented yet

- [ ] **Step 6: Commit the geometry and material work**

```bash
git -C C:\dev\helprojs\demodisc add assets/codebase/game.tools/SplitPlayGoldenCoinAssetGenerator.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: generate Split Play coin geometry and material"
```

### Task 4: Implement the blueprint payload and DS mesh override

**Files:**
- Modify: `assets/codebase/game.tools/SplitPlayGoldenCoinAssetGenerator.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs`

- [ ] **Step 1: Write the minimal blueprint construction code**

```csharp
BlueprintAsset CreateBlueprintAsset() {
    SceneAssetReference commonModelReference = SceneAssetReferenceFactory.CreateFileSystemModel(SplitPlayAssetCatalog.GoldenCoinCommonModelRelativePath);
    SceneAssetReference dsModelReference = SceneAssetReferenceFactory.CreateFileSystemModel(SplitPlayAssetCatalog.GoldenCoinDsModelRelativePath);
    SceneAssetReference materialReference = SceneAssetReferenceFactory.CreateFileSystemMaterial(SplitPlayAssetCatalog.GoldenCoinMaterialRelativePath);

    EntityComponentSaveState meshSaveState = new EntityComponentSaveState();
    MeshComponent meshComponent = new MeshComponent();
    meshSaveState.SetAssetReference(meshComponent, "ModelReference", commonModelReference);
    meshSaveState.SetAssetReference(meshComponent, "Materials[0]", materialReference);

    EntityComponentPlatformOverrideState dsOverride = meshSaveState.GetOrCreateComponentPlatformOverride("ds");
    dsOverride.SetAssetReference("ModelReference", dsModelReference);

    ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
    SceneComponentAssetRecord serializedMesh = registry.GetDescriptor(meshComponent).SerializeComponent(meshComponent, 0, meshSaveState);

    return new BlueprintAsset {
        Id = SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath,
        RootEntity = new SceneEntityAsset {
            Id = 1u,
            Name = "GoldenCoin",
            LayerMask = EditorLayerMasks.SceneObjects,
            LocalPosition = float3.Zero,
            LocalScale = float3.One,
            LocalOrientation = float4.Identity,
            Components = [serializedMesh],
            Children = Array.Empty<SceneEntityAsset>()
        },
        AssetReferences = [commonModelReference, dsModelReference, materialReference]
    };
}
```

- [ ] **Step 2: Tighten the asset test to inspect the DS override**

```csharp
EntityComponentPlatformOverrideState dsOverride = Assert.Single(meshComponent.PlatformOverrides, state => state.PlatformId == "ds");
Assert.Contains(dsOverride.AssetReferences, entry => entry.Name == "ModelReference" && entry.Reference.RelativePath == "models/games/split_play/golden_coin_ds.hasset");
Assert.DoesNotContain(dsOverride.AssetReferences, entry => entry.Name == "Materials[0]");
```

- [ ] **Step 3: Run the focused asset-generation tests to verify they pass**

Run:

```powershell
dotnet test C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests --filter "FullyQualifiedName~SplitPlayGoldenCoinAssetGenerationTests"
```

Expected:

- `SplitPlayGoldenCoinAssetGenerationTests` passes

- [ ] **Step 4: Commit the blueprint authoring**

```bash
git -C C:\dev\helprojs\demodisc add ^
  assets/codebase/game.tools/SplitPlayGoldenCoinAssetGenerator.cs ^
  assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: generate Split Play coin blueprint"
```

### Task 5: Wire gameplay generation to emit the coin assets

**Files:**
- Modify: `assets/codebase/game.tools/GameSceneGenerator.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinSourceTests.cs`

- [ ] **Step 1: Add the generator call to gameplay scene generation**

```csharp
public void Generate(string projectRootPath) {
    if (string.IsNullOrWhiteSpace(projectRootPath)) {
        throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
    }

    TiltTrialPlayerSphereMarbleMaterialFactory materialFactory = new TiltTrialPlayerSphereMarbleMaterialFactory();
    materialFactory.WriteMaterialAsset(projectRootPath);

    SplitPlayGoldenCoinAssetGenerator splitPlayGoldenCoinAssetGenerator = new SplitPlayGoldenCoinAssetGenerator();
    splitPlayGoldenCoinAssetGenerator.Generate(projectRootPath);

    RenderingSceneAssetPreparationService assetPreparationService = new RenderingSceneAssetPreparationService();
    RenderingSceneGenerationAssets assets = assetPreparationService.Prepare(projectRootPath);
    GameSceneFactory factory = new GameSceneFactory(assets);
    GeneratedAuthoringSceneWriteService sceneWriteService = new GeneratedAuthoringSceneWriteService(ScriptTypeResolverValue);
    GeneratedAuthoringSceneDefinition tiltTrialLevelSelectScene = factory.CreateTiltTrialLevelSelectScene();
    sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);

    IReadOnlyList<GeneratedAuthoringSceneDefinition> tiltTrialLevelScenes = factory.CreateTiltTrialLevelScenes();
    for (int index = 0; index < tiltTrialLevelScenes.Count; index++) {
        sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelScenes[index]);
    }
}
```

- [ ] **Step 2: Run the source-wiring tests to verify they pass**

Run:

```powershell
dotnet test C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests --filter "FullyQualifiedName~SplitPlayGoldenCoinSourceTests"
```

Expected:

- `SplitPlayGoldenCoinSourceTests` passes

- [ ] **Step 3: Commit the generator wiring**

```bash
git -C C:\dev\helprojs\demodisc add ^
  assets/codebase/game.tools/GameSceneGenerator.cs ^
  assets/codebase/game.tools.tests/SplitPlayGoldenCoinSourceTests.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: wire Split Play coin asset generation into gameplay output"
```

### Task 6: Run the final verification slice and inspect authored outputs

**Files:**
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinAssetGenerationTests.cs`
- Test: `assets/codebase/game.tools.tests/SplitPlayGoldenCoinSourceTests.cs`

- [ ] **Step 1: Run the full focused Split Play test slice**

Run:

```powershell
dotnet test C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests --filter "FullyQualifiedName~SplitPlayGoldenCoin" -v minimal
```

Expected:

- all Split Play golden coin tests pass

- [ ] **Step 2: Regenerate the city gameplay assets**

Run:

```powershell
dotnet run --project C:\dev\helprojs\demodisc\assets\codebase\game.tools -- generate-game-scenes
```

Expected:

- command completes successfully
- the following assets exist:
  - `C:\dev\helprojs\demodisc\assets\models\games\split_play\golden_coin.hasset`
  - `C:\dev\helprojs\demodisc\assets\models\games\split_play\golden_coin_ds.hasset`
  - `C:\dev\helprojs\demodisc\assets\materials\games\split_play\GoldenCoin.hasset`
  - `C:\dev\helprojs\demodisc\assets\blueprints\games\split_play\GoldenCoin.hblueprint`

- [ ] **Step 3: Spot-check the generated blueprint asset**

Run:

```powershell
@'
using System;
using System.IO;
using helengine;

using FileStream stream = File.OpenRead(@"C:\dev\helprojs\demodisc\assets\blueprints\games\split_play\GoldenCoin.hblueprint");
BlueprintAsset blueprint = (BlueprintAsset)AssetSerializer.Deserialize(stream);
Console.WriteLine(blueprint.Id);
Console.WriteLine(blueprint.AssetReferences.Length);
Console.WriteLine(blueprint.RootEntity.Name);
'@ | dotnet-script
```

Expected:

- blueprint id prints `blueprints/games/split_play/GoldenCoin.hblueprint`
- asset reference count prints `3`
- root entity name prints `GoldenCoin`

- [ ] **Step 4: Commit the generated outputs if this repo tracks them**

```bash
git -C C:\dev\helprojs\demodisc add ^
  assets/models/games/split_play/golden_coin.hasset ^
  assets/models/games/split_play/golden_coin_ds.hasset ^
  assets/materials/games/split_play/GoldenCoin.hasset ^
  assets/blueprints/games/split_play/GoldenCoin.hblueprint
git -C C:\dev\helprojs\demodisc commit -m "feat: add generated Split Play coin assets"
```

## Self-Review

- Spec coverage:
  - common and DS model assets: Tasks 2-4
  - shared gold material: Task 3
  - single blueprint with `ds` mesh override: Task 4
  - generator wiring into city gameplay output: Task 5
  - verification of serialized asset result and path contract: Tasks 1 and 6
- Placeholder scan:
  - no `TODO`, `TBD`, or "similar to previous task" placeholders remain
  - each code-changing step contains explicit code
- Type consistency:
  - `SplitPlayGoldenCoinAssetGenerator`, `SplitPlayGeneratedModelAssetWriteService`, `SplitPlayGeneratedBlueprintAssetWriteService`, and `SplitPlayAssetCatalog` are used consistently across tasks
  - common and DS model relative paths match the approved spec everywhere
