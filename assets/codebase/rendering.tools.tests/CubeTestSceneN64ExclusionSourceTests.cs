using city.testing;

namespace city.tests {
    /// <summary>
    /// Verifies the cube-test scene keeps its UI root on N64 while still stripping the two input-driven
    /// controls that N64 has no input handling to drive.
    /// </summary>
    public sealed class CubeTestSceneN64ExclusionSourceTests {
        static readonly string SourcePath = DemoDiscTestProject.GetPath("assets", "codebase", "rendering.tools", "CubeTestSceneFactory.cs");

        [Fact]
        public void Textless_tier_is_declared_as_dreamcast_only() {
            string source = File.ReadAllText(SourcePath);

            Assert.Contains(
                "static readonly string[] TextlessRuntimeExcludedPlatformIds = [DreamcastPlatformId];",
                source,
                StringComparison.Ordinal);
        }

        [Fact]
        public void Ui_root_is_excluded_on_the_textless_tier_instead_of_the_minimal_tier() {
            string source = File.ReadAllText(SourcePath);

            Assert.Contains(
                "ExcludeN64Root(projectRootPath, uiEntity, TextlessRuntimeExcludedPlatformIds);",
                source,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "ExcludeN64Root(projectRootPath, uiEntity, MinimalRuntimeExcludedPlatformIds);",
                source,
                StringComparison.Ordinal);
        }

        [Fact]
        public void Desktop_overlay_stays_minimal_and_console_blueprint_stays_spriteless() {
            string source = File.ReadAllText(SourcePath);

            Assert.Contains(
                "ExcludeN64Root(projectRootPath, instructionOverlayEntity, MinimalRuntimeExcludedPlatformIds);",
                source,
                StringComparison.Ordinal);
            Assert.Contains(
                "ExcludeN64Root(projectRootPath, consoleInstructionBlueprintEntity, SpritelessRuntimeExcludedPlatformIds);",
                source,
                StringComparison.Ordinal);
        }

        [Fact]
        public void Return_to_menu_and_light_toggle_components_are_excluded_from_n64_alone() {
            string source = File.ReadAllText(SourcePath);

            Assert.Contains(
                "static readonly string[] Nintendo64OnlyExcludedPlatformIds = [Nintendo64PlatformId];",
                source,
                StringComparison.Ordinal);
            Assert.Contains(
                "ExcludeN64UiInputComponents(projectRootPath, uiEntity, Nintendo64OnlyExcludedPlatformIds);",
                source,
                StringComparison.Ordinal);

            int methodIndex = source.IndexOf("void ExcludeN64UiInputComponents(", StringComparison.Ordinal);
            Assert.True(methodIndex >= 0);
            string methodBody = source.Substring(methodIndex);

            Assert.Contains("city.menu.DemoDiscReturnToMenuComponent>()", methodBody, StringComparison.Ordinal);
            Assert.Contains("city.rendering.DemoDiscLightToggleComponent>()", methodBody, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(methodBody.Substring(0, methodBody.IndexOf("\n        }", StringComparison.Ordinal)), "ExcludeComponentFromPlatforms("));
        }

        static int CountOccurrences(string haystack, string needle) {
            int count = 0;
            int searchIndex = 0;
            while (true) {
                int foundIndex = haystack.IndexOf(needle, searchIndex, StringComparison.Ordinal);
                if (foundIndex < 0) {
                    break;
                }

                count++;
                searchIndex = foundIndex + needle.Length;
            }

            return count;
        }
    }
}
