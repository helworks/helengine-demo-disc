namespace city.tests {
    public sealed class PbrMaterialGallerySceneFactorySourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Gallery_scene_factory_declares_its_scene_id_and_five_light_lit_grid() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrMaterialGallerySceneFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("public const string SceneId = \"scenes/rendering/pbr_material_gallery.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel planeModel, RuntimeModel sphereModel, RuntimeMaterial groundMaterial, RuntimeMaterial[] galleryMaterials)", source, StringComparison.Ordinal);
            Assert.Contains("new DirectionalLightComponent", source, StringComparison.Ordinal);
            Assert.Contains("new AmbientLightComponent", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscSceneUiKitFactory", source, StringComparison.Ordinal);
            Assert.Contains("\"13. PBR Gallery\"", source, StringComparison.Ordinal);
            Assert.Contains("PbrMaterialGalleryMaterialFactory.ResolveIndex", source, StringComparison.Ordinal);
        }
    }
}
