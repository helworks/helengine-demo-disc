using helengine;
using helengine.editor;
using System.Security.Cryptography;

namespace city.rendering.tools {
    /// <summary>
    /// Routes generated source, import-settings, and cache bytes through the
    /// caller-owned editor transaction.
    /// </summary>
    public static class GeneratedFileTransactionWriter {
        const string DefaultTextureImporterId = "gdi";

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
            } else if (sourceBytes == null || sourceBytes.Length == 0) {
                throw new ArgumentException("Texture source bytes must be provided.", nameof(sourceBytes));
            } else if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }

            string normalizedAssetsPath = NormalizeRelativePath(assetsRelativePath);
            string projectSourcePath = "assets/" + normalizedAssetsPath;
            authoringSession.WriteGeneratedFile(
                projectSourcePath,
                sourceBytes,
                transaction.GetCurrentFileHash(projectSourcePath),
                EditorGeneratedFileKind.Source,
                transaction);

            settings.Importer ??= new AssetImporterSettings();
            settings.Importer.ImporterId = string.IsNullOrWhiteSpace(settings.Importer.ImporterId)
                ? DefaultTextureImporterId
                : settings.Importer.ImporterId;
            settings.Importer.SourceChecksum = ComputeSourceChecksum(sourceBytes);
            if (string.IsNullOrWhiteSpace(settings.Importer.AssetId)) {
                settings.Importer.AssetId = BuildImporterQualifiedAssetId(
                    settings.Importer.SourceChecksum,
                    settings.Importer.ImporterId);
            }

            using MemoryStream settingsBytes = new MemoryStream();
            TextureAssetImportSettingsBinarySerializer.Serialize(settingsBytes, settings);
            WriteImportSettingsBytes(authoringSession, transaction, projectSourcePath, settingsBytes.ToArray());
            return settings.Importer.AssetId;
        }

        /// <summary>
        /// Stages one import-settings sidecar for an existing or generated source.
        /// </summary>
        public static void WriteTextureImportSettings(
            IEditorProjectAuthoringSession authoringSession,
            EditorAuthoringTransaction transaction,
            string assetsRelativePath,
            TextureAssetImportSettings settings) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }
            using MemoryStream settingsBytes = new MemoryStream();
            TextureAssetImportSettingsBinarySerializer.Serialize(settingsBytes, settings);
            WriteImportSettingsBytes(
                authoringSession,
                transaction,
                "assets/" + NormalizeRelativePath(assetsRelativePath),
                settingsBytes.ToArray());
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

        static void WriteImportSettingsBytes(
            IEditorProjectAuthoringSession authoringSession,
            EditorAuthoringTransaction transaction,
            string projectSourcePath,
            byte[] bytes) {
            const string importSettingsExtension = ".hasset";
            string projectSettingsPath = projectSourcePath + importSettingsExtension;
            authoringSession.WriteGeneratedFile(
                projectSettingsPath,
                bytes,
                transaction.GetCurrentFileHash(projectSettingsPath),
                EditorGeneratedFileKind.ImportSettings,
                transaction);
        }

        static string ComputeSourceChecksum(byte[] sourceBytes) {
            return Convert.ToHexString(SHA256.HashData(sourceBytes)).ToLowerInvariant();
        }

        static string BuildImporterQualifiedAssetId(string sourceChecksum, string importerId) {
            byte[] identityBytes = System.Text.Encoding.UTF8.GetBytes("importer\n" + sourceChecksum + "\n" + importerId);
            return Convert.ToHexString(SHA256.HashData(identityBytes)).ToLowerInvariant();
        }
    }
}
