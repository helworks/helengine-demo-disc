namespace city.tests {
    /// <summary>
    /// Verifies the generated Tilt Trial scene source emits selectors, gameplay levels, and presentation Blueprints.
    /// </summary>
    public sealed class TiltTrialSceneGenerationSourceTests {
        /// <summary>
        /// Ensures targeted Tilt Trial regeneration updates both selector variants without rewriting gameplay levels.
        /// </summary>
        [Fact]
        public void Targeted_tilt_trial_generation_writes_standard_and_handheld_selectors() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");
            int methodStart = source.IndexOf("public void GenerateTiltTrialScene(string projectRootPath)", StringComparison.Ordinal);

            Assert.True(methodStart >= 0);
            string methodSource = source.Substring(methodStart);
            Assert.Contains("factory.CreateTiltTrialScene()", methodSource, StringComparison.Ordinal);
            Assert.Contains("handheldLevelSelectSceneFactory.Create(factory)", methodSource, StringComparison.Ordinal);
            Assert.DoesNotContain("factory.CreateTiltTrialLevelScenes()", methodSource, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_generator_writes_selectors_gameplay_levels_and_presentation_blueprints() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");

            Assert.Contains("CreateTiltTrialScene()", source, StringComparison.Ordinal);
            Assert.Contains("sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelSelectScene);", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevelScenes()", source, StringComparison.Ordinal);
            Assert.Contains("sceneWriteService.WriteScene(projectRootPath, tiltTrialLevelScenes[index]);", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialGameplayPresentationBlueprintGenerator", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialHandheldLevelSelectSceneFactory", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_authors_level_settings_and_session_components() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("const string TiltTrialLevelSelectSceneAssetRelativePath = \"scenes/games/tilt/tilt_trial.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("const string TiltTrialGameplaySceneAssetDirectoryRelativePath = \"scenes/games/tilt\";", source, StringComparison.Ordinal);
            Assert.Contains("SceneAssetRelativePath = TiltTrialLevelSelectSceneAssetRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("SceneAssetRelativePath = BuildTiltTrialGameplaySceneAssetRelativePath(levelEntry.SceneId)", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialLevelSettingsComponent", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialSessionComponent", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialConsolePresentationRoot", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialHandheldPresentationRoot", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialPresentationActionComponent", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultRetryButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultExitButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialResultNextButton", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialStartOverlay", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialStartPrompt", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialStartPromptPrefixText", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialStartPromptIcon", source, StringComparison.Ordinal);
            Assert.Contains("TiltTrialStartPromptSuffixText", source, StringComparison.Ordinal);
            Assert.Contains("RequireIcon(ProjectRootPath, \"windows\", \"enter\", AssetAuthoringService)", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialStartPromptPlatformOverride", source, StringComparison.Ordinal);
            Assert.Contains("\"ps2\", \"ps2\", \"cross\"", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Press \\\"X\\\" to start", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltTrialLevelSelectComponent", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialCoinText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"Coins 0/0\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialTargetTimesText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltTrialLevelSelectTargetTimes\"", source, StringComparison.Ordinal);
            Assert.Contains("OwningCore.EntityFactory.CreateChild(entity, \"TiltTrialResultsOverlay\")", source, StringComparison.Ordinal);
            Assert.Contains("resultsOverlayEntity.LocalPosition = new float3(16f, 8f, 0f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 40f, 0.1f), new int2(200, 30)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 78f, 0.1f), new int2(200, 30)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 116f, 0.1f), new int2(200, 30)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(320f, 130f, 0f), new int2(640, 380)", source, StringComparison.Ordinal);
            Assert.Contains("new int2(420, 220)", source, StringComparison.Ordinal);
            Assert.Contains("new ReferenceCanvasFitComponent", source, StringComparison.Ordinal);
            Assert.Contains("ConfigureTiltTrialGoalTarget(stageRootEntity, playerSphereEntity);", source, StringComparison.Ordinal);
            Assert.Contains("if (parent.Children[childIndex] is EditorEntity childEntity", source, StringComparison.Ordinal);
            Assert.DoesNotContain("child?.Name", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Nintendo DS start icon is promoted above opaque bottom-screen panel sprites.
        /// </summary>
        [Fact]
        public void Game_scene_factory_promotes_ds_start_icon_to_foreground_obj_priority() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("const byte NintendoDsStartPromptIconRenderOrder = 220;", source, StringComparison.Ordinal);
            Assert.Contains("string.Equals(platformId, NintendoDsPlatformId, StringComparison.Ordinal)", source, StringComparison.Ordinal);
            Assert.Contains("overrideComponent.RenderOrder2D = NintendoDsStartPromptIconRenderOrder;", source, StringComparison.Ordinal);
            Assert.Contains("nameof(SpriteComponent.RenderOrder2D)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the coin blueprint carries a trigger volume twice as tall as its horizontal footprint and the factory wires it per instance.
        /// </summary>
        [Fact]
        public void Game_scene_factory_authors_tall_box_trigger_for_collectible_coins() {
            string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
            string coinGeneratorSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\SplitPlayGoldenCoinAssetGenerator.cs");

            Assert.Contains("Size = new float3(3f, 6f, 3f)", coinGeneratorSource, StringComparison.Ordinal);
            Assert.Contains("triggerCollider.IsTrigger = true;", coinGeneratorSource, StringComparison.Ordinal);
            Assert.Contains("ComponentKey = SplitPlayGoldenCoinAssetGenerator.TriggerObserverComponentKey", factorySource, StringComparison.Ordinal);
            Assert.Contains("PropertyName = \"TargetEntityReference\"", factorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the Tilt Trial front door is generated as a title shell before the existing level selector.
        /// </summary>
        [Fact]
        public void Tilt_trial_front_door_generates_title_options_and_level_select_panels() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateTiltPlayShellUiEntity()", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayTitlePanel\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TILT TRIAL\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayOptionsPanel\"", source, StringComparison.Ordinal);
            Assert.Contains("\"Settings coming soon\"", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayLevelSelectPanel\"", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltPlayMenuComponent()", source, StringComparison.Ordinal);
            Assert.Contains("new city.game.TiltPlayMenuActionComponent", source, StringComparison.Ordinal);
            Assert.Contains("new float3(12f, 2f, 0.1f)", source, StringComparison.Ordinal);
            Assert.Contains("new int2(size.X - 16, size.Y - 8)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the textured title panel keeps the stable role required by the runtime Tilt Play menu controller.
        /// </summary>
        [Fact]
        public void Tilt_trial_front_door_title_panel_has_its_required_presentation_role() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("titlePanel.AddComponent(new city.game.TiltTrialPresentationRoleComponent {", source, StringComparison.Ordinal);
            Assert.Contains("Role = \"TiltPlayTitlePanel\"", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies title-screen sprites retain their stable roles for runtime menu dependency resolution.
        /// </summary>
        [Fact]
        public void Tilt_trial_front_door_sprite_factory_assigns_the_sprite_role() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("spriteEntity.AddComponent(new city.game.TiltTrialPresentationRoleComponent {", source, StringComparison.Ordinal);
            Assert.Contains("Role = name", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Tilt Trial front door emits the approved game-show arena title treatment without changing menu actions.
        /// </summary>
        [Fact]
        public void Tilt_trial_front_door_generates_the_arena_title_treatment() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
            string menuSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltPlayMenuComponent.cs");

            Assert.Contains("CreateTiltPlayViewportBackgroundEntity()", source, StringComparison.Ordinal);
            Assert.Contains("\"TiltPlayPlayButton\"", source, StringComparison.Ordinal);
            Assert.Contains("new int2(520, 72)", source, StringComparison.Ordinal);
            Assert.Contains("city.game.TiltPlayMenuAction.Play", source, StringComparison.Ordinal);
            Assert.Contains("city.game.TiltPlayMenuAction.Options", source, StringComparison.Ordinal);
            Assert.Contains("city.game.TiltPlayMenuAction.BackToDemoDisc", source, StringComparison.Ordinal);
            Assert.Contains("ApplyTitleActionSelection", menuSource, StringComparison.Ordinal);
            Assert.Contains("PlayButtonSelectedOverlay.Enabled", menuSource, StringComparison.Ordinal);
            Assert.Contains("OptionsButtonSelectedOverlay.Enabled", menuSource, StringComparison.Ordinal);
            Assert.Contains("DemoDiscButtonSelectedOverlay.Enabled", menuSource, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_play_title_background_uses_a_screen_viewport_outside_the_fitted_shell() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateTiltPlayViewportBackgroundEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltPlayShellUiEntity()", source, StringComparison.Ordinal);
            Assert.Contains("Entity CreateTiltPlayViewportBackgroundEntity()", source, StringComparison.Ordinal);
            Assert.Contains("BindingMode = ViewportComponent.ScreenBindingMode", source, StringComparison.Ordinal);
            Assert.Contains("LayoutSpace = LayoutComponent.CameraViewportLayoutSpace", source, StringComparison.Ordinal);
            Assert.Contains("SetAnchorDistances(left: 0f, top: 0f, right: 0f, bottom: 0f)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateTiltPlayTitleBackgroundSprite(titlePanel);", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Tilt Trial title screen uses authored sprite textures instead of renderer-specific rounded rectangles.
        /// </summary>
        [Fact]
        public void Tilt_trial_front_door_uses_authored_png_sprites_for_title_chrome() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
            string menuSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltPlayMenuComponent.cs");

            Assert.Contains("images/ui/tilt_trial/title/background.png", source, StringComparison.Ordinal);
            Assert.Contains("images/ui/tilt_trial/title/button_primary.png", source, StringComparison.Ordinal);
            Assert.Contains("images/ui/tilt_trial/title/button_primary_selected.png", source, StringComparison.Ordinal);
            Assert.Contains("images/ui/tilt_trial/title/button_secondary_options.png", source, StringComparison.Ordinal);
            Assert.Contains("images/ui/tilt_trial/title/button_secondary_options_selected.png", source, StringComparison.Ordinal);
            Assert.Contains("images/ui/tilt_trial/title/button_secondary_demo_disc.png", source, StringComparison.Ordinal);
            Assert.Contains("images/ui/tilt_trial/title/button_secondary_demo_disc_selected.png", source, StringComparison.Ordinal);
            Assert.Contains("new SpriteComponent", source, StringComparison.Ordinal);
            Assert.Contains("CreateFileSystemTexture", source, StringComparison.Ordinal);
            Assert.Contains("Entity PlayButtonSelectedOverlay", menuSource, StringComparison.Ordinal);
            Assert.Contains("ApplyTitleActionSelection", menuSource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the dedicated render test scene isolates one clipping-probe cube with deterministic camera controls.
        /// </summary>
        [Fact]
        public void Level_01_render_test_scene_uses_one_cube_light_camera_and_fps_only() {
            string catalogSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneCatalog.cs");
            string generatorSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneGenerator.cs");
            string preparationSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneAssetPreparationService.cs");
            string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("TiltTrialLevel01RenderTestSceneId", catalogSource, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel01RenderTestScene", generatorSource, StringComparison.Ordinal);
            Assert.Contains("CreateLevel01RenderOnlyCourseBoxEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("new FPSComponent", factorySource, StringComparison.Ordinal);
            Assert.Contains("test_scene_tilt_trial_level_01_render.helen", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateLevel01RenderOnlyStageRootEntity", factorySource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialClippingProbeModel = tiltTrialClippingProbeModel", preparationSource, StringComparison.Ordinal);
            Assert.Contains("TiltTrialClippingProbeMaterial = tiltTrialClippingProbeMaterial", preparationSource, StringComparison.Ordinal);
            Assert.Contains("Model = TiltTrialClippingProbeModel", factorySource, StringComparison.Ordinal);
            Assert.Contains("Materials = new[] { TiltTrialClippingProbeMaterial }", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateFileSystemModel(TiltTrialClippingProbeModelFactory.ModelRelativePath)", factorySource, StringComparison.Ordinal);
            Assert.Contains("CreateFileSystemMaterial(TiltTrialClippingProbeMaterialFactory.MaterialRelativePath)", factorySource, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateLevel01RenderOnlyCourseBoxEntity(\"ClipProbeCube\", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true)", factorySource, StringComparison.Ordinal);
            Assert.Contains("entity.LocalPosition = new float3(6f, 4f, 8f)", factorySource, StringComparison.Ordinal);
            Assert.Contains("float4.CreateFromYawPitchRoll(0.6435011f, -0.3805064f, 0f, out orientation)", factorySource, StringComparison.Ordinal);
            Assert.Contains("ManualYawSpeedRadians = 0f", factorySource, StringComparison.Ordinal);
            Assert.Contains("ManualPitchSpeedRadians = 0f", factorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies the render-only stage excludes the former course, sphere, coin, and flag root attachments.
        /// </summary>
        [Fact]
        public void Level_01_render_test_scene_excludes_unrelated_visual_roots() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.DoesNotContain("entity.AddChild(CreateLevel01RenderOnlyPlayerSphereEntity())", source, StringComparison.Ordinal);
            Assert.DoesNotContain("entity.AddChild(CreateLevel01RenderOnlyCoinEntity(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("entity.AddChild(CreateLevel01RenderOnlyGoalFlagEntity(", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures gameplay sessions bind and refresh target-time text from the current level metadata.
        /// </summary>
        [Fact]
        public void Gameplay_session_refreshes_target_times_from_current_level() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialSessionComponent.cs");

            Assert.Contains("TargetTimesTextComponent", source, StringComparison.Ordinal);
            Assert.Contains("TryFindNamedEntity(Parent, \"TiltTrialTargetTimesText\")", source, StringComparison.Ordinal);
            Assert.Contains("RefreshTargetTimesText();", source, StringComparison.Ordinal);
            Assert.Contains("CurrentLevel.GoldTimeSeconds", source, StringComparison.Ordinal);
            Assert.Contains("CurrentLevel.SilverTimeSeconds", source, StringComparison.Ordinal);
            Assert.Contains("CurrentLevel.BronzeTimeSeconds", source, StringComparison.Ordinal);
            Assert.Contains("Targets G", source, StringComparison.Ordinal);
        }
    }
}
