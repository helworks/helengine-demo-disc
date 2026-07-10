namespace city.tests {
    /// <summary>
    /// Verifies gameplay generation invokes the Split Play goal-flag asset generator.
    /// </summary>
    public sealed class SplitPlayGoalFlagSourceTests {
        [Fact]
        public void Game_scene_generator_invokes_split_play_goal_flag_generation() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("SplitPlayGoalFlagAssetGenerator", source, StringComparison.Ordinal);
            Assert.Contains("splitPlayGoalFlagAssetGenerator.Generate(projectRootPath);", source, StringComparison.Ordinal);
        }
    }
}
