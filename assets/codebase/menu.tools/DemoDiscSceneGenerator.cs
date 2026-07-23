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
        /// Authoring helper used to stamp build-scene-driven platform existence overrides into the canonical generated menu scene.
        /// </summary>
        readonly DemoDiscMenuBuildSceneAuthoringService MenuBuildSceneAuthoringService;

        /// <summary>
        /// Factory used to author the initial Helen of Code splash scene.
        /// </summary>
        readonly HelenOfCodeSplashSceneFactory SplashSceneFactory;

        /// <summary>
        /// Initializes one demo-disc scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to restore project-authored components during temporary handheld clone loads.</param>
        public DemoDiscSceneGenerator(IScriptTypeResolver scriptTypeResolver = null) {
            SceneWriteService = new GeneratedAuthoringSceneWriteService(scriptTypeResolver);
            SceneFactory = new DemoDiscMainMenuSceneFactory();
            MenuBuildSceneAuthoringService = new DemoDiscMenuBuildSceneAuthoringService();
            SplashSceneFactory = new HelenOfCodeSplashSceneFactory();
        }

        /// <summary>
        /// Rebuilds the authored demo-disc main menu scene for the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            DeleteObsoleteNintendoHandheldCompanionScene(projectRootPath);
            DemoDiscMenuDefinitionProvider provider = new DemoDiscMenuDefinitionProvider();
            MenuDefinition definition = provider.CreateMenuDefinition();
            string providerTypeName = BuildProviderTypeName(typeof(DemoDiscMenuDefinitionProvider));
            SceneWriteService.WriteScene(projectRootPath, SplashSceneFactory.CreateSceneDefinition());
            GeneratedAuthoringSceneDefinition standardSceneDefinition = SceneFactory.CreateStandardSceneDefinition(providerTypeName, definition);
            MenuBuildSceneAuthoringService.ApplyBuildSceneAvailability(projectRootPath, standardSceneDefinition, definition);
            SceneWriteService.WriteScene(projectRootPath, standardSceneDefinition);

            GeneratedAuthoringSceneDefinition handheldSceneDefinition = SceneFactory.CreateHandheldSceneDefinition(providerTypeName, definition);
            MenuBuildSceneAuthoringService.ApplyBuildSceneAvailability(projectRootPath, handheldSceneDefinition, definition);
            SceneWriteService.WriteScene(projectRootPath, handheldSceneDefinition);
        }

        /// <summary>
        /// Deletes the obsolete Nintendo handheld companion menu scene so stale generated output does not remain discoverable in the project scene catalog.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        static void DeleteObsoleteNintendoHandheldCompanionScene(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string obsoleteScenePath = Path.Combine(Path.GetFullPath(projectRootPath), "assets", "scenes", "DemoDiscMainMenuDs.helen");
            if (File.Exists(obsoleteScenePath)) {
                File.Delete(obsoleteScenePath);
            }
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
