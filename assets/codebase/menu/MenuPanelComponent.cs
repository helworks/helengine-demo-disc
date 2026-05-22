namespace city.menu {
    /// <summary>
    /// Identifies one baked city demo menu panel inside the generated menu hierarchy.
    /// </summary>
    public sealed class MenuPanelComponent : Component {
        /// <summary>
        /// Current payload version used by scene persistence for baked menu panels.
        /// </summary>
        public const byte CurrentVersion = 1;

        /// <summary>
        /// Stable serialized component type id used by baked menu panel scene records.
        /// </summary>
        public const string SerializedComponentTypeId = "city.menu.MenuPanelComponent, gameplay";

        /// <summary>
        /// Backing field for the stable panel id.
        /// </summary>
        string PanelIdValue;

        /// <summary>
        /// Initializes a new baked city menu panel component.
        /// </summary>
        public MenuPanelComponent() {
            PanelIdValue = string.Empty;
        }

        /// <summary>
        /// Gets or sets the stable panel id represented by the owning entity.
        /// </summary>
        public string PanelId {
            get { return PanelIdValue; }
            set { PanelIdValue = value ?? string.Empty; }
        }
    }
}
