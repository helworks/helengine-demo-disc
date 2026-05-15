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
            InputSystem inputSystem = Core.Instance.Input;
            if (!WasReturnPressed(inputSystem)) {
                PreviousGamepadState = inputSystem.GetGamepadState(0);
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
        /// <returns>True when the current frame should navigate back to the main menu.</returns>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        bool WasReturnPressed(InputSystem inputSystem) {
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Escape) || inputSystem.WasKeyPressed(Keys.Back)) {
                return true;
            }
#endif

            InputGamepadState currentGamepadState = inputSystem.GetGamepadState(0);
            if (!currentGamepadState.Connected) {
                PreviousGamepadState = currentGamepadState;
                return false;
            }

            bool wasReturnPressed =
                inputSystem.WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.East)
                || inputSystem.WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.North)
                || inputSystem.WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.Select);
            PreviousGamepadState = currentGamepadState;
            return wasReturnPressed;
        }
    }
}
