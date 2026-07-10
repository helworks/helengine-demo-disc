namespace city.tests {
    /// <summary>
    /// Verifies the generated game-scene catalog reuses the runtime Zombislayer scene id instead of duplicating a string literal.
    /// </summary>
    public sealed class ZombislayerGameSceneCatalogSourceTests {
        /// <summary>
        /// Ensures the generated game-scene catalog points at the runtime-owned Zombislayer scene id.
        /// </summary>
        [Fact]
        public void Scene_catalog_reuses_runtime_zombislayer_scene_id() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneCatalog.cs");

            Assert.Contains("global::city.game.ZombislayerSceneIds.GameplaySceneId", source, StringComparison.Ordinal);
        }
    }
}
