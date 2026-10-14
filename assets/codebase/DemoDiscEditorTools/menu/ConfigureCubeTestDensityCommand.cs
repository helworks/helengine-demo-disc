using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>Authors Android density settings on the existing Cube Test scene without regenerating its content.</summary>
    public sealed class ConfigureCubeTestDensityCommand : IEditorCommand {
        /// <summary>Gets the command used by the editor CLI to configure the existing scene.</summary>
        public string CommandId => "menu.configure-cube-test-density";
        /// <summary>Gets the menu label for the density configuration action.</summary>
        public string DisplayName => "Configure Cube Test Density";
        /// <summary>Publishes Android viewport, font and edge-anchor overrides through the normal authoring transaction.</summary>
        /// <param name="context">Host-owned authoring session for the existing Cube Test and shared Blueprint.</param>
        public void Execute(IEditorCommandContext context) {
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create(context.ScriptTypeResolver);
            AndroidSceneViewportAuthoringService viewports = new AndroidSceneViewportAuthoringService();
            SceneAsset scene = context.Authoring.LoadNativeAsset<SceneAsset>(CubeTestSceneFactory.SceneId);
            ISceneAssetReferenceResolver resolver = context.Authoring.CreateSceneAssetReferenceResolver();
            viewports.ConfigureScene(scene, registry, resolver);
            BlueprintAsset blueprint = context.Authoring.LoadNativeAsset<BlueprintAsset>(ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath);
            viewports.ConfigureBlueprint(blueprint, registry, resolver);
            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            transaction.WriteAsset(CubeTestSceneFactory.SceneId, scene);
            transaction.WriteAsset(ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath, blueprint);
            transaction.Commit();
        }
    }
}
