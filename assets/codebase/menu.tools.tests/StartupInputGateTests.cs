namespace city.menu.tools.tests {
    /// <summary>
    /// Verifies the boot-time gate that temporarily owns Demo Disc menu input.
    /// </summary>
    public sealed class StartupInputGateTests {
        /// <summary>
        /// Ensures releasing the active splash gate returns menu input ownership to the loaded menu.
        /// </summary>
        [Fact]
        public void Release_after_acquire_allows_menu_input() {
            StartupInputGate.Acquire();

            Assert.True(StartupInputGate.IsBlocked);

            StartupInputGate.Release();

            Assert.False(StartupInputGate.IsBlocked);
        }
    }
}
