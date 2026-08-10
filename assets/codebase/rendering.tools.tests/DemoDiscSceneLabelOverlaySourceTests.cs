namespace city.tests {
    public sealed class DemoDiscSceneLabelOverlaySourceTests {
        static readonly string ProjectRootPath = ResolveProjectRoot();

        static string ResolveProjectRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo currentDirectory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (currentDirectory != null) {
                string assetsPath = Path.Combine(currentDirectory.FullName, "assets");
                string projectFilePath = Path.Combine(currentDirectory.FullName, "project.heproj");
                if (Directory.Exists(assetsPath) && File.Exists(projectFilePath)) {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }
            throw new InvalidOperationException("Unable to locate the demo-disc checkout root from the test working directory.");
        }

        [Fact]
        public void Shared_label_overlay_uses_fixed_top_right_body_font_layout() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoDiscSceneLabelOverlayFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("const float SceneLabelRight = 24f;", source, StringComparison.Ordinal);
            Assert.Contains("const float SceneLabelTop = 72f;", source, StringComparison.Ordinal);
            Assert.Contains("CreateChild(sceneUiEntity, LabelEntityName)", source, StringComparison.Ordinal);
            Assert.Contains("const int SceneLabelCanvasWidth = 1280;", source, StringComparison.Ordinal);
            Assert.Contains("const string SceneLabelFontRelativePath = \"Fonts/DemoDiscBody.ttf\";", source, StringComparison.Ordinal);
            Assert.Contains("SceneLabelCanvasWidth - SceneLabelRight - SceneLabelWidth", source, StringComparison.Ordinal);
            Assert.Contains("labelEntity.Static = false;", source, StringComparison.Ordinal);
            Assert.Contains("Alignment = TextAlignment.Left", source, StringComparison.Ordinal);
            Assert.Contains("SceneAssetReferenceFactory.CreateFileSystemFont(SceneLabelFontRelativePath)", source, StringComparison.Ordinal);
            Assert.Contains("const int SceneLabelRenderOrder = 7;", source, StringComparison.Ordinal);
            Assert.Contains("RenderOrder2D = SceneLabelRenderOrder", source, StringComparison.Ordinal);
            Assert.Contains("sceneUiEntity.AddComponent(new city.rendering.DemoDiscDebugSceneLabelComponent())", source, StringComparison.Ordinal);
            Assert.Contains("labelEntity.Enabled = true;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Shared_label_overlay_marks_text_component_absent_on_nintendo_handhelds() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoDiscSceneLabelOverlayFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("const string NintendoDsPlatformId = \"ds\";", source, StringComparison.Ordinal);
            Assert.Contains("const string Nintendo3DsPlatformId = \"3ds\";", source, StringComparison.Ordinal);
            Assert.Contains("saveComponent.GetOrCreateExistencePlatformOverride(NintendoDsPlatformId).Exists = false;", source, StringComparison.Ordinal);
            Assert.Contains("saveComponent.GetOrCreateExistencePlatformOverride(Nintendo3DsPlatformId).Exists = false;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("RemoveComponent(labelComponent", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Curated_rendering_factories_contain_the_approved_labels() {
            (string FileName, string Label)[] expected = [
                ("CubeTestSceneFactory.cs", "1. Cube Test"),
                ("ColoredCubeGridSceneFactory.cs", "2. Colored Cubes"),
                ("TexturedCubeGridSceneFactory.cs", "3. Textured Cubes"),
                ("AxisTestSceneFactory.cs", "4. Axis 1"),
                ("AxisTest2SceneFactory.cs", "5. Axis 2"),
                ("DirectionalShadowPlazaSceneFactory.cs", "7. Shadow Plaza"),
                ("PbrMaterialGallerySceneFactory.cs", "13. PBR Gallery"),
                ("PbrTexturedShowcaseSceneFactory.cs", "14. PBR Textures"),
                ("PbrShadowTheaterSceneFactory.cs", "15. PBR Shadow Theater")
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
        public void Gameplay_scene_generator_uses_the_debug_scene_label_overlay() {
            string[] paths = [
                Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "NintendoDsRenderingSceneScaffoldFactory.cs"),
                Path.Combine(ProjectRootPath, "assets", "codebase", "physics.tools", "PhysicsNintendoDsSceneGenerator.cs")
            ];
            foreach (string path in paths) {
                Assert.DoesNotContain("DemoDiscSceneLabelOverlayFactory", File.ReadAllText(path), StringComparison.Ordinal);
            }

            string gameplaySource = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "game.tools", "GameSceneFactory.cs"));
            Assert.Contains("DemoDiscSceneLabelOverlayFactory", gameplaySource, StringComparison.Ordinal);
            Assert.Contains("levelEntry.DisplayName", gameplaySource, StringComparison.Ordinal);

            string runtimeSource = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering", "DemoDiscDebugSceneLabelComponent.cs"));
            Assert.Contains("#if HELENGINE_ENV_DEBUG", runtimeSource, StringComparison.Ordinal);
            Assert.Contains("overlayEntity.Enabled = false", runtimeSource, StringComparison.Ordinal);
            Assert.Contains("SetOverlayVisibility();", runtimeSource, StringComparison.Ordinal);
            Assert.Contains("public override void ComponentAdded(Entity entity)", runtimeSource, StringComparison.Ordinal);

            string physicsSource = File.ReadAllText(paths[1]);
            Assert.Contains("static readonly string[] NintendoHandheldPlatformIds = [\"ds\", \"3ds\"];", physicsSource, StringComparison.Ordinal);
            Assert.Contains("authoredSceneAsset.RootEntities = RemoveNintendoHandheldOnlyEntities(authoredSceneAsset.RootEntities, supportedPlatformIds);", physicsSource, StringComparison.Ordinal);
            Assert.Contains("SceneEntityAsset[] RemoveNintendoHandheldOnlyEntities", physicsSource, StringComparison.Ordinal);
            Assert.Contains("return existsOnNintendoHandheld && !existsOnNonHandheld;", physicsSource, StringComparison.Ordinal);
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
