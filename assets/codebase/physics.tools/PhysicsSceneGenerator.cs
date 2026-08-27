using city.rendering.tools;
using helengine.editor;

namespace city.physics.tools {
    public sealed class PhysicsSceneGenerator {
        /// <summary>
        /// Host-owned capability used by the generated physics scenes to resolve fonts and author import settings.
        /// </summary>
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        /// <summary>
        /// Initializes one authored physics showcase generator.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used by project scene factories.</param>
        public PhysicsSceneGenerator(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

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
                new DemoSceneInstructionOverlayFactory(AssetAuthoringService),
                editorCore.DefaultFontAssetForEditor);

            PhysicsSceneFactory factory = new PhysicsSceneFactory(AssetAuthoringService);
            factory.WriteScenes(projectRootPath);
        }
    }
}
