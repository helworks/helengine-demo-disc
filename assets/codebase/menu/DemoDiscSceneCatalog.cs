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
                new MenuItemDefinition("scene-cube-test", "Cube Test", "Minimal one-cube rendering validation scene.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "cube_test")),
                new MenuItemDefinition("scene-colored-cube-grid", "Colored Cubes", "Sixteen rotating cubes with distinct lit material colors.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "colored_cube_grid")),
                new MenuItemDefinition("scene-textured-cube-grid", "Textured Cubes", "Sixteen rotating cubes with distinct lit texture materials.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "textured_cube_grid")),
                new MenuItemDefinition("scene-axis-test", "Axis 1", "Three-axis rotation validation scene with a directional-light arrow.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test")),
                new MenuItemDefinition("scene-axis-test-2", "Axis 2", "Mirrored axis showcase that validates the right-side directional layout.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test2")),
                new MenuItemDefinition("scene-directional-shadow-plaza", "Directional Shadow Plaza", "Lighting showcase with an orbiting camera and decorative plaza geometry.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "directional_shadow_plaza")),
                new MenuItemDefinition("scene-spotlight-street-slice", "Spotlight Street Slice", "Night street showcase with lamppost lighting and the racer hero model.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "spotlight_street_slice")),
                new MenuItemDefinition("scene-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }
    }
}
