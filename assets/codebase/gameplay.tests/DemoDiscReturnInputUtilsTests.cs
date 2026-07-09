using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies the shared demo-disc return-input helper recognizes the generic reject button used by handheld scene exits.
    /// </summary>
    public sealed class DemoDiscReturnInputUtilsTests {
        /// <summary>
        /// Ensures one newly pressed East button is treated as a shared fallback return press.
        /// </summary>
        [Fact]
        public void Fallback_return_press_detects_east_button_transition() {
            InputGamepadState previousState = CreateConnectedState();
            InputGamepadState currentState = CreateConnectedState();
            currentState.SetButtonDown(InputGamepadButton.East, true);

            bool wasPressed = city.menu.DemoDiscReturnInputUtils.WasFallbackRejectButtonPressed(currentState, previousState);

            Assert.True(wasPressed);
        }

        /// <summary>
        /// Ensures disconnected gamepads never trigger the shared fallback return press.
        /// </summary>
        [Fact]
        public void Fallback_return_press_ignores_disconnected_gamepads() {
            InputGamepadState previousState = new InputGamepadState();
            InputGamepadState currentState = new InputGamepadState();
            currentState.SetButtonDown(InputGamepadButton.East, true);

            bool wasPressed = city.menu.DemoDiscReturnInputUtils.WasFallbackRejectButtonPressed(currentState, previousState);

            Assert.False(wasPressed);
        }

        /// <summary>
        /// Creates one connected gamepad state with no active buttons.
        /// </summary>
        /// <returns>Connected gamepad state used by the fallback-return tests.</returns>
        static InputGamepadState CreateConnectedState() {
            return new InputGamepadState {
                Connected = true
            };
        }
    }
}
