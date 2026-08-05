namespace city.tests {
    public sealed class PbrShadowTheaterSceneFactorySourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Shadow_theater_scene_factory_declares_its_scene_id_and_two_shadow_casting_lights() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrShadowTheaterSceneFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("public const string SceneId = \"scenes/rendering/pbr_shadow_theater.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel sphereModel, RuntimeMaterial pedestalMaterial, RuntimeMaterial[] galleryMaterials)", source, StringComparison.Ordinal);
            Assert.Contains("new DirectionalLightComponent", source, StringComparison.Ordinal);
            Assert.Contains("new SpotLightComponent", source, StringComparison.Ordinal);
            Assert.Contains("PbrMaterialGalleryMaterialFactory.ResolveIndex", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
            Assert.Contains("\"15. PBR Shadow Theater\"", source, StringComparison.Ordinal);
            int shadowsEnabledCount = System.Text.RegularExpressions.Regex.Matches(source, "ShadowsEnabled = true").Count;
            Assert.Equal(2, shadowsEnabledCount);
        }
    }
}
