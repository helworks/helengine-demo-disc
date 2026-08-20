using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies project build configurations package every Tilt Trial gameplay scene when the selector front door is selected.
    /// </summary>
    public sealed class TiltTrialBuildConfigTests {
        static readonly string[] RequiredTiltTrialGameplaySceneIds = [
            city.game.TiltTrialSceneIds.Level01SceneId,
            city.game.TiltTrialSceneIds.Level02SceneId,
            city.game.TiltTrialSceneIds.Level03SceneId,
            city.game.TiltTrialSceneIds.Level04SceneId,
            city.game.TiltTrialSceneIds.Level05SceneId
        ];

        [Fact]
        public void Build_configs_that_package_tilt_trial_also_package_every_tilt_trial_level() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement.ArrayEnumerator platforms = document.RootElement.GetProperty("platforms").EnumerateArray();
            foreach (JsonElement platform in platforms) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                HashSet<string> selectedSceneIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonElement sceneId in platform.GetProperty("selectedSceneIds").EnumerateArray()) {
                    selectedSceneIds.Add(sceneId.GetString() ?? string.Empty);
                }

                if (!selectedSceneIds.Contains(city.game.TiltTrialSceneIds.LevelSelectSceneId)
                    && !selectedSceneIds.Contains(city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId)) {
                    continue;
                }

                foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                    Assert.Contains(requiredSceneId, selectedSceneIds);
                }
            }
        }

        [Fact]
        public void Windows_build_starts_with_demo_disc_main_menu_without_removing_other_tilt_trial_scenes() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement windowsPlatform = document.RootElement
                .GetProperty("platforms")
                .EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "windows", StringComparison.Ordinal));

            Assert.Equal("DemoDiscMainMenu", windowsPlatform.GetProperty("selectedSceneIds")[2].GetString());
            Assert.Contains(
                windowsPlatform.GetProperty("sceneOrders").EnumerateArray(),
                sceneOrder => string.Equals(sceneOrder.GetProperty("sceneId").GetString(), "DemoDiscMainMenu", StringComparison.Ordinal)
                    && sceneOrder.GetProperty("orderNumber").GetInt32() == 3);

            HashSet<string> selectedSceneIds = new HashSet<string>(
                windowsPlatform.GetProperty("selectedSceneIds").EnumerateArray().Select(sceneId => sceneId.GetString() ?? string.Empty),
                StringComparer.Ordinal);
            foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                Assert.Contains(requiredSceneId, selectedSceneIds);
            }
        }

        /// <summary>
        /// Ensures the Nintendo DS build packages the Tilt Trial selector and every authored Tilt Trial level with stable scene-order slots.
        /// </summary>
        [Fact]
        public void Nintendo_ds_build_packages_tilt_trial_selector_and_all_levels() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement dsPlatform = document.RootElement
                .GetProperty("platforms")
                .EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "ds", StringComparison.Ordinal));

            HashSet<string> selectedSceneIds = new HashSet<string>(
                dsPlatform.GetProperty("selectedSceneIds").EnumerateArray().Select(sceneId => sceneId.GetString() ?? string.Empty),
                StringComparer.Ordinal);
            Assert.Contains(city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId, selectedSceneIds);
            Assert.Contains(helengine.PlatformMenuSceneResolver.NintendoHandheldMainMenuSceneId, selectedSceneIds);
            foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                Assert.Contains(requiredSceneId, selectedSceneIds);
            }

            Assert.Contains(
                dsPlatform.GetProperty("sceneOrders").EnumerateArray(),
                sceneOrder => string.Equals(sceneOrder.GetProperty("sceneId").GetString(), city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId, StringComparison.Ordinal)
                    && sceneOrder.GetProperty("orderNumber").GetInt32() == 12);
            Assert.Contains(
                dsPlatform.GetProperty("sceneOrders").EnumerateArray(),
                sceneOrder => string.Equals(sceneOrder.GetProperty("sceneId").GetString(), city.game.TiltTrialSceneIds.Level01SceneId, StringComparison.Ordinal)
                    && sceneOrder.GetProperty("orderNumber").GetInt32() == 13);
            Assert.Contains(
                dsPlatform.GetProperty("sceneOrders").EnumerateArray(),
                sceneOrder => string.Equals(sceneOrder.GetProperty("sceneId").GetString(), city.game.TiltTrialSceneIds.Level05SceneId, StringComparison.Ordinal)
                    && sceneOrder.GetProperty("orderNumber").GetInt32() == 17);
        }

        /// <summary>
        /// Ensures the Nintendo 3DS build selects its handheld selector while retaining the shared gameplay scenes.
        /// </summary>
        [Fact]
        public void Nintendo_3ds_build_packages_handheld_selector_and_shared_levels() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement platform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(value => string.Equals(value.GetProperty("platformId").GetString(), "3ds", StringComparison.Ordinal));
            HashSet<string> selectedSceneIds = new HashSet<string>(
                platform.GetProperty("selectedSceneIds").EnumerateArray().Select(sceneId => sceneId.GetString() ?? string.Empty),
                StringComparer.Ordinal);

            Assert.Contains(city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId, selectedSceneIds);
            Assert.DoesNotContain(city.game.TiltTrialSceneIds.LevelSelectSceneId, selectedSceneIds);
            foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                Assert.Contains(requiredSceneId, selectedSceneIds);
            }
        }
    }
}
