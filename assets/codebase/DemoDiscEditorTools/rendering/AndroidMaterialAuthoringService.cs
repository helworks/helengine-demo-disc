using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>Shares the DemoDisc's authored standard-material coefficients and imported texture references with its Android renderer.</summary>
    public sealed class AndroidMaterialAuthoringService {
        /// <summary>Standard fields whose semantics are shared by the Windows authoring preview and Android Vulkan.</summary>
        static readonly string[] SharedFields = ["base-color", "texture-id", "roughness-texture-id", "metallic", "roughness", "specular", "alpha-mode", "double-sided"];
        /// <summary>Copies only compatible material values into the Android scope through the public settings authoring API.</summary>
        /// <param name="authoring">Project-scoped session that retains every other platform's settings.</param>
        public void Configure(IEditorProjectAuthoringSession authoring) {
            string root = Path.Combine(authoring.ProjectRootPath, "assets", "materials");
            using EditorAuthoringTransaction transaction = authoring.BeginTransaction();
            foreach (string path in Directory.EnumerateFiles(root, "*.hasset", SearchOption.AllDirectories)) {
                if (Path.GetFileName(path).Contains(".hasset.", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                string relative = Path.GetRelativePath(Path.Combine(authoring.ProjectRootPath, "assets"), path).Replace('\\', '/');
                MaterialAssetProcessorSettings windows = authoring.LoadMaterialPlatformSettings(relative, "windows", transaction);
                if (windows.SchemaId != "standard-shader") {
                    continue;
                }
                string overridePath = path + ".android.hasset";
                MaterialAssetPlatformOverrideDocument android;
                if (File.Exists(overridePath)) {
                    using FileStream existing = File.OpenRead(overridePath);
                    android = MaterialAssetPlatformOverrideDocumentBinarySerializer.Deserialize(existing);
                } else {
                    android = new MaterialAssetPlatformOverrideDocument { PlatformId = "android" };
                }
                android.Processor.HasSchemaIdOverride = true;
                android.Processor.SchemaId = "standard-shader";
                foreach (string field in SharedFields) {
                    if (windows.FieldValues.TryGetValue(field, out string value)) {
                        android.Processor.FieldValues[field] = value;
                    }
                    if (windows.AssetReferenceValues.TryGetValue(field, out SceneAssetReference reference)) {
                        android.Processor.AssetReferenceValues[field] = reference;
                    }
                }
                using MemoryStream output = new MemoryStream();
                MaterialAssetPlatformOverrideDocumentBinarySerializer.Serialize(output, android);
                string priorHash = File.Exists(overridePath) ? "sha256:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(overridePath))).ToLowerInvariant() : string.Empty;
                authoring.WriteGeneratedFile("assets/" + relative + ".android.hasset", output.ToArray(), priorHash, EditorGeneratedFileKind.ImportSettings, transaction);
            }
            transaction.Commit();
        }
    }
}
