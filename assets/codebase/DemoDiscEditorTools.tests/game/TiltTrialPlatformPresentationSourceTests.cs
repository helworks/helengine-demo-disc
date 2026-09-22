using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies that Tilt Trial platform presentation is authored as cook-time Blueprints and semantic actions.
    /// </summary>
    public sealed class TiltTrialPlatformPresentationSourceTests {
        /// <summary>
        /// Ensures the committed handheld Blueprint starts with only the prompt visible over the camera clear color.
        /// </summary>
        [Fact]
        public void Handheld_pre_start_screen_contains_only_the_start_prompt_ui() {
            string blueprintPath = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "blueprints", "games", "tilt", "TiltTrialHandheldPresentation.hblueprint");
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

            string roleComponentTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(DemoDisc.TiltPlay.TiltTrialPresentationRoleComponent));
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
            string blueprintPath = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "blueprints", "games", "tilt", "TiltTrialHandheldPresentation.hblueprint");
            using FileStream stream = File.OpenRead(blueprintPath);
            global::helengine.BlueprintAsset blueprint = Assert.IsType<global::helengine.BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] entities = EnumerateEntities(blueprint.RootEntity).ToArray();
            SceneEntityAsset resultsOverlay = Assert.Single(entities, entity => entity.Name == "TiltTrialResultsOverlay");
            string roundedRectTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(RoundedRectComponent));
            string spriteTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(SpriteComponent));
            string textTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TextComponent));
            ComponentPersistenceRegistry registry = DemoDisc.EditorTools.GeneratedScenePersistenceRegistryFactory.Create();

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
        /// Ensures the generated handheld selector shows one maximum duration and keeps the medal-threshold text entity hidden.
        /// </summary>
        [Fact]
        public void Handheld_level_selector_presents_only_the_maximum_time() {
            string scenePath = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial_ds.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] entities = sceneAsset.RootEntities.SelectMany(EnumerateEntities).ToArray();
            SceneEntityAsset maximumTimeEntity = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectTimer");
            SceneEntityAsset targetTimesEntity = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectTargetTimes");
            string textTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TextComponent));
            ComponentPersistenceRegistry registry = DemoDisc.EditorTools.GeneratedScenePersistenceRegistryFactory.Create();
            TextComponent maximumTimeText = DeserializeTextComponent(maximumTimeEntity, textTypeId, registry);
            TextComponent targetTimesText = DeserializeTextComponent(targetTimesEntity, textTypeId, registry);

            Assert.Equal("MAX 99.00", maximumTimeText.Text);
            Assert.False(targetTimesEntity.Enabled);
            Assert.Equal(string.Empty, targetTimesText.Text);
        }

        /// <summary>
        /// Ensures every authored Tilt Trial level serializes the debug root so it exists on every single-screen device
        /// in debug builds, never on the dual-screen rigs, and never in release builds.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_keep_debug_root_out_of_release_and_dual_screen() {
            string sceneDirectory = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt");
            string[] scenePaths = Directory.GetFiles(sceneDirectory, "tilt_trial_level_*.helen");

            Assert.NotEmpty(scenePaths);
            foreach (string scenePath in scenePaths) {
                using FileStream stream = File.OpenRead(scenePath);
                SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
                SceneEntityAsset debugRoot = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "TiltTrialPhysicsBoundsDebug"));

                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(debugRoot, "windows", "debug"));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(debugRoot, "ps2", "debug"));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(debugRoot, "ds", "debug"));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(debugRoot, "3ds", "debug"));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(debugRoot, "windows", "release"));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(debugRoot, "ps2", "release"));
            }
        }

        /// <summary>
        /// Ensures the handheld presentation root is restricted to the Nintendo dual-screen group rather than
        /// enumerating the platforms it should skip, so a platform outside the group never receives it — including
        /// ones the old exclusion list never named and ones added to the project later.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_scope_handheld_presentation_root_to_the_dual_screen_group() {
            string sceneDirectory = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt");
            string[] scenePaths = Directory.GetFiles(sceneDirectory, "tilt_trial_level_*.helen");
            string[] excludedPlatformIds = ["windows", "ps2", "n64", "ps1", "dc", "ps3", "x360", "psp", "psvita", "switch"];

            Assert.NotEmpty(scenePaths);
            foreach (string scenePath in scenePaths) {
                using FileStream stream = File.OpenRead(scenePath);
                SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
                SceneEntityAsset handheldRoot = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "TiltTrialHandheldPresentation"));

                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(handheldRoot, "ds"));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(handheldRoot, "3ds"));
                for (int index = 0; index < excludedPlatformIds.Length; index++) {
                    Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(handheldRoot, excludedPlatformIds[index]));
                }
            }
        }

        /// <summary>
        /// Ensures every authored Tilt Trial level carries the console presentation root, that it survives on
        /// single-screen devices in both build configs, and that the dual-screen rigs never receive it.
        /// </summary>
        [Fact]
        public void Authored_gameplay_scenes_keep_console_presentation_off_the_dual_screen_rigs() {
            string sceneDirectory = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt");
            string[] scenePaths = Directory.GetFiles(sceneDirectory, "tilt_trial_level_*.helen");

            Assert.NotEmpty(scenePaths);
            foreach (string scenePath in scenePaths) {
                using FileStream stream = File.OpenRead(scenePath);
                SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
                SceneEntityAsset consoleRoot = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "TiltTrialConsolePresentation"));

                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(consoleRoot, "windows", "debug"));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(consoleRoot, "windows", "release"));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(consoleRoot, "ps2", "release"));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(consoleRoot, "ds"));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(consoleRoot, "3ds"));
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
