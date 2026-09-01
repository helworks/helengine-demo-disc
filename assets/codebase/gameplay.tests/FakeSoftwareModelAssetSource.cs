using helengine;

namespace city.tests {
    /// <summary>
    /// Provides fresh owned raw model assets to software trace scene tests and observes release ordering.
    /// </summary>
    public sealed class FakeSoftwareModelAssetSource : city.rendering.ISoftwareModelAssetSource {
        readonly List<Entry> entries = new List<Entry>();
        readonly List<ModelAsset> loadedAssets = new List<ModelAsset>();
        readonly List<string> loadedRelativePaths = new List<string>();

        /// <summary>
        /// Gets or sets whether the next load should fail before an owned asset is returned.
        /// </summary>
        public bool ThrowAfterLoad { get; set; }

        /// <summary>
        /// Gets the number of registered identities loaded by the source.
        /// </summary>
        public int LoadCount { get; private set; }

        /// <summary>
        /// Gets the number of loaded assets whose owned arrays have been released.
        /// </summary>
        public int DisposedCount {
            get {
                int count = 0;
                for (int index = 0; index < loadedAssets.Count; index++) {
                    if (IsDisposed(loadedAssets[index])) {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the source-relative paths in first-seen load order.
        /// </summary>
        public IReadOnlyList<string> LoadedRelativePaths => loadedRelativePaths;

        /// <summary>
        /// Gets whether each new load observed all prior owned assets released.
        /// </summary>
        public bool LoadObservedAllPreviousAssetsDisposed { get; private set; } = true;

        /// <summary>
        /// Registers one stable reference identity and an asset factory.
        /// </summary>
        /// <param name="reference">Reference identity to register.</param>
        /// <param name="factory">Factory that creates a fresh owned model asset.</param>
        public void Register(SceneAssetReference reference, Func<ModelAsset> factory) {
            if (reference == null) {
                throw new ArgumentNullException(nameof(reference));
            }
            if (factory == null) {
                throw new ArgumentNullException(nameof(factory));
            }

            entries.Add(new Entry(reference, factory));
        }

        /// <summary>
        /// Loads one fresh model asset for the requested stable identity.
        /// </summary>
        /// <param name="reference">Reference identity to resolve.</param>
        /// <returns>A fresh owned model asset.</returns>
        public ModelAsset LoadOwned(SceneAssetReference reference) {
            if (reference == null) {
                throw new ArgumentNullException(nameof(reference));
            }

            for (int index = 0; index < loadedAssets.Count; index++) {
                if (!IsDisposed(loadedAssets[index])) {
                    LoadObservedAllPreviousAssetsDisposed = false;
                }
            }

            Entry entry = FindEntry(reference);
            if (ThrowAfterLoad) {
                throw new InvalidOperationException("Injected fake model source failure before load ownership transfer.");
            }
            ModelAsset asset = entry.Factory();
            if (asset == null) {
                throw new InvalidOperationException("Fake model asset factory returned null.");
            }
            loadedAssets.Add(asset);
            loadedRelativePaths.Add(reference.RelativePath);
            LoadCount++;
            return asset;
        }

        static bool IsDisposed(ModelAsset asset) {
            return asset != null && asset.Positions == null && asset.Normals == null && asset.TexCoords == null && asset.Indices16 == null && asset.Indices32 == null && asset.Submeshes == null;
        }

        Entry FindEntry(SceneAssetReference reference) {
            for (int index = 0; index < entries.Count; index++) {
                Entry entry = entries[index];
                if (entry.Reference.SourceKind == reference.SourceKind &&
                    string.Equals(entry.Reference.ProviderId, reference.ProviderId, StringComparison.Ordinal) &&
                    string.Equals(entry.Reference.AssetId, reference.AssetId, StringComparison.Ordinal) &&
                    string.Equals(entry.Reference.RelativePath, reference.RelativePath, StringComparison.Ordinal)) {
                    return entry;
                }
            }

            throw new InvalidOperationException("Fake model source has no asset for the requested stable reference identity.");
        }

        sealed class Entry {
            public readonly SceneAssetReference Reference;
            public readonly Func<ModelAsset> Factory;

            public Entry(SceneAssetReference reference, Func<ModelAsset> factory) {
                Reference = reference;
                Factory = factory;
            }
        }
    }
}
