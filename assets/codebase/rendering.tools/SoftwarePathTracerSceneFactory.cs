using city.rendering;
using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the fixed common Cornell-box authoring graph for the software path tracer.
    /// </summary>
    public sealed class SoftwarePathTracerSceneFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Stable generated scene path and identity key.
        /// </summary>
        public const string SceneId = "scenes/rendering/software_path_tracer.helen";

        /// <summary>
        /// Initializes the scene factory with the active editor authoring capabilities.
        /// </summary>
        /// <param name="assetAuthoringService">Authoring session used to create live entities.</param>
        public SoftwarePathTracerSceneFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        /// <summary>
        /// Creates the fixed Cornell-box scene and its ordinary 2D presentation graph.
        /// </summary>
        /// <param name="projectRootPath">Project root path required by the public authoring contract.</param>
        /// <param name="cubeReference">CPU-readable cube reference assigned to every traced model.</param>
        /// <param name="hudFont">Font assigned to the three diagnostic text entities.</param>
        /// <returns>The live-authored software path tracer scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, SceneAssetReference cubeReference, FontAsset hudFont) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            } else if (hudFont == null) {
                throw new ArgumentNullException(nameof(hudFont));
            }

            Entity cameraEntity = CreatePresentationCameraEntity();
            Entity outputEntity = CreateOutputSpriteEntity();
            Entity sppTextEntity = CreateHudTextEntity("SoftwarePathTracerSppText", "SPP: 0", new float3(16f, 16f, 0.1f), hudFont);
            Entity elapsedTextEntity = CreateHudTextEntity("SoftwarePathTracerElapsedText", "Time: 0.0s", new float3(16f, 44f, 0.1f), hudFont);
            Entity raysPerSecondTextEntity = CreateHudTextEntity("SoftwarePathTracerRaysPerSecondText", "Rays/s: 0", new float3(16f, 72f, 0.1f), hudFont);
            Entity controllerEntity = CreateControllerEntity(outputEntity, sppTextEntity, elapsedTextEntity, raysPerSecondTextEntity);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerFloor", new float3(0f, -1f, 0f), new float3(2f, 0.05f, 2f), 0f, new float3(0.75f, 0.75f, 0.75f), cubeReference);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerCeiling", new float3(0f, 1f, 0f), new float3(2f, 0.05f, 2f), 0f, new float3(0.75f, 0.75f, 0.75f), cubeReference);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerBack", new float3(0f, 0f, -1f), new float3(2f, 2f, 0.05f), 0f, new float3(0.75f, 0.75f, 0.75f), cubeReference);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerLeft", new float3(-1f, 0f, 0f), new float3(0.05f, 2f, 2f), 0f, new float3(0.75f, 0.05f, 0.05f), cubeReference);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerRight", new float3(1f, 0f, 0f), new float3(0.05f, 2f, 2f), 0f, new float3(0.05f, 0.75f, 0.05f), cubeReference);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerShortBox", new float3(-0.35f, -0.55f, 0.15f), new float3(0.6f, 0.9f, 0.6f), 0.30f, new float3(0.75f, 0.75f, 0.75f), cubeReference);
            CreateSurfaceEntity(controllerEntity, "SoftwarePathTracerTallBox", new float3(0.38f, -0.25f, 0.35f), new float3(0.55f, 1.45f, 0.55f), -0.28f, new float3(0.75f, 0.75f, 0.75f), cubeReference);
            CreateEmitterEntity(controllerEntity, cubeReference);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = new[] {
                    cameraEntity,
                    outputEntity,
                    sppTextEntity,
                    elapsedTextEntity,
                    raysPerSecondTextEntity,
                    controllerEntity
                }
            };
        }

        /// <summary>
        /// Creates the ordinary full-screen camera used to present the runtime output and HUD.
        /// </summary>
        Entity CreatePresentationCameraEntity() {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("SoftwarePathTracerCamera");
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(0f, 0f, 0f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the runtime output sprite target. The controller supplies its texture later.
        /// </summary>
        Entity CreateOutputSpriteEntity() {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("SoftwarePathTracerOutput");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 0f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new SpriteComponent {
                Size = new int2(320, 240),
                Color = new byte4(255, 255, 255, 255),
                RenderOrder2D = 0
            });
            return entity;
        }

        /// <summary>
        /// Creates one diagnostic text row in the common 2D presentation graph.
        /// </summary>
        Entity CreateHudTextEntity(string name, string text, float3 position, FontAsset font) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new TextComponent {
                Text = text,
                Font = font,
                FontScale = 1f,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(320, 24),
                RenderOrder2D = 1
            });
            TextComponent textComponent = entity.Components.OfType<TextComponent>().Single();
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(textComponent, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorFontReference(AssetAuthoringService));
            return entity;
        }

        /// <summary>
        /// Creates the tracer controller and references the presentation entities by saved ids.
        /// </summary>
        Entity CreateControllerEntity(Entity outputEntity, Entity sppTextEntity, Entity elapsedTextEntity, Entity raysPerSecondTextEntity) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("SoftwarePathTracerController");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new SoftwarePathTracerComponent {
                OutputSpriteEntityReference = CreateEntityReference(outputEntity),
                SppTextEntityReference = CreateEntityReference(sppTextEntity),
                ElapsedTextEntityReference = CreateEntityReference(elapsedTextEntity),
                RaysPerSecondTextEntityReference = CreateEntityReference(raysPerSecondTextEntity),
                TraceCameraOrigin = new float3(0f, 0f, 3f),
                TraceCameraForward = new float3(0f, 0f, -1f),
                TraceCameraRight = new float3(1f, 0f, 0f),
                TraceCameraUp = new float3(0f, 1f, 0f),
                VerticalFieldOfViewDegrees = 55f,
                Exposure = 1f
            });
            return entity;
        }

        /// <summary>
        /// Creates one traced cube instance with exactly one software material description.
        /// </summary>
        Entity CreateSurfaceEntity(Entity parent, string name, float3 position, float3 scale, float yaw, float3 diffuseColor, SceneAssetReference cubeReference, float3 emissionColor = default, float emissionStrength = 0f) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            float4 orientation;
            float4.CreateFromYawPitchRoll(yaw, 0f, 0f, out orientation);
            entity.LocalOrientation = orientation;
            entity.AddComponent(new SoftwareModelComponent {
                ModelReference = cubeReference,
                Materials = new[] {
                    new SoftwareMaterial {
                        DiffuseColor = diffuseColor,
                        EmissionColor = emissionColor,
                        EmissionStrength = emissionStrength
                    }
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the single ceiling area-light cube with its fixed emissive material.
        /// </summary>
        Entity CreateEmitterEntity(Entity parent, SceneAssetReference cubeReference) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, "SoftwarePathTracerEmitter");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 0.93f, 0f);
            entity.LocalScale = new float3(0.55f, 0.025f, 0.45f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new SoftwareModelComponent {
                ModelReference = cubeReference,
                Materials = new[] {
                    new SoftwareMaterial {
                        DiffuseColor = float3.Zero,
                        EmissionColor = float3.One,
                        EmissionStrength = 14f
                    }
                }
            });
            return entity;
        }

        /// <summary>
        /// Returns the saved-id reference for one authored entity, allocating an id when needed.
        /// </summary>
        SceneEntityReference CreateEntityReference(Entity entity) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            if (saveComponent.EntityId == 0u) {
                if (AssetAuthoringService.OwningCore is not EditorCore editorCore || editorCore.SceneEntityIdAllocator == null) {
                    throw new InvalidOperationException("Software path tracer references require an active editor scene-entity id allocator.");
                }

                saveComponent.EntityId = editorCore.SceneEntityIdAllocator.Allocate();
            }

            return new SceneEntityReference { EntityId = saveComponent.EntityId };
        }

        /// <summary>
        /// Finds the persistence component attached by the editor entity factory.
        /// </summary>
        static EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException("Generated software path tracer entities require initialized save state.");
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated software path tracer entities require one EntitySaveComponent.");
        }
    }
}
