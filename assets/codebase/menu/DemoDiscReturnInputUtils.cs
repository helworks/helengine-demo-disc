namespace city.menu {
    /// <summary>
    /// Resolves the shared demo-disc scene return input semantics so authored scene exits honor the configured platform return action and retain the generic reject fallback outside PS2.
    /// </summary>
    public static class DemoDiscReturnInputUtils {
        /// <summary>
        /// Returns whether the current frame pressed one of the shared demo-disc return inputs.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current and previous frame state.</param>
        /// <returns>True when the configured standard return action was pressed, or when the fallback reject button was pressed on a non-PS2 platform.</returns>
        public static bool WasReturnPressed(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before querying shared demo-disc return input.");
            }

            return Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Return)
                || (!IsPs2Platform() && DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.East));
        }

        /// <summary>
        /// Returns whether the active runtime platform is PlayStation 2.
        /// </summary>
        /// <returns>True when the active runtime platform is PS2.</returns>
        static bool IsPs2Platform() {
            PlatformInfo platformInfo = Core.Instance?.PlatformInfo;
            return platformInfo != null && string.Equals(platformInfo.Name, "ps2", StringComparison.OrdinalIgnoreCase);
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
