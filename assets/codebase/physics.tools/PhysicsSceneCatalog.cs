namespace city.physics.tools {
    /// <summary>
    /// Enumerates the exportable physics validation scenes authored for end-to-end runtime testing.
    /// </summary>
    public static class PhysicsSceneCatalog {
        /// <summary>
        /// Relative scene id for the character slope validation scene.
        /// </summary>
        public const string CharacterSlopeSceneId = "scenes/physics/test_scene_character_slope.helen";

        /// <summary>
        /// Relative scene id for the character steps validation scene.
        /// </summary>
        public const string CharacterStepsSceneId = "scenes/physics/test_scene_character_steps.helen";

        /// <summary>
        /// Relative scene id for the character moving-platform validation scene.
        /// </summary>
        public const string CharacterMovingPlatformSceneId = "scenes/physics/test_scene_character_moving_platform.helen";

        /// <summary>
        /// Relative scene id for the stacked dynamic-body validation scene.
        /// </summary>
        public const string DynamicStackBoxesSceneId = "scenes/physics/test_scene_dynamic_stack_boxes.helen";

        /// <summary>
        /// Relative scene id for the minimal falling-cube validation scene.
        /// </summary>
        public const string SingleFallingCubeSceneId = "scenes/physics/test_scene_single_falling_cube.helen";

        /// <summary>
        /// Relative scene id for the dynamic sphere-stack validation scene.
        /// </summary>
        public const string DynamicSphereStackSceneId = "scenes/physics/test_scene_dynamic_sphere_stack.helen";

        /// <summary>
        /// Relative scene id for the strict rotated-box parity validation scene.
        /// </summary>
        public const string StrictRotatedBoxCompareSceneId = "scenes/physics/test_scene_strict_rotated_box_compare.helen";

        /// <summary>
        /// Relative scene id for the render-only slope validation scene.
        /// </summary>
        public const string RenderOnlySlopeSceneId = "scenes/physics/test_scene_render_only_slope.helen";

        /// <summary>
        /// Relative scene id for the render-only matrix probe validation scene.
        /// </summary>
        public const string RenderMatrixProbeSceneId = "scenes/physics/test_scene_render_matrix_probe.helen";

        /// <summary>
        /// Relative scene id for the render-only motion probe validation scene.
        /// </summary>
        public const string RenderMotionProbeSceneId = "scenes/physics/test_scene_render_motion_probe.helen";

        /// <summary>
        /// Relative scene id for the mixed dynamic box and sphere stack validation scene.
        /// </summary>
        public const string DynamicMixedStackSceneId = "scenes/physics/test_scene_dynamic_mixed_stack.helen";

        /// <summary>
        /// Relative scene id for the kinematic push validation scene.
        /// </summary>
        public const string KinematicPushSceneId = "scenes/physics/test_scene_kinematic_push.helen";

        /// <summary>
        /// Relative scene id for the static-mesh ground stability validation scene.
        /// </summary>
        public const string MeshGroundStabilitySceneId = "scenes/physics/test_scene_mesh_ground_stability.helen";

        /// <summary>
        /// Relative scene id for the playable static-mesh showcase validation scene.
        /// </summary>
        public const string StaticMeshShowcaseSceneId = "scenes/physics/test_scene_static_mesh_showcase.helen";

        /// <summary>
        /// Relative scene id for the minimal playable static-mesh validation scene.
        /// </summary>
        public const string StaticMeshMinimalSceneId = "scenes/physics/test_scene_static_mesh_minimal.helen";

        /// <summary>
        /// Relative scene id for the trigger-volume validation scene.
        /// </summary>
        public const string TriggerVolumeSceneId = "scenes/physics/test_scene_trigger_volume.helen";

        /// <summary>
        /// Stable ordered list of authored physics validation scene ids.
        /// </summary>
        static readonly string[] SceneIds = new[] {
            CharacterSlopeSceneId,
            CharacterStepsSceneId,
            CharacterMovingPlatformSceneId,
            DynamicStackBoxesSceneId,
            SingleFallingCubeSceneId,
            DynamicSphereStackSceneId,
            StrictRotatedBoxCompareSceneId,
            RenderOnlySlopeSceneId,
            RenderMatrixProbeSceneId,
            RenderMotionProbeSceneId,
            DynamicMixedStackSceneId,
            KinematicPushSceneId,
            MeshGroundStabilitySceneId,
            StaticMeshShowcaseSceneId,
            StaticMeshMinimalSceneId,
            TriggerVolumeSceneId
        };

        /// <summary>
        /// Gets the stable ordered list of exportable physics validation scene ids.
        /// </summary>
        /// <returns>Ordered scene ids used by validation tooling and generated demo content.</returns>
        public static string[] GetSceneIds() {
            string[] copy = new string[SceneIds.Length];
            Array.Copy(SceneIds, copy, SceneIds.Length);
            return copy;
        }
    }
}
