using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Shares the DemoDisc's authored standard-material coefficients and imported texture references with every platform
    /// of the <c>mobile</c> group. Material settings have no group scope: each platform reads its own override document
    /// beside the material, so this service writes one per mobile platform, copying the desktop reference platform's
    /// values. The generator commands that write materials run it after their materials are published.
    /// </summary>
    public sealed class MobileMaterialAuthoringService {
        /// <summary>
        /// Desktop platform whose resolved standard-material values the mobile renderer shares.
        /// </summary>
        public const string DesktopReferencePlatformId = "windows";

        /// <summary>
        /// Standard fields whose semantics are shared by the desktop authoring preview and the mobile Vulkan renderer.
        /// </summary>
        static readonly string[] SharedFields = ["base-color", "texture-id", "roughness-texture-id", "metallic", "roughness", "specular", "alpha-mode", "double-sided"];

        /// <summary>
        /// Copies only compatible material values into each mobile platform's override document through the public
        /// settings authoring API, retaining every other platform's settings.
        /// </summary>
        /// <param name="authoring">Project-scoped session whose published materials are read and whose overrides are written.</param>
        public void Configure(IEditorProjectAuthoringSession authoring) {
            if (authoring == null) {
                throw new ArgumentNullException(nameof(authoring));
            }

            IReadOnlyList<string> mobilePlatformIds = ResolveMobilePlatformIds(authoring.ProjectRootPath);
            string root = Path.Combine(authoring.ProjectRootPath, "assets", "materials");
            using EditorAuthoringTransaction transaction = authoring.BeginTransaction();
            foreach (string path in Directory.EnumerateFiles(root, "*.hasset", SearchOption.AllDirectories)) {
                if (Path.GetFileName(path).Contains(".hasset.", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                string relative = Path.GetRelativePath(Path.Combine(authoring.ProjectRootPath, "assets"), path).Replace('\\', '/');
                MaterialAssetProcessorSettings desktop = authoring.LoadMaterialPlatformSettings(relative, DesktopReferencePlatformId, transaction);
                if (desktop.SchemaId != "standard-shader") {
                    continue;
                }
                for (int index = 0; index < mobilePlatformIds.Count; index++) {
                    WritePlatformOverride(authoring, transaction, path, relative, desktop, mobilePlatformIds[index]);
                }
            }
            transaction.Commit();
        }

        /// <summary>
        /// Writes one mobile platform's override document for one standard material.
        /// </summary>
        /// <param name="authoring">Project-scoped session used to stage the generated document.</param>
        /// <param name="transaction">Transaction that publishes every mobile material override together.</param>
        /// <param name="path">Absolute path of the material's common settings document.</param>
        /// <param name="relative">Assets-relative material path.</param>
        /// <param name="desktop">Resolved desktop reference settings whose shared fields are copied.</param>
        /// <param name="platformId">Mobile platform whose override document is written.</param>
        static void WritePlatformOverride(IEditorProjectAuthoringSession authoring, EditorAuthoringTransaction transaction, string path, string relative, MaterialAssetProcessorSettings desktop, string platformId) {
            string overridePath = path + "." + platformId + ".hasset";
            MaterialAssetPlatformOverrideDocument mobile;
            if (File.Exists(overridePath)) {
                using FileStream existing = File.OpenRead(overridePath);
                mobile = MaterialAssetPlatformOverrideDocumentBinarySerializer.Deserialize(existing);
            } else {
                mobile = new MaterialAssetPlatformOverrideDocument { PlatformId = platformId };
            }
            mobile.Processor.HasSchemaIdOverride = true;
            mobile.Processor.SchemaId = "standard-shader";
            foreach (string field in SharedFields) {
                if (desktop.FieldValues.TryGetValue(field, out string value)) {
                    mobile.Processor.FieldValues[field] = value;
                }
                if (desktop.AssetReferenceValues.TryGetValue(field, out SceneAssetReference reference)) {
                    mobile.Processor.AssetReferenceValues[field] = reference;
                }
            }
            using MemoryStream output = new MemoryStream();
            MaterialAssetPlatformOverrideDocumentBinarySerializer.Serialize(output, mobile);
            string priorHash = File.Exists(overridePath) ? "sha256:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(overridePath))).ToLowerInvariant() : string.Empty;
            authoring.WriteGeneratedFile("assets/" + relative + "." + platformId + ".hasset", output.ToArray(), priorHash, EditorGeneratedFileKind.ImportSettings, transaction);
        }

        /// <summary>
        /// Reads the platforms of the mobile group from the project's platform group tree.
        /// </summary>
        /// <param name="projectRootPath">Project root whose <c>settings/platform-groups.json</c> is read.</param>
        /// <returns>Platform ids of the mobile group.</returns>
        static IReadOnlyList<string> ResolveMobilePlatformIds(string projectRootPath) {
            EditorProjectPlatformGroupsDocument groups = new EditorProjectPlatformGroupsService(projectRootPath).Read();
            EditorProjectPlatformGroupDefinition mobile = EditorProjectPlatformGroupsService.FindGroup(groups, DemoDiscOverrideScopes.MobileGroupId)
                ?? throw new InvalidOperationException("The project's platform groups must define the " + DemoDiscOverrideScopes.MobileGroupId + " group.");
            return mobile.PlatformIds;
        }
    }
}
