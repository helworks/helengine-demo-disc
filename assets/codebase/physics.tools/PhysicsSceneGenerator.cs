using city.rendering.tools;
using helengine.editor;

namespace city.physics.tools {
    public sealed class PhysicsSceneGenerator {
        /// <summary>
        /// Host-owned capability used by the generated physics scenes to resolve fonts and author import settings.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Initializes one authored physics showcase generator.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used by project scene factories.</param>
        public PhysicsSceneGenerator(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Writes the current authored physics showcase scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            FontAsset editorFont = AssetAuthoringService.RendererResources.DefaultFontAsset;
            if (editorFont == null) {
                throw new InvalidOperationException("Physics scene generation requires the editor default font for the console instruction Blueprint.");
            }

            ConsoleCameraLightInstructionsBlueprintGenerator consoleInstructionBlueprintGenerator = new ConsoleCameraLightInstructionsBlueprintGenerator(AssetAuthoringService, Transaction);
            consoleInstructionBlueprintGenerator.Generate(
                projectRootPath,
                new DemoSceneInstructionOverlayFactory(AssetAuthoringService),
                editorFont);

            PhysicsSceneFactory factory = new PhysicsSceneFactory(AssetAuthoringService, Transaction);
            factory.WriteScenes(projectRootPath);
        }
    }
}
