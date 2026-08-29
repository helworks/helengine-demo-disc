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
            Assert.Contains("ZombislayerGenerationAssets zombislayerAssets = zombislayerAssetPreparationService.Prepare();", source, StringComparison.Ordinal);
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
            Assert.Contains("AuthoringSession.CreateFileReference(ZombislayerAssetCatalog.EnvironmentModelRelativePath, AssetEntryKind.Model)", source, StringComparison.Ordinal);
            Assert.Contains("AuthoringSession.CreateFileReference(ZombislayerAssetCatalog.WeaponModelRelativePath, AssetEntryKind.Model)", source, StringComparison.Ordinal);
            Assert.Contains("const string GameplaySceneAssetRelativePath = \"scenes/games/zombislayer.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("SceneId = GameSceneCatalog.ZombislayerSceneId", source, StringComparison.Ordinal);
            Assert.Contains("SceneAssetRelativePath = GameplaySceneAssetRelativePath", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures only the canonical current authored path is registered for the runtime Zombislayer scene id.
        /// </summary>
        [Fact]
        public void Zombislayer_scene_identity_catalog_uses_only_the_canonical_current_path() {
            const string canonicalPath = "scenes/games/zombislayer.helen";
            const string runtimeSceneId = "zombislayer";
            const string expectedIdentity = "10000000000000000000000000000038";

            Assert.Equal(expectedIdentity, global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(canonicalPath));
            Assert.Throws<InvalidOperationException>(() => global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(runtimeSceneId));
            Assert.Throws<InvalidOperationException>(() => global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity("zombislayer.helen"));
        }

        /// <summary>
        /// Loads the generated current-format Zombislayer scene and verifies its embedded identity and authored model references.
        /// </summary>
        [Fact]
        public void Generated_zombislayer_scene_loads_from_the_canonical_current_path() {
            const string scenePath = @"C:\dev\helprojs\demodisc\assets\scenes\games\zombislayer.helen";
            const string sidecarPath = scenePath + ".hmeta";
            const string expectedIdentity = "10000000000000000000000000000038";

            Assert.True(File.Exists(scenePath), $"Expected generated scene '{scenePath}'.");
            Assert.False(File.Exists(sidecarPath), $"Current native scene must not have a sidecar '{sidecarPath}'.");
            using FileStream stream = File.OpenRead(scenePath);
            byte[] header = new byte[4];
            stream.ReadExactly(header);
            Assert.Equal("HELE", global::System.Text.Encoding.ASCII.GetString(header));
            stream.Position = 0;
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));

            Assert.Equal(expectedIdentity, scene.AuthoringAssetId);
            Assert.Equal("zombislayer", global::city.game.ZombislayerSceneIds.GameplaySceneId);
            Assert.Contains(scene.AssetReferences, reference => string.Equals(
                reference.RelativePath,
                global::city.game.tools.ZombislayerAssetCatalog.EnvironmentModelRelativePath,
                StringComparison.OrdinalIgnoreCase));
            Assert.Contains(scene.AssetReferences, reference => string.Equals(
                reference.RelativePath,
                global::city.game.tools.ZombislayerAssetCatalog.WeaponModelRelativePath,
                StringComparison.OrdinalIgnoreCase));
        }
    }
}
