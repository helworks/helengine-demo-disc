namespace DemoDisc.menu {
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
                new MenuItemDefinition("scene-matrix-render", "Matrix Render", true, new MenuActionDefinition(MenuActionKind.LoadScene, "test_scene_matrix_render")),
                new MenuItemDefinition("scene-directional-shadow-plaza", "Directional Shadow Plaza", true, new MenuActionDefinition(MenuActionKind.LoadScene, "directional_shadow_plaza")),
                new MenuItemDefinition("scene-pbr-material-gallery", "PBR Material Gallery", true, new MenuActionDefinition(MenuActionKind.LoadScene, "pbr_material_gallery")),
                new MenuItemDefinition("scene-pbr-textured-showcase", "PBR Textured Showcase", true, new MenuActionDefinition(MenuActionKind.LoadScene, "pbr_textured_showcase")),
                new MenuItemDefinition("scene-pbr-shadow-theater", "PBR Shadow Theater", true, new MenuActionDefinition(MenuActionKind.LoadScene, "pbr_shadow_theater")),
                new MenuItemDefinition("scene-ray-tracing", "Ray Tracing", true, new MenuActionDefinition(MenuActionKind.OpenPanel, "ray-tracing-select")),
                new MenuItemDefinition("scene-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            };
        }

        /// <summary>
        /// Builds the scenes rendered by the shared software ray tracer.
        /// </summary>
        /// <returns>Ray-traced scene entries followed by the category return action.</returns>
        public MenuItemDefinition[] CreateSoftwareRayTracingSceneItems() {
            return [
                new MenuItemDefinition("scene-software-path-tracer", "Cornell Box", true, new MenuActionDefinition(MenuActionKind.LoadScene, "software_path_tracer")),
                new MenuItemDefinition("scene-ray-tracing-teapot", "Teapot", true, new MenuActionDefinition(MenuActionKind.LoadScene, "ray_tracing_teapot")),
                new MenuItemDefinition("scene-ray-tracing-spheres", "Material Spheres", true, new MenuActionDefinition(MenuActionKind.LoadScene, "ray_tracing_spheres")),
                new MenuItemDefinition("scene-ray-tracing-soft-shadows", "Soft Shadows", true, new MenuActionDefinition(MenuActionKind.LoadScene, "ray_tracing_soft_shadows")),
                new MenuItemDefinition("ray-tracing-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            ];
        }

        /// <summary>
        /// Builds the ordered physics-demo scene menu items shown by the demo-disc menu.
        /// </summary>
        /// <returns>Curated physics scene menu items.</returns>
        public MenuItemDefinition[] CreatePhysicsSceneItems() {
#if DESKTOP_PLATFORM
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries = CreateDesktopPhysicsSceneEntries();
#else
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries = CreatePhysicsSceneEntries();
#endif
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
        /// Builds the ordered game-demo scene menu items shown by the demo-disc menu.
        /// </summary>
        /// <returns>Curated game scene menu items.</returns>
        public MenuItemDefinition[] CreateGameSceneItems() {
            IReadOnlyList<DemoDiscGameSceneEntry> gameSceneEntries = CreateGameSceneEntries();
            MenuItemDefinition[] items = new MenuItemDefinition[gameSceneEntries.Count + 1];
            for (int index = 0; index < gameSceneEntries.Count; index++) {
                DemoDiscGameSceneEntry sceneEntry = gameSceneEntries[index];
                items[index] = new MenuItemDefinition(
                    sceneEntry.MenuItemId,
                    sceneEntry.DisplayName,
                    true,
                    new MenuActionDefinition(MenuActionKind.LoadScene, sceneEntry.SceneId));
            }

            items[gameSceneEntries.Count] = new MenuItemDefinition("games-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty));
            return items;
        }

        /// <summary>
        /// Builds the benchmark menu items shown by the demo-disc menu.
        /// </summary>
        /// <returns>Benchmark entries followed by the category return action.</returns>
        public MenuItemDefinition[] CreateBenchmarkSceneItems() {
            return [
                new MenuItemDefinition("benchmark-max-objects", "Max Objects", true, new MenuActionDefinition(MenuActionKind.LoadScene, "max_objects")),
                new MenuItemDefinition("benchmarks-back", "Back", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
            ];
        }

        /// <summary>
        /// Builds the curated physics scene entry set shared by the demo-disc menu and handheld scene generators.
        /// </summary>
        /// <returns>Curated physics scene entries.</returns>
        public IReadOnlyList<DemoDiscPhysicsSceneEntry> CreatePhysicsSceneEntries() {
            return [
                new DemoDiscPhysicsSceneEntry(
                    "physics-dynamic-stack-boxes",
                    "Stacked Boxes",
                    "test_scene_dynamic_stack_boxes"),
                new DemoDiscPhysicsSceneEntry(
                    "physics-dynamic-sphere-stack",
                    "Sphere Stack",
                    "test_scene_dynamic_sphere_stack"),
                new DemoDiscPhysicsSceneEntry(
                    "physics-dynamic-mixed-stack",
                    "Mixed Stack",
                    "test_scene_dynamic_mixed_stack")
            ];
        }

        /// <summary>
        /// Exposes all twelve authored desktop physics scenes while retaining the separate curated handheld list.
        /// </summary>
        /// <returns>Complete desktop physics entries in stable menu order.</returns>
        public IReadOnlyList<DemoDiscPhysicsSceneEntry> CreateDesktopPhysicsSceneEntries() {
            return [
                new DemoDiscPhysicsSceneEntry("physics-dynamic-stack-boxes", "Stacked Boxes", "test_scene_dynamic_stack_boxes"),
                new DemoDiscPhysicsSceneEntry("physics-dynamic-sphere-stack", "Sphere Stack", "test_scene_dynamic_sphere_stack"),
                new DemoDiscPhysicsSceneEntry("physics-dynamic-mixed-stack", "Mixed Stack", "test_scene_dynamic_mixed_stack"),
                new DemoDiscPhysicsSceneEntry("physics-single-falling-cube", "Falling Cube", "test_scene_single_falling_cube"),
                new DemoDiscPhysicsSceneEntry("physics-character-moving-platform", "Moving Platform", "test_scene_character_moving_platform"),
                new DemoDiscPhysicsSceneEntry("physics-character-slope", "Character Slope", "test_scene_character_slope"),
                new DemoDiscPhysicsSceneEntry("physics-character-steps", "Character Steps", "test_scene_character_steps"),
                new DemoDiscPhysicsSceneEntry("physics-kinematic-push", "Kinematic Push", "test_scene_kinematic_push"),
                new DemoDiscPhysicsSceneEntry("physics-mesh-ground-stability", "Mesh Ground", "test_scene_mesh_ground_stability"),
                new DemoDiscPhysicsSceneEntry("physics-render-only-slope", "Render Slope", "test_scene_render_only_slope"),
                new DemoDiscPhysicsSceneEntry("physics-strict-rotated-box", "Rotated Box", "test_scene_strict_rotated_box_compare"),
                new DemoDiscPhysicsSceneEntry("physics-trigger-volume", "Trigger Volume", "test_scene_trigger_volume")
            ];
        }
        /// <summary>
        /// Builds the curated Nintendo handheld physics scene entry set, including the matrix render probe that should not appear in the playable physics menu.
        /// </summary>
        /// <returns>Curated Nintendo handheld scene entries.</returns>
        public IReadOnlyList<DemoDiscPhysicsSceneEntry> CreatePhysicsNintendoHandheldSceneEntries() {
            IReadOnlyList<DemoDiscPhysicsSceneEntry> physicsSceneEntries = CreatePhysicsSceneEntries();
            DemoDiscPhysicsSceneEntry[] nintendoDsSceneEntries = new DemoDiscPhysicsSceneEntry[physicsSceneEntries.Count + 1];
            for (int index = 0; index < physicsSceneEntries.Count; index++) {
                nintendoDsSceneEntries[index] = physicsSceneEntries[index];
            }

            nintendoDsSceneEntries[physicsSceneEntries.Count] = new DemoDiscPhysicsSceneEntry(
                "physics-matrix-render-handheld",
                "Matrix Render",
                "test_scene_matrix_render");
            return nintendoDsSceneEntries;
        }

        /// <summary>
        /// Builds the curated game scene entry set shared by the demo-disc menu and generated gameplay scene pipeline.
        /// </summary>
        /// <returns>Curated game scene entries.</returns>
        public IReadOnlyList<DemoDiscGameSceneEntry> CreateGameSceneEntries() {
            return [
                new DemoDiscGameSceneEntry(
                    "games-tilt-trial",
                    "Tilt Trial",
                    "tilt_trial"),
#if DESKTOP_PLATFORM
                new DemoDiscGameSceneEntry("games-zombislayer", "Zombislayer", "zombislayer")
#endif
            ];
        }
    }
}
