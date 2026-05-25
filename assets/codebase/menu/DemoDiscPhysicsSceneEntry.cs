namespace city.menu {
    /// <summary>
    /// Describes one curated physics showcase scene and its Nintendo DS companion scene id.
    /// </summary>
    public sealed class DemoDiscPhysicsSceneEntry {
        /// <summary>
        /// Initializes one curated physics scene entry used by the demo-disc menu and DS scene generators.
        /// </summary>
        /// <param name="menuItemId">Stable menu item id shown in the physics scene list.</param>
        /// <param name="displayName">Human-readable scene label shown to players.</param>
        /// <param name="sceneId">Stable logical scene id used by the desktop and generic runtime flow.</param>
        /// <param name="nintendoDsSceneId">Stable logical scene id used by the Nintendo DS companion scene.</param>
        public DemoDiscPhysicsSceneEntry(string menuItemId, string displayName, string sceneId, string nintendoDsSceneId) {
            if (string.IsNullOrWhiteSpace(menuItemId)) {
                throw new ArgumentException("Menu item id must be provided.", nameof(menuItemId));
            } else if (string.IsNullOrWhiteSpace(displayName)) {
                throw new ArgumentException("Display name must be provided.", nameof(displayName));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (string.IsNullOrWhiteSpace(nintendoDsSceneId)) {
                throw new ArgumentException("Nintendo DS scene id must be provided.", nameof(nintendoDsSceneId));
            }

            MenuItemId = menuItemId;
            DisplayName = displayName;
            SceneId = sceneId;
            NintendoDsSceneId = nintendoDsSceneId;
        }

        /// <summary>
        /// Gets the stable menu item id used by the generated demo-disc menu.
        /// </summary>
        public string MenuItemId { get; }

        /// <summary>
        /// Gets the human-readable scene label shown by the menu.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the logical runtime scene id used by the non-DS flow.
        /// </summary>
        public string SceneId { get; }

        /// <summary>
        /// Gets the logical runtime scene id used by the Nintendo DS companion scene.
        /// </summary>
        public string NintendoDsSceneId { get; }
    }
}
