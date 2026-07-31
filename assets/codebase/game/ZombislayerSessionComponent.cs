using city.menu;

namespace city.game {
    /// <summary>
    /// Owns the Zombislayer gameplay session state, pause-overlay visibility, and return-to-menu flow.
    /// </summary>
    public sealed class ZombislayerSessionComponent : UpdateComponent {
        /// <summary>
        /// Stable overlay entity name used by the generated scene.
        /// </summary>
        public const string PauseOverlayEntityName = "ZombislayerPauseOverlay";

        /// <summary>
        /// Backing state machine used by the active gameplay session.
        /// </summary>
        readonly FiniteStateMachine<ZombislayerSessionState> SessionStateMachine;

        /// <summary>
        /// Tracks whether the runtime state machine has been initialized yet.
        /// </summary>
        bool IsSessionStateInitialized;

        /// <summary>
        /// Cached pause overlay entity resolved from the generated gameplay scene.
        /// </summary>
        Entity PauseOverlayEntity;

        /// <summary>
        /// Initializes one Zombislayer gameplay session controller.
        /// </summary>
        public ZombislayerSessionComponent() {
            SessionStateMachine = CreateStateMachine();
            UpdateOrder = 1;
        }

        /// <summary>
        /// Gets the current runtime session state.
        /// </summary>
        public ZombislayerSessionState CurrentSessionState => SessionStateMachine.CurrentState;

        /// <summary>
        /// Builds the Zombislayer runtime session state machine.
        /// </summary>
        /// <returns>Configured finite state machine.</returns>
        public static FiniteStateMachine<ZombislayerSessionState> CreateStateMachine() {
            FiniteStateMachine<ZombislayerSessionState> machine = new FiniteStateMachine<ZombislayerSessionState>();
            machine.RegisterState(ZombislayerSessionState.Playing, new FiniteStateDefinition<ZombislayerSessionState>());
            machine.RegisterState(ZombislayerSessionState.Paused, new FiniteStateDefinition<ZombislayerSessionState>());
            return machine;
        }

        /// <summary>
        /// Resolves the next session state after a pause-toggle request.
        /// </summary>
        /// <param name="currentState">Current gameplay session state.</param>
        /// <returns>Toggled session state.</returns>
        public static ZombislayerSessionState ResolveStateAfterPauseToggle(ZombislayerSessionState currentState) {
            return currentState == ZombislayerSessionState.Playing
                ? ZombislayerSessionState.Paused
                : ZombislayerSessionState.Playing;
        }

        /// <summary>
        /// Returns whether the pause overlay should be visible for the supplied session state.
        /// </summary>
        /// <param name="state">Current gameplay session state.</param>
        /// <returns>True when the pause overlay should be shown.</returns>
        public static bool ShouldShowPauseOverlay(ZombislayerSessionState state) {
            return state == ZombislayerSessionState.Paused;
        }

        /// <summary>
        /// Advances the gameplay session state using the current keyboard input.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("ZombislayerSessionComponent requires an attached gameplay UI root entity.");
            }

            EnsureSessionStateInitialized();
            ResolvePauseOverlayEntityWhenNeeded();
            UpdateSessionState();
            RefreshOverlayPresentation();
            Core.Instance.Input.SetPointerWrapEnabled(SessionStateMachine.CurrentState == ZombislayerSessionState.Playing);
        }

        /// <summary>
        /// Ensures the runtime state machine starts in playing state exactly once.
        /// </summary>
        void EnsureSessionStateInitialized() {
            if (IsSessionStateInitialized) {
                return;
            }

            SessionStateMachine.Initialize(ZombislayerSessionState.Playing);
            IsSessionStateInitialized = true;
        }

        /// <summary>
        /// Resolves the generated pause overlay entity the first time the session updates.
        /// </summary>
        void ResolvePauseOverlayEntityWhenNeeded() {
            if (PauseOverlayEntity != null) {
                return;
            }

            PauseOverlayEntity = FindRequiredChildEntity(Parent, 0, PauseOverlayEntityName);
        }

        /// <summary>
        /// Polls the current frame input and updates the gameplay session state.
        /// </summary>
        void UpdateSessionState() {
            InputSystem inputSystem = Core.Instance.Input;
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Escape)) {
                SessionStateMachine.TryChangeState(ResolveStateAfterPauseToggle(SessionStateMachine.CurrentState));
                return;
            }
#endif

            if (SessionStateMachine.CurrentState != ZombislayerSessionState.Paused) {
                return;
            }

#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Enter)) {
                SessionStateMachine.TryChangeState(ZombislayerSessionState.Playing);
            } else if (inputSystem.WasKeyPressed(Keys.Back)) {
                LoadResolvedMainMenuScene();
            }
#endif
        }

        /// <summary>
        /// Synchronizes the generated pause overlay visibility with the current session state.
        /// </summary>
        void RefreshOverlayPresentation() {
            if (PauseOverlayEntity == null) {
                return;
            }

            PauseOverlayEntity.Enabled = ShouldShowPauseOverlay(SessionStateMachine.CurrentState);
        }

        /// <summary>
        /// Loads the shared demo-disc main menu scene.
        /// </summary>
        void LoadResolvedMainMenuScene() {
            string resolvedSceneId = DemoDiscMainMenuSceneResolver.ResolveRuntimeSceneId();
            Core.Instance.SceneManager.RequestSceneTransition(resolvedSceneId);
        }

        /// <summary>
        /// Finds one required direct child entity at the supplied index.
        /// </summary>
        /// <param name="entity">Parent entity that should own the required child.</param>
        /// <param name="childIndex">Zero-based child index.</param>
        /// <param name="entityRole">Human-readable role used when building exception messages.</param>
        /// <returns>Matching child entity instance.</returns>
        static Entity FindRequiredChildEntity(Entity entity, int childIndex, string entityRole) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (childIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be non-negative.");
            } else if (entity.Children == null || entity.Children.Count <= childIndex) {
                throw new InvalidOperationException($"Zombislayer session could not resolve required child entity '{entityRole}'.");
            }

            return entity.Children[childIndex];
        }
    }
}
