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
                new MenuItemDefinition("scene-cube-test", "Cube Test", true, new MenuActionDefinition(MenuActionKind.LoadScene, "cube_test")),
                new MenuItemDefinition("scene-scaled-cube", "Scaled Cube", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scaled_cube")),
                new MenuItemDefinition("scene-colored-cube-grid", "Colored Cubes", true, new MenuActionDefinition(MenuActionKind.LoadScene, "colored_cube_grid")),
                new MenuItemDefinition("scene-textured-cube-grid", "Textured Cubes", true, new MenuActionDefinition(MenuActionKind.LoadScene, "textured_cube_grid")),
                new MenuItemDefinition("scene-axis-test", "Axis 1", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test")),
                new MenuItemDefinition("scene-axis-test-2", "Axis 2", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test2")),
                new MenuItemDefinition("scene-directional-shadow-plaza", "Directional Shadow Plaza", true, new MenuActionDefinition(MenuActionKind.LoadScene, "directional_shadow_plaza")),
                new MenuItemDefinition("scene-spotlight-street-slice", "Spotlight Street Slice", true, new MenuActionDefinition(MenuActionKind.LoadScene, "spotlight_street_slice")),
                new MenuItemDefinition("scene-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }
    }
}
