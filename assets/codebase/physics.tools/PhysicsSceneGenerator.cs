namespace city.physics.tools {
    /// <summary>
    /// Generates the complete city physics showcase scene set declared by the demo-disc scene catalog.
    /// </summary>
    public sealed class PhysicsSceneGenerator {
        /// <summary>
        /// Writes the current city physics showcase scene set into the supplied project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            PhysicsValidationSceneFactory factory = new PhysicsValidationSceneFactory();
            factory.WriteScenes(projectRootPath);
        }
    }
}
