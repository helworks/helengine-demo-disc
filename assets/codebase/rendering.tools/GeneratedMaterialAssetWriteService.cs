using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes generated material assets plus their per-platform settings sidecars using the current editor material-settings pipeline.
    /// </summary>
    public sealed class GeneratedMaterialAssetWriteService {
        /// <summary>
        /// Shared settings service used to seed and persist material sidecars.
        /// </summary>
        readonly MaterialAssetSettingsService SettingsService;

        /// <summary>
        /// Initializes one generated material write service.
        /// </summary>
        public GeneratedMaterialAssetWriteService() {
            SettingsService = new MaterialAssetSettingsService();
        }

        /// <summary>
        /// Writes one generated material asset under the project assets folder and persists its per-platform sidecar settings.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path.</param>
        /// <param name="relativePath">Project-relative material asset path.</param>
        /// <param name="definition">Generated material definition to write.</param>
        public void WriteMaterial(string projectRootPath, string relativePath, GeneratedMaterialAssetDefinition definition) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative material path must be provided.", nameof(relativePath));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (definition.MaterialAsset == null) {
                throw new InvalidOperationException("Generated material definitions must provide a material asset.");
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string fullMaterialPath = Path.GetFullPath(Path.Combine(
                fullProjectRootPath,
                "assets",
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            string directoryPath = Path.GetDirectoryName(fullMaterialPath);
            if (!string.IsNullOrWhiteSpace(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            using (FileStream stream = new FileStream(fullMaterialPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                global::helengine.editor.AssetSerializer.Serialize(stream, definition.MaterialAsset);
            }

            EditorProjectBootstrapContext bootstrap = EditorProjectBootstrapper.Create(fullProjectRootPath);
            MaterialAssetImportSettings settings = SettingsService.LoadOrCreate(
                fullMaterialPath,
                definition.MaterialAsset,
                bootstrap.SupportedPlatforms,
                bootstrap.ResolveSelectionModel);
            ApplyPlatforms(settings, definition);
            SettingsService.Save(fullMaterialPath, settings);
        }

        /// <summary>
        /// Copies one generated material definition's platform settings into the persisted sidecar payload.
        /// </summary>
        /// <param name="settings">Material settings sidecar to update.</param>
        /// <param name="definition">Generated material definition that supplies authored platform values.</param>
        void ApplyPlatforms(MaterialAssetImportSettings settings, GeneratedMaterialAssetDefinition definition) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            } else if (settings.Processor == null) {
                throw new InvalidOperationException("Material settings must include processor settings.");
            } else if (settings.Processor.Platforms == null) {
                throw new InvalidOperationException("Material settings must include processor platform settings.");
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            foreach (KeyValuePair<string, GeneratedMaterialPlatformDefinition> entry in definition.Platforms) {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null) {
                    continue;
                }

                if (!settings.Processor.Platforms.TryGetValue(entry.Key, out MaterialAssetProcessorSettings platformSettings) || platformSettings == null) {
                    platformSettings = new MaterialAssetProcessorSettings();
                    settings.Processor.Platforms[entry.Key] = platformSettings;
                }
                if (platformSettings.FieldValues == null) {
                    platformSettings.FieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                platformSettings.SchemaId = entry.Value.SchemaId ?? string.Empty;
                platformSettings.FieldValues.Clear();
                foreach (KeyValuePair<string, string> fieldEntry in entry.Value.FieldValues) {
                    if (string.IsNullOrWhiteSpace(fieldEntry.Key)) {
                        continue;
                    }

                    platformSettings.FieldValues[fieldEntry.Key] = fieldEntry.Value ?? string.Empty;
                }
            }
        }
    }
}
