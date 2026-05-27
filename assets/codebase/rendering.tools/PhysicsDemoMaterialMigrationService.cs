using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Migrates the shared physics demo materials into the per-platform material settings flow required by Nintendo DS cooking.
    /// </summary>
    public sealed class PhysicsDemoMaterialMigrationService {
        /// <summary>
        /// Shared importer identifier used by material settings documents.
        /// </summary>
        const string MaterialImporterId = "helengine.material";

        /// <summary>
        /// Stable Windows schema id used by the standard shader path.
        /// </summary>
        const string WindowsSchemaId = "standard-shader";

        /// <summary>
        /// Stable PS2 schema id used by the existing project physics materials.
        /// </summary>
        const string Ps2SchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable Nintendo DS schema id used by the cooked platform-owned DS material path.
        /// </summary>
        const string NintendoDsSchemaId = "ds-standard-textured";

        /// <summary>
        /// Shared settings writer used to persist one common material document plus per-platform overrides.
        /// </summary>
        readonly MaterialAssetSettingsService MaterialAssetSettingsServiceValue;

        /// <summary>
        /// Initializes one physics demo material migration service.
        /// </summary>
        public PhysicsDemoMaterialMigrationService() {
            MaterialAssetSettingsServiceValue = new MaterialAssetSettingsService();
        }

        /// <summary>
        /// Rewrites every shared physics demo material as a per-platform material settings document.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Migrate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            PhysicsDemoMaterialDefinition[] definitions = CreateDefinitions();
            for (int index = 0; index < definitions.Length; index++) {
                MigrateMaterial(fullProjectRootPath, definitions[index]);
            }
        }

        /// <summary>
        /// Creates the ordered shared physics demo material definitions that should be migrated.
        /// </summary>
        /// <returns>Ordered physics demo material definitions.</returns>
        PhysicsDemoMaterialDefinition[] CreateDefinitions() {
            return [
                new PhysicsDemoMaterialDefinition("PhysicsDemoNeutral", "#C4CCD6FF"),
                new PhysicsDemoMaterialDefinition("PhysicsDemoBlue", "#548FE5FF"),
                new PhysicsDemoMaterialDefinition("PhysicsDemoGreen", "#61C27DFF"),
                new PhysicsDemoMaterialDefinition("PhysicsDemoMagenta", "#D161C0FF"),
                new PhysicsDemoMaterialDefinition("PhysicsDemoYellow", "#E2C75AFF")
            ];
        }

        /// <summary>
        /// Rewrites one shared physics demo material as a per-platform material settings document.
        /// </summary>
        /// <param name="fullProjectRootPath">Absolute city project root path.</param>
        /// <param name="definition">Physics demo material definition being migrated.</param>
        void MigrateMaterial(string fullProjectRootPath, PhysicsDemoMaterialDefinition definition) {
            if (string.IsNullOrWhiteSpace(fullProjectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(fullProjectRootPath));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            string materialAssetPath = Path.Combine(fullProjectRootPath, "assets", "materials", "physics", definition.AssetName + ".hasset");
            MaterialAssetImportSettings settings = BuildImportSettings(definition);
            MaterialAssetSettingsServiceValue.Save(materialAssetPath, settings);
        }

        /// <summary>
        /// Builds one per-platform material settings payload for the supplied physics demo material definition.
        /// </summary>
        /// <param name="definition">Physics demo material definition being migrated.</param>
        /// <returns>Per-platform material settings payload.</returns>
        MaterialAssetImportSettings BuildImportSettings(PhysicsDemoMaterialDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            MaterialAssetImportSettings settings = new MaterialAssetImportSettings();
            settings.Importer.ImporterId = MaterialImporterId;
            settings.Importer.SourceChecksum = string.Empty;
            settings.Importer.AssetId = "Materials.physics." + definition.AssetName;
            settings.Processor.Platforms["windows"] = BuildWindowsSettings(definition);
            settings.Processor.Platforms["ds"] = BuildNintendoDsSettings(definition);
            settings.Processor.Platforms["ps2"] = BuildPs2Settings(definition);
            return settings;
        }

        /// <summary>
        /// Builds the Windows material settings payload used by editor and desktop runtime paths.
        /// </summary>
        /// <param name="definition">Physics demo material definition being migrated.</param>
        /// <returns>Windows material settings payload.</returns>
        MaterialAssetProcessorSettings BuildWindowsSettings(PhysicsDemoMaterialDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            MaterialAssetProcessorSettings settings = new MaterialAssetProcessorSettings();
            settings.SchemaId = WindowsSchemaId;
            settings.FieldValues["use-custom-shader"] = "false";
            settings.FieldValues["shader-asset-id"] = "ForwardStandardShader";
            settings.FieldValues["casts-shadow"] = "false";
            settings.FieldValues["receives-shadow"] = "true";
            settings.FieldValues["base-color"] = definition.BaseColor;
            return settings;
        }

        /// <summary>
        /// Builds the Nintendo DS material settings payload consumed by the DS cooked platform-owned material path.
        /// </summary>
        /// <param name="definition">Physics demo material definition being migrated.</param>
        /// <returns>Nintendo DS material settings payload.</returns>
        MaterialAssetProcessorSettings BuildNintendoDsSettings(PhysicsDemoMaterialDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            MaterialAssetProcessorSettings settings = new MaterialAssetProcessorSettings();
            settings.SchemaId = NintendoDsSchemaId;
            settings.FieldValues["texture-relative-path"] = string.Empty;
            settings.FieldValues["double-sided"] = "false";
            settings.FieldValues["vertex-color-mode"] = "multiply";
            settings.FieldValues["base-color"] = definition.BaseColor;
            settings.FieldValues["lighting-mode"] = "lit";
            return settings;
        }

        /// <summary>
        /// Builds the PlayStation 2 material settings payload that preserves the current project-authored physics material behavior.
        /// </summary>
        /// <param name="definition">Physics demo material definition being migrated.</param>
        /// <returns>PlayStation 2 material settings payload.</returns>
        MaterialAssetProcessorSettings BuildPs2Settings(PhysicsDemoMaterialDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            MaterialAssetProcessorSettings settings = new MaterialAssetProcessorSettings();
            settings.SchemaId = Ps2SchemaId;
            settings.FieldValues["texture-relative-path"] = string.Empty;
            settings.FieldValues["alpha-mode"] = "opaque";
            settings.FieldValues["double-sided"] = "false";
            settings.FieldValues["cast-shadows"] = "false";
            settings.FieldValues["vertex-color-mode"] = "multiply";
            settings.FieldValues["base-color"] = definition.BaseColor;
            return settings;
        }
    }
}
