using helengine;

namespace city.game {
    /// <summary>
    /// Converts a serialized Tilt Play action-host press into a semantic menu action.
    /// </summary>
    public sealed class TiltPlayMenuActionComponent : Component {
        /// <summary>
        /// Gets or sets the menu action emitted after a press and release occur inside the owning interactable.
        /// </summary>
        public TiltPlayMenuAction Action { get; set; }

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

            throw new InvalidOperationException("Tilt Play menu action requires a sibling InteractableComponent.");
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
        /// <param name="delta">Pointer movement reported by the shared interaction router.</param>
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
                    TiltPlayMenuComponent menu = ResolveMenu();
                    menu.HandleAction(Action);
                }
                return;
            }
            if (interaction == PointerInteraction.Leave) {
                PointerPressStartedInside = false;
            }
        }

        /// <summary>
        /// Finds the nearest Tilt Play menu controller in the owning presentation hierarchy.
        /// </summary>
        /// <returns>Active title-shell controller.</returns>
        TiltPlayMenuComponent ResolveMenu() {
            Entity current = Parent;
            while (current != null) {
                if (current.Components != null) {
                    for (int componentIndex = 0; componentIndex < current.Components.Count; componentIndex++) {
                        if (current.Components[componentIndex] is TiltPlayMenuComponent menuComponent) {
                            return menuComponent;
                        }
                    }
                }

                current = current.Parent;
            }

            throw new InvalidOperationException("Tilt Play menu action requires an attached TiltPlayMenuComponent.");
        }
    }
}
