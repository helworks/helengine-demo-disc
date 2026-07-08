namespace city.game {
    /// <summary>
    /// Stores one ordered Tilt Trial level entry shared by selector and gameplay progression.
    /// </summary>
    public sealed class TiltTrialLevelCatalogEntry {
        /// <summary>
        /// Initializes one ordered Tilt Trial level entry.
        /// </summary>
        public TiltTrialLevelCatalogEntry(
            string levelId,
            string displayName,
            string sceneId,
            float startTimeSeconds,
            float goldTimeSeconds,
            float silverTimeSeconds,
            float bronzeTimeSeconds,
            string previewTexturePath) {
            if (string.IsNullOrWhiteSpace(levelId)) {
                throw new ArgumentException("Level id must be provided.", nameof(levelId));
            } else if (string.IsNullOrWhiteSpace(displayName)) {
                throw new ArgumentException("Display name must be provided.", nameof(displayName));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (startTimeSeconds <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(startTimeSeconds), "Start time must be positive.");
            } else if (goldTimeSeconds <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(goldTimeSeconds), "Gold time must be positive.");
            } else if (silverTimeSeconds < goldTimeSeconds) {
                throw new ArgumentOutOfRangeException(nameof(silverTimeSeconds), "Silver time must be greater than or equal to the gold time.");
            } else if (bronzeTimeSeconds < silverTimeSeconds) {
                throw new ArgumentOutOfRangeException(nameof(bronzeTimeSeconds), "Bronze time must be greater than or equal to the silver time.");
            }

            LevelId = levelId;
            DisplayName = displayName;
            SceneId = sceneId;
            StartTimeSeconds = startTimeSeconds;
            GoldTimeSeconds = goldTimeSeconds;
            SilverTimeSeconds = silverTimeSeconds;
            BronzeTimeSeconds = bronzeTimeSeconds;
            PreviewTexturePath = previewTexturePath ?? string.Empty;
        }

        /// <summary>
        /// Gets the stable logical level id.
        /// </summary>
        public string LevelId { get; }

        /// <summary>
        /// Gets the human-readable level name shown by the selector.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the runtime gameplay scene id loaded for this level.
        /// </summary>
        public string SceneId { get; }

        /// <summary>
        /// Gets the starting countdown value in seconds.
        /// </summary>
        public float StartTimeSeconds { get; }

        /// <summary>
        /// Gets the clear time threshold for a gold result.
        /// </summary>
        public float GoldTimeSeconds { get; }

        /// <summary>
        /// Gets the clear time threshold for a silver result.
        /// </summary>
        public float SilverTimeSeconds { get; }

        /// <summary>
        /// Gets the clear time threshold for a bronze result.
        /// </summary>
        public float BronzeTimeSeconds { get; }

        /// <summary>
        /// Gets the optional project-relative preview texture path.
        /// </summary>
        public string PreviewTexturePath { get; }
    }
}
