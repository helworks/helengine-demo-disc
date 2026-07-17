using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Provides the separately generated handheld Tilt Trial level-selector scene definition.
    /// </summary>
    public sealed class TiltTrialHandheldLevelSelectSceneFactory {
        /// <summary>
        /// Creates the handheld selector scene through the shared game scene factory.
        /// </summary>
        /// <param name="sceneFactory">Shared scene factory owning fonts and generated scene assets.</param>
        /// <returns>Handheld selector scene definition.</returns>
        public GeneratedAuthoringSceneDefinition Create(GameSceneFactory sceneFactory) {
            if (sceneFactory == null) {
                throw new ArgumentNullException(nameof(sceneFactory));
            }

            return sceneFactory.CreateTiltTrialHandheldLevelSelectScene();
        }
    }
}
