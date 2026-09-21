namespace DemoDisc.TiltPlay.tests {
    /// <summary>
    /// Verifies the presentation-independent Tilt Play title-menu state contract.
    /// </summary>
    public sealed class TiltPlayMenuComponentTests {
        /// <summary>
        /// Verifies that the title state is accepted as the initial menu state.
        /// </summary>
        [Fact]
        public void CreateStateMachine_starts_at_title_when_initialized() {
            global::helengine.FiniteStateMachine<DemoDisc.TiltPlay.TiltPlayMenuState> machine = DemoDisc.TiltPlay.TiltPlayMenuComponent.CreateStateMachine();

            machine.Initialize(DemoDisc.TiltPlay.TiltPlayMenuState.Title);

            Assert.Equal(DemoDisc.TiltPlay.TiltPlayMenuState.Title, machine.CurrentState);
        }

        /// <summary>
        /// Verifies title actions route to their corresponding menu panels.
        /// </summary>
        [Fact]
        public void ResolveActionState_routes_play_and_options_to_their_panels() {
            Assert.Equal(DemoDisc.TiltPlay.TiltPlayMenuState.LevelSelect, DemoDisc.TiltPlay.TiltPlayMenuComponent.ResolveActionState(DemoDisc.TiltPlay.TiltPlayMenuAction.Play));
            Assert.Equal(DemoDisc.TiltPlay.TiltPlayMenuState.Options, DemoDisc.TiltPlay.TiltPlayMenuComponent.ResolveActionState(DemoDisc.TiltPlay.TiltPlayMenuAction.Options));
        }

        /// <summary>
        /// Verifies that returning from either title submenu restores the title state.
        /// </summary>
        [Fact]
        public void ResolveBackState_returns_title_from_submenus() {
            Assert.Equal(DemoDisc.TiltPlay.TiltPlayMenuState.Title, DemoDisc.TiltPlay.TiltPlayMenuComponent.ResolveBackState(DemoDisc.TiltPlay.TiltPlayMenuState.Options));
            Assert.Equal(DemoDisc.TiltPlay.TiltPlayMenuState.Title, DemoDisc.TiltPlay.TiltPlayMenuComponent.ResolveBackState(DemoDisc.TiltPlay.TiltPlayMenuState.LevelSelect));
        }

        /// <summary>
        /// Verifies that only the visible level-selector panel may consume selector input.
        /// </summary>
        [Fact]
        public void ShouldLevelSelectorProcessInput_is_only_true_in_level_select_state() {
            Assert.False(DemoDisc.TiltPlay.TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(DemoDisc.TiltPlay.TiltPlayMenuState.Title));
            Assert.False(DemoDisc.TiltPlay.TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(DemoDisc.TiltPlay.TiltPlayMenuState.Options));
            Assert.True(DemoDisc.TiltPlay.TiltPlayMenuComponent.ShouldLevelSelectorProcessInput(DemoDisc.TiltPlay.TiltPlayMenuState.LevelSelect));
        }

        /// <summary>
        /// Verifies title navigation wraps across the three available actions.
        /// </summary>
        [Fact]
        public void ResolveTitleActionIndexAfterNavigation_wraps_at_both_ends() {
            Assert.Equal(2, DemoDisc.TiltPlay.TiltPlayMenuComponent.ResolveTitleActionIndexAfterNavigation(0, false));
            Assert.Equal(0, DemoDisc.TiltPlay.TiltPlayMenuComponent.ResolveTitleActionIndexAfterNavigation(2, true));
        }

        /// <summary>
        /// Verifies Tilt Play avoids exception overloads unsupported by generated PSP C++.
        /// </summary>
        [Fact]
        public void TiltPlayMenuComponent_uses_codegen_supported_argument_out_of_range_exceptions() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltPlayMenuComponent.cs");

            Assert.DoesNotContain("nameof(action), action,", source, StringComparison.Ordinal);
            Assert.DoesNotContain("nameof(currentState), currentState,", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the title controller displays only the selected action's authored overlay.
        /// </summary>
        [Fact]
        public void TiltPlayMenuComponent_applies_title_action_selection_presentation() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltPlayMenuComponent.cs");

            Assert.Contains("ApplyTitleActionSelection();", source, StringComparison.Ordinal);
            Assert.Contains("PlayButtonSelectedOverlay.Enabled = isTitleVisible && SelectedTitleActionIndex == 0;", source, StringComparison.Ordinal);
            Assert.Contains("OptionsButtonSelectedOverlay.Enabled = isTitleVisible && SelectedTitleActionIndex == 1;", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscButtonSelectedOverlay.Enabled = isTitleVisible && SelectedTitleActionIndex == 2;", source, StringComparison.Ordinal);
        }
    }
}
