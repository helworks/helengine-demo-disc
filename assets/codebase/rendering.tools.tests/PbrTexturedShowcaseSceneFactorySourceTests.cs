namespace city.tests {
    public sealed class PbrTexturedShowcaseSceneFactorySourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Textured_showcase_scene_factory_declares_its_scene_id_and_two_props() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrTexturedShowcaseSceneFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("public const string SceneId = \"scenes/rendering/pbr_textured_showcase.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeModel planeModel, RuntimeMaterial groundMaterial, RuntimeMaterial metalMaterial, RuntimeMaterial woodMaterial)", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
            Assert.Contains("\"14. PBR Textures\"", source, StringComparison.Ordinal);
            Assert.Contains("ShadowsEnabled = true", source, StringComparison.Ordinal);
        }
    }
}
