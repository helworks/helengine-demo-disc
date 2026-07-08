namespace city.menu {
    /// <summary>
    /// Describes one curated physics showcase scene exposed by the demo-disc menu and handheld scene augmentation pipeline.
    /// </summary>
    public sealed class DemoDiscPhysicsSceneEntry {
        /// <summary>
        /// Initializes one curated physics scene entry used by the demo-disc menu and handheld scene generators.
        /// </summary>
        /// <param name="menuItemId">Stable menu item id shown in the physics scene list.</param>
        /// <param name="displayName">Human-readable scene label shown to players.</param>
        /// <param name="sceneId">Stable logical scene id used by the desktop and generic runtime flow.</param>
        public DemoDiscPhysicsSceneEntry(string menuItemId, string displayName, string sceneId) {
            if (string.IsNullOrWhiteSpace(menuItemId)) {
                throw new ArgumentException("Menu item id must be provided.", nameof(menuItemId));
            } else if (string.IsNullOrWhiteSpace(displayName)) {
                throw new ArgumentException("Display name must be provided.", nameof(displayName));
            } else if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            MenuItemId = menuItemId;
            DisplayName = displayName;
            SceneId = sceneId;
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
        /// Gets the logical runtime scene id shared by every platform-specific authored variation of this curated scene.
        /// </summary>
        public string SceneId { get; }
    }
}
