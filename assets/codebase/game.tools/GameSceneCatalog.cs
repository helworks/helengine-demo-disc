namespace city.game.tools {
    /// <summary>
    /// Stores the generated authored game-scene ids contributed by the city demo-disc project.
    /// </summary>
    public static class GameSceneCatalog {
        /// <summary>
        /// Stable scene id used by the generated Tilt Trial gameplay scene.
        /// </summary>
        public const string TiltTrialSceneId = global::city.game.TiltTrialSceneIds.LevelSelectSceneId;

        /// <summary>
        /// Stable scene id used by the generated DS and 3DS level selector.
        /// </summary>
        public const string TiltTrialHandheldLevelSelectSceneId = global::city.game.TiltTrialSceneIds.HandheldLevelSelectSceneId;

        /// <summary>
        /// Stable scene id used by the first generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel01SceneId = global::city.game.TiltTrialSceneIds.Level01SceneId;

        /// <summary>
        /// Stable scene id used by the render-only Level 1 PS2 validation scene.
        /// </summary>
        public const string TiltTrialLevel01RenderTestSceneId = "test_scene_tilt_trial_level_01_render";

        /// <summary>
        /// Stable scene id used by the second generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel02SceneId = global::city.game.TiltTrialSceneIds.Level02SceneId;

        /// <summary>
        /// Stable scene id used by the third generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel03SceneId = global::city.game.TiltTrialSceneIds.Level03SceneId;

        /// <summary>
        /// Stable scene id used by the fourth generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel04SceneId = global::city.game.TiltTrialSceneIds.Level04SceneId;

        /// <summary>
        /// Stable scene id used by the fifth generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel05SceneId = global::city.game.TiltTrialSceneIds.Level05SceneId;

        /// <summary>
        /// Stable scene id used by the generated Zombislayer gameplay scene.
        /// </summary>
        public const string ZombislayerSceneId = global::city.game.ZombislayerSceneIds.GameplaySceneId;

        /// <summary>
        /// Returns the complete generated game-scene id set currently emitted by the city project.
        /// </summary>
        /// <returns>Ordered generated game-scene ids.</returns>
        public static IReadOnlyList<string> GetSceneIds() {
            return [
                TiltTrialSceneId,
                TiltTrialHandheldLevelSelectSceneId,
                TiltTrialLevel01SceneId,
                TiltTrialLevel01RenderTestSceneId,
                TiltTrialLevel02SceneId,
                TiltTrialLevel03SceneId,
                TiltTrialLevel04SceneId,
                TiltTrialLevel05SceneId,
                ZombislayerSceneId,
            ];
        }
    }
}
