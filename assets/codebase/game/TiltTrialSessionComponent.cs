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
        TiltTrialLevelCatalogEntry CurrentLevel;
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
        TextComponent TargetTimesTextComponent;
        Entity ResultsOverlayEntity;
        TextComponent ResultsTitleTextComponent;
        TextComponent ResultsBodyTextComponent;
        /// <summary>
        /// Handheld Retry result button entity resolved from the presentation hierarchy.
        /// </summary>
        Entity ResultsRetryButtonEntity;
        /// <summary>
        /// Handheld Exit result button entity resolved from the presentation hierarchy.
        /// </summary>
        Entity ResultsExitButtonEntity;
        /// <summary>
        /// Handheld Next result button entity resolved from the presentation hierarchy.
        /// </summary>
        Entity ResultsNextButtonEntity;
        /// <summary>
        /// Background used to present the selected state of the handheld Retry button.
        /// </summary>
        RoundedRectComponent ResultsRetryButtonBackground;
        /// <summary>
        /// Background used to present the selected state of the handheld Exit button.
        /// </summary>
        RoundedRectComponent ResultsExitButtonBackground;
        /// <summary>
        /// Background used to present the selected state of the handheld Next button.
        /// </summary>
        RoundedRectComponent ResultsNextButtonBackground;
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
            // Native builds do not zero-initialize C# instance fields automatically.
            // Keep every runtime-only dependency in a known state until it is resolved.
            CurrentLevel = null;
            PlayerSphereEntity = null;
            PlayerRigidBody = null;
            GoalEntity = null;
            GoalTriggerObserver = null;
            StageComponent = null;
            FollowCameraComponent = null;
            BallResetComponent = null;
            SpeedTextComponent = null;
            TimerTextComponent = null;
            CoinTextComponent = null;
            TargetTimesTextComponent = null;
            ResultsOverlayEntity = null;
            ResultsTitleTextComponent = null;
            ResultsBodyTextComponent = null;
            ResultsRetryButtonEntity = null;
            ResultsExitButtonEntity = null;
            ResultsNextButtonEntity = null;
            ResultsRetryButtonBackground = null;
            ResultsExitButtonBackground = null;
            ResultsNextButtonBackground = null;
            FailOverlayEntity = null;
            FailTitleTextComponent = null;
            FailBodyTextComponent = null;
            CollectibleCoinComponents = null;
            IsSessionStateInitialized = false;
            RemainingTimeSeconds = 0f;
            ElapsedTimeSeconds = 0f;
            FinalTimeSeconds = 0f;
            AwardedMedal = TiltTrialMedal.None;
            OverlaySelectionIndex = 0;
            DeferredDependencyResolutionFrameCount = 0;
            FrozenPlayerPosition = float3.Zero;
            FrozenPlayerOrientation = new float4(0f, 0f, 0f, 1f);
            HasFrozenPlayerPose = false;
            UpdateOrder = 1;
        }

        /// <summary>
        /// Advances gameplay countdown state or overlay interaction depending on the active session state.
        /// </summary>
        public override void Update() {
            base.Update();
            ReportStage("TiltTrialSession:Update:Begin");

            if (Parent == null) {
                throw new InvalidOperationException("TiltTrialSessionComponent requires an attached gameplay UI root entity.");
            }

            ReportStage("TiltTrialSession:Update:ResolveDependencies");
            if (!TryResolveRuntimeDependenciesWhenNeeded()) {
                return;
            }

            ReportStage("TiltTrialSession:Update:EnsureSessionState");
            EnsureSessionStateInitialized();

            ReportStage("TiltTrialSession:Update:AfterEnsureSessionState");
            if (SessionStateMachine.CurrentState == TiltTrialSessionState.Playing) {
                ReportStage("TiltTrialSession:Update:Playing");
                UpdatePlayingState();
                return;
            }

            ReportStage("TiltTrialSession:Update:FrozenPose");
            MaintainFrozenPlayerPose();
            ReportStage("TiltTrialSession:Update:RefreshOverlay");
            RefreshOverlayPresentation();
            if (SessionStateMachine.CurrentState == TiltTrialSessionState.Paused) {
                return;
            } else if (SessionStateMachine.CurrentState == TiltTrialSessionState.Results) {
                ReportStage("TiltTrialSession:Update:ResultsOverlay");
                UpdateResultsOverlay();
            } else if (SessionStateMachine.CurrentState == TiltTrialSessionState.Failed) {
                ReportStage("TiltTrialSession:Update:FailedOverlay");
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

        public static TiltTrialMedal ResolveMedal(TiltTrialLevelCatalogEntry level, float clearTimeSeconds) {
            if (level == null) {
                throw new ArgumentNullException(nameof(level));
            }

            if (clearTimeSeconds <= level.GoldTimeSeconds) {
                return TiltTrialMedal.Gold;
            } else if (clearTimeSeconds <= level.SilverTimeSeconds) {
                return TiltTrialMedal.Silver;
            } else if (clearTimeSeconds <= level.BronzeTimeSeconds) {
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
        /// Resolves the trigger observer used by one collectible coin, walking up authored wrapper entities when needed.
        /// </summary>
        /// <param name="entity">Coin entity or wrapper entity whose trigger should be resolved.</param>
        /// <returns>First trigger observer found on the supplied entity or one ancestor; otherwise null.</returns>
        public static global::helengine.SceneEntityTriggerObserverComponent ResolveCoinTriggerObserver(Entity entity) {
            Entity current = entity;
            while (current != null) {
                global::helengine.SceneEntityTriggerObserverComponent triggerObserver = TryFindTriggerObserverComponent(current);
                if (triggerObserver != null) {
                    return triggerObserver;
                }

                current = current.Parent;
            }

            return null;
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
        /// Resolves whether one runtime scene load should be treated as a reload because the same scene is already active.
        /// </summary>
        /// <param name="sceneId">Requested target scene id.</param>
        /// <param name="loadedSceneIds">Currently loaded runtime scene ids.</param>
        /// <returns>True when the target scene is already loaded and should be explicitly unloaded before reloading.</returns>
        public static bool RequiresExplicitSceneReload(string sceneId, IReadOnlyList<string> loadedSceneIds) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }
            if (loadedSceneIds == null) {
                throw new ArgumentNullException(nameof(loadedSceneIds));
            }

            for (int index = 0; index < loadedSceneIds.Count; index++) {
                if (string.Equals(loadedSceneIds[index], sceneId, StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates one uninitialized session-state machine used by runtime and unit tests.
        /// </summary>
        /// <returns>Uninitialized Tilt Trial state machine with registered states.</returns>
        public static FiniteStateMachine<TiltTrialSessionState> CreateStateMachine() {
            FiniteStateMachine<TiltTrialSessionState> machine = new FiniteStateMachine<TiltTrialSessionState>();
            machine.RegisterState(TiltTrialSessionState.Playing, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Paused, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Results, new FiniteStateDefinition<TiltTrialSessionState>());
            machine.RegisterState(TiltTrialSessionState.Failed, new FiniteStateDefinition<TiltTrialSessionState>());
            return machine;
        }

        /// <summary>
        /// Applies one presentation-independent action to the active session.
        /// </summary>
        /// <param name="action">Semantic action requested by a physical or platform-specific presentation control.</param>
        public void RequestAction(TiltTrialSessionAction action) {
            if (!IsSessionStateInitialized) {
                return;
            }

            if (action == TiltTrialSessionAction.TogglePause) {
                if (SessionStateMachine.CurrentState == TiltTrialSessionState.Playing) {
                    CaptureFrozenPlayerPose();
                    SetGameplayUpdatesSuppressed(true);
                    SessionStateMachine.TryChangeState(TiltTrialSessionState.Paused);
                } else if (SessionStateMachine.CurrentState == TiltTrialSessionState.Paused) {
                    HasFrozenPlayerPose = false;
                    SetGameplayUpdatesSuppressed(false);
                    SessionStateMachine.TryChangeState(TiltTrialSessionState.Playing);
                }
                return;
            }

            if (SessionStateMachine.CurrentState == TiltTrialSessionState.Results) {
                RequestResultsAction(action);
            } else if (SessionStateMachine.CurrentState == TiltTrialSessionState.Failed) {
                RequestFailedAction(action);
            }
        }

        /// <summary>
        /// Applies one action to the result overlay selection and navigation.
        /// </summary>
        /// <param name="action">Requested result overlay action.</param>
        void RequestResultsAction(TiltTrialSessionAction action) {
            if (action == TiltTrialSessionAction.NavigatePrevious) {
                OverlaySelectionIndex = OverlaySelectionIndex <= 0 ? 2 : OverlaySelectionIndex - 1;
            } else if (action == TiltTrialSessionAction.NavigateNext) {
                OverlaySelectionIndex = OverlaySelectionIndex >= 2 ? 0 : OverlaySelectionIndex + 1;
            } else if (action == TiltTrialSessionAction.Retry) {
                LoadScene(CurrentLevel.SceneId);
            } else if (action == TiltTrialSessionAction.Next) {
                LoadScene(ResolveNextSceneId(CurrentLevel.LevelId, TiltTrialSceneIds.ResolveLevelSelectSceneId()));
            } else if (action == TiltTrialSessionAction.LevelSelect || action == TiltTrialSessionAction.Accept) {
                LoadScene(ResolveResultAcceptSceneId());
            }
        }

        /// <summary>
        /// Applies one action to the failed-level overlay selection and navigation.
        /// </summary>
        /// <param name="action">Requested failed overlay action.</param>
        void RequestFailedAction(TiltTrialSessionAction action) {
            if (action == TiltTrialSessionAction.NavigatePrevious) {
                OverlaySelectionIndex = OverlaySelectionIndex <= 0 ? 1 : OverlaySelectionIndex - 1;
            } else if (action == TiltTrialSessionAction.NavigateNext) {
                OverlaySelectionIndex = OverlaySelectionIndex >= 1 ? 0 : OverlaySelectionIndex + 1;
            } else if (action == TiltTrialSessionAction.Retry) {
                LoadScene(CurrentLevel.SceneId);
            } else if (action == TiltTrialSessionAction.LevelSelect || action == TiltTrialSessionAction.Accept) {
                LoadScene(ResolveFailedAcceptSceneId());
            }
        }

        /// <summary>
        /// Resolves the result overlay destination for the current selection.
        /// </summary>
        /// <returns>Selected retry, next-level, or level-select scene id.</returns>
        string ResolveResultAcceptSceneId() {
            if (OverlaySelectionIndex == 0) {
                return CurrentLevel.SceneId;
            } else if (OverlaySelectionIndex == 1) {
                return ResolveNextSceneId(CurrentLevel.LevelId, TiltTrialSceneIds.ResolveLevelSelectSceneId());
            }

            return TiltTrialSceneIds.ResolveLevelSelectSceneId();
        }

        /// <summary>
        /// Resolves the failed overlay destination for the current selection.
        /// </summary>
        /// <returns>Selected retry or level-select scene id.</returns>
        string ResolveFailedAcceptSceneId() {
            return OverlaySelectionIndex == 0 ? CurrentLevel.SceneId : TiltTrialSceneIds.ResolveLevelSelectSceneId();
        }

        void EnsureSessionStateInitialized() {
            if (IsSessionStateInitialized) {
                return;
            }

            ReportStage("TiltTrialSession:EnsureSessionState:Validate");
            if (CurrentLevel == null) {
                throw new InvalidOperationException("Tilt Trial session requires a resolvable current level catalog entry.");
            }
            ReportStage("TiltTrialSession:EnsureSessionState:ApplyInitialState");
            RemainingTimeSeconds = CurrentLevel.StartTimeSeconds;
            ElapsedTimeSeconds = 0f;
            FinalTimeSeconds = 0f;
            AwardedMedal = TiltTrialMedal.None;
            OverlaySelectionIndex = 0;
            SessionStateMachine.Initialize(TiltTrialSessionState.Playing);
            SetGameplayUpdatesSuppressed(false);
            ReportStage("TiltTrialSession:EnsureSessionState:RefreshOverlay");
            RefreshOverlayPresentation();
            IsSessionStateInitialized = true;
        }

        void UpdatePlayingState() {
            ReportStage("TiltTrialSession:UpdatePlayingState:RefreshTimerStart");
            RefreshTimerText(RemainingTimeSeconds);
            ReportStage("TiltTrialSession:UpdatePlayingState:RefreshCoinStart");
            RefreshCoinText();
            if (WasReturnPressed()) {
                LoadScene(TiltTrialSceneIds.ResolveLevelSelectSceneId());
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

            ReportStage("TiltTrialSession:UpdatePlayingState:RefreshTimerEnd");
            RefreshTimerText(RemainingTimeSeconds);
            ReportStage("TiltTrialSession:UpdatePlayingState:RefreshCoinEnd");
            RefreshCoinText();
        }

        void EnterResultsState() {
            ReportStage("TiltTrialSession:EnterResultsState");
            FinalTimeSeconds = ElapsedTimeSeconds;
            AwardedMedal = ResolveMedal(CurrentLevel, FinalTimeSeconds);
            OverlaySelectionIndex = 0;
            CaptureFrozenPlayerPose();
            SetGameplayUpdatesSuppressed(true);
            SessionStateMachine.TryChangeState(TiltTrialSessionState.Results);
            RefreshOverlayPresentation();
        }

        void EnterFailedState() {
            ReportStage("TiltTrialSession:EnterFailedState");
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
                    LoadScene(CurrentLevel.SceneId);
                } else if (OverlaySelectionIndex == 1) {
                    LoadScene(ResolveNextSceneId(CurrentLevel.LevelId, TiltTrialSceneIds.ResolveLevelSelectSceneId()));
                } else {
                    LoadScene(TiltTrialSceneIds.ResolveLevelSelectSceneId());
                }
                return;
            } else if (WasReturnPressed()) {
                LoadScene(TiltTrialSceneIds.ResolveLevelSelectSceneId());
                return;
            }

            if (ResultsTitleTextComponent == null || ResultsBodyTextComponent == null) {
                return;
            }

            ResultsTitleTextComponent.Text = $"Clear - {AwardedMedal}";
            ResultsBodyTextComponent.Text = HasResultActionButtons()
                ? $"Time {TiltTrialLevelSelectComponent.FormatTimerSeconds(FinalTimeSeconds)}"
                : $"Time {TiltTrialLevelSelectComponent.FormatTimerSeconds(FinalTimeSeconds)}\n\n{BuildResultsOptionLine(0, "Retry")}\n{BuildResultsOptionLine(1, "Next")}\n{BuildResultsOptionLine(2, "Level Select")}";
            ApplyResultButtonSelection();
        }

        /// <summary>
        /// Returns whether the active presentation provides all three handheld result controls.
        /// </summary>
        /// <returns>True when Retry, Exit, and Next button backgrounds are all available.</returns>
        bool HasResultActionButtons() {
            return ResultsRetryButtonBackground != null
                && ResultsExitButtonBackground != null
                && ResultsNextButtonBackground != null;
        }

        /// <summary>
        /// Applies the current result selection to the handheld result button backgrounds when present.
        /// </summary>
        void ApplyResultButtonSelection() {
            if (!HasResultActionButtons()) {
                return;
            }

            ApplyResultButtonSelection(ResultsRetryButtonBackground, OverlaySelectionIndex == 0);
            ApplyResultButtonSelection(ResultsExitButtonBackground, OverlaySelectionIndex == 1);
            ApplyResultButtonSelection(ResultsNextButtonBackground, OverlaySelectionIndex == 2);
        }

        /// <summary>
        /// Applies selected or idle colors to one result button background.
        /// </summary>
        /// <param name="background">Button background to update.</param>
        /// <param name="isSelected">Whether the button is currently selected.</param>
        static void ApplyResultButtonSelection(RoundedRectComponent background, bool isSelected) {
            if (background == null) {
                return;
            }

            background.FillColor = isSelected
                ? new byte4(255, 193, 94, 255)
                : new byte4(40, 58, 87, 255);
            background.BorderColor = isSelected
                ? new byte4(255, 237, 196, 255)
                : new byte4(0, 0, 0, 0);
        }

        void UpdateFailedOverlay() {
            if (WasNavigatePreviousPressed()) {
                OverlaySelectionIndex = OverlaySelectionIndex <= 0 ? 1 : OverlaySelectionIndex - 1;
            } else if (WasNavigateNextPressed()) {
                OverlaySelectionIndex = OverlaySelectionIndex >= 1 ? 0 : OverlaySelectionIndex + 1;
            } else if (WasAcceptPressed()) {
                if (OverlaySelectionIndex == 0) {
                    LoadScene(CurrentLevel.SceneId);
                } else {
                    LoadScene(TiltTrialSceneIds.ResolveLevelSelectSceneId());
                }
                return;
            } else if (WasReturnPressed()) {
                LoadScene(TiltTrialSceneIds.ResolveLevelSelectSceneId());
                return;
            }

            if (FailTitleTextComponent == null || FailBodyTextComponent == null) {
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
            if (TargetTimesTextComponent != null && CurrentLevel != null) {
                RefreshTargetTimesText();
            }
            if (ResultsOverlayEntity != null) {
                ResultsOverlayEntity.Enabled = SessionStateMachine.CurrentState == TiltTrialSessionState.Results;
            }
            if (FailOverlayEntity != null) {
                FailOverlayEntity.Enabled = SessionStateMachine.CurrentState == TiltTrialSessionState.Failed;
            }
            ApplyResultButtonSelection();
        }

        void RefreshTimerText(float remainingTimeSeconds) {
            ReportStage("TiltTrialSession:RefreshTimerText:Format");
            if (TimerTextComponent == null) {
                return;
            }

            TimerTextComponent.Text = TiltTrialLevelSelectComponent.FormatTimerSeconds(remainingTimeSeconds);
        }

        void RefreshCoinText() {
            ReportStage("TiltTrialSession:RefreshCoinText:Format");
            if (CoinTextComponent == null) {
                return;
            }

            CoinTextComponent.Text = FormatCoinProgress(ResolveCollectedCoinCount(), ResolveTotalCoinCount());
        }

        /// <summary>
        /// Applies the current level's medal target times to the handheld gameplay HUD.
        /// </summary>
        void RefreshTargetTimesText() {
            ReportStage("TiltTrialSession:RefreshTargetTimesText:Format");
            TargetTimesTextComponent.Text = $"Targets G{TiltTrialLevelSelectComponent.FormatTimerSeconds(CurrentLevel.GoldTimeSeconds)} S{TiltTrialLevelSelectComponent.FormatTimerSeconds(CurrentLevel.SilverTimeSeconds)} B{TiltTrialLevelSelectComponent.FormatTimerSeconds(CurrentLevel.BronzeTimeSeconds)}";
        }

        void ReportStage(string stage) {
            Core core = Core.Instance;
            if (core != null) {
                core.ReportSceneTransitionStage(stage);
            }
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
            if (GoalTriggerObserver == null) {
                return false;
            }

            return GoalTriggerObserver.GetWasEnteredThisFrame()
                || GoalTriggerObserver.GetIsTriggered();
        }

        void CollectCoinsIfNeeded() {
            int coinCount = ResolveTotalCoinCount();
            for (int coinIndex = 0; coinIndex < coinCount; coinIndex++) {
                TiltTrialCollectibleCoinComponent coinComponent = CollectibleCoinComponents[coinIndex];
                if (coinComponent == null || coinComponent.IsCollected || coinComponent.Parent == null) {
                    continue;
                }

                global::helengine.SceneEntityTriggerObserverComponent triggerObserver = ResolveCoinTriggerObserver(coinComponent.Parent);
                if (triggerObserver != null && triggerObserver.GetWasEnteredThisFrame()) {
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
            if (CurrentLevel == null) {
                CurrentLevel = ResolveCurrentLevel();
                if (CurrentLevel == null) {
                    missingDependencies.Add("current level");
                }
            }
            if (PlayerSphereEntity == null) {
                PlayerSphereEntity = FindPlayerSphereEntityAcrossScene();
                if (PlayerSphereEntity == null) {
                    missingDependencies.Add("player sphere");
                }
            }
            if (PlayerRigidBody == null && PlayerSphereEntity != null) {
                PlayerRigidBody = TryFindRigidBodyComponent(PlayerSphereEntity);
            }
            if (PlayerRigidBody == null) {
                missingDependencies.Add("player rigid body");
            }
            if (GoalEntity == null) {
                GoalEntity = FindGoalEntityAcrossScene();
                if (GoalEntity == null) {
                    missingDependencies.Add("goal entity");
                }
            }
            if (GoalTriggerObserver == null && GoalEntity != null) {
                GoalTriggerObserver = TryFindTriggerObserverComponent(GoalEntity);
            }
            if (GoalTriggerObserver == null) {
                missingDependencies.Add("goal trigger observer");
            }
            if (TimerTextComponent == null) {
                Entity timerTextEntity = TryFindNamedEntity(Parent, "TiltTrialTimerText");
                TimerTextComponent = TryFindTextComponent(timerTextEntity);
            }
            if (CoinTextComponent == null) {
                Entity coinTextEntity = TryFindNamedEntity(Parent, "TiltTrialCoinText");
                CoinTextComponent = TryFindTextComponent(coinTextEntity);
            }
            if (TargetTimesTextComponent == null) {
                Entity targetTimesTextEntity = TryFindNamedEntity(Parent, "TiltTrialTargetTimesText");
                TargetTimesTextComponent = TryFindTextComponent(targetTimesTextEntity);
            }
            if (ResultsOverlayEntity == null) {
                ResultsOverlayEntity = TryFindNamedEntity(Parent, "TiltTrialResultsOverlay");
            }
            if (ResultsTitleTextComponent == null) {
                Entity resultsTitleEntity = TryFindNamedEntity(ResultsOverlayEntity, "TiltTrialResultsTitleText");
                ResultsTitleTextComponent = TryFindTextComponent(resultsTitleEntity);
            }
            if (ResultsBodyTextComponent == null) {
                Entity resultsBodyEntity = TryFindNamedEntity(ResultsOverlayEntity, "TiltTrialResultsBodyText");
                ResultsBodyTextComponent = TryFindTextComponent(resultsBodyEntity);
            }
            if (ResultsRetryButtonEntity == null) {
                ResultsRetryButtonEntity = TryFindNamedEntity(ResultsOverlayEntity, "TiltTrialResultRetryButton");
            }
            if (ResultsExitButtonEntity == null) {
                ResultsExitButtonEntity = TryFindNamedEntity(ResultsOverlayEntity, "TiltTrialResultExitButton");
            }
            if (ResultsNextButtonEntity == null) {
                ResultsNextButtonEntity = TryFindNamedEntity(ResultsOverlayEntity, "TiltTrialResultNextButton");
            }
            if (ResultsRetryButtonBackground == null) {
                ResultsRetryButtonBackground = TryFindRoundedRectComponent(ResultsRetryButtonEntity);
            }
            if (ResultsExitButtonBackground == null) {
                ResultsExitButtonBackground = TryFindRoundedRectComponent(ResultsExitButtonEntity);
            }
            if (ResultsNextButtonBackground == null) {
                ResultsNextButtonBackground = TryFindRoundedRectComponent(ResultsNextButtonEntity);
            }
            if (FailOverlayEntity == null) {
                FailOverlayEntity = TryFindNamedEntity(Parent, "TiltTrialFailOverlay");
            }
            if (FailTitleTextComponent == null) {
                Entity failTitleEntity = TryFindNamedEntity(FailOverlayEntity, "TiltTrialFailTitleText");
                FailTitleTextComponent = TryFindTextComponent(failTitleEntity);
            }
            if (FailBodyTextComponent == null) {
                Entity failBodyEntity = TryFindNamedEntity(FailOverlayEntity, "TiltTrialFailBodyText");
                FailBodyTextComponent = TryFindTextComponent(failBodyEntity);
            }
            if (CollectibleCoinComponents == null || CollectibleCoinComponents.Count == 0) {
                CollectibleCoinComponents = FindCollectibleCoinComponentsAcrossScene();
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
                || city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadLeft)
                || city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadUp);
        }

        bool WasNavigateNextPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Right)
                || inputSystem.WasKeyPressed(Keys.Down)
                || inputSystem.WasKeyPressed(Keys.D)
                || inputSystem.WasKeyPressed(Keys.S)
                || city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadRight)
                || city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadDown);
        }

        bool WasAcceptPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Enter)
                || inputSystem.WasKeyPressed(Keys.Space)
                || city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South)
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

            SceneManager sceneManager = Core.Instance.SceneManager;
            if (RequiresExplicitSceneReload(sceneId, sceneManager.GetLoadedSceneIds())) {
                sceneManager.UnloadScene(sceneId);
            }

            sceneManager.LoadScene(sceneId, SceneLoadMode.Single);
        }

        TiltTrialLevelCatalogEntry ResolveCurrentLevel() {
            Core core = Core.Instance;
            if (core == null) {
                return null;
            }

            List<string> loadedSceneIds = core.SceneManager.GetLoadedSceneIds();
            for (int index = loadedSceneIds.Count - 1; index >= 0; index--) {
                TiltTrialLevelCatalogEntry entry = TryFindLevelEntryBySceneId(loadedSceneIds[index]);
                if (entry != null) {
                    return entry;
                }
            }

            return null;
        }

        static TiltTrialLevelCatalogEntry TryFindLevelEntryBySceneId(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                return null;
            }

            IReadOnlyList<TiltTrialLevelCatalogEntry> entries = TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < entries.Count; index++) {
                if (string.Equals(entries[index].SceneId, sceneId, StringComparison.Ordinal)) {
                    return entries[index];
                }
            }

            return null;
        }

        Entity FindPlayerSphereEntityAcrossScene() {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity match = FindPlayerSphereEntityRecursive(entities[entityIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        Entity FindGoalEntityAcrossScene() {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity match = FindGoalEntityRecursive(entities[entityIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        static Entity FindPlayerSphereEntityRecursive(Entity entity) {
            if (entity == null) {
                return null;
            }
            RigidBody3DComponent rigidBody = TryFindRigidBodyComponent(entity);
            if (rigidBody != null
                && TryFindBallResetComponent(entity) != null
                && TryFindSphereColliderComponent(entity) != null) {
                return entity;
            }
            if (entity.Children != null) {
                for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                    Entity match = FindPlayerSphereEntityRecursive(entity.Children[childIndex]);
                    if (match != null) {
                        return match;
                    }
                }
            }

            return null;
        }

        static Entity FindGoalEntityRecursive(Entity entity) {
            if (entity == null) {
                return null;
            }
            if (TryFindGoalComponent(entity) != null) {
                return entity;
            }
            if (entity.Children != null) {
                for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                    Entity match = FindGoalEntityRecursive(entity.Children[childIndex]);
                    if (match != null) {
                        return match;
                    }
                }
            }

            return null;
        }

        static TextComponent TryFindTextComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TextComponent component) {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the rounded rectangle background directly attached to one presentation button.
        /// </summary>
        /// <param name="entity">Button entity whose visual background should be inspected.</param>
        /// <returns>Button background component, or null when the entity does not own one.</returns>
        static RoundedRectComponent TryFindRoundedRectComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is RoundedRectComponent component) {
                    return component;
                }
            }

            return null;
        }

        static RigidBody3DComponent TryFindRigidBodyComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is RigidBody3DComponent component) {
                    return component;
                }
            }

            return null;
        }

        static DemoTiltBallResetComponent TryFindBallResetComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is DemoTiltBallResetComponent component) {
                    return component;
                }
            }

            return null;
        }

        static SphereCollider3DComponent TryFindSphereColliderComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is SphereCollider3DComponent component) {
                    return component;
                }
            }

            return null;
        }

        static TiltTrialGoalComponent TryFindGoalComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TiltTrialGoalComponent component) {
                    return component;
                }
            }

            return null;
        }

        static global::helengine.SceneEntityTriggerObserverComponent TryFindTriggerObserverComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                return null;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is global::helengine.SceneEntityTriggerObserverComponent component) {
                    return component;
                }
            }

            return null;
        }

        List<TiltTrialCollectibleCoinComponent> FindCollectibleCoinComponentsAcrossScene() {
            List<TiltTrialCollectibleCoinComponent> matches = new List<TiltTrialCollectibleCoinComponent>();
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                CollectCoinComponentsRecursive(entities[entityIndex], matches);
            }

            return matches;
        }

        static void CollectCoinComponentsRecursive(Entity entity, List<TiltTrialCollectibleCoinComponent> matches) {
            if (entity == null) {
                return;
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is TiltTrialCollectibleCoinComponent coinComponent) {
                        if (!matches.Contains(coinComponent)) {
                            matches.Add(coinComponent);
                        }
                    }
                }
            }
            if (entity.Children != null) {
                for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                    CollectCoinComponentsRecursive(entity.Children[childIndex], matches);
                }
            }
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
        /// Finds one named entity beneath a hierarchy without depending on authoring child order.
        /// </summary>
        /// <param name="root">Hierarchy root to search.</param>
        /// <param name="name">Exact entity name to find.</param>
        /// <returns>First matching entity, or null when no matching entity exists.</returns>
        static Entity TryFindNamedEntity(Entity root, string name) {
            if (root == null || string.IsNullOrWhiteSpace(name)) {
                return null;
            }
            if (root.Components != null) {
                for (int componentIndex = 0; componentIndex < root.Components.Count; componentIndex++) {
                    if (root.Components[componentIndex] is TiltTrialPresentationRoleComponent roleComponent
                        && string.Equals(roleComponent.Role, name, StringComparison.Ordinal)) {
                        return root;
                    }
                }
            }
            if (root.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < root.Children.Count; childIndex++) {
                Entity match = TryFindNamedEntity(root.Children[childIndex], name);
                if (match != null) {
                    return match;
                }
            }

            return null;
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
