using helengine;

namespace city.game {
    /// <summary>
    /// Owns the high-level Tilt Play title, options, and level-selector state contract.
    /// </summary>
    public sealed class TiltPlayMenuComponent : UpdateComponent {
        /// <summary>
        /// Number of title actions that participate in wrapped vertical navigation.
        /// </summary>
        const int TitleActionCount = 3;

        /// <summary>
        /// Raw left-stick magnitude required to emit one title-menu navigation request.
        /// </summary>
        const short GamepadStickNavigationThreshold = 16384;

        /// <summary>
        /// Generated title panel resolved through its stable presentation role.
        /// </summary>
        Entity TitlePanelEntity;

        /// <summary>
        /// Generated options placeholder panel resolved through its stable presentation role.
        /// </summary>
        Entity OptionsPanelEntity;

        /// <summary>
        /// Generated existing-level-selector panel resolved through its stable presentation role.
        /// </summary>
        Entity LevelSelectPanelEntity;

        /// <summary>
        /// Background that presents the focused Play title action.
        /// </summary>
        RoundedRectComponent PlayButtonBackground;

        /// <summary>
        /// Background that presents the focused Options title action.
        /// </summary>
        RoundedRectComponent OptionsButtonBackground;

        /// <summary>
        /// Background that presents the focused Demo Disc return title action.
        /// </summary>
        RoundedRectComponent DemoDiscButtonBackground;

        /// <summary>
        /// Existing selector controller whose input ownership follows the active panel state.
        /// </summary>
        TiltTrialLevelSelectComponent LevelSelectComponent;

        /// <summary>
        /// Tracks whether the state machine has been initialized after attachment to the generated shell.
        /// </summary>
        bool IsStateInitialized;

        /// <summary>
        /// Zero-based title action currently selected by keyboard and gamepad navigation.
        /// </summary>
        int SelectedTitleActionIndex;

        /// <summary>
        /// Initializes one Tilt Play menu controller with the title as its initial state.
        /// </summary>
        public TiltPlayMenuComponent() {
            StateMachine = CreateStateMachine();
        }

        /// <summary>
        /// Gets the state machine that represents the currently visible Tilt Play menu panel.
        /// </summary>
        public FiniteStateMachine<TiltPlayMenuState> StateMachine { get; }

        /// <summary>
        /// Advances high-level Tilt Play navigation and updates the visible generated panel.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("Tilt Play menu controller requires an attached shell entity.");
            }

            ResolveRuntimeDependenciesWhenNeeded();
            InitializeStateWhenNeeded();
            HandleInput();
            ApplyPresentation();
        }

        /// <summary>
        /// Creates an uninitialized Tilt Play menu state machine with every supported panel registered.
        /// </summary>
        /// <returns>Uninitialized menu state machine ready for title-state initialization.</returns>
        public static FiniteStateMachine<TiltPlayMenuState> CreateStateMachine() {
            FiniteStateMachine<TiltPlayMenuState> machine = new FiniteStateMachine<TiltPlayMenuState>();
            machine.RegisterState(TiltPlayMenuState.Title, new FiniteStateDefinition<TiltPlayMenuState>());
            machine.RegisterState(TiltPlayMenuState.Options, new FiniteStateDefinition<TiltPlayMenuState>());
            machine.RegisterState(TiltPlayMenuState.LevelSelect, new FiniteStateDefinition<TiltPlayMenuState>());
            return machine;
        }

        /// <summary>
        /// Resolves the panel state selected by one title action.
        /// </summary>
        /// <param name="action">Title action requesting a panel transition.</param>
        /// <returns>Target panel state for the supplied action.</returns>
        public static TiltPlayMenuState ResolveActionState(TiltPlayMenuAction action) {
            if (action == TiltPlayMenuAction.Play) {
                return TiltPlayMenuState.LevelSelect;
            } else if (action == TiltPlayMenuAction.Options) {
                return TiltPlayMenuState.Options;
            }

            throw new ArgumentOutOfRangeException(nameof(action));
        }

        /// <summary>
        /// Resolves the title state reached when a visible Tilt Play submenu receives a Back action.
        /// </summary>
        /// <param name="currentState">Current visible Tilt Play menu panel.</param>
        /// <returns>The title state for supported submenu states.</returns>
        public static TiltPlayMenuState ResolveBackState(TiltPlayMenuState currentState) {
            if (currentState == TiltPlayMenuState.Options || currentState == TiltPlayMenuState.LevelSelect) {
                return TiltPlayMenuState.Title;
            }

            throw new ArgumentOutOfRangeException(nameof(currentState));
        }

        /// <summary>
        /// Determines whether the existing level selector may consume input for the visible Tilt Play menu state.
        /// </summary>
        /// <param name="currentState">Current visible Tilt Play menu panel.</param>
        /// <returns>True only while the level-selector panel is visible.</returns>
        public static bool ShouldLevelSelectorProcessInput(TiltPlayMenuState currentState) {
            return currentState == TiltPlayMenuState.LevelSelect;
        }

        /// <summary>
        /// Resolves a wrapped title action index after one vertical navigation request.
        /// </summary>
        /// <param name="currentIndex">Current zero-based title action index.</param>
        /// <param name="moveForward">True to select the following action; otherwise selects the preceding action.</param>
        /// <returns>Wrapped zero-based title action index.</returns>
        public static int ResolveTitleActionIndexAfterNavigation(int currentIndex, bool moveForward) {
            if (currentIndex < 0 || currentIndex >= TitleActionCount) {
                throw new ArgumentOutOfRangeException(nameof(currentIndex));
            }

            if (moveForward) {
                return currentIndex >= TitleActionCount - 1 ? 0 : currentIndex + 1;
            }

            return currentIndex <= 0 ? TitleActionCount - 1 : currentIndex - 1;
        }

        /// <summary>
        /// Applies one semantic Tilt Play menu action requested by input or a pointer action host.
        /// </summary>
        /// <param name="action">Action to apply to the active Tilt Play menu state.</param>
        public void HandleAction(TiltPlayMenuAction action) {
            InitializeStateWhenNeeded();
            TiltPlayMenuState currentState = StateMachine.CurrentState;
            if (action == TiltPlayMenuAction.BackToDemoDisc) {
                Core.Instance.SceneManager.RequestSceneTransition(global::city.menu.DemoDiscMainMenuSceneResolver.ResolveRuntimeSceneId());
                return;
            } else if (action == TiltPlayMenuAction.Back) {
                if (currentState != TiltPlayMenuState.Title) {
                    StateMachine.TryChangeState(ResolveBackState(currentState));
                }

                return;
            } else if (currentState == TiltPlayMenuState.Title) {
                StateMachine.TryChangeState(ResolveActionState(action));
            }
        }

        /// <summary>
        /// Resolves generated shell dependencies required to control visibility and selector input.
        /// </summary>
        void ResolveRuntimeDependenciesWhenNeeded() {
            if (TitlePanelEntity != null && OptionsPanelEntity != null && LevelSelectPanelEntity != null && LevelSelectComponent != null
                && PlayButtonBackground != null && OptionsButtonBackground != null && DemoDiscButtonBackground != null) {
                return;
            }

            TitlePanelEntity = FindRequiredNamedEntity(Parent, "TiltPlayTitlePanel");
            OptionsPanelEntity = FindRequiredNamedEntity(Parent, "TiltPlayOptionsPanel");
            LevelSelectPanelEntity = FindRequiredNamedEntity(Parent, "TiltPlayLevelSelectPanel");
            LevelSelectComponent = FindRequiredComponent<TiltTrialLevelSelectComponent>(LevelSelectPanelEntity);
            PlayButtonBackground = FindRequiredComponent<RoundedRectComponent>(FindRequiredNamedEntity(Parent, "TiltPlayPlayButton"));
            OptionsButtonBackground = FindRequiredComponent<RoundedRectComponent>(FindRequiredNamedEntity(Parent, "TiltPlayOptionsButton"));
            DemoDiscButtonBackground = FindRequiredComponent<RoundedRectComponent>(FindRequiredNamedEntity(Parent, "TiltPlayDemoDiscButton"));
        }

        /// <summary>
        /// Initializes the title state once the menu enters the runtime hierarchy.
        /// </summary>
        void InitializeStateWhenNeeded() {
            if (IsStateInitialized) {
                return;
            }

            StateMachine.Initialize(TiltPlayMenuState.Title);
            SelectedTitleActionIndex = 0;
            IsStateInitialized = true;
        }

        /// <summary>
        /// Maps physical input to the active Tilt Play menu state.
        /// </summary>
        void HandleInput() {
            InputSystem inputSystem = Core.Instance.Input;
            if (StateMachine.CurrentState == TiltPlayMenuState.Title) {
                if (WasNavigatePreviousPressed(inputSystem)) {
                    SelectedTitleActionIndex = ResolveTitleActionIndexAfterNavigation(SelectedTitleActionIndex, false);
                } else if (WasNavigateNextPressed(inputSystem)) {
                    SelectedTitleActionIndex = ResolveTitleActionIndexAfterNavigation(SelectedTitleActionIndex, true);
                } else if (WasAcceptPressed(inputSystem)) {
                    HandleAction(ResolveSelectedTitleAction());
                } else if (WasBackPressed(inputSystem)) {
                    HandleAction(TiltPlayMenuAction.BackToDemoDisc);
                }
            } else if (StateMachine.CurrentState == TiltPlayMenuState.Options && (WasAcceptPressed(inputSystem) || WasBackPressed(inputSystem))) {
                HandleAction(TiltPlayMenuAction.Back);
            } else if (StateMachine.CurrentState == TiltPlayMenuState.LevelSelect && WasBackPressed(inputSystem)) {
                HandleAction(TiltPlayMenuAction.Back);
            }
        }

        /// <summary>
        /// Applies panel visibility and routes input ownership to the active panel.
        /// </summary>
        void ApplyPresentation() {
            TiltPlayMenuState currentState = StateMachine.CurrentState;
            TitlePanelEntity.Enabled = currentState == TiltPlayMenuState.Title;
            OptionsPanelEntity.Enabled = currentState == TiltPlayMenuState.Options;
            LevelSelectPanelEntity.Enabled = currentState == TiltPlayMenuState.LevelSelect;
            LevelSelectComponent.AcceptsInput = ShouldLevelSelectorProcessInput(currentState);
            ApplyTitleActionSelection();
        }

        /// <summary>
        /// Applies a purple focus treatment to the title action selected by keyboard or gamepad navigation.
        /// </summary>
        void ApplyTitleActionSelection() {
            bool isTitleVisible = StateMachine.CurrentState == TiltPlayMenuState.Title;
            ApplyTitleActionButtonStyle(PlayButtonBackground, isTitleVisible && SelectedTitleActionIndex == 0);
            ApplyTitleActionButtonStyle(OptionsButtonBackground, isTitleVisible && SelectedTitleActionIndex == 1);
            ApplyTitleActionButtonStyle(DemoDiscButtonBackground, isTitleVisible && SelectedTitleActionIndex == 2);
        }

        /// <summary>
        /// Applies either the focused purple title-button treatment or the neutral title-button treatment.
        /// </summary>
        /// <param name="background">Button background to style.</param>
        /// <param name="isSelected">Whether the button owns current title focus.</param>
        void ApplyTitleActionButtonStyle(RoundedRectComponent background, bool isSelected) {
            if (background == null) {
                throw new ArgumentNullException(nameof(background));
            }

            if (isSelected) {
                background.FillColor = new byte4(102, 56, 160, 255);
                background.BorderColor = new byte4(190, 142, 255, 255);
            } else {
                background.FillColor = new byte4(40, 58, 87, 255);
                background.BorderColor = new byte4(109, 138, 170, 255);
            }
        }

        /// <summary>
        /// Resolves the semantic action represented by the currently focused title option.
        /// </summary>
        /// <returns>Focused title option action.</returns>
        TiltPlayMenuAction ResolveSelectedTitleAction() {
            if (SelectedTitleActionIndex == 0) {
                return TiltPlayMenuAction.Play;
            } else if (SelectedTitleActionIndex == 1) {
                return TiltPlayMenuAction.Options;
            }

            return TiltPlayMenuAction.BackToDemoDisc;
        }

        /// <summary>
        /// Returns whether the current frame requested backward vertical navigation.
        /// </summary>
        /// <param name="inputSystem">Input source for the current runtime frame.</param>
        /// <returns>True when previous-option navigation was requested.</returns>
        bool WasNavigatePreviousPressed(InputSystem inputSystem) {
            return inputSystem.WasKeyPressed(Keys.Up)
                || inputSystem.WasKeyPressed(Keys.W)
                || global::city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadUp)
                || (global::city.menu.DemoDiscGamepadInput.GetLeftStickY(inputSystem) <= -GamepadStickNavigationThreshold
                    && global::city.menu.DemoDiscGamepadInput.GetPreviousLeftStickY(inputSystem) > -GamepadStickNavigationThreshold);
        }

        /// <summary>
        /// Returns whether the current frame requested forward vertical navigation.
        /// </summary>
        /// <param name="inputSystem">Input source for the current runtime frame.</param>
        /// <returns>True when next-option navigation was requested.</returns>
        bool WasNavigateNextPressed(InputSystem inputSystem) {
            return inputSystem.WasKeyPressed(Keys.Down)
                || inputSystem.WasKeyPressed(Keys.S)
                || global::city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadDown)
                || (global::city.menu.DemoDiscGamepadInput.GetLeftStickY(inputSystem) >= GamepadStickNavigationThreshold
                    && global::city.menu.DemoDiscGamepadInput.GetPreviousLeftStickY(inputSystem) < GamepadStickNavigationThreshold);
        }

        /// <summary>
        /// Returns whether the current frame requested activation of the focused action.
        /// </summary>
        /// <param name="inputSystem">Input source for the current runtime frame.</param>
        /// <returns>True when accepting the focused action was requested.</returns>
        bool WasAcceptPressed(InputSystem inputSystem) {
            return inputSystem.WasKeyPressed(Keys.Enter)
                || inputSystem.WasKeyPressed(Keys.J)
                || inputSystem.WasKeyPressed(Keys.Space)
                || global::city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South)
                || Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept);
        }

        /// <summary>
        /// Returns whether the current frame requested back navigation.
        /// </summary>
        /// <param name="inputSystem">Input source for the current runtime frame.</param>
        /// <returns>True when return navigation was requested.</returns>
        bool WasBackPressed(InputSystem inputSystem) {
            return inputSystem.WasKeyPressed(Keys.Escape)
                || inputSystem.WasKeyPressed(Keys.Back)
                || inputSystem.WasKeyPressed(Keys.K)
                || global::city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.East);
        }

        /// <summary>
        /// Finds one required descendant entity by its generated stable name.
        /// </summary>
        /// <param name="root">Hierarchy root to search.</param>
        /// <param name="entityName">Generated entity name to resolve.</param>
        /// <returns>Required matching entity.</returns>
        static Entity FindRequiredNamedEntity(Entity root, string entityName) {
            if (root == null) {
                throw new ArgumentNullException(nameof(root));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            if (HasPresentationRole(root, entityName)) {
                return root;
            }

            if (root.Children != null) {
                for (int childIndex = 0; childIndex < root.Children.Count; childIndex++) {
                    Entity result = FindNamedEntity(root.Children[childIndex], entityName);
                    if (result != null) {
                        return result;
                    }
                }
            }

            throw new InvalidOperationException($"Tilt Play menu could not resolve required entity '{entityName}'.");
        }

        /// <summary>
        /// Searches one hierarchy branch for an entity with the supplied generated name.
        /// </summary>
        /// <param name="root">Hierarchy branch to search.</param>
        /// <param name="entityName">Generated entity name to resolve.</param>
        /// <returns>Matching entity, or null when absent from the branch.</returns>
        static Entity FindNamedEntity(Entity root, string entityName) {
            if (root == null) {
                return null;
            } else if (HasPresentationRole(root, entityName)) {
                return root;
            }

            if (root.Children != null) {
                for (int childIndex = 0; childIndex < root.Children.Count; childIndex++) {
                    Entity result = FindNamedEntity(root.Children[childIndex], entityName);
                    if (result != null) {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether one generated entity carries the supplied stable presentation role.
        /// </summary>
        /// <param name="entity">Generated entity to inspect.</param>
        /// <param name="role">Stable role identifier expected by the menu controller.</param>
        /// <returns>True when the entity carries the requested role.</returns>
        static bool HasPresentationRole(Entity entity, string role) {
            if (entity == null || entity.Components == null) {
                return false;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TiltTrialPresentationRoleComponent roleComponent
                    && string.Equals(roleComponent.Role, role, StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds one required component on an entity or one of its descendants.
        /// </summary>
        /// <typeparam name="TComponent">Required component type.</typeparam>
        /// <param name="root">Hierarchy root to search.</param>
        /// <returns>Required matching component.</returns>
        static TComponent FindRequiredComponent<TComponent>(Entity root) where TComponent : Component {
            if (root.Components != null) {
                for (int componentIndex = 0; componentIndex < root.Components.Count; componentIndex++) {
                    if (root.Components[componentIndex] is TComponent component) {
                        return component;
                    }
                }
            }

            if (root.Children != null) {
                for (int childIndex = 0; childIndex < root.Children.Count; childIndex++) {
                    TComponent result = FindComponent<TComponent>(root.Children[childIndex]);
                    if (result != null) {
                        return result;
                    }
                }
            }

            throw new InvalidOperationException($"Tilt Play menu could not resolve required component '{typeof(TComponent).Name}'.");
        }

        /// <summary>
        /// Searches one hierarchy branch for the supplied component type.
        /// </summary>
        /// <typeparam name="TComponent">Component type to resolve.</typeparam>
        /// <param name="root">Hierarchy branch to search.</param>
        /// <returns>Matching component, or null when absent from the branch.</returns>
        static TComponent FindComponent<TComponent>(Entity root) where TComponent : Component {
            if (root.Components != null) {
                for (int componentIndex = 0; componentIndex < root.Components.Count; componentIndex++) {
                    if (root.Components[componentIndex] is TComponent component) {
                        return component;
                    }
                }
            }

            if (root.Children != null) {
                for (int childIndex = 0; childIndex < root.Children.Count; childIndex++) {
                    TComponent result = FindComponent<TComponent>(root.Children[childIndex]);
                    if (result != null) {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
