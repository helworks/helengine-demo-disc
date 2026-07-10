namespace city.game.tools {
    /// <summary>
    /// Bundles the runtime assets required to compose the generated Zombislayer gameplay scene.
    /// </summary>
    public sealed class ZombislayerGenerationAssets {
        /// <summary>
        /// Gets or sets the imported environment runtime model used by the gameplay scene.
        /// </summary>
        public RuntimeModel EnvironmentModel { get; set; }

        /// <summary>
        /// Gets or sets the imported weapon runtime model attached to the first-person viewmodel anchor.
        /// </summary>
        public RuntimeModel WeaponModel { get; set; }
    }
}
