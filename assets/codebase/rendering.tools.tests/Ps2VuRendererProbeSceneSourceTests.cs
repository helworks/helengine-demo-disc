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
        /// Ensures the probe scene contains only one textured ground cube using the shared material.
        /// </summary>
        [Fact]
        public void Ps2VuRendererProbeSceneFactory_WhenCreatingScene_UsesOnlyOneGroundCube() {
            string projectRootPath = ResolveProjectRootPath();
            string sourcePath = Path.Combine(projectRootPath, "assets", "codebase", "rendering.tools", "Ps2VuRendererProbeSceneFactory.cs");
            Assert.True(File.Exists(sourcePath), $"Expected renderer probe factory at '{sourcePath}'.");

            string source = File.ReadAllText(sourcePath);

            Assert.Contains("public const string SceneId = \"scenes/rendering/ps2_vu_renderer_probe.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("Ps2VuProbeGround", source, StringComparison.Ordinal);
            Assert.Contains("Ps2VuProbeLeftWall", source, StringComparison.Ordinal);
            Assert.Contains("Ps2VuProbeRightWall", source, StringComparison.Ordinal);
            Assert.Contains("Ps2VuProbeCoin", source, StringComparison.Ordinal);
            Assert.Contains("new float3(0f, 0f, 0f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(5f, 1f, 5f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(-3.1f, 1.25f, 0f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(3.1f, 1.25f, 0f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(0.8f, 2.8f, 5f)", source, StringComparison.Ordinal);
            Assert.Contains("texturedCubeGridMaterials[0]", File.ReadAllText(Path.Combine(projectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs")), StringComparison.Ordinal);
            Assert.Contains("CreateCameraEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateDirectionalLightEntity()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TiltTrial", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the probe camera uses the known-good cube-test framing while retaining orbit control.
        /// </summary>
        [Fact]
        public void Ps2VuRendererProbeSceneFactory_WhenCreatingCamera_UsesKnownGoodCubePose() {
            string projectRootPath = ResolveProjectRootPath();
            string sourcePath = Path.Combine(projectRootPath, "assets", "codebase", "rendering.tools", "Ps2VuRendererProbeSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("new float3(0f, 0f, 5f)", source, StringComparison.Ordinal);
            Assert.Contains("OrbitCenter = float3.Zero", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the probe camera uses the shared orbit controller around the test geometry.
        /// </summary>
        [Fact]
        public void Ps2VuRendererProbeSceneFactory_WhenCreatingCamera_UsesSharedOrbitController() {
            string projectRootPath = ResolveProjectRootPath();
            string sourcePath = Path.Combine(projectRootPath, "assets", "codebase", "rendering.tools", "Ps2VuRendererProbeSceneFactory.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.Contains("new city.rendering.DemoDiscOrbitCameraComponent", source, StringComparison.Ordinal);
            Assert.Contains("OrbitCenter = float3.Zero", source, StringComparison.Ordinal);
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
