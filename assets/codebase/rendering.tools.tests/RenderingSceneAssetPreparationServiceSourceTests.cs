namespace city.tests {
    public sealed class RenderingSceneAssetPreparationServiceSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Asset_bundle_exposes_the_textured_showcase_materials() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerationAssets.cs");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("public RuntimeMaterial PbrTexturedShowcaseMetalMaterial { get; set; }", source, StringComparison.Ordinal);
            Assert.Contains("public RuntimeMaterial PbrTexturedShowcaseWoodMaterial { get; set; }", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Preparation_service_writes_and_loads_the_textured_showcase_materials() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneAssetPreparationService.cs");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("PbrTexturedShowcaseMaterialFactory", source, StringComparison.Ordinal);
            Assert.Contains("PbrTexturedShowcaseMaterialFactory.MetalMaterialRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("PbrTexturedShowcaseMaterialFactory.WoodMaterialRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("PbrTexturedShowcaseMetalMaterial = ", source, StringComparison.Ordinal);
            Assert.Contains("PbrTexturedShowcaseWoodMaterial = ", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Preparation_service_writes_the_authored_walnut_material_through_the_editor_capability() {
            string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneAssetPreparationService.cs");
            string source = File.ReadAllText(sourcePath);
            Assert.Contains("TiltTrialPlayerSphereWalnutMaterialFactory", source, StringComparison.Ordinal);
            Assert.Contains("WriteMaterialAsset(fullProjectRootPath, AssetAuthoringService)", source, StringComparison.Ordinal);
        }
    }
}
