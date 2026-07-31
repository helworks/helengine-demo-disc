using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies the menu-browsable scene package for standard platforms and the isolated renderer-performance package for PS2.
    /// </summary>
    public sealed class DemoDiscBuildConfigTests {
        /// <summary>
        /// Ordered scene ids shared by menu-browsable platforms except Nintendo DS, Nintendo 3DS, and the dedicated PS2 renderer-performance export.
        /// </summary>
        static readonly string[] CommonNonHandheldSceneIds = [
            "HelenOfCodeSplash",
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
        /// Ensures every menu-browsable non-handheld platform uses the same ordered scene package and starts with the main menu.
        /// </summary>
        [Fact]
        public void Non_handheld_platforms_share_the_main_menu_scene_package() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement.ArrayEnumerator platforms = document.RootElement.GetProperty("platforms").EnumerateArray();
            foreach (JsonElement platform in platforms) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                if (string.Equals(platformId, "windows", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(platformId, "ds", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(platformId, "3ds", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(platformId, "ps2", StringComparison.OrdinalIgnoreCase)) {
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
        /// Ensures the Windows package contains the persistent scene that presents every normal transition.
        /// </summary>
        [Fact]
        public void Windows_platform_packages_the_persistent_loading_screen_scene() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement windowsPlatform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "windows", StringComparison.OrdinalIgnoreCase));

            string[] selectedSceneIds = windowsPlatform.GetProperty("selectedSceneIds")
                .EnumerateArray()
                .Select(sceneId => sceneId.GetString() ?? string.Empty)
                .ToArray();
            string[] orderedSceneIds = windowsPlatform.GetProperty("sceneOrders")
                .EnumerateArray()
                .OrderBy(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32())
                .Select(sceneOrder => sceneOrder.GetProperty("sceneId").GetString() ?? string.Empty)
                .ToArray();

            Assert.Contains("SceneLoadingScreen", selectedSceneIds);
            Assert.Contains("SceneLoadingScreen", orderedSceneIds);
        }

        /// <summary>
        /// Ensures the Wii U package includes the persistent loading scene required by the splash transition path.
        /// </summary>
        [Fact]
        public void Wii_u_platform_packages_the_persistent_loading_screen_scene() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement wiiUPlatform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "wiiu", StringComparison.OrdinalIgnoreCase));

            string[] selectedSceneIds = wiiUPlatform.GetProperty("selectedSceneIds")
                .EnumerateArray()
                .Select(sceneId => sceneId.GetString() ?? string.Empty)
                .ToArray();
            string[] orderedSceneIds = wiiUPlatform.GetProperty("sceneOrders")
                .EnumerateArray()
                .OrderBy(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32())
                .Select(sceneOrder => sceneOrder.GetProperty("sceneId").GetString() ?? string.Empty)
                .ToArray();

            Assert.Contains("SceneLoadingScreen", selectedSceneIds);
            Assert.Contains("SceneLoadingScreen", orderedSceneIds);
        }

        /// <summary>
        /// Ensures the Wii package includes the persistent loading scene required by the splash transition path.
        /// </summary>
        [Fact]
        public void Wii_platform_packages_the_persistent_loading_screen_scene() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement wiiPlatform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "wii", StringComparison.OrdinalIgnoreCase));

            string[] selectedSceneIds = wiiPlatform.GetProperty("selectedSceneIds")
                .EnumerateArray()
                .Select(sceneId => sceneId.GetString() ?? string.Empty)
                .ToArray();
            string[] orderedSceneIds = wiiPlatform.GetProperty("sceneOrders")
                .EnumerateArray()
                .OrderBy(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32())
                .Select(sceneOrder => sceneOrder.GetProperty("sceneId").GetString() ?? string.Empty)
                .ToArray();

            Assert.Contains("SceneLoadingScreen", selectedSceneIds);
            Assert.Contains("SceneLoadingScreen", orderedSceneIds);
        }

        /// <summary>
        /// Ensures the dedicated PS2 renderer-performance export contains only the Level 1 render-test scene so startup enters it directly.
        /// </summary>
        [Fact]
        public void Ps2_renderer_performance_export_boots_the_level_01_render_test_scene() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement ps2Platform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "ps2", StringComparison.Ordinal));
            string[] selectedSceneIds = ps2Platform.GetProperty("selectedSceneIds")
                .EnumerateArray()
                .Select(sceneId => sceneId.GetString() ?? string.Empty)
                .ToArray();
            string[] orderedSceneIds = ps2Platform.GetProperty("sceneOrders")
                .EnumerateArray()
                .OrderBy(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32())
                .Select(sceneOrder => sceneOrder.GetProperty("sceneId").GetString() ?? string.Empty)
                .ToArray();

            Assert.Equal(new[] { "test_scene_tilt_trial_level_01_render" }, selectedSceneIds);
            Assert.Equal(new[] { "test_scene_tilt_trial_level_01_render" }, orderedSceneIds);
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
                if (string.Equals(commonSceneId, "HelenOfCodeSplash", StringComparison.Ordinal)) {
                    continue;
                }

                string expectedSceneId = commonSceneId switch {
                    "DemoDiscMainMenu" => "DemoDiscMainMenuHandheld",
                    "tilt_trial" => "tilt_trial_ds",
                    _ => commonSceneId
                };
                Assert.Contains(expectedSceneId, selectedSceneIds);
            }
        }

        /// <summary>
        /// Ensures the Nintendo handheld startup packages remain independent from the standard splash scene.
        /// </summary>
        [Fact]
        public void Handheld_platforms_do_not_package_the_standard_splash_scene() {
            string json = File.ReadAllText(@"C:\dev\helprojs\demodisc\user_settings\build_config.json");
            using JsonDocument document = JsonDocument.Parse(json);

            foreach (JsonElement platform in document.RootElement.GetProperty("platforms").EnumerateArray()) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                if (!string.Equals(platformId, "ds", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(platformId, "3ds", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                string[] selectedSceneIds = platform.GetProperty("selectedSceneIds")
                    .EnumerateArray()
                    .Select(sceneId => sceneId.GetString() ?? string.Empty)
                    .ToArray();
                Assert.DoesNotContain("HelenOfCodeSplash", selectedSceneIds);
            }
        }
    }
}
