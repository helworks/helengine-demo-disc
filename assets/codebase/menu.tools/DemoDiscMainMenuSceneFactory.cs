using city.menu;
using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Coordinates demo-disc main-menu scene authoring across the standard and Nintendo handheld scene builders.
    /// </summary>
    public sealed class DemoDiscMainMenuSceneFactory {
        /// <summary>
        /// Stable scene id used by callers that still target the standard demo-disc main menu scene.
        /// </summary>
        public const string SceneId = DemoDiscStandardMainMenuSceneFactory.SceneId;

        /// <summary>
        /// Standard main-menu scene builder.
        /// </summary>
        readonly DemoDiscStandardMainMenuSceneFactory StandardSceneFactory;

        /// <summary>
        /// Nintendo handheld main-menu scene builder.
        /// </summary>
        readonly DemoDiscHandheldMainMenuSceneFactory HandheldSceneFactory;

        /// <summary>
        /// Initializes one coordinator over the standard and handheld scene builders.
        /// </summary>
        public DemoDiscMainMenuSceneFactory() {
            StandardSceneFactory = new DemoDiscStandardMainMenuSceneFactory();
            HandheldSceneFactory = new DemoDiscHandheldMainMenuSceneFactory();
        }

        /// <summary>
        /// Creates the authored standard main-menu scene definition.
        /// </summary>
        /// <param name="providerTypeName">Assembly-qualified menu provider type name persisted on the menu root.</param>
        /// <param name="definition">Menu definition used to author the live hierarchy.</param>
        /// <returns>Standard live-authored demo-disc main menu scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateStandardSceneDefinition(string providerTypeName, MenuDefinition definition) {
            return StandardSceneFactory.CreateSceneDefinition(providerTypeName, definition);
        }

        /// <summary>
        /// Creates the authored Nintendo handheld main-menu scene definition.
        /// </summary>
        /// <param name="providerTypeName">Assembly-qualified menu provider type name persisted on the menu root.</param>
        /// <param name="definition">Menu definition used to author the live hierarchy.</param>
        /// <returns>Nintendo handheld live-authored demo-disc main menu scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateHandheldSceneDefinition(string providerTypeName, MenuDefinition definition) {
            return HandheldSceneFactory.CreateSceneDefinition(providerTypeName, definition);
        }
    }
}
