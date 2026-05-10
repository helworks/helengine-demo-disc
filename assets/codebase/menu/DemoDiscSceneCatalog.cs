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
                new MenuItemDefinition("scene-cube-test", "Cube Test", "Minimal one-cube rendering validation scene.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/rendering/cube_test.helen")),
                new MenuItemDefinition("scene-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }
    }
}
