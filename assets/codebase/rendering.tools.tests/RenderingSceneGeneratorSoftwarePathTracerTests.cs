using city.rendering;
using city.rendering.tools;
using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Verifies registration and serialized output for the shared software path tracer scene.
    /// </summary>
    public sealed class RenderingSceneGeneratorSoftwarePathTracerTests {
        const string SceneId = "scenes/rendering/software_path_tracer.helen";
        static readonly string ProjectRootPath = ResolveProjectRoot();

        [Fact]
        public void Generator_registers_one_shared_software_path_tracer_scene_without_companion_ids() {
            string source = ReadGeneratorSource();

            Assert.Contains("public const string SoftwarePathTracerSceneId = \"scenes/rendering/software_path_tracer.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("readonly SoftwarePathTracerSceneFactory SoftwarePathTracerFactory;", source, StringComparison.Ordinal);
            Assert.Contains("SoftwarePathTracerFactory = new SoftwarePathTracerSceneFactory(AssetAuthoringService);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("software_path_tracer_ds", source, StringComparison.Ordinal);
            Assert.DoesNotContain("software_path_tracer_3ds", source, StringComparison.Ordinal);

            int factoryIndex = source.IndexOf("SoftwarePathTracerFactory.CreateSceneDefinition(", StringComparison.Ordinal);
            int factoryEndIndex = source.IndexOf(";", factoryIndex);
            Assert.True(factoryIndex >= 0 && factoryEndIndex > factoryIndex);
            string factoryCall = source.Substring(factoryIndex, factoryEndIndex - factoryIndex);
            Assert.Contains("EngineSceneAssetReferenceFactory.CreateCubeModel()", factoryCall, StringComparison.Ordinal);
            Assert.Contains("editorCore.DefaultFontAssetForEditor", factoryCall, StringComparison.Ordinal);
            Assert.DoesNotContain("assets.GeneratedCubeModel", factoryCall, StringComparison.Ordinal);
        }

        [Fact]
        public void Generator_writes_the_shared_software_scene_once_after_existing_rendering_showcases() {
            string source = ReadGeneratorSource();
            const string definition = "GeneratedAuthoringSceneDefinition softwarePathTracerSceneDefinition = SoftwarePathTracerFactory.CreateSceneDefinition(";
            const string write = "AuthoringSceneWriteService.WriteScene(softwarePathTracerSceneDefinition);";

            int definitionIndex = source.IndexOf(definition, StringComparison.Ordinal);
            int matrixWriteIndex = source.IndexOf("AuthoringSceneWriteService.WriteScene(matrixRenderSceneDefinition);", StringComparison.Ordinal);
            int writeIndex = source.IndexOf(write, StringComparison.Ordinal);
            Assert.True(definitionIndex >= 0);
            Assert.True(matrixWriteIndex >= 0);
            Assert.True(writeIndex > matrixWriteIndex);
            Assert.True(writeIndex > definitionIndex);
            Assert.Equal(writeIndex, source.LastIndexOf(write, StringComparison.Ordinal));
        }

        [Fact]
        public void Generated_shared_software_scene_contains_the_expected_serialized_graph_and_references() {
            SceneAsset scene = LoadSceneAsset();
            Assert.Equal(SceneId, scene.Id);

            SceneEntityAsset[] entities = EnumerateEntities(scene.RootEntities).ToArray();
            string tracerTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(SoftwarePathTracerComponent));
            string modelTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(SoftwareModelComponent));
            string spriteTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(SpriteComponent));
            string meshTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(MeshComponent));
            string textTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TextComponent));
            string viewportTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(ViewportComponent));

            Assert.Equal(1, EnumerateComponents(entities).Count(component => component.ComponentTypeId == tracerTypeId));
            Assert.Equal(8, EnumerateComponents(entities).Count(component => component.ComponentTypeId == modelTypeId));
            Assert.Equal(1, EnumerateComponents(entities).Count(component => component.ComponentTypeId == spriteTypeId));
            Assert.DoesNotContain(EnumerateComponents(entities), component => component.ComponentTypeId == meshTypeId);
            Assert.Contains(entities, entity => entity.Name == "SoftwarePathTracerDesktopHudRoot");
            Assert.Contains(entities, entity => entity.Name == "SoftwarePathTracerHandheldHudRoot");
            Assert.DoesNotContain(scene.RootEntities, entity => entity.Name.Contains("software_path_tracer_ds", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(scene.RootEntities, entity => entity.Name.Contains("software_path_tracer_3ds", StringComparison.OrdinalIgnoreCase));

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            ComponentPlatformOverridePayloadService overridePayloadService = new ComponentPlatformOverridePayloadService();
            SceneComponentAssetRecord tracerRecord = Assert.Single(
                EnumerateComponents(entities),
                component => component.ComponentTypeId == tracerTypeId);
            SceneComponentAssetRecord tracerBaseRecord = overridePayloadService.UnwrapBaseRecord(tracerRecord);
            SoftwarePathTracerComponent tracer = Assert.IsType<SoftwarePathTracerComponent>(
                registry.GetDescriptor(tracerBaseRecord.ComponentTypeId).DeserializeComponent(
                    tracerBaseRecord,
                    new EntitySaveComponent(),
                    null));
            uint[] commonReferences = {
                tracer.OutputSpriteEntityReference.EntityId,
                tracer.SppTextEntityReference.EntityId,
                tracer.ElapsedTextEntityReference.EntityId,
                tracer.RaysPerSecondTextEntityReference.EntityId
            };
            Assert.All(commonReferences, reference => Assert.NotEqual(0u, reference));
            Assert.Equal(commonReferences.Length, commonReferences.Distinct().Count());

            SceneEntityAsset[] commonHudTextEntities = {
                Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerSppText"),
                Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerElapsedText"),
                Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerRaysPerSecondText")
            };
            SceneEntityAsset[] handheldHudTextEntities = {
                Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerHandheldSppText"),
                Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerHandheldElapsedText"),
                Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerHandheldRaysPerSecondText")
            };
            uint[] commonHudEntityIds = commonHudTextEntities.Select(entity => {
                Assert.Single(entity.Components ?? Array.Empty<SceneComponentAssetRecord>(), component => component.ComponentTypeId == textTypeId);
                return entity.Id;
            }).ToArray();
            uint[] handheldHudEntityIds = handheldHudTextEntities.Select(entity => {
                Assert.Single(entity.Components ?? Array.Empty<SceneComponentAssetRecord>(), component => component.ComponentTypeId == textTypeId);
                return entity.Id;
            }).ToArray();
            Assert.All(commonHudEntityIds.Concat(handheldHudEntityIds), id => Assert.NotEqual(0u, id));
            Assert.Equal(commonHudEntityIds.Length, commonHudEntityIds.Distinct().Count());
            Assert.Equal(handheldHudEntityIds.Length, handheldHudEntityIds.Distinct().Count());
            Assert.Empty(commonHudEntityIds.Intersect(handheldHudEntityIds));
            Assert.Equal(commonHudEntityIds[0], tracer.SppTextEntityReference.EntityId);
            Assert.Equal(commonHudEntityIds[1], tracer.ElapsedTextEntityReference.EntityId);
            Assert.Equal(commonHudEntityIds[2], tracer.RaysPerSecondTextEntityReference.EntityId);

            SceneEntityAsset presentationViewportEntity = Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerPresentationViewport");
            SceneComponentAssetRecord presentationViewportRecord = Assert.Single(
                presentationViewportEntity.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == viewportTypeId);
            ViewportComponent presentationViewport = DeserializeComponent<ViewportComponent>(
                overridePayloadService.UnwrapBaseRecord(presentationViewportRecord),
                registry);
            Assert.Equal(new int2(320, 240), presentationViewport.FixedSize);
            Assert.Equal(320, presentationViewport.ReferenceWidth);
            Assert.Equal(240, presentationViewport.ReferenceHeight);
            ViewportComponent dsPresentationViewport = DeserializeOverrideComponent<ViewportComponent>(
                presentationViewportRecord,
                Assert.Single(overridePayloadService.ReadOverrideStates(presentationViewportRecord), state => state.PlatformId == "ds"),
                registry);
            Assert.Equal(new int2(256, 192), dsPresentationViewport.FixedSize);
            Assert.Equal(256, dsPresentationViewport.ReferenceWidth);
            Assert.Equal(192, dsPresentationViewport.ReferenceHeight);
            ViewportComponent threeDsPresentationViewport = DeserializeOverrideComponent<ViewportComponent>(
                presentationViewportRecord,
                Assert.Single(overridePayloadService.ReadOverrideStates(presentationViewportRecord), state => state.PlatformId == "3ds"),
                registry);
            Assert.Equal(new int2(400, 240), threeDsPresentationViewport.FixedSize);
            Assert.Equal(400, threeDsPresentationViewport.ReferenceWidth);
            Assert.Equal(240, threeDsPresentationViewport.ReferenceHeight);

            SceneEntityAsset bottomViewportEntity = Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerBottomScreenViewport");
            SceneComponentAssetRecord bottomViewportRecord = Assert.Single(
                bottomViewportEntity.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == viewportTypeId);
            ViewportComponent bottomViewport = DeserializeComponent<ViewportComponent>(
                overridePayloadService.UnwrapBaseRecord(bottomViewportRecord),
                registry);
            Assert.Equal(new int2(256, 192), bottomViewport.FixedSize);
            Assert.Equal(256, bottomViewport.ReferenceWidth);
            Assert.Equal(192, bottomViewport.ReferenceHeight);
            ViewportComponent threeDsBottomViewport = DeserializeOverrideComponent<ViewportComponent>(
                bottomViewportRecord,
                Assert.Single(overridePayloadService.ReadOverrideStates(bottomViewportRecord), state => state.PlatformId == "3ds"),
                registry);
            Assert.Equal(new int2(320, 240), threeDsBottomViewport.FixedSize);
            Assert.Equal(256, threeDsBottomViewport.ReferenceWidth);
            Assert.Equal(192, threeDsBottomViewport.ReferenceHeight);

            SceneAssetReference engineCube = EngineSceneAssetReferenceFactory.CreateCubeModel();
            foreach (SceneComponentAssetRecord modelRecord in EnumerateComponents(entities).Where(component => component.ComponentTypeId == modelTypeId)) {
                SceneComponentAssetRecord modelBaseRecord = overridePayloadService.UnwrapBaseRecord(modelRecord);
                SoftwareModelComponent model = Assert.IsType<SoftwareModelComponent>(
                    registry.GetDescriptor(modelBaseRecord.ComponentTypeId).DeserializeComponent(
                        modelBaseRecord,
                        new EntitySaveComponent(),
                        null));
                Assert.NotNull(model.ModelReference);
                Assert.Equal(engineCube.SourceKind, model.ModelReference.SourceKind);
                Assert.Equal(engineCube.ProviderId, model.ModelReference.ProviderId);
                Assert.Equal(engineCube.AssetId, model.ModelReference.AssetId);
                Assert.Equal(engineCube.RelativePath, model.ModelReference.RelativePath);
                Assert.Equal(engineCube.ContentHash, model.ModelReference.ContentHash);
                Assert.NotNull(model.Materials);
            }

            EntityComponentPlatformOverrideState[] handheldOverrides = overridePayloadService
                .ReadOverrideStates(tracerRecord)
                .Where(state => state.PlatformId == "ds" || state.PlatformId == "3ds")
                .ToArray();
            Assert.Equal(new[] { "3ds", "ds" }, handheldOverrides.Select(state => state.PlatformId).OrderBy(platformId => platformId, StringComparer.Ordinal).ToArray());
            string[] expectedOverrideProperties = {
                nameof(SoftwarePathTracerComponent.ElapsedTextEntityReference),
                nameof(SoftwarePathTracerComponent.RaysPerSecondTextEntityReference),
                nameof(SoftwarePathTracerComponent.SppTextEntityReference)
            };
            uint[] commonHudReferences = {
                tracer.SppTextEntityReference.EntityId,
                tracer.ElapsedTextEntityReference.EntityId,
                tracer.RaysPerSecondTextEntityReference.EntityId
            };
            foreach (EntityComponentPlatformOverrideState overrideState in handheldOverrides) {
                Assert.NotEmpty(overrideState.Payload);
                Assert.Equal(expectedOverrideProperties, overrideState.EnumeratePropertyOverrides().OrderBy(property => property, StringComparer.Ordinal).ToArray());
                SoftwarePathTracerComponent handheldTracer = DeserializeOverrideComponent<SoftwarePathTracerComponent>(tracerRecord, overrideState, registry);
                uint[] handheldHudReferences = {
                    handheldTracer.SppTextEntityReference.EntityId,
                    handheldTracer.ElapsedTextEntityReference.EntityId,
                    handheldTracer.RaysPerSecondTextEntityReference.EntityId
                };
                Assert.All(handheldHudReferences, reference => Assert.NotEqual(0u, reference));
                Assert.Equal(handheldHudReferences.Length, handheldHudReferences.Distinct().Count());
                Assert.Empty(handheldHudReferences.Intersect(commonHudReferences));
                Assert.Equal(handheldHudEntityIds[0], handheldTracer.SppTextEntityReference.EntityId);
                Assert.Equal(handheldHudEntityIds[1], handheldTracer.ElapsedTextEntityReference.EntityId);
                Assert.Equal(handheldHudEntityIds[2], handheldTracer.RaysPerSecondTextEntityReference.EntityId);
            }

            SceneEntityAsset outputEntity = Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerOutput");
            SceneComponentAssetRecord outputSpriteRecord = Assert.Single(outputEntity.Components, component => component.ComponentTypeId == spriteTypeId);
            EntityComponentPlatformOverrideState dsOutputOverride = Assert.Single(
                overridePayloadService.ReadOverrideStates(outputSpriteRecord),
                state => state.PlatformId == "ds");
            SpriteComponent dsOutputSprite = DeserializeOverrideComponent<SpriteComponent>(outputSpriteRecord, dsOutputOverride, registry);
            Assert.Equal(new int2(256, 192), dsOutputSprite.Size);
            Assert.Equal(new int2(320, 240), DeserializeComponent<SpriteComponent>(overridePayloadService.UnwrapBaseRecord(outputSpriteRecord), registry).Size);
            Assert.Contains(outputEntity.PlatformTransformOverrides ?? Array.Empty<SceneEntityPlatformTransformOverrideAsset>(),
                transform => transform.PlatformId == "3ds" && transform.HasLocalPositionOverride && transform.LocalPosition.X == 40f);

            SceneEntityAsset desktopHudRoot = Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerDesktopHudRoot");
            SceneEntityAsset bottomCamera = Assert.Single(entities, entity => entity.Name == "SoftwarePathTracerBottomScreenCamera");
            Assert.Contains(desktopHudRoot.PlatformExistenceOverrides ?? Array.Empty<SceneEntityPlatformExistenceOverrideAsset>(),
                overrideAsset => overrideAsset.PlatformId == "ds" && !overrideAsset.Exists);
            Assert.Contains(desktopHudRoot.PlatformExistenceOverrides ?? Array.Empty<SceneEntityPlatformExistenceOverrideAsset>(),
                overrideAsset => overrideAsset.PlatformId == "3ds" && !overrideAsset.Exists);
            string[] nonHandheldPlatforms = { "windows", "gamecube", "ps2", "psp", "psvita", "wii", "wiiu", "switch" };
            foreach (string platformId in nonHandheldPlatforms) {
                Assert.Contains(bottomCamera.PlatformExistenceOverrides ?? Array.Empty<SceneEntityPlatformExistenceOverrideAsset>(),
                    overrideAsset => overrideAsset.PlatformId == platformId && !overrideAsset.Exists);
            }
        }

        static string ReadGeneratorSource() {
            return File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "RenderingSceneGenerator.cs"));
        }

        static SceneAsset LoadSceneAsset() {
            string fullPath = Path.Combine(ProjectRootPath, "assets", "scenes", "rendering", "software_path_tracer.helen");
            Assert.True(File.Exists(fullPath), $"Expected generated scene asset '{fullPath}' to exist.");
            using FileStream stream = File.OpenRead(fullPath);
            return Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
        }

        static IEnumerable<SceneComponentAssetRecord> EnumerateComponents(IEnumerable<SceneEntityAsset> entities) {
            foreach (SceneEntityAsset entity in entities ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneComponentAssetRecord component in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                    yield return component;
                }
            }
        }

        static T DeserializeComponent<T>(SceneComponentAssetRecord record, ComponentPersistenceRegistry registry) where T : Component {
            return Assert.IsType<T>(registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(record, new EntitySaveComponent(), null));
        }

        static T DeserializeOverrideComponent<T>(
            SceneComponentAssetRecord commonRecord,
            EntityComponentPlatformOverrideState overrideState,
            ComponentPersistenceRegistry registry) where T : Component {
            return DeserializeComponent<T>(new SceneComponentAssetRecord {
                ComponentTypeId = commonRecord.ComponentTypeId,
                ComponentIndex = commonRecord.ComponentIndex,
                ComponentKey = commonRecord.ComponentKey,
                Payload = overrideState.Payload
            }, registry);
        }

        static IEnumerable<SceneEntityAsset> EnumerateEntities(IEnumerable<SceneEntityAsset> entities) {
            foreach (SceneEntityAsset entity in entities ?? Array.Empty<SceneEntityAsset>()) {
                yield return entity;
                foreach (SceneEntityAsset child in EnumerateEntities(entity.Children)) {
                    yield return child;
                }
            }
        }

        static string ResolveProjectRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") {
            DirectoryInfo currentDirectory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (currentDirectory != null) {
                if (Directory.Exists(Path.Combine(currentDirectory.FullName, "assets"))
                    && File.Exists(Path.Combine(currentDirectory.FullName, "project.heproj"))) {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }

            throw new InvalidOperationException("Unable to locate the active checkout root from the test source path.");
        }
    }
}
