using System.Text.Json;
using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies Zombislayer is not part of any current demo-disc platform package.
    /// </summary>
    public sealed class ZombislayerBuildConfigTests {
        /// <summary>
        /// Ensures every platform's project scene package omits the retired Zombislayer scene.
        /// </summary>
        [Fact]
        public void All_platform_builds_omit_zombislayer() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();

            foreach (JsonElement platform in document.RootElement.GetProperty("platforms").EnumerateArray()) {
                Assert.DoesNotContain(DemoDisc.Zombislayer.ZombislayerSceneIds.GameplaySceneId, DemoDiscBuildConfigTestPaths.SceneIdsOf(platform));
            }
        }
    }
}
