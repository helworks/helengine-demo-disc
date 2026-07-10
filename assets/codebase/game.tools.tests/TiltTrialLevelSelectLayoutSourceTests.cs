namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial level-select layout moves the left level list down below the title while keeping the compact row cards.
    /// </summary>
    public sealed class TiltTrialLevelSelectLayoutSourceTests {
        [Fact]
        public void Game_scene_factory_uses_compact_level_select_list_layout() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateRoundedPanelEntity(entity, \"TiltTrialLevelSelectListPanel\", new float3(40f, 92f, 0f), new int2(420, 596)", source, StringComparison.Ordinal);
            Assert.Contains("CreateRoundedPanelEntity(entity, \"TiltTrialLevelSelectDetailsPanel\", new float3(500f, 72f, 0f), new int2(740, 596)", source, StringComparison.Ordinal);
            Assert.Contains("float top = 22f + (index * 94f);", source, StringComparison.Ordinal);
            Assert.Contains("new int2(372, 76)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(20f, 18f, 0.1f)", source, StringComparison.Ordinal);
        }
    }
}
