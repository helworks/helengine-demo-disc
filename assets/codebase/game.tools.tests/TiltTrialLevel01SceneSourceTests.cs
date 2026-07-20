namespace city.tests {
    /// <summary>
    /// Verifies the first Tilt Trial gameplay level now uses a dedicated beginner layout with collectible coins and a finish flag blueprint.
    /// </summary>
    public sealed class TiltTrialLevel01SceneSourceTests {
        [Fact]
        public void Game_scene_factory_authors_dedicated_cube_layouts_for_levels_02_through_05() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateTiltTrialLevel02StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel03StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel04StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel05StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel02StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel03StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel04StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel05StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("Level02StartPad", source, StringComparison.Ordinal);
            Assert.Contains("Level03Platform01", source, StringComparison.Ordinal);
            Assert.Contains("Level04Blocker03", source, StringComparison.Ordinal);
            Assert.Contains("Level05Platform04", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_authors_dedicated_level_01_layout_with_beginner_collectibles_and_flag() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateStageRootEntity(levelEntry)", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel01StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateGoalFlagEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateGoalPadEntity(new float3(1.35f, 1.05f, 16.95f), new float3(2f, 2f, 2f));", source, StringComparison.Ordinal);
            Assert.Contains("CreateCollectibleCoinEntity(", source, StringComparison.Ordinal);
            Assert.Contains("SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("new BlueprintInstanceComponent", source, StringComparison.Ordinal);
            Assert.Contains("entity.LocalScale = new float3(0.51f, 0.51f, 0.51f);", source, StringComparison.Ordinal);
        }
    }
}
