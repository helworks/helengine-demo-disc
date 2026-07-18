namespace city.rendering.tools {
    /// <summary>
    /// Stores one generated Nintendo handheld scene augmentation merged into the canonical authored scene with per-platform entity existence rules.
    /// </summary>
    public sealed class GeneratedDsSceneDefinition {
        /// <summary>
        /// Gets or sets optional live root entities that should be merged directly into the canonical scene when the handheld build needs a custom authored layout.
        /// </summary>
        public Entity[] RootEntities { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the standard DS bottom overlay should be emitted automatically.
        /// </summary>
        public bool UseDefaultBottomOverlay { get; set; }

        /// <summary>
        /// Gets or sets optional custom bottom-screen root entities supplied by a generator when it opts out of the default overlay.
        /// </summary>
        public Entity[] BottomScreenRootEntities { get; set; }

        /// <summary>
        /// Gets or sets whether authored 2D roots should be relocated beneath the shared bottom-screen viewport.
        /// </summary>
        public bool MoveTopScreen2DRootsToBottomScreen { get; set; } = true;
    }
}
