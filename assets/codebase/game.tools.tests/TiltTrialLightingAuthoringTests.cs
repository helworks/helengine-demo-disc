namespace city.tests {
    /// <summary>
    /// Verifies the authored Tilt Trial lighting source keeps the requested stronger key and fill contribution for the player sphere.
    /// </summary>
    public sealed class TiltTrialLightingAuthoringTests {
        /// <summary>
        /// Ensures the Tilt Trial scene authors one slightly stronger key light plus a stronger shadowless fill light so the sphere's dark hemisphere stays readable.
        /// </summary>
        [Fact]
        public void Tilt_trial_scene_source_authors_stronger_key_and_shadowless_fill_light() {
            string sourcePath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "game.tools", "GameSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("Entity entity = OwningCore.EntityFactory.Create(\"TiltTrialSun\");", source, StringComparison.Ordinal);
            Assert.Contains("Intensity = 1.15f,", source, StringComparison.Ordinal);
            Assert.Contains("CreateDirectionalFillLightEntity(),", source, StringComparison.Ordinal);
            Assert.Contains("Entity entity = OwningCore.EntityFactory.Create(\"TiltTrialFill\");", source, StringComparison.Ordinal);
            Assert.Contains("Intensity = 0.7f,", source, StringComparison.Ordinal);
            Assert.Contains("ShadowsEnabled = false,", source, StringComparison.Ordinal);
        }
    }
}
