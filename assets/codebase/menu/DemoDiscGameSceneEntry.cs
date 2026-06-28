namespace city.menu {
    /// <summary>
    /// Describes one curated game showcase scene surfaced by the demo-disc menu.
    /// </summary>
    public sealed class DemoDiscGameSceneEntry {
        /// <summary>
        /// Initializes one curated game-scene entry used by the demo-disc menu.
        /// </summary>
        /// <param name="menuItemId">Stable menu item id shown in the games scene list.</param>
        /// <param name="displayName">Human-readable scene label shown to players.</param>
        /// <param name="sceneId">Stable logical scene id used by the runtime load flow.</param>
        public DemoDiscGameSceneEntry(string menuItemId, string displayName, string sceneId) {
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
        /// Gets the logical runtime scene id loaded when the menu item is activated.
        /// </summary>
        public string SceneId { get; }
    }
}
