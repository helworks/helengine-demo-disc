namespace city.tests {
    public sealed class PbrTexturedShowcaseTextureAssetTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        [Theory]
        [InlineData("Metal032Albedo.jpg")]
        [InlineData("Metal032Roughness.jpg")]
        [InlineData("WoodFloor041Albedo.jpg")]
        [InlineData("WoodFloor041Roughness.jpg")]
        public void Downloaded_pbr_showcase_texture_exists_and_is_a_real_image_file(string fileName) {
            string path = Path.Combine(ProjectRootPath, "assets", "textures", "rendering", "pbr_textured_showcase", fileName);
            Assert.True(File.Exists(path), $"Expected '{path}' to exist.");
            Assert.True(new FileInfo(path).Length > 10_000, $"Expected '{path}' to be a real downloaded JPG, not a placeholder.");
        }
    }
}
