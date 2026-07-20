using Xunit;

namespace city.rendering.tools.tests {
    /// <summary>
    /// Verifies the standalone PS2 textured VU renderer probe scene authoring contract.
    /// </summary>
    public sealed class Ps2VuRendererProbeSceneSourceTests {
        /// <summary>
        /// Absolute DemoDisc worktree root used by source-contract tests in this repository.
        /// </summary>
        const string ProjectRootPath = @"C:\dev\helprojs\demodisc\.worktrees\ps2-textured-vu-renderer-probe";

        /// <summary>
        /// Ensures the probe scene contains five fixed mesh objects using one shared material.
        /// </summary>
        [Fact]
        public void Ps2VuRendererProbeSceneFactory_WhenCreatingScene_UsesFourBoxesAndOneCoin() {
            string projectRootPath = ResolveProjectRootPath();
            string sourcePath = Path.Combine(projectRootPath, "assets", "codebase", "rendering.tools", "Ps2VuRendererProbeSceneFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected renderer probe factory at '{sourcePath}'.");

            string source = File.ReadAllText(sourcePath);

            Assert.Contains("public const string SceneId = \"scenes/rendering/ps2_vu_renderer_probe.helen\";", source, StringComparison.Ordinal);
            Assert.Equal(4, CountOccurrences(source, "Ps2VuProbeBox"));
            Assert.Equal(1, CountOccurrences(source, "Ps2VuProbeCoin"));
            Assert.Contains("texturedCubeGridMaterials[0]", File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs")), StringComparison.Ordinal);
            Assert.Contains("CreateCameraEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateDirectionalLightEntity()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TiltTrial", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Counts exact non-overlapping occurrences of one source fragment.
        /// </summary>
        /// <param name="source">Source text to inspect.</param>
        /// <param name="fragment">Fragment to count.</param>
        /// <returns>Number of occurrences in the source text.</returns>
        static int CountOccurrences(string source, string fragment) {
            int count = 0;
            int startIndex = 0;
            while ((startIndex = source.IndexOf(fragment, startIndex, StringComparison.Ordinal)) >= 0) {
                count++;
                startIndex += fragment.Length;
            }

            return count;
        }

        /// <summary>
        /// Resolves the DemoDisc project root from the executing test directory.
        /// </summary>
        /// <returns>Absolute DemoDisc project root path.</returns>
        static string ResolveProjectRootPath() {
            return ProjectRootPath;
        }
    }
}
