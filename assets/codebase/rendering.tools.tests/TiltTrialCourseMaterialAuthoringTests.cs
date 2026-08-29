namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial scene uses one dedicated authored course material instead of the shared engine standard material.
    /// </summary>
    public sealed class TiltTrialCourseMaterialAuthoringTests {
        /// <summary>
        /// Ensures the Tilt Trial course and catch floor are wired through one authored runtime material so scene-specific look changes do not mutate every showcase surface.
        /// </summary>
        [Fact]
        public void Tilt_trial_scene_source_authors_one_dedicated_course_material() {
            string assetsSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneGenerationAssets.cs";
            string preparationSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneAssetPreparationService.cs";
            string gameSceneSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs";

            string assetsSource = File.ReadAllText(assetsSourcePath);
            string preparationSource = File.ReadAllText(preparationSourcePath);
            string gameSceneSource = File.ReadAllText(gameSceneSourcePath);

            Assert.Contains("public RuntimeMaterial TiltTrialCourseMaterial { get; set; }", assetsSource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialCourseMaterialFactory tiltTrialCourseMaterialFactory = new TiltTrialCourseMaterialFactory(AuthoringSession, Transaction);", preparationSource, StringComparison.Ordinal);
            Assert.Contains("tiltTrialCourseMaterialFactory.WriteMaterialAsset(fullProjectRootPath, AuthoringSession);", preparationSource, StringComparison.Ordinal);
            Assert.Contains("RuntimeMaterial tiltTrialCourseMaterial = LoadRuntimeMaterial(TiltTrialCourseMaterialFactory.MaterialRelativePath);", preparationSource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialCourseMaterial = tiltTrialCourseMaterial,", preparationSource, StringComparison.Ordinal);
            Assert.Contains("readonly RuntimeMaterial TiltTrialCourseMaterial;", gameSceneSource, StringComparison.Ordinal);
            Assert.Contains("const string TiltTrialCourseMaterialRelativePath = \"materials/rendering/tilt_trial/Course.hasset\";", gameSceneSource, StringComparison.Ordinal);
            Assert.Contains("assets.TiltTrialCourseMaterial == null", gameSceneSource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialCourseMaterial = assets.TiltTrialCourseMaterial;", gameSceneSource, StringComparison.Ordinal);
            Assert.Contains("Materials = new[] { TiltTrialCourseMaterial },", gameSceneSource, StringComparison.Ordinal);
            Assert.Contains("AssetAuthoringService.CreateFileReference(TiltTrialCourseMaterialRelativePath, AssetEntryKind.Material)", gameSceneSource, StringComparison.Ordinal);
            Assert.DoesNotContain("Materials = new[] { GeneratedStandardMaterial },", gameSceneSource, StringComparison.Ordinal);
        }
    }
}
