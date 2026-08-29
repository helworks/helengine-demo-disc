using helengine;
using helengine.editor;
using city.game.tools;
using city.rendering.tools;
using System.Linq;

namespace city.tests {
    /// <summary>
    /// Verifies Split Play support asset generation writes one common coin model, one DS override model, one shared material, and one blueprint with the expected model override and collectible behavior.
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
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(ProjectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(ProjectRootPath);
            using EditorAuthoringTransaction transaction = authoringSession.BeginTransaction();
            SplitPlayGoldenCoinAssetGenerator generator = new SplitPlayGoldenCoinAssetGenerator(
                authoringSession,
                transaction);

            generator.Generate(ProjectRootPath);
            transaction.Commit();

            string commonModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "tilt", "golden_coin.hasset");
            string dsModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "tilt", "golden_coin_ds.hasset");
            string materialPath = Path.Combine(ProjectRootPath, "assets", "materials", "games", "tilt", "GoldenCoin.hasset");
            string blueprintPath = Path.Combine(ProjectRootPath, "assets", "blueprints", "games", "tilt", "GoldenCoin.hblueprint");

            Assert.True(File.Exists(commonModelPath));
            Assert.True(File.Exists(dsModelPath));
            Assert.True(File.Exists(materialPath));
            Assert.True(File.Exists(blueprintPath));

            ModelAsset commonModel;
            using (FileStream stream = File.OpenRead(commonModelPath)) {
                commonModel = Assert.IsType<ModelAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            }

            ModelAsset dsModel;
            using (FileStream stream = File.OpenRead(dsModelPath)) {
                dsModel = Assert.IsType<ModelAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            }

            MaterialAssetSettingsService materialSettingsService = new MaterialAssetSettingsService(ProjectRootPath);
            Assert.True(materialSettingsService.TryLoadPlatformSettings(materialPath, "windows", out MaterialAssetProcessorSettings windowsSettings));
            Assert.Equal("#FFE27AFF", windowsSettings.FieldValues["base-color"]);
            Assert.Equal("#FFD54A33", windowsSettings.FieldValues["emissive-color"]);
            Assert.Equal("0.28", windowsSettings.FieldValues["roughness"]);
            Assert.Equal("0.10", windowsSettings.FieldValues["metallic"]);
            Assert.Equal("0.65", windowsSettings.FieldValues["specular"]);
            Assert.Equal("false", windowsSettings.FieldValues["double-sided"]);
            Assert.Equal("false", windowsSettings.FieldValues["casts-shadow"]);
            Assert.Equal("false", windowsSettings.FieldValues["receives-shadow"]);

            ShaderMaterialAsset windowsMaterial = materialSettingsService.LoadMaterialAsset(materialPath, "windows");
            MaterialConstantBufferAsset roughnessBuffer = Assert.Single(
                windowsMaterial.ConstantBuffers,
                constantBuffer => constantBuffer.Name == StandardMaterialRoughnessDefaults.RoughnessBufferName);
            MaterialConstantBufferAsset emissiveBuffer = Assert.Single(
                windowsMaterial.ConstantBuffers,
                constantBuffer => constantBuffer.Name == StandardMaterialEmissiveColorDefaults.EmissiveColorBufferName);
            MaterialConstantBufferAsset metallicBuffer = Assert.Single(
                windowsMaterial.ConstantBuffers,
                constantBuffer => constantBuffer.Name == StandardMaterialMetallicDefaults.MetallicBufferName);
            MaterialConstantBufferAsset specularBuffer = Assert.Single(
                windowsMaterial.ConstantBuffers,
                constantBuffer => constantBuffer.Name == StandardMaterialSpecularDefaults.SpecularBufferName);

            Assert.Equal(StandardMaterialRoughnessDefaults.CreateConstantBufferData(0.28f), roughnessBuffer.Data);
            Assert.Equal(StandardMaterialEmissiveColorDefaults.CreateConstantBufferData(new float4(1f, 213f / 255f, 74f / 255f, 51f / 255f)), emissiveBuffer.Data);
            Assert.Equal(StandardMaterialMetallicDefaults.CreateConstantBufferData(0.10f), metallicBuffer.Data);
            Assert.Equal(StandardMaterialSpecularDefaults.CreateConstantBufferData(0.65f), specularBuffer.Data);

            BlueprintAsset blueprint;
            using (FileStream stream = File.OpenRead(blueprintPath)) {
                blueprint = Assert.IsType<BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            }

            Assert.NotNull(commonModel.Indices16);
            Assert.NotNull(dsModel.Indices16);
            Assert.True(commonModel.Positions.Length > dsModel.Positions.Length);
            Assert.True(commonModel.Indices16.Length > dsModel.Indices16.Length);
            Assert.Equal(commonModel.BoundsMin.Z, dsModel.BoundsMin.Z);
            Assert.Equal(commonModel.BoundsMax.Z, dsModel.BoundsMax.Z);
            Assert.Equal("40000000000000000000000000000002", commonModel.AuthoringAssetId);
            Assert.Equal("40000000000000000000000000000003", dsModel.AuthoringAssetId);
            Assert.Contains(
                commonModel.Normals,
                normal => normal.Z > 0.8f);
            Assert.Contains(
                commonModel.Normals,
                normal => normal.Z < -0.8f);
            AssertAllTriangleWindingsAgreeWithNormals(commonModel);
            Assert.Equal("blueprints/games/tilt/GoldenCoin.hblueprint", blueprint.Id);
            Assert.Equal("40000000000000000000000000000006", blueprint.AuthoringAssetId);
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/tilt/golden_coin.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/tilt/golden_coin_ds.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/tilt/GoldenCoin.hasset");

            Assert.NotNull(blueprint.RootEntity);
            SceneEntityAsset meshRoot = blueprint.RootEntity;
            Assert.Equal(6, meshRoot.Components.Count());
            Assert.Contains(meshRoot.Components, record => record.ComponentTypeId.Contains("SceneEntityTriggerObserverComponent", StringComparison.Ordinal)
                && record.ComponentKey == SplitPlayGoldenCoinAssetGenerator.TriggerObserverComponentKey);
            Assert.Contains(meshRoot.Components, record => record.ComponentTypeId.Contains("RigidBody3DComponent", StringComparison.Ordinal));
            Assert.Contains(meshRoot.Components, record => record.ComponentTypeId.Contains("BoxCollider3DComponent", StringComparison.Ordinal));
            SceneComponentAssetRecord meshComponent = meshRoot.Components[0];
            ComponentPlatformOverridePayloadService overridePayloadService = new ComponentPlatformOverridePayloadService();
            IReadOnlyList<EntityComponentPlatformOverrideState> overrideStates = overridePayloadService.ReadOverrideStates(meshComponent);
            EntityComponentPlatformOverrideState dsOverride = Assert.Single(overrideStates, state => state.PlatformId == "ds");
            SceneComponentAssetRecord unwrappedMeshComponent = overridePayloadService.UnwrapBaseRecord(meshComponent);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            MeshComponent restoredMeshComponent = Assert.IsType<MeshComponent>(
                registry.GetDescriptor(unwrappedMeshComponent.ComponentTypeId).DeserializeComponent(
                    unwrappedMeshComponent,
                    new EntitySaveComponent(),
                    null));

            Assert.True(dsOverride.TryGetAssetReference("Model", out SceneAssetReference dsModelReference));
            Assert.Equal("models/games/tilt/golden_coin_ds.hasset", dsModelReference.RelativePath);
            Assert.False(dsOverride.TryGetAssetReference("Materials[0]", out _));
            Assert.Single(restoredMeshComponent.Materials);

            SceneComponentAssetRecord collectibleComponent = meshRoot.Components[1];
            Assert.Equal(1, collectibleComponent.ComponentIndex);
            Assert.Contains("TiltTrialCollectibleCoinComponent", collectibleComponent.ComponentTypeId, StringComparison.Ordinal);

            SceneComponentAssetRecord idleMotionComponent = meshRoot.Components[2];
            Assert.Equal(2, idleMotionComponent.ComponentIndex);
            Assert.Contains("SplitPlayIdleMotionComponent", idleMotionComponent.ComponentTypeId, StringComparison.Ordinal);

            byte[] firstModelBytes = File.ReadAllBytes(commonModelPath);
            byte[] firstBlueprintBytes = File.ReadAllBytes(blueprintPath);
            using EditorAuthoringTransaction secondTransaction = authoringSession.BeginTransaction();
            new SplitPlayGoldenCoinAssetGenerator(authoringSession, secondTransaction).Generate(ProjectRootPath);
            secondTransaction.Commit();
            Assert.Equal(firstModelBytes, File.ReadAllBytes(commonModelPath));
            Assert.Equal(firstBlueprintBytes, File.ReadAllBytes(blueprintPath));
        }

        [Fact]
        public void Committed_project_golden_coin_material_matches_the_generated_windows_gold_settings() {
            string materialPath = @"C:\dev\helprojs\demodisc\assets\materials\games\tilt\GoldenCoin.hasset";

            string committedProjectRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(materialPath), "..", "..", "..", ".."));
            MaterialAssetSettingsService materialSettingsService = new MaterialAssetSettingsService(committedProjectRoot);
            Assert.True(materialSettingsService.TryLoadPlatformSettings(materialPath, "windows", out MaterialAssetProcessorSettings windowsSettings));
            Assert.Equal("#FFE27AFF", windowsSettings.FieldValues["base-color"]);
            Assert.Equal("#FFD54A33", windowsSettings.FieldValues["emissive-color"]);
            Assert.Equal("0.28", windowsSettings.FieldValues["roughness"]);
            Assert.Equal("0.10", windowsSettings.FieldValues["metallic"]);
            Assert.Equal("0.65", windowsSettings.FieldValues["specular"]);
            Assert.Equal("false", windowsSettings.FieldValues["double-sided"]);
            Assert.Equal("false", windowsSettings.FieldValues["casts-shadow"]);
            Assert.Equal("false", windowsSettings.FieldValues["receives-shadow"]);
        }

        static void AssertAllTriangleWindingsAgreeWithNormals(ModelAsset modelAsset) {
            Assert.NotNull(modelAsset.Positions);
            Assert.NotNull(modelAsset.Normals);
            Assert.NotNull(modelAsset.Indices16);

            for (int index = 0; index < modelAsset.Indices16.Length; index += 3) {
                float3 positionA = modelAsset.Positions[modelAsset.Indices16[index]];
                float3 positionB = modelAsset.Positions[modelAsset.Indices16[index + 1]];
                float3 positionC = modelAsset.Positions[modelAsset.Indices16[index + 2]];

                float3 edgeAB = positionB - positionA;
                float3 edgeAC = positionC - positionA;
                float3 triangleNormal = float3.Cross(edgeAB, edgeAC);
                if (triangleNormal.LengthSquared() <= 0.000001f) {
                    continue;
                }

                triangleNormal = float3.Normalize(triangleNormal);

                float3 averagedNormal = modelAsset.Normals[modelAsset.Indices16[index]]
                    + modelAsset.Normals[modelAsset.Indices16[index + 1]]
                    + modelAsset.Normals[modelAsset.Indices16[index + 2]];
                if (averagedNormal.LengthSquared() <= 0.000001f) {
                    continue;
                }

                averagedNormal = float3.Normalize(averagedNormal);
                float alignment = float3.Dot(triangleNormal, averagedNormal);
                Assert.True(
                    alignment > 0.05f,
                    $"Triangle starting at index {index} has winding that disagrees with its vertex normals. Alignment={alignment}.");
            }
        }
    }
}
