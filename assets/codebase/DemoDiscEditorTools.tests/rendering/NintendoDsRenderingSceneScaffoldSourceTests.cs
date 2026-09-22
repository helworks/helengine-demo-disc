using DemoDisc.EditorTools;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies that generated Nintendo DS bottom-screen controls retain their authored presentation details.
    /// </summary>
    public sealed class NintendoDsRenderingSceneScaffoldSourceTests {
        /// <summary>
        /// Ensures the shared bottom-screen chrome Blueprint carries both action buttons with palette-free
        /// bodies, visible border overlays, and the dedicated light swatch the light cycle recolors.
        /// </summary>
        [Fact]
        public void Bottom_screen_chrome_blueprint_carries_both_action_buttons() {
            SceneEntityAsset chromeRoot = LoadChromeBlueprintRoot();
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();

            foreach (string buttonName in new[] { "DemoDiscBottomScreenLightButton", "DemoDiscBottomScreenBackButton" }) {
                SceneEntityAsset button = Assert.Single(chromeRoot.Children, child => child.Name == buttonName);
                RoundedRectComponent body = ReadRoundedRect(button, registry);
                Assert.Equal(new byte4(48, 29, 65, 255), body.FillColor);
                Assert.Equal(0f, body.BorderThickness);

                SceneEntityAsset border = Assert.Single(button.Children, child => child.Name == "Border");
                RoundedRectComponent borderRect = ReadRoundedRect(border, registry);
                Assert.Equal(2f, borderRect.BorderThickness);
                Assert.Equal(220, borderRect.RenderOrder2D);
                Assert.Equal(0, borderRect.FillColor.W);
            }

            SceneEntityAsset lightButton = Assert.Single(chromeRoot.Children, child => child.Name == "DemoDiscBottomScreenLightButton");
            SceneEntityAsset swatch = Assert.Single(lightButton.Children, child => child.Name == "DemoDiscBottomScreenLightSwatch");
            RoundedRectComponent swatchRect = ReadRoundedRect(swatch, registry);
            Assert.Equal(16, swatchRect.Size.X);
            Assert.Equal(16, swatchRect.Size.Y);
            // The DS renderer only promotes orders at or above 220 to the foreground sprite priority, and at the
            // base priority the earlier-drawn opaque button body hides the swatch.
            Assert.True(swatchRect.RenderOrder2D >= 220);
        }

        /// <summary>
        /// Ensures the action-button labels read as centered LIGHT and BACK text.
        /// </summary>
        [Fact]
        public void Bottom_screen_chrome_blueprint_labels_are_centered() {
            SceneEntityAsset chromeRoot = LoadChromeBlueprintRoot();
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            string textTypeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TextComponent));

            (string ButtonName, string LabelName, string Text)[] expected = [
                ("DemoDiscBottomScreenLightButton", "DemoDiscBottomScreenLightButtonLabel", "LIGHT"),
                ("DemoDiscBottomScreenBackButton", "DemoDiscBottomScreenBackButtonLabel", "BACK")
            ];

            foreach ((string buttonName, string labelName, string text) in expected) {
                SceneEntityAsset button = Assert.Single(chromeRoot.Children, child => child.Name == buttonName);
                SceneEntityAsset label = Assert.Single(button.Children, child => child.Name == labelName);
                SceneComponentAssetRecord record = Assert.Single(label.Components, component => component.ComponentTypeId == textTypeId);
                // The labels carry a 3DS font-scale override, so the persisted record wraps the base payload.
                SceneComponentAssetRecord baseRecord = new global::helengine.editor.ComponentPlatformOverridePayloadService().UnwrapBaseRecord(record);
                TextComponent labelComponent = Assert.IsType<TextComponent>(
                    registry.GetDescriptor(baseRecord.ComponentTypeId).DeserializeComponent(baseRecord, new EntitySaveComponent(), null));

                Assert.Equal(text, labelComponent.Text);
                Assert.Equal(TextAlignment.Center, labelComponent.Alignment);
            }
        }

        /// <summary>
        /// Ensures a generated showcase authors its content once and varies only the presentation by group:
        /// the content reaches every device, the single-screen presentation stops at the dual-screen rigs, and
        /// the bottom screen exists only there.
        /// </summary>
        [Fact]
        public void Generated_showcase_shares_its_content_and_varies_only_the_presentation() {
            string scenePath = global::DemoDisc.testing.DemoDiscTestProject.GetPath(
                "assets", "scenes", "rendering", "cube_test.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));

            SceneEntityAsset cube = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "CubeTestCube"));
            SceneEntityAsset camera = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "CubeTestCamera"));
            SceneEntityAsset desktopUi = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "CubeTestUi"));
            SceneEntityAsset bottomScreenCamera = Assert.Single(sceneAsset.RootEntities.Where(entity => entity != null && entity.Name == "DemoDiscBottomScreenCamera"));

            // Content and camera are authored once and reach every device.
            foreach (string platformId in new[] { "windows", "ps2", "ds", "3ds" }) {
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(cube, platformId));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(camera, platformId));
            }

            Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(desktopUi, "windows"));
            Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(desktopUi, "ps2"));
            Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(desktopUi, "ds"));
            Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(desktopUi, "3ds"));

            Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(bottomScreenCamera, "ds"));
            Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(bottomScreenCamera, "3ds"));
            Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(bottomScreenCamera, "windows"));
            Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(bottomScreenCamera, "ps2"));
        }

        /// <summary>
        /// Loads the generated shared bottom-screen chrome Blueprint root.
        /// </summary>
        /// <returns>Root entity of the chrome Blueprint.</returns>
        static SceneEntityAsset LoadChromeBlueprintRoot() {
            string blueprintPath = global::DemoDisc.testing.DemoDiscTestProject.GetPath(
                "assets", "blueprints", "handheld", "HandheldBottomScreenChrome.hblueprint");
            using FileStream stream = File.OpenRead(blueprintPath);
            global::helengine.BlueprintAsset blueprint = Assert.IsType<global::helengine.BlueprintAsset>(
                global::helengine.editor.AssetSerializer.Deserialize(stream));
            Assert.NotNull(blueprint.RootEntity);
            return blueprint.RootEntity;
        }

        /// <summary>
        /// Reads the single rounded-rect component owned by one serialized entity.
        /// </summary>
        /// <param name="entity">Entity that owns the rounded rect.</param>
        /// <param name="registry">Registry used to materialize the component.</param>
        /// <returns>Deserialized rounded-rect component.</returns>
        static RoundedRectComponent ReadRoundedRect(SceneEntityAsset entity, ComponentPersistenceRegistry registry) {
            string typeId = global::helengine.editor.AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(RoundedRectComponent));
            SceneComponentAssetRecord record = Assert.Single(entity.Components, component => component.ComponentTypeId == typeId);
            return Assert.IsType<RoundedRectComponent>(
                registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(record, new EntitySaveComponent(), null));
        }
    }
}
