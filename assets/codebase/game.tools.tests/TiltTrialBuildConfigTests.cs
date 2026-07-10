using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies project build configurations package every Tilt Trial gameplay scene when the selector front door is selected.
    /// </summary>
    public sealed class TiltTrialBuildConfigTests {
        static readonly string[] RequiredTiltTrialSceneIds = [
            city.game.TiltTrialSceneIds.LevelSelectSceneId,
            city.game.TiltTrialSceneIds.Level01SceneId,
            city.game.TiltTrialSceneIds.Level02SceneId,
            city.game.TiltTrialSceneIds.Level03SceneId,
            city.game.TiltTrialSceneIds.Level04SceneId,
            city.game.TiltTrialSceneIds.Level05SceneId
        ];

        [Fact]
        public void Build_configs_that_package_tilt_trial_also_package_every_tilt_trial_level() {
            string json = File.ReadAllText(@"C:\dev\helprojs\city\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement.ArrayEnumerator platforms = document.RootElement.GetProperty("platforms").EnumerateArray();
            foreach (JsonElement platform in platforms) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                HashSet<string> selectedSceneIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonElement sceneId in platform.GetProperty("selectedSceneIds").EnumerateArray()) {
                    selectedSceneIds.Add(sceneId.GetString() ?? string.Empty);
                }

                if (!selectedSceneIds.Contains(city.game.TiltTrialSceneIds.LevelSelectSceneId)) {
                    continue;
                }

                foreach (string requiredSceneId in RequiredTiltTrialSceneIds) {
                    Assert.Contains(requiredSceneId, selectedSceneIds);
                }
            }
        }
    }
}
