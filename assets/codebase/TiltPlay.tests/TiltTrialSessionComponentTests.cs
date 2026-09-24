using System.Reflection;
using System.Runtime.CompilerServices;

namespace DemoDisc.TiltPlay.tests {
    /// <summary>
    /// Verifies the Tilt Trial session controller drives timeout and completion flow deterministically.
    /// </summary>
    public sealed class TiltTrialSessionComponentTests {
        [Fact]
        public void Resolve_coin_trigger_observer_returns_wrapper_trigger_for_coin_child() {
            global::helengine.SceneEntityTriggerObserverComponent wrapperTriggerObserver = new global::helengine.SceneEntityTriggerObserverComponent();
            helengine.Entity wrapperEntity = CreateEntity(null, [wrapperTriggerObserver]);
            helengine.Entity coinEntity = CreateEntity(wrapperEntity, [new DemoDisc.TiltPlay.TiltTrialCollectibleCoinComponent()]);

            global::helengine.SceneEntityTriggerObserverComponent resolvedTriggerObserver = DemoDisc.TiltPlay.TiltTrialSessionComponent.ResolveCoinTriggerObserver(coinEntity);

            Assert.Same(wrapperTriggerObserver, resolvedTriggerObserver);
        }

        [Fact]
        public void Collect_coin_disables_direct_parent_when_no_wrapper_trigger_entity_is_present() {
            helengine.Entity wrapperEntity = CreateEntity(null, []);
            DemoDisc.TiltPlay.TiltTrialCollectibleCoinComponent coinComponent = AttachComponent<DemoDisc.TiltPlay.TiltTrialCollectibleCoinComponent>();
            helengine.Entity coinEntity = CreateEntity(wrapperEntity, [coinComponent]);
            SetChildren(wrapperEntity, [coinEntity]);

            coinComponent.Collect();

            Assert.True(coinComponent.IsCollected);
            Assert.False(coinEntity.Enabled);
        }

        [Fact]
        public void Resolve_medal_returns_gold_for_fastest_clear() {
            DemoDisc.TiltPlay.TiltTrialLevelSettingsComponent settings = new DemoDisc.TiltPlay.TiltTrialLevelSettingsComponent {
                LevelId = "tilt-trial-01",
                DisplayName = "Level 1",
                SceneId = DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId,
                StartTimeSeconds = 99f,
                GoldTimeSeconds = 20f,
                SilverTimeSeconds = 35f,
                BronzeTimeSeconds = 50f
            };

            DemoDisc.TiltPlay.TiltTrialMedal medal = DemoDisc.TiltPlay.TiltTrialSessionComponent.ResolveMedal(settings, 19.5f);
            Assert.Equal(DemoDisc.TiltPlay.TiltTrialMedal.Gold, medal);
        }

        [Fact]
        public void Resolve_next_scene_id_returns_level_select_when_current_level_is_last() {
            string nextSceneId = DemoDisc.TiltPlay.TiltTrialSessionComponent.ResolveNextSceneId(
                "tilt-trial-05",
                DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId);

            Assert.Equal(DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId, nextSceneId);
        }

        [Fact]
        public void Requires_explicit_scene_reload_returns_true_when_target_scene_is_already_loaded() {
            bool requiresReload = DemoDisc.TiltPlay.TiltTrialSessionComponent.RequiresExplicitSceneReload(
                DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId,
                [
                    DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId,
                    DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId
                ]);

            Assert.True(requiresReload);
        }

        [Fact]
        public void Requires_explicit_scene_reload_returns_false_when_target_scene_is_not_loaded() {
            bool requiresReload = DemoDisc.TiltPlay.TiltTrialSessionComponent.RequiresExplicitSceneReload(
                DemoDisc.TiltPlay.TiltTrialSceneIds.Level01SceneId,
                [
                    DemoDisc.TiltPlay.TiltTrialSceneIds.LevelSelectSceneId
                ]);

            Assert.False(requiresReload);
        }

        [Fact]
        public void Build_state_machine_transitions_from_playing_to_failed_when_timeout_occurs() {
            helengine.FiniteStateMachine<DemoDisc.TiltPlay.TiltTrialSessionState> machine = DemoDisc.TiltPlay.TiltTrialSessionComponent.CreateStateMachine();

            machine.Initialize(DemoDisc.TiltPlay.TiltTrialSessionState.Playing);
            bool changed = machine.TryChangeState(DemoDisc.TiltPlay.TiltTrialSessionState.Failed);

            Assert.True(changed);
            Assert.Equal(DemoDisc.TiltPlay.TiltTrialSessionState.Failed, machine.CurrentState);
        }

        /// <summary>
        /// Ensures every Tilt Trial session can wait for Accept before entering active gameplay.
        /// </summary>
        [Fact]
        public void Build_state_machine_starts_waiting_for_accept_and_transitions_to_playing() {
            helengine.FiniteStateMachine<DemoDisc.TiltPlay.TiltTrialSessionState> machine = DemoDisc.TiltPlay.TiltTrialSessionComponent.CreateStateMachine();

            machine.Initialize(DemoDisc.TiltPlay.TiltTrialSessionState.Start);
            bool changed = machine.TryChangeState(DemoDisc.TiltPlay.TiltTrialSessionState.Playing);

            Assert.True(changed);
            Assert.Equal(DemoDisc.TiltPlay.TiltTrialSessionState.Playing, machine.CurrentState);
        }

        /// <summary>
        /// Ensures the handheld gameplay HUD is absent from the pre-start screen and appears only during active play.
        /// </summary>
        [Fact]
        public void Gameplay_panel_is_visible_only_while_the_session_is_playing() {
            Assert.False(DemoDisc.TiltPlay.TiltTrialSessionComponent.ShouldShowGameplayPanel(DemoDisc.TiltPlay.TiltTrialSessionState.Start));
            Assert.True(DemoDisc.TiltPlay.TiltTrialSessionComponent.ShouldShowGameplayPanel(DemoDisc.TiltPlay.TiltTrialSessionState.Playing));
            Assert.False(DemoDisc.TiltPlay.TiltTrialSessionComponent.ShouldShowGameplayPanel(DemoDisc.TiltPlay.TiltTrialSessionState.Paused));
            Assert.False(DemoDisc.TiltPlay.TiltTrialSessionComponent.ShouldShowGameplayPanel(DemoDisc.TiltPlay.TiltTrialSessionState.Results));
            Assert.False(DemoDisc.TiltPlay.TiltTrialSessionComponent.ShouldShowGameplayPanel(DemoDisc.TiltPlay.TiltTrialSessionState.Failed));
        }

        [Fact]
        public void Session_component_exposes_authored_player_goal_and_stage_links() {
            Type sessionType = typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent);

            Assert.NotNull(sessionType.GetProperty("PlayerSphereReference"));
            Assert.NotNull(sessionType.GetProperty("GoalEntityReference"));
            Assert.NotNull(sessionType.GetProperty("StageRootReference"));
            Assert.NotNull(sessionType.GetProperty("GameplayPanelReference"));
        }

        [Fact]
        public void Session_binds_renamed_scene_targets_from_references_and_allows_absent_handheld_panel() {
            helengine.Entity player = CreateEntity(null, [
                new RigidBody3DComponent(),
                new DemoDisc.TiltPlay.DemoTiltBallResetComponent()
            ]);
            helengine.Entity goal = CreateEntity(null, [new global::helengine.SceneEntityTriggerObserverComponent()]);
            helengine.Entity camera = CreateEntity(null, [new DemoDisc.TiltPlay.DemoTiltFollowCameraComponent()]);
            DemoDisc.TiltPlay.DemoTiltStageComponent stage = new DemoDisc.TiltPlay.DemoTiltStageComponent {
                OrbitCameraReference = ResolvedReference(camera)
            };
            helengine.Entity stageRoot = CreateEntity(null, [stage]);
            helengine.Entity timer = CreateEntity(null, [new TextComponent()]);
            helengine.Entity coinText = CreateEntity(null, [new TextComponent()]);
            helengine.Entity start = CreateEntity(null, []);
            helengine.Entity results = CreateEntity(null, []);
            helengine.Entity fail = CreateEntity(null, []);
            DemoDisc.TiltPlay.TiltTrialSessionComponent session = new DemoDisc.TiltPlay.TiltTrialSessionComponent {
                PlayerSphereReference = ResolvedReference(player),
                GoalEntityReference = ResolvedReference(goal),
                StageRootReference = ResolvedReference(stageRoot),
                TimerTextReference = ResolvedReference(timer),
                CoinTextReference = ResolvedReference(coinText),
                StartOverlayReference = ResolvedReference(start),
                ResultsOverlayReference = ResolvedReference(results),
                FailOverlayReference = ResolvedReference(fail)
            };

            typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent)
                .GetMethod("BindFixedReferences", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(session, null);

            Type sessionType = typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent);
            Assert.Same(player, sessionType.GetField("PlayerSphereEntity", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(session));
            Assert.Same(goal, sessionType.GetField("GoalEntity", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(session));
            Assert.Same(stage, sessionType.GetField("StageComponent", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(session));
            Assert.Null(sessionType.GetField("GameplayPanelEntity", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(session));
        }

        [Fact]
        public void Format_coin_progress_returns_expected_hud_label() {
            string label = DemoDisc.TiltPlay.TiltTrialSessionComponent.FormatCoinProgress(3, 7);

            Assert.Equal("Coins 3/7", label);
        }

        /// <summary>
        /// Ensures the results selection order matches the visible Next, Retry, and Back to Menu buttons.
        /// </summary>
        /// <param name="selectionIndex">Zero-based visible button selection.</param>
        /// <param name="expectedSceneId">Literal scene id that accepting the selection must load.</param>
        [Theory]
        [InlineData(0, "tilt_trial_level_02")]
        [InlineData(1, "tilt_trial_level_01")]
        [InlineData(2, "tilt_trial")]
        public void Result_selection_resolves_next_retry_and_back_to_menu_in_visible_order(int selectionIndex, string expectedSceneId) {
            DemoDisc.TiltPlay.TiltTrialSessionComponent session = new DemoDisc.TiltPlay.TiltTrialSessionComponent();
            DemoDisc.TiltPlay.TiltTrialLevelCatalogEntry currentLevel = Assert.Single(
                DemoDisc.TiltPlay.TiltTrialLevelCatalog.CreateEntries(),
                entry => entry.LevelId == "tilt-trial-01");
            typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent).GetField("CurrentLevel", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(session, currentLevel);
            typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent).GetField("OverlaySelectionIndex", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(session, selectionIndex);
            MethodInfo resolveMethod = typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent).GetMethod("ResolveResultAcceptSceneId", BindingFlags.Instance | BindingFlags.NonPublic)!;

            string sceneId = Assert.IsType<string>(resolveMethod.Invoke(session, null));

            Assert.Equal(expectedSceneId, sceneId);
        }

        /// <summary>
        /// Ensures result focus swaps the same background and label colors used by the working Tilt Trial selector buttons.
        /// </summary>
        [Fact]
        public void Result_selection_swaps_button_background_and_label_colors() {
            RoundedRectComponent nextBackground = new RoundedRectComponent();
            RoundedRectComponent retryBackground = new RoundedRectComponent();
            RoundedRectComponent exitBackground = new RoundedRectComponent();
            TextComponent nextLabel = new TextComponent();
            TextComponent retryLabel = new TextComponent();
            TextComponent exitLabel = new TextComponent();
            helengine.Entity nextButton = CreateEntity(null, [nextBackground]);
            helengine.Entity retryButton = CreateEntity(null, [retryBackground]);
            helengine.Entity exitButton = CreateEntity(null, [exitBackground]);
            SetChildren(nextButton, [CreateEntity(nextButton, [
                nextLabel,
                new DemoDisc.TiltPlay.TiltTrialPresentationRoleComponent { Role = "TiltTrialResultNextButtonLabel" }
            ])]);
            SetChildren(retryButton, [CreateEntity(retryButton, [
                retryLabel,
                new DemoDisc.TiltPlay.TiltTrialPresentationRoleComponent { Role = "TiltTrialResultRetryButtonLabel" }
            ])]);
            SetChildren(exitButton, [CreateEntity(exitButton, [
                exitLabel,
                new DemoDisc.TiltPlay.TiltTrialPresentationRoleComponent { Role = "TiltTrialResultExitButtonLabel" }
            ])]);
            DemoDisc.TiltPlay.TiltTrialSessionComponent session = new DemoDisc.TiltPlay.TiltTrialSessionComponent();
            Type sessionType = typeof(DemoDisc.TiltPlay.TiltTrialSessionComponent);
            sessionType.GetField("ResultsNextButtonBackground", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, nextBackground);
            sessionType.GetField("ResultsRetryButtonBackground", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, retryBackground);
            sessionType.GetField("ResultsExitButtonBackground", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, exitBackground);
            sessionType.GetField("ResultsNextButtonLabel", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, nextLabel);
            sessionType.GetField("ResultsRetryButtonLabel", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, retryLabel);
            sessionType.GetField("ResultsExitButtonLabel", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, exitLabel);
            sessionType.GetField("OverlaySelectionIndex", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, 0);

            sessionType.GetMethod("ApplyResultButtonSelection", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(session, null);

            Assert.Equal((byte)255, nextBackground.FillColor.X);
            Assert.Equal((byte)193, nextBackground.FillColor.Y);
            Assert.Equal((byte)94, nextBackground.FillColor.Z);
            Assert.Equal((byte)28, nextLabel.Color.X);
            Assert.Equal((byte)40, retryBackground.FillColor.X);
            Assert.Equal((byte)58, retryBackground.FillColor.Y);
            Assert.Equal((byte)87, retryBackground.FillColor.Z);
            Assert.Equal((byte)247, retryLabel.Color.X);
            Assert.Equal((byte)40, exitBackground.FillColor.X);
            Assert.Equal((byte)247, exitLabel.Color.X);
        }

        static helengine.Entity CreateEntity(helengine.Entity parent, List<helengine.Component> components) {
            helengine.Entity entity = (helengine.Entity)RuntimeHelpers.GetUninitializedObject(typeof(helengine.Entity));
            typeof(helengine.Entity).GetField("IsEnabled", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, true);
            typeof(helengine.Entity).GetField("LayerMaskValue", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, (ushort)1);
            typeof(helengine.Entity).GetProperty(nameof(helengine.Entity.Parent), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entity, parent);
            typeof(helengine.Entity).GetField("ComponentsValue", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, components);
            typeof(helengine.Entity).GetField("ChildrenValue", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, new List<helengine.Entity>());
            for (int componentIndex = 0; componentIndex < components.Count; componentIndex++) {
                typeof(helengine.Component).GetProperty(nameof(helengine.Component.Parent), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                    .SetValue(components[componentIndex], entity);
            }
            return entity;
        }

        static T AttachComponent<T>()
            where T : helengine.Component, new() {
            return new T();
        }

        static SceneEntityReference ResolvedReference(helengine.Entity entity) {
            SceneEntityReference reference = new SceneEntityReference();
            typeof(SceneEntityReference).GetProperty(nameof(SceneEntityReference.ResolvedEntity), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(reference, entity);
            return reference;
        }

        static void SetChildren(helengine.Entity entity, List<helengine.Entity> children) {
            typeof(helengine.Entity).GetField("ChildrenValue", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, children);
        }
    }
}
