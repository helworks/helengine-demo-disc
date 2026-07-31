using helengine;

namespace city.game {
    /// <summary>
    /// Drives the dedicated Tilt Trial level-select scene.
    /// </summary>
    public sealed class TiltTrialLevelSelectComponent : UpdateComponent {
        /// <summary>Raw left-stick axis magnitude required to produce one directional navigation event.</summary>
        const short GamepadStickNavigationThreshold = 16384;
        /// <summary>Stores the generated stage button backgrounds used for selection feedback.</summary>
        readonly List<RoundedRectComponent> RowBackgrounds;
        /// <summary>Stores the generated stage button labels used for selection feedback.</summary>
        readonly List<TextComponent> RowLabels;
        /// <summary>Stores the immutable stage catalog reused by every selector update and pointer action.</summary>
        readonly IReadOnlyList<TiltTrialLevelCatalogEntry> LevelEntries;

        /// <summary>Generated panel containing the stage list buttons.</summary>
        Entity ListPanelEntity;
        /// <summary>Generated panel containing the selected stage details and actions.</summary>
        Entity DetailsPanelEntity;
        /// <summary>Generated Back action entity whose visibility follows the details stage.</summary>
        Entity DetailBackButtonEntity;
        /// <summary>Generated Play action entity whose visibility follows the details stage.</summary>
        Entity DetailPlayButtonEntity;
        /// <summary>Indicates whether the detail screen is currently visible.</summary>
        bool IsDetailsVisible;
        /// <summary>Stores the focused detail action, where zero is Back and one is Play.</summary>
        int DetailActionIndex;

        /// <summary>Text displaying the selected stage name.</summary>
        TextComponent LevelNameTextComponent;
        /// <summary>Text displaying the selected stage start time.</summary>
        TextComponent LevelTimerTextComponent;
        /// <summary>Text displaying the selected stage target times.</summary>
        TextComponent LevelTargetTimesTextComponent;
        /// <summary>Background used to show the focused Back action.</summary>
        RoundedRectComponent DetailBackButtonBackground;
        /// <summary>Background used to show the focused Play action.</summary>
        RoundedRectComponent DetailPlayButtonBackground;
        /// <summary>Label whose color reflects Back action focus.</summary>
        TextComponent DetailBackButtonLabel;
        /// <summary>Label whose color reflects Play action focus.</summary>
        TextComponent DetailPlayButtonLabel;

        /// <summary>
        /// Gets the zero-based selected level index.
        /// </summary>
        public int SelectedIndex { get; private set; }

        /// <summary>
        /// Gets or sets whether the selector may process navigation and activation input.
        /// </summary>
        public bool AcceptsInput { get; set; } = true;

        /// <summary>
        /// Gets or sets whether accepting a level opens a separate details stage before play.
        /// </summary>
        public bool UseDetailsStage { get; set; }

        /// <summary>
        /// Initializes one level-select controller.
        /// </summary>
        public TiltTrialLevelSelectComponent() {
            RowBackgrounds = new List<RoundedRectComponent>();
            RowLabels = new List<TextComponent>();
            LevelEntries = TiltTrialLevelCatalog.CreateEntries();
        }

        /// <summary>
        /// Processes selector navigation, detail navigation, and stage activation.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("TiltTrialLevelSelectComponent requires an attached selector root entity.");
            }

            if (!AcceptsInput) {
                return;
            }

            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = LevelEntries;
            ResolveUiBindingsWhenNeeded(levels.Count);

            if (UseDetailsStage) {
                if (IsDetailsVisible) {
                    if (WasBackPressed()) {
                        ShowStageList();
                    } else if (WasNavigatePreviousPressed() || WasNavigateNextPressed()) {
                        DetailActionIndex = DetailActionIndex == 0 ? 1 : 0;
                        ApplyDetailActionSelection();
                    } else if (WasAcceptPressed()) {
                        if (DetailActionIndex == 0) {
                            ShowStageList();
                        } else {
                            PlaySelectedStage();
                        }
                    }
                } else if (WasNavigatePreviousPressed()) {
                    SelectedIndex = SelectedIndex <= 0 ? levels.Count - 1 : SelectedIndex - 1;
                } else if (WasNavigateNextPressed()) {
                    SelectedIndex = SelectedIndex >= levels.Count - 1 ? 0 : SelectedIndex + 1;
                } else if (WasAcceptPressed()) {
                    ShowDetails();
                }
            } else {
                if (WasNavigatePreviousPressed()) {
                    SelectedIndex = SelectedIndex <= 0 ? levels.Count - 1 : SelectedIndex - 1;
                } else if (WasNavigateNextPressed()) {
                    SelectedIndex = SelectedIndex >= levels.Count - 1 ? 0 : SelectedIndex + 1;
                } else if (WasAcceptPressed()) {
                    PlaySelectedStage();
                }
            }

            ApplySelectionToUi(levels[SelectedIndex]);
        }

        /// <summary>
        /// Handles a pointer action emitted by a generated selector button.
        /// </summary>
        /// <param name="action">Selector action to execute.</param>
        /// <param name="stageIndex">Zero-based stage index for stage selection.</param>
        public void HandleAction(TiltTrialLevelSelectAction action, int stageIndex) {
            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = LevelEntries;
            ResolveUiBindingsWhenNeeded(levels.Count);
            switch (action) {
                case TiltTrialLevelSelectAction.SelectStage:
                    if (stageIndex < 0 || stageIndex >= levels.Count) {
                        throw new ArgumentOutOfRangeException(nameof(stageIndex));
                    }

                    SelectedIndex = stageIndex;
                    if (UseDetailsStage) {
                        ShowDetails();
                    } else {
                        ShowCombinedView();
                    }
                    break;
                case TiltTrialLevelSelectAction.BackToStages:
                    if (UseDetailsStage) {
                        ShowStageList();
                    } else {
                        ShowCombinedView();
                    }
                    break;
                case TiltTrialLevelSelectAction.PlaySelectedStage:
                    PlaySelectedStage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action));
            }

            ApplySelectionToUi(levels[SelectedIndex]);
        }

        /// <summary>
        /// Shows the selected stage details and hides the stage list.
        /// </summary>
        public void ShowDetails() {
            if (ListPanelEntity == null || DetailsPanelEntity == null) {
                throw new InvalidOperationException("Tilt Trial selector panels are not resolved.");
            }

            IsDetailsVisible = true;
            DetailActionIndex = 1;
            ListPanelEntity.Enabled = false;
            DetailsPanelEntity.Enabled = true;
            DetailBackButtonEntity.Enabled = true;
            DetailPlayButtonEntity.Enabled = true;
            ApplyDetailActionSelection();
        }

        /// <summary>
        /// Shows the stage list and hides the selected stage details.
        /// </summary>
        public void ShowStageList() {
            if (ListPanelEntity == null || DetailsPanelEntity == null) {
                throw new InvalidOperationException("Tilt Trial selector panels are not resolved.");
            }

            IsDetailsVisible = false;
            DetailActionIndex = 0;
            ListPanelEntity.Enabled = true;
            DetailsPanelEntity.Enabled = false;
            DetailBackButtonEntity.Enabled = false;
            DetailPlayButtonEntity.Enabled = false;
        }

        /// <summary>
        /// Keeps the level list and selected-level details visible together for non-handheld selectors.
        /// </summary>
        void ShowCombinedView() {
            if (ListPanelEntity == null || DetailsPanelEntity == null) {
                throw new InvalidOperationException("Tilt Trial selector panels are not resolved.");
            }

            IsDetailsVisible = false;
            DetailActionIndex = 1;
            ListPanelEntity.Enabled = true;
            DetailsPanelEntity.Enabled = true;
        }

        /// <summary>
        /// Loads the selected stage through the normal single-scene transition.
        /// </summary>
        public void PlaySelectedStage() {
            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = LevelEntries;
            if (SelectedIndex < 0 || SelectedIndex >= levels.Count) {
                throw new InvalidOperationException("Tilt Trial selector has no valid selected stage.");
            }

            Core.Instance.SceneManager.RequestSceneTransition(levels[SelectedIndex].SceneId);
        }

        /// <summary>
        /// Applies the selected level's details to the authored selector UI.
        /// </summary>
        /// <param name="selectedLevel">Selected level to present.</param>
        void ApplySelectionToUi(TiltTrialLevelCatalogEntry selectedLevel) {
            if (selectedLevel == null) {
                throw new ArgumentNullException(nameof(selectedLevel));
            }

            ApplyLevelNameText(selectedLevel.DisplayName);
            ApplyLevelTimerText(selectedLevel.StartTimeSeconds);
            ApplyLevelTargetTimesText(selectedLevel.GoldTimeSeconds, selectedLevel.SilverTimeSeconds, selectedLevel.BronzeTimeSeconds);
            ApplyRowSelectionState(selectedLevel.LevelId);
        }

        /// <summary>
        /// Resolves the authored selector text and row visuals once the scene hierarchy is live.
        /// </summary>
        /// <param name="expectedRowCount">Expected number of generated level rows.</param>
        void ResolveUiBindingsWhenNeeded(int expectedRowCount) {
            if (LevelNameTextComponent != null && RowBackgrounds.Count == expectedRowCount && RowLabels.Count == expectedRowCount) {
                return;
            }

            RowBackgrounds.Clear();
            RowLabels.Clear();

            ListPanelEntity = FindRequiredChildEntity(Parent, 0, "Tilt Trial selector list panel");
            DetailsPanelEntity = FindRequiredChildEntity(Parent, 1, "Tilt Trial selector details panel");

            LevelNameTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(DetailsPanelEntity, 0, "Tilt Trial selector level name text"));
            LevelTimerTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(DetailsPanelEntity, 1, "Tilt Trial selector level timer text"));
            LevelTargetTimesTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(DetailsPanelEntity, 2, "Tilt Trial selector target times text"));
            if (UseDetailsStage) {
                DetailBackButtonEntity = FindRequiredChildEntity(DetailsPanelEntity, 3, "Tilt Trial selector Back button");
                DetailPlayButtonEntity = FindRequiredChildEntity(DetailsPanelEntity, 4, "Tilt Trial selector Play button");
                DetailBackButtonBackground = FindRequiredComponent<RoundedRectComponent>(DetailBackButtonEntity);
                DetailPlayButtonBackground = FindRequiredComponent<RoundedRectComponent>(DetailPlayButtonEntity);
                DetailBackButtonLabel = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(DetailBackButtonEntity, 0, "Tilt Trial selector Back button label"));
                DetailPlayButtonLabel = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(DetailPlayButtonEntity, 0, "Tilt Trial selector Play button label"));
            }

            for (int rowIndex = 0; rowIndex < expectedRowCount; rowIndex++) {
                Entity rowEntity = FindRequiredChildEntity(ListPanelEntity, rowIndex, $"Tilt Trial selector row {rowIndex + 1}");
                RowBackgrounds.Add(FindRequiredComponent<RoundedRectComponent>(rowEntity));
                RowLabels.Add(FindRequiredComponent<TextComponent>(FindRequiredChildEntity(rowEntity, 0, $"Tilt Trial selector row {rowIndex + 1} label")));
            }

            if (UseDetailsStage) {
                ShowStageList();
            } else {
                ShowCombinedView();
            }
        }

        /// <summary>
        /// Updates the selected row visuals and row label text.
        /// </summary>
        /// <param name="selectedLevelId">Stable selected level id.</param>
        void ApplyRowSelectionState(string selectedLevelId) {
            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = LevelEntries;
            for (int rowIndex = 0; rowIndex < levels.Count; rowIndex++) {
                bool isSelected = string.Equals(levels[rowIndex].LevelId, selectedLevelId, StringComparison.Ordinal);
                RowBackgrounds[rowIndex].FillColor = isSelected
                    ? new byte4(255, 193, 94, 255)
                    : new byte4(40, 58, 87, 255);
                RowBackgrounds[rowIndex].BorderColor = isSelected
                    ? new byte4(255, 237, 196, 255)
                    : new byte4(109, 138, 170, 255);
                RowLabels[rowIndex].Text = levels[rowIndex].DisplayName;
                RowLabels[rowIndex].Color = isSelected
                    ? new byte4(28, 18, 14, 255)
                    : new byte4(247, 248, 252, 255);
            }
        }

        /// <summary>
        /// Updates detail action backgrounds and labels to identify the focused action.
        /// </summary>
        void ApplyDetailActionSelection() {
            bool isBackSelected = DetailActionIndex == 0;
            DetailBackButtonBackground.FillColor = isBackSelected
                ? new byte4(255, 193, 94, 255)
                : new byte4(40, 58, 87, 255);
            DetailBackButtonBackground.BorderColor = isBackSelected
                ? new byte4(255, 237, 196, 255)
                : new byte4(0, 0, 0, 0);
            DetailBackButtonLabel.Color = isBackSelected
                ? new byte4(28, 18, 14, 255)
                : new byte4(247, 248, 252, 255);

            DetailPlayButtonBackground.FillColor = isBackSelected
                ? new byte4(40, 58, 87, 255)
                : new byte4(255, 193, 94, 255);
            DetailPlayButtonBackground.BorderColor = isBackSelected
                ? new byte4(0, 0, 0, 0)
                : new byte4(255, 237, 196, 255);
            DetailPlayButtonLabel.Color = isBackSelected
                ? new byte4(247, 248, 252, 255)
                : new byte4(28, 18, 14, 255);
        }

        /// <summary>
        /// Applies the selected level name to the details panel.
        /// </summary>
        void ApplyLevelNameText(string displayName) {
            LevelNameTextComponent.Text = displayName ?? string.Empty;
        }

        /// <summary>
        /// Applies the selected level start time to the details panel.
        /// </summary>
        void ApplyLevelTimerText(float startTimeSeconds) {
            LevelTimerTextComponent.Text = $"Limit {FormatTimerSeconds(startTimeSeconds)}";
        }

        /// <summary>
        /// Applies the selected level medal thresholds to the details panel.
        /// </summary>
        void ApplyLevelTargetTimesText(float goldTimeSeconds, float silverTimeSeconds, float bronzeTimeSeconds) {
            LevelTargetTimesTextComponent.Text = $"Gold  {FormatTimerSeconds(goldTimeSeconds)}\nSilver {FormatTimerSeconds(silverTimeSeconds)}\nBronze {FormatTimerSeconds(bronzeTimeSeconds)}";
        }

        /// <summary>
        /// <summary>
        /// Formats one seconds value as `99.00`.
        /// </summary>
        public static string FormatTimerSeconds(float seconds) {
            if (seconds < 0f) {
                seconds = 0f;
            }

            int wholeSeconds = (int)Math.Floor(seconds);
            int hundredths = (int)Math.Floor((seconds - wholeSeconds) * 100f);
            if (hundredths < 0) {
                hundredths = 0;
            } else if (hundredths > 99) {
                hundredths = 99;
            }

            return $"{wholeSeconds:0}.{hundredths:00}";
        }

        /// <summary>
        /// Returns whether the current frame requested the previous selector row.
        /// </summary>
        bool WasNavigatePreviousPressed() {
            InputSystem inputSystem = Core.Instance.Input;
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Up) || inputSystem.WasKeyPressed(Keys.W)) {
                return true;
            }
#endif
            return city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadUp)
                || WasLeftStickUpPressed();
        }

        /// <summary>
        /// Returns whether the current frame requested the next selector row.
        /// </summary>
        bool WasNavigateNextPressed() {
            InputSystem inputSystem = Core.Instance.Input;
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Down) || inputSystem.WasKeyPressed(Keys.S)) {
                return true;
            }
#endif
            return city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.DPadDown)
                || WasLeftStickDownPressed();
        }

        /// <summary>
        /// Returns whether the left stick crossed into its upward navigation zone this frame.
        /// </summary>
        bool WasLeftStickUpPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            short currentStickY = city.menu.DemoDiscGamepadInput.GetLeftStickY(inputSystem);
            short previousStickY = city.menu.DemoDiscGamepadInput.GetPreviousLeftStickY(inputSystem);
            return currentStickY <= -GamepadStickNavigationThreshold
                && previousStickY > -GamepadStickNavigationThreshold;
        }

        /// <summary>
        /// Returns whether the left stick crossed into its downward navigation zone this frame.
        /// </summary>
        bool WasLeftStickDownPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            short currentStickY = city.menu.DemoDiscGamepadInput.GetLeftStickY(inputSystem);
            short previousStickY = city.menu.DemoDiscGamepadInput.GetPreviousLeftStickY(inputSystem);
            return currentStickY >= GamepadStickNavigationThreshold
                && previousStickY < GamepadStickNavigationThreshold;
        }

        /// <summary>
        /// Returns whether the current frame requested level activation.
        /// </summary>
        bool WasAcceptPressed() {
            InputSystem inputSystem = Core.Instance.Input;
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Enter) || inputSystem.WasKeyPressed(Keys.J) || inputSystem.WasKeyPressed(Keys.Space)) {
                return true;
            }
#endif
            return city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South)
                || Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept);
        }

        /// <summary>
        /// Returns whether the current frame requested the detail screen to close.
        /// </summary>
        bool WasBackPressed() {
            InputSystem inputSystem = Core.Instance.Input;
#if DESKTOP_PLATFORM
            if (inputSystem.WasKeyPressed(Keys.Escape) || inputSystem.WasKeyPressed(Keys.Back) || inputSystem.WasKeyPressed(Keys.K)) {
                return true;
            }
#endif
            return city.menu.DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.East);
        }

        /// <summary>
        /// Finds one required child entity at the supplied authored child index.
        /// </summary>
        /// <param name="entity">Parent entity whose child collection should be indexed.</param>
        /// <param name="childIndex">Zero-based child index in the generated hierarchy.</param>
        /// <param name="description">Human-readable child description used for failure messages.</param>
        /// <returns>Required child entity at the supplied index.</returns>
        static Entity FindRequiredChildEntity(Entity entity, int childIndex, string description) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (childIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be non-negative.");
            } else if (string.IsNullOrWhiteSpace(description)) {
                throw new ArgumentException("Child description must be provided.", nameof(description));
            }

            if (entity.Children == null || childIndex >= entity.Children.Count) {
                throw new InvalidOperationException($"Tilt Trial level selector could not resolve required entity '{description}'.");
            }

            return entity.Children[childIndex];
        }

        /// <summary>
        /// Finds one required component on the supplied entity.
        /// </summary>
        static TComponent FindRequiredComponent<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException($"Tilt Trial level selector could not resolve component '{typeof(TComponent).Name}'.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TComponent typedComponent) {
                    return typedComponent;
                }
            }

            throw new InvalidOperationException($"Tilt Trial level selector could not resolve component '{typeof(TComponent).Name}'.");
        }
    }
}
