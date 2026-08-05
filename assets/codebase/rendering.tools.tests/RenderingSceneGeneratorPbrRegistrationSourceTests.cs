namespace city.tests {
    public sealed class RenderingSceneGeneratorPbrRegistrationSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Generator_declares_the_three_new_pbr_scene_ids() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs"));
            Assert.Contains("public const string PbrMaterialGallerySceneId = \"scenes/rendering/pbr_material_gallery.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public const string PbrTexturedShowcaseSceneId = \"scenes/rendering/pbr_textured_showcase.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public const string PbrShadowTheaterSceneId = \"scenes/rendering/pbr_shadow_theater.helen\";", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Generator_writes_the_three_new_pbr_scenes_after_the_existing_spotlight_scene() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs"));
            int spotlightWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, spotlightStreetSliceSceneDefinition);", StringComparison.Ordinal);
            int galleryWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, pbrMaterialGallerySceneDefinition);", StringComparison.Ordinal);
            int texturedWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, pbrTexturedShowcaseSceneDefinition);", StringComparison.Ordinal);
            int theaterWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(projectRootPath, pbrShadowTheaterSceneDefinition);", StringComparison.Ordinal);
            Assert.True(spotlightWriteIndex >= 0 && galleryWriteIndex > spotlightWriteIndex && texturedWriteIndex > galleryWriteIndex && theaterWriteIndex > texturedWriteIndex,
                "Expected the three new PBR scenes to be written, in order, after the existing spotlight street-slice scene.");
            Assert.Contains("PbrMaterialGalleryMaterials.WriteMaterialAssets(projectRootPath);", source, StringComparison.Ordinal);
        }
    }
}
