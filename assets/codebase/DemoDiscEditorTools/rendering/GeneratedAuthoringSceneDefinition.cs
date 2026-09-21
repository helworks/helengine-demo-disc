using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Stores one generated live-authored scene definition before editor serialization persists it.
    /// </summary>
    public sealed class GeneratedAuthoringSceneDefinition {
        /// <summary>
        /// Gets or sets the stable scene id written to disk.
        /// </summary>
        public string SceneId { get; set; }

        /// <summary>
        /// Gets or sets the optional project-relative asset path used when the authored scene file should live at a different location than its runtime scene id.
        /// </summary>
        public string SceneAssetRelativePath { get; set; }

        /// <summary>
        /// Gets or sets the explicit stable identity embedded in the generated native scene.
        /// </summary>
        public string AuthoringAssetId { get; set; }

        /// <summary>
        /// Gets or sets the scene-level settings persisted with the generated scene.
        /// </summary>
        public SceneSettingsAsset SceneSettings { get; set; }

        /// <summary>
        /// Gets or sets the live root entities that define the scene. These carry the content every platform
        /// shares and are written without any platform scope.
        /// </summary>
        public Entity[] RootEntities { get; set; }

        /// <summary>
        /// Gets or sets the roots that present the scene on a single screen: its instruction panels and the
        /// standard UI kit. They are excluded from the dual-screen group, which presents the same content
        /// through <see cref="NintendoDsScene"/> instead.
        /// </summary>
        public Entity[] DesktopPresentationRootEntities { get; set; }

        /// <summary>
        /// Gets or sets the optional dual-screen presentation merged into the canonical generated scene.
        /// </summary>
        public GeneratedDsSceneDefinition NintendoDsScene { get; set; }
    }
}
