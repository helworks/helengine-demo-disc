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
