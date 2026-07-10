using helengine;

namespace city.game {
    /// <summary>
    /// Stores authored per-level Tilt Trial metadata used by timer, medals, and next-scene flow.
    /// </summary>
    public sealed class TiltTrialLevelSettingsComponent : Component {
        /// <summary>
        /// Gets or sets the stable logical level id.
        /// </summary>
        public string LevelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable level name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the runtime gameplay scene id for the owning level.
        /// </summary>
        public string SceneId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the starting countdown value in seconds.
        /// </summary>
        public float StartTimeSeconds { get; set; } = 99f;

        /// <summary>
        /// Gets or sets the clear time threshold for a gold result.
        /// </summary>
        public float GoldTimeSeconds { get; set; } = 20f;

        /// <summary>
        /// Gets or sets the clear time threshold for a silver result.
        /// </summary>
        public float SilverTimeSeconds { get; set; } = 35f;

        /// <summary>
        /// Gets or sets the clear time threshold for a bronze result.
        /// </summary>
        public float BronzeTimeSeconds { get; set; } = 50f;

        /// <summary>
        /// Gets or sets the optional project-relative preview texture path.
        /// </summary>
        public string PreviewTexturePath { get; set; } = string.Empty;

        /// <summary>
        /// Validates the current level settings before selector or session code consumes them.
        /// </summary>
        public void Validate() {
            if (IsMissingRequiredText(LevelId)) {
                throw new InvalidOperationException("Tilt Trial level settings require a level id.");
            } else if (IsMissingRequiredText(DisplayName)) {
                throw new InvalidOperationException("Tilt Trial level settings require a display name.");
            } else if (IsMissingRequiredText(SceneId)) {
                throw new InvalidOperationException("Tilt Trial level settings require a scene id.");
            } else if (StartTimeSeconds <= 0f) {
                throw new InvalidOperationException("Tilt Trial level settings require a positive start time.");
            } else if (GoldTimeSeconds <= 0f || SilverTimeSeconds < GoldTimeSeconds || BronzeTimeSeconds < SilverTimeSeconds) {
                throw new InvalidOperationException("Tilt Trial level settings require ascending gold, silver, and bronze medal times.");
            }
        }

        static bool IsMissingRequiredText(string value) {
            return value == null || value.Length == 0;
        }
    }
}
