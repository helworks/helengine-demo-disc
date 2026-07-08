using helengine;

namespace city.game {
    /// <summary>
    /// Drives the dedicated Tilt Trial level-select scene.
    /// </summary>
    public sealed class TiltTrialLevelSelectComponent : UpdateComponent {
        readonly List<RoundedRectComponent> RowBackgrounds;
        readonly List<TextComponent> RowLabels;

        TextComponent LevelNameTextComponent;
        TextComponent LevelTimerTextComponent;
        TextComponent LevelMedalTextComponent;
        TextComponent PreviewPlaceholderTextComponent;

        /// <summary>
        /// Gets the zero-based selected level index.
        /// </summary>
        public int SelectedIndex { get; private set; }

        /// <summary>
        /// Initializes one level-select controller.
        /// </summary>
        public TiltTrialLevelSelectComponent() {
            RowBackgrounds = new List<RoundedRectComponent>();
            RowLabels = new List<TextComponent>();
        }

        /// <summary>
        /// Processes selector navigation and applies the current level details to the authored UI.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("TiltTrialLevelSelectComponent requires an attached selector root entity.");
            }

            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = TiltTrialLevelCatalog.CreateEntries();
            ResolveUiBindingsWhenNeeded(levels.Count);

            if (WasNavigatePreviousPressed()) {
                SelectedIndex = SelectedIndex <= 0 ? levels.Count - 1 : SelectedIndex - 1;
            } else if (WasNavigateNextPressed()) {
                SelectedIndex = SelectedIndex >= levels.Count - 1 ? 0 : SelectedIndex + 1;
            } else if (WasAcceptPressed()) {
                Core.Instance.SceneManager.LoadScene(levels[SelectedIndex].SceneId, SceneLoadMode.Single);
                return;
            }

            ApplySelectionToUi(levels[SelectedIndex]);
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
            ApplyLevelMedalText(selectedLevel.GoldTimeSeconds, selectedLevel.SilverTimeSeconds, selectedLevel.BronzeTimeSeconds);
            ApplyPreviewState(selectedLevel.PreviewTexturePath);
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

            Entity listPanelEntity = FindRequiredChildEntity(Parent, 0, "Tilt Trial selector list panel");
            Entity detailsPanelEntity = FindRequiredChildEntity(Parent, 1, "Tilt Trial selector details panel");
            Entity previewPanelEntity = FindRequiredChildEntity(detailsPanelEntity, 3, "Tilt Trial selector preview panel");

            LevelNameTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(detailsPanelEntity, 0, "Tilt Trial selector level name text"));
            LevelTimerTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(detailsPanelEntity, 1, "Tilt Trial selector level timer text"));
            LevelMedalTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(detailsPanelEntity, 2, "Tilt Trial selector level medal text"));
            PreviewPlaceholderTextComponent = FindRequiredComponent<TextComponent>(FindRequiredChildEntity(previewPanelEntity, 0, "Tilt Trial selector preview placeholder text"));

            for (int rowIndex = 0; rowIndex < expectedRowCount; rowIndex++) {
                Entity rowEntity = FindRequiredChildEntity(listPanelEntity, rowIndex, $"Tilt Trial selector row {rowIndex + 1}");
                RowBackgrounds.Add(FindRequiredComponent<RoundedRectComponent>(rowEntity));
                RowLabels.Add(FindRequiredComponent<TextComponent>(FindRequiredChildEntity(rowEntity, 0, $"Tilt Trial selector row {rowIndex + 1} label")));
            }
        }

        /// <summary>
        /// Updates the selected row visuals and row label text.
        /// </summary>
        /// <param name="selectedLevelId">Stable selected level id.</param>
        void ApplyRowSelectionState(string selectedLevelId) {
            IReadOnlyList<TiltTrialLevelCatalogEntry> levels = TiltTrialLevelCatalog.CreateEntries();
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
        /// Applies the selected level name to the details panel.
        /// </summary>
        void ApplyLevelNameText(string displayName) {
            LevelNameTextComponent.Text = displayName ?? string.Empty;
        }

        /// <summary>
        /// Applies the selected level start time to the details panel.
        /// </summary>
        void ApplyLevelTimerText(float startTimeSeconds) {
            LevelTimerTextComponent.Text = $"Start {FormatTimerSeconds(startTimeSeconds)}";
        }

        /// <summary>
        /// Applies the selected level medal thresholds to the details panel.
        /// </summary>
        void ApplyLevelMedalText(float goldTimeSeconds, float silverTimeSeconds, float bronzeTimeSeconds) {
            LevelMedalTextComponent.Text = $"Gold  {FormatTimerSeconds(goldTimeSeconds)}\nSilver {FormatTimerSeconds(silverTimeSeconds)}\nBronze {FormatTimerSeconds(bronzeTimeSeconds)}";
        }

        /// <summary>
        /// Applies preview fallback presentation for the selected level.
        /// </summary>
        void ApplyPreviewState(string previewTexturePath) {
            PreviewPlaceholderTextComponent.Text = string.IsNullOrWhiteSpace(previewTexturePath)
                ? "Preview Coming Soon"
                : previewTexturePath;
        }

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
            return inputSystem.WasKeyPressed(Keys.Up)
                || inputSystem.WasKeyPressed(Keys.W)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.DPadUp);
        }

        /// <summary>
        /// Returns whether the current frame requested the next selector row.
        /// </summary>
        bool WasNavigateNextPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Down)
                || inputSystem.WasKeyPressed(Keys.S)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.DPadDown);
        }

        /// <summary>
        /// Returns whether the current frame requested level activation.
        /// </summary>
        bool WasAcceptPressed() {
            InputSystem inputSystem = Core.Instance.Input;
            return inputSystem.WasKeyPressed(Keys.Enter)
                || inputSystem.WasKeyPressed(Keys.Space)
                || Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept);
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
