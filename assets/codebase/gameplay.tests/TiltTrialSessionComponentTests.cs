namespace city.tests {
    /// <summary>
    /// Verifies the Tilt Trial session controller drives timeout and completion flow deterministically.
    /// </summary>
    public sealed class TiltTrialSessionComponentTests {
        [Fact]
        public void Resolve_medal_returns_gold_for_fastest_clear() {
            city.game.TiltTrialLevelSettingsComponent settings = new city.game.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = city.game.TiltTrialSceneIds.Level01SceneId,
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 20f,
                SilverTimeSeconds = 35f,
                BronzeTimeSeconds = 50f
            };

            city.game.TiltTrialMedal medal = city.game.TiltTrialSessionComponent.ResolveMedal(settings, 19.5f);
            Assert.Equal(city.game.TiltTrialMedal.Gold, medal);
        }

        [Fact]
        public void Resolve_next_scene_id_returns_level_select_when_current_level_is_last() {
            string nextSceneId = city.game.TiltTrialSessionComponent.ResolveNextSceneId(
                "tilt-trial-05",
                city.game.TiltTrialSceneIds.LevelSelectSceneId);

            Assert.Equal(city.game.TiltTrialSceneIds.LevelSelectSceneId, nextSceneId);
        }

        [Fact]
        public void Build_state_machine_transitions_from_playing_to_failed_when_timeout_occurs() {
            helengine.FiniteStateMachine<city.game.TiltTrialSessionState> machine = city.game.TiltTrialSessionComponent.CreateStateMachine();

            machine.Initialize(city.game.TiltTrialSessionState.Playing);
            bool changed = machine.TryChangeState(city.game.TiltTrialSessionState.Failed);

            Assert.True(changed);
            Assert.Equal(city.game.TiltTrialSessionState.Failed, machine.CurrentState);
        }

        [Fact]
        public void Format_coin_progress_returns_expected_hud_label() {
            string label = city.game.TiltTrialSessionComponent.FormatCoinProgress(3, 7);

            Assert.Equal("Coins 3/7", label);
        }
    }
}
