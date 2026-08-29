using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Provides a minimal public authoring capability for material-definition unit tests.
    /// </summary>
    public sealed class TestEditorProjectAssetAuthoringService : IEditorProjectAuthoringSession {
        public string ProjectRootPath { get; }
        public Core OwningCore => throw Unsupported();
        public GeneratedAssetProviderRegistry GeneratedAssetProviders => throw Unsupported();
        public EngineGeneratedModelCache GeneratedModelCache => throw Unsupported();
        public EngineGeneratedMaterialCache GeneratedMaterialCache => throw Unsupported();
        public EditorSessionRendererResources RendererResources => throw Unsupported();
        public EditorAssetRepairReport RepairReport { get; } = new EditorAssetRepairReport();
        /// <summary>
        /// Initializes a capability that intentionally rejects project I/O because the test only inspects authored definitions.
        /// </summary>
        /// <param name="projectRootPath">Test project root retained for call-site clarity.</param>
        public TestEditorProjectAssetAuthoringService(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }
            ProjectRootPath = Path.GetFullPath(projectRootPath);
        }

        public SceneAssetReference CreateReference(string relativePath, AssetEntryKind expectedKind) => CreateFileReference(relativePath, expectedKind);
        public AssetReferenceResolution ResolveReference(SceneAssetReference reference, AssetEntryKind expectedKind) => throw Unsupported();
        public RuntimeModel LoadImportedRuntimeModel(string relativePath) => throw Unsupported();
        public ShaderAsset LoadBuiltInShaderAsset(string shaderFileName) => throw Unsupported();
        public EditorAssetWriteResult WriteAsset(string relativePath, Asset asset) => throw Unsupported();
        public EditorAuthoringTransaction BeginTransaction() => throw Unsupported();
        public void RefreshExternalChanges() { }
        public void Dispose() { }

        /// <inheritdoc />
        public TextureAssetImportSettings LoadOrCreateTextureImportSettings(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public void SaveTextureImportSettings(string sourcePath, TextureAssetImportSettings settings) => throw Unsupported();
        /// <inheritdoc />
        public ModelAssetImportSettings LoadOrCreateModelImportSettings(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public AudioAssetImportSettings LoadOrCreateAudioImportSettings(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public AssetImportSettings LoadOrCreateSectionedImportSettings(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public void SaveModelImportSettings(string sourcePath, ModelAssetImportSettings settings) => throw Unsupported();
        /// <inheritdoc />
        public void SaveAudioImportSettings(string sourcePath, AudioAssetImportSettings settings) => throw Unsupported();
        /// <inheritdoc />
        public void SaveSectionedImportSettings(string sourcePath, AssetImportSettings settings) => throw Unsupported();
        /// <inheritdoc />
        public RuntimeModel ResolveRuntimeModel(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public FontAsset ResolveFontAsset(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public TextureAsset ResolveTextureAsset(string sourcePath) => throw Unsupported();
        /// <inheritdoc />
        public ISceneAssetReferenceResolver CreateSceneAssetReferenceResolver() => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeAsset(string relativePath, Asset asset) => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeAsset(string relativePath, Asset asset, string authoringAssetId) => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeScene(
            string relativePath,
            SceneSettingsAsset sceneSettings,
            Entity[] roots,
            ComponentPersistenceRegistry persistenceRegistry,
            string authoringAssetId) => throw Unsupported();
        /// <inheritdoc />
        public bool CanonicalizeAssetReferences(Component component, EntityComponentSaveState saveState) => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeBlueprint(string relativePath, ComponentPersistenceRegistry persistenceRegistry) => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeBlueprint(string relativePath, ComponentPersistenceRegistry persistenceRegistry, string authoringAssetId) => throw Unsupported();
        /// <inheritdoc />
        public void WriteGeneratedCacheAsset(string relativePath, Asset asset) => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeMaterial(string relativePath, GeneratedMaterialAssetDefinition definition) => throw Unsupported();
        /// <inheritdoc />
        public void WriteNativeMaterial(string relativePath, GeneratedMaterialAssetDefinition definition, string authoringAssetId) => throw Unsupported();
        /// <inheritdoc />
        public SceneAssetReference CreateFileReference(string relativePath, AssetEntryKind expectedKind) => throw Unsupported();
        /// <inheritdoc />
        public TAsset LoadNativeAsset<TAsset>(string relativePath) where TAsset : Asset => throw Unsupported<TAsset>();
        /// <inheritdoc />
        public bool TryLoadImportedTextureAsset(string assetId, out TextureAsset textureAsset) {
            textureAsset = null;
            throw Unsupported();
        }
        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedPlatformIds() => throw Unsupported<IReadOnlyList<string>>();

        /// <summary>
        /// Creates the expected failure for an unsupported operation in this definition-only test double.
        /// </summary>
        /// <returns>Exception describing the intentionally unsupported operation.</returns>
        static NotSupportedException Unsupported() {
            return new NotSupportedException("This definition-only test capability does not perform project I/O.");
        }

        /// <summary>
        /// Creates the expected failure for an unsupported typed operation.
        /// </summary>
        /// <typeparam name="T">Unsupported return type.</typeparam>
        /// <returns>Exception describing the intentionally unsupported operation.</returns>
        static NotSupportedException Unsupported<T>() {
            return Unsupported();
        }
    }
}
