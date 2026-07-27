namespace city.tests {
    /// <summary>
    /// Verifies the generated Tilt Trial scene source emits selectors and presentation Blueprints without rewriting gameplay levels.
    /// </summary>
    public sealed class TiltTrialSceneGenerationSourceTests {
        [Fact]
        public void Game_scene_generator_writes_selectors_and_presentation_blueprints_without_gameplay_levels() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("CreateTiltTrialScene()", source, StringComparison.Ordinal);
            Assert.Contains("sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialGameplayPresentationBlueprintGenerator", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldLevelSelectSceneFactory", source, StringComparison.Ordinal);
            Assert.DoesNotContain("sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelScenes[index]);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_authors_level_settings_and_session_components() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("const string TiltTrialLevelSelectSceneAssetRelativePath = \"scenes/games/tilt/tilt_trial.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("const string TiltTrialGameplaySceneAssetDirectoryRelativePath = \"scenes/games/tilt\";", source, StringComparison.Ordinal);
            Assert.Contains("SceneAssetRelativePath = TiltTrialLevelSelectSceneAssetRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("SceneAssetRelativePath = BuildTiltTrialGameplaySceneAssetRelativePath(levelEntry.SceneId)", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialLevelSettingsComponent", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialSessionComponent", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialConsolePresentationRoot", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialHandheldPresentationRoot", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialPresentationActionComponent", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultRetryButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultExitButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultNextButton", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialLevelSelectComponent", source, StringComparison.Ordinal);
            Assert.Contains("new global::helengine.SceneEntityTriggerObserverComponent()", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialCoinText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"Coins 0/0\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialTargetTimesText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"Targets G18.00 S28.00 B40.00\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialLevelSelectTargetTimes\"", source, StringComparison.Ordinal);
            Assert.Contains("new float3(16f, 8f, 0f), new int2(224, 176)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 56f, 0.1f), new int2(200, 30)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 91f, 0.1f), new int2(200, 30)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 126f, 0.1f), new int2(200, 30)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(320f, 130f, 0f), new int2(640, 380)", source, StringComparison.Ordinal);
            Assert.Contains("new int2(420, 220)", source, StringComparison.Ordinal);
            Assert.Contains("new ReferenceCanvasFitComponent", source, StringComparison.Ordinal);
            Assert.Contains("FindRequiredBoxColliderComponent(entity).IsTrigger = true;", source, StringComparison.Ordinal);
            Assert.Contains("ConfigureTiltTrialGoalTarget(stageRootEntity, playerSphereEntity);", source, StringComparison.Ordinal);
            Assert.Contains("if (parent.Children[childIndex] is EditorEntity childEntity", source, StringComparison.Ordinal);
            Assert.DoesNotContain("child?.Name", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies collectible coin instances use a trigger volume that is twice as tall as its horizontal footprint.
        /// </summary>
        [Fact]
        public void Game_scene_factory_authors_tall_box_trigger_for_collectible_coins() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("new BoxCollider3DComponent {", source, StringComparison.Ordinal);
            Assert.Contains("Size = new float3(1.5f, 3f, 1.5f)", source, StringComparison.Ordinal);
            Assert.Contains("IsTrigger = true", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the Tilt Trial front door is generated as a title shell before the existing level selector.
        /// </summary>
        [Fact]
        public void Tilt_trial_front_door_generates_title_options_and_level_select_panels() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateTiltPlayShellUiEntity()", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayTitlePanel\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TILT PLAY\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayOptionsPanel\"", source, StringComparison.Ordinal);
            Assert.Contains("\"Settings coming soon\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayLevelSelectPanel\"", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltPlayMenuComponent()", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltPlayMenuActionComponent", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 2f, 0.1f)", source, StringComparison.Ordinal);
            Assert.Contains("new int2(size.X - 24, size.Y - 4)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the dedicated Level 1 render test scene reuses the visible course assets without gameplay components.
        /// </summary>
        [Fact]
        public void Level_01_render_test_scene_uses_visual_assets_and_fps_only() {
            string catalogSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneCatalog.cs");
            string generatorSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");
            string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("TiltTrialLevel01RenderTestSceneId", catalogSource, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel01RenderTestScene", generatorSource, StringComparison.Ordinal);
            Assert.Contains("CreateLevel01RenderOnlyCourseBoxEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("GoldenCoinBlueprintRelativePath", factorySource, StringComparison.Ordinal);
            Assert.Contains("GoalFlagBlueprintRelativePath", factorySource, StringComparison.Ordinal);
            Assert.Contains("new FPSComponent", factorySource, StringComparison.Ordinal);
            Assert.Contains("test_scene_tilt_trial_level_01_render.helen", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateLevel01RenderOnlyStageRootEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateLevel01RenderOnlyCoinEntity", factorySource, StringComparison.Ordinal);
      Assert.Contains("CreateLevel01RenderOnlyGoalFlagEntity", factorySource, StringComparison.Ordinal);
      Assert.Contains("new city.rendering.DemoDiscOrbitCameraComponent", factorySource, StringComparison.Ordinal);
      Assert.DoesNotContain("CreateLevel01RenderOnlyCoinEntity(\"Coin01\"", factorySource, StringComparison.Ordinal);
      Assert.DoesNotContain("CreateLevel01RenderOnlyGoalFlagEntity(new float3(1.35f, 0.65f, 16.6f))", factorySource, StringComparison.Ordinal);
  }

        /// <summary>
        /// Ensures gameplay sessions bind and refresh target-time text from the current level metadata.
        /// </summary>
        [Fact]
        public void Gameplay_session_refreshes_target_times_from_current_level() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialSessionComponent.cs");

            Assert.Contains("TargetTimesTextComponent", source, StringComparison.Ordinal);
            Assert.Contains("TryFindNamedEntity(Parent, \"TiltTrialTargetTimesText\")", source, StringComparison.Ordinal);
            Assert.Contains("RefreshTargetTimesText();", source, StringComparison.Ordinal);
            Assert.Contains("CurrentLevel.GoldTimeSeconds", source, StringComparison.Ordinal);
            Assert.Contains("CurrentLevel.SilverTimeSeconds", source, StringComparison.Ordinal);
            Assert.Contains("CurrentLevel.BronzeTimeSeconds", source, StringComparison.Ordinal);
            Assert.Contains("Targets G", source, StringComparison.Ordinal);
        }
    }
}
