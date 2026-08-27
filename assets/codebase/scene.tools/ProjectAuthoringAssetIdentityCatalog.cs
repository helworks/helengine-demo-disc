namespace city.scene.tools {
    /// <summary>
    /// Explicit identities assigned to project-authored native outputs.
    /// </summary>
    public static class ProjectAuthoringAssetIdentityCatalog {
        static readonly IReadOnlyDictionary<string, string> SceneIdentities =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                ["Scenes/DemoDiscMainMenu.helen"] = "10000000000000000000000000000001",
                ["Scenes/DemoDiscMainMenuHandheld.helen"] = "10000000000000000000000000000002",
                ["Scenes/HelenOfCodeSplash.helen"] = "10000000000000000000000000000003",
                ["Scenes/SceneLoadingScreen.helen"] = "10000000000000000000000000000004",
                ["scenes/rendering/cube_test.helen"] = "10000000000000000000000000000010",
                ["scenes/rendering/colored_cube_grid.helen"] = "10000000000000000000000000000011",
                ["scenes/rendering/scaled_cube.helen"] = "10000000000000000000000000000012",
                ["scenes/rendering/directional_shadow_plaza.helen"] = "10000000000000000000000000000013",
                ["scenes/rendering/ground_cube_probe.helen"] = "10000000000000000000000000000014",
                ["scenes/rendering/textured_cube_grid.helen"] = "10000000000000000000000000000015",
                ["scenes/rendering/spotlight_street_slice.helen"] = "10000000000000000000000000000016",
                ["scenes/rendering/axis_test.helen"] = "10000000000000000000000000000017",
                ["scenes/rendering/axis_test2.helen"] = "10000000000000000000000000000018",
                ["scenes/rendering/depth_clip_probe.helen"] = "10000000000000000000000000000019",
                ["scenes/rendering/scene_memory_probe.helen"] = "1000000000000000000000000000001a",
                ["scenes/rendering/pbr_material_gallery.helen"] = "1000000000000000000000000000001b",
                ["scenes/rendering/pbr_textured_showcase.helen"] = "1000000000000000000000000000001c",
                ["scenes/rendering/pbr_shadow_theater.helen"] = "1000000000000000000000000000001d",
                ["scenes/rendering/test_scene_matrix_render.helen"] = "1000000000000000000000000000001e",
                ["scenes/physics/test_scene_character_slope.helen"] = "10000000000000000000000000000020",
                ["scenes/physics/test_scene_character_steps.helen"] = "10000000000000000000000000000021",
                ["scenes/physics/test_scene_character_moving_platform.helen"] = "10000000000000000000000000000022",
                ["scenes/physics/test_scene_dynamic_stack_boxes.helen"] = "10000000000000000000000000000023",
                ["scenes/physics/test_scene_single_falling_cube.helen"] = "10000000000000000000000000000024",
                ["scenes/physics/test_scene_dynamic_sphere_stack.helen"] = "10000000000000000000000000000025",
                ["scenes/physics/test_scene_strict_rotated_box_compare.helen"] = "10000000000000000000000000000026",
                ["scenes/physics/test_scene_render_only_slope.helen"] = "10000000000000000000000000000027",
                ["scenes/physics/test_scene_dynamic_mixed_stack.helen"] = "10000000000000000000000000000028",
                ["scenes/physics/test_scene_kinematic_push.helen"] = "10000000000000000000000000000029",
                ["scenes/physics/test_scene_mesh_ground_stability.helen"] = "1000000000000000000000000000002a",
                ["scenes/physics/test_scene_trigger_volume.helen"] = "1000000000000000000000000000002b",
                ["scenes/games/tilt/tilt_trial.helen"] = "10000000000000000000000000000030",
                ["scenes/games/tilt/tilt_trial_ds.helen"] = "10000000000000000000000000000031",
                ["scenes/physics/test_scene_tilt_trial_level_01_render.helen"] = "10000000000000000000000000000032",
                ["scenes/games/tilt/tilt_trial_level_01.helen"] = "10000000000000000000000000000033",
                ["scenes/games/tilt/tilt_trial_level_02.helen"] = "10000000000000000000000000000034",
                ["scenes/games/tilt/tilt_trial_level_03.helen"] = "10000000000000000000000000000035",
                ["scenes/games/tilt/tilt_trial_level_04.helen"] = "10000000000000000000000000000036",
                ["scenes/games/tilt/tilt_trial_level_05.helen"] = "10000000000000000000000000000037",
                ["scenes/games/zombislayer.helen"] = "10000000000000000000000000000038",
                ["zombislayer.helen"] = "10000000000000000000000000000038",
                ["zombislayer"] = "10000000000000000000000000000038"
            };

        static readonly IReadOnlyDictionary<string, string> NativeAssetIdentities =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                ["animations/DemoDiscLogoIdle.hanim"] = "40000000000000000000000000000001",
                ["models/games/tilt/golden_coin.hasset"] = "40000000000000000000000000000002",
                ["models/games/tilt/golden_coin_ds.hasset"] = "40000000000000000000000000000003",
                ["models/games/tilt/goal_flag.hasset"] = "40000000000000000000000000000004",
                ["models/games/tilt/goal_flag_ds.hasset"] = "40000000000000000000000000000005",
                ["models/games/tilt/rotating_platform.hasset"] = "4000000000000000000000000000000c",
                ["models/games/tilt/pendulum_hammer.hasset"] = "4000000000000000000000000000000d",
                ["models/games/tilt/pendulum_hammer_ds.hasset"] = "4000000000000000000000000000000e",
                ["blueprints/games/tilt/GoldenCoin.hblueprint"] = "40000000000000000000000000000006",
                ["blueprints/games/tilt/GoalFlag.hblueprint"] = "40000000000000000000000000000007",
                ["blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint"] = "40000000000000000000000000000008",
                ["blueprints/games/tilt/TiltTrialHandheldPresentation.hblueprint"] = "40000000000000000000000000000009",
                ["blueprints/ui/ConsoleCameraLightInstructions.hblueprint"] = "4000000000000000000000000000000a",
                ["blueprints/games/tilt/RotatingPlatform.hblueprint"] = "4000000000000000000000000000000f",
                ["blueprints/games/tilt/PendulumHammer.hblueprint"] = "40000000000000000000000000000010",
                ["models/rendering/tilt_trial/clipping_probe_face_colors.hasset"] = "4000000000000000000000000000000b"
            };

        /// <summary>
        /// Resolves the explicit identity for one generated native scene path.
        /// </summary>
        public static string GetSceneIdentity(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Generated scene path must be provided.", nameof(relativePath));
            }

            string normalizedPath = relativePath.Replace('\\', '/');
            if (!SceneIdentities.TryGetValue(normalizedPath, out string identity)) {
                throw new InvalidOperationException($"No explicit project authoring identity is registered for generated scene '{normalizedPath}'.");
            }

            return identity;
        }

        /// <summary>
        /// Resolves the explicit identity for one generated native asset path.
        /// </summary>
        public static string GetNativeAssetIdentity(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Generated asset path must be provided.", nameof(relativePath));
            }

            string normalizedPath = relativePath.Replace('\\', '/');
            if (!NativeAssetIdentities.TryGetValue(normalizedPath, out string identity)) {
                throw new InvalidOperationException($"No explicit project authoring identity is registered for generated asset '{normalizedPath}'.");
            }

            return identity;
        }

        /// <summary>
        /// Resolves the explicit identity for one generated material path.
        /// </summary>
        public static string GetMaterialIdentity(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Generated material path must be provided.", nameof(relativePath));
            }

            string normalizedPath = relativePath.Replace('\\', '/');
            if (normalizedPath.StartsWith("materials/rendering/colored_cube_grid/Cube", StringComparison.OrdinalIgnoreCase)
                && TryResolveIndexedIdentity(normalizedPath, "materials/rendering/colored_cube_grid/Cube", 16, "200000000000000000000000000000" , out string coloredIdentity)) {
                return coloredIdentity;
            }
            if (normalizedPath.StartsWith("materials/rendering/textured_cube_grid/Cube", StringComparison.OrdinalIgnoreCase)
                && TryResolveIndexedIdentity(normalizedPath, "materials/rendering/textured_cube_grid/Cube", 16, "210000000000000000000000000000", out string texturedIdentity)) {
                return texturedIdentity;
            }
            if (normalizedPath.StartsWith("materials/rendering/pbr_gallery/M", StringComparison.OrdinalIgnoreCase)) {
                int separator = normalizedPath.IndexOf("R", StringComparison.OrdinalIgnoreCase);
                if (separator > 0 && int.TryParse(normalizedPath.Substring("materials/rendering/pbr_gallery/M".Length, separator - "materials/rendering/pbr_gallery/M".Length), out int metallic)
                    && int.TryParse(normalizedPath.Substring(separator + 1).Replace(".hasset", string.Empty, StringComparison.OrdinalIgnoreCase), out int roughness)
                    && metallic >= 0 && metallic < 5 && roughness >= 0 && roughness < 5) {
                    return "220000000000000000000000000000" + metallic.ToString("X1") + roughness.ToString("X1");
                }
            }

            IReadOnlyDictionary<string, string> fixedIdentities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                ["materials/rendering/cube_test/CubeTestSolid.hasset"] = "23000000000000000000000000000001",
                ["materials/rendering/depth_clip_probe/DepthClipProbeSolid.hasset"] = "23000000000000000000000000000002",
                ["materials/rendering/depth_clip_probe/DepthClipProbeCenterSolid.hasset"] = "23000000000000000000000000000003",
                ["materials/rendering/axis_test/X.hasset"] = "23000000000000000000000000000004",
                ["materials/rendering/axis_test/Y.hasset"] = "23000000000000000000000000000005",
                ["materials/rendering/axis_test/Z.hasset"] = "23000000000000000000000000000006",
                ["materials/rendering/axis_test/Ground.hasset"] = "23000000000000000000000000000007",
                ["materials/rendering/axis_test/Marker.hasset"] = "23000000000000000000000000000008",
                ["materials/rendering/matrix_render/Hero.hasset"] = "23000000000000000000000000000009",
                ["materials/rendering/pbr_textured_showcase/ScuffedMetal.hasset"] = "2300000000000000000000000000000a",
                ["materials/rendering/pbr_textured_showcase/WoodPlanks.hasset"] = "2300000000000000000000000000000b",
                ["materials/rendering/tilt_trial/Course.hasset"] = "2300000000000000000000000000000c",
                ["materials/rendering/tilt_trial/PlayerSphereWalnut.hasset"] = "2300000000000000000000000000000d",
                ["materials/rendering/tilt_trial/PlayerSphereMarble.hasset"] = "2300000000000000000000000000000e",
                ["materials/rendering/tilt_trial/ClippingProbeFaceColors.hasset"] = "2300000000000000000000000000000f",
                ["materials/games/tilt/GoldenCoin.hasset"] = "23000000000000000000000000000010",
                ["materials/games/tilt/GoalFlagPole.hasset"] = "23000000000000000000000000000011",
                ["materials/games/tilt/GoalFlagBanner.hasset"] = "23000000000000000000000000000012",
                ["materials/games/tilt/RotatingPlatform.hasset"] = "23000000000000000000000000000013",
                ["materials/games/tilt/PendulumHammerHandle.hasset"] = "23000000000000000000000000000014",
                ["materials/games/tilt/PendulumHammerHead.hasset"] = "23000000000000000000000000000015",
                ["materials/physics/PhysicsDemoGround.hasset"] = "23000000000000000000000000000020",
                ["materials/physics/PhysicsDemoNeutral.hasset"] = "23000000000000000000000000000021",
                ["materials/physics/PhysicsDemoBlue.hasset"] = "23000000000000000000000000000022",
                ["materials/physics/PhysicsDemoGreen.hasset"] = "23000000000000000000000000000023",
                ["materials/physics/PhysicsDemoMagenta.hasset"] = "23000000000000000000000000000024",
                ["materials/physics/PhysicsDemoYellow.hasset"] = "23000000000000000000000000000025",
                ["materials/physics/PhysicsDemoCyan.hasset"] = "23000000000000000000000000000026",
                ["materials/physics/PhysicsDemoRed.hasset"] = "23000000000000000000000000000027",
                ["materials/physics/PhysicsDemoOrange.hasset"] = "23000000000000000000000000000028",
                ["materials/physics/PhysicsDemoPurple.hasset"] = "23000000000000000000000000000029",
                ["materials/physics/PhysicsDemoSphereStackBlue.hasset"] = "2300000000000000000000000000002a",
                ["materials/physics/PhysicsDemoSphereStackGreen.hasset"] = "2300000000000000000000000000002b",
                ["materials/physics/PhysicsDemoSphereStackMagenta.hasset"] = "2300000000000000000000000000002c",
                ["materials/physics/PhysicsDemoSphereStackYellow.hasset"] = "2300000000000000000000000000002d",
                ["materials/physics/PhysicsDemoSphereStackCyan.hasset"] = "2300000000000000000000000000002e",
                ["materials/physics/PhysicsDemoSphereStackRed.hasset"] = "2300000000000000000000000000002f",
                ["materials/physics/PhysicsDemoSphereStackOrange.hasset"] = "23000000000000000000000000000030",
                ["materials/physics/PhysicsDemoSphereStackPurple.hasset"] = "23000000000000000000000000000031"
            };
            if (fixedIdentities.TryGetValue(normalizedPath, out string fixedIdentity)) {
                return fixedIdentity;
            }

            throw new InvalidOperationException($"No explicit project authoring identity is registered for generated material '{normalizedPath}'.");
        }

        static bool TryResolveIndexedIdentity(string path, string prefix, int count, string identityPrefix, out string identity) {
            identity = string.Empty;
            string suffix = path.Substring(prefix.Length).Replace(".hasset", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (!int.TryParse(suffix, out int index) || index < 0 || index >= count) {
                return false;
            }

            identity = identityPrefix + index.ToString("X2");
            return true;
        }
    }
}
