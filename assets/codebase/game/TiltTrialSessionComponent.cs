using helengine;
using city.menu;

namespace city.game {
    /// <summary>
    /// Owns Tilt Trial timer state, finish/fail transitions, and Retry/Next/Level Select scene actions.
    /// </summary>
    public sealed class TiltTrialSessionComponent : UpdateComponent {
        const int MaxDependencyResolutionDeferralFrames = 8;

        /// <summary>
        /// Backing state machine used by the active gameplay session.
        /// </summary>
        readonly FiniteStateMachine<TiltTrialSessionState> SessionStateMachine;
        TiltTrialLevelSettingsComponent LevelSettings;
        Entity PlayerSphereEntity;
        RigidBody3DComponent PlayerRigidBody;
        Entity GoalEntity;
        global::helengine.SceneEntityTriggerObserverComponent GoalTriggerObserver;
        DemoTiltStageComponent StageComponent;
        DemoTiltFollowCameraComponent FollowCameraComponent;
        DemoTiltBallResetComponent BallResetComponent;
        DemoTiltSpeedTextComponent SpeedTextComponent;
        TextComponent TimerTextComponent;
        TextComponent CoinTextComponent;
        Entity ResultsOverlayEntity;
        TextComponent ResultsTitleTextComponent;
        TextComponent ResultsBodyTextComponent;
        Entity FailOverlayEntity;
        TextComponent FailTitleTextComponent;
        TextComponent FailBodyTextComponent;
        List<TiltTrialCollectibleCoinComponent> CollectibleCoinComponents;
        bool IsSessionStateInitialized;
        float RemainingTimeSeconds;
        float ElapsedTimeSeconds;
        float FinalTimeSeconds;
        TiltTrialMedal AwardedMedal;
        int OverlaySelectionIndex;
        int DeferredDependencyResolutionFrameCount;
        float3 FrozenPlayerPosition;
        float4 FrozenPlayerOrientation;
        bool HasFrozenPlayerPose;

        /// <summary>
        /// Initializes one Tilt Trial session controller.
        /// </summary>
        public TiltTrialSessionComponent() {
            SessionStateMachine = CreateStateMachine();
            UpdateOrder = 1;
        }

        /// <summary>
        /// Advances gameplay countdown state or overlay interaction depending on the active session state.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("TiltTrialSessionComponent requires an attached gameplay UI root entity.");
            }

            if (!TryResolveRuntimeDependenciesWhenNeeded()) {
                return;
            }

            EnsureSessionStateInitialized();

            if (SessionStateMachine.CurrentState == TiltTrialSessionState.Playing) {
                UpdatePlayingState();
                return;
            }

            MaintainFrozenPlayerPose();
            RefreshOverlayPresentation();
            if (SessionStateMachine.CurrentState == TiltTrialSessionState.Results) {
                UpdateResultsOverlay();
            } else if (SessionStateMachine.CurrentState == TiltTrialSessionState.Failed) {
                UpdateFailedOverlay();
            }
        }

        /// <summary>
        /// Resolves the medal tier awarded for one completed level clear.
        /// </summary>
        /// <param name="settings">Validated level settings.</param>
        /// <param name="clearTimeSeconds">Measured clear time in seconds.</param>
        /// <returns>Awarded medal tier.</returns>
        public static TiltTrialMedal ResolveMedal(TiltTrialLevelSettingsComponent settings, float clearTimeSeconds) {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.Validate();
            if (clearTimeSeconds <= settings.GoldTimeSeconds) {
                return TiltTrialMedal.Gold;
            } else if (clearTimeSeconds <= settings.SilverTimeSeconds) {
                return TiltTrialMedal.Silver;
            } else if (clearTimeSeconds <= settings.BronzeTimeSeconds) {
                return TiltTrialMedal.Bronze;
            }

            return TiltTrialMedal.None;
        }

        /// <summary>
        /// Resolves the next gameplay scene for the supplied current level id, or the selector scene when progression is complete.
        /// </summary>
        /// <param name="currentLevelId">Stable current logical level id.</param>
        /// <param name="levelSelectSceneId">Stable selector scene id used as the fallback target.</param>
        /// <returns>Next level scene id or the selector scene id when the current level is last or unknown.</returns>
        public static string ResolveNextSceneId(string currentLevelId, string levelSelectSceneId) {
            if (string.IsNullOrWhiteSpace(levelSelectSceneId)) {
                throw new ArgumentException("Level select scene id must be provided.", nameof(levelSelectSceneId));
            }

            IReadOnlyList<TiltTrialLevelCatalogEntry> entries = TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < entries.Count; index++) {
                if (!string.Equals(entries[index].LevelId, currentLevelId, StringComparison.Ordinal)) {
                    continue;
                }

                return index == entries.Count - 1 ? levelSelectSceneId : entries[index + 1].SceneId;
            }

            return levelSelectSceneId;
        }

        /// <summary>
        /// Formats one collectible coin progress label for the Tilt Trial gameplay HUD.
        /// </summary>
        /// <param name="collectedCoinCount">Number of coins collected so far.</param>
        /// <param name="totalCoinCount">Total number of collectible coins authored in the level.</param>
        /// <returns>Formatted HUD label.</returns>
        public static string FormatCoinProgress(int collectedCoinCount, int totalCoinCount) {
            if (collectedCoinCount < 0) {
                throw new ArgumentOutOfRangeException(nameof(collectedCoinCount), "Collected coin count must be non-negative.");
            } else if (totalCoinCount < 0) {
                throw new ArgumentOutOfRangeException(nameof(totalCoinCount), "Total coin count must be non-negative.");
            } else if (collectedCoinCount > totalCoinCount) {
                throw new ArgumentOutOfRangeException(nameof(collectedCoinCount), "Collected coin count cannot exceed the total authored coin count.");
            }

            return $"Coins {collectedCoinCount}/{totalCoinCount}";
        }

        /// <summary>
        /// Creates one uninitialized session-state machine used by runtime and unit tests.
        /// </summary>
        /// <returns>Uninitialized Tilt Trial state machine with registered states.</returns>
        public static FiniteStateMachine<TiltTrialSessionState> CreateStateMachine() {
            FiniteStateMachine<TiltTrialSessionState> machine = new FiniteStateMachine<TiltTrialSessionState>();
            machine.RegisterState(TiltTrialSessionState.Playing, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Results, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Failed, new FiniteStateDefinition<TiltTrialSessionState>());
            return machine;
        }

        void EnsureSessionStateInitialized() {
            if (IsSessionStateInitialized) {
                return;
            }

            LevelSettings.Validate();
            RemainingTimeSeconds = LevelSettings.StartTimeSeconds;
            ElapsedTimeSeconds = 0f;
            FinalTimeSeconds = 0f;
            AwardedMedal = TiltTrialMedal.None;
            OverlaySelectionIndex = 0;
            SessionStateMachine.Initialize(TiltTrialSessionState.Playing);
            SetGameplayUpdatesSuppressed(false);
            RefreshOverlayPresentation();
            IsSessionStateInitialized = true;
        }

        void UpdatePlayingState() {
            RefreshTimerText(RemainingTimeSeconds);
            RefreshCoinText();
            if (WasReturnPressed()) {
                LoadScene(TiltTrialSceneIds.LevelSelectSceneId);
                return;
            }

            double elapsedSeconds = Core.Instance.FrameDeltaSeconds;
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(Core.Instance.FrameDeltaSeconds), "Tilt Trial session updates require a finite non-negative frame delta.");
            } else if (elapsedSeconds == 0d) {
                return;
            }

            ElapsedTimeSeconds += (float)elapsedSeconds;
            RemainingTimeSeconds = Math.Max(0f, RemainingTimeSeconds - (float)elapsedSeconds);
            CollectCoinsIfNeeded();
            if (IsGoalReached()) {
                EnterResultsState();
                return;
            }
            if (RemainingTimeSeconds <= 0f) {
                EnterFailedState();
                return;
            }

            RefreshTimerText(RemainingTimeSeconds);
            RefreshCoinText();
        }

        void EnterResultsState() {
            FinalTimeSeconds = ElapsedTimeSeconds;
            AwardedMedal = ResolveMedal(LevelSettings, FinalTimeSeconds);
            OverlaySelectionIndex = 0;
            CaptureFrozenPlayerPose();
            SetGameplayUpdatesSuppressed(true);
            SessionStateMachine.TryChangeState(TiltTrialSessionState.Results);
            RefreshOverlayPresentation();
        }

        void EnterFailedState() {
            FinalTimeSeconds = ElapsedTimeSeconds;
            AwardedMedal = TiltTrialMedal.None;
            OverlaySelectionIndex = 0;
            CaptureFrozenPlayerPose();
            SetGameplayUpdatesSuppressed(true);
            SessionStateMachine.TryChangeState(TiltTrialSessionState.Failed);
            RefreshOverlayPresentation();
        }

        void UpdateResultsOverlay() {
            if (WasNavigatePreviousPressed()) {
                OverlaySelectionIndex = OverlaySelectionIndex <= 0 ? 2 : OverlaySelectionIndex - 1;
            } else if (WasNavigateNextPressed()) {
                OverlaySelectionIndex = OverlaySelectionIndex >= 2 ? 0 : OverlaySelectionIndex + 1;
            } else if (WasAcceptPressed()) {
                if (OverlaySelectionIndex == 0) {
                    LoadScene(LevelSettings.SceneId);
                } else if (OverlaySelectionIndex == 1) {
                    LoadScene(ResolveNextSceneId(LevelSettings.LevelId, TiltTrialSceneIds.LevelSelectSceneId));
                } else {
                    LoadScene(TiltTrialSceneIds.LevelSelectSceneId);
                }
                return;
            } else if (WasReturnPressed()) {
                LoadScene(TiltTrialSceneIds.LevelSelectSceneId);
                return;
            }

            ResultsTitleTextComponent.Text = $"Clear - {AwardedMedal}";
            ResultsBodyTextComponent.Text = $"Time {TiltTrialLevelSelectComponent.FormatTimerSeconds(FinalTimeSeconds)}\n\n{BuildResultsOptionLine(0, "Retry")}\n{BuildResultsOptionLine(1, "Next")}\n{BuildResultsOptionLine(2, "Level Select")}";
        }

        void UpdateFailedOverlay() {
            if (WasNavigatePreviousPressed()) {
                OverlaySelectionIndex = OverlaySelectionIndex <= 0 ? 1 : OverlaySelectionIndex - 1;
            } else if (WasNavigateNextPressed()) {
                OverlaySelectionIndex = OverlaySelectionIndex >= 1 ? 0 : OverlaySelectionIndex + 1;
            } else if (WasAcceptPressed()) {
                if (OverlaySelectionIndex == 0) {
                    LoadScene(LevelSettings.SceneId);
                } else {
                    LoadScene(TiltTrialSceneIds.LevelSelectSceneId);
                }
                return;
            } else if (WasReturnPressed()) {
                LoadScene(TiltTrialSceneIds.LevelSelectSceneId);
                return;
            }

            FailTitleTextComponent.Text = "Time Up";
            FailBodyTextComponent.Text = $"{BuildResultsOptionLine(0, "Retry")}\n{BuildResultsOptionLine(1, "Level Select")}";
        }

        string BuildResultsOptionLine(int optionIndex, string label) {
            return (OverlaySelectionIndex == optionIndex ? "> " : "  ") + label;
        }

        void RefreshOverlayPresentation() {
            if (TimerTextComponent != null) {
                RefreshTimerText(RemainingTimeSeconds);
            }
            if (CoinTextComponent != null) {
                RefreshCoinText();
            }
            if (ResultsOverlayEntity != null) {
                ResultsOverlayEntity.Enabled = SessionStateMachine.CurrentState == TiltTrialSessionState.Results;
            }
            if (FailOverlayEntity != null) {
                FailOverlayEntity.Enabled = SessionStateMachine.CurrentState == TiltTrialSessionState.Failed;
            }
        }

        void RefreshTimerText(float remainingTimeSeconds) {
            TimerTextComponent.Text = TiltTrialLevelSelectComponent.FormatTimerSeconds(remainingTimeSeconds);
        }

        void RefreshCoinText() {
            CoinTextComponent.Text = FormatCoinProgress(ResolveCollectedCoinCount(), ResolveTotalCoinCount());
        }

        void CaptureFrozenPlayerPose() {
            if (PlayerSphereEntity == null) {
                return;
            }

            FrozenPlayerPosition = PlayerSphereEntity.LocalPosition;
            FrozenPlayerOrientation = PlayerSphereEntity.LocalOrientation;
            HasFrozenPlayerPose = true;
            MaintainFrozenPlayerPose();
        }

        void MaintainFrozenPlayerPose() {
            if (!HasFrozenPlayerPose || PlayerSphereEntity == null || PlayerRigidBody == null) {
                return;
            }

            PlayerSphereEntity.LocalPosition = FrozenPlayerPosition;
            PlayerSphereEntity.LocalOrientation = FrozenPlayerOrientation;
            PlayerRigidBody.SetLinearVelocity(float3.Zero);
            PlayerRigidBody.SetAngularVelocity(float3.Zero);
            if (Core.Instance?.PhysicsRuntime is IPhysicsBodySynchronizationRuntime3D physicsRuntime) {
                physicsRuntime.SynchronizeDynamicBody(PlayerSphereEntity);
            }
        }

        bool IsGoalReached() {
            if (GoalEntity == null || GoalTriggerObserver == null || PlayerSphereEntity == null) {
                return false;
            }

            return GoalTriggerObserver.GetIsTriggered();
        }

        void CollectCoinsIfNeeded() {
            int coinCount = ResolveTotalCoinCount();
            for (int coinIndex = 0; coinIndex < coinCount; coinIndex++) {
                TiltTrialCollectibleCoinComponent coinComponent = CollectibleCoinComponents[coinIndex];
                if (coinComponent == null || coinComponent.IsCollected || coinComponent.Parent == null) {
                    continue;
                }

                global::helengine.SceneEntityTriggerObserverComponent triggerObserver = FindRequiredComponent<global::helengine.SceneEntityTriggerObserverComponent>(coinComponent.Parent);
                if (triggerObserver.GetWasEnteredThisFrame()) {
                    coinComponent.Collect();
                }
            }
        }

        int ResolveCollectedCoinCount() {
            int totalCoinCount = ResolveTotalCoinCount();
            int collectedCoinCount = 0;
            for (int coinIndex = 0; coinIndex < totalCoinCount; coinIndex++) {
                if (CollectibleCoinComponents[coinIndex] != null && CollectibleCoinComponents[coinIndex].IsCollected) {
                    collectedCoinCount++;
                }
            }

            return collectedCoinCount;
        }

        int ResolveTotalCoinCount() {
            return CollectibleCoinComponents == null ? 0 : CollectibleCoinComponents.Count;
        }

        void SetGameplayUpdatesSuppressed(bool updatesAreSuppressed) {
            if (StageComponent != null) {
                StageComponent.UpdatesAreSuppressed = updatesAreSuppressed;
            }
            if (FollowCameraComponent != null) {
                FollowCameraComponent.UpdatesAreSuppressed = updatesAreSuppressed;
            }
            if (BallResetComponent != null) {
                BallResetComponent.UpdatesAreSuppressed = updatesAreSuppressed;
            }
            if (SpeedTextComponent != null) {
                SpeedTextComponent.UpdatesAreSuppressed = updatesAreSuppressed;
            }
        }

        bool TryResolveRuntimeDependenciesWhenNeeded() {
            List<string> missingDependencies = new List<string>();
            if (LevelSettings == null) {
                LevelSettings = FindOptionalComponentAcrossScene<TiltTrialLevelSettingsComponent>();
                if (LevelSettings == null) {
                    missingDependencies.Add("level settings");
                }
            }
            if (PlayerSphereEntity == null) {
                PlayerSphereEntity = FindOptionalEntityWithComponentsAcrossScene<RigidBody3DComponent, SphereCollider3DComponent>();
                if (PlayerSphereEntity == null) {
                    missingDependencies.Add("player sphere");
                }
            }
            if (PlayerRigidBody == null && PlayerSphereEntity != null) {
                PlayerRigidBody = TryFindComponent<RigidBody3DComponent>(PlayerSphereEntity);
            }
            if (PlayerRigidBody == null) {
                missingDependencies.Add("player rigid body");
            }
            if (GoalEntity == null) {
                GoalEntity = FindOptionalEntityWithComponentAcrossScene<TiltTrialGoalComponent>();
                if (GoalEntity == null) {
                    missingDependencies.Add("goal entity");
                }
            }
            if (GoalTriggerObserver == null && GoalEntity != null) {
                GoalTriggerObserver = TryFindComponent<global::helengine.SceneEntityTriggerObserverComponent>(GoalEntity);
            }
            if (GoalTriggerObserver == null) {
                missingDependencies.Add("goal trigger observer");
            }
            if (StageComponent == null) {
                StageComponent = FindOptionalComponentAcrossScene<DemoTiltStageComponent>();
            }
            if (FollowCameraComponent == null) {
                FollowCameraComponent = FindOptionalComponentAcrossScene<DemoTiltFollowCameraComponent>();
            }
            if (BallResetComponent == null) {
                BallResetComponent = FindOptionalComponentAcrossScene<DemoTiltBallResetComponent>();
            }
            if (SpeedTextComponent == null) {
                SpeedTextComponent = FindOptionalComponentAcrossScene<DemoTiltSpeedTextComponent>();
            }
            if (TimerTextComponent == null) {
                Entity timerTextEntity = TryFindChildEntity(Parent, 0);
                TimerTextComponent = TryFindComponent<TextComponent>(timerTextEntity);
            }
            if (TimerTextComponent == null) {
                missingDependencies.Add("timer text");
            }
            if (CoinTextComponent == null) {
                Entity coinTextEntity = TryFindChildEntity(Parent, 4);
                CoinTextComponent = TryFindComponent<TextComponent>(coinTextEntity);
            }
            if (CoinTextComponent == null) {
                missingDependencies.Add("coin text");
            }
            if (ResultsOverlayEntity == null) {
                ResultsOverlayEntity = TryFindChildEntity(Parent, 2);
            }
            if (ResultsOverlayEntity == null) {
                missingDependencies.Add("results overlay");
            }
            if (ResultsTitleTextComponent == null) {
                Entity resultsTitleEntity = TryFindChildEntity(ResultsOverlayEntity, 0);
                ResultsTitleTextComponent = TryFindComponent<TextComponent>(resultsTitleEntity);
            }
            if (ResultsTitleTextComponent == null) {
                missingDependencies.Add("results title text");
            }
            if (ResultsBodyTextComponent == null) {
                Entity resultsBodyEntity = TryFindChildEntity(ResultsOverlayEntity, 1);
                ResultsBodyTextComponent = TryFindComponent<TextComponent>(resultsBodyEntity);
            }
            if (ResultsBodyTextComponent == null) {
                missingDependencies.Add("results body text");
            }
            if (FailOverlayEntity == null) {
                FailOverlayEntity = TryFindChildEntity(Parent, 3);
            }
            if (FailOverlayEntity == null) {
                missingDependencies.Add("fail overlay");
            }
            if (FailTitleTextComponent == null) {
                Entity failTitleEntity = TryFindChildEntity(FailOverlayEntity, 0);
                FailTitleTextComponent = TryFindComponent<TextComponent>(failTitleEntity);
            }
            if (FailTitleTextComponent == null) {
                missingDependencies.Add("fail title text");
            }
            if (FailBodyTextComponent == null) {
                Entity failBodyEntity = TryFindChildEntity(FailOverlayEntity, 1);
                FailBodyTextComponent = TryFindComponent<TextComponent>(failBodyEntity);
            }
            if (FailBodyTextComponent == null) {
                missingDependencies.Add("fail body text");
            }
            if (CollectibleCoinComponents == null) {
                CollectibleCoinComponents = FindComponentsAcrossScene<TiltTrialCollectibleCoinComponent>();
            }

            if (missingDependencies.Count == 0) {
                DeferredDependencyResolutionFrameCount = 0;
                return true;
            }

            DeferredDependencyResolutionFrameCount++;
            if (DeferredDependencyResolutionFrameCount <= MaxDependencyResolutionDeferralFrames) {
                return false;
            }

            throw new InvalidOperationException($"Tilt Trial session could not resolve required runtime dependencies: {string.Join(", ", missingDependencies)}.");
        }

        bool WasNavigatePreviousPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Left)
                || inputSystem.WasKeyPressed(Keys.Up)
                || inputSystem.WasKeyPressed(Keys.A)
                || inputSystem.WasKeyPressed(Keys.W)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.DPadLeft)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.DPadUp);
        }

        bool WasNavigateNextPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Right)
                || inputSystem.WasKeyPressed(Keys.Down)
                || inputSystem.WasKeyPressed(Keys.D)
                || inputSystem.WasKeyPressed(Keys.S)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.DPadRight)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.DPadDown);
        }

        bool WasAcceptPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Enter)
                || inputSystem.WasKeyPressed(Keys.Space)
                || Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept);
        }

        bool WasReturnPressed() {
            InputSystem inputSystem = Core.Instance.Input;
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Escape) || inputSystem.WasKeyPressed(Keys.Back)) {
                return true;
            }
#endif
            return DemoDiscReturnInputUtils.WasReturnPressed(inputSystem);
        }

        void LoadScene(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new InvalidOperationException("Tilt Trial session scene loads require a valid target scene id.");
            }

            Core.Instance.SceneManager.LoadScene(sceneId, SceneLoadMode.Single);
        }

        TComponent FindRequiredComponentAcrossScene<TComponent>() where TComponent : Component {
            TComponent component = FindOptionalComponentAcrossScene<TComponent>();
            if (component != null) {
                return component;
            }

            throw new InvalidOperationException($"Tilt Trial session could not resolve required component '{typeof(TComponent).Name}'.");
        }

        TComponent FindOptionalComponentAcrossScene<TComponent>() where TComponent : Component {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                TComponent match = TryFindComponentRecursive<TComponent>(entities[entityIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        List<TComponent> FindComponentsAcrossScene<TComponent>() where TComponent : Component {
            List<TComponent> matches = new List<TComponent>();
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                CollectComponentsRecursive(entities[entityIndex], matches);
            }

            return matches;
        }

        Entity FindRequiredEntityWithComponentAcrossScene<TComponent>() where TComponent : Component {
            Entity entity = FindOptionalEntityWithComponentAcrossScene<TComponent>();
            if (entity != null) {
                return entity;
            }

            throw new InvalidOperationException($"Tilt Trial session could not resolve required entity with component '{typeof(TComponent).Name}'.");
        }

        Entity FindOptionalEntityWithComponentAcrossScene<TComponent>() where TComponent : Component {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity match = TryFindEntityWithComponentRecursive<TComponent>(entities[entityIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        Entity FindRequiredEntityWithComponentsAcrossScene<TFirstComponent, TSecondComponent>()
            where TFirstComponent : Component
            where TSecondComponent : Component {
            Entity entity = FindOptionalEntityWithComponentsAcrossScene<TFirstComponent, TSecondComponent>();
            if (entity != null) {
                return entity;
            }

            throw new InvalidOperationException($"Tilt Trial session could not resolve required entity with components '{typeof(TFirstComponent).Name}' and '{typeof(TSecondComponent).Name}'.");
        }

        Entity FindOptionalEntityWithComponentsAcrossScene<TFirstComponent, TSecondComponent>()
            where TFirstComponent : Component
            where TSecondComponent : Component {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity match = TryFindEntityWithComponentsRecursive<TFirstComponent, TSecondComponent>(entities[entityIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        static TComponent FindRequiredComponent<TComponent>(Entity entity) where TComponent : Component {
            TComponent component = TryFindComponent<TComponent>(entity);
            if (component != null) {
                return component;
            }

            throw new InvalidOperationException($"Tilt Trial session could not resolve component '{typeof(TComponent).Name}'.");
        }

        static TComponent TryFindComponent<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null || entity.Components == null) {
                return null;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                Component candidateComponent = entity.Components[componentIndex];
                if (candidateComponent is TComponent) {
                    return (TComponent)candidateComponent;
                }
            }

            return null;
        }

        static TComponent TryFindComponentRecursive<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null) {
                return null;
            }
            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    Component candidateComponent = entity.Components[componentIndex];
                    if (candidateComponent is TComponent) {
                        return (TComponent)candidateComponent;
                    }
                }
            }
            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                TComponent match = TryFindComponentRecursive<TComponent>(entity.Children[childIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        static void CollectComponentsRecursive<TComponent>(Entity entity, List<TComponent> matches) where TComponent : Component {
            if (entity == null) {
                return;
            } else if (matches == null) {
                throw new ArgumentNullException(nameof(matches));
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    Component candidateComponent = entity.Components[componentIndex];
                    if (candidateComponent is TComponent) {
                        matches.Add((TComponent)candidateComponent);
                    }
                }
            }
            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                CollectComponentsRecursive(entity.Children[childIndex], matches);
            }
        }

        static Entity TryFindEntityWithComponentRecursive<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null) {
                return null;
            }
            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is TComponent) {
                        return entity;
                    }
                }
            }
            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                Entity match = TryFindEntityWithComponentRecursive<TComponent>(entity.Children[childIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds one required child entity at the supplied authored child index.
        /// </summary>
        /// <param name="entity">Parent entity whose child collection should be indexed.</param>
        /// <param name="childIndex">Zero-based child index in the generated hierarchy.</param>
        /// <param name="description">Human-readable child description used for failure messages.</param>
        /// <returns>Required child entity at the supplied index.</returns>
        static Entity FindRequiredChildEntity(Entity entity, int childIndex, string description) {
            Entity childEntity = TryFindChildEntity(entity, childIndex);
            if (childEntity != null) {
                return childEntity;
            }

            throw new InvalidOperationException($"Tilt Trial session could not resolve required entity '{description}'.");
        }

        static Entity TryFindChildEntity(Entity entity, int childIndex) {
            if (entity == null) {
                return null;
            } else if (childIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be non-negative.");
            }

            if (entity.Children == null || childIndex >= entity.Children.Count) {
                return null;
            }

            return entity.Children[childIndex];
        }

        /// <summary>
        /// Recursively finds one entity that owns both required component types.
        /// </summary>
        /// <typeparam name="TFirstComponent">First required component type.</typeparam>
        /// <typeparam name="TSecondComponent">Second required component type.</typeparam>
        /// <param name="entity">Candidate entity root.</param>
        /// <returns>First matching entity, or <c>null</c> when none is present.</returns>
        static Entity TryFindEntityWithComponentsRecursive<TFirstComponent, TSecondComponent>(Entity entity)
            where TFirstComponent : Component
            where TSecondComponent : Component {
            if (entity == null) {
                return null;
            }

            if (HasComponent<TFirstComponent>(entity) && HasComponent<TSecondComponent>(entity)) {
                return entity;
            }

            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                Entity match = TryFindEntityWithComponentsRecursive<TFirstComponent, TSecondComponent>(entity.Children[childIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns whether the supplied entity directly owns the requested component type.
        /// </summary>
        /// <typeparam name="TComponent">Required direct component type.</typeparam>
        /// <param name="entity">Entity to inspect.</param>
        /// <returns>True when the entity directly owns the component type; otherwise false.</returns>
        static bool HasComponent<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null || entity.Components == null) {
                return false;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TComponent) {
                    return true;
                }
            }

            return false;
        }
    }
}
