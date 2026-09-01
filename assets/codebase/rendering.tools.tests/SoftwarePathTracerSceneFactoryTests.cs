using city.rendering;
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

            Entity cameraEntity = Assert.Single(entities.Where(entity => entity.Components.OfType<CameraComponent>().Any()));
            CameraComponent camera = Component<CameraComponent>(cameraEntity);
            Assert.Equal(EditorLayerMasks.SceneObjects, camera.LayerMask);
            Assert.Equal(new float4(0f, 0f, 1f, 1f), camera.Viewport);
            Assert.Equal(new float3(0f, 0f, 0f), cameraEntity.LocalPosition);

            Entity outputEntity = Assert.Single(entities.Where(entity => EntityName(entity) == "SoftwarePathTracerOutput"));
            Assert.Single(outputEntity.Components.OfType<SpriteComponent>());
            Assert.Equal(EditorLayerMasks.SceneObjects, outputEntity.LayerMask);

            Entity[] hudEntities = entities.Where(entity => entity.Components.OfType<TextComponent>().Any()).ToArray();
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
