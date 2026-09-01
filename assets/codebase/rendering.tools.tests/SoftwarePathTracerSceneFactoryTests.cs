using city.rendering;
using city.menu;
using city.rendering.tools;
using city.scene.tools;
using helengine;
using helengine.editor;
using System.Reflection;

namespace city.tests {
    /// <summary>
    /// Verifies the fixed common authoring graph used by the software path tracer.
    /// </summary>
    public sealed class SoftwarePathTracerSceneFactoryTests {
        const string SceneId = "scenes/rendering/software_path_tracer.helen";
        const string SceneIdentity = "1000000000000000000000000000001f";

        [Fact]
        public void Creates_the_fixed_eight_model_cornell_graph_without_mesh_components() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            SceneAssetReference cubeReference = EngineSceneAssetReferenceFactory.CreateCubeModel();
            FontAsset hudFont = CreateHudFont();
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                cubeReference,
                hudFont);

            Assert.Equal(SceneId, definition.SceneId);
            Assert.Equal(SceneIdentity, ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(SceneId));

            Entity[] entities = FlattenEntities(definition.RootEntities).ToArray();
            Entity controllerEntity = Assert.Single(entities.Where(entity => entity.Components.OfType<SoftwarePathTracerComponent>().Any()));
            Entity[] tracedEntities = entities
                .Where(entity => entity.Components.OfType<SoftwareModelComponent>().Any())
                .ToArray();
            Assert.Equal(8, tracedEntities.Length);
            Assert.All(tracedEntities, entity => Assert.True(IsDescendant(controllerEntity, entity)));

            AssertSurface(tracedEntities, "SoftwarePathTracerFloor", new float3(0f, -1f, 0f), new float3(2f, 0.05f, 2f), 0f, new float3(0.75f, 0.75f, 0.75f));
            AssertSurface(tracedEntities, "SoftwarePathTracerCeiling", new float3(0f, 1f, 0f), new float3(2f, 0.05f, 2f), 0f, new float3(0.75f, 0.75f, 0.75f));
            AssertSurface(tracedEntities, "SoftwarePathTracerBack", new float3(0f, 0f, -1f), new float3(2f, 2f, 0.05f), 0f, new float3(0.75f, 0.75f, 0.75f));
            AssertSurface(tracedEntities, "SoftwarePathTracerLeft", new float3(-1f, 0f, 0f), new float3(0.05f, 2f, 2f), 0f, new float3(0.75f, 0.05f, 0.05f));
            AssertSurface(tracedEntities, "SoftwarePathTracerRight", new float3(1f, 0f, 0f), new float3(0.05f, 2f, 2f), 0f, new float3(0.05f, 0.75f, 0.05f));
            AssertSurface(tracedEntities, "SoftwarePathTracerShortBox", new float3(-0.35f, -0.55f, 0.15f), new float3(0.6f, 0.9f, 0.6f), 0.30f, new float3(0.75f, 0.75f, 0.75f));
            AssertSurface(tracedEntities, "SoftwarePathTracerTallBox", new float3(0.38f, -0.25f, 0.35f), new float3(0.55f, 1.45f, 0.55f), -0.28f, new float3(0.75f, 0.75f, 0.75f));

            Entity emitter = Assert.Single(tracedEntities.Where(entity => Component<SoftwareModelComponent>(entity).Materials.Single().EmissionStrength > 0f));
            Assert.Equal("SoftwarePathTracerEmitter", EntityName(emitter));
            AssertVector(emitter.LocalPosition, new float3(0f, 0.93f, 0f));
            AssertVector(emitter.LocalScale, new float3(0.55f, 0.025f, 0.45f));
            AssertVector(emitter.LocalOrientation, float4.Identity);
            AssertVector(Component<SoftwareModelComponent>(emitter).Materials.Single().DiffuseColor, float3.Zero);
            AssertVector(Component<SoftwareModelComponent>(emitter).Materials.Single().EmissionColor, float3.One);
            Assert.Equal(14f, Component<SoftwareModelComponent>(emitter).Materials.Single().EmissionStrength);

            foreach (Entity entity in tracedEntities) {
                SoftwareModelComponent model = Component<SoftwareModelComponent>(entity);
                Assert.Equal(cubeReference.SourceKind, model.ModelReference.SourceKind);
                Assert.Equal(cubeReference.RelativePath, model.ModelReference.RelativePath);
                Assert.Equal(cubeReference.ProviderId, model.ModelReference.ProviderId);
                Assert.Equal(cubeReference.AssetId, model.ModelReference.AssetId);
                Assert.Equal(cubeReference.ContentHash, model.ModelReference.ContentHash);
                Assert.Single(model.Materials);
                Assert.Empty(entity.Components.OfType<MeshComponent>());
            }
        }

        [Fact]
        public void Controller_root_is_consumable_by_the_recursive_software_trace_scene_scan() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            SceneAssetReference cubeReference = EngineSceneAssetReferenceFactory.CreateCubeModel();
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                cubeReference,
                CreateHudFont());
            Entity controllerEntity = Assert.Single(
                FlattenEntities(definition.RootEntities)
                    .Where(entity => entity.Components.OfType<SoftwarePathTracerComponent>().Any()));
            RecursiveScanModelSource source = new RecursiveScanModelSource(cubeReference);

            SoftwareTraceScene scene = SoftwareTraceScene.Build(new[] { controllerEntity }, source);

            Assert.Equal(8 * 12, scene.Triangles.Length);
            Assert.Equal(1, source.LoadCount);
            Assert.NotNull(source.LastAsset);
            Assert.Null(source.LastAsset.Positions);
            Assert.Null(source.LastAsset.Indices16);
            Assert.Null(source.LastAsset.Submeshes);
        }

        [Fact]
        public void Creates_the_presentation_camera_hud_and_stably_wired_controller() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            FontAsset hudFont = CreateHudFont();
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                EngineSceneAssetReferenceFactory.CreateCubeModel(),
                hudFont);
            Entity[] entities = FlattenEntities(definition.RootEntities).ToArray();

            Entity cameraEntity = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerCamera"));
            CameraComponent camera = Component<CameraComponent>(cameraEntity);
            Assert.Equal(EditorLayerMasks.SceneObjects, camera.LayerMask);
            Assert.Equal(new float4(0f, 0f, 1f, 1f), camera.Viewport);
            Assert.Equal(new float3(0f, 0f, 0f), cameraEntity.LocalPosition);

            Entity outputEntity = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerOutput"));
            Assert.Single(outputEntity.Components.OfType<SpriteComponent>());
            Assert.Equal(EditorLayerMasks.SceneObjects, outputEntity.LayerMask);

            Entity[] hudEntities = entities.Where(entity => entity.Components.OfType<TextComponent>().Any()
                && new[] { "SoftwarePathTracerSppText", "SoftwarePathTracerElapsedText", "SoftwarePathTracerRaysPerSecondText" }.Contains(EntityName(entity))).ToArray();
            Assert.Equal(3, hudEntities.Length);
            Assert.All(hudEntities, entity => {
                TextComponent text = Component<TextComponent>(entity);
                Assert.Same(hudFont, text.Font);
                Assert.NotEqual(string.Empty, text.Text);
                EntitySaveComponent saveComponent = Component<EntitySaveComponent>(entity);
                Assert.True(saveComponent.TryGetComponentState(text, out EntityComponentSaveState saveState));
                Assert.True(saveState.TryGetAssetReference("Font", out SceneAssetReference fontReference));
                Assert.NotNull(fontReference);
                Assert.Equal(SceneAssetReferenceSourceKind.FileSystem, fontReference.SourceKind);
                Assert.Equal("Fonts/DemoDiscBody.ttf", fontReference.RelativePath);
            });

            Entity controllerEntity = Assert.Single(entities.Where(entity => entity.Components.OfType<SoftwarePathTracerComponent>().Any()));
            SoftwarePathTracerComponent controller = Component<SoftwarePathTracerComponent>(controllerEntity);
            AssertVector(controller.TraceCameraOrigin, new float3(0f, 0f, 3f));
            AssertVector(controller.TraceCameraForward, new float3(0f, 0f, -1f));
            AssertVector(controller.TraceCameraRight, new float3(1f, 0f, 0f));
            AssertVector(controller.TraceCameraUp, new float3(0f, 1f, 0f));
            Assert.Equal(55f, controller.VerticalFieldOfViewDegrees);
            Assert.Equal(1f, controller.Exposure);
            uint outputId = SaveId(outputEntity);
            uint sppId = SaveId(hudEntities.Single(entity => EntityName(entity) == "SoftwarePathTracerSppText"));
            uint elapsedId = SaveId(hudEntities.Single(entity => EntityName(entity) == "SoftwarePathTracerElapsedText"));
            uint raysPerSecondId = SaveId(hudEntities.Single(entity => EntityName(entity) == "SoftwarePathTracerRaysPerSecondText"));
            uint[] targetIds = new[] { outputId, sppId, elapsedId, raysPerSecondId };
            Assert.All(targetIds, id => Assert.NotEqual(0u, id));
            Assert.Equal(targetIds.Length, targetIds.Distinct().Count());
            Assert.Equal(outputId, controller.OutputSpriteEntityReference.EntityId);
            Assert.Equal(sppId, controller.SppTextEntityReference.EntityId);
            Assert.Equal(elapsedId, controller.ElapsedTextEntityReference.EntityId);
            Assert.Equal(raysPerSecondId, controller.RaysPerSecondTextEntityReference.EntityId);

            Assert.DoesNotContain(entities, entity => entity.Components.OfType<DirectionalLightComponent>().Any());
            Assert.DoesNotContain(entities, entity => entity.Components.OfType<SpotLightComponent>().Any());
        }

        [Fact]
        public void Creates_one_reference_canvas_output_viewport_with_ds_and_3ds_presentation_overrides() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                EngineSceneAssetReferenceFactory.CreateCubeModel(),
                CreateHudFont());
            Entity[] entities = FlattenEntities(definition.RootEntities).ToArray();
            Entity cameraEntity = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerCamera"));
            Entity outputEntity = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerOutput"));
            SpriteComponent outputSprite = Component<SpriteComponent>(outputEntity);
            EntitySaveComponent outputSave = Component<EntitySaveComponent>(outputEntity);
            Entity presentationViewportEntity = Assert.Single(cameraEntity.Children.Where(entity => entity.Components.OfType<ViewportComponent>().Any()));
            ViewportComponent presentationViewport = Component<ViewportComponent>(presentationViewportEntity);

            Assert.Equal(ViewportComponent.AncestorCameraBindingMode, presentationViewport.BindingMode);
            Assert.Equal(ViewportComponent.ReferenceCanvasScalingMode, presentationViewport.ScalingMode);
            Assert.Equal(new int2(320, 240), presentationViewport.FixedSize);
            Assert.Equal(320, presentationViewport.ReferenceWidth);
            Assert.Equal(240, presentationViewport.ReferenceHeight);
            Assert.Contains(outputEntity, presentationViewportEntity.Children);
            Assert.Equal(new int2(320, 240), outputSprite.Size);
            Assert.Null(outputSprite.Texture);
            Assert.Equal(1, entities.Count(entity => entity.Components.OfType<SpriteComponent>().Any()));

            ComponentPlatformEditingService platformEditingService = new ComponentPlatformEditingService();
            SpriteComponent dsSprite = Assert.IsType<SpriteComponent>(platformEditingService.ResolveEditableComponent(outputSprite, outputSave, "ds"));
            Assert.Equal(new int2(256, 192), dsSprite.Size);
            ViewportComponent dsViewport = Assert.IsType<ViewportComponent>(platformEditingService.ResolveEditableComponent(presentationViewport, Component<EntitySaveComponent>(presentationViewportEntity), "ds"));
            Assert.Equal(new int2(256, 192), dsViewport.FixedSize);
            Assert.Equal(256, dsViewport.ReferenceWidth);
            Assert.Equal(192, dsViewport.ReferenceHeight);

            ViewportComponent threeDsViewport = Assert.IsType<ViewportComponent>(platformEditingService.ResolveEditableComponent(presentationViewport, Component<EntitySaveComponent>(presentationViewportEntity), "3ds"));
            Assert.Equal(new int2(400, 240), threeDsViewport.FixedSize);
            Assert.Equal(400, threeDsViewport.ReferenceWidth);
            Assert.Equal(240, threeDsViewport.ReferenceHeight);
            Assert.True(outputSave.TryGetTransformPlatformOverride("3ds", out SceneEntityPlatformTransformOverrideAsset outputTransform));
            Assert.True(outputTransform.HasLocalPositionOverride);
            AssertVector(outputTransform.LocalPosition, new float3(40f, 0f, 0f));
        }

        [Fact]
        public void Persists_ds_and_3ds_controller_hud_reference_overrides_without_duplicating_the_output_sprite() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                EngineSceneAssetReferenceFactory.CreateCubeModel(),
                CreateHudFont());
            Entity[] entities = FlattenEntities(definition.RootEntities).ToArray();
            Entity controllerEntity = Assert.Single(entities.Where(entity => entity.Components.OfType<SoftwarePathTracerComponent>().Any()));
            SoftwarePathTracerComponent commonController = Component<SoftwarePathTracerComponent>(controllerEntity);
            EntitySaveComponent controllerSave = Component<EntitySaveComponent>(controllerEntity);
            ComponentPlatformEditingService platformEditingService = new ComponentPlatformEditingService();
            Entity[] commonHudEntities = new[] {
                entities.Single(entity => EntityName(entity) == "SoftwarePathTracerSppText"),
                entities.Single(entity => EntityName(entity) == "SoftwarePathTracerElapsedText"),
                entities.Single(entity => EntityName(entity) == "SoftwarePathTracerRaysPerSecondText")
            };
            Entity[] handheldHudEntities = new[] {
                entities.Single(entity => EntityName(entity) == "SoftwarePathTracerHandheldSppText"),
                entities.Single(entity => EntityName(entity) == "SoftwarePathTracerHandheldElapsedText"),
                entities.Single(entity => EntityName(entity) == "SoftwarePathTracerHandheldRaysPerSecondText")
            };
            uint[] commonHudIds = commonHudEntities.Select(SaveId).ToArray();
            uint[] handheldHudIds = handheldHudEntities.Select(SaveId).ToArray();
            Assert.All(commonHudIds.Concat(handheldHudIds), id => Assert.NotEqual(0u, id));
            Assert.Equal(3, commonHudIds.Distinct().Count());
            Assert.Equal(3, handheldHudIds.Distinct().Count());
            Assert.Empty(commonHudIds.Intersect(handheldHudIds));

            foreach (string platformId in new[] { "ds", "3ds" }) {
                SoftwarePathTracerComponent effectiveController = Assert.IsType<SoftwarePathTracerComponent>(platformEditingService.ResolveEditableComponent(commonController, controllerSave, platformId));
                Assert.Equal(handheldHudIds[0], effectiveController.SppTextEntityReference.EntityId);
                Assert.Equal(handheldHudIds[1], effectiveController.ElapsedTextEntityReference.EntityId);
                Assert.Equal(handheldHudIds[2], effectiveController.RaysPerSecondTextEntityReference.EntityId);
                Assert.DoesNotContain(effectiveController.SppTextEntityReference.EntityId, commonHudIds);
                Assert.DoesNotContain(effectiveController.ElapsedTextEntityReference.EntityId, commonHudIds);
                Assert.DoesNotContain(effectiveController.RaysPerSecondTextEntityReference.EntityId, commonHudIds);
            }

            Assert.Equal(1, entities.Count(entity => entity.Components.OfType<SpriteComponent>().Any()));
        }

        [Fact]
        public void Creates_mutually_exclusive_desktop_and_handheld_hud_trees_with_platform_return_owners() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                EngineSceneAssetReferenceFactory.CreateCubeModel(),
                CreateHudFont());
            Entity[] entities = FlattenEntities(definition.RootEntities).ToArray();
            Assert.Equal(2, entities.Count(entity => entity.Components.OfType<CameraComponent>().Any()));
            Entity topCamera = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerCamera"));
            CameraComponent topCameraComponent = Component<CameraComponent>(topCamera);
            Assert.Equal(0, topCameraComponent.CameraDrawOrder);
            Entity bottomCamera = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerBottomScreenCamera"));
            CameraComponent bottomCameraComponent = Component<CameraComponent>(bottomCamera);
            Assert.Equal(1, bottomCameraComponent.CameraDrawOrder);
            Assert.Equal(new float4(0f, 1f, 1f, 1f), bottomCameraComponent.Viewport);
            Entity bottomViewportEntity = Assert.Single(bottomCamera.Children.Where(entity => entity.Components.OfType<ViewportComponent>().Any()));
            ViewportComponent bottomViewport = Component<ViewportComponent>(bottomViewportEntity);
            Assert.Equal(ViewportComponent.AncestorCameraBindingMode, bottomViewport.BindingMode);
            Assert.Equal(ViewportComponent.ReferenceCanvasScalingMode, bottomViewport.ScalingMode);
            Assert.Equal(new int2(256, 192), bottomViewport.FixedSize);
            Assert.Equal(256, bottomViewport.ReferenceWidth);
            Assert.Equal(192, bottomViewport.ReferenceHeight);

            Entity desktopRoot = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerDesktopHudRoot"));
            Entity handheldRoot = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerHandheldHudRoot"));
            Assert.True(IsDescendant(bottomViewportEntity, handheldRoot));
            Assert.True(IsDescendant(topCamera, desktopRoot));
            Entity desktopPanel = Assert.Single(desktopRoot.Children.Where(entity => entity.Components.OfType<RoundedRectComponent>().Any()));
            Assert.Equal(new byte4(18, 27, 43, 220), Component<RoundedRectComponent>(desktopPanel).FillColor);
            Entity desktopReturn = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerDesktopReturnButton"));
            InteractableComponent desktopInteractable = Component<InteractableComponent>(desktopReturn);
            Assert.Equal(new int2(144, 28), desktopInteractable.Size);
            DemoDiscReturnToMenuComponent desktopReturnComponent = Component<DemoDiscReturnToMenuComponent>(desktopReturn);
            Assert.False(desktopReturnComponent.AllowKeyboardReturn);
            Assert.False(desktopReturnComponent.AllowGamepadReturn);
            Assert.True(desktopReturnComponent.AllowPointerReturn);
            Entity handheldReturn = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerHandheldReturnButton"));
            Assert.Single(handheldReturn.Components.OfType<InteractableComponent>());
            Assert.Single(handheldReturn.Components.OfType<NintendoDsReturnOverlayComponent>());

            string[] supportedPlatforms = new[] { "windows", "gamecube", "ps2", "psp", "psvita", "wii", "wiiu", "switch", "ds", "3ds" };
            EntitySaveComponent desktopSave = Component<EntitySaveComponent>(desktopRoot);
            EntitySaveComponent bottomCameraSave = Component<EntitySaveComponent>(bottomCamera);
            foreach (string platformId in supportedPlatforms) {
                bool isHandheld = platformId == "ds" || platformId == "3ds";
                Assert.Equal(!isHandheld, ResolveExistence(desktopSave, platformId));
                Assert.Equal(isHandheld, ResolveExistence(bottomCameraSave, platformId));
            }
        }

        [Fact]
        public void Requires_the_public_factory_inputs() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = CreateReferenceOnlyAuthoringSession(graph);
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session);
            FontAsset font = CreateHudFont();
            SceneAssetReference cube = EngineSceneAssetReferenceFactory.CreateCubeModel();

            Assert.Throws<ArgumentException>(() => factory.CreateSceneDefinition("", cube, font));
            Assert.Throws<ArgumentNullException>(() => factory.CreateSceneDefinition(CreateProjectRoot(), null, font));
            Assert.Throws<ArgumentNullException>(() => factory.CreateSceneDefinition(CreateProjectRoot(), cube, null));
        }

        static void AssertSurface(Entity[] entities, string name, float3 position, float3 scale, float yaw, float3 diffuse) {
            Entity entity = Assert.Single(entities.Where(candidate => EntityName(candidate) == name));
            AssertVector(entity.LocalPosition, position);
            AssertVector(entity.LocalScale, scale);
            float4 expectedOrientation;
            float4.CreateFromYawPitchRoll(yaw, 0f, 0f, out expectedOrientation);
            AssertVector(entity.LocalOrientation, expectedOrientation);
            SoftwareModelComponent model = Component<SoftwareModelComponent>(entity);
            AssertVector(model.Materials.Single().DiffuseColor, diffuse);
            AssertVector(model.Materials.Single().EmissionColor, float3.Zero);
            Assert.Equal(0f, model.Materials.Single().EmissionStrength);
        }

        static T Component<T>(Entity entity) where T : Component {
            return Assert.Single(entity.Components.OfType<T>());
        }

        static uint SaveId(Entity entity) {
            return Component<EntitySaveComponent>(entity).EntityId;
        }

        static bool ResolveExistence(EntitySaveComponent saveComponent, string platformId) {
            return saveComponent.TryGetExistencePlatformOverride(platformId, out SceneEntityPlatformExistenceOverrideAsset overrideState)
                ? overrideState.Exists
                : true;
        }

        static string EntityName(Entity entity) {
            return Assert.IsType<EditorEntity>(entity).Name;
        }

        static bool IsDescendant(Entity ancestor, Entity candidate) {
            if (ancestor == null || candidate == null || ancestor.Children == null) {
                return false;
            }

            foreach (Entity child in ancestor.Children) {
                if (ReferenceEquals(child, candidate) || IsDescendant(child, candidate)) {
                    return true;
                }
            }

            return false;
        }

        static void AssertVector(float3 actual, float3 expected) {
            Assert.Equal(expected.X, actual.X);
            Assert.Equal(expected.Y, actual.Y);
            Assert.Equal(expected.Z, actual.Z);
        }

        static void AssertVector(float4 actual, float4 expected) {
            Assert.Equal(expected.X, actual.X);
            Assert.Equal(expected.Y, actual.Y);
            Assert.Equal(expected.Z, actual.Z);
            Assert.Equal(expected.W, actual.W);
        }

        static IEnumerable<Entity> FlattenEntities(IEnumerable<Entity> roots) {
            if (roots == null) {
                yield break;
            }

            foreach (Entity entity in roots) {
                if (entity == null) {
                    continue;
                }

                yield return entity;
                foreach (Entity child in FlattenEntities(entity.Children)) {
                    yield return child;
                }
            }
        }

        static FontAsset CreateHudFont() {
            return new FontAsset(
                new FontInfo("SoftwarePathTracerTest", 16, 4f),
                null,
                new Dictionary<char, FontChar>(),
                16f,
                1,
                1);
        }

        static string CreateProjectRoot() {
            return Path.Combine(Path.GetTempPath(), "software-path-tracer-scene-tests", Guid.NewGuid().ToString("N"));
        }

        static IEditorProjectAuthoringSession CreateReferenceOnlyAuthoringSession(TestGeneratedAssetGraph graph) {
            return ReferenceOnlyAuthoringSession.Create(graph.CreateAuthoringSession(CreateProjectRoot()));
        }

        sealed class RecursiveScanModelSource : ISoftwareModelAssetSource {
            readonly SceneAssetReference reference;

            public int LoadCount { get; private set; }
            public ModelAsset LastAsset { get; private set; }

            public RecursiveScanModelSource(SceneAssetReference reference) {
                this.reference = reference ?? throw new ArgumentNullException(nameof(reference));
            }

            public ModelAsset LoadOwned(SceneAssetReference requestedReference) {
                Assert.Equal(reference.SourceKind, requestedReference.SourceKind);
                Assert.Equal(reference.AssetId, requestedReference.AssetId);
                Assert.Equal(reference.RelativePath, requestedReference.RelativePath);
                LoadCount++;
                LastAsset = CreateCubeAsset();
                return LastAsset;
            }
        }

        static ModelAsset CreateCubeAsset() {
            return new ModelAsset {
                Positions = new[] {
                    new float3(-1f, -1f, -1f), new float3(1f, -1f, -1f), new float3(1f, 1f, -1f), new float3(-1f, 1f, -1f),
                    new float3(-1f, -1f, 1f), new float3(1f, -1f, 1f), new float3(1f, 1f, 1f), new float3(-1f, 1f, 1f)
                },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7, 0, 1, 5, 0, 5, 4, 3, 7, 6, 3, 6, 2, 0, 4, 7, 0, 7, 3, 1, 2, 6, 1, 6, 5 },
                Submeshes = new[] { new ModelSubmeshAsset { MaterialSlotName = "DefaultMaterial", IndexStart = 0, IndexCount = 36 } }
            };
        }

        public class ReferenceOnlyAuthoringSession : DispatchProxy {
            IEditorProjectAuthoringSession inner;

            public static IEditorProjectAuthoringSession Create(IEditorProjectAuthoringSession inner) {
                if (inner == null) {
                    throw new ArgumentNullException(nameof(inner));
                }

                IEditorProjectAuthoringSession proxy = Create<IEditorProjectAuthoringSession, ReferenceOnlyAuthoringSession>();
                ((ReferenceOnlyAuthoringSession)(object)proxy).inner = inner;
                return proxy;
            }

            protected override object Invoke(MethodInfo targetMethod, object[] args) {
                if (string.Equals(targetMethod.Name, nameof(IEditorProjectAuthoringSession.CreateFileReference), StringComparison.Ordinal)) {
                    return global::helengine.SceneAssetReferenceFactory.CreateFileSystemFont("Fonts/DemoDiscBody.ttf");
                }

                return targetMethod.Invoke(inner, args);
            }
        }
    }
}
