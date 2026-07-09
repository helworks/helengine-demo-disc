using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Resolves generated control icons into both source paths and imported texture asset ids.
    /// </summary>
    public sealed class GeneratedControlIconAssetResolver {
        public ResolvedControlIcon RequireIcon(string projectRootPath, string platformId, string controlId) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string familyId = GeneratedControlIconPlatformMap.ResolveFamilyId(platformId);
            GeneratedControlIconCatalog catalog = GeneratedControlIconCatalog.Load(fullProjectRootPath);
            string relativePath = catalog.RequireControlPath(familyId, controlId);

            string fullAssetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string fullSourcePath = Path.Combine(fullAssetsRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullSourcePath)) {
                throw new InvalidOperationException($"Generated control icon source '{relativePath}' was not found for platform '{platformId}' and control '{controlId}'.");
            }

            AssetImportManager importManager = CreateImportManager(fullProjectRootPath);
            TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(fullSourcePath);
            if (settings == null || settings.Importer == null || string.IsNullOrWhiteSpace(settings.Importer.AssetId)) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' did not produce a persisted imported texture asset id.");
            }

            return new ResolvedControlIcon {
                PlatformId = platformId,
                FamilyId = familyId,
                ControlId = controlId,
                SourcePngRelativePath = relativePath,
                ImportedTextureAssetId = settings.Importer.AssetId
            };
        }

        static AssetImportManager CreateImportManager(string fullProjectRootPath) {
            try {
                return GeneratedAuthoringSceneWriteService.CreateGeneratedSceneAssetImportManager(fullProjectRootPath);
            } catch (FileNotFoundException) {
                // Tests do not load the editor app assembly, but committed texture sidecars are enough
                // to recover the imported asset ids that scene authoring persists.
                string fullAssetsRootPath = Path.Combine(fullProjectRootPath, "assets");
                ContentManager assetContentManager = new ContentManager(new HostFileSystemContentStreamSource(fullAssetsRootPath));
                EditorContentManagerConfiguration.ConfigureEditorContentManager(assetContentManager);
                return new AssetImportManager(fullProjectRootPath, assetContentManager);
            }
        }
    }
}
