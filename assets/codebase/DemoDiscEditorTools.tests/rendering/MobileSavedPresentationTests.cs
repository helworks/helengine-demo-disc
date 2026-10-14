using DemoDisc.rendering;
using DemoDisc.testing;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Guards the saved rendering, benchmark, physics and Tilt Trial level scenes on disk, not the live generator output: the mobile
    /// presentation used to be written into the saved files by a separate command that every regeneration dropped, so
    /// these tests read the committed assets back and resolve them for the Android build target exactly as the
    /// packager does, through the project's platform groups and each entity's level order.
    /// </summary>
    public sealed class MobileSavedPresentationTests {
        /// <summary>
        /// Platform id of the only member of the mobile group, used to resolve scopes as an Android build would.
        /// </summary>
        const string AndroidPlatformId = "android";

        /// <summary>
        /// Desktop platform used to prove the mobile rules leave every other group on the common presentation.
        /// </summary>
        const string WindowsPlatformId = "windows";

        /// <summary>
        /// Resolver over the committed platform group tree.
        /// </summary>
        static readonly EditorOverrideScopeResolver Resolver = EditorOverrideScopeResolver.Load(DemoDiscTestProject.RootPath);

        /// <summary>
        /// Generated scenes that present the standard scene UI kit beside the shared instruction Blueprint.
        /// </summary>
        public static TheoryData<string> BlueprintInstructionScenes => new TheoryData<string> {
            "scenes/rendering/axis_test.helen",
            "scenes/rendering/axis_test2.helen",
            "scenes/rendering/colored_cube_grid.helen",
            "scenes/rendering/cube_test.helen",
            "scenes/rendering/directional_shadow_plaza.helen",
            "scenes/rendering/pbr_material_gallery.helen",
            "scenes/rendering/pbr_shadow_theater.helen",
            "scenes/rendering/pbr_textured_showcase.helen",
            "scenes/rendering/test_scene_matrix_render.helen",
            "scenes/rendering/textured_cube_grid.helen",
            "scenes/physics/test_scene_dynamic_mixed_stack.helen",
            "scenes/physics/test_scene_dynamic_sphere_stack.helen",
            "scenes/physics/test_scene_dynamic_stack_boxes.helen",
            "scenes/physics/test_scene_single_falling_cube.helen",
            "scenes/physics/test_scene_strict_rotated_box_compare.helen"
        };

        /// <summary>
        /// Physics validation scenes built from authored scene assets; they mount the shared instruction Blueprint
        /// beside the legacy panel like the playable showcases.
        /// </summary>
        public static TheoryData<string> ValidationInstructionScenes => new TheoryData<string> {
            "scenes/physics/test_scene_character_moving_platform.helen",
            "scenes/physics/test_scene_character_slope.helen",
            "scenes/physics/test_scene_character_steps.helen",
            "scenes/physics/test_scene_kinematic_push.helen",
            "scenes/physics/test_scene_mesh_ground_stability.helen",
            "scenes/physics/test_scene_render_only_slope.helen",
            "scenes/physics/test_scene_trigger_volume.helen"
        };

        /// <summary>
        /// Assets-relative path of the Tilt Trial console presentation Blueprint mounted by every level scene.
        /// </summary>
        const string TiltTrialConsolePresentationBlueprintPath = "blueprints/games/tilt/TiltTrialConsolePresentation.hblueprint";

        /// <summary>
        /// Tilt Trial level scenes, where the console presentation Blueprint supplies the HUD and lighting rig on mobile.
        /// </summary>
        public static TheoryData<string> TiltTrialLevelScenes => new TheoryData<string> {
            "scenes/games/tilt/tilt_trial_level_01.helen",
            "scenes/games/tilt/tilt_trial_level_02.helen",
            "scenes/games/tilt/tilt_trial_level_03.helen",
            "scenes/games/tilt/tilt_trial_level_04.helen",
            "scenes/games/tilt/tilt_trial_level_05.helen"
        };

        /// <summary>
        /// Every generated scene and Blueprint that carries the mobile presentation.
        /// </summary>
        public static TheoryData<string> MobilePresentationAssets {
            get {
                TheoryData<string> assets = new TheoryData<string> {
                    "scenes/benchmarks/max_objects.helen",
                    "scenes/rendering/software_path_tracer.helen",
                    "scenes/rendering/ray_tracing_soft_shadows.helen",
                    "scenes/rendering/ray_tracing_spheres.helen",
                    "scenes/rendering/ray_tracing_teapot.helen",
                    "blueprints/ui/ConsoleCameraLightInstructions.hblueprint",
                    TiltTrialConsolePresentationBlueprintPath
                };
                foreach (object[] row in TiltTrialLevelScenes) {
                    assets.Add((string)row[0]);
                }
                foreach (object[] row in BlueprintInstructionScenes) {
                    assets.Add((string)row[0]);
                }
                foreach (object[] row in ValidationInstructionScenes) {
                    assets.Add((string)row[0]);
                }

                return assets;
            }
        }

        /// <summary>
        /// Scenes whose authored overlay layout is sampled: their viewport roots must sit at the canvas origin.
        /// </summary>
        public static TheoryData<string> LayoutSampleScenes => new TheoryData<string> {
            "scenes/rendering/axis_test.helen",
            "scenes/rendering/test_scene_matrix_render.helen",
            "scenes/benchmarks/max_objects.helen",
            "scenes/physics/test_scene_single_falling_cube.helen",
            "scenes/physics/test_scene_kinematic_push.helen"
        };

        /// <summary>
        /// Checks Android swaps the legacy instruction panel for the shared Blueprint while Windows keeps both roots
        /// exactly as before.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        [Theory]
        [MemberData(nameof(BlueprintInstructionScenes))]
        [MemberData(nameof(ValidationInstructionScenes))]
        public void Android_presents_the_instruction_blueprint_instead_of_the_legacy_panel(string scenePath) {
            SceneAsset scene = LoadScene(scenePath);
            SceneEntityAsset legacy = FindRoot(scene, MobilePresentationAuthoringService.LegacyInstructionRootEntityName);
            SceneEntityAsset blueprint = FindRoot(scene, MobilePresentationAuthoringService.InstructionBlueprintRootEntityName);

            Assert.False(DemoDiscOverrideScopeReader.ExistsOnPlatform(legacy, AndroidPlatformId));
            Assert.True(DemoDiscOverrideScopeReader.ExistsOnPlatform(blueprint, AndroidPlatformId));
            Assert.True(DemoDiscOverrideScopeReader.ExistsOnPlatform(legacy, WindowsPlatformId));
        }

        /// <summary>
        /// Checks the standard scene UI resolves to density scaling, scalable FPS glyphs, the added FPS density
        /// viewport and the pinned light swatch reference on Android, and to the common values on Windows.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        [Theory]
        [MemberData(nameof(BlueprintInstructionScenes))]
        [MemberData(nameof(ValidationInstructionScenes))]
        public void Android_resolves_the_density_scaled_scene_ui(string scenePath) {
            SceneAsset scene = LoadScene(scenePath);
            SceneEntityAsset ui = FindUiRoot(scene);

            FPSComponent fps = ResolveComponent<FPSComponent>(ui, "helengine.FPSComponent", AndroidPlatformId);
            Assert.Equal(TextNativePixelMode.Disabled, fps.NativeFontPixels);
            Assert.NotEqual(TextNativePixelMode.Disabled, ResolveComponent<FPSComponent>(ui, "helengine.FPSComponent", WindowsPlatformId).NativeFontPixels);
            ViewportComponent fpsViewport = ResolveAddedComponent<ViewportComponent>(ui, MobilePresentationAuthoringService.FpsDensityViewportComponentKey, AndroidPlatformId);
            Assert.Equal(ViewportComponent.DensityScalingMode, fpsViewport.ScalingMode);
            Assert.Equal(MobilePresentationAuthoringService.MobileUiScale, fpsViewport.UiScale);
            Assert.Null(TryResolveAddedComponentRecord(ui, MobilePresentationAuthoringService.FpsDensityViewportComponentKey, WindowsPlatformId));

            SceneEntityAsset indicatorViewport = Find(ui, DemoDiscLightIndicatorOverlayFactory.IndicatorViewportEntityName);
            AssertDensityViewportOnAndroidOnly(indicatorViewport);

            SceneEntityAsset swatch = Find(ui, DemoDiscLightIndicatorOverlayFactory.IndicatorSwatchEntityName);
            DemoDiscLightToggleComponent toggle = ResolveComponent<DemoDiscLightToggleComponent>(ui,
                AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(DemoDiscLightToggleComponent)), AndroidPlatformId);
            Assert.Equal(swatch.Id, toggle.IndicatorSwatchEntityReference?.EntityId);
        }

        /// <summary>
        /// Checks every Micro SD platform shows exactly one instruction panel: the legacy panel steps aside and the
        /// shared console instruction Blueprint presents the controls.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        [Theory]
        [MemberData(nameof(BlueprintInstructionScenes))]
        [MemberData(nameof(ValidationInstructionScenes))]
        public void Micro_sd_presents_the_instruction_blueprint_instead_of_the_legacy_panel(string scenePath) {
            SceneAsset scene = LoadScene(scenePath);
            SceneEntityAsset legacy = FindRoot(scene, MobilePresentationAuthoringService.LegacyInstructionRootEntityName);
            SceneEntityAsset blueprint = FindRoot(scene, MobilePresentationAuthoringService.InstructionBlueprintRootEntityName);

            foreach (string platformId in MicroSdPlatformIds()) {
                Assert.False(DemoDiscOverrideScopeReader.ExistsOnPlatform(legacy, platformId), "The legacy panel must step aside on " + platformId + ".");
                Assert.True(DemoDiscOverrideScopeReader.ExistsOnPlatform(blueprint, platformId), "The instruction Blueprint must present on " + platformId + ".");
            }
        }

        /// <summary>
        /// Reads the Micro SD group's platforms from the committed platform group tree.
        /// </summary>
        /// <returns>Platform ids of the Micro SD group; never empty.</returns>
        static IReadOnlyList<string> MicroSdPlatformIds() {
            EditorProjectPlatformGroupsDocument groups = new EditorProjectPlatformGroupsService(DemoDiscTestProject.RootPath).Read();
            EditorProjectPlatformGroupDefinition microSd = EditorProjectPlatformGroupsService.FindGroup(groups, DemoDiscOverrideScopes.MicroSdGroupId);
            Assert.NotNull(microSd);
            Assert.NotEmpty(microSd.PlatformIds);
            return microSd.PlatformIds;
        }

        /// <summary>
        /// Checks the software path tracer scenes scale their full-screen HUD on Android.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        [Theory]
        [InlineData("scenes/rendering/software_path_tracer.helen")]
        [InlineData("scenes/rendering/ray_tracing_soft_shadows.helen")]
        [InlineData("scenes/rendering/ray_tracing_spheres.helen")]
        [InlineData("scenes/rendering/ray_tracing_teapot.helen")]
        public void Android_scales_the_path_tracer_hud(string scenePath) {
            SceneAsset scene = LoadScene(scenePath);
            SceneEntityAsset camera = FindRoot(scene, "SoftwarePathTracerCamera");

            AssertDensityViewportOnAndroidOnly(Find(camera, "SoftwarePathTracerPresentationViewport"));
            TextComponent spp = ResolveComponent<TextComponent>(Find(camera, "SoftwarePathTracerSppText"), "helengine.TextComponent", AndroidPlatformId);
            Assert.Equal(TextNativePixelMode.Disabled, spp.NativeFontPixels);
        }

        /// <summary>
        /// Checks the shared instruction Blueprint resolves its density viewport and edge anchor on Android.
        /// </summary>
        [Fact]
        public void Android_resolves_the_instruction_blueprint_presentation() {
            BlueprintAsset blueprint = LoadBlueprint("blueprints/ui/ConsoleCameraLightInstructions.hblueprint");

            AssertDensityViewportOnAndroidOnly(blueprint.RootEntity);
            SceneEntityAsset panel = Find(blueprint.RootEntity, "ConsoleCameraLightInstructionsPanel");
            LayoutComponent anchor = ResolveAddedComponent<LayoutComponent>(panel, MobilePresentationAuthoringService.EdgeAnchorLayoutComponentKey, AndroidPlatformId);
            Assert.Equal((byte)(LayoutComponent.LeftAnchorFlag | LayoutComponent.BottomAnchorFlag), anchor.AnchorFlags);
        }

        /// <summary>
        /// Checks each Tilt Trial level keeps one HUD and one lighting rig on Android: the level's own session root and
        /// light roots step aside for the console presentation Blueprint there, and stay on Windows.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        [Theory]
        [MemberData(nameof(TiltTrialLevelScenes))]
        public void Android_drops_the_level_session_and_lights_for_the_presentation_blueprint(string scenePath) {
            SceneAsset scene = LoadScene(scenePath);
            foreach (string name in new[] { "TiltTrialSun", "TiltTrialFill", "TiltTrialAmbient", "TiltTrialUi" }) {
                SceneEntityAsset root = FindRoot(scene, name);
                Assert.False(DemoDiscOverrideScopeReader.ExistsOnPlatform(root, AndroidPlatformId), name + " must step aside on Android.");
                Assert.True(DemoDiscOverrideScopeReader.ExistsOnPlatform(root, WindowsPlatformId), name + " must stay on Windows.");
            }

            Assert.True(DemoDiscOverrideScopeReader.ExistsOnPlatform(FindRoot(scene, "TiltTrialConsolePresentation"), AndroidPlatformId));
        }

        /// <summary>
        /// Checks the Tilt Trial console presentation Blueprint drops its own camera on Android, where the level scene's
        /// camera presents the course, and keeps it on Windows.
        /// </summary>
        [Fact]
        public void Android_drops_the_tilt_presentation_blueprint_camera() {
            BlueprintAsset blueprint = LoadBlueprint(TiltTrialConsolePresentationBlueprintPath);
            SceneEntityAsset camera = Find(blueprint.RootEntity, "TiltTrialCamera");

            Assert.False(DemoDiscOverrideScopeReader.ExistsOnPlatform(camera, AndroidPlatformId));
            Assert.True(DemoDiscOverrideScopeReader.ExistsOnPlatform(camera, WindowsPlatformId));
        }

        /// <summary>
        /// Checks no regenerated asset still authors an Android rule at a platform scope. Android belongs to the mobile
        /// group, so a <c>platform:android</c> scope on a group-first entity no longer matches any build target.
        /// </summary>
        /// <param name="assetPath">Assets-relative scene or Blueprint path.</param>
        [Theory]
        [MemberData(nameof(MobilePresentationAssets))]
        public void Saved_assets_scope_android_rules_to_the_mobile_group(string assetPath) {
            foreach (SceneEntityAsset entity in EnumerateEntities(LoadRoots(assetPath))) {
                foreach (EditorOverrideScope scope in EnumerateScopes(entity)) {
                    Assert.False(DemoDiscOverrideScopeReader.PlatformIdOf(scope) == AndroidPlatformId,
                        entity.Name + " still names the android platform in " + scope + ".");
                    if (scope.Equals(MobilePresentationAuthoringService.MobileScope)) {
                        Assert.True(entity.HasOverrideLevelOrder && entity.OverrideLevelOrder.SequenceEqual(DemoDiscOverrideScopes.GroupFirstLevelOrder),
                            entity.Name + " carries a mobile rule without the group-first level order, so the rule never matches.");
                    }
                }
            }
        }

        /// <summary>
        /// Samples the authored overlay layout: every scene-local viewport root sits at the canvas origin with unit
        /// scale, the legacy panel at its authored position, and the light swatch on the panel's light row.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        [Theory]
        [MemberData(nameof(LayoutSampleScenes))]
        public void Saved_scene_keeps_its_authored_overlay_layout(string scenePath) {
            SceneAsset scene = LoadScene(scenePath);
            List<SceneEntityAsset> viewportRoots = EnumerateEntities(scene.RootEntities)
                .Where(entity => entity.Name == MobilePresentationAuthoringService.LegacyInstructionRootEntityName
                    || entity.Name == DemoDiscLightIndicatorOverlayFactory.IndicatorViewportEntityName
                    || entity.Name == "DemoDiscSceneLabelViewport")
                .ToList();
            Assert.NotEmpty(viewportRoots);
            foreach (SceneEntityAsset root in viewportRoots) {
                Assert.True(root.LocalPosition.X == 0f && root.LocalPosition.Y == 0f && root.LocalPosition.Z == 0f, root.Name + " must sit at the canvas origin.");
                Assert.True(root.LocalScale.X == 1f && root.LocalScale.Y == 1f, root.Name + " must keep unit scale.");
            }

            SceneEntityAsset legacy = scene.RootEntities.SingleOrDefault(root => root.Name == MobilePresentationAuthoringService.LegacyInstructionRootEntityName);
            if (legacy != null) {
                SceneEntityAsset panel = Find(legacy, "DemoSceneInstructionPanel");
                Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft, panel.LocalPosition.X);
                Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop, panel.LocalPosition.Y);
            }

            SceneEntityAsset swatch = Find(FindUiRoot(scene), DemoDiscLightIndicatorOverlayFactory.IndicatorSwatchEntityName);
            Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelLeft + DemoSceneInstructionOverlayFactory.LightSwatchPanelLeft, swatch.LocalPosition.X);
            Assert.Equal(DemoSceneInstructionOverlayFactory.DesktopInstructionPanelTop + DemoSceneInstructionOverlayFactory.LightSwatchPanelTop, swatch.LocalPosition.Y);
        }

        /// <summary>
        /// Asserts one entity's viewport resolves to density scaling with the approved mobile multiplier on Android
        /// and keeps a non-density mode on Windows.
        /// </summary>
        /// <param name="entity">Saved entity that owns the viewport.</param>
        static void AssertDensityViewportOnAndroidOnly(SceneEntityAsset entity) {
            ViewportComponent android = ResolveComponent<ViewportComponent>(entity, "helengine.ViewportComponent", AndroidPlatformId);
            Assert.Equal(ViewportComponent.DensityScalingMode, android.ScalingMode);
            Assert.Equal(MobilePresentationAuthoringService.MobilePixelsPerCentimeter, android.PixelsPerCentimeter);
            Assert.Equal(MobilePresentationAuthoringService.MobileUiScale, android.UiScale);
            Assert.NotEqual(ViewportComponent.DensityScalingMode, ResolveComponent<ViewportComponent>(entity, "helengine.ViewportComponent", WindowsPlatformId).ScalingMode);
        }

        /// <summary>
        /// Builds one platform's override target path under the entity's level order.
        /// </summary>
        /// <param name="entity">Saved entity whose level order applies.</param>
        /// <param name="platformId">Build target platform.</param>
        /// <returns>The target path the packager matches scopes against.</returns>
        static EditorOverrideScope BuildTarget(SceneEntityAsset entity, string platformId) {
            IReadOnlyList<SceneOverrideScopeStepKind> levelOrder = Resolver.ResolveLevelOrder(entity.HasOverrideLevelOrder ? entity.OverrideLevelOrder : null);
            return Resolver.BuildTargetPath(levelOrder, platformId, string.Empty);
        }

        /// <summary>
        /// Deserializes the value one component resolves to on a build target: the deepest matching override payload,
        /// or the common payload when no override scope matches.
        /// </summary>
        /// <param name="entity">Saved entity that owns the component.</param>
        /// <param name="typeId">Persisted component type id.</param>
        /// <param name="platformId">Build target platform.</param>
        /// <returns>The resolved component value.</returns>
        static TComponent ResolveComponent<TComponent>(SceneEntityAsset entity, string typeId, string platformId) where TComponent : Component {
            ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
            SceneComponentAssetRecord record = Assert.Single(entity.Components, item => item.ComponentTypeId == typeId);
            SceneComponentAssetRecord baseRecord = payloads.UnwrapBaseRecord(record);
            byte[] payload = baseRecord.Payload;
            if (EditorOverrideScopeResolver.TrySelectDeepest(payloads.ReadOverrideStates(record).ToList(), state => state.Scope, BuildTarget(entity, platformId), out EntityComponentPlatformOverrideState selected)) {
                payload = selected.Payload;
            }

            SceneComponentAssetRecord resolved = new SceneComponentAssetRecord {
                ComponentTypeId = baseRecord.ComponentTypeId,
                ComponentKey = baseRecord.ComponentKey,
                ComponentIndex = baseRecord.ComponentIndex,
                Payload = payload
            };
            return (TComponent)new ComponentPersistenceRegistry().GetDescriptor(typeId).DeserializeComponent(resolved, new EntitySaveComponent(), null);
        }

        /// <summary>
        /// Deserializes one component the entity gains on a build target under a stable key.
        /// </summary>
        /// <param name="entity">Saved entity that owns the component addition.</param>
        /// <param name="componentKey">Stable key of the added component.</param>
        /// <param name="platformId">Build target platform.</param>
        /// <returns>The added component.</returns>
        static TComponent ResolveAddedComponent<TComponent>(SceneEntityAsset entity, string componentKey, string platformId) where TComponent : Component {
            SceneComponentAssetRecord record = TryResolveAddedComponentRecord(entity, componentKey, platformId);
            Assert.NotNull(record);
            return (TComponent)new ComponentPersistenceRegistry().GetDescriptor(record.ComponentTypeId).DeserializeComponent(record, new EntitySaveComponent(), null);
        }

        /// <summary>
        /// Finds the component addition the deepest matching component override of a build target carries.
        /// </summary>
        /// <param name="entity">Saved entity that owns the component overrides.</param>
        /// <param name="componentKey">Stable key of the added component.</param>
        /// <param name="platformId">Build target platform.</param>
        /// <returns>The added component record, or null when the target gains none under that key.</returns>
        static SceneComponentAssetRecord TryResolveAddedComponentRecord(SceneEntityAsset entity, string componentKey, string platformId) {
            SceneEntityPlatformComponentOverrideAsset[] overrides = entity.PlatformComponentOverrides ?? Array.Empty<SceneEntityPlatformComponentOverrideAsset>();
            if (!EditorOverrideScopeResolver.TrySelectDeepest(overrides, item => EditorOverrideScope.FromSteps(item.Scope), BuildTarget(entity, platformId), out SceneEntityPlatformComponentOverrideAsset selected)) {
                return null;
            }

            return selected.AddedComponents.FirstOrDefault(item => item.Component.ComponentKey == componentKey)?.Component;
        }

        /// <summary>
        /// Enumerates every override scope authored on one saved entity.
        /// </summary>
        /// <param name="entity">Saved entity to inspect.</param>
        /// <returns>Existence, transform, component-set and component-property scopes.</returns>
        static IEnumerable<EditorOverrideScope> EnumerateScopes(SceneEntityAsset entity) {
            foreach (SceneEntityPlatformExistenceOverrideAsset item in entity.PlatformExistenceOverrides ?? Array.Empty<SceneEntityPlatformExistenceOverrideAsset>()) {
                yield return EditorOverrideScope.FromSteps(item.Scope);
            }
            foreach (SceneEntityPlatformTransformOverrideAsset item in entity.PlatformTransformOverrides ?? Array.Empty<SceneEntityPlatformTransformOverrideAsset>()) {
                yield return EditorOverrideScope.FromSteps(item.Scope);
            }
            foreach (SceneEntityPlatformComponentOverrideAsset item in entity.PlatformComponentOverrides ?? Array.Empty<SceneEntityPlatformComponentOverrideAsset>()) {
                yield return EditorOverrideScope.FromSteps(item.Scope);
            }
            ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
            foreach (SceneComponentAssetRecord record in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                foreach (EntityComponentPlatformOverrideState state in payloads.ReadOverrideStates(record)) {
                    yield return state.Scope;
                }
            }
        }

        /// <summary>
        /// Loads the root entities of one committed scene or Blueprint.
        /// </summary>
        /// <param name="assetPath">Assets-relative scene or Blueprint path.</param>
        /// <returns>The asset's root entities.</returns>
        static SceneEntityAsset[] LoadRoots(string assetPath) {
            return assetPath.EndsWith(".hblueprint", StringComparison.Ordinal)
                ? new[] { LoadBlueprint(assetPath).RootEntity }
                : LoadScene(assetPath).RootEntities;
        }

        /// <summary>
        /// Loads one committed scene asset.
        /// </summary>
        /// <param name="scenePath">Assets-relative scene path.</param>
        static SceneAsset LoadScene(string scenePath) {
            using FileStream stream = File.OpenRead(DemoDiscTestProject.GetPath(("assets/" + scenePath).Split('/')));
            return Assert.IsType<SceneAsset>(global::helengine.AssetSerializer.Deserialize(stream));
        }

        /// <summary>
        /// Loads one committed Blueprint asset.
        /// </summary>
        /// <param name="blueprintPath">Assets-relative Blueprint path.</param>
        static BlueprintAsset LoadBlueprint(string blueprintPath) {
            using FileStream stream = File.OpenRead(DemoDiscTestProject.GetPath(("assets/" + blueprintPath).Split('/')));
            return Assert.IsType<BlueprintAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
        }

        /// <summary>
        /// Finds the single-screen scene UI root: the root that owns the light toggle.
        /// </summary>
        /// <param name="scene">Saved scene.</param>
        static SceneEntityAsset FindUiRoot(SceneAsset scene) {
            string toggleTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(DemoDiscLightToggleComponent));
            return Assert.Single(scene.RootEntities, root => root.Components.Any(record => record.ComponentTypeId == toggleTypeId));
        }

        /// <summary>
        /// Finds one top-level scene root by name.
        /// </summary>
        static SceneEntityAsset FindRoot(SceneAsset scene, string name) {
            return Assert.Single(scene.RootEntities, root => root.Name == name);
        }

        /// <summary>
        /// Finds the first entity with the supplied name in one subtree, depth first.
        /// </summary>
        static SceneEntityAsset Find(SceneEntityAsset root, string name) {
            SceneEntityAsset found = EnumerateEntities(new[] { root }).FirstOrDefault(entity => entity.Name == name);
            return found ?? throw new InvalidOperationException($"Saved asset is missing '{name}' beneath '{root.Name}'.");
        }

        /// <summary>
        /// Flattens entity subtrees depth first.
        /// </summary>
        static IEnumerable<SceneEntityAsset> EnumerateEntities(IEnumerable<SceneEntityAsset> roots) {
            foreach (SceneEntityAsset root in roots ?? Array.Empty<SceneEntityAsset>()) {
                yield return root;
                foreach (SceneEntityAsset child in EnumerateEntities(root.Children)) {
                    yield return child;
                }
            }
        }
    }
}
