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
                new MenuItemDefinition("scene-new-scene", "Neon Crossroads", "Loads the original city sandbox scene.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "NewScene.helen")),
                new MenuItemDefinition("scene-stack-boxes", "Stack Boxes", "Physics stress test with stacked dynamic boxes.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_dynamic_stack_boxes.helen")),
                new MenuItemDefinition("scene-ramp", "Sphere Ramp", "Dynamic sphere ramp test for broad motion and bounce.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_dynamic_sphere_ramp.helen")),
                new MenuItemDefinition("scene-steps", "Character Steps", "Character controller step traversal test.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_character_steps.helen")),
                new MenuItemDefinition("scene-slope", "Character Slope", "Character controller slope handling test.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_character_slope.helen")),
                new MenuItemDefinition("scene-platform", "Moving Platform", "Character moving-platform interaction test.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_character_moving_platform.helen")),
                new MenuItemDefinition("scene-push", "Kinematic Push", "Kinematic pusher interaction test.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_kinematic_push.helen")),
                new MenuItemDefinition("scene-ground", "Ground Stability", "Ground contact and settling stability test.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_mesh_ground_stability.helen")),
                new MenuItemDefinition("scene-trigger", "Trigger Volume", "Trigger enter and exit test scene.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/physics/test_scene_trigger_volume.helen")),
                new MenuItemDefinition("scene-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }
    }
}
