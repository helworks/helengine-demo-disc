namespace city.menu {
    /// <summary>
    /// Stores the curated playable scene items shown by the first-pass demo-disc scene selector.
    /// </summary>
    public sealed class DemoDiscSceneCatalog {
        /// <summary>
        /// Builds the ordered rendering-demo scene menu items shown by the demo-disc menu.
        /// </summary>
        /// <returns>Curated rendering scene menu items.</returns>
        public MenuItemDefinition[] CreateDemoSceneItems() {
            return new[] {
                new MenuItemDefinition("scene-cube-test", "Cube Test", true, new MenuActionDefinition(MenuActionKind.LoadScene, "cube_test")),
                new MenuItemDefinition("scene-colored-cube-grid", "Colored Cubes", true, new MenuActionDefinition(MenuActionKind.LoadScene, "colored_cube_grid")),
                new MenuItemDefinition("scene-textured-cube-grid", "Textured Cubes", true, new MenuActionDefinition(MenuActionKind.LoadScene, "textured_cube_grid")),
                new MenuItemDefinition("scene-axis-test", "Axis 1", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test")),
                new MenuItemDefinition("scene-axis-test-2", "Axis 2", true, new MenuActionDefinition(MenuActionKind.LoadScene, "axis_test2")),
                new MenuItemDefinition("scene-directional-shadow-plaza", "Directional Shadow Plaza", true, new MenuActionDefinition(MenuActionKind.LoadScene, "directional_shadow_plaza")),
                new MenuItemDefinition("scene-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }

        /// <summary>
        /// Builds the ordered physics-demo scene menu items shown by the demo-disc menu.
        /// </summary>
        /// <returns>Curated physics scene menu items.</returns>
        public MenuItemDefinition[] CreatePhysicsSceneItems() {
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries = CreatePhysicsSceneEntries();
            MenuItemDefinition[] items = new MenuItemDefinition[physicsSceneEntries.Count + 1];
            for (int index = 0; index < physicsSceneEntries.Count; index++) {
                DemoDiscPhysicsSceneEntry sceneEntry = physicsSceneEntries[index];
                items[index] = new MenuItemDefinition(
                    sceneEntry.MenuItemId,
                    sceneEntry.DisplayName,
                    true,
                    new MenuActionDefinition(MenuActionKind.LoadScene, sceneEntry.SceneId));
            }

            items[physicsSceneEntries.Count] = new MenuItemDefinition("physics-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty));
            return items;
        }

        /// <summary>
        /// Builds the curated physics scene entry set shared by the demo-disc menu and DS companion scene generators.
        /// </summary>
        /// <returns>Curated physics scene entries.</returns>
        public IReadOnlyList<DemoDiscPhysicsSceneEntry> CreatePhysicsSceneEntries() {
            return [
                new DemoDiscPhysicsSceneEntry(
                    "physics-dynamic-stack-boxes",
                    "Stacked Boxes",
                    "test_scene_dynamic_stack_boxes",
                    "test_scene_dynamic_stack_boxes_ds"),
                new DemoDiscPhysicsSceneEntry(
                    "physics-dynamic-sphere-stack",
                    "Sphere Stack",
                    "test_scene_dynamic_sphere_stack",
                    "test_scene_dynamic_sphere_stack_ds"),
                new DemoDiscPhysicsSceneEntry(
                    "physics-dynamic-mixed-stack",
                    "Mixed Stack",
                    "test_scene_dynamic_mixed_stack",
                    "test_scene_dynamic_mixed_stack_ds")
            ];
        }
    }
}
