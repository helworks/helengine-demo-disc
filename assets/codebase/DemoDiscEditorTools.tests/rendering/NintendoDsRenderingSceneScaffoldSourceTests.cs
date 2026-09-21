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
        /// Ensures light-cycle updates target the dedicated small swatch instead of recoloring the whole button body.
        /// </summary>
        [Fact]
        public void Light_toggle_targets_named_swatch_instead_of_button_body() {
            string source = File.ReadAllText(global::DemoDisc.testing.DemoDiscTestProject.GetPath(
                "assets", "codebase", "rendering", "NintendoDsLightToggleOverlayComponent.cs"));

            Assert.Contains("roundedRectComponent.Size.X == IndicatorSwatchSize", source, StringComparison.Ordinal);
            Assert.Contains("roundedRectComponent.Size.Y == IndicatorSwatchSize", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the PS2 light toggle uses Circle while the handheld companion keeps its North face-button binding.
        /// </summary>
        [Fact]
        public void Light_toggle_uses_ps2_circle_and_keeps_handheld_north_binding() {
            string sharedSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering\DemoDiscLightToggleComponent.cs");
            string handheldSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs");

            Assert.Contains("IsPs2Platform()", sharedSource, StringComparison.Ordinal);
            Assert.Contains("InputGamepadButton.East", sharedSource, StringComparison.Ordinal);
            Assert.Contains("InputGamepadButton.North", sharedSource, StringComparison.Ordinal);
            Assert.Contains("InputGamepadButton.North", handheldSource, StringComparison.Ordinal);
            Assert.DoesNotContain("InputGamepadButton.RightShoulder", sharedSource, StringComparison.Ordinal);
            Assert.DoesNotContain("InputGamepadButton.RightShoulder", handheldSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the PS2 instruction overlay uses Circle for the light action rather than Triangle or the retired shoulder-button artwork.
        /// </summary>
        [Fact]
        public void Light_instruction_icons_match_ps2_circle_binding() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\rendering\DemoSceneInstructionOverlayFactory.cs");

            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"windows\", \"y\", new int2(46, 46), \"xbox360\")", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"xbox360\", \"y\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"switch\", \"x\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"gamecube\", \"y\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"wii\", \"2\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"ps2\", \"circle\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"wiiu\", \"y\", new int2(46, 46), \"xbox360\")", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new DesktopInstructionPlatformIconSpec(\"xbox360\", \"rb\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new DesktopInstructionPlatformIconSpec(\"ps2\", \"r1\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new DesktopInstructionPlatformIconSpec(\"ps2\", \"triangle\", new int2(46, 46))", source, StringComparison.Ordinal);
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
