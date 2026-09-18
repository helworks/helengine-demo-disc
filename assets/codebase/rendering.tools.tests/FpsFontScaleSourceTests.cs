using helengine;
using helengine.editor;

using city.testing;

namespace city.tests {
    /// <summary>
    /// Verifies that non-Nintendo DS FPS overlays use the shared demo-disc font scale.
    /// </summary>
    public sealed class FpsFontScaleSourceTests {
        [Fact]
        public void Non_nintendo_ds_fps_components_use_the_standard_two_x_font_scale() {
            string kitSource = File.ReadAllText(DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "DemoDiscSceneUiKitFactory.cs"));
            Assert.Contains("FPSComponent", kitSource, StringComparison.Ordinal);
            Assert.DoesNotContain("FontScale = 1f", kitSource, StringComparison.Ordinal);
            Assert.Contains("FontScale = 2f", kitSource, StringComparison.Ordinal);
            Assert.Contains("PspFpsComponentOverrideService.Apply", kitSource, StringComparison.Ordinal);

            string[] kitFactorySourcePaths = [
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "AxisTestSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "AxisTest2SceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "ColoredCubeGridSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "CubeTestSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "DepthClipProbeSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "DirectionalShadowPlazaSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "GroundCubeProbeSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "MatrixRenderSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "physics.tools", "PhysicsSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "ScaledCubeSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "SpotlightStreetSliceSceneFactory.cs"),
                DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "TexturedCubeGridSceneFactory.cs")
            ];
            foreach (string sourcePath in kitFactorySourcePaths) {
                string source = File.ReadAllText(sourcePath);
                string sourceForUiKitAssertion = source.Replace(
                    "new city.rendering.tools.DemoDiscSceneUiKitFactory",
                    "new DemoDiscSceneUiKitFactory",
                    StringComparison.Ordinal);
                Assert.Contains("new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateStandardSceneUi", sourceForUiKitAssertion, StringComparison.Ordinal);
                Assert.DoesNotContain("FontScale = 1f", source, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void Psp_fps_override_persists_the_font_reference_before_serializing_the_override() {
            string sourcePath = DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "PspFpsComponentOverrideService.cs");
            string source = File.ReadAllText(sourcePath);

            int fontReferenceIndex = source.IndexOf("saveComponent.SetAssetReference(", StringComparison.Ordinal);
            int overrideSerializationIndex = source.IndexOf(
                "FPSComponent overrideComponent = (FPSComponent)PlatformEditingService.EnsurePlatformOverrideComponent",
                StringComparison.Ordinal);

            Assert.True(fontReferenceIndex >= 0);
            Assert.True(overrideSerializationIndex >= 0);
            Assert.True(fontReferenceIndex < overrideSerializationIndex);
        }

        /// <summary>
        /// Ensures the shared handheld scaffold keeps DS button labels at the common scale while authoring a centered half-scale 3DS override.
        /// </summary>
        [Fact]
        public void Nintendo_handheld_button_labels_use_centered_text_and_a_half_scale_3ds_override() {
            string sourcePath = DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "NintendoDsRenderingSceneScaffoldFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("const float NintendoDsBottomOverlayFontScale = 1f;", source, StringComparison.Ordinal);
            Assert.Contains("const float Nintendo3DsBottomButtonLabelFontScale = 0.5f;", source, StringComparison.Ordinal);
            Assert.Contains("const float Nintendo3DsFpsFontScale = 1f;", source, StringComparison.Ordinal);
            Assert.Contains("const byte NintendoDsLightSwatchRenderOrder = 222;", source, StringComparison.Ordinal);
            Assert.Contains("Alignment = TextAlignment.Center", source, StringComparison.Ordinal);
            Assert.Contains("EnsurePlatformOverrideComponent", source, StringComparison.Ordinal);
            Assert.Contains("nameof(TextComponent.FontScale)", source, StringComparison.Ordinal);
            Assert.Contains("nameof(FPSComponent.FontScale)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures Cube Test keeps the shared bottom-screen scaffold so its generated 3DS overrides are authored into the scene.
        /// </summary>
        [Fact]
        public void Cube_test_uses_the_shared_bottom_screen_scaffold() {
            string sourcePath = DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "CubeTestSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("UseDefaultBottomOverlay = true", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Matrix_render_scene_contains_the_standard_fps_component() {
            string scenePath = DemoDiscTestProject.GetPath("assets", "scenes", "rendering", "test_scene_matrix_render.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            string fpsComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(FPSComponent));

            Assert.Contains(
                EnumerateComponents(scene.RootEntities),
                component => string.Equals(component.ComponentTypeId, fpsComponentTypeId, StringComparison.Ordinal));
        }

        [Fact]
        public void Matrix_render_ui_root_preserves_the_runtime_scene_layer_mask() {
            string scenePath = DemoDiscTestProject.GetPath("assets", "scenes", "rendering", "test_scene_matrix_render.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] matrixRenderUiRoots = EnumerateEntities(scene.RootEntities)
                .Where(entity => string.Equals(entity.Name, "MatrixRenderUi", StringComparison.Ordinal))
                .ToArray();

            Assert.NotEmpty(matrixRenderUiRoots);
            Assert.All(matrixRenderUiRoots, matrixRenderUi => {
                Assert.True(matrixRenderUi.Enabled);
                Assert.Equal(EditorLayerMasks.SceneObjects, matrixRenderUi.LayerMask);
            });
        }

        [Fact]
        public void Matrix_render_scene_includes_the_shared_fps_ui_font_reference() {
            string scenePath = DemoDiscTestProject.GetPath("assets", "scenes", "rendering", "test_scene_matrix_render.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));

            Assert.Contains(
                scene.AssetReferences ?? Array.Empty<SceneAssetReference>(),
                reference => string.Equals(reference.RelativePath, "generated/editor/fonts/ui.hefont", StringComparison.Ordinal));
        }

        static IEnumerable<SceneComponentAssetRecord> EnumerateComponents(SceneEntityAsset[] entities) {
            foreach (SceneEntityAsset entity in entities ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneComponentAssetRecord component in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                    yield return component;
                }

                foreach (SceneComponentAssetRecord component in EnumerateComponents(entity.Children)) {
                    yield return component;
                }
            }
        }

        static IEnumerable<SceneEntityAsset> EnumerateEntities(SceneEntityAsset[] entities) {
            foreach (SceneEntityAsset entity in entities ?? Array.Empty<SceneEntityAsset>()) {
                yield return entity;

                foreach (SceneEntityAsset child in EnumerateEntities(entity.Children)) {
                    yield return child;
                }
            }
        }
    }
}
