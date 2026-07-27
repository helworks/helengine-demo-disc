namespace city.menu.tools.tests {
    /// <summary>
    /// Protects the loading screen's use of the reference-canvas scale after runtime geometry changes.
    /// </summary>
    public sealed class SceneLoadingScreenComponentSourceTests {
        /// <summary>
        /// Verifies that the loading fill applies the canvas-owned horizontal scale after changing its authored width.
        /// </summary>
        [Fact]
        public void Loading_screen_scales_its_runtime_fill_width_from_the_reference_canvas() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu\SceneLoadingScreenComponent.cs");

            Assert.Contains("ReferenceCanvasFitComponent.CalculateScale()", source, StringComparison.Ordinal);
            Assert.Contains("ProgressTrackWidth * clampedProgress * canvasScale.X", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the loading blackout rectangle covers the live viewport rather than inheriting the letterboxed reference-canvas size.
        /// </summary>
        [Fact]
        public void Loading_screen_background_uses_the_camera_hierarchy_and_live_viewport_size() {
            string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\SceneLoadingScreenFactory.cs");
            string componentSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu\SceneLoadingScreenComponent.cs");

            Assert.Contains("Entity background = CreateRectangle(camera, \"LoadingBackground\"", factorySource, StringComparison.Ordinal);
            Assert.Contains("Background.Size = Core.Instance.RenderManager3D.MainWindowSize", componentSource, StringComparison.Ordinal);
        }
    }
}
