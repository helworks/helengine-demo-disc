namespace city.tests {
    /// <summary>
    /// Verifies the generated game-scene catalog reuses the runtime Tilt Trial scene ids instead of duplicating string literals.
    /// </summary>
    public sealed class GameSceneCatalogSourceTests {
        [Fact]
        public void Scene_catalog_reuses_runtime_tilt_trial_scene_ids() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneCatalog.cs");

            Assert.Contains("global::city.game.TiltTrialSceneIds.LevelSelectSceneId", source, StringComparison.Ordinal);
            Assert.Contains("global::city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId", source, StringComparison.Ordinal);
            Assert.Contains("global::city.game.TiltTrialSceneIds.Level01SceneId", source, StringComparison.Ordinal);
            Assert.Contains("global::city.game.TiltTrialSceneIds.Level05SceneId", source, StringComparison.Ordinal);
        }
    }
}
