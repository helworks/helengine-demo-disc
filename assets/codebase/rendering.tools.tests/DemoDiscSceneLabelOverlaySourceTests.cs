namespace city.tests {
    public sealed class DemoDiscSceneLabelOverlaySourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Shared_label_overlay_uses_fixed_top_right_body_font_layout() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoDiscSceneLabelOverlayFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("const int ReferenceViewportWidth = 1280;", source, StringComparison.Ordinal);
            Assert.Contains("const int ReferenceViewportHeight = 720;", source, StringComparison.Ordinal);
            Assert.Contains("const float SceneLabelRight = 24f;", source, StringComparison.Ordinal);
            Assert.Contains("const float SceneLabelTop = 24f;", source, StringComparison.Ordinal);
            Assert.Contains("BindingMode = ViewportComponent.ScreenBindingMode", source, StringComparison.Ordinal);
            Assert.Contains("Alignment = TextAlignment.Right", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscSceneComponentRecordFactory.CreateEditorFontReference()", source, StringComparison.Ordinal);
            Assert.Contains("RenderOrder2D = SceneLabelRenderOrder", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Curated_rendering_factories_contain_the_approved_labels() {
            (string FileName, string Label)[] expected = [
                ("CubeTestSceneFactory.cs", "1. Cube Test"),
                ("ColoredCubeGridSceneFactory.cs", "2. Colored Cubes"),
                ("TexturedCubeGridSceneFactory.cs", "3. Textured Cubes"),
                ("AxisTestSceneFactory.cs", "4. Axis 1"),
                ("AxisTest2SceneFactory.cs", "5. Axis 2"),
                ("DirectionalShadowPlazaSceneFactory.cs", "7. Shadow Plaza")
            ];
            foreach ((string fileName, string label) in expected) {
                string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", fileName));
                Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
                Assert.Contains($"\"{label}\"", source, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void Physics_factory_contains_only_the_curated_physics_labels() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "physics.tools", "PhysicsSceneFactory.cs"));
            Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
            foreach (string label in new[] {
                "6. Matrix Render", "8. Stacked Boxes", "9. Sphere Stack",
                "10. Mixed Stack", "11. Static Mesh", "12. Simple Mesh"
            }) {
                Assert.Contains($"\"{label}\"", source, StringComparison.Ordinal);
            }
            Assert.Contains("ResolveDemoDiscSceneLabel", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_and_handheld_generators_do_not_reference_the_label_overlay() {
            string[] paths = [
                Path.Combine(ProjectRootPath, "assets", "codebase", "game.tools", "GameSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "NintendoDsRenderingSceneScaffoldFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "physics.tools", "PhysicsNintendoDsSceneGenerator.cs")
            ];
            foreach (string path in paths) {
                Assert.DoesNotContain("DemoDiscSceneLabelOverlayFactory", File.ReadAllText(path), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void Non_menu_rendering_factories_remain_without_scene_labels() {
            string[] paths = [
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "GroundCubeProbeSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ScaledCubeSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "SpotlightStreetSliceSceneFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "SceneMemoryProbeSceneFactory.cs")
            ];
            foreach (string path in paths) {
                Assert.DoesNotContain("DemoDiscSceneLabelOverlayFactory", File.ReadAllText(path), StringComparison.Ordinal);
            }
        }
    }
}
