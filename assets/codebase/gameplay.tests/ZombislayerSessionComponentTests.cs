namespace city.tests {
    /// <summary>
    /// Verifies the Zombislayer session controller exposes deterministic pause-state helpers for the gameplay scene.
    /// </summary>
    public sealed class ZombislayerSessionComponentTests {
        /// <summary>
        /// Ensures the runtime session state machine can transition between playing and paused states.
        /// </summary>
        [Fact]
        public void Build_state_machine_transitions_between_playing_and_paused() {
            helengine.FiniteStateMachine<city.game.ZombislayerSessionState> machine = city.game.ZombislayerSessionComponent.CreateStateMachine();

            machine.Initialize(city.game.ZombislayerSessionState.Playing);
            bool paused = machine.TryChangeState(city.game.ZombislayerSessionState.Paused);
            bool resumed = machine.TryChangeState(city.game.ZombislayerSessionState.Playing);

            Assert.True(paused);
            Assert.True(resumed);
            Assert.Equal(city.game.ZombislayerSessionState.Playing, machine.CurrentState);
        }

        /// <summary>
        /// Ensures the pause toggle helper flips the state from playing to paused and back again.
        /// </summary>
        [Fact]
        public void Resolve_state_after_pause_toggle_flips_between_playing_and_paused() {
            city.game.ZombislayerSessionState paused = city.game.ZombislayerSessionComponent.ResolveStateAfterPauseToggle(city.game.ZombislayerSessionState.Playing);
            city.game.ZombislayerSessionState resumed = city.game.ZombislayerSessionComponent.ResolveStateAfterPauseToggle(city.game.ZombislayerSessionState.Paused);

            Assert.Equal(city.game.ZombislayerSessionState.Paused, paused);
            Assert.Equal(city.game.ZombislayerSessionState.Playing, resumed);
        }

        /// <summary>
        /// Ensures the pause overlay is shown only while the session is paused.
        /// </summary>
        [Fact]
        public void Should_show_pause_overlay_returns_true_only_for_paused_state() {
            Assert.False(city.game.ZombislayerSessionComponent.ShouldShowPauseOverlay(city.game.ZombislayerSessionState.Playing));
            Assert.True(city.game.ZombislayerSessionComponent.ShouldShowPauseOverlay(city.game.ZombislayerSessionState.Paused));
        }
    }
}
