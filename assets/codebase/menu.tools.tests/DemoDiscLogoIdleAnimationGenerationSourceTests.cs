using Xunit;

namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies that the demo-disc logo animation is authored through the current public asset writer.
    /// </summary>
    public sealed class DemoDiscLogoIdleAnimationGenerationSourceTests {
        /// <summary>
        /// Ensures main-menu regeneration invokes the current animation generator instead of reading a stale fixture.
        /// </summary>
        [Fact]
        public void Main_menu_generation_authors_logo_animation_through_current_writer() {
            string commandSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\RegenerateDemoDiscMainMenuCommand.cs");
            string generatorSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscLogoIdleAnimationGenerator.cs");

            Assert.Contains("DemoDiscLogoIdleAnimationGenerator", commandSource, StringComparison.Ordinal);
            Assert.Contains("IEditorProjectAuthoringSession", generatorSource, StringComparison.Ordinal);
            Assert.Contains("WriteAsset", generatorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("GeneratedAssetWriteService", generatorSource, StringComparison.Ordinal);
            Assert.Contains("RotationTracks", generatorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("EditorAssetBinarySerializer", generatorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("AssetSerializer", generatorSource, StringComparison.Ordinal);
        }
    }
}
