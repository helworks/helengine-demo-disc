namespace city.tests {
    /// <summary>
    /// Verifies the shared Tilt Trial level catalog stays complete and deterministic.
    /// </summary>
    public sealed class TiltTrialLevelCatalogTests {
        /// <summary>
        /// Ensures the selector/session catalog exposes exactly five ordered level entries.
        /// </summary>
        [Fact]
        public void Catalog_returns_exactly_five_ordered_levels() {
            IReadOnlyList<city.game.TiltTrialLevelCatalogEntry> entries = city.game.TiltTrialLevelCatalog.CreateEntries();

            Assert.Equal(5, entries.Count);
            Assert.Equal("tilt-trial-01", entries[0].LevelId);
            Assert.Equal("Level 1", entries[0].DisplayName);
            Assert.Equal(city.game.TiltTrialSceneIds.Level01SceneId, entries[0].SceneId);
            Assert.Equal("tilt-trial-05", entries[4].LevelId);
        }

        /// <summary>
        /// Ensures every level entry carries the metadata required by the selector and session controller.
        /// </summary>
        [Fact]
        public void Catalog_entries_expose_scene_name_timer_medals_and_optional_preview() {
            foreach (city.game.TiltTrialLevelCatalogEntry entry in city.game.TiltTrialLevelCatalog.CreateEntries()) {
                Assert.False(string.IsNullOrWhiteSpace(entry.LevelId));
                Assert.False(string.IsNullOrWhiteSpace(entry.DisplayName));
                Assert.False(string.IsNullOrWhiteSpace(entry.SceneId));
                Assert.True(entry.StartTimeSeconds > 0f);
                Assert.True(entry.GoldTimeSeconds > 0f);
                Assert.True(entry.SilverTimeSeconds >= entry.GoldTimeSeconds);
                Assert.True(entry.BronzeTimeSeconds >= entry.SilverTimeSeconds);
            }
        }
    }
}
