using helengine.editor;

namespace city.rendering.tools {
    /// <summary>Regenerates the four ray tracing scenes and their two procedural model assets.</summary>
    public sealed class RegenerateRayTracingShowcasesCommand : IEditorCommand {
        /// <summary>Stable headless editor command identifier.</summary>
        public string CommandId => "rendering.regenerate-ray-tracing-showcases";
        /// <summary>Label shown by the editor command catalog.</summary>
        public string DisplayName => "Regenerate Ray Tracing Showcases";
        /// <summary>Publishes the requested scenes through the normal transactional editor save pipeline.</summary>
        public void Execute(IEditorCommandContext context) {
            if (context == null) throw new ArgumentNullException(nameof(context));
            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            SoftwareRayTracingMeshFactory.WriteAssets(transaction);
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(context.Authoring);
            GeneratedAuthoringSceneWriteService writer = new GeneratedAuthoringSceneWriteService(context.ScriptTypeResolver, context.Authoring, transaction);
            EditorCore core = (EditorCore)context.Authoring.OwningCore;
            writer.WriteScene(factory.CreateSceneDefinition(context.ProjectRootPath, EngineSceneAssetReferenceFactory.CreateCubeModel(), core.DefaultFontAssetForEditor));
            foreach (string sceneId in new[] { SoftwareRayTracingShowcaseFactory.TeapotSceneId, SoftwareRayTracingShowcaseFactory.SpheresSceneId, SoftwareRayTracingShowcaseFactory.ShadowsSceneId }) {
                writer.WriteScene(factory.CreateShowcaseDefinition(context.ProjectRootPath, sceneId, core.DefaultFontAssetForEditor, transaction.CreateReference(SoftwareRayTracingMeshFactory.SpherePath, AssetEntryKind.Model), transaction.CreateReference(SoftwareRayTracingMeshFactory.TeapotPath, AssetEntryKind.Model)));
            }
            transaction.Commit();
        }
    }
}
