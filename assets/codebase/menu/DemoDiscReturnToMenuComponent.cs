namespace city.menu {
    /// <summary>
    /// Returns the active demo-disc scene to the main menu when the temporary back bind is pressed.
    /// </summary>
    public sealed class DemoDiscReturnToMenuComponent : UpdateComponent {
        /// <summary>
        /// Stable runtime scene id used by the demo-disc main menu.
        /// </summary>
        public const string MainMenuSceneId = "DemoDiscMainMenu";

        /// <summary>
        /// Performs per-frame input polling for the demo-disc return bind.
        /// </summary>
        public override void Update() {
            InputSystem inputSystem = Core.Instance.Input;
            bool wasReturnPressed = WasKeyboardReturnPressed(inputSystem)
                || WasGamepadReturnPressed(inputSystem);

            if (wasReturnPressed) {
                Core.Instance.SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Single);
            }
        }

        /// <summary>
        /// Returns whether the current frame pressed one of the desktop return keys.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>True when one desktop return key pressed this frame.</returns>
        bool WasKeyboardReturnPressed(InputSystem inputSystem) {
#if DESKTOP_PLATFORM
            return inputSystem.WasKeyPressed(Keys.Escape) || inputSystem.WasKeyPressed(Keys.Back);
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns whether the current frame pressed one of the temporary gamepad return buttons.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>True when one return button pressed this frame.</returns>
        bool WasGamepadReturnPressed(InputSystem inputSystem) {
            return inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.East)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.North)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.Select);
        }
    }
}
