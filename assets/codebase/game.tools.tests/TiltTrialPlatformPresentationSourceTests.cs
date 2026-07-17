namespace city.tests {
    /// <summary>
    /// Verifies that Tilt Trial platform presentation is authored as cook-time Blueprints and semantic actions.
    /// </summary>
    public sealed class TiltTrialPlatformPresentationSourceTests {
        /// <summary>
        /// Ensures presentation generation writes Blueprint assets instead of gameplay level scenes.
        /// </summary>
        [Fact]
        public void Presentation_generator_writes_only_console_and_handheld_blueprints() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\TiltTrialGameplayPresentationBlueprintGenerator.cs");

            Assert.Contains("TiltTrialConsolePresentation.hblueprint", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldPresentation.hblueprint", source, StringComparison.Ordinal);
            Assert.Contains("BlueprintSaveService", source, StringComparison.Ordinal);
            Assert.DoesNotContain(".helen", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures platform-specific controls target semantic session actions rather than UI child positions.
        /// </summary>
        [Fact]
        public void Handheld_presentation_uses_serialized_semantic_action_bridges() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("TiltTrialPresentationActionComponent", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialSessionAction.LevelSelect", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultRetryButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultExitButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultNextButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialSessionAction.Retry", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialSessionAction.Next", source, StringComparison.Ordinal);
            Assert.Contains("new InteractableComponent", source, StringComparison.Ordinal);
            Assert.Contains("TargetEntityName = \"PlayerSphere\"", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the handheld results menu exposes stable roles for selection-state presentation.
        /// </summary>
        [Fact]
        public void Session_component_binds_handheld_result_button_visuals_by_stable_role() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialSessionComponent.cs");

            Assert.Contains("TiltTrialResultRetryButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultExitButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultNextButton", source, StringComparison.Ordinal);
            Assert.Contains("TryFindRoundedRectComponent", source, StringComparison.Ordinal);
            Assert.Contains("ApplyResultButtonSelection", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the runtime session resolves its presentation-owned controls by stable names.
        /// </summary>
        [Fact]
        public void Session_component_does_not_depend_on_presentation_child_order() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialSessionComponent.cs");

            Assert.Contains("TryFindNamedEntity(Parent, \"TiltTrialTimerText\")", source, StringComparison.Ordinal);
            Assert.Contains("TryFindNamedEntity(Parent, \"TiltTrialResultsOverlay\")", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TryFindChildEntity(Parent, 0)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the handheld gameplay controller owns the game-specific bottom-screen presentation.
        /// </summary>
        [Fact]
        public void Handheld_gameplay_controller_owns_bottom_screen_presentation() {
            string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
            string sessionSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialSessionComponent.cs");

            Assert.Contains("CreateHandheldGameplayControllerEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("root.AddChild(CreateHandheldGameplayControllerEntity(levelEntry));", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateHandheldGameplayBottomScreenCameraEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldGameplayBottomScreenRoot", factorySource, StringComparison.Ordinal);
            Assert.Contains("controllerEntity.AddComponent(new city.game.TiltTrialSessionComponent());", factorySource, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateHandheldBottomUiEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("if (ResultsTitleTextComponent == null || ResultsBodyTextComponent == null)", sessionSource, StringComparison.Ordinal);
            Assert.DoesNotContain("missingDependencies.Add(\"timer text\")", sessionSource, StringComparison.Ordinal);
            Assert.DoesNotContain("missingDependencies.Add(\"target times text\")", sessionSource, StringComparison.Ordinal);
            Assert.Contains("if (TimerTextComponent == null) {\n                return;\n            }", sessionSource, StringComparison.Ordinal);
            Assert.Contains("if (CoinTextComponent == null) {\n                return;\n            }", sessionSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Windows-only physics bounds debug root is excluded from handheld scene cooks.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_exclude_windows_only_debug_root_from_handheld_platforms() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\TiltTrialGameplayPresentationAttachmentService.cs");

            Assert.Contains("TiltTrialPhysicsBoundsDebug", source, StringComparison.Ordinal);
            Assert.Contains("PlatformId = \"ds\", Exists = false", source, StringComparison.Ordinal);
            Assert.Contains("PlatformId = \"3ds\", Exists = false", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures every authored Tilt Trial level serializes the Windows-only debug root exclusion.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_scope_windows_only_debug_root_to_windows() {
            string sceneDirectory = @"C:\dev\helprojs\demodisc\assets\scenes\games\tilt";
            string[] scenePaths = Directory.GetFiles(sceneDirectory, "tilt_trial_level_*.helen");

            Assert.NotEmpty(scenePaths);
            foreach (string scenePath in scenePaths) {
                using FileStream stream = File.OpenRead(scenePath);
                SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
                SceneEntityAsset debugRoot = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "TiltTrialPhysicsBoundsDebug"));

                Assert.Contains(debugRoot.PlatformExistenceOverrides, overrideAsset => overrideAsset.PlatformId == "ds" && !overrideAsset.Exists);
                Assert.Contains(debugRoot.PlatformExistenceOverrides, overrideAsset => overrideAsset.PlatformId == "3ds" && !overrideAsset.Exists);
            }
        }
    }
}
