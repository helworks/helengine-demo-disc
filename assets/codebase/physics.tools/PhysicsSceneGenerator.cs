using city.rendering.tools;

namespace city.physics.tools {
    /// <summary>
    /// Generates the complete authored physics showcase scene set inside the active city project.
    /// </summary>
    public sealed class PhysicsSceneGenerator {
        /// <summary>
        /// Writes the current authored physics showcase scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            if (Core.Instance is not EditorCore editorCore) {
                throw new InvalidOperationException("Physics scene generation requires an editor core for the console instruction Blueprint.");
            } else if (editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("Physics scene generation requires the editor default font for the console instruction Blueprint.");
            }

            ConsoleCameraLightInstructionsBlueprintGenerator consoleInstructionBlueprintGenerator = new ConsoleCameraLightInstructionsBlueprintGenerator();
            consoleInstructionBlueprintGenerator.Generate(
                projectRootPath,
                new DemoSceneInstructionOverlayFactory(),
                editorCore.DefaultFontAssetForEditor);

            PhysicsSceneFactory factory = new PhysicsSceneFactory();
            factory.WriteScenes(projectRootPath);
        }
    }
}
