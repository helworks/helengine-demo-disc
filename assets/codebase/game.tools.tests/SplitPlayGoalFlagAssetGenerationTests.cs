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
            SplitPlayGoalFlagAssetGenerator generator = new SplitPlayGoalFlagAssetGenerator();

            generator.Generate(ProjectRootPath);

            string commonModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "split_play", "goal_flag.hasset");
            string dsModelPath = Path.Combine(ProjectRootPath, "assets", "models", "games", "split_play", "goal_flag_ds.hasset");
            string poleMaterialPath = Path.Combine(ProjectRootPath, "assets", "materials", "games", "split_play", "GoalFlagPole.hasset");
            string bannerMaterialPath = Path.Combine(ProjectRootPath, "assets", "materials", "games", "split_play", "GoalFlagBanner.hasset");
            string blueprintPath = Path.Combine(ProjectRootPath, "assets", "blueprints", "games", "split_play", "GoalFlag.hblueprint");

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
            Assert.Equal(2, commonModel.Submeshes.Length);
            Assert.Equal(2, dsModel.Submeshes.Length);
            Assert.Equal("blueprints/games/split_play/GoalFlag.hblueprint", blueprint.Id);
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/split_play/goal_flag.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/split_play/goal_flag_ds.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/split_play/GoalFlagPole.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/split_play/GoalFlagBanner.hasset");

            Assert.NotNull(blueprint.RootEntity);
            SceneEntityAsset root = blueprint.RootEntity;
            SceneComponentAssetRecord meshComponent = Assert.Single(root.Components);
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
            Assert.Equal("models/games/split_play/goal_flag_ds.hasset", dsModelReference.RelativePath);
            Assert.False(dsOverride.TryGetAssetReference("Materials[0]", out _));
            Assert.False(dsOverride.TryGetAssetReference("Materials[1]", out _));
            Assert.Equal(2, restoredMeshComponent.Materials.Length);
        }
    }
}
