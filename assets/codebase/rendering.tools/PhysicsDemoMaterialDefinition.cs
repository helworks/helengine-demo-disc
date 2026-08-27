namespace city.rendering.tools {
    /// <summary>
    /// Describes one shared physics demo material authored through the per-platform material settings flow.
    /// </summary>
    public sealed class PhysicsDemoMaterialDefinition {
        /// <summary>
        /// Initializes one physics demo material definition.
        /// </summary>
        /// <param name="assetName">Stable material asset filename without extension.</param>
        /// <param name="baseColor">Authored base color preserved across platform settings.</param>
        public PhysicsDemoMaterialDefinition(string assetName, string baseColor) {
            if (string.IsNullOrWhiteSpace(assetName)) {
                throw new ArgumentException("Asset name must be provided.", nameof(assetName));
            } else if (string.IsNullOrWhiteSpace(baseColor)) {
                throw new ArgumentException("Base color must be provided.", nameof(baseColor));
            }

            AssetName = assetName;
            BaseColor = baseColor;
        }

        /// <summary>
        /// Gets the stable material asset filename without extension.
        /// </summary>
        public string AssetName { get; }

        /// <summary>
        /// Gets the authored base color preserved across every platform variant.
        /// </summary>
        public string BaseColor { get; }
    }
}
