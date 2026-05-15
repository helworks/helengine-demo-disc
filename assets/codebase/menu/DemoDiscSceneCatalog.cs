namespace city.menu {
    /// <summary>
    /// Stores the curated playable scene items shown by the first-pass demo-disc scene selector.
    /// </summary>
    public sealed class DemoDiscSceneCatalog {
        /// <summary>
        /// Builds the ordered playable scene menu items shown by the demo-disc menu.
        /// </summary>
        /// <returns>Curated scene menu items.</returns>
        public MenuItemDefinition[] CreateSceneItems() {
            return new[] {
                new MenuItemDefinition("scene-directional-shadow-plaza", "Directional Shadow Plaza", "Lighting showcase with an orbiting camera and decorative plaza geometry.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "directional_shadow_plaza")),
                new MenuItemDefinition("scene-spotlight-street-slice", "Spotlight Street Slice", "Night street showcase with lamppost lighting and the racer hero model.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "spotlight_street_slice")),
                new MenuItemDefinition("scene-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }
    }
}

