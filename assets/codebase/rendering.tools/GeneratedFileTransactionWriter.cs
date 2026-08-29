using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Routes generated source, import-settings, and cache bytes through the
    /// caller-owned editor transaction.
    /// </summary>
    public static class GeneratedFileTransactionWriter {
        /// <summary>
        /// Stages one generated source texture and its import-settings sidecar.
        /// </summary>
        public static string WriteTexture(
            IEditorProjectAuthoringSession authoringSession,
            EditorAuthoringTransaction transaction,
            string assetsRelativePath,
            byte[] sourceBytes,
            TextureAssetImportSettings settings) {
            if (authoringSession == null) {
                throw new ArgumentNullException(nameof(authoringSession));
            } else if (transaction == null) {
                throw new ArgumentNullException(nameof(transaction));
            } else if (string.IsNullOrWhiteSpace(assetsRelativePath)) {
                throw new ArgumentException("Texture path must be provided.", nameof(assetsRelativePath));
            } else if (sourceBytes == null) {
                throw new ArgumentNullException(nameof(sourceBytes));
            } else if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }

            string normalizedAssetsPath = NormalizeRelativePath(assetsRelativePath);
            if (settings.Importer == null || string.IsNullOrWhiteSpace(settings.Importer.ImporterId)) {
                throw new InvalidOperationException($"Generated texture '{normalizedAssetsPath}' requires an explicit registered importer id.");
            }
            TextureAssetImportSettings prepared = authoringSession.WriteGeneratedTexture(
                normalizedAssetsPath,
                sourceBytes,
                settings,
                transaction);
            return prepared.Importer.AssetId;
        }

        /// <summary>
        /// Stages one serialized generated runtime cache asset.
        /// </summary>
        public static void WriteCache(
            IEditorProjectAuthoringSession authoringSession,
            EditorAuthoringTransaction transaction,
            string cacheRelativePath,
            Asset asset) {
            if (authoringSession == null) {
                throw new ArgumentNullException(nameof(authoringSession));
            } else if (transaction == null) {
                throw new ArgumentNullException(nameof(transaction));
            } else if (string.IsNullOrWhiteSpace(cacheRelativePath)) {
                throw new ArgumentException("Cache path must be provided.", nameof(cacheRelativePath));
            } else if (asset == null) {
                throw new ArgumentNullException(nameof(asset));
            }

            authoringSession.WriteGeneratedCacheAsset(NormalizeRelativePath(cacheRelativePath), asset, transaction);
        }

        static string NormalizeRelativePath(string relativePath) {
            string normalized = relativePath.Replace('\\', '/').Trim('/');
            if (normalized.Length == 0 || normalized.Split('/').Any(segment => segment.Length == 0 || segment == "." || segment == "..")) {
                throw new ArgumentException("Generated paths must be normalized and traversal-free.", nameof(relativePath));
            }
            return normalized;
        }

    }
}
