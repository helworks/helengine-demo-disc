using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the generated game-scene catalog reuses the runtime Tilt Trial scene ids instead of duplicating string literals.
    /// </summary>
    public sealed class GameSceneCatalogSourceTests {
        [Fact]
        public void Scene_catalog_reuses_runtime_tilt_trial_scene_ids() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\GameSceneCatalog.cs");

            Assert.Contains("global::DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId", source, StringComparison.Ordinal);
            Assert.Contains("global::DemoDisc.TiltPlay.TiltTrialSceneIds.HandheldLevelSelectSceneId", source, StringComparison.Ordinal);
            Assert.Contains("global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId", source, StringComparison.Ordinal);
            Assert.Contains("global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level05SceneId", source, StringComparison.Ordinal);
        }
    }
}
