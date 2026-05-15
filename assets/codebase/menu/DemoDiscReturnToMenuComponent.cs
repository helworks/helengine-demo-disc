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
        /// Stable authored scene path used by editor-mode direct scene loading.
        /// </summary>
        public const string MainMenuScenePath = "scenes/DemoDiscMainMenu.helen";

        /// <summary>
        /// Previous primary gamepad state used for edge detection.
        /// </summary>
        InputGamepadState PreviousGamepadState;

        /// <summary>
        /// Performs per-frame input polling for the demo-disc return bind.
        /// </summary>
        public override void Update() {
            if (!WasReturnPressed()) {
                PreviousGamepadState = Core.Instance.Input.GetGamepadState(0);
                return;
            }

            ReturnToMainMenu();
        }

        /// <summary>
        /// Returns the active demo scene to the main menu using the current execution mode's scene-loading path.
        /// </summary>
        void ReturnToMainMenu() {
            Core.Instance.SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Single);
        }

        /// <summary>
        /// Returns whether the current frame pressed the temporary platform return bind.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>True when the current frame should navigate back to the main menu.</returns>
        bool WasReturnPressed() {
#if DESKTOP_PLATFORM
            if (Core.Instance.Input.WasKeyPressed(Keys.Escape) || Core.Instance.Input.WasKeyPressed(Keys.Back)) {
                return true;
            }
#endif

            InputGamepadState currentGamepadState = Core.Instance.Input.GetGamepadState(0);
            if (!currentGamepadState.Connected) {
                PreviousGamepadState = currentGamepadState;
                return false;
            }

            bool wasReturnPressed =
                WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.East)
                || WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.North)
                || WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.Select);
            PreviousGamepadState = currentGamepadState;
            return wasReturnPressed;
        }

        /// <summary>
        /// Returns whether one abstract gamepad button transitioned from up to down on the current frame.
        /// </summary>
        /// <param name="currentState">Current raw gamepad state.</param>
        /// <param name="previousState">Previous raw gamepad state.</param>
        /// <param name="button">Button to test.</param>
        /// <returns>True when the button was pressed this frame.</returns>
        bool WasGamepadButtonPressed(InputGamepadState currentState, InputGamepadState previousState, InputGamepadButton button) {
            return currentState.IsButtonDown(button) && !previousState.IsButtonDown(button);
        }
    }
}
