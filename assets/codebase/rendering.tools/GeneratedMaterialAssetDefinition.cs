using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Describes one authored material asset plus its per-platform material settings.
    /// </summary>
    public sealed class GeneratedMaterialAssetDefinition {
        /// <summary>
        /// Backing store for per-platform material settings.
        /// </summary>
        readonly Dictionary<string, GeneratedMaterialPlatformDefinition> PlatformsById;

        /// <summary>
        /// Initializes one generated material definition with an empty platform map.
        /// </summary>
        public GeneratedMaterialAssetDefinition() {
            PlatformsById = new Dictionary<string, GeneratedMaterialPlatformDefinition>(StringComparer.OrdinalIgnoreCase);
            MaterialAsset = new ShaderMaterialAsset();
        }

        /// <summary>
        /// Gets or sets the serialized top-level material asset written to disk.
        /// </summary>
        public ShaderMaterialAsset MaterialAsset { get; set; }

        /// <summary>
        /// Gets the authored platform settings keyed by platform id.
        /// </summary>
        public IReadOnlyDictionary<string, GeneratedMaterialPlatformDefinition> Platforms => PlatformsById;

        /// <summary>
        /// Returns the existing platform settings for one platform id or creates an empty record when none exists yet.
        /// </summary>
        /// <param name="platformId">Stable platform identifier.</param>
        /// <returns>Mutable platform settings record.</returns>
        public GeneratedMaterialPlatformDefinition GetOrCreatePlatform(string platformId) {
            if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            if (!PlatformsById.TryGetValue(platformId, out GeneratedMaterialPlatformDefinition platformDefinition) || platformDefinition == null) {
                platformDefinition = new GeneratedMaterialPlatformDefinition();
                PlatformsById[platformId] = platformDefinition;
            }

            return platformDefinition;
        }
    }
}
