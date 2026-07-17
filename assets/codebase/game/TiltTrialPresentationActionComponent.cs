namespace city.game {
    /// <summary>
    /// Converts a serialized interactable press into a semantic action on the active Tilt Trial session.
    /// </summary>
    public sealed class TiltTrialPresentationActionComponent : Component {
        /// <summary>
        /// Gets or sets the session action emitted after a press and release occur inside the owning interactable.
        /// </summary>
        public TiltTrialSessionAction Action { get; set; }

        /// <summary>
        /// Interactable sibling that supplies pointer and touch events.
        /// </summary>
        InteractableComponent BoundInteractable;

        /// <summary>
        /// Tracks whether the current pointer interaction began inside the interactable.
        /// </summary>
        bool PointerPressStartedInside;

        /// <summary>
        /// Attaches the action bridge to a sibling interactable when the entity enters the runtime hierarchy.
        /// </summary>
        /// <param name="entity">Entity receiving this component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            TryBindInteractable();
        }

        /// <summary>
        /// Removes the pointer event subscription when the component leaves the runtime hierarchy.
        /// </summary>
        /// <param name="entity">Entity losing this component.</param>
        public override void ComponentRemoved(Entity entity) {
            UnbindInteractable();
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Releases the pointer event subscription owned by this component.
        /// </summary>
        public override void Dispose() {
            UnbindInteractable();
            base.Dispose();
        }

        /// <summary>
        /// Binds the first interactable on the owning entity as the serialized action source.
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
        }

        /// <summary>
        /// Removes the current pointer event subscription and clears its in-progress press state.
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
        /// Emits the configured semantic action only when a pointer press is released inside the same interactable.
        /// </summary>
        /// <param name="relativePosition">Pointer position relative to the interactable.</param>
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
                    TiltTrialSessionComponent session = ResolveSession();
                    if (session == null) {
                        throw new InvalidOperationException("Tilt Trial presentation action requires an attached session component.");
                    }

                    session.RequestAction(Action);
                }
                return;
            }
            if (interaction == PointerInteraction.Leave) {
                PointerPressStartedInside = false;
            }
        }

        /// <summary>
        /// Finds the nearest Tilt Trial session component in the owning presentation hierarchy.
        /// </summary>
        /// <returns>Active session component, or null when the presentation is not attached to gameplay.</returns>
        TiltTrialSessionComponent ResolveSession() {
            Entity current = Parent;
            while (current != null) {
                if (current.Components != null) {
                    for (int componentIndex = 0; componentIndex < current.Components.Count; componentIndex++) {
                        if (current.Components[componentIndex] is TiltTrialSessionComponent sessionComponent) {
                            return sessionComponent;
                        }
                    }
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
