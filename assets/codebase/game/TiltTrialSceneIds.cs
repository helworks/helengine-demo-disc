namespace city.game {
    /// <summary>
    /// Stores the shared runtime scene ids used by the Tilt Trial selector and gameplay flow.
    /// </summary>
    public static class TiltTrialSceneIds {
        /// <summary>
        /// Stable scene id used by the Tilt Trial level-select front door.
        /// </summary>
        public const string LevelSelectSceneId = "tilt_trial";

        /// <summary>
        /// Stable scene id used by the DS and 3DS-specific Tilt Trial level selector.
        /// </summary>
        public const string HandheldLevelSelectSceneId = "tilt_trial_ds";

        /// <summary>
        /// Resolves the selector scene id appropriate for the active runtime platform.
        /// </summary>
        /// <returns>Handheld selector id on DS and 3DS, otherwise the console selector id.</returns>
        public static string ResolveLevelSelectSceneId() {
            PlatformInfo platformInfo = Core.Instance?.PlatformInfo;
            if (platformInfo != null && (string.Equals(platformInfo.Name, "ds", StringComparison.OrdinalIgnoreCase)
                || string.Equals(platformInfo.Name, "3ds", StringComparison.OrdinalIgnoreCase))) {
                return HandheldLevelSelectSceneId;
            }

            return LevelSelectSceneId;
        }

        /// <summary>
        /// Stable scene id used by the first Tilt Trial gameplay level.
        /// </summary>
        public const string Level01SceneId = "tilt_trial_level_01";

        /// <summary>
        /// Stable scene id used by the second Tilt Trial gameplay level.
        /// </summary>
        public const string Level02SceneId = "tilt_trial_level_02";

        /// <summary>
        /// Stable scene id used by the third Tilt Trial gameplay level.
        /// </summary>
        public const string Level03SceneId = "tilt_trial_level_03";

        /// <summary>
        /// Stable scene id used by the fourth Tilt Trial gameplay level.
        /// </summary>
        public const string Level04SceneId = "tilt_trial_level_04";

        /// <summary>
        /// Stable scene id used by the fifth Tilt Trial gameplay level.
        /// </summary>
        public const string Level05SceneId = "tilt_trial_level_05";
    }
}
