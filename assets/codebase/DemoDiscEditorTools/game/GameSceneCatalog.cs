
namespace DemoDisc.EditorTools {
    /// <summary>
    /// Stores the generated authored game-scene ids contributed by the city demo-disc project.
    /// </summary>
    public static class GameSceneCatalog {
        /// <summary>
        /// Stable scene id used by the generated Tilt Trial gameplay scene.
        /// </summary>
        public const string TiltTrialSceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId;

        /// <summary>
        /// Stable scene id used by the generated DS and 3DS level selector.
        /// </summary>
        public const string TiltTrialHandheldLevelSelectSceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.HandheldLevelSelectSceneId;

        /// <summary>
        /// Stable scene id used by the first generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel01SceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId;

        /// <summary>
        /// Stable scene id used by the second generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel02SceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level02SceneId;

        /// <summary>
        /// Stable scene id used by the third generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel03SceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level03SceneId;

        /// <summary>
        /// Stable scene id used by the fourth generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel04SceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level04SceneId;

        /// <summary>
        /// Stable scene id used by the fifth generated Tilt Trial gameplay level.
        /// </summary>
        public const string TiltTrialLevel05SceneId = global::DemoDisc.TiltPlay.TiltTrialSceneIds.Level05SceneId;

        /// <summary>
        /// Returns the complete generated game-scene id set currently emitted by the city project.
        /// </summary>
        /// <returns>Ordered generated game-scene ids.</returns>
        public static IReadOnlyList<string> GetSceneIds() {
            return [
                TiltTrialSceneId,
                TiltTrialHandheldLevelSelectSceneId,
                TiltTrialLevel01SceneId,
                TiltTrialLevel02SceneId,
                TiltTrialLevel03SceneId,
                TiltTrialLevel04SceneId,
                TiltTrialLevel05SceneId,
            ];
        }
    }
}
