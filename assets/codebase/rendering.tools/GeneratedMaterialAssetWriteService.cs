using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes generated material assets plus their per-platform settings sidecars using the current editor material-settings pipeline.
    /// </summary>
    public sealed class GeneratedMaterialAssetWriteService {
        /// <summary>
        /// Stable importer identifier used for generated material settings sidecars.
        /// </summary>
        const string MaterialImporterId = "helengine.material";

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

            MaterialAssetImportSettings settings = BuildImportSettings(definition);
            SettingsService.Save(fullMaterialPath, settings);
        }

        /// <summary>
        /// Converts one generated material definition into the shared material-settings import document shape.
        /// </summary>
        /// <param name="definition">Generated material definition to translate.</param>
        /// <returns>Shared material-settings import document.</returns>
        MaterialAssetImportSettings BuildImportSettings(GeneratedMaterialAssetDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (definition.MaterialAsset == null) {
                throw new InvalidOperationException("Generated material definitions must provide a material asset.");
            }

            MaterialAssetImportSettings settings = new MaterialAssetImportSettings();
            settings.Importer.ImporterId = MaterialImporterId;
            settings.Importer.SourceChecksum = string.Empty;
            settings.Importer.AssetId = definition.MaterialAsset.Id ?? string.Empty;

            foreach (KeyValuePair<string, GeneratedMaterialPlatformDefinition> entry in definition.Platforms) {
                settings.Processor.Platforms[entry.Key] = BuildPlatformSettings(entry.Key, entry.Value);
            }

            return settings;
        }

        /// <summary>
        /// Converts one generated per-platform material definition into the shared processor-settings payload.
        /// </summary>
        /// <param name="platformId">Platform id that owns the generated material schema values.</param>
        /// <param name="definition">Generated per-platform material definition to translate.</param>
        /// <returns>Shared material processor settings payload.</returns>
        MaterialAssetProcessorSettings BuildPlatformSettings(string platformId, GeneratedMaterialPlatformDefinition definition) {
            if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            } else if (definition == null) {
                throw new InvalidOperationException($"Generated material platform '{platformId}' is missing its definition.");
            } else if (string.IsNullOrWhiteSpace(definition.SchemaId)) {
                throw new InvalidOperationException($"Generated material platform '{platformId}' must specify a schema id.");
            }

            MaterialAssetProcessorSettings settings = new MaterialAssetProcessorSettings();
            settings.SchemaId = definition.SchemaId;
            foreach (KeyValuePair<string, string> fieldEntry in definition.FieldValues) {
                settings.FieldValues[fieldEntry.Key] = fieldEntry.Value ?? string.Empty;
            }

            return settings;
        }
    }
}
