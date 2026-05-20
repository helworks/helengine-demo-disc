namespace city.rendering.tools {
    /// <summary>
    /// Stores one generated live-authored scene definition before editor serialization persists it.
    /// </summary>
    public sealed class GeneratedAuthoringSceneDefinition {
        /// <summary>
        /// Gets or sets the stable scene id written to disk.
        /// </summary>
        public string SceneId { get; set; }

        /// <summary>
        /// Gets or sets the scene-level settings persisted with the generated scene.
        /// </summary>
        public SceneSettingsAsset SceneSettings { get; set; }

        /// <summary>
        /// Gets or sets the live root entities that define the scene.
        /// </summary>
        public Entity[] RootEntities { get; set; }

        /// <summary>
        /// Gets or sets the optional Nintendo DS companion-scene definition emitted alongside the default generated scene.
        /// </summary>
        public GeneratedDsSceneDefinition NintendoDsScene { get; set; }
    }
}
