namespace city.tests {
    public sealed class PbrTexturedShowcaseMaterialFactorySourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Textured_showcase_material_factory_references_downloaded_textures_and_metallic_split() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrTexturedShowcaseMaterialFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("textures/rendering/pbr_textured_showcase/Metal032Albedo.jpg", source, StringComparison.Ordinal);
            Assert.Contains("textures/rendering/pbr_textured_showcase/Metal032Roughness.jpg", source, StringComparison.Ordinal);
            Assert.Contains("textures/rendering/pbr_textured_showcase/WoodFloor041Albedo.jpg", source, StringComparison.Ordinal);
            Assert.Contains("textures/rendering/pbr_textured_showcase/WoodFloor041Roughness.jpg", source, StringComparison.Ordinal);
            Assert.Contains("public const string MetalMaterialRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("public const string WoodMaterialRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("public void WriteMaterialAssets(string projectRootPath)", source, StringComparison.Ordinal);
            Assert.Contains("\"1.0\"", source, StringComparison.Ordinal);
            Assert.Contains("\"0.0\"", source, StringComparison.Ordinal);
        }
    }
}
