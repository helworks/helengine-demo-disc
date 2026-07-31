namespace city.rendering.tools.tests {
    /// <summary>
    /// Verifies the Colored Cubes profiling scene reserves its overlay space for performance diagnostics instead of the temporary light indicator.
    /// </summary>
    public sealed class ColoredCubeGridProfilingOverlaySourceTests {
        /// <summary>
        /// Ensures Colored Cubes retains its FPS component without any instructional or light-control overlay.
        /// </summary>
        [Fact]
        public void Colored_cube_grid_scene_uses_fps_overlay_without_light_indicator() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\ColoredCubeGridSceneFactory.cs");

            Assert.Contains("entity.AddComponent(new FPSComponent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("entity.AddComponent(new DemoDiscLightToggleComponent())", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateDesktopInstructionOverlayRoot", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ConsoleCameraLightInstructionsSceneAttachmentService", source, StringComparison.Ordinal);
            Assert.DoesNotContain("consoleInstructionAttachmentService.CreateBlueprintInstanceRoot", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory", source, StringComparison.Ordinal);
            Assert.DoesNotContain("lightIndicatorOverlayFactory.AttachToSceneUi", source, StringComparison.Ordinal);
        }
    }
}
