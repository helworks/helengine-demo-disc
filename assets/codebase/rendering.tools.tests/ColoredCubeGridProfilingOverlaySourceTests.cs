namespace city.rendering.tools.tests {
    /// <summary>
    /// Verifies the Colored Cubes scene uses the shared demo-disc UI kit plus the shared camera and light instruction overlays.
    /// </summary>
    public sealed class ColoredCubeGridProfilingOverlaySourceTests {
        /// <summary>
        /// Ensures Colored Cubes delegates its 2D overlay to the shared scene UI kit and authors the shared instruction overlays.
        /// </summary>
        [Fact]
        public void Colored_cube_grid_scene_uses_the_shared_scene_ui_kit() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\ColoredCubeGridSceneFactory.cs");

            Assert.Contains("DemoDiscSceneUiKitFactory().CreateStandardSceneUi", source, StringComparison.Ordinal);
            Assert.Contains("CreateDesktopInstructionOverlayRoot", source, StringComparison.Ordinal);
            Assert.Contains("consoleInstructionAttachmentService.CreateBlueprintInstanceRoot", source, StringComparison.Ordinal);
        }
    }
}
