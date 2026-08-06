namespace city.menu {
    /// <summary>
    /// Describes one logical city demo-disc menu screen hosted inside the shared menu scene.
    /// </summary>
    public sealed class MenuPanelDefinition {
        /// <summary>
        /// Initializes one city menu panel definition.
        /// </summary>
        /// <param name="panelId">Stable panel identifier used for navigation.</param>
        /// <param name="heading">Primary heading rendered when the panel is active.</param>
        /// <param name="visibleItemCount">Maximum number of visible rows before the panel scrolls.</param>
        /// <param name="items">Selectable items rendered by the panel.</param>
        public MenuPanelDefinition(string panelId, string heading, int visibleItemCount, [NativeTakesOwnership] MenuItemDefinition[] items) {
            if (string.IsNullOrWhiteSpace(panelId)) {
                throw new ArgumentException("Panel id must be provided.", nameof(panelId));
            }
            if (string.IsNullOrWhiteSpace(heading)) {
                throw new ArgumentException("Panel heading must be provided.", nameof(heading));
            }
            if (visibleItemCount < 1) {
                throw new ArgumentOutOfRangeException(nameof(visibleItemCount), "Visible item count must be at least one.");
            }
            if (items == null) {
                throw new ArgumentNullException(nameof(items));
            }
            if (items.Length == 0) {
                throw new InvalidOperationException("Menu panels must contain at least one item.");
            }

            PanelId = panelId;
            Heading = heading;
            VisibleItemCount = visibleItemCount;
            Items = items;
        }

        /// <summary>
        /// Gets the stable panel identifier.
        /// </summary>
        public string PanelId { get; }

        /// <summary>
        /// Gets the heading rendered for the panel.
        /// </summary>
        public string Heading { get; }

        /// <summary>
        /// Gets the maximum number of visible rows before the panel scrolls.
        /// </summary>
        public int VisibleItemCount { get; }

        /// <summary>
        /// Gets the selectable items rendered by the panel.
        /// </summary>
        public MenuItemDefinition[] Items { get; }
    }
}
