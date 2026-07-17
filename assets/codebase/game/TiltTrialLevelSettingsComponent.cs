using helengine;

namespace city.game {
    /// <summary>
    /// Stores authored per-level Tilt Trial metadata used by timer, medals, and next-scene flow.
    /// </summary>
    public sealed class TiltTrialLevelSettingsComponent : Component {
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
        /// Gets or sets the stable logical level id.
        /// </summary>
        public string LevelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the human-readable level name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional project-relative preview texture path.
        /// </summary>
        public string PreviewTexturePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the runtime gameplay scene id for the owning level.
        /// </summary>
        public string SceneId { get; set; } = string.Empty;

        /// <summary>
        /// Validates the current level settings before selector or session code consumes them.
        /// </summary>
        public override void ComponentAdded(Entity entity) {
            ReportValues("TiltTrialLevelSettings:ComponentAdded");
        }

        public override void ComponentInitialized(Entity entity) {
            ReportValues("TiltTrialLevelSettings:ComponentInitialized");
        }

        public void Validate() {
            if (LevelId == null || LevelId.Length == 0) {
                throw new InvalidOperationException(
                    $"Tilt Trial level settings require a level id. lengths: level={GetLengthOrNegative(LevelId)} display={GetLengthOrNegative(DisplayName)} scene={GetLengthOrNegative(SceneId)} preview={GetLengthOrNegative(PreviewTexturePath)}.");
            } else if (DisplayName == null || DisplayName.Length == 0) {
                throw new InvalidOperationException("Tilt Trial level settings require a display name.");
            } else if (SceneId == null || SceneId.Length == 0) {
                throw new InvalidOperationException("Tilt Trial level settings require a scene id.");
            } else if (StartTimeSeconds <= 0f) {
                throw new InvalidOperationException(
                    $"Tilt Trial level settings require a positive start time. values: start={StartTimeSeconds} gold={GoldTimeSeconds} silver={SilverTimeSeconds} bronze={BronzeTimeSeconds}.");
            } else if (GoldTimeSeconds <= 0f || SilverTimeSeconds < GoldTimeSeconds || BronzeTimeSeconds < SilverTimeSeconds) {
                throw new InvalidOperationException(
                    $"Tilt Trial level settings require ascending gold, silver, and bronze medal times. values: start={StartTimeSeconds} gold={GoldTimeSeconds} silver={SilverTimeSeconds} bronze={BronzeTimeSeconds}.");
            }
        }

        static int GetLengthOrNegative(string value) {
            return value == null ? -1 : value.Length;
        }

        void ReportValues(string stage) {
            Core core = Core.Instance;
            if (core != null) {
                core.ReportSceneTransitionStage(
                    $"{stage}:start={StartTimeSeconds}:gold={GoldTimeSeconds}:silver={SilverTimeSeconds}:bronze={BronzeTimeSeconds}");
            }
        }
    }
}
