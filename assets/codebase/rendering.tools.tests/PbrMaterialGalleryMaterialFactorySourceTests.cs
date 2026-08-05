namespace city.tests {
    public sealed class PbrMaterialGalleryMaterialFactorySourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Gallery_material_factory_sweeps_five_metallic_and_five_roughness_steps() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "PbrMaterialGalleryMaterialFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("public const int MetallicSteps = 5;", source, StringComparison.Ordinal);
            Assert.Contains("public const int RoughnessSteps = 5;", source, StringComparison.Ordinal);
            Assert.Contains("public static int ResolveIndex(int metallicIndex, int roughnessIndex)", source, StringComparison.Ordinal);
            Assert.Contains("public RuntimeMaterial[] CreateRuntimeMaterials()", source, StringComparison.Ordinal);
            Assert.Contains("public void WriteMaterialAssets(string projectRootPath)", source, StringComparison.Ordinal);
            Assert.Contains("StandardMaterialMetallicDefaults.MetallicBufferName", source, StringComparison.Ordinal);
            Assert.Contains("StandardMaterialRoughnessDefaults.RoughnessBufferName", source, StringComparison.Ordinal);
            Assert.Contains("StandardMaterialSpecularDefaults.SpecularBufferName", source, StringComparison.Ordinal);
            Assert.Contains("materials/rendering/pbr_gallery", source, StringComparison.Ordinal);
        }
    }
}
