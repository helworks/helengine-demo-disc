namespace city.tests {
    /// <summary>
    /// Verifies the presentation-independent Tilt Play title-menu state contract.
    /// </summary>
    public sealed class TiltPlayMenuComponentTests {
        /// <summary>
        /// Verifies that the title state is accepted as the initial menu state.
        /// </summary>
        [Fact]
        public void CreateStateMachine_starts_at_title_when_initialized() {
            global::helengine.FiniteStateMachine<city.game.TiltPlayMenuState> machine = city.game.TiltPlayMenuComponent.CreateStateMachine();

            machine.Initialize(city.game.TiltPlayMenuState.Title);

            Assert.Equal(city.game.TiltPlayMenuState.Title, machine.CurrentState);
        }

        /// <summary>
        /// Verifies title actions route to their corresponding menu panels.
        /// </summary>
        [Fact]
        public void ResolveActionState_routes_play_and_options_to_their_panels() {
            Assert.Equal(city.game.TiltPlayMenuState.LevelSelect, city.game.TiltPlayMenuComponent.ResolveActionState(city.game.TiltPlayMenuAction.Play));
            Assert.Equal(city.game.TiltPlayMenuState.Options, city.game.TiltPlayMenuComponent.ResolveActionState(city.game.TiltPlayMenuAction.Options));
        }

        /// <summary>
        /// Verifies that returning from either title submenu restores the title state.
        /// </summary>
        [Fact]
        public void ResolveBackState_returns_title_from_submenus() {
            Assert.Equal(city.game.TiltPlayMenuState.Title, city.game.TiltPlayMenuComponent.ResolveBackState(city.game.TiltPlayMenuState.Options));
            Assert.Equal(city.game.TiltPlayMenuState.Title, city.game.TiltPlayMenuComponent.ResolveBackState(city.game.TiltPlayMenuState.LevelSelect));
        }

        /// <summary>
        /// Verifies that only the visible level-selector panel may consume selector input.
        /// </summary>
        [Fact]
        public void ShouldLevelSelectorProcessInput_is_only_true_in_level_select_state() {
            Assert.False(city.game.TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(city.game.TiltPlayMenuState.Title));
            Assert.False(city.game.TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(city.game.TiltPlayMenuState.Options));
            Assert.True(city.game.TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(city.game.TiltPlayMenuState.LevelSelect));
        }

        /// <summary>
        /// Verifies title navigation wraps across the three available actions.
        /// </summary>
        [Fact]
        public void ResolveTitleActionIndexAfterNavigation_wraps_at_both_ends() {
            Assert.Equal(2, city.game.TiltPlayMenuComponent.ResolveTitleActionIndexAfterNavigation(0, false));
            Assert.Equal(0, city.game.TiltPlayMenuComponent.ResolveTitleActionIndexAfterNavigation(2, true));
        }

        /// <summary>
        /// Verifies Tilt Play avoids exception overloads unsupported by generated PSP C++.
        /// </summary>
        [Fact]
        public void TiltPlayMenuComponent_uses_codegen_supported_argument_out_of_range_exceptions() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltPlayMenuComponent.cs");

            Assert.DoesNotContain("nameof(action), action,", source, StringComparison.Ordinal);
            Assert.DoesNotContain("nameof(currentState), currentState,", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the title controller applies focused-action presentation rather than leaving every button identical.
        /// </summary>
        [Fact]
        public void TiltPlayMenuComponent_applies_title_action_selection_presentation() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltPlayMenuComponent.cs");

            Assert.Contains("ApplyTitleActionSelection();", source, StringComparison.Ordinal);
            Assert.Contains("TiltPlayPlayButton", source, StringComparison.Ordinal);
            Assert.Contains("new byte4(102, 56, 160, 255)", source, StringComparison.Ordinal);
        }
    }
}
