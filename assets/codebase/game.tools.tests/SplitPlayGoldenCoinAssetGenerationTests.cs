using helengine;
using helengine.editor;
using city.game.tools;
using System.Linq;

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
            Assert.Equal(commonModel.BoundsMin.Z, dsModel.BoundsMin.Z);
            Assert.Equal(commonModel.BoundsMax.Z, dsModel.BoundsMax.Z);
            Assert.Equal("blueprints/games/split_play/GoldenCoin.hblueprint", blueprint.Id);
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/split_play/golden_coin.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "models/games/split_play/golden_coin_ds.hasset");
            Assert.Contains(blueprint.AssetReferences, reference => reference.RelativePath == "materials/games/split_play/GoldenCoin.hasset");

            Assert.NotNull(blueprint.RootEntity);
            SceneEntityAsset meshRoot = blueprint.RootEntity;
            Assert.Equal(2, meshRoot.Components.Count());
            SceneComponentAssetRecord meshComponent = meshRoot.Components[0];
            ComponentPlatformOverridePayloadService overridePayloadService = new ComponentPlatformOverridePayloadService();
            IReadOnlyList<EntityComponentPlatformOverrideState> overrideStates = overridePayloadService.ReadOverrideStates(meshComponent);
            EntityComponentPlatformOverrideState dsOverride = Assert.Single(overrideStates, state => state.PlatformId == "ds");

            Assert.True(dsOverride.TryGetAssetReference("Model", out SceneAssetReference dsModelReference));
            Assert.Equal("models/games/split_play/golden_coin_ds.hasset", dsModelReference.RelativePath);
            Assert.False(dsOverride.TryGetAssetReference("Materials[0]", out _));

            SceneComponentAssetRecord idleMotionComponent = meshRoot.Components[1];
            Assert.Equal(1, idleMotionComponent.ComponentIndex);
            Assert.Contains("SplitPlayIdleMotionComponent", idleMotionComponent.ComponentTypeId, StringComparison.Ordinal);
        }
    }
}
