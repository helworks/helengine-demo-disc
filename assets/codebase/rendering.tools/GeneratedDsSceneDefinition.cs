namespace city.rendering.tools {
    /// <summary>
    /// Stores one generated Nintendo DS companion-scene definition emitted alongside a default generated rendering scene.
    /// </summary>
    public sealed class GeneratedDsSceneDefinition {
        /// <summary>
        /// Gets or sets the stable DS companion scene id written to disk.
        /// </summary>
        public string SceneId { get; set; }

        /// <summary>
        /// Gets or sets optional live root entities that should be written directly when the DS companion scene needs a custom authored layout.
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
    }
}
