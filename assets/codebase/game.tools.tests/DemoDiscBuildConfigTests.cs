using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies non-handheld demo-disc builds package the same menu-browsable scene set and boot into the main menu.
    /// </summary>
    public sealed class DemoDiscBuildConfigTests {
        /// <summary>
        /// Ordered scene ids shared by every platform except Nintendo DS and Nintendo 3DS.
        /// </summary>
        static readonly string[] CommonNonHandheldSceneIds = [
            "DemoDiscMainMenu",
            "cube_test",
            "colored_cube_grid",
            "textured_cube_grid",
            "axis_test",
            "axis_test2",
            "test_scene_matrix_render",
            "directional_shadow_plaza",
            "test_scene_dynamic_stack_boxes",
            "test_scene_dynamic_sphere_stack",
            "test_scene_dynamic_mixed_stack",
            "test_scene_static_mesh_showcase",
            "test_scene_static_mesh_minimal",
            "tilt_trial",
            "tilt_trial_level_01",
            "tilt_trial_level_02",
            "tilt_trial_level_03",
            "tilt_trial_level_04",
            "tilt_trial_level_05"
        ];

        /// <summary>
        /// Ensures every non-handheld platform uses the same ordered scene package and starts with the main menu.
        /// </summary>
        [Fact]
        public void Non_handheld_platforms_share_the_main_menu_scene_package() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement.ArrayEnumerator platforms = document.RootElement.GetProperty("platforms").EnumerateArray();
            foreach (JsonElement platform in platforms) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                if (string.Equals(platformId, "ds", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(platformId, "3ds", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                string[] selectedSceneIds = platform.GetProperty("selectedSceneIds")
                    .EnumerateArray()
                    .Select(sceneId => sceneId.GetString() ?? string.Empty)
                    .ToArray();
                string[] orderedSceneIds = platform.GetProperty("sceneOrders")
                    .EnumerateArray()
                    .OrderBy(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32())
                    .Select(sceneOrder => sceneOrder.GetProperty("sceneId").GetString() ?? string.Empty)
                    .ToArray();

                Assert.Equal(CommonNonHandheldSceneIds, selectedSceneIds);
                Assert.Equal(CommonNonHandheldSceneIds, orderedSceneIds);
            }
        }

        /// <summary>
        /// Ensures Nintendo DS retains the shared rendering and physics scene package while replacing only the menu and Tilt Trial selector ids.
        /// </summary>
        [Fact]
        public void Nintendo_ds_shares_the_common_demo_scene_package() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement dsPlatform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "ds", StringComparison.Ordinal));
            HashSet<string> selectedSceneIds = new HashSet<string>(
                dsPlatform.GetProperty("selectedSceneIds").EnumerateArray().Select(sceneId => sceneId.GetString() ?? string.Empty),
                StringComparer.Ordinal);

            foreach (string commonSceneId in CommonNonHandheldSceneIds) {
                string expectedSceneId = commonSceneId switch {
                    "DemoDiscMainMenu" => "DemoDiscMainMenuHandheld",
                    "tilt_trial" => "tilt_trial_ds",
                    _ => commonSceneId
                };
                Assert.Contains(expectedSceneId, selectedSceneIds);
            }
        }
    }
}
