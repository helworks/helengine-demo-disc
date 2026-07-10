namespace city.tests {
    /// <summary>
    /// Verifies the first Tilt Trial gameplay level now uses a dedicated beginner layout with collectible coins and a finish flag blueprint.
    /// </summary>
    public sealed class TiltTrialLevel01SceneSourceTests {
        [Fact]
        public void Game_scene_factory_authors_dedicated_level_01_layout_with_beginner_collectibles_and_flag() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateStageRootEntity(levelEntry)", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel01StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateGoalFlagEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateCollectibleCoinEntity(", source, StringComparison.Ordinal);
            Assert.Contains("SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialCollectibleCoinComponent()", source, StringComparison.Ordinal);
            Assert.Contains("new BlueprintInstanceComponent", source, StringComparison.Ordinal);
        }
    }
}
