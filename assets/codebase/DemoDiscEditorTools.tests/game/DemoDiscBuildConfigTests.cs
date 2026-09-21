using System.Text.Json;
using System.Runtime.CompilerServices;
using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the project-shared scene package in <c>settings/build_config.json</c>: the menu-browsable package for
    /// standard platforms and the handheld package for Nintendo DS and 3DS.
    /// </summary>
    public sealed class DemoDiscBuildConfigTests {
        /// <summary>
        /// Ordered scene ids shared by every menu-browsable platform except Nintendo DS and Nintendo 3DS.
        /// </summary>
        static readonly string[] CommonNonHandheldSceneIds = [
            "HelenOfCodeSplash",
            "SceneLoadingScreen",
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
            "tilt_trial",
            "tilt_trial_level_01",
            "tilt_trial_level_02",
            "tilt_trial_level_03",
            "tilt_trial_level_04",
            "tilt_trial_level_05",
            "pbr_material_gallery",
            "pbr_textured_showcase",
            "pbr_shadow_theater",
            "software_path_tracer"
        ];

        /// <summary>
        /// Ensures every menu-browsable non-handheld platform ships the same ordered scene package and starts with the splash.
        /// </summary>
        [Fact]
        public void Non_handheld_platforms_share_the_main_menu_scene_package() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();

            foreach (JsonElement platform in document.RootElement.GetProperty("platforms").EnumerateArray()) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                if (string.Equals(platformId, "ds", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(platformId, "3ds", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                Assert.Equal(CommonNonHandheldSceneIds, DemoDiscBuildConfigTestPaths.SceneIdsOf(platform));
                Assert.Equal(CommonNonHandheldSceneIds, DemoDiscBuildConfigTestPaths.OrderedSceneIdsOf(platform));
            }
        }

        /// <summary>
        /// Ensures the PSP debug and release profiles both regenerate the main menu before cooking.
        /// </summary>
        [Fact]
        public void Psp_debug_and_release_profiles_regenerate_the_demo_disc_main_menu() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadLocalBuildConfig();
            JsonElement pspPlatform = document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "psp", StringComparison.Ordinal));
            JsonElement prebuildCommands = pspPlatform.GetProperty("editorPrebuildCommandIdsByBuildProfileId");

            foreach (string profileId in new[] { "debug", "release" }) {
                Assert.Contains(
                    prebuildCommands.GetProperty(profileId).EnumerateArray(),
                    command => string.Equals(command.GetString(), "menu.regenerate-demo-disc-main-menu", StringComparison.Ordinal));
            }
        }

        [Fact]
        public void Windows_platform_packages_the_persistent_loading_screen_scene() {
            AssertPlatformPackagesLoadingScreen("windows");
        }

        /// <summary>
        /// Ensures the Wii U package includes the persistent loading scene required by the splash transition path.
        /// </summary>
        [Fact]
        public void Wii_u_platform_packages_the_persistent_loading_screen_scene() {
            AssertPlatformPackagesLoadingScreen("wiiu");
        }

        /// <summary>
        /// Ensures the Wii package includes the persistent loading scene required by the splash transition path.
        /// </summary>
        [Fact]
        public void Wii_platform_packages_the_persistent_loading_screen_scene() {
            AssertPlatformPackagesLoadingScreen("wii");
        }

        /// <summary>
        /// Ensures Nintendo DS retains the shared rendering and physics scene package while replacing only the menu and Tilt Trial selector ids.
        /// </summary>
        [Fact]
        public void Nintendo_ds_shares_the_common_demo_scene_package() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();
            JsonElement dsPlatform = DemoDiscBuildConfigTestPaths.FindPlatform(document, "ds");
            HashSet<string> selectedSceneIds = new HashSet<string>(DemoDiscBuildConfigTestPaths.SceneIdsOf(dsPlatform), StringComparer.Ordinal);

            foreach (string commonSceneId in CommonNonHandheldSceneIds) {
                if (string.Equals(commonSceneId, "HelenOfCodeSplash", StringComparison.Ordinal)
                    || string.Equals(commonSceneId, "SceneLoadingScreen", StringComparison.Ordinal)) {
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
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();

            foreach (JsonElement platform in document.RootElement.GetProperty("platforms").EnumerateArray()) {
                string platformId = platform.GetProperty("platformId").GetString() ?? string.Empty;
                if (!string.Equals(platformId, "ds", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(platformId, "3ds", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                Assert.DoesNotContain("HelenOfCodeSplash", DemoDiscBuildConfigTestPaths.SceneIdsOf(platform));
            }
        }

        /// <summary>
        /// Ensures the local file carries no scene package for platforms that follow the project, so the package has one home.
        /// </summary>
        [Fact]
        public void Local_build_config_carries_scenes_only_for_platforms_with_a_local_override() {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadLocalBuildConfig();

            foreach (JsonElement platform in document.RootElement.GetProperty("platforms").EnumerateArray()) {
                bool overridesProjectScenes = platform.GetProperty("overridesProjectScenes").GetBoolean();
                int sceneCount = platform.GetProperty("selectedSceneReferences").GetArrayLength();
                Assert.True(overridesProjectScenes || sceneCount == 0, $"Platform '{platform.GetProperty("platformId").GetString()}' follows the project but lists {sceneCount} local scenes.");
            }
        }

        static void AssertPlatformPackagesLoadingScreen(string platformId) {
            using JsonDocument document = DemoDiscBuildConfigTestPaths.ReadProjectBuildConfig();
            JsonElement platform = DemoDiscBuildConfigTestPaths.FindPlatform(document, platformId);

            Assert.Contains("SceneLoadingScreen", DemoDiscBuildConfigTestPaths.SceneIdsOf(platform));
            Assert.Contains("SceneLoadingScreen", DemoDiscBuildConfigTestPaths.OrderedSceneIdsOf(platform));
        }
    }

    /// <summary>
    /// Locates and reads the two build configuration files and derives scene ids from their persisted scene references
    /// the way the engine does (scene file name without extension).
    /// </summary>
    internal static class DemoDiscBuildConfigTestPaths {
        /// <summary>
        /// Reads the project-shared scene package in <c>settings/build_config.json</c>.
        /// </summary>
        public static JsonDocument ReadProjectBuildConfig() {
            return JsonDocument.Parse(File.ReadAllText(Path.Combine(FindCheckoutRoot(), "settings", "build_config.json")));
        }

        /// <summary>
        /// Reads the machine-local build state in <c>user_settings/build_config.json</c>.
        /// </summary>
        public static JsonDocument ReadLocalBuildConfig() {
            return JsonDocument.Parse(File.ReadAllText(Path.Combine(FindCheckoutRoot(), "user_settings", "build_config.json")));
        }

        public static JsonElement FindPlatform(JsonDocument document, string platformId) {
            return document.RootElement.GetProperty("platforms").EnumerateArray()
                .Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), platformId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Scene ids of one platform entry in persisted order.
        /// </summary>
        public static string[] SceneIdsOf(JsonElement platform) {
            return platform.GetProperty("selectedSceneReferences")
                .EnumerateArray()
                .Select(SceneIdOfReference)
                .ToArray();
        }

        /// <summary>
        /// Scene ids of one platform entry sorted by their persisted order number.
        /// </summary>
        public static string[] OrderedSceneIdsOf(JsonElement platform) {
            return platform.GetProperty("sceneOrders")
                .EnumerateArray()
                .OrderBy(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32())
                .Select(sceneOrder => SceneIdOfReference(sceneOrder.GetProperty("sceneReference")))
                .ToArray();
        }

        /// <summary>
        /// Order number persisted for one scene of one platform entry, or -1 when the scene has no order entry.
        /// </summary>
        public static int OrderNumberOf(JsonElement platform, string sceneId) {
            foreach (JsonElement sceneOrder in platform.GetProperty("sceneOrders").EnumerateArray()) {
                if (string.Equals(SceneIdOfReference(sceneOrder.GetProperty("sceneReference")), sceneId, StringComparison.Ordinal)) {
                    return sceneOrder.GetProperty("orderNumber").GetInt32();
                }
            }

            return -1;
        }

        public static string SceneIdOfReference(JsonElement reference) {
            string relativePath = reference.GetProperty("relativePath").GetString() ?? string.Empty;
            return relativePath.EndsWith(".helen", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileNameWithoutExtension(relativePath)
                : relativePath;
        }

        public static string FindCheckoutRoot([CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (directory != null) {
                if (File.Exists(Path.Combine(directory.FullName, "project.heproj"))
                    && File.Exists(Path.Combine(directory.FullName, "settings", "build_config.json"))) {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate the Demo Disc checkout root.");
        }
    }
}
