using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies Zombislayer is not part of any current demo-disc platform build.
    /// </summary>
    public sealed class ZombislayerBuildConfigTests {
        /// <summary>
        /// Ensures every configured platform omits the retired Zombislayer scene.
        /// </summary>
        [Fact]
        public void All_platform_builds_omit_zombislayer() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement.ArrayEnumerator platforms = document.RootElement.GetProperty("platforms").EnumerateArray();
            foreach (JsonElement platform in platforms) {
                HashSet<string> selectedSceneIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonElement sceneId in platform.GetProperty("selectedSceneIds").EnumerateArray()) {
                    selectedSceneIds.Add(sceneId.GetString() ?? string.Empty);
                }

                Assert.DoesNotContain(city.game.ZombislayerSceneIds.GameplaySceneId, selectedSceneIds);
            }
        }
    }
}
