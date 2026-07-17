namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial course material is backed by one generated lilac grid texture.
    /// </summary>
    public sealed class TiltTrialCourseTextureAuthoringTests {
        /// <summary>
        /// Ensures the course material factory writes a dedicated lilac grid texture and binds it as the diffuse texture source.
        /// </summary>
        [Fact]
        public void Tilt_trial_course_material_factory_binds_generated_lilac_grid_texture() {
            string textureFactorySourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialCourseTextureFactory.cs";
            string materialFactorySourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialCourseMaterialFactory.cs";
            string textureFactorySource = File.ReadAllText(textureFactorySourcePath);
            string materialFactorySource = File.ReadAllText(materialFactorySourcePath);

            Assert.Contains("public const string TextureRelativePath = \"textures/rendering/tilt_trial/CourseLilacGrid.bmp\";", textureFactorySource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialCourseTextureFactory textureFactory = new TiltTrialCourseTextureFactory();", materialFactorySource, StringComparison.Ordinal);
            Assert.Contains("string textureAssetId = textureFactory.WriteTextureAsset(projectRootPath);", materialFactorySource, StringComparison.Ordinal);
            Assert.Contains("DiffuseTextureAssetId = textureAssetId,", materialFactorySource, StringComparison.Ordinal);
            Assert.Contains("platformDefinition.SetFieldValue(TextureIdFieldId, textureAssetId);", materialFactorySource, StringComparison.Ordinal);
            Assert.Contains("platformDefinition.SetFieldValue(Ps2TextureRelativePathFieldId, \"cooked/imported/\" + textureAssetId);", materialFactorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the texture factory source keeps the BMP writer and importer hookup needed by the asset pipeline.
        /// </summary>
        [Fact]
        public void Tilt_trial_course_texture_factory_keeps_bitmap_writer_and_importer_hookup() {
            string textureFactorySourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TiltTrialCourseTextureFactory.cs";
            string textureFactorySource = File.ReadAllText(textureFactorySourcePath);

            Assert.Contains("fileBytes[0] = (byte)'B';", textureFactorySource, StringComparison.Ordinal);
            Assert.Contains("fileBytes[1] = (byte)'M';", textureFactorySource, StringComparison.Ordinal);
            Assert.Contains("TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(fullTexturePath);", textureFactorySource, StringComparison.Ordinal);
            Assert.Contains("string assetId = settings.Importer.AssetId;", textureFactorySource, StringComparison.Ordinal);
            Assert.Contains("Directory.CreateDirectory(textureDirectoryPath);", textureFactorySource, StringComparison.Ordinal);
        }
    }
}
