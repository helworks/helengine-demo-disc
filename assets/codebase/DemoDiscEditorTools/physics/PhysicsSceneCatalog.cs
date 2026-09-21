namespace DemoDisc.EditorTools {
    /// <summary>
    /// Enumerates the exportable physics validation scenes authored for end-to-end runtime testing.
    /// </summary>
    public static class PhysicsSceneCatalog {
        /// <summary>
        /// Relative scene id for the stacked dynamic-body validation scene.
        /// </summary>
        public const string DynamicStackBoxesSceneId = "scenes/physics/test_scene_dynamic_stack_boxes.helen";

        /// <summary>
        /// Relative scene id for the dynamic sphere-stack validation scene.
        /// </summary>
        public const string DynamicSphereStackSceneId = "scenes/physics/test_scene_dynamic_sphere_stack.helen";

        /// <summary>
        /// Relative scene id for the mixed dynamic box and sphere stack validation scene.
        /// </summary>
        public const string DynamicMixedStackSceneId = "scenes/physics/test_scene_dynamic_mixed_stack.helen";

        /// <summary>
        /// Stable ordered list of authored physics validation scene ids.
        /// </summary>
        static readonly string[] SceneIds = new[] {
            DynamicStackBoxesSceneId,
            DynamicSphereStackSceneId,
            DynamicMixedStackSceneId
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
