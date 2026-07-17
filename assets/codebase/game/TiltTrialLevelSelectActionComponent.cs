namespace city.game {
    /// <summary>
    /// Converts a handheld selector button release into a command on the owning level-select controller.
    /// </summary>
    public sealed class TiltTrialLevelSelectActionComponent : Component {
        /// <summary>
        /// Gets or sets the selector command emitted by this button.
        /// </summary>
        public TiltTrialLevelSelectAction Action { get; set; }

        /// <summary>
        /// Gets or sets the zero-based stage index used by the SelectStage action.
        /// </summary>
        public int StageIndex { get; set; }

        InteractableComponent BoundInteractable;
        bool PointerPressStartedInside;

        /// <summary>
        /// Attaches the selector action to the sibling interactable when the button enters the runtime hierarchy.
        /// </summary>
        /// <param name="entity">Entity receiving this component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            TryBindInteractable();
        }

        /// <summary>
        /// Removes the selector action event subscription when the button leaves the runtime hierarchy.
        /// </summary>
        /// <param name="entity">Entity losing this component.</param>
        public override void ComponentRemoved(Entity entity) {
            UnbindInteractable();
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Releases the selector action event subscription and clears the active press state.
        /// </summary>
        public override void Dispose() {
            UnbindInteractable();
            base.Dispose();
        }

        /// <summary>
        /// Binds the first interactable sibling as the touch source for this selector action.
        /// </summary>
        void TryBindInteractable() {
            if (BoundInteractable != null || Parent == null || Parent.Components == null) {
                return;
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is InteractableComponent interactable) {
                    BoundInteractable = interactable;
                    BoundInteractable.CursorEvent += HandleCursorEvent;
                    return;
                }
            }

            throw new InvalidOperationException("TiltTrialLevelSelectActionComponent requires a sibling InteractableComponent.");
        }

        /// <summary>
        /// Removes the current interactable event subscription and clears the pending press state.
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
        /// Emits the configured selector command after a press and release occur inside this button.
        /// </summary>
        /// <param name="relativePosition">Pointer position relative to the button.</param>
        /// <param name="delta">Pointer movement reported by the interaction router.</param>
        /// <param name="interaction">Pointer interaction state.</param>
        void HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction) {
            if (interaction == PointerInteraction.Press) {
                PointerPressStartedInside = true;
                return;
            }
            if (interaction == PointerInteraction.Release) {
                bool shouldEmitAction = PointerPressStartedInside;
                PointerPressStartedInside = false;
                if (shouldEmitAction) {
                    TiltTrialLevelSelectComponent selector = ResolveSelector();
                    if (selector == null) {
                        throw new InvalidOperationException("TiltTrialLevelSelectActionComponent requires an attached TiltTrialLevelSelectComponent.");
                    }

                    selector.HandleAction(Action, StageIndex);
                }
                return;
            }
            if (interaction == PointerInteraction.Leave) {
                PointerPressStartedInside = false;
            }
        }

        /// <summary>
        /// Finds the nearest level-select controller in the owning selector hierarchy.
        /// </summary>
        /// <returns>Nearest level-select controller, or null when the hierarchy is not attached.</returns>
        TiltTrialLevelSelectComponent ResolveSelector() {
            Entity current = Parent;
            while (current != null) {
                if (current.Components != null) {
                    for (int componentIndex = 0; componentIndex < current.Components.Count; componentIndex++) {
                        if (current.Components[componentIndex] is TiltTrialLevelSelectComponent selector) {
                            return selector;
                        }
                    }
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
