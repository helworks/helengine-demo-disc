namespace DemoDisc.Zombislayer.tests {
    /// <summary>
    /// Verifies the Zombislayer session controller exposes deterministic pause-state helpers for the gameplay scene.
    /// </summary>
    public sealed class ZombislayerSessionComponentTests {
        /// <summary>
        /// Ensures the runtime session state machine can transition between playing and paused states.
        /// </summary>
        [Fact]
        public void Build_state_machine_transitions_between_playing_and_paused() {
            helengine.FiniteStateMachine<DemoDisc.Zombislayer.ZombislayerSessionState> machine = DemoDisc.Zombislayer.ZombislayerSessionComponent.CreateStateMachine();

            machine.Initialize(DemoDisc.Zombislayer.ZombislayerSessionState.Playing);
            bool paused = machine.TryChangeState(DemoDisc.Zombislayer.ZombislayerSessionState.Paused);
            bool resumed = machine.TryChangeState(DemoDisc.Zombislayer.ZombislayerSessionState.Playing);

            Assert.True(paused);
            Assert.True(resumed);
            Assert.Equal(DemoDisc.Zombislayer.ZombislayerSessionState.Playing, machine.CurrentState);
        }

        /// <summary>
        /// Ensures the pause toggle helper flips the state from playing to paused and back again.
        /// </summary>
        [Fact]
        public void Resolve_state_after_pause_toggle_flips_between_playing_and_paused() {
            DemoDisc.Zombislayer.ZombislayerSessionState paused = DemoDisc.Zombislayer.ZombislayerSessionComponent.ResolveStateAfterPauseToggle(DemoDisc.Zombislayer.ZombislayerSessionState.Playing);
            DemoDisc.Zombislayer.ZombislayerSessionState resumed = DemoDisc.Zombislayer.ZombislayerSessionComponent.ResolveStateAfterPauseToggle(DemoDisc.Zombislayer.ZombislayerSessionState.Paused);

            Assert.Equal(DemoDisc.Zombislayer.ZombislayerSessionState.Paused, paused);
            Assert.Equal(DemoDisc.Zombislayer.ZombislayerSessionState.Playing, resumed);
        }

        /// <summary>
        /// Ensures the pause overlay is shown only while the session is paused.
        /// </summary>
        [Fact]
        public void Should_show_pause_overlay_returns_true_only_for_paused_state() {
            Assert.False(DemoDisc.Zombislayer.ZombislayerSessionComponent.ShouldShowPauseOverlay(DemoDisc.Zombislayer.ZombislayerSessionState.Playing));
            Assert.True(DemoDisc.Zombislayer.ZombislayerSessionComponent.ShouldShowPauseOverlay(DemoDisc.Zombislayer.ZombislayerSessionState.Paused));
        }
    }
}
