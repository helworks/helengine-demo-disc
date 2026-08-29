using city.rendering.tools;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Exposes explicit editor generation for the shared console camera/light instruction Blueprint.
    /// </summary>
    public sealed class GenerateConsoleCameraLightInstructionsBlueprintCommand : IEditorCommand {
        /// <summary>
        /// Gets the stable editor command identifier.
        /// </summary>
        public string CommandId => "menu.generate-console-camera-light-instructions-blueprint";

        /// <summary>
        /// Gets the human-readable command label.
        /// </summary>
        public string DisplayName => "Generate Console Camera/Light Instructions Blueprint";

        /// <summary>
        /// Generates the shared console camera/light instruction Blueprint for the active project.
        /// </summary>
        /// <param name="context">Editor command context supplied by the host.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Authoring.OwningCore is not EditorCore editorCore) {
                throw new InvalidOperationException("Console camera/light Blueprint generation requires an editor core.");
            } else if (context.Authoring.RendererResources.DefaultFontAsset == null) {
                throw new InvalidOperationException("Console camera/light Blueprint generation requires the editor default font.");
            }

            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            ConsoleCameraLightInstructionsBlueprintGenerator generator = new ConsoleCameraLightInstructionsBlueprintGenerator(context.Authoring, transaction);
            generator.Generate(
                context.ProjectRootPath,
                new DemoSceneInstructionOverlayFactory(context.Authoring, transaction),
                context.Authoring.RendererResources.DefaultFontAsset);
            transaction.Commit();
        }
    }
}
