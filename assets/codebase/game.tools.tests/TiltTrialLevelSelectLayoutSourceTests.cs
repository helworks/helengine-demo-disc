namespace city.tests {
    /// <summary>
    /// Verifies the handheld Tilt Trial selector uses a two-stage full-width touch layout.
    /// </summary>
    public sealed class TiltTrialLevelSelectLayoutSourceTests {
        [Fact]
        public void Game_scene_factory_uses_two_stage_full_width_handheld_layout() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("viewportRoot.AddChild(CreateHandheldLevelSelectUiEntity());", source, StringComparison.Ordinal);
            Assert.Contains("CreateRoundedPanelEntity(entity, \"TiltTrialLevelSelectListPanel\", new float3(6f, 8f, 0f), new int2(244, 176)", source, StringComparison.Ordinal);
            Assert.Contains("CreateRoundedPanelEntity(entity, \"TiltTrialLevelSelectDetailsPanel\", new float3(6f, 8f, 0f), new int2(244, 176)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(0f, 3f + (index * 32f), 0f), new int2(244, 30)", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectAction.SelectStage", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectBackButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectPlayButton", source, StringComparison.Ordinal);
            Assert.Contains("detailsPanelEntity.Enabled = false;", source, StringComparison.Ordinal);
            Assert.Contains("backButtonEntity.Enabled = false;", source, StringComparison.Ordinal);
            Assert.Contains("playButtonEntity.Enabled = false;", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectAction.PlaySelectedStage", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldLevelSelectPreviewPanel", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldLevelSelectPreviewText", source, StringComparison.Ordinal);
            Assert.Contains("new float3(72f, 67f, 0f), new int2(112, 112)", source, StringComparison.Ordinal);
            Assert.Contains("new int2(96, 28)", source, StringComparison.Ordinal);
            Assert.Contains("new byte4(26, 40, 61, 255), new byte4(122, 147, 182, 255)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TiltTrialLevelSelectPreviewPlaceholder", source, StringComparison.Ordinal);

            string componentSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialLevelSelectComponent.cs");
            Assert.Contains("public void HandleAction(TiltTrialLevelSelectAction action, int stageIndex)", componentSource, StringComparison.Ordinal);
            Assert.Contains("ShowDetails();", componentSource, StringComparison.Ordinal);
            Assert.Contains("ShowStageList();", componentSource, StringComparison.Ordinal);
            Assert.Contains("PlaySelectedStage();", componentSource, StringComparison.Ordinal);
            Assert.Contains("public bool UseDetailsStage { get; set; }", componentSource, StringComparison.Ordinal);
            Assert.Contains("if (UseDetailsStage)", componentSource, StringComparison.Ordinal);
            Assert.Contains("ShowCombinedView();", componentSource, StringComparison.Ordinal);
            Assert.Contains("DetailBackButtonEntity.Enabled = true;", componentSource, StringComparison.Ordinal);
            Assert.Contains("DetailPlayButtonEntity.Enabled = true;", componentSource, StringComparison.Ordinal);
            Assert.Contains("DetailBackButtonEntity.Enabled = false;", componentSource, StringComparison.Ordinal);
            Assert.Contains("DetailPlayButtonEntity.Enabled = false;", componentSource, StringComparison.Ordinal);
            int combinedViewStart = componentSource.IndexOf("void ShowCombinedView()", StringComparison.Ordinal);
            int playSelectedStageStart = componentSource.IndexOf("public void PlaySelectedStage()", combinedViewStart, StringComparison.Ordinal);
            Assert.True(combinedViewStart >= 0);
            Assert.True(playSelectedStageStart > combinedViewStart);
            string combinedViewSource = componentSource.Substring(combinedViewStart, playSelectedStageStart - combinedViewStart);
            Assert.DoesNotContain("DetailBackButtonEntity.Enabled", combinedViewSource, StringComparison.Ordinal);
            Assert.DoesNotContain("DetailPlayButtonEntity.Enabled", combinedViewSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures every non-handheld platform shares one selector and the generator has no PS2-only branch.
        /// </summary>
        [Fact]
        public void Game_scene_factory_uses_one_shared_non_handheld_selector() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.DoesNotContain("Ps2", source, StringComparison.Ordinal);
            Assert.Contains("CreateLevelSelectUiEntity()", source, StringComparison.Ordinal);
            Assert.Contains("FixedSize = new int2(1280, 720)", source, StringComparison.Ordinal);
            Assert.Contains("ReferenceWidth = 1280", source, StringComparison.Ordinal);
            Assert.Contains("ReferenceHeight = 720", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectBackButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectPlayButton", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the desktop selector creates the detail Back and Play actions required by its runtime controller.
        /// </summary>
        [Fact]
        public void Game_scene_factory_uses_desktop_selector_detail_action_buttons() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
            int desktopMethodStart = source.IndexOf("EditorEntity CreateLevelSelectUiEntity()", StringComparison.Ordinal);

            Assert.True(desktopMethodStart >= 0);

            string desktopMethodSource = source.Substring(desktopMethodStart);
            Assert.Contains("CreateLevelSelectActionButton(detailsPanelEntity, \"TiltTrialLevelSelectBackButton\"", desktopMethodSource, StringComparison.Ordinal);
            Assert.Contains("CreateLevelSelectActionButton(detailsPanelEntity, \"TiltTrialLevelSelectPlayButton\"", desktopMethodSource, StringComparison.Ordinal);
            Assert.Contains("GetOrCreateExistencePlatformOverride(\"ds\").Exists = false", desktopMethodSource, StringComparison.Ordinal);
            Assert.Contains("GetOrCreateExistencePlatformOverride(\"3ds\").Exists = false", desktopMethodSource, StringComparison.Ordinal);
            Assert.DoesNotContain("GetOrCreateExistencePlatformOverride(\"ps2\")", desktopMethodSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures only the handheld selector enables the two-stage details flow.
        /// </summary>
        [Fact]
        public void Game_scene_factory_enables_details_stage_only_for_handheld_selector() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            int handheldMethodStart = source.IndexOf("EditorEntity CreateHandheldLevelSelectUiEntity()", StringComparison.Ordinal);
            int standardMethodStart = source.IndexOf("EditorEntity CreateLevelSelectUiEntity()", StringComparison.Ordinal);

            Assert.True(handheldMethodStart >= 0);
            Assert.True(standardMethodStart > handheldMethodStart);

            string handheldMethodSource = source.Substring(handheldMethodStart, standardMethodStart - handheldMethodStart);
            string standardMethodSource = source.Substring(standardMethodStart);

            Assert.Contains("UseDetailsStage = true", handheldMethodSource, StringComparison.Ordinal);
            Assert.Contains("entity.AddComponent(new DemoDiscReturnToMenuComponent());", handheldMethodSource, StringComparison.Ordinal);
            Assert.Contains("UseDetailsStage = false", standardMethodSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the shared selector keeps desktop medal targets readable one medal per line without changing handheld formatting.
        /// </summary>
        [Fact]
        public void Level_select_controller_formats_medals_one_per_line_on_all_platforms() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialLevelSelectComponent.cs");

            Assert.DoesNotContain("UseDetailsStage\n                ? $\"Targets G", source, StringComparison.Ordinal);
            Assert.Contains("$\"Gold  {FormatTimerSeconds(goldTimeSeconds)}\\nSilver {FormatTimerSeconds(silverTimeSeconds)}\\nBronze {FormatTimerSeconds(bronzeTimeSeconds)}\"", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the non-handheld selector presents platform-appropriate confirm and return button labels.
        /// </summary>
        [Fact]
        public void Level_select_controller_resolves_platform_button_hint() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialLevelSelectComponent.cs");

            Assert.Contains("ResolvePlatformHintText", source, StringComparison.Ordinal);
            Assert.Contains("gamecube", source, StringComparison.Ordinal);
            Assert.Contains("Play   B Menu", source, StringComparison.Ordinal);
            Assert.Contains("Cross Play   Circle Menu", source, StringComparison.Ordinal);
            Assert.Contains("A Play   B Menu", source, StringComparison.Ordinal);
            Assert.Contains("Enter Play   Esc Menu", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures non-handheld selector layouts share the static preview placeholder naming used by the combined view.
        /// </summary>
        [Fact]
        public void Game_scene_factory_uses_shared_non_handheld_preview_placeholders() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("TiltTrialLevelSelectPreviewPanel", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialLevelSelectPreviewText", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TiltTrialPs2LevelSelectTargetTimes", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TiltTrialPs2LevelSelectPreviewPanel", source, StringComparison.Ordinal);
            Assert.DoesNotContain("TiltTrialPs2LevelSelectPreviewText", source, StringComparison.Ordinal);
            Assert.Contains("\"Preview\"", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures selector updates reuse one catalog instead of allocating stage entries every frame.
        /// </summary>
        [Fact]
        public void Level_select_controller_reuses_one_catalog_across_frames() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialLevelSelectComponent.cs");

            Assert.Contains("readonly IReadOnlyList<TiltTrialLevelCatalogEntry> LevelEntries;", source, StringComparison.Ordinal);
            Assert.Contains("LevelEntries = TiltTrialLevelCatalog.CreateEntries();", source, StringComparison.Ordinal);
            Assert.Equal(1, CountOccurrences(source, "TiltTrialLevelCatalog.CreateEntries()"));
        }

        /// <summary>
        /// Ensures the detail screen exposes stick navigation between Back and Play actions.
        /// </summary>
        [Fact]
        public void Level_select_controller_supports_detail_action_stick_navigation() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialLevelSelectComponent.cs");

            Assert.Contains("int DetailActionIndex;", source, StringComparison.Ordinal);
            Assert.Contains("WasLeftStickUpPressed()", source, StringComparison.Ordinal);
            Assert.Contains("WasLeftStickDownPressed()", source, StringComparison.Ordinal);
            Assert.Contains("DetailActionIndex = DetailActionIndex == 0 ? 1 : 0;", source, StringComparison.Ordinal);
            Assert.Contains("if (DetailActionIndex == 0)", source, StringComparison.Ordinal);
            Assert.Contains("ApplyDetailActionSelection();", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the shared face-A and face-B keyboard bindings work on handheld selectors.
        /// </summary>
        [Fact]
        public void Level_select_controller_supports_shared_keyboard_face_buttons() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialLevelSelectComponent.cs");

            Assert.Contains("inputSystem.WasKeyPressed(Keys.J)", source, StringComparison.Ordinal);
            Assert.Contains("inputSystem.WasKeyPressed(Keys.K)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Counts exact non-overlapping occurrences of one source fragment.
        /// </summary>
        /// <param name="source">Source text to inspect.</param>
        /// <param name="value">Fragment whose occurrences should be counted.</param>
        /// <returns>Number of exact fragment occurrences.</returns>
        static int CountOccurrences(string source, string value) {
            int count = 0;
            int searchStart = 0;
            while ((searchStart = source.IndexOf(value, searchStart, StringComparison.Ordinal)) >= 0) {
                count++;
                searchStart += value.Length;
            }

            return count;
        }
    }
}
