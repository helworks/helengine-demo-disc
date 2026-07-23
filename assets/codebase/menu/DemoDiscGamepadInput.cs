namespace city.menu {
    /// <summary>
    /// Aggregates the separate console controller devices exposed by the engine into the shared Demo Disc game input surface.
    /// </summary>
    public static class DemoDiscGamepadInput {
        /// <summary>
        /// Returns whether any connected controller currently holds the requested abstract button.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <param name="button">Abstract button to query.</param>
        /// <returns>True when at least one connected controller holds the button.</returns>
        public static bool IsButtonDown(InputSystem inputSystem, InputGamepadButton button) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            for (int controllerIndex = 0; controllerIndex < inputSystem.GetGamepadCount(); controllerIndex++) {
                InputGamepadState state = inputSystem.GetGamepadState(controllerIndex);
                if (state.Connected && state.IsButtonDown(button)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether any controller newly pressed the requested abstract button this frame.
        /// </summary>
        /// <param name="inputSystem">Input system supplying current and previous frame states.</param>
        /// <param name="button">Abstract button to query.</param>
        /// <returns>True when at least one controller transitioned from released to pressed.</returns>
        public static bool WasButtonPressed(InputSystem inputSystem, InputGamepadButton button) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            for (int controllerIndex = 0; controllerIndex < inputSystem.GetGamepadCount(); controllerIndex++) {
                InputGamepadState currentState = inputSystem.GetGamepadState(controllerIndex);
                if (currentState.Connected && currentState.IsButtonDown(button)
                    && !inputSystem.GetPreviousGamepadState(controllerIndex).IsButtonDown(button)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the strongest connected left-stick horizontal value across all controller devices.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>The signed left-stick X value with the greatest absolute magnitude.</returns>
        public static short GetLeftStickX(InputSystem inputSystem) {
            return GetStrongestAxis(inputSystem, true);
        }

        /// <summary>
        /// Returns the strongest connected left-stick vertical value across all controller devices.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>The signed left-stick Y value with the greatest absolute magnitude.</returns>
        public static short GetLeftStickY(InputSystem inputSystem) {
            return GetStrongestAxis(inputSystem, false);
        }

        /// <summary>
        /// Returns the strongest connected left-stick vertical value from the previous frame.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the previous frame state.</param>
        /// <returns>The signed previous-frame left-stick Y value with the greatest absolute magnitude.</returns>
        public static short GetPreviousLeftStickY(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            short strongestValue = 0;
            for (int controllerIndex = 0; controllerIndex < inputSystem.GetGamepadCount(); controllerIndex++) {
                InputGamepadState state = inputSystem.GetPreviousGamepadState(controllerIndex);
                if (state.Connected && Math.Abs((int)state.LeftStickY) > Math.Abs((int)strongestValue)) {
                    strongestValue = state.LeftStickY;
                }
            }

            return strongestValue;
        }

        /// <summary>
        /// Returns the strongest connected right-stick horizontal value across all controller devices.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>The signed right-stick X value with the greatest absolute magnitude.</returns>
        public static short GetRightStickX(InputSystem inputSystem) {
            return GetStrongestRightAxis(inputSystem, true);
        }

        /// <summary>
        /// Returns the strongest connected right-stick vertical value across all controller devices.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>The signed right-stick Y value with the greatest absolute magnitude.</returns>
        public static short GetRightStickY(InputSystem inputSystem) {
            return GetStrongestRightAxis(inputSystem, false);
        }

        /// <summary>
        /// Selects the signed axis value with the greatest magnitude from connected controller devices.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <param name="horizontal">True to select the X axis; false to select the Y axis.</param>
        /// <returns>The strongest signed axis value, or zero when no controller is connected.</returns>
        static short GetStrongestAxis(InputSystem inputSystem, bool horizontal) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            short strongestValue = 0;
            for (int controllerIndex = 0; controllerIndex < inputSystem.GetGamepadCount(); controllerIndex++) {
                InputGamepadState state = inputSystem.GetGamepadState(controllerIndex);
                short axisValue = horizontal ? state.LeftStickX : state.LeftStickY;
                if (state.Connected && Math.Abs((int)axisValue) > Math.Abs((int)strongestValue)) {
                    strongestValue = axisValue;
                }
            }

            return strongestValue;
        }

        /// <summary>
        /// Selects the strongest signed right-stick axis value from connected controller devices.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <param name="horizontal">True to select the X axis; false to select the Y axis.</param>
        /// <returns>The strongest signed axis value, or zero when no controller is connected.</returns>
        static short GetStrongestRightAxis(InputSystem inputSystem, bool horizontal) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }

            short strongestValue = 0;
            for (int controllerIndex = 0; controllerIndex < inputSystem.GetGamepadCount(); controllerIndex++) {
                InputGamepadState state = inputSystem.GetGamepadState(controllerIndex);
                short axisValue = horizontal ? state.RightStickX : state.RightStickY;
                if (state.Connected && Math.Abs((int)axisValue) > Math.Abs((int)strongestValue)) {
                    strongestValue = axisValue;
                }
            }

            return strongestValue;
        }
    }
}
