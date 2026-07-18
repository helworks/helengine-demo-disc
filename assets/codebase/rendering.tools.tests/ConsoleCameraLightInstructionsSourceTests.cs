namespace city.tests {
    /// <summary>
    /// Verifies the console camera/light Blueprint pipeline is wired through the shared authoring code.
    /// </summary>
    public sealed class ConsoleCameraLightInstructionsSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Console_blueprint_catalog_uses_the_stable_ui_asset_path_and_target_set() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ConsoleCameraLightInstructionsAssetCatalog.cs"));

            Assert.Contains("ConsoleCameraLightInstructionsBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("blueprints/ui/ConsoleCameraLightInstructions.hblueprint", source, StringComparison.Ordinal);
            Assert.Contains("\"ps2\", \"gamecube\", \"wii\", \"switch\", \"wiiu\"", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Console_blueprint_generator_serializes_the_shared_overlay_root() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ConsoleCameraLightInstructionsBlueprintGenerator.cs"));

            Assert.Contains("CreateConsoleCameraLightInstructionsRoot", source, StringComparison.Ordinal);
            Assert.Contains("BlueprintSaveService", source, StringComparison.Ordinal);
            Assert.Contains("ConsoleCameraLightInstructionsBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("root.Dispose()", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Console_scene_attachment_uses_a_blueprint_instance_and_console_platform_existence_rules() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ConsoleCameraLightInstructionsSceneAttachmentService.cs"));

            Assert.Contains("BlueprintInstanceComponent", source, StringComparison.Ordinal);
            Assert.Contains("ConsoleCameraLightInstructionsBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("\"windows\", \"psp\", \"psvita\", \"ds\", \"3ds\"", source, StringComparison.Ordinal);
            Assert.Contains("GetOrCreateExistencePlatformOverride", source, StringComparison.Ordinal);
            Assert.Contains(".Exists = false", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Console_factory_authoring_uses_one_camera_slot_and_keeps_light_behavior_outside_the_blueprint() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoSceneInstructionOverlayFactory.cs"));

            Assert.Contains("CreateConsoleCameraLightInstructionsRoot", source, StringComparison.Ordinal);
            Assert.Contains("ConsoleCameraIconSpecs", source, StringComparison.Ordinal);
            Assert.Contains("ConsoleLightIconSpecs", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ConsoleCameraIconSecondary", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Console_factory_makes_only_the_background_fifteen_percent_wider_without_moving_it() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoSceneInstructionOverlayFactory.cs"));

            Assert.Contains("const int DesktopInstructionPanelWidth = 345;", source, StringComparison.Ordinal);
            Assert.Contains("const int ConsoleInstructionPanelWidth = 345;", source, StringComparison.Ordinal);
            Assert.Contains("panelWidth: ConsoleInstructionPanelWidth", source, StringComparison.Ordinal);
            Assert.Contains("DesktopInstructionPanelLeft, DesktopInstructionPanelTop", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Windows_camera_row_targets_xbox360_dpad_and_left_stick_assets() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoSceneInstructionOverlayFactory.cs"));

            Assert.Contains("new DesktopInstructionPlatformIconSlotSpec(\"windows\", \"dpad\", new int2(48, 48), 0, \"xbox360\")", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSlotSpec(\"windows\", \"left_stick\", new int2(48, 48), 1, \"xbox360\")", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSlotSpec(\"win32\", \"dpad\", new int2(48, 48), 0, \"xbox360\")", source, StringComparison.Ordinal);
            Assert.Contains("new DesktopInstructionPlatformIconSlotSpec(\"win32\", \"left_stick\", new int2(48, 48), 1, \"xbox360\")", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Shared_instruction_labels_use_one_x_position_after_camera_shift() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoSceneInstructionOverlayFactory.cs"));

            Assert.Contains("const float DesktopInstructionLabelLeft = 130f;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DesktopInstructionCameraTextLeft", source, StringComparison.Ordinal);
            const string labelPosition = "new float3(DesktopInstructionLabelLeft, topOffset + textTopAdjustment, 0.1f)";
            Assert.Equal(2, source.Split(labelPosition, StringSplitOptions.None).Length - 1);
        }

        [Fact]
        public void Targeted_scene_factories_delegate_console_attachment_to_the_shared_service() {
            string[] sourcePaths = [
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "AxisTestSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "AxisTest2SceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "CubeTestSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ColoredCubeGridSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DirectionalShadowPlazaSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ScaledCubeSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "TexturedCubeGridSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "physics.tools", "PhysicsSceneFactory.cs")
            ];

            foreach (string sourcePath in sourcePaths) {
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("ConsoleCameraLightInstructionsSceneAttachmentService", source, StringComparison.Ordinal);
            }
        }
    }
}
