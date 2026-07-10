using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies the first Zombislayer slice is packaged only for the Windows demo-disc build.
    /// </summary>
    public sealed class ZombislayerBuildConfigTests {
        /// <summary>
        /// Ensures the Windows build packages Zombislayer and assigns it a stable scene-order slot, while other platforms omit it.
        /// </summary>
        [Fact]
        public void Windows_build_packages_zombislayer_and_other_platforms_omit_it() {
            string json = File.ReadAllText(@"C:\dev\helprojs\city\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement.ArrayEnumerator platforms = document.RootElement.GetProperty("platforms").EnumerateArray();
            foreach (JsonElement platform in platforms) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                HashSet<string> selectedSceneIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonElement sceneId in platform.GetProperty("selectedSceneIds").EnumerateArray()) {
                    selectedSceneIds.Add(sceneId.GetString() ?? string.Empty);
                }

                if (string.Equals(platformId, "windows", StringComparison.Ordinal)) {
                    Assert.Contains(city.game.ZombislayerSceneIds.GameplaySceneId, selectedSceneIds);
                    Assert.Contains(
                        platform.GetProperty("sceneOrders").EnumerateArray(),
                        sceneOrder => string.Equals(sceneOrder.GetProperty("sceneId").GetString(), city.game.ZombislayerSceneIds.GameplaySceneId, StringComparison.Ordinal)
                            && sceneOrder.GetProperty("orderNumber").GetInt32() == 21);
                    continue;
                }

                Assert.DoesNotContain(city.game.ZombislayerSceneIds.GameplaySceneId, selectedSceneIds);
            }
        }
    }
}
