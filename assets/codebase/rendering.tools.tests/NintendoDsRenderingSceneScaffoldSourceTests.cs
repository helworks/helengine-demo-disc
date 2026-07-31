namespace city.tests {
    /// <summary>
    /// Verifies that generated Nintendo DS bottom-screen controls retain their authored presentation details.
    /// </summary>
    public sealed class NintendoDsRenderingSceneScaffoldSourceTests {
        /// <summary>
        /// Ensures the LIGHT and BACK controls author visible border overlays independently of their sprite bodies.
        /// </summary>
        [Fact]
        public void Bottom_screen_action_buttons_have_border_overlays() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs");

            Assert.Contains("CreateBottomScreenButtonBorder(lightButtonEntity);", source, StringComparison.Ordinal);
            Assert.Contains("CreateBottomScreenButtonBorder(backButtonEntity);", source, StringComparison.Ordinal);
            Assert.Contains("CreateChild(buttonEntity, \"Border\")", source, StringComparison.Ordinal);
            Assert.Contains("const byte NintendoDsBottomButtonBorderRenderOrder = 220;", source, StringComparison.Ordinal);
            Assert.Contains("BorderThickness = NintendoDsBottomButtonBorderThickness", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the DS action-button bodies use palette-free solid geometry so they remain visible when scene sprites exhaust OBJ palette banks.
        /// </summary>
        [Fact]
        public void Bottom_screen_action_buttons_use_palette_free_bodies() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs");

            Assert.Contains("CreateBottomScreenButtonBody(lightButtonEntity);", source, StringComparison.Ordinal);
            Assert.Contains("CreateBottomScreenButtonBody(backButtonEntity);", source, StringComparison.Ordinal);
            Assert.Contains("FillColor = new byte4(48, 29, 65, 255)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ApplyTextureReference(lightButtonEntity, spriteComponent, NintendoDsBackButtonTexturePath);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ApplyTextureReference(backButtonEntity, spriteComponent, NintendoDsBackButtonTexturePath);", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures light-cycle updates target the dedicated small swatch instead of recoloring the entire LIGHT button body.
        /// </summary>
        [Fact]
        public void Light_toggle_targets_named_swatch_instead_of_button_body() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs");

            Assert.Contains("roundedRectComponent.Size.X == IndicatorSwatchSize", source, StringComparison.Ordinal);
            Assert.Contains("roundedRectComponent.Size.Y == IndicatorSwatchSize", source, StringComparison.Ordinal);

            string scaffoldSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\NintendoDsRenderingSceneScaffoldFactory.cs");
            Assert.Contains("const int NintendoDsLightSwatchSize = 16;", scaffoldSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures every platform light-toggle implementation uses the North face button instead of the shoulder button.
        /// </summary>
        [Fact]
        public void Light_toggle_uses_north_face_button_on_all_platforms() {
            string sharedSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering\DemoDiscLightToggleComponent.cs");
            string handheldSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs");

            Assert.Contains("InputGamepadButton.North", sharedSource, StringComparison.Ordinal);
            Assert.Contains("InputGamepadButton.North", handheldSource, StringComparison.Ordinal);
            Assert.DoesNotContain("InputGamepadButton.RightShoulder", sharedSource, StringComparison.Ordinal);
            Assert.DoesNotContain("InputGamepadButton.RightShoulder", handheldSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures instruction overlays use each platform's concrete North-button icon rather than the retired shoulder-button artwork.
        /// </summary>
        [Fact]
        public void Light_instruction_icons_match_platform_north_button_bindings() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"windows\", \"y\", new int2(46, 46), \"xbox360\")", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"xbox360\", \"y\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"switch\", \"x\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"gamecube\", \"y\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"wii\", \"2\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"ps2\", \"triangle\", new int2(46, 46))", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSpec(\"wiiu\", \"y\", new int2(46, 46), \"xbox360\")", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new DesktopInstructionPlatformIconSpec(\"xbox360\", \"rb\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new DesktopInstructionPlatformIconSpec(\"ps2\", \"r1\"", source, StringComparison.Ordinal);
        }
    }
}
