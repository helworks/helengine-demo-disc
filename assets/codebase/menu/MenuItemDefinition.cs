namespace city.menu {
    /// <summary>
    /// Describes one selectable entry rendered inside a city demo-disc menu panel.
    /// </summary>
    public sealed class MenuItemDefinition {
        /// <summary>
        /// Initializes one city menu item definition.
        /// </summary>
        /// <param name="itemId">Stable item identifier used for validation and diagnostics.</param>
        /// <param name="label">Visible label shown on the interactive button.</param>
        /// <param name="enabled">True when the item may currently be selected and activated.</param>
        /// <param name="action">Action performed when the item is confirmed.</param>
        public MenuItemDefinition(string itemId, string label, bool enabled, MenuActionDefinition action) {
            if (string.IsNullOrWhiteSpace(itemId)) {
                throw new ArgumentException("Menu item id must be provided.", nameof(itemId));
            }
            if (string.IsNullOrWhiteSpace(label)) {
                throw new ArgumentException("Menu item label must be provided.", nameof(label));
            }
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            ItemId = itemId;
            Label = label;
            Enabled = enabled;
            Action = action;
        }

        /// <summary>
        /// Gets the stable item identifier.
        /// </summary>
        public string ItemId { get; }

        /// <summary>
        /// Gets the visible label rendered for the item.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets a value indicating whether the item may be selected and activated.
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// Gets the action performed when the item is confirmed.
        /// </summary>
        public MenuActionDefinition Action { get; }
    }
}
