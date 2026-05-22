using city.menu;
using city.rendering.tools;

namespace city.menu.tools {
    /// <summary>
    /// Generates the authored demo-disc main menu scene inside the active city project.
    /// </summary>
    public sealed class DemoDiscSceneGenerator {
        /// <summary>
        /// Writer used to persist generated live-authored scenes through the editor scene save pipeline.
        /// </summary>
        readonly GeneratedAuthoringSceneWriteService SceneWriteService;

        /// <summary>
        /// Factory used to author the live demo-disc main menu scene hierarchy.
        /// </summary>
        readonly DemoDiscMainMenuSceneFactory SceneFactory;

        /// <summary>
        /// Initializes one demo-disc scene generator.
        /// </summary>
        public DemoDiscSceneGenerator() {
            SceneWriteService = new GeneratedAuthoringSceneWriteService();
            SceneFactory = new DemoDiscMainMenuSceneFactory();
        }

        /// <summary>
        /// Rebuilds the authored demo-disc main menu scene for the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            DemoDiscMenuDefinitionProvider provider = new DemoDiscMenuDefinitionProvider();
            MenuDefinition definition = provider.CreateMenuDefinition();
            string providerTypeName = BuildProviderTypeName(typeof(DemoDiscMenuDefinitionProvider));
            GeneratedAuthoringSceneDefinition sceneDefinition = SceneFactory.CreateSceneDefinition(providerTypeName, definition);
            SceneWriteService.WriteScene(projectRootPath, sceneDefinition);
        }

        /// <summary>
        /// Builds the persisted provider type id stored on the authored menu root.
        /// </summary>
        /// <param name="providerType">Provider type that should be rebuilt by editor tooling later.</param>
        /// <returns>Assembly-qualified provider type id.</returns>
        string BuildProviderTypeName(Type providerType) {
            if (providerType == null) {
                throw new ArgumentNullException(nameof(providerType));
            } else if (string.IsNullOrWhiteSpace(providerType.FullName)) {
                throw new InvalidOperationException("Menu provider types must expose a full name.");
            }

            string assemblyName = providerType.Assembly.GetName().Name;
            if (string.IsNullOrWhiteSpace(assemblyName)) {
                throw new InvalidOperationException($"Menu provider type '{providerType.FullName}' must belong to one named assembly.");
            }

            return providerType.FullName + ", " + assemblyName;
        }
    }
}
