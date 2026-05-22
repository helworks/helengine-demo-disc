namespace city.physics.tools {
    /// <summary>
    /// Generates the complete city physics showcase scene set declared by the demo-disc scene catalog.
    /// </summary>
    public sealed class PhysicsSceneGenerator {
        /// <summary>
        /// Writer used to persist generated live-authored physics scenes through the editor save pipeline.
        /// </summary>
        readonly PhysicsAuthoringSceneWriteService AuthoringSceneWriteService;

        /// <summary>
        /// Initializes one city physics scene generator.
        /// </summary>
        public PhysicsSceneGenerator() {
            AuthoringSceneWriteService = new PhysicsAuthoringSceneWriteService();
        }

        /// <summary>
        /// Writes the current city physics showcase scene set into the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            PhysicsSceneFactory factory = new PhysicsSceneFactory();
            factory.WriteSupportAssets(projectRootPath);

            string[] sceneIds = PhysicsSceneCatalog.GetSceneIds();
            for (int index = 0; index < sceneIds.Length; index++) {
                PhysicsAuthoringSceneDefinition sceneDefinition = factory.CreateSceneDefinition(sceneIds[index]);
                AuthoringSceneWriteService.WriteScene(projectRootPath, sceneDefinition);
            }
        }
    }
}
