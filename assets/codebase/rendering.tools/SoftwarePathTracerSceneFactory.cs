using city.rendering;
using city.menu;
using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the fixed common Cornell-box authoring graph for the software path tracer.
    /// </summary>
    public sealed class SoftwarePathTracerSceneFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

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

            Entity cameraEntity = CreatePresentationCameraEntity(out Entity presentationViewportEntity);
            Entity outputEntity = CreateOutputSpriteEntity(presentationViewportEntity);
            (Entity sppTextEntity, Entity elapsedTextEntity, Entity raysPerSecondTextEntity) = CreateDesktopHud(
                presentationViewportEntity,
                hudFont);
            Entity bottomCameraEntity = CreateBottomScreenCameraEntity(out Entity bottomViewportEntity);
            Entity handheldHudRoot = CreateHandheldHudRoot(bottomViewportEntity);
            Entity handheldSppTextEntity = CreateHudTextEntity(handheldHudRoot, "SoftwarePathTracerHandheldSppText", "SPP: 0", new float3(16f, 16f, 0.1f), hudFont);
            Entity handheldElapsedTextEntity = CreateHudTextEntity(handheldHudRoot, "SoftwarePathTracerHandheldElapsedText", "Time: 0.0s", new float3(16f, 44f, 0.1f), hudFont);
            Entity handheldRaysPerSecondTextEntity = CreateHudTextEntity(handheldHudRoot, "SoftwarePathTracerHandheldRaysPerSecondText", "Rays/s: 0", new float3(16f, 72f, 0.1f), hudFont);
            CreateHandheldReturnButton(handheldHudRoot, hudFont);
            Entity controllerEntity = CreateControllerEntity(
                outputEntity,
                sppTextEntity,
                elapsedTextEntity,
                raysPerSecondTextEntity,
                handheldSppTextEntity,
                handheldElapsedTextEntity,
                handheldRaysPerSecondTextEntity);
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
                    bottomCameraEntity,
                    controllerEntity
                }
            };
        }

        /// <summary>
        /// Creates the ordinary full-screen camera used to present the runtime output and HUD.
        /// </summary>
        Entity CreatePresentationCameraEntity(out Entity presentationViewportEntity) {
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
            presentationViewportEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(entity, "SoftwarePathTracerPresentationViewport");
            presentationViewportEntity.LayerMask = EditorLayerMasks.SceneObjects;
            presentationViewportEntity.LocalPosition = float3.Zero;
            presentationViewportEntity.LocalScale = float3.One;
            presentationViewportEntity.LocalOrientation = float4.Identity;
            ViewportComponent presentationViewport = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(320, 240),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = 320,
                ReferenceHeight = 240
            };
            presentationViewportEntity.AddComponent(presentationViewport);
            ApplyPresentationViewportOverrides(presentationViewportEntity, presentationViewport);
            return entity;
        }

        /// <summary>
        /// Creates the DS bottom-screen camera and its shared handheld reference-canvas viewport.
        /// </summary>
        Entity CreateBottomScreenCameraEntity(out Entity bottomViewportEntity) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("SoftwarePathTracerBottomScreenCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            EntitySaveComponent cameraSave = FindRequiredEntitySaveComponent(entity);
            foreach (string platformId in new[] { "windows", "gamecube", "ps2", "psp", "psvita", "wii", "wiiu", "switch" }) {
                cameraSave.GetOrCreateExistencePlatformOverride(platformId).Exists = false;
            }
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 1,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 1f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(30f / 255f, 17f / 255f, 41f / 255f, 1f),
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
            bottomViewportEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(entity, "SoftwarePathTracerBottomScreenViewport");
            bottomViewportEntity.LayerMask = EditorLayerMasks.SceneObjects;
            bottomViewportEntity.LocalPosition = float3.Zero;
            bottomViewportEntity.LocalScale = float3.One;
            bottomViewportEntity.LocalOrientation = float4.Identity;
            ViewportComponent bottomViewport = new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(256, 192),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = 256,
                ReferenceHeight = 192
            };
            bottomViewportEntity.AddComponent(bottomViewport);
            ApplyBottomViewportOverrides(bottomViewportEntity, bottomViewport);
            return entity;
        }

        /// <summary>
        /// Creates the desktop statistics panel and pointer-only Return action beneath the top viewport.
        /// </summary>
        (Entity SppText, Entity ElapsedText, Entity RaysPerSecondText) CreateDesktopHud(Entity parent, FontAsset hudFont) {
            Entity root = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, "SoftwarePathTracerDesktopHudRoot");
            root.LayerMask = EditorLayerMasks.SceneObjects;
            root.LocalPosition = float3.Zero;
            root.LocalScale = float3.One;
            root.LocalOrientation = float4.Identity;
            EntitySaveComponent rootSave = FindRequiredEntitySaveComponent(root);
            rootSave.GetOrCreateExistencePlatformOverride("ds").Exists = false;
            rootSave.GetOrCreateExistencePlatformOverride("3ds").Exists = false;

            Entity panel = CreateRoundedPanelEntity(
                root,
                "SoftwarePathTracerDesktopHudPanel",
                new float3(8f, 8f, 0f),
                new int2(304, 136),
                6f,
                2f,
                new byte4(18, 27, 43, 220),
                new byte4(96, 128, 168, 255),
                1);
            Entity sppText = CreateHudTextEntity(panel, "SoftwarePathTracerSppText", "SPP: 0", new float3(8f, 8f, 0.1f), hudFont);
            Entity elapsedText = CreateHudTextEntity(panel, "SoftwarePathTracerElapsedText", "Time: 0.0s", new float3(8f, 36f, 0.1f), hudFont);
            Entity raysPerSecondText = CreateHudTextEntity(panel, "SoftwarePathTracerRaysPerSecondText", "Rays/s: 0", new float3(8f, 64f, 0.1f), hudFont);
            Entity returnButton = CreateRoundedPanelEntity(
                panel,
                "SoftwarePathTracerDesktopReturnButton",
                new float3(8f, 98f, 0.1f),
                new int2(144, 28),
                5f,
                1f,
                new byte4(40, 58, 87, 255),
                new byte4(122, 147, 182, 255),
                2);
            returnButton.AddComponent(new InteractableComponent { Size = new int2(144, 28) });
            returnButton.AddComponent(new DemoDiscReturnToMenuComponent {
                AllowKeyboardReturn = false,
                AllowGamepadReturn = false,
                AllowPointerReturn = true
            });
            CreateHudTextEntity(returnButton, "SoftwarePathTracerDesktopReturnLabel", "RETURN", new float3(8f, 2f, 0.1f), hudFont, 3);
            return (sppText, elapsedText, raysPerSecondText);
        }

        /// <summary>
        /// Creates one local rounded rectangle used by the software path tracer's authored HUD trees.
        /// </summary>
        Entity CreateRoundedPanelEntity(Entity parent, string name, float3 position, int2 size, float radius, float borderThickness, byte4 fillColor, byte4 borderColor, byte renderOrder) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new RoundedRectComponent {
                Size = size,
                Radius = radius,
                BorderThickness = borderThickness,
                FillColor = fillColor,
                BorderColor = borderColor,
                RenderOrder2D = renderOrder
            });
            return entity;
        }

        /// <summary>
        /// Creates the runtime output sprite target. The controller supplies its texture later.
        /// </summary>
        Entity CreateOutputSpriteEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, "SoftwarePathTracerOutput");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 0f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            SpriteComponent outputSprite = new SpriteComponent {
                Size = new int2(320, 240),
                Color = new byte4(255, 255, 255, 255),
                RenderOrder2D = 0
            };
            entity.AddComponent(outputSprite);
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            SpriteComponent dsSprite = (SpriteComponent)PlatformEditingService.EnsurePlatformOverrideComponent(outputSprite, saveComponent, "ds");
            dsSprite.Size = new int2(256, 192);
            PlatformEditingService.MarkPropertyOverride(outputSprite, saveComponent, "ds", nameof(SpriteComponent.Size));
            PlatformEditingService.PersistPlatformOverride(outputSprite, dsSprite, saveComponent, "ds");
            SceneEntityPlatformTransformOverrideAsset threeDsTransform = saveComponent.GetOrCreateTransformPlatformOverride("3ds");
            threeDsTransform.HasLocalPositionOverride = true;
            threeDsTransform.LocalPosition = new float3(40f, 0f, 0f);
            return entity;
        }

        /// <summary>
        /// Creates one diagnostic text row under either a desktop root or a handheld presentation subtree.
        /// </summary>
        Entity CreateHudTextEntity(Entity parent, string name, string text, float3 position, FontAsset font, byte renderOrder2D = 1) {
            Entity entity = parent == null
                ? AssetAuthoringService.OwningCore.EntityFactory.Create(name)
                : AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, name);
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
                RenderOrder2D = renderOrder2D
            });
            TextComponent textComponent = entity.Components.OfType<TextComponent>().Single();
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(textComponent, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorFontReference(AssetAuthoringService));
            return entity;
        }

        /// <summary>
        /// Creates the tracer controller and references the presentation entities by saved ids.
        /// </summary>
        Entity CreateControllerEntity(
            Entity outputEntity,
            Entity sppTextEntity,
            Entity elapsedTextEntity,
            Entity raysPerSecondTextEntity,
            Entity handheldSppTextEntity,
            Entity handheldElapsedTextEntity,
            Entity handheldRaysPerSecondTextEntity) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("SoftwarePathTracerController");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            SoftwarePathTracerComponent controller = new SoftwarePathTracerComponent {
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
            };
            entity.AddComponent(controller);
            ApplyControllerHudReferenceOverrides(
                entity,
                controller,
                CreateEntityReference(handheldSppTextEntity),
                CreateEntityReference(handheldElapsedTextEntity),
                CreateEntityReference(handheldRaysPerSecondTextEntity));
            return entity;
        }

        /// <summary>
        /// Creates the handheld diagnostic rows that are selected by the DS and 3DS controller overrides.
        /// </summary>
        Entity CreateHandheldHudRoot(Entity parent) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, "SoftwarePathTracerHandheldHudRoot");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            return entity;
        }

        /// <summary>
        /// Creates the bottom-screen handheld Return action and persists its label font.
        /// </summary>
        Entity CreateHandheldReturnButton(Entity parent, FontAsset hudFont) {
            Entity button = CreateRoundedPanelEntity(
                parent,
                "SoftwarePathTracerHandheldReturnButton",
                new float3(16f, 154f, 0.1f),
                new int2(224, 32),
                5f,
                2f,
                new byte4(40, 58, 87, 255),
                new byte4(122, 147, 182, 255),
                2);
            button.AddComponent(new InteractableComponent { Size = new int2(224, 32) });
            button.AddComponent(new NintendoDsReturnOverlayComponent());
            CreateHudTextEntity(button, "SoftwarePathTracerHandheldReturnLabel", "RETURN", new float3(80f, 6f, 0.1f), hudFont, 3);
            return button;
        }

        /// <summary>
        /// Persists the common top presentation viewport dimensions for DS and 3DS without adding cameras.
        /// </summary>
        void ApplyPresentationViewportOverrides(Entity viewportEntity, ViewportComponent commonViewport) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(viewportEntity);
            ViewportComponent dsViewport = (ViewportComponent)PlatformEditingService.EnsurePlatformOverrideComponent(commonViewport, saveComponent, "ds");
            dsViewport.FixedSize = new int2(256, 192);
            dsViewport.ReferenceWidth = 256;
            dsViewport.ReferenceHeight = 192;
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "ds", nameof(ViewportComponent.FixedSize));
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "ds", nameof(ViewportComponent.ReferenceWidth));
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "ds", nameof(ViewportComponent.ReferenceHeight));
            PlatformEditingService.PersistPlatformOverride(commonViewport, dsViewport, saveComponent, "ds");

            ViewportComponent threeDsViewport = (ViewportComponent)PlatformEditingService.EnsurePlatformOverrideComponent(commonViewport, saveComponent, "3ds");
            threeDsViewport.FixedSize = new int2(400, 240);
            threeDsViewport.ReferenceWidth = 400;
            threeDsViewport.ReferenceHeight = 240;
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "3ds", nameof(ViewportComponent.FixedSize));
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "3ds", nameof(ViewportComponent.ReferenceWidth));
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "3ds", nameof(ViewportComponent.ReferenceHeight));
            PlatformEditingService.PersistPlatformOverride(commonViewport, threeDsViewport, saveComponent, "3ds");
        }

        /// <summary>
        /// Persists the native 3DS bottom-screen dimensions while retaining the DS reference canvas.
        /// </summary>
        void ApplyBottomViewportOverrides(Entity viewportEntity, ViewportComponent commonViewport) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(viewportEntity);
            ViewportComponent threeDsViewport = (ViewportComponent)PlatformEditingService.EnsurePlatformOverrideComponent(commonViewport, saveComponent, "3ds");
            threeDsViewport.FixedSize = new int2(320, 240);
            threeDsViewport.ReferenceWidth = 256;
            threeDsViewport.ReferenceHeight = 192;
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "3ds", nameof(ViewportComponent.FixedSize));
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "3ds", nameof(ViewportComponent.ReferenceWidth));
            PlatformEditingService.MarkPropertyOverride(commonViewport, saveComponent, "3ds", nameof(ViewportComponent.ReferenceHeight));
            PlatformEditingService.PersistPlatformOverride(commonViewport, threeDsViewport, saveComponent, "3ds");
        }

        /// <summary>
        /// Persists handheld text references while inheriting the shared output and tracer settings.
        /// </summary>
        void ApplyControllerHudReferenceOverrides(
            Entity controllerEntity,
            SoftwarePathTracerComponent commonController,
            SceneEntityReference handheldSppReference,
            SceneEntityReference handheldElapsedReference,
            SceneEntityReference handheldRaysPerSecondReference) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(controllerEntity);
            foreach (string platformId in new[] { "ds", "3ds" }) {
                SoftwarePathTracerComponent overrideController = (SoftwarePathTracerComponent)PlatformEditingService.EnsurePlatformOverrideComponent(commonController, saveComponent, platformId);
                overrideController.SppTextEntityReference = handheldSppReference;
                overrideController.ElapsedTextEntityReference = handheldElapsedReference;
                overrideController.RaysPerSecondTextEntityReference = handheldRaysPerSecondReference;
                PlatformEditingService.MarkPropertyOverride(commonController, saveComponent, platformId, nameof(SoftwarePathTracerComponent.SppTextEntityReference));
                PlatformEditingService.MarkPropertyOverride(commonController, saveComponent, platformId, nameof(SoftwarePathTracerComponent.ElapsedTextEntityReference));
                PlatformEditingService.MarkPropertyOverride(commonController, saveComponent, platformId, nameof(SoftwarePathTracerComponent.RaysPerSecondTextEntityReference));
                PlatformEditingService.PersistPlatformOverride(commonController, overrideController, saveComponent, platformId);
            }
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
