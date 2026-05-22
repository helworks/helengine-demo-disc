namespace city.physics.tools {
    /// <summary>
    /// Describes one generated physics authoring scene that should be persisted through the editor scene save pipeline.
    /// </summary>
    public sealed class PhysicsAuthoringSceneDefinition {
        /// <summary>
        /// Gets or sets the project-relative scene id that will receive the saved authoring scene.
        /// </summary>
        public string SceneId { get; set; }

        /// <summary>
        /// Gets or sets the scene-level settings saved alongside the generated roots.
        /// </summary>
        public SceneSettingsAsset SceneSettings { get; set; }

        /// <summary>
        /// Gets or sets the live generated root entities that should be visible to the editor save service.
        /// </summary>
        public Entity[] RootEntities { get; set; }
    }
}
