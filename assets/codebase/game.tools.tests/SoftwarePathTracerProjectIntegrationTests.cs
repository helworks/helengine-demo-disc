using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace city.tests {
    /// <summary>
    /// Verifies every configured target packages and regenerates the shared software path tracer scene.
    /// </summary>
    public sealed class SoftwarePathTracerProjectIntegrationTests {
        static readonly string[] ExpectedPlatformIds = {
            "windows",
            "ps2",
            "psp",
            "psvita",
            "ds",
            "3ds",
            "gamecube",
            "wii",
            "switch",
            "wiiu"
        };

        static readonly string[] RequiredPrebuildCommandIds = {
            "menu.generate-rendering-scenes",
            "menu.regenerate-demo-disc-main-menu"
        };

        /// <summary>
        /// Ensures the project and canonical platform settings expose exactly the ten supported target identifiers.
        /// </summary>
        [Fact]
        public void Project_platforms_match_the_canonical_ten_target_set() {
            using JsonDocument project = LoadJson("project.heproj");
            using JsonDocument settings = LoadJson(Path.Combine("settings", "platforms.json"));

            string[] projectPlatformIds = project.RootElement.GetProperty("supportedPlatforms")
                .EnumerateArray()
                .Select(platformId => platformId.GetString() ?? string.Empty)
                .ToArray();
            string[] settingsPlatformIds = settings.RootElement.GetProperty("supportedPlatforms")
                .EnumerateArray()
                .Select(platformId => platformId.GetString() ?? string.Empty)
                .ToArray();

            Assert.Equal(ExpectedPlatformIds.OrderBy(platformId => platformId, StringComparer.Ordinal), projectPlatformIds.OrderBy(platformId => platformId, StringComparer.Ordinal));
            Assert.Equal(ExpectedPlatformIds.OrderBy(platformId => platformId, StringComparer.Ordinal), settingsPlatformIds.OrderBy(platformId => platformId, StringComparer.Ordinal));
            Assert.Equal(1, projectPlatformIds.Count(platformId => platformId == "gamecube"));
            Assert.DoesNotContain("gc", projectPlatformIds);
        }

        /// <summary>
        /// Ensures the isolated GameCube build catalog selects exactly one version-matched installed builder and its native payload roots.
        /// </summary>
        [Fact]
        public void GameCube_build_catalog_contains_one_version_matched_installed_descriptor() {
            using JsonDocument project = LoadJson("project.heproj");
            using JsonDocument catalog = LoadJson(Path.Combine("user_settings", "gamecube-build-platforms", "platforms.json"));

            string requiredEngineVersion = project.RootElement.GetProperty("requiredEngineVersion").GetString() ?? string.Empty;
            JsonElement[] platforms = catalog.RootElement.GetProperty("platforms").EnumerateArray().ToArray();
            JsonElement gameCube = Assert.Single(platforms, platform => platform.GetProperty("platformId").GetString() == "gamecube");

            Assert.Equal(requiredEngineVersion, gameCube.GetProperty("engineVersion").GetString());
            Assert.Equal(
                @"C:\dev\helprojs\.worktrees\helengine-gc-path-tracer-repair\builder\bin\Debug\net9.0\helengine.gamecube.builder.dll",
                gameCube.GetProperty("builderAssemblyPath").GetString());
            Assert.Equal(@"C:\dev\helprojs\.worktrees\helengine-gc-path-tracer-repair", gameCube.GetProperty("playerSourceRootPath").GetString());
            Assert.Equal(
                @"C:\dev\helworks\helengine\tmp\helengine-core-cpp-regenerated",
                gameCube.GetProperty("generatedCoreCppRootPath").GetString());
            Assert.Equal(
                @"C:\dev\helworks\csharpcodegen\codegen\bin\Release\net9.0\codegen.exe",
                gameCube.GetProperty("codegenToolPath").GetString());
        }

        /// <summary>
        /// Ensures the shared software path tracer scene follows PBR Shadow Theater in every target package.
        /// </summary>
        [Fact]
        public void Every_target_packages_software_path_tracer_after_pbr_shadow_theater() {
            using JsonDocument buildConfig = LoadJson(Path.Combine("user_settings", "build_config.json"));
            JsonElement[] platforms = buildConfig.RootElement.GetProperty("platforms").EnumerateArray().ToArray();

            foreach (string platformId in ExpectedPlatformIds) {
                JsonElement platform = Assert.Single(platforms, candidate => candidate.GetProperty("platformId").GetString() == platformId);
                string[] selectedSceneIds = platform.GetProperty("selectedSceneIds")
                    .EnumerateArray()
                    .Select(sceneId => sceneId.GetString() ?? string.Empty)
                    .ToArray();
                Assert.Equal(1, selectedSceneIds.Count(sceneId => sceneId == "software_path_tracer"));
                int pbrSceneIndex = Array.IndexOf(selectedSceneIds, "pbr_shadow_theater");
                Assert.Equal(pbrSceneIndex + 1, Array.IndexOf(selectedSceneIds, "software_path_tracer"));

                JsonElement[] sceneOrders = platform.GetProperty("sceneOrders").EnumerateArray().ToArray();
                JsonElement pbrSceneOrder = Assert.Single(sceneOrders, scene => scene.GetProperty("sceneId").GetString() == "pbr_shadow_theater");
                JsonElement softwarePathTracerSceneOrder = Assert.Single(sceneOrders, scene => scene.GetProperty("sceneId").GetString() == "software_path_tracer");
                Assert.Equal(pbrSceneOrder.GetProperty("orderNumber").GetInt32() + 1, softwarePathTracerSceneOrder.GetProperty("orderNumber").GetInt32());
            }
        }

        /// <summary>
        /// Ensures each selected target profile regenerates rendering scenes before regenerating the Demo Disc menu.
        /// </summary>
        [Fact]
        public void Selected_profiles_regenerate_rendering_scenes_before_the_demo_disc_menu() {
            using JsonDocument buildConfig = LoadJson(Path.Combine("user_settings", "build_config.json"));
            JsonElement[] platforms = buildConfig.RootElement.GetProperty("platforms").EnumerateArray().ToArray();

            foreach (string platformId in ExpectedPlatformIds) {
                JsonElement platform = Assert.Single(platforms, candidate => candidate.GetProperty("platformId").GetString() == platformId);
                string selectedProfileId = platform.GetProperty("selectedBuildProfileId").GetString() ?? string.Empty;
                JsonElement commandMap = platform.GetProperty("editorPrebuildCommandIdsByBuildProfileId");
                bool hasSelectedCommands = commandMap.TryGetProperty(selectedProfileId, out JsonElement selectedCommandArray);
                Assert.True(hasSelectedCommands, $"Selected profile '{selectedProfileId}' is missing from the prebuild command map for '{platformId}'.");
                string[] selectedCommands = hasSelectedCommands
                    ? selectedCommandArray.EnumerateArray().Select(commandId => commandId.GetString() ?? string.Empty).ToArray()
                    : Array.Empty<string>();

                Assert.Equal(1, selectedCommands.Count(commandId => commandId == RequiredPrebuildCommandIds[0]));
                Assert.Equal(1, selectedCommands.Count(commandId => commandId == RequiredPrebuildCommandIds[1]));
                Assert.True(Array.IndexOf(selectedCommands, RequiredPrebuildCommandIds[0]) < Array.IndexOf(selectedCommands, RequiredPrebuildCommandIds[1]));
            }
        }

        /// <summary>
        /// Ensures existing profile semantics, selected graphics, and software trace resolution contracts remain intact.
        /// </summary>
        [Fact]
        public void Existing_profile_semantics_graphics_and_trace_resolutions_remain_stable() {
            using JsonDocument buildConfig = LoadJson(Path.Combine("user_settings", "build_config.json"));
            JsonElement[] platforms = buildConfig.RootElement.GetProperty("platforms").EnumerateArray().ToArray();
            JsonElement ps2 = Assert.Single(platforms, platform => platform.GetProperty("platformId").GetString() == "ps2");
            JsonElement ps2Commands = ps2.GetProperty("editorPrebuildCommandIdsByBuildProfileId");
            Assert.Empty(ps2Commands.GetProperty("colored-cube-grid").EnumerateArray());
            Assert.Equal(RequiredPrebuildCommandIds, ReadCommands(ps2Commands, "debug"));
            Assert.Equal(RequiredPrebuildCommandIds, ReadCommands(ps2Commands, "release"));

            JsonElement psp = Assert.Single(platforms, platform => platform.GetProperty("platformId").GetString() == "psp");
            JsonElement pspCommands = psp.GetProperty("editorPrebuildCommandIdsByBuildProfileId");
            Assert.Equal(RequiredPrebuildCommandIds, ReadCommands(pspCommands, "debug"));
            Assert.Equal(RequiredPrebuildCommandIds, ReadCommands(pspCommands, "release"));

            JsonElement windows = Assert.Single(platforms, platform => platform.GetProperty("platformId").GetString() == "windows");
            Assert.Equal("directx11", windows.GetProperty("selectedGraphicsProfileId").GetString());

            foreach (string platformId in ExpectedPlatformIds) {
                city.rendering.SoftwareTraceResolution resolution = city.rendering.SoftwareTraceResolution.ForPlatform(platformId);
                if (platformId == "ds") {
                    Assert.Equal(256, resolution.Width);
                    Assert.Equal(192, resolution.Height);
                }
                else {
                    Assert.Equal(320, resolution.Width);
                    Assert.Equal(240, resolution.Height);
                }
            }
        }

        static string[] ReadCommands(JsonElement commandMap, string profileId) {
            return commandMap.GetProperty(profileId).EnumerateArray()
                .Select(commandId => commandId.GetString() ?? string.Empty)
                .ToArray();
        }

        static JsonDocument LoadJson(string relativePath) {
            string checkoutRoot = DemoDiscBuildConfigTestPaths.FindCheckoutRoot();
            return JsonDocument.Parse(File.ReadAllText(Path.Combine(checkoutRoot, relativePath)));
        }
    }
}
