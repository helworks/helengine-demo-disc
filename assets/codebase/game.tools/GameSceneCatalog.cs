namespace city.game.tools {
    /// <summary>
    /// Stores the generated authored game-scene ids contributed by the city demo-disc project.
    /// </summary>
    public static class GameSceneCatalog {
        /// <summary>
        /// Stable scene id used by the generated Tilt Trial gameplay scene.
        /// </summary>
        public const string TiltTrialSceneId = "scenes/games/tilt_trial.helen";

        /// <summary>
        /// Stable scene id used by the first generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel01SceneId = "scenes/games/tilt_trial_level_01.helen";

        /// <summary>
        /// Stable scene id used by the second generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel02SceneId = "scenes/games/tilt_trial_level_02.helen";

        /// <summary>
        /// Stable scene id used by the third generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel03SceneId = "scenes/games/tilt_trial_level_03.helen";

        /// <summary>
        /// Stable scene id used by the fourth generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel04SceneId = "scenes/games/tilt_trial_level_04.helen";

        /// <summary>
        /// Stable scene id used by the fifth generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel05SceneId = "scenes/games/tilt_trial_level_05.helen";

        /// <summary>
        /// Returns the complete generated game-scene id set currently emitted by the city project.
        /// </summary>
        /// <returns>Ordered generated game-scene ids.</returns>
        public static IReadOnlyList<string> GetSceneIds() {
            return [
                TiltTrialSceneId,
                TiltTrialLevel01SceneId,
                TiltTrialLevel02SceneId,
                TiltTrialLevel03SceneId,
                TiltTrialLevel04SceneId,
                TiltTrialLevel05SceneId,
            ];
        }
    }
}
