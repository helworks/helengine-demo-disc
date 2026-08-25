namespace city.rendering.tools {
    /// <summary>
    /// Writes generated material assets through the public editor material authoring API.
    /// </summary>
    public sealed class GeneratedMaterialAssetWriteService {
        readonly global::helengine.editor.GeneratedMaterialAssetWriteService EditorWriteService;

        /// <summary>
        /// Initializes one generated material write service.
        /// </summary>
        public GeneratedMaterialAssetWriteService() {
            EditorWriteService = new global::helengine.editor.GeneratedMaterialAssetWriteService();
        }

        /// <summary>
        /// Writes one generated material asset under the project assets folder and persists its per-platform sidecar settings.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path.</param>
        /// <param name="relativePath">Project-relative material asset path.</param>
        /// <param name="definition">Generated material definition to write.</param>
        public void WriteMaterial(string projectRootPath, string relativePath, GeneratedMaterialAssetDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            global::helengine.editor.GeneratedMaterialAssetDefinition editorDefinition =
                new global::helengine.editor.GeneratedMaterialAssetDefinition {
                    MaterialAsset = definition.MaterialAsset,
                    SourceChecksum = string.Empty
                };
            foreach (KeyValuePair<string, GeneratedMaterialPlatformDefinition> platformEntry in definition.Platforms) {
                if (platformEntry.Value == null) {
                    continue;
                }

                global::helengine.editor.GeneratedMaterialPlatformDefinition editorPlatform =
                    editorDefinition.GetOrCreatePlatform(platformEntry.Key);
                editorPlatform.SchemaId = platformEntry.Value.SchemaId;
                foreach (KeyValuePair<string, string> fieldEntry in platformEntry.Value.FieldValues) {
                    editorPlatform.SetFieldValue(fieldEntry.Key, fieldEntry.Value ?? string.Empty);
                }
            }

            EditorWriteService.WriteMaterial(projectRootPath, relativePath, editorDefinition);
        }
    }
}
