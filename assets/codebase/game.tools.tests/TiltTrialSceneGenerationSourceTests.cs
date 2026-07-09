namespace city.tests {
    /// <summary>
    /// Verifies the generated Tilt Trial scene source now emits selector plus scaffolded gameplay levels.
    /// </summary>
    public sealed class TiltTrialSceneGenerationSourceTests {
        [Fact]
        public void Game_scene_generator_writes_selector_and_all_five_levels() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("CreateTiltTrialLevelSelectScene()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevelScenes()", source, StringComparison.Ordinal);
            Assert.Contains("sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_authors_level_settings_and_session_components() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("new city.game.TiltTrialLevelSettingsComponent", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialSessionComponent", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialLevelSelectComponent", source, StringComparison.Ordinal);
            Assert.Contains("new global::helengine.SceneEntityTriggerObserverComponent()", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialCoinText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"Coins 0/0\"", source, StringComparison.Ordinal);
            Assert.Contains("FindRequiredBoxColliderComponent(entity).IsTrigger = true;", source, StringComparison.Ordinal);
            Assert.Contains("ConfigureTiltTrialGoalTarget(stageRootEntity, playerSphereEntity);", source, StringComparison.Ordinal);
            Assert.Contains("if (parent.Children[childIndex] is EditorEntity childEntity", source, StringComparison.Ordinal);
            Assert.DoesNotContain("child?.Name", source, StringComparison.Ordinal);
        }
    }
}
