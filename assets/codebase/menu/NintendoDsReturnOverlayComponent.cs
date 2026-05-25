namespace city.menu {
    /// <summary>
    /// Owns Nintendo DS companion-scene return behavior from the bottom-screen back overlay.
    /// </summary>
    public sealed class NintendoDsReturnOverlayComponent : UpdateComponent {
        /// <summary>
        /// Stable logical scene id used by the demo-disc main menu.
        /// </summary>
        public const string MainMenuSceneId = "DemoDiscMainMenu";

        /// <summary>
        /// Interactable host used by the bottom-screen back button.
        /// </summary>
        InteractableComponent BoundInteractable;

        /// <summary>
        /// Tracks whether the active pointer press began inside the bound interactable.
        /// </summary>
        bool PointerPressStartedInside;

        /// <summary>
        /// Tracks whether this component already requested the return transition during its current lifetime.
        /// </summary>
        bool SceneLoadWasRequested;

        /// <summary>
        /// Binds the sibling interactable when the component is attached to the back-button host.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            TryBindInteractable();
        }

        /// <summary>
        /// Performs per-frame polling for the Nintendo DS back bind.
        /// </summary>
        public override void Update() {
            TryBindInteractable();
            InputSystem inputSystem = Core.Instance.Input;
            if (WasGamepadReturnPressed(inputSystem)) {
                LoadResolvedMainMenuScene();
            }
        }

        /// <summary>
        /// Releases the sibling interactable subscription before the component instance is deleted.
        /// </summary>
        public void Dispose() {
            UnbindInteractable();
        }

        /// <summary>
        /// Releases the sibling interactable subscription when the component detaches from its owner.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentRemoved(Entity entity) {
            UnbindInteractable();
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Binds the sibling interactable used to receive pointer clicks from the back button host.
        /// </summary>
        void TryBindInteractable() {
            if (BoundInteractable != null) {
                return;
            } else if (Parent == null || Parent.Components == null) {
                return;
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is InteractableComponent interactable) {
                    BoundInteractable = interactable;
                    BoundInteractable.CursorEvent += HandleCursorEvent;
                    return;
                }
            }

            throw new InvalidOperationException("NintendoDsReturnOverlayComponent requires a sibling InteractableComponent.");
        }

        /// <summary>
        /// Releases the current interactable binding and clears active press state.
        /// </summary>
        void UnbindInteractable() {
            if (BoundInteractable == null) {
                return;
            }

            BoundInteractable.CursorEvent -= HandleCursorEvent;
            BoundInteractable = null;
            PointerPressStartedInside = false;
        }

        /// <summary>
        /// Handles pointer press and release events from the back button interactable.
        /// </summary>
        /// <param name="relativePosition">Pointer position relative to the interactable.</param>
        /// <param name="delta">Pointer delta reported by the shared interaction router.</param>
        /// <param name="interaction">Current pointer interaction state.</param>
        void HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction) {
            if (interaction == PointerInteraction.Press) {
                PointerPressStartedInside = true;
                return;
            }
            if (interaction == PointerInteraction.Release) {
                bool shouldReturnToMenu = PointerPressStartedInside;
                PointerPressStartedInside = false;
                if (shouldReturnToMenu) {
                    LoadResolvedMainMenuScene();
                }
                return;
            }
            if (interaction == PointerInteraction.Leave) {
                PointerPressStartedInside = false;
            }
        }

        /// <summary>
        /// Returns whether the current frame pressed the configured standard platform return action.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <returns>True when the configured standard platform return action was pressed this frame.</returns>
        bool WasGamepadReturnPressed(InputSystem inputSystem) {
            if (inputSystem == null) {
                throw new ArgumentNullException(nameof(inputSystem));
            }
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before querying the standard platform return action.");
            }

            return Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Return);
        }

        /// <summary>
        /// Loads the logical main menu scene after resolving any active scene-map override.
        /// </summary>
        void LoadResolvedMainMenuScene() {
            if (SceneLoadWasRequested) {
                return;
            }
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before returning to the main menu.");
            }
            if (Core.Instance.SceneManager == null) {
                throw new InvalidOperationException("Core scene manager must be initialized before runtime menu scene loading can occur.");
            }

            string resolvedSceneId = SceneMapComponent.ResolveSceneId(MainMenuSceneId);
            SceneLoadWasRequested = true;
            Core.Instance.SceneManager.LoadScene(resolvedSceneId, SceneLoadMode.Single);
        }
    }
}
