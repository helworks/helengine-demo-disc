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
            InputSystem inputSystem = Core.Instance != null ? Core.Instance.Input : null;
            if (inputSystem == null) {
                PreviousGamepadState = default;
                return;
            }

            if (!WasReturnPressed(inputSystem)) {
                PreviousGamepadState = ReadPrimaryGamepadState(inputSystem);
                return;
            }

            ReturnToMainMenu();
        }

        /// <summary>
        /// Returns the active demo scene to the main menu using the current execution mode's scene-loading path.
        /// </summary>
        void ReturnToMainMenu() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before returning to the demo-disc main menu.");
            }

#if HELENGINE_EDITOR
            if (true) {
                if (Core.Instance.SceneLoadService == null) {
                    throw new InvalidOperationException("Core scene loading services must be initialized before returning to the demo-disc main menu.");
                }

                SceneAsset sceneAsset = Core.Instance.ContentManager.Load<SceneAsset>(MainMenuScenePath, RuntimeContentProcessorIds.SceneAsset);
                Core.Instance.SceneLoadService.Load(sceneAsset);
                if (Parent != null) {
                    Parent.Enabled = false;
                }
            }
#else
            if (Core.Instance.SceneManager == null) {
                throw new InvalidOperationException("Core scene manager must be initialized before returning to the demo-disc main menu.");
            } else {
                Core.Instance.SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Single);
            }
#endif
        }

        /// <summary>
        /// Returns whether the current frame pressed the temporary platform return bind.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>True when the current frame should navigate back to the main menu.</returns>
        bool WasReturnPressed(InputSystem inputSystem) {
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Escape) || inputSystem.WasKeyPressed(Keys.Back)) {
                return true;
            }
#endif

            InputGamepadState currentGamepadState = ReadPrimaryGamepadState(inputSystem);
            if (!currentGamepadState.Connected) {
                PreviousGamepadState = currentGamepadState;
                return false;
            }

            return WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.East)
                || WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.North)
                || WasGamepadButtonPressed(currentGamepadState, PreviousGamepadState, InputGamepadButton.Select);
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

        /// <summary>
        /// Reads the current primary gamepad state from the shared input system.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>Current primary gamepad state.</returns>
        InputGamepadState ReadPrimaryGamepadState(InputSystem inputSystem) {
            if (inputSystem == null) {
                return default;
            }

            return inputSystem.GetGamepadState(0);
        }
    }
}


