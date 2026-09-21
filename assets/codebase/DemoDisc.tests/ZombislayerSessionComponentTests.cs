namespace DemoDisc.tests {
    /// <summary>
    /// Verifies the Zombislayer session controller exposes deterministic pause-state helpers for the gameplay scene.
    /// </summary>
    public sealed class ZombislayerSessionComponentTests {
        /// <summary>
        /// Ensures the runtime session state machine can transition between playing and paused states.
        /// </summary>
        [Fact]
        public void Build_state_machine_transitions_between_playing_and_paused() {
            helengine.FiniteStateMachine<DemoDisc.game.ZombislayerSessionState> machine = DemoDisc.game.ZombislayerSessionComponent.CreateStateMachine();

            machine.Initialize(DemoDisc.game.ZombislayerSessionState.Playing);
            bool paused = machine.TryChangeState(DemoDisc.game.ZombislayerSessionState.Paused);
            bool resumed = machine.TryChangeState(DemoDisc.game.ZombislayerSessionState.Playing);

            Assert.True(paused);
            Assert.True(resumed);
            Assert.Equal(DemoDisc.game.ZombislayerSessionState.Playing, machine.CurrentState);
        }

        /// <summary>
        /// Ensures the pause toggle helper flips the state from playing to paused and back again.
        /// </summary>
        [Fact]
        public void Resolve_state_after_pause_toggle_flips_between_playing_and_paused() {
            DemoDisc.game.ZombislayerSessionState paused = DemoDisc.game.ZombislayerSessionComponent.ResolveStateAfterPauseToggle(DemoDisc.game.ZombislayerSessionState.Playing);
            DemoDisc.game.ZombislayerSessionState resumed = DemoDisc.game.ZombislayerSessionComponent.ResolveStateAfterPauseToggle(DemoDisc.game.ZombislayerSessionState.Paused);

            Assert.Equal(DemoDisc.game.ZombislayerSessionState.Paused, paused);
            Assert.Equal(DemoDisc.game.ZombislayerSessionState.Playing, resumed);
        }

        /// <summary>
        /// Ensures the pause overlay is shown only while the session is paused.
        /// </summary>
        [Fact]
        public void Should_show_pause_overlay_returns_true_only_for_paused_state() {
            Assert.False(DemoDisc.game.ZombislayerSessionComponent.ShouldShowPauseOverlay(DemoDisc.game.ZombislayerSessionState.Playing));
            Assert.True(DemoDisc.game.ZombislayerSessionComponent.ShouldShowPauseOverlay(DemoDisc.game.ZombislayerSessionState.Paused));
        }
    }
}
