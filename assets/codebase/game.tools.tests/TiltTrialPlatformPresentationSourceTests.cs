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
            string source = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "TiltTrialGameplayPresentationBlueprintGenerator.cs"));

            Assert.Contains("TiltTrialConsolePresentation.hblueprint", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldPresentation.hblueprint", source, StringComparison.Ordinal);
            Assert.Contains("WriteNativeBlueprint", source, StringComparison.Ordinal);
            Assert.DoesNotContain("BlueprintSaveService", source, StringComparison.Ordinal);
            Assert.DoesNotContain(".helen", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures platform-specific controls target semantic session actions rather than UI child positions.
        /// </summary>
        [Fact]
        public void Handheld_presentation_uses_serialized_semantic_action_bridges() {
            string source = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs"));

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
            string source = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game", "TiltTrialSessionComponent.cs"));

            Assert.Contains("TiltTrialResultRetryButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultExitButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultNextButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultRetryButtonLabel", source, StringComparison.Ordinal);
            Assert.Contains("TryFindRoundedRectComponent", source, StringComparison.Ordinal);
            Assert.Contains("TryFindResultButtonLabel", source, StringComparison.Ordinal);
            Assert.Contains("ApplyResultButtonSelection", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the runtime session resolves its presentation-owned controls by stable names.
        /// </summary>
        [Fact]
        public void Session_component_does_not_depend_on_presentation_child_order() {
            string source = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game", "TiltTrialSessionComponent.cs"));

            Assert.Contains("TryFindNamedEntity(Parent, \"TiltTrialTimerText\")", source, StringComparison.Ordinal);
            Assert.Contains("TryFindNamedEntity(Parent, \"TiltTrialResultsOverlay\")", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TryFindChildEntity(Parent, 0)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the handheld gameplay controller owns the game-specific bottom-screen presentation.
        /// </summary>
        [Fact]
        public void Handheld_gameplay_controller_owns_bottom_screen_presentation() {
            string factorySource = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs"));
            string sessionSource = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game", "TiltTrialSessionComponent.cs"));
            sessionSource = sessionSource.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("CreateHandheldGameplayControllerEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("root.AddChild(CreateHandheldGameplayControllerEntity(levelEntry));", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateHandheldGameplayBottomScreenCameraEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldGameplayBottomScreenRoot", factorySource, StringComparison.Ordinal);
            Assert.Contains("controllerEntity.AddComponent(new city.game.TiltTrialSessionComponent());", factorySource, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateHandheldBottomUiEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("if (ResultsBodyTextComponent == null)", sessionSource, StringComparison.Ordinal);
            Assert.DoesNotContain("missingDependencies.Add(\"timer text\")", sessionSource, StringComparison.Ordinal);
            Assert.DoesNotContain("missingDependencies.Add(\"target times text\")", sessionSource, StringComparison.Ordinal);
            Assert.Contains("if (TimerTextComponent == null) {\n                return;\n            }", sessionSource, StringComparison.Ordinal);
            Assert.Contains("if (CoinTextComponent == null) {\n                return;\n            }", sessionSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the committed handheld Blueprint starts with only the prompt visible over the camera clear color.
        /// </summary>
        [Fact]
        public void Handheld_pre_start_screen_contains_only_the_start_prompt_ui() {
            string blueprintPath = global::city.testing.DemoDiscTestProject.GetPath("assets", "blueprints", "games", "tilt", "TiltTrialHandheldPresentation.hblueprint");
            using FileStream stream = File.OpenRead(blueprintPath);
            global::helengine.BlueprintAsset blueprint = Assert.IsType<global::helengine.BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] entities = EnumerateEntities(blueprint.RootEntity).ToArray();

            SceneEntityAsset gameplayPanel = Assert.Single(entities, entity => entity.Name == "TiltTrialHandheldGameplayPanel");
            SceneEntityAsset startOverlay = Assert.Single(entities, entity => entity.Name == "TiltTrialStartOverlay");
            SceneEntityAsset resultsOverlay = Assert.Single(entities, entity => entity.Name == "TiltTrialResultsOverlay");
            SceneEntityAsset failOverlay = Assert.Single(entities, entity => entity.Name == "TiltTrialFailOverlay");

            Assert.False(gameplayPanel.Enabled);
            Assert.True(startOverlay.Enabled);
            Assert.False(resultsOverlay.Enabled);
            Assert.False(failOverlay.Enabled);

            string roleComponentTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(city.game.TiltTrialPresentationRoleComponent));
            SceneComponentAssetRecord startOverlayComponent = Assert.Single(startOverlay.Components);
            Assert.Equal(roleComponentTypeId, startOverlayComponent.ComponentTypeId);
            Assert.Equal(
                new[] { "TiltTrialStartPromptIcon", "TiltTrialStartPromptPrefixText", "TiltTrialStartPromptSuffixText" },
                startOverlay.Children.Select(child => child.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray());
        }

        /// <summary>
        /// Ensures the handheld results screen uses the working level-selector button presentation without an outer panel.
        /// </summary>
        [Fact]
        public void Handheld_results_screen_uses_background_swap_buttons_without_an_outer_panel() {
            string blueprintPath = global::city.testing.DemoDiscTestProject.GetPath("assets", "blueprints", "games", "tilt", "TiltTrialHandheldPresentation.hblueprint");
            using FileStream stream = File.OpenRead(blueprintPath);
            global::helengine.BlueprintAsset blueprint = Assert.IsType<global::helengine.BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] entities = EnumerateEntities(blueprint.RootEntity).ToArray();
            SceneEntityAsset resultsOverlay = Assert.Single(entities, entity => entity.Name == "TiltTrialResultsOverlay");
            string roundedRectTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(RoundedRectComponent));
            string spriteTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(SpriteComponent));
            string textTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TextComponent));
            ComponentPersistenceRegistry registry = city.rendering.tools.GeneratedScenePersistenceRegistryFactory.Create();

            Assert.DoesNotContain(resultsOverlay.Components, component => component.ComponentTypeId == roundedRectTypeId);
            string[] expectedButtonNames = [
                "TiltTrialResultNextButton",
                "TiltTrialResultRetryButton",
                "TiltTrialResultExitButton"
            ];
            string[] expectedButtonLabels = ["NEXT", "RETRY", "BACK TO MENU"];
            for (int buttonIndex = 0; buttonIndex < expectedButtonNames.Length; buttonIndex++) {
                SceneEntityAsset button = Assert.Single(resultsOverlay.Children, child => child.Name == expectedButtonNames[buttonIndex]);
                Assert.Contains(button.Components, component => component.ComponentTypeId == roundedRectTypeId);
                Assert.DoesNotContain(button.Components, component => component.ComponentTypeId == spriteTypeId);
                SceneEntityAsset label = Assert.Single(button.Children, child => child.Name == expectedButtonNames[buttonIndex] + "Label");
                SceneComponentAssetRecord labelRecord = Assert.Single(label.Components, component => component.ComponentTypeId == textTypeId);
                TextComponent labelComponent = Assert.IsType<TextComponent>(
                    registry.GetDescriptor(labelRecord.ComponentTypeId).DeserializeComponent(
                        labelRecord,
                        new EntitySaveComponent(),
                        null));

                Assert.Equal(expectedButtonLabels[buttonIndex], labelComponent.Text);
            }
        }

        /// <summary>
        /// Ensures the DS start-prompt sprite samples its complete texture because the OBJ path cannot crop SourceRect.
        /// </summary>
        [Fact]
        public void Nintendo_ds_start_prompt_uses_the_full_texture_source_rect() {
            string source = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs"));
            source = source.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains(
                "overrideComponent.SourceRect = string.Equals(platformId, NintendoDsPlatformId, StringComparison.Ordinal)\n                ? new float4(0f, 0f, 1f, 1f)\n                : resolvedIcon.SourceRect;",
                source,
                StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the generated handheld selector shows one maximum duration and keeps the medal-threshold text entity hidden.
        /// </summary>
        [Fact]
        public void Handheld_level_selector_presents_only_the_maximum_time() {
            string scenePath = global::city.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial_ds.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] entities = sceneAsset.RootEntities.SelectMany(EnumerateEntities).ToArray();
            SceneEntityAsset maximumTimeEntity = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectTimer");
            SceneEntityAsset targetTimesEntity = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectTargetTimes");
            string textTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TextComponent));
            ComponentPersistenceRegistry registry = city.rendering.tools.GeneratedScenePersistenceRegistryFactory.Create();
            TextComponent maximumTimeText = DeserializeTextComponent(maximumTimeEntity, textTypeId, registry);
            TextComponent targetTimesText = DeserializeTextComponent(targetTimesEntity, textTypeId, registry);

            Assert.Equal("MAX 99.00", maximumTimeText.Text);
            Assert.False(targetTimesEntity.Enabled);
            Assert.Equal(string.Empty, targetTimesText.Text);
        }

        /// <summary>
        /// Ensures the Windows-only physics bounds debug root is excluded from handheld scene cooks.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_exclude_windows_only_debug_root_from_handheld_platforms() {
            string source = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "TiltTrialGameplayPresentationAttachmentService.cs"));

            Assert.Contains("TiltTrialPhysicsBoundsDebug", source, StringComparison.Ordinal);
            Assert.Contains("PlatformId = \"ds\", Exists = false", source, StringComparison.Ordinal);
            Assert.Contains("PlatformId = \"3ds\", Exists = false", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures every authored Tilt Trial level serializes the Windows-only debug root exclusion.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_scope_windows_only_debug_root_to_windows() {
            string sceneDirectory = global::city.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt");
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

        static IEnumerable<SceneEntityAsset> EnumerateEntities(SceneEntityAsset root) {
            if (root == null) {
                yield break;
            }

            yield return root;
            foreach (SceneEntityAsset child in root.Children ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneEntityAsset descendant in EnumerateEntities(child)) {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Deserializes the single text component owned by one generated scene entity.
        /// </summary>
        /// <param name="entity">Generated entity that owns the text record.</param>
        /// <param name="textTypeId">Stable automatic text-component type id.</param>
        /// <param name="registry">Persistence registry used to materialize the component.</param>
        /// <returns>Deserialized text component.</returns>
        static TextComponent DeserializeTextComponent(SceneEntityAsset entity, string textTypeId, ComponentPersistenceRegistry registry) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (string.IsNullOrWhiteSpace(textTypeId)) {
                throw new ArgumentException("Text component type id must be provided.", nameof(textTypeId));
            } else if (registry == null) {
                throw new ArgumentNullException(nameof(registry));
            }

            SceneComponentAssetRecord record = Assert.Single(entity.Components, component => component.ComponentTypeId == textTypeId);
            return Assert.IsType<TextComponent>(registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(
                record,
                new EntitySaveComponent(),
                null));
        }
    }
}
