using DemoDisc.EditorTools;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    public sealed class RenderingSceneGeneratorPbrRegistrationSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Fact]
        public void Generator_declares_the_three_new_pbr_scene_ids() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "DemoDiscEditorTools", "rendering", "RenderingSceneGenerator.cs"));
            Assert.Contains("public const string PbrMaterialGallerySceneId = \"scenes/rendering/pbr_material_gallery.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public const string PbrTexturedShowcaseSceneId = \"scenes/rendering/pbr_textured_showcase.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("public const string PbrShadowTheaterSceneId = \"scenes/rendering/pbr_shadow_theater.helen\";", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Generator_writes_the_three_new_pbr_scenes_after_the_directional_shadow_plaza_scene() {
            string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "DemoDiscEditorTools", "rendering", "RenderingSceneGenerator.cs"));
            int plazaWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(directionalShadowPlazaSceneDefinition);", StringComparison.Ordinal);
            int galleryWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(pbrMaterialGallerySceneDefinition);", StringComparison.Ordinal);
            int texturedWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(pbrTexturedShowcaseSceneDefinition);", StringComparison.Ordinal);
            int theaterWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(pbrShadowTheaterSceneDefinition);", StringComparison.Ordinal);
            Assert.True(plazaWriteIndex >= 0 && galleryWriteIndex > plazaWriteIndex && texturedWriteIndex > galleryWriteIndex && theaterWriteIndex > texturedWriteIndex,
                "Expected the three new PBR scenes to be written, in order, after the directional-shadow plaza scene.");
            Assert.Contains("PbrMaterialGalleryMaterials.WriteMaterialAssets(projectRootPath);", source, StringComparison.Ordinal);
        }
    }
}
