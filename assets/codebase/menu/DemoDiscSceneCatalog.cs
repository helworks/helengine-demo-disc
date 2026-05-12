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
                new MenuItemDefinition("scene-colored-cube-grid", "Colored Cube Grid", "Sixteen rotating cubes with distinct lit material colors.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "colored_cube_grid")),
                new MenuItemDefinition("scene-textured-cube-grid", "Textured Cube Grid", "Sixteen rotating cubes with distinct lit texture materials.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "textured_cube_grid")),
                new MenuItemDefinition("scene-axis-test", "Axis Test", "Axis-aligned debug scene for orientation and transform validation.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test")),
                new MenuItemDefinition("scene-axis-test-2", "Axis Test 2", "Axis-aligned lighting test with camera-forward directional light rotation.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test2")),
                new MenuItemDefinition("scene-directional-shadow-plaza", "Directional Shadow Plaza", "Directional light showcase scene with shadowed plaza lighting.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "directional_shadow_plaza")),
                new MenuItemDefinition("scene-spotlight-street-slice", "Spotlight Street Slice", "Spotlight showcase scene with a narrow street and bright pool lighting.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "spotlight_street_slice")),
                new MenuItemDefinition("scene-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }
    }
}

