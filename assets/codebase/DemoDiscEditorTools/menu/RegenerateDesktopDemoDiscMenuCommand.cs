using DemoDisc.menu;
using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>Rebuilds only the complete desktop menu through the official authoring transaction.</summary>
    public sealed class RegenerateDesktopDemoDiscMenuCommand : IEditorCommand {
        public string CommandId => "menu.regenerate-desktop-demo-disc-menu";
        public string DisplayName => "Regenerate Desktop Demo Disc Menu";
        public void Execute(IEditorCommandContext context) {
            if (context == null) throw new ArgumentNullException(nameof(context));
            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            DemoDiscMenuDefinitionProvider provider = new DemoDiscMenuDefinitionProvider();
            DemoDiscStandardMainMenuSceneFactory factory = new DemoDiscStandardMainMenuSceneFactory(context.Authoring);
            GeneratedAuthoringSceneWriteService writer = new GeneratedAuthoringSceneWriteService(context.ScriptTypeResolver, context.Authoring, transaction);
            writer.WriteScene(factory.CreateSceneDefinition("DemoDisc.menu.DemoDiscMenuDefinitionProvider, DemoDisc", new DemoDiscDesktopMenuDefinitionFactory().Create()));
            transaction.Commit();
        }
    }
}
