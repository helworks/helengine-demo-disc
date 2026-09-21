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

        /// <summary>
        /// Ensures session initialization freezes gameplay and only the explicit start branch can release it.
        /// </summary>
        [Fact]
        public void Session_initializes_frozen_until_the_accept_start_transition() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialSessionComponent.cs");

            Assert.Contains("SessionStateMachine.Initialize(TiltTrialSessionState.Start)", source, StringComparison.Ordinal);
            Assert.Contains("CaptureFrozenPlayerPose();", source, StringComparison.Ordinal);
            Assert.Contains("SetGameplayUpdatesSuppressed(true);", source, StringComparison.Ordinal);
            Assert.Contains("void UpdateStartState()", source, StringComparison.Ordinal);
            Assert.Contains("if (!WasAcceptPressed())", source, StringComparison.Ordinal);
            Assert.Contains("SetGameplayUpdatesSuppressed(false);", source, StringComparison.Ordinal);
            Assert.Contains("SessionStateMachine.TryChangeState(TiltTrialSessionState.Playing);", source, StringComparison.Ordinal);
            Assert.Contains("StartOverlayEntity.Enabled = SessionStateMachine.CurrentState == TiltTrialSessionState.Start", source, StringComparison.Ordinal);

            int startStateMethodIndex = source.IndexOf("void UpdateStartState()", StringComparison.Ordinal);
            int playingStateMethodIndex = source.IndexOf("void UpdatePlayingState()", StringComparison.Ordinal);
            string startStateMethodSource = source.Substring(startStateMethodIndex, playingStateMethodIndex - startStateMethodIndex);

            Assert.Contains("RefreshOverlayPresentation();", startStateMethodSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures every non-playing Tilt Trial session state stops shared fixed-step physics and scene disposal releases that stop.
        /// </summary>
        [Fact]
        public void Session_pauses_shared_physics_until_playing_and_releases_it_on_disposal() {
            string source = File.ReadAllText(ResolveTiltTrialSessionComponentSourcePath());

            Assert.Contains("Core.Instance.PhysicsSimulationIsPaused = updatesAreSuppressed;", source, StringComparison.Ordinal);
            Assert.Contains("public override void Dispose()", source, StringComparison.Ordinal);
            Assert.Contains("Core.Instance.PhysicsSimulationIsPaused = false;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Format_coin_progress_returns_expected_hud_label() {
            string label = DemoDisc.TiltPlay.TiltTrialSessionComponent.FormatCoinProgress(3, 7);

            Assert.Equal("Coins 3/7", label);
        }

        [Fact]
        public void Session_retries_coin_discovery_when_scene_expansion_has_not_finished() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialSessionComponent.cs");

            Assert.Contains("if (CollectibleCoinComponents == null || CollectibleCoinComponents.Count == 0) {", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Goal_clear_uses_trigger_observer_state_instead_of_level_01_center_distance_check() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialSessionComponent.cs");

            Assert.Contains("GoalTriggerObserver.GetWasEnteredThisFrame()", source, StringComparison.Ordinal);
            Assert.Contains("|| GoalTriggerObserver.GetIsTriggered()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("dx <=", source, StringComparison.Ordinal);
            Assert.DoesNotContain("dy <=", source, StringComparison.Ordinal);
            Assert.DoesNotContain("dz <=", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Clear/results overlay accepts left-stick vertical navigation in addition to the D-pad.
        /// </summary>
        [Fact]
        public void Clear_overlay_navigation_accepts_left_stick_vertical_direction() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialSessionComponent.cs");

            Assert.Contains("|| WasLeftStickUpPressed();", source, StringComparison.Ordinal);
            Assert.Contains("|| WasLeftStickDownPressed();", source, StringComparison.Ordinal);
            Assert.Contains("bool WasLeftStickUpPressed()", source, StringComparison.Ordinal);
            Assert.Contains("bool WasLeftStickDownPressed()", source, StringComparison.Ordinal);
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
            sessionType.GetField("ResultsNextButtonEntity", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, nextButton);
            sessionType.GetField("ResultsRetryButtonEntity", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, retryButton);
            sessionType.GetField("ResultsExitButtonEntity", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(session, exitButton);
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

        static string ResolveTiltTrialSessionComponentSourcePath() {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null) {
                string candidate = Path.Combine(directory.FullName, "assets", "codebase", "TiltPlay", "TiltTrialSessionComponent.cs");
                if (File.Exists(candidate)) {
                    return candidate;
                }

                directory = directory.Parent;
            }

            const string checkoutSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialSessionComponent.cs";
            if (File.Exists(checkoutSourcePath)) {
                return checkoutSourcePath;
            }

            throw new FileNotFoundException("Unable to locate TiltTrialSessionComponent.cs from the active test checkout.");
        }

        static helengine.Entity CreateEntity(helengine.Entity parent, List<helengine.Component> components) {
            helengine.Entity entity = (helengine.Entity)RuntimeHelpers.GetUninitializedObject(typeof(helengine.Entity));
            typeof(helengine.Entity).GetField("isEnabled", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, true);
            typeof(helengine.Entity).GetField("layerMask", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, (ushort)1);
            typeof(helengine.Entity).GetProperty(nameof(helengine.Entity.Parent), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(entity, parent);
            typeof(helengine.Entity).GetField("components", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, components);
            typeof(helengine.Entity).GetField("children", BindingFlags.Instance | BindingFlags.NonPublic)!
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

        static void SetChildren(helengine.Entity entity, List<helengine.Entity> children) {
            typeof(helengine.Entity).GetField("children", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(entity, children);
        }
    }
}
