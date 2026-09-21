using System.Text.Json;
using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the project scene package ships every Tilt Trial gameplay scene wherever a selector front door is packaged.
    /// </summary>
    public sealed class TiltTrialBuildConfigTests {
        static readonly string[] RequiredTiltTrialGameplaySceneIds = [
            DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId,
            DemoDisc.TiltPlay.TiltTrialSceneIds.Level02SceneId,
            DemoDisc.TiltPlay.TiltTrialSceneIds.Level03SceneId,
            DemoDisc.TiltPlay.TiltTrialSceneIds.Level04SceneId,
            DemoDisc.TiltPlay.TiltTrialSceneIds.Level05SceneId
        ];

        [Fact]
        public void Build_configs_that_package_tilt_trial_also_package_every_tilt_trial_level() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();

            foreach (JsonElement platform in document.RootElement.GetProperty("platforms").EnumerateArray()) {
                HashSet<string> selectedSceneIds = new HashSet<string>(DemoDiscBuildConfigTestPaths.SceneIdsOf(platform), StringComparer.Ordinal);
                if (!selectedSceneIds.Contains(DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId)
                    && !selectedSceneIds.Contains(DemoDisc.TiltPlay.TiltTrialSceneIds.HandheldLevelSelectSceneId)) {
                    continue;
                }

                foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                    Assert.Contains(requiredSceneId, selectedSceneIds);
                }
            }
        }

        [Fact]
        public void Windows_build_starts_with_demo_disc_main_menu_without_removing_other_tilt_trial_scenes() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();
            JsonElement windowsPlatform = DemoDiscBuildConfigTestPaths.FindPlatform(document, "windows");
            string[] sceneIds = DemoDiscBuildConfigTestPaths.SceneIdsOf(windowsPlatform);

            Assert.Equal("DemoDiscMainMenu", sceneIds[2]);
            Assert.Equal(3, DemoDiscBuildConfigTestPaths.OrderNumberOf(windowsPlatform, "DemoDiscMainMenu"));

            HashSet<string> selectedSceneIds = new HashSet<string>(sceneIds, StringComparer.Ordinal);
            foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                Assert.Contains(requiredSceneId, selectedSceneIds);
            }
        }

        /// <summary>
        /// Ensures the Nintendo DS package ships the Tilt Trial selector and every authored Tilt Trial level with stable scene-order slots.
        /// </summary>
        [Fact]
        public void Nintendo_ds_build_packages_tilt_trial_selector_and_all_levels() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();
            JsonElement dsPlatform = DemoDiscBuildConfigTestPaths.FindPlatform(document, "ds");
            HashSet<string> selectedSceneIds = new HashSet<string>(DemoDiscBuildConfigTestPaths.SceneIdsOf(dsPlatform), StringComparer.Ordinal);

            Assert.Contains(DemoDisc.TiltPlay.TiltTrialSceneIds.HandheldLevelSelectSceneId, selectedSceneIds);
            Assert.Contains(helengine.PlatformMenuSceneResolver.NintendoHandheldMainMenuSceneId, selectedSceneIds);
            foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                Assert.Contains(requiredSceneId, selectedSceneIds);
            }

            Assert.Equal(12, DemoDiscBuildConfigTestPaths.OrderNumberOf(dsPlatform, DemoDisc.TiltPlay.TiltTrialSceneIds.HandheldLevelSelectSceneId));
            Assert.Equal(13, DemoDiscBuildConfigTestPaths.OrderNumberOf(dsPlatform, DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId));
            Assert.Equal(17, DemoDiscBuildConfigTestPaths.OrderNumberOf(dsPlatform, DemoDisc.TiltPlay.TiltTrialSceneIds.Level05SceneId));
        }

        /// <summary>
        /// Ensures the Nintendo 3DS package selects its handheld selector while retaining the shared gameplay scenes.
        /// </summary>
        [Fact]
        public void Nintendo_3ds_build_packages_handheld_selector_and_shared_levels() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();
            JsonElement platform = DemoDiscBuildConfigTestPaths.FindPlatform(document, "3ds");
            HashSet<string> selectedSceneIds = new HashSet<string>(DemoDiscBuildConfigTestPaths.SceneIdsOf(platform), StringComparer.Ordinal);

            Assert.Contains(DemoDisc.TiltPlay.TiltTrialSceneIds.HandheldLevelSelectSceneId, selectedSceneIds);
            Assert.DoesNotContain(DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId, selectedSceneIds);
            foreach (string requiredSceneId in RequiredTiltTrialGameplaySceneIds) {
                Assert.Contains(requiredSceneId, selectedSceneIds);
            }
        }
    }
}
