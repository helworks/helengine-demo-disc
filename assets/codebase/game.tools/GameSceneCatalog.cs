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
        /// Returns the complete generated game-scene id set currently emitted by the city project.
        /// </summary>
        /// <returns>Ordered generated game-scene ids.</returns>
        public static IReadOnlyList<string> GetSceneIds() {
            return [
                TiltTrialSceneId,
            ];
        }
    }
}
