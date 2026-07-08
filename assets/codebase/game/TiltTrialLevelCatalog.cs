namespace city.game {
    /// <summary>
    /// Exposes the canonical Tilt Trial level order used by selector and Next progression.
    /// </summary>
    public static class TiltTrialLevelCatalog {
        /// <summary>
        /// Creates the current ordered Tilt Trial level set.
        /// </summary>
        /// <returns>Ordered selector/gameplay level entries.</returns>
        public static IReadOnlyList<TiltTrialLevelCatalogEntry> CreateEntries() {
            return [
                new TiltTrialLevelCatalogEntry("tilt-trial-01", "Level 1", TiltTrialSceneIds.Level01SceneId, 99f, 18f, 28f, 40f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-02", "Level 2", TiltTrialSceneIds.Level02SceneId, 99f, 20f, 31f, 44f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-03", "Level 3", TiltTrialSceneIds.Level03SceneId, 99f, 23f, 35f, 48f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-04", "Level 4", TiltTrialSceneIds.Level04SceneId, 99f, 25f, 38f, 52f, string.Empty),
                new TiltTrialLevelCatalogEntry("tilt-trial-05", "Level 5", TiltTrialSceneIds.Level05SceneId, 99f, 27f, 41f, 56f, string.Empty),
            ];
        }
    }
}
