namespace city.tests {
    /// <summary>
    /// Verifies rendering and physics prompt overlays consume the shared generated control-icon resolver.
    /// </summary>
    public sealed class PromptIconOverlaySourceTests {
        [Fact]
        public void Demo_scene_instruction_overlay_source_uses_shared_resolver_and_editor_platform_overrides() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

            Assert.Contains("GeneratedControlIconAssetResolver", source, StringComparison.Ordinal);
            Assert.Contains("ComponentPlatformEditingService", source, StringComparison.Ordinal);
            Assert.Contains("EnsurePlatformOverrideComponent", source, StringComparison.Ordinal);
            Assert.Contains("PersistPlatformOverride", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DemoScenePlatformInstructionIconSetComponent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Images/Instructions/Controls/xbox360_dpad.png", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Images/Instructions/Controls/ps2_r1.png", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Images/Instructions/Controls/switch_r.png", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Demo_scene_instruction_overlay_source_uses_raw_control_ids_for_keyboard_and_console_rows() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

            Assert.Contains("\"wasd\"", source, StringComparison.Ordinal);
            Assert.Contains("\"key_l\"", source, StringComparison.Ordinal);
            Assert.Contains("\"dpad\"", source, StringComparison.Ordinal);
            Assert.Contains("\"rb\"", source, StringComparison.Ordinal);
            Assert.Contains("\"r1\"", source, StringComparison.Ordinal);
            Assert.Contains("\"r\"", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Physics_scene_factory_source_still_delegates_instruction_overlay_to_shared_rendering_factory() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs");

            Assert.Contains("instructionOverlayFactory.CreateDesktopInstructionOverlayRoot", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Images/Instructions/Controls/xbox360_dpad.png", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Images/Instructions/Controls/generated/", source, StringComparison.Ordinal);
        }
    }
}
