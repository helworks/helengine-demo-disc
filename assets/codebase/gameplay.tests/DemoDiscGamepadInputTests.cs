using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies Demo Disc actions aggregate the separate Wii controller devices exposed by the engine.
    /// </summary>
    public sealed class DemoDiscGamepadInputTests {
        /// <summary>
        /// Ensures a button on a non-primary controller slot is visible to shared game actions.
        /// </summary>
        [Fact]
        public void Button_press_aggregates_across_controller_slots() {
            DemoDiscGamepadInputTestBackend backend = new DemoDiscGamepadInputTestBackend();
            backend.Enqueue(CreateFrame(CreateConnectedState(), CreateConnectedState()));
            backend.Enqueue(CreateFrame(CreateConnectedState(), CreateState(InputGamepadButton.South)));
            InputSystem inputSystem = new InputSystem();
            inputSystem.SetBackend(backend);

            inputSystem.EarlyUpdate();
            inputSystem.Update();
            inputSystem.EarlyUpdate();

            Assert.True(city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South));
        }

        /// <summary>
        /// Ensures the strongest left-stick value is selected across Wiimote/Nunchuk and GameCube slots.
        /// </summary>
        [Fact]
        public void Left_stick_aggregates_the_strongest_connected_axis() {
            DemoDiscGamepadInputTestBackend backend = new DemoDiscGamepadInputTestBackend();
            backend.Enqueue(CreateFrame(CreateStateWithLeftStick(1000), CreateStateWithLeftStick(-20000)));
            InputSystem inputSystem = new InputSystem();
            inputSystem.SetBackend(backend);
            inputSystem.EarlyUpdate();

            Assert.Equal((short)-20000, city.menu.DemoDiscGamepadInput.GetLeftStickX(inputSystem));
        }

        /// <summary>
        /// Creates one frame containing the supplied controller states.
        /// </summary>
        /// <param name="states">Controller states captured for the frame.</param>
        /// <returns>Input frame containing the supplied controllers.</returns>
        static InputFrameState CreateFrame(params InputGamepadState[] states) {
            return new InputFrameState {
                Gamepads = states,
                GamepadCount = states.Length
            };
        }

        /// <summary>
        /// Creates a connected controller with one held button.
        /// </summary>
        /// <param name="button">Button to hold.</param>
        /// <returns>Connected controller state.</returns>
        static InputGamepadState CreateState(InputGamepadButton button) {
            InputGamepadState state = CreateConnectedState();
            state.SetButtonDown(button, true);
            return state;
        }

        /// <summary>
        /// Creates a connected controller with one horizontal stick value.
        /// </summary>
        /// <param name="axisValue">Horizontal stick value.</param>
        /// <returns>Connected controller state.</returns>
        static InputGamepadState CreateStateWithLeftStick(short axisValue) {
            InputGamepadState state = CreateConnectedState();
            state.LeftStickX = axisValue;
            return state;
        }

        /// <summary>
        /// Creates a connected controller with no active controls.
        /// </summary>
        /// <returns>Connected controller state.</returns>
        static InputGamepadState CreateConnectedState() {
            return new InputGamepadState {
                Connected = true
            };
        }
    }
}
