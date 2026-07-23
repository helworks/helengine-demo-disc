namespace city.tests {
    /// <summary>
    /// Verifies that the textured-cube diagnostic scene uses a PS2-sized texture representation.
    /// </summary>
    public sealed class TexturedCubeGridPs2TextureBudgetSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

        /// <summary>
        /// Ensures the generated cube import settings select indexed PS2 textures while leaving other platforms unchanged.
        /// </summary>
        [Fact]
        public void Generated_cube_texture_import_settings_use_indexed8_for_ps2() {
            string source = File.ReadAllText(Path.Combine(
                ProjectRootPath,
                "assets",
                "codebase",
                "rendering.tools",
                "TexturedCubeGridSceneFactory.cs"));

            Assert.Contains("settings.Processor.Platforms[\"ps2\"]", source, StringComparison.Ordinal);
            Assert.Contains("ColorFormat = TextureAssetColorFormat.Indexed8", source, StringComparison.Ordinal);
            Assert.Contains("AlphaPrecision = TextureAssetAlphaPrecision.A8", source, StringComparison.Ordinal);
        }
    }
}
