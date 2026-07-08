namespace city.tests {
    /// <summary>
    /// Verifies Tilt Trial authored material colors use the shared <c>#RRGGBBAA</c> contract with fully opaque alpha.
    /// </summary>
    public sealed class TiltTrialMaterialColorAuthoringTests {
        /// <summary>
        /// Ensures the course and legacy walnut sphere colors keep their warm RGB values while ending in an explicit opaque alpha channel.
        /// </summary>
        [Fact]
        public void Tilt_trial_authored_material_colors_keep_opaque_alpha_in_rgba_order() {
            string courseSourcePath = @"C:\dev\helprojs\city\assets\codebase\rendering.tools\TiltTrialCourseMaterialFactory.cs";
            string walnutSourcePath = @"C:\dev\helprojs\city\assets\codebase\rendering.tools\TiltTrialPlayerSphereWalnutMaterialFactory.cs";
            string courseSource = File.ReadAllText(courseSourcePath);
            string walnutSource = File.ReadAllText(walnutSourcePath);

            Assert.Contains("const string CourseBaseColor = \"#F4E8D8FF\";", courseSource, StringComparison.Ordinal);
            Assert.DoesNotContain("const string CourseBaseColor = \"#FFF4E8D8\";", courseSource, StringComparison.Ordinal);
            Assert.Contains("const string WalnutBaseColor = \"#F0E2CEFF\";", walnutSource, StringComparison.Ordinal);
            Assert.DoesNotContain("const string WalnutBaseColor = \"#FFF0E2CE\";", walnutSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the generated Tilt Trial course material asset on disk matches the authored opaque RGBA color expected by the factories.
        /// </summary>
        [Fact]
        public void Tilt_trial_course_material_asset_keeps_opaque_alpha_in_rgba_order() {
            string courseMaterialAssetPath = @"C:\dev\helprojs\city\assets\materials\rendering\tilt_trial\Course.hasset";
            string courseMaterialAsset = File.ReadAllText(courseMaterialAssetPath);

            Assert.Contains("#F4E8D8FF", courseMaterialAsset, StringComparison.Ordinal);
            Assert.DoesNotContain("#FFF4E8D8", courseMaterialAsset, StringComparison.Ordinal);
        }
    }
}
