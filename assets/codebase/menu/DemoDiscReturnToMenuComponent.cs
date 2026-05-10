namespace city.menu {
    /// <summary>
    /// Returns the current demo-disc scene to the curated main menu when the platform back bind is pressed.
    /// </summary>
    public sealed class DemoDiscReturnToMenuComponent : UpdateComponent {
        /// <summary>
        /// Stable authored scene id used for the demo-disc main menu.
        /// </summary>
        public const string MainMenuSceneId = "Scenes/DemoDiscMainMenu.helen";

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
        /// Returns the active demo scene to the main menu using the current execution mode's scene loading path.
        /// </summary>
        void ReturnToMainMenu() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before returning to the demo-disc main menu.");
            }

            if (ComponentExecutionContext.CurrentMode == ComponentExecutionMode.Editor) {
                if (Core.Instance.SceneLoadService == null) {
                    throw new InvalidOperationException("Core scene loading services must be initialized before returning to the demo-disc main menu.");
                }

                string resolvedScenePath = ResolveSceneContentPath(MainMenuSceneId);
                SceneAsset sceneAsset = Core.Instance.ContentManager.Load<SceneAsset>(resolvedScenePath, RuntimeContentProcessorIds.SceneAsset);
                Core.Instance.SceneLoadService.Load(sceneAsset);
                if (Parent != null) {
                    Parent.Enabled = false;
                }
            } else if (Core.Instance.SceneManager == null) {
                throw new InvalidOperationException("Core scene manager must be initialized before returning to the demo-disc main menu.");
            } else {
                Core.Instance.SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Single);
            }
        }

        /// <summary>
        /// Returns whether the current frame pressed the platform return bind.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>True when the current frame should navigate back to the main menu.</returns>
        bool WasReturnPressed(InputSystem inputSystem) {
            if (inputSystem.WasKeyPressed(Keys.Escape) || inputSystem.WasKeyPressed(Keys.Back)) {
                return true;
            }

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

        /// <summary>
        /// Resolves one authored scene id into the content-relative path available in the current execution layout.
        /// </summary>
        /// <param name="scenePath">Authored or packaged scene path requested by the return bind.</param>
        /// <returns>Content-relative path that exists beneath the current content root.</returns>
        string ResolveSceneContentPath(string scenePath) {
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before resolving demo-disc scene paths.");
            }
            if (string.IsNullOrWhiteSpace(scenePath)) {
                throw new ArgumentException("Scene path must be provided.", nameof(scenePath));
            }

            string normalizedScenePath = NormalizeRelativeContentPath(scenePath);
            string contentRootPath = Core.Instance.InitializationOptions.ContentRootPath;
            if (DoesContentFileExist(contentRootPath, normalizedScenePath)) {
                return normalizedScenePath;
            }
            if (ComponentExecutionContext.CurrentMode == ComponentExecutionMode.Editor) {
                throw new InvalidOperationException(
                    $"Demo-disc scene '{scenePath}' could not be found in authored form '{normalizedScenePath}'.");
            }

            string packagedScenePath = BuildPackagedSceneContentPath(normalizedScenePath);
            if (DoesContentFileExist(contentRootPath, packagedScenePath)) {
                return packagedScenePath;
            }

            throw new InvalidOperationException(
                $"Demo-disc scene '{scenePath}' could not be found in authored form '{normalizedScenePath}' or packaged form '{packagedScenePath}'.");
        }

        /// <summary>
        /// Builds the packaged content-relative path used by player builds for one authored scene id.
        /// </summary>
        /// <param name="scenePath">Normalized authored scene id.</param>
        /// <returns>Packaged content-relative scene path.</returns>
        string BuildPackagedSceneContentPath(string scenePath) {
            if (string.IsNullOrWhiteSpace(scenePath)) {
                throw new ArgumentException("Scene path must be provided.", nameof(scenePath));
            }

            if (scenePath.EndsWith(".hasset", StringComparison.OrdinalIgnoreCase)) {
                return scenePath;
            }
            if (scenePath.StartsWith("cooked/", StringComparison.OrdinalIgnoreCase)) {
                return scenePath;
            }

            string changedExtensionPath = Path.ChangeExtension(scenePath, ".hasset");
            return NormalizeRelativeContentPath(Path.Combine("scenes", changedExtensionPath));
        }

        /// <summary>
        /// Returns whether the supplied content-relative path exists beneath the current content root.
        /// </summary>
        /// <param name="contentRootPath">Absolute content root path.</param>
        /// <param name="relativePath">Content-relative path to inspect.</param>
        /// <returns>True when the content file exists.</returns>
        bool DoesContentFileExist(string contentRootPath, string relativePath) {
            if (string.IsNullOrWhiteSpace(contentRootPath)) {
                throw new ArgumentException("Content root path must be provided.", nameof(contentRootPath));
            }
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            string normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string fullPath = Path.GetFullPath(Path.Combine(contentRootPath, normalizedRelativePath));
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Normalizes one content-relative path to the forward-slash form used by runtime asset ids.
        /// </summary>
        /// <param name="relativePath">Relative content path to normalize.</param>
        /// <returns>Normalized content-relative path.</returns>
        string NormalizeRelativeContentPath(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            return relativePath.Replace('\\', '/');
        }
    }
}
