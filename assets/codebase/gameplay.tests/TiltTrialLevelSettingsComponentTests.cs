namespace city.tests {
    /// <summary>
    /// Verifies per-level Tilt Trial metadata rejects invalid authoring.
    /// </summary>
    public sealed class TiltTrialLevelSettingsComponentTests {
        [Fact]
        public void Validate_throws_when_scene_id_is_missing() {
            city.game.TiltTrialLevelSettingsComponent component = new city.game.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = string.Empty,
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 20f,
                SilverTimeSeconds = 35f,
                BronzeTimeSeconds = 50f
            };

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => component.Validate());
            Assert.Contains("scene id", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Validate_throws_when_medal_times_are_not_ascending() {
            city.game.TiltTrialLevelSettingsComponent component = new city.game.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = city.game.TiltTrialSceneIds.Level01SceneId,
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 40f,
                SilverTimeSeconds = 30f,
                BronzeTimeSeconds = 20f
            };

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => component.Validate());
            Assert.Contains("medal", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
