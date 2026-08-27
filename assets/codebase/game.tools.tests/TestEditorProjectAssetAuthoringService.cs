using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Provides a current-format test double for the public project asset-authoring capability.
    /// </summary>
    public sealed class TestEditorProjectAssetAuthoringService : IEditorProjectAssetAuthoringService {
        /// <summary>
        /// Gets the temporary project root that receives authored test assets.
        /// </summary>
        public string ProjectRootPath { get; }

        /// <summary>
        /// Initializes one test authoring capability rooted at the supplied temporary project.
        /// </summary>
        /// <param name="projectRootPath">Temporary project root used by the test.</param>
        public TestEditorProjectAssetAuthoringService(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            ProjectRootPath = Path.GetFullPath(projectRootPath);
            Directory.CreateDirectory(Path.Combine(ProjectRootPath, "assets"));
        }

        /// <inheritdoc />
        public TextureAssetImportSettings LoadOrCreateTextureImportSettings(string sourcePath) {
            throw new NotSupportedException("Texture settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public void SaveTextureImportSettings(string sourcePath, TextureAssetImportSettings settings) {
            throw new NotSupportedException("Texture settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public ModelAssetImportSettings LoadOrCreateModelImportSettings(string sourcePath) {
            throw new NotSupportedException("Model settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public AudioAssetImportSettings LoadOrCreateAudioImportSettings(string sourcePath) {
            throw new NotSupportedException("Audio settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public AssetImportSettings LoadOrCreateSectionedImportSettings(string sourcePath) {
            throw new NotSupportedException("Sectioned settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public void SaveModelImportSettings(string sourcePath, ModelAssetImportSettings settings) {
            throw new NotSupportedException("Model settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public void SaveAudioImportSettings(string sourcePath, AudioAssetImportSettings settings) {
            throw new NotSupportedException("Audio settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public void SaveSectionedImportSettings(string sourcePath, AssetImportSettings settings) {
            throw new NotSupportedException("Sectioned settings are not used by this test capability.");
        }

        /// <inheritdoc />
        public RuntimeModel ResolveRuntimeModel(string sourcePath) {
            throw new NotSupportedException("Runtime model resolution is not used by this test capability.");
        }

        /// <inheritdoc />
        public FontAsset ResolveFontAsset(string sourcePath) {
            throw new NotSupportedException("Font resolution is not used by this test capability.");
        }

        /// <inheritdoc />
        public TextureAsset ResolveTextureAsset(string sourcePath) {
            throw new NotSupportedException("Texture resolution is not used by this test capability.");
        }

        /// <inheritdoc />
        public ISceneAssetReferenceResolver CreateSceneAssetReferenceResolver() {
            throw new NotSupportedException("Scene reference resolution is not used by this test capability.");
        }

        /// <inheritdoc />
        public void WriteNativeAsset(string relativePath, Asset asset) {
            ValidateAssetPath(relativePath);
            if (asset == null) {
                throw new ArgumentNullException(nameof(asset));
            }

            if (string.IsNullOrWhiteSpace(asset.AuthoringAssetId)) {
                asset.AuthoringAssetId = Guid.NewGuid().ToString("N");
            }
            asset.FormerAuthoringAssetIds ??= Array.Empty<string>();

            string fullPath = ResolveAssetPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            using FileStream stream = File.Create(fullPath);
            global::helengine.editor.AssetSerializer.Serialize(stream, asset);
        }

        /// <inheritdoc />
        public void WriteNativeAsset(string relativePath, Asset asset, string authoringAssetId) {
            if (asset == null) {
                throw new ArgumentNullException(nameof(asset));
            }
            asset.AuthoringAssetId = authoringAssetId;
            asset.FormerAuthoringAssetIds ??= Array.Empty<string>();
            WriteNativeAsset(relativePath, asset);
        }

        /// <inheritdoc />
        public void WriteNativeScene(
            string relativePath,
            SceneSettingsAsset sceneSettings,
            Entity[] roots,
            ComponentPersistenceRegistry persistenceRegistry,
            string authoringAssetId) {
            throw new NotSupportedException("Scene authoring is not used by this detached asset test capability.");
        }

        /// <inheritdoc />
        public bool CanonicalizeAssetReferences(Component component, EntityComponentSaveState saveState) {
            throw new NotSupportedException("Reference canonicalization is not used by this detached asset test capability.");
        }

        /// <inheritdoc />
        public void WriteNativeBlueprint(string relativePath, ComponentPersistenceRegistry persistenceRegistry) {
            throw new NotSupportedException("Blueprint editor-scene authoring is not used by this test capability.");
        }

        /// <inheritdoc />
        public void WriteNativeBlueprint(string relativePath, ComponentPersistenceRegistry persistenceRegistry, string authoringAssetId) {
            throw new NotSupportedException("Blueprint editor-scene authoring is not used by this test capability.");
        }

        /// <inheritdoc />
        public void WriteGeneratedCacheAsset(string relativePath, Asset asset) {
            throw new NotSupportedException("Generated cache authoring is not used by this test capability.");
        }

        /// <inheritdoc />
        public void WriteNativeMaterial(string relativePath, GeneratedMaterialAssetDefinition definition) {
            ValidateAssetPath(relativePath);
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

            new global::helengine.editor.GeneratedMaterialAssetWriteService().WriteMaterial(
                ProjectRootPath,
                relativePath,
                editorDefinition);
        }

        /// <inheritdoc />
        public void WriteNativeMaterial(string relativePath, GeneratedMaterialAssetDefinition definition, string authoringAssetId) {
            if (definition == null || definition.MaterialAsset == null) {
                throw new ArgumentException("Material definition must include an asset.", nameof(definition));
            }
            definition.MaterialAsset.AuthoringAssetId = authoringAssetId;
            WriteNativeMaterial(relativePath, definition);
        }

        /// <inheritdoc />
        public SceneAssetReference CreateFileReference(string relativePath, AssetEntryKind expectedKind) {
            ValidateAssetPath(relativePath);
            string fullPath = ResolveAssetPath(relativePath);
            if (!File.Exists(fullPath)) {
                throw new FileNotFoundException("The test capability cannot reference an asset that was not written.", fullPath);
            }

            string assetId = new AssetIdentityMetadataService().Load(fullPath).AssetId;
            string contentHash = "sha256:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(fullPath))).ToLowerInvariant();
            return global::helengine.SceneAssetReferenceFactory.CreateFileSystemReference(
                assetId,
                relativePath.Replace('\\', '/'),
                contentHash);
        }

        /// <inheritdoc />
        public TAsset LoadNativeAsset<TAsset>(string relativePath) where TAsset : Asset {
            ValidateAssetPath(relativePath);
            string fullPath = ResolveAssetPath(relativePath);
            using FileStream stream = File.OpenRead(fullPath);
            if (global::helengine.editor.AssetSerializer.Deserialize(stream) is not TAsset asset) {
                throw new InvalidOperationException($"Native asset '{relativePath}' is not a {typeof(TAsset).Name}.");
            }

            return asset;
        }

        /// <inheritdoc />
        public bool TryLoadImportedTextureAsset(string assetId, out TextureAsset textureAsset) {
            textureAsset = null;
            return false;
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSupportedPlatformIds() => Array.Empty<string>();

        /// <summary>
        /// Validates one project-relative native asset path.
        /// </summary>
        /// <param name="relativePath">Path relative to the project assets directory.</param>
        static void ValidateAssetPath(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Asset relative path must be provided.", nameof(relativePath));
            } else if (Path.IsPathRooted(relativePath)) {
                throw new ArgumentException("Asset relative path must not be rooted.", nameof(relativePath));
            }
        }

        /// <summary>
        /// Resolves one validated assets-relative path beneath this test project.
        /// </summary>
        /// <param name="relativePath">Path relative to the project assets directory.</param>
        /// <returns>Absolute native asset path.</returns>
        string ResolveAssetPath(string relativePath) {
            ValidateAssetPath(relativePath);
            return Path.Combine(ProjectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
