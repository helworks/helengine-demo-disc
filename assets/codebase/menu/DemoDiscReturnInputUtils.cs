namespace city.menu {
    /// <summary>
    /// Resolves the shared demo-disc scene return input semantics so authored scene exits honor both the configured platform return action and the generic reject button.
    /// </summary>
    public static class DemoDiscReturnInputUtils {
        /// <summary>
        /// Stable primary gamepad index used by demo-disc scenes.
        /// </summary>
        const int PrimaryGamepadIndex = 0;

        /// <summary>
        /// Returns whether the current frame pressed one of the shared demo-disc return inputs.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current and previous frame state.</param>
        /// <returns>True when either the configured standard return action or the fallback reject button was pressed this frame.</returns>
        public static bool WasReturnPressed(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before querying shared demo-disc return input.");
            }

            return Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Return)
                || WasFallbackRejectButtonPressed(
                    inputSystem.GetGamepadState(PrimaryGamepadIndex),
                    inputSystem.GetPreviousGamepadState(PrimaryGamepadIndex));
        }

        /// <summary>
        /// Returns whether the generic reject button transitioned from up to down between the supplied gamepad states.
        /// </summary>
        /// <param name="currentState">Current frame gamepad state.</param>
        /// <param name="previousState">Previous frame gamepad state.</param>
        /// <returns>True when the shared reject button was newly pressed this frame.</returns>
        public static bool WasFallbackRejectButtonPressed(InputGamepadState currentState, InputGamepadState previousState) {
            if (!currentState.Connected) {
                return false;
            }

            return currentState.IsButtonDown(InputGamepadButton.East)
                && !previousState.IsButtonDown(InputGamepadButton.East);
        }
    }
}
