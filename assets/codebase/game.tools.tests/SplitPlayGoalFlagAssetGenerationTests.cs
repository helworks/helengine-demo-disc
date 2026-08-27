using helengine;
using helengine.editor;
using city.game.tools;
using city.rendering.tools;
using System.Linq;

namespace city.tests {
    /// <summary>
    /// Verifies Split Play support asset generation writes one common goal-flag model, one DS override model, shared materials, and one reusable blueprint.
    /// </summary>
    public sealed class SplitPlayGoalFlagAssetGenerationTests : IDisposable {
        readonly string ProjectRootPath;

        public SplitPlayGoalFlagAssetGenerationTests() {
            ProjectRootPath = Path.Combine(Path.GetTempPath(), "city-split-play-goal-flag-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(ProjectRootPath, "assets"));
        }

        public void Dispose() {
            if (Directory.Exists(ProjectRootPath)) {
                Directory.Delete(ProjectRootPath, true);
            }
        }

        [Fact]
        public void Generate_writes_goal_flag_models_materials_and_blueprint_with_ds_model_override() {
            SplitPlayGoalFlagAssetGenerator generator = new SplitPlayGoalFlagAssetGenerator(
                new TestEditorProjectAssetAuthoringService(ProjectRootPath));

            generator.Generate(ProjectRootPath);

            string commonModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "tilt", "goal_flag.hasset");
            string dsModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "tilt", "goal_flag_ds.hasset");
            string poleMaterialPath = Path.Combine(ProjectRootPath, "assets", "materials", "games", "tilt", "GoalFlagPole.hasset");
            string bannerMaterialPath = Path.Combine(ProjectRootPath, "assets", "materials", "games", "tilt", "GoalFlagBanner.hasset");
            string blueprintPath = Path.Combine(ProjectRootPath, "assets", "blueprints", "games", "tilt", "GoalFlag.hblueprint");

            Assert.True(File.Exists(commonModelPath));
            Assert.True(File.Exists(dsModelPath));
            Assert.True(File.Exists(poleMaterialPath));
            Assert.True(File.Exists(bannerMaterialPath));
            Assert.True(File.Exists(blueprintPath));

            ModelAsset commonModel;
            using (FileStream stream = File.OpenRead(commonModelPath)) {
                commonModel = Assert.IsType<ModelAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            }

            ModelAsset dsModel;
            using (FileStream stream = File.OpenRead(dsModelPath)) {
                dsModel = Assert.IsType<ModelAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            }

            BlueprintAsset blueprint;
            using (FileStream stream = File.OpenRead(blueprintPath)) {
                blueprint = Assert.IsType<BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            }

            Assert.NotNull(commonModel.Indices16);
            Assert.NotNull(dsModel.Indices16);
            Assert.True(commonModel.Positions.Length > dsModel.Positions.Length);
            Assert.True(commonModel.Indices16.Length > dsModel.Indices16.Length);
            AssertAllTriangleWindingsAgreeWithNormals(commonModel);
            AssertAllTriangleWindingsAgreeWithNormals(dsModel);
            Assert.Equal(2, commonModel.Submeshes.Length);
            Assert.Equal(2, dsModel.Submeshes.Length);
            Assert.Equal("40000000000000000000000000000004", commonModel.AuthoringAssetId);
            Assert.Equal("40000000000000000000000000000005", dsModel.AuthoringAssetId);
            Assert.Equal("blueprints/games/tilt/GoalFlag.hblueprint", blueprint.Id);
            Assert.Equal("40000000000000000000000000000007", blueprint.AuthoringAssetId);
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/tilt/goal_flag.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/tilt/goal_flag_ds.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/tilt/GoalFlagPole.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/tilt/GoalFlagBanner.hasset");

            Assert.NotNull(blueprint.RootEntity);
            SceneEntityAsset root = blueprint.RootEntity;
            Assert.Equal(5, root.Components.Length);
            Assert.Contains(root.Components, record => record.ComponentTypeId.Contains("TiltTrialGoalComponent", StringComparison.Ordinal));
            Assert.Contains(root.Components, record => record.ComponentTypeId.Contains("SceneEntityTriggerObserverComponent", StringComparison.Ordinal)
                && record.ComponentKey == SplitPlayGoalFlagAssetGenerator.TriggerObserverComponentKey);
            Assert.Contains(root.Components, record => record.ComponentTypeId.Contains("RigidBody3DComponent", StringComparison.Ordinal));
            Assert.Contains(root.Components, record => record.ComponentTypeId.Contains("BoxCollider3DComponent", StringComparison.Ordinal));
            SceneComponentAssetRecord meshComponent = root.Components[0];
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
            Assert.Equal("models/games/tilt/goal_flag_ds.hasset", dsModelReference.RelativePath);
            Assert.False(dsOverride.TryGetAssetReference("Materials[0]", out _));
            Assert.False(dsOverride.TryGetAssetReference("Materials[1]", out _));
            Assert.Equal(2, restoredMeshComponent.Materials.Length);

            byte[] firstModelBytes = File.ReadAllBytes(commonModelPath);
            byte[] firstBlueprintBytes = File.ReadAllBytes(blueprintPath);
            generator.Generate(ProjectRootPath);
            Assert.Equal(firstModelBytes, File.ReadAllBytes(commonModelPath));
            Assert.Equal(firstBlueprintBytes, File.ReadAllBytes(blueprintPath));
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
