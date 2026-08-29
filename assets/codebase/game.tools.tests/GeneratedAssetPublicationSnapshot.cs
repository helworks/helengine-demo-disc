using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Captures the published generated assets and their timestamps so a
    /// repeated authoring command can prove byte-level and path-level no-op
    /// behavior.
    /// </summary>
    sealed class GeneratedAssetPublicationSnapshot {
        readonly string ProjectRootPath;
        readonly Dictionary<string, byte[]> BytesByPath;
        readonly Dictionary<string, DateTime> LastWriteTimesUtcByPath;

        GeneratedAssetPublicationSnapshot(
            string projectRootPath,
            Dictionary<string, byte[]> bytesByPath,
            Dictionary<string, DateTime> lastWriteTimesUtcByPath) {
            ProjectRootPath = projectRootPath;
            BytesByPath = bytesByPath;
            LastWriteTimesUtcByPath = lastWriteTimesUtcByPath;
        }

        public IReadOnlyCollection<string> RelativePaths => BytesByPath.Keys;

        public static GeneratedAssetPublicationSnapshot Capture(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string root = Path.GetFullPath(projectRootPath);
            string assetsRoot = Path.Combine(root, "assets");
            Dictionary<string, byte[]> bytesByPath = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            Dictionary<string, DateTime> timestampsByPath = new Dictionary<string, DateTime>(StringComparer.Ordinal);
            if (Directory.Exists(assetsRoot)) {
                foreach (string fullPath in Directory.EnumerateFiles(assetsRoot, "*", SearchOption.AllDirectories)) {
                    string relativePath = Path.GetRelativePath(assetsRoot, fullPath).Replace('\\', '/');
                    bytesByPath.Add(relativePath, File.ReadAllBytes(fullPath));
                    timestampsByPath.Add(relativePath, File.GetLastWriteTimeUtc(fullPath));
                }
            }

            return new GeneratedAssetPublicationSnapshot(root, bytesByPath, timestampsByPath);
        }

        public void AssertExactPaths(IEnumerable<string> expectedRelativePaths) {
            string[] expected = expectedRelativePaths
                .Select(path => path.Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            string[] actual = RelativePaths
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(expected, actual);
        }

        public void AssertUnchanged() {
            GeneratedAssetPublicationSnapshot current = Capture(ProjectRootPath);
            Assert.Equal(
                RelativePaths.OrderBy(path => path, StringComparer.Ordinal),
                current.RelativePaths.OrderBy(path => path, StringComparer.Ordinal));
            foreach (string relativePath in RelativePaths) {
                Assert.Equal(BytesByPath[relativePath], current.BytesByPath[relativePath]);
                Assert.Equal(LastWriteTimesUtcByPath[relativePath], current.LastWriteTimesUtcByPath[relativePath]);
            }
        }
    }

    /// <summary>Captures public identity/reference data for generated assets.</summary>
    sealed class GeneratedAssetReferenceSnapshot {
        readonly Dictionary<string, ReferenceValue> ValuesByPath;

        GeneratedAssetReferenceSnapshot(Dictionary<string, ReferenceValue> valuesByPath) {
            ValuesByPath = valuesByPath;
        }

        public static GeneratedAssetReferenceSnapshot Capture(
            IEditorProjectAuthoringSession authoringSession,
            IReadOnlyDictionary<string, AssetEntryKind> assetKindsByPath) {
            if (authoringSession == null) {
                throw new ArgumentNullException(nameof(authoringSession));
            } else if (assetKindsByPath == null) {
                throw new ArgumentNullException(nameof(assetKindsByPath));
            }

            Dictionary<string, ReferenceValue> valuesByPath = new Dictionary<string, ReferenceValue>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, AssetEntryKind> entry in assetKindsByPath) {
                SceneAssetReference reference = authoringSession.CreateFileReference(entry.Key, entry.Value);
                valuesByPath.Add(entry.Key, new ReferenceValue(reference));
            }
            return new GeneratedAssetReferenceSnapshot(valuesByPath);
        }

        public void AssertUnchanged(
            IEditorProjectAuthoringSession authoringSession,
            IReadOnlyDictionary<string, AssetEntryKind> assetKindsByPath) {
            GeneratedAssetReferenceSnapshot current = Capture(authoringSession, assetKindsByPath);
            Assert.Equal(ValuesByPath.Keys.OrderBy(path => path, StringComparer.Ordinal), current.ValuesByPath.Keys.OrderBy(path => path, StringComparer.Ordinal));
            foreach (string path in ValuesByPath.Keys) {
                Assert.Equal(ValuesByPath[path].AssetId, current.ValuesByPath[path].AssetId);
                Assert.Equal(ValuesByPath[path].ContentHash, current.ValuesByPath[path].ContentHash);
                Assert.Equal(ValuesByPath[path].RelativePath, current.ValuesByPath[path].RelativePath);
                Assert.Equal(ValuesByPath[path].ProviderId, current.ValuesByPath[path].ProviderId);
                Assert.Equal(ValuesByPath[path].SourceKind, current.ValuesByPath[path].SourceKind);
            }
        }

        readonly struct ReferenceValue {
            public ReferenceValue(SceneAssetReference reference) {
                AssetId = reference.AssetId;
                ContentHash = reference.ContentHash;
                RelativePath = reference.RelativePath;
                ProviderId = reference.ProviderId;
                SourceKind = reference.SourceKind.ToString();
            }

            public string AssetId { get; }
            public string ContentHash { get; }
            public string RelativePath { get; }
            public string ProviderId { get; }
            public string SourceKind { get; }
        }
    }
}
