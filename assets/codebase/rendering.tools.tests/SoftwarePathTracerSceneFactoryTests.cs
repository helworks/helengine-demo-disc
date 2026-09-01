using city.rendering;
using city.rendering.tools;
using city.scene.tools;
using helengine;
using helengine.editor;

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
            IEditorProjectAuthoringSession session = graph.CreateAuthoringSession(CreateProjectRoot());
            using EditorAuthoringTransaction transaction = session.BeginTransaction();
            SceneAssetReference cubeReference = EngineSceneAssetReferenceFactory.CreateCubeModel();
            FontAsset hudFont = CreateHudFont();
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session, transaction);

            GeneratedAuthoringSceneDefinition definition = factory.CreateSceneDefinition(
                CreateProjectRoot(),
                cubeReference,
                hudFont);

            Assert.Equal(SceneId, definition.SceneId);
            Assert.Equal(SceneIdentity, ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(SceneId));

            Entity[] entities = FlattenEntities(definition.RootEntities).ToArray();
            Entity[] tracedEntities = entities
                .Where(entity => entity.Components.OfType<SoftwareModelComponent>().Any())
                .ToArray();
            Assert.Equal(8, tracedEntities.Length);

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
        public void Creates_the_presentation_camera_hud_and_stably_wired_controller() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = graph.CreateAuthoringSession(CreateProjectRoot());
            using EditorAuthoringTransaction transaction = session.BeginTransaction();
            FontAsset hudFont = CreateHudFont();
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session, transaction);

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
            });

            Entity controllerEntity = Assert.Single(entities.Where(entity => entity.Components.OfType<SoftwarePathTracerComponent>().Any()));
            SoftwarePathTracerComponent controller = Component<SoftwarePathTracerComponent>(controllerEntity);
            AssertVector(controller.TraceCameraOrigin, new float3(0f, 0f, 3f));
            AssertVector(controller.TraceCameraForward, new float3(0f, 0f, -1f));
            AssertVector(controller.TraceCameraRight, new float3(1f, 0f, 0f));
            AssertVector(controller.TraceCameraUp, new float3(0f, 1f, 0f));
            Assert.Equal(55f, controller.VerticalFieldOfViewDegrees);
            Assert.Equal(1f, controller.Exposure);
            Assert.Equal(SaveId(outputEntity), controller.OutputSpriteEntityReference.EntityId);
            Assert.Equal(SaveId(hudEntities.Single(entity => EntityName(entity) == "SoftwarePathTracerSppText")), controller.SppTextEntityReference.EntityId);
            Assert.Equal(SaveId(hudEntities.Single(entity => EntityName(entity) == "SoftwarePathTracerElapsedText")), controller.ElapsedTextEntityReference.EntityId);
            Assert.Equal(SaveId(hudEntities.Single(entity => EntityName(entity) == "SoftwarePathTracerRaysPerSecondText")), controller.RaysPerSecondTextEntityReference.EntityId);

            Assert.DoesNotContain(entities, entity => entity.Components.OfType<DirectionalLightComponent>().Any());
            Assert.DoesNotContain(entities, entity => entity.Components.OfType<SpotLightComponent>().Any());
        }

        [Fact]
        public void Requires_the_public_factory_inputs() {
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(CreateProjectRoot());
            IEditorProjectAuthoringSession session = graph.CreateAuthoringSession(CreateProjectRoot());
            using EditorAuthoringTransaction transaction = session.BeginTransaction();
            SoftwarePathTracerSceneFactory factory = new SoftwarePathTracerSceneFactory(session, transaction);
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
    }
}
