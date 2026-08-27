namespace city.tests {
    /// <summary>
    /// Verifies the Zombislayer game-scene generator writes a dedicated gameplay scene backed by imported environment and weapon assets.
    /// </summary>
    public sealed class ZombislayerSceneGenerationSourceTests {
        /// <summary>
        /// Ensures the top-level gameplay scene generator stages Zombislayer assets and writes the dedicated gameplay scene.
        /// </summary>
        [Fact]
        public void Game_scene_generator_writes_zombislayer_gameplay_scene() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("ZombislayerAssetPreparationService zombislayerAssetPreparationService = new ZombislayerAssetPreparationService(AssetAuthoringService);", source, StringComparison.Ordinal);
            Assert.Contains("ZombislayerGenerationAssets zombislayerAssets = zombislayerAssetPreparationService.Prepare(projectRootPath);", source, StringComparison.Ordinal);
            Assert.Contains("ZombislayerSceneFactory zombislayerSceneFactory = new ZombislayerSceneFactory(zombislayerAssets, AssetAuthoringService);", source, StringComparison.Ordinal);
            Assert.Contains("GeneratedAuthoringSceneDefinition zombislayerScene = zombislayerSceneFactory.CreateGameplayScene();", source, StringComparison.Ordinal);
            Assert.Contains("sceneWriteService.WriteScene(projectRootPath, zombislayerScene);", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Zombislayer scene factory authors imported environment and weapon entities together with gameplay-owned runtime components.
        /// </summary>
        [Fact]
        public void Zombislayer_scene_factory_authors_imported_models_session_and_fps_controller() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\ZombislayerSceneFactory.cs");

            Assert.Contains("using city.rendering.tools;", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.ZombislayerSessionComponent()", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.ZombislayerFpsControllerComponent()", source, StringComparison.Ordinal);
            Assert.Contains("\"ZombislayerWeapon\"", source, StringComparison.Ordinal);
            Assert.Contains("\"ZombislayerPauseOverlay\"", source, StringComparison.Ordinal);
            Assert.Contains("AssetAuthoringService.CreateFileReference(ZombislayerAssetCatalog.EnvironmentModelRelativePath, AssetEntryKind.Model)", source, StringComparison.Ordinal);
            Assert.Contains("AssetAuthoringService.CreateFileReference(ZombislayerAssetCatalog.WeaponModelRelativePath, AssetEntryKind.Model)", source, StringComparison.Ordinal);
        }
    }
}
