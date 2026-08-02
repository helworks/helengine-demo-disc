using city.menu;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical authored scene asset for the directional-shadow plaza showcase.
    /// </summary>
    public sealed class DirectionalShadowPlazaSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated directional-shadow plaza asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/directional_shadow_plaza.helen";

        /// <summary>
        /// Layer mask used by user-authored scene objects in packaged runtime scenes.
        /// </summary>
        const ushort SceneObjectsLayerMask = 0b0100000000000000;

        /// <summary>
        /// Stable save-state slot name used for serialized mesh model references.
        /// </summary>
        const string MeshModelReferenceName = "Model";

        /// <summary>
        /// Stable save-state slot name used for serialized mesh material references.
        /// </summary>
        const string MeshMaterialReferenceName = "Material";

        /// <summary>
        /// Stable save-state slot name used for serialized font references.
        /// </summary>
        const string FontReferenceName = "Font";

        /// <summary>
        /// Placeholder font assigned during live authoring before the real generated editor-font reference is serialized.
        /// </summary>
        readonly FontAsset PlaceholderFont;

        /// <summary>
        /// Initializes one directional-shadow plaza scene factory.
        /// </summary>
        public DirectionalShadowPlazaSceneFactory() {
            PlaceholderFont = new FontAsset(
                new FontInfo("DirectionalShadowPlazaPlaceholder", 16, 4f),
                new ManagedRuntimeTexture {
                    Width = 1,
                    Height = 1
                },
                new Dictionary<char, FontChar>(),
                16f,
                1,
                1);
        }

        /// <summary>
        /// Creates the live-authored directional-shadow plaza scene definition that the editor save pipeline will serialize.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated prompt icons.</param>
        /// <param name="planeModel">Generated plane runtime model used by the ground mesh.</param>
        /// <param name="cubeModel">Generated cube runtime model used by the buildings and shadow mast.</param>
        /// <param name="sphereModel">Generated sphere runtime model used by the orbiting hero landmark.</param>
        /// <param name="standardMaterial">Runtime standard material assigned to every plaza mesh.</param>
        /// <returns>Live-authored scene definition for the directional-shadow plaza showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(
            string projectRootPath,
            RuntimeModel planeModel,
            RuntimeModel cubeModel,
            RuntimeModel sphereModel,
            RuntimeMaterial standardMaterial) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (planeModel == null) {
                throw new ArgumentNullException(nameof(planeModel));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (sphereModel == null) {
                throw new ArgumentNullException(nameof(sphereModel));
            } else if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
            Entity cameraEntity = CreateCameraEntity();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
            ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new ConsoleCameraLightInstructionsSceneAttachmentService();
            consoleInstructionAttachmentService.ExcludeLegacyOverlayFromConsoles(projectRootPath, instructionOverlayEntity);
            Entity consoleInstructionBlueprintEntity = consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = instructionOverlayFactory.CreateNintendoDsBottomInstructionRoots(instructionFont)
                },
                RootEntities = new[] {
                    cameraEntity,
                    instructionOverlayEntity,
                    consoleInstructionBlueprintEntity,
                    CreateFpsEntity(),
                    CreateDirectionalLightEntity(),
                    CreateGroundEntity(planeModel, standardMaterial),
                    CreateShadowMastEntity(cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaWestTower", new float3(-16f, 7f, -9f), new float3(6f, 14f, 6f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaCentralTower", new float3(0f, 9f, -12f), new float3(7f, 18f, 7f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaEastTower", new float3(15f, 6f, -7f), new float3(5f, 12f, 5f), cubeModel, standardMaterial),
                    CreateOrbitHeroEntity(sphereModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaSouthwestBlock", new float3(-15f, 3f, 12f), new float3(6f, 6f, 6f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaSouthCentralBlock", new float3(-4f, 2.5f, 14f), new float3(5f, 5f, 5f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaNortheastBlock", new float3(13f, 2f, 11f), new float3(4f, 4f, 4f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaMidriseBlock", new float3(8f, 3.5f, 2f), new float3(5f, 7f, 5f), cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the live directional-shadow plaza scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.28f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("DirectionalShadowPlazaCamera");
            entity.LocalPosition = new float3(0f, 24f, 64f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = SceneObjectsLayerMask,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 200f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 80f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = new float3(0f, 0f, 0f),
                AutoYawSpeedRadians = 0.07f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored FPS overlay entity for the live directional-shadow plaza scene.
        /// </summary>
        /// <returns>Live authored FPS overlay entity.</returns>
        Entity CreateFpsEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("DirectionalShadowPlazaFps");
            entity.LayerMask = SceneObjectsLayerMask;
            FPSComponent fpsComponent = new FPSComponent {
                Font = PlaceholderFont,
                FontScale = 2f
            };
            entity.AddComponent(fpsComponent);
            PspFpsComponentOverrideService.Apply(entity);
            ApplyEditorFontReference(entity, fpsComponent);
            entity.AddComponent(new DemoDiscLightToggleComponent());
            DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory();
            lightIndicatorOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont());
            DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
            sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "7. Shadow Plaza");
            return entity;
        }

        /// <summary>
        /// Creates the authored directional light entity for the live directional-shadow plaza scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.72f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("DirectionalShadowPlazaSun");
            entity.LayerMask = SceneObjectsLayerMask;
            entity.LocalPosition = new float3(0f, 18f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.95f, 0.9f, 1f),
                Intensity = 1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 80f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored ground receiver mesh for the live directional-shadow plaza scene.
        /// </summary>
        /// <param name="model">Runtime plane model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity("DirectionalShadowPlazaGround", new float3(0f, 0f, 0f), new float3(48f, 1f, 48f), model, material);
        }

        /// <summary>
        /// Creates the authored shadow mast mesh for the live directional-shadow plaza scene.
        /// </summary>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored shadow mast entity.</returns>
        Entity CreateShadowMastEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity("DirectionalShadowPlazaShadowMast", new float3(-9f, 7f, 4f), new float3(1.4f, 14f, 1.4f), model, material);
        }

        /// <summary>
        /// Creates one live authored building entity for the directional-shadow plaza scene.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored building entity.</returns>
        Entity CreateBuildingEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity(name, localPosition, localScale, model, material);
        }

        /// <summary>
        /// Creates the authored orbiting sphere landmark for the live directional-shadow plaza scene.
        /// </summary>
        /// <param name="model">Runtime sphere model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored orbit hero entity.</returns>
        Entity CreateOrbitHeroEntity(RuntimeModel model, RuntimeMaterial material) {
            Entity entity = CreateMeshEntity("DirectionalShadowPlazaHeroSphere", new float3(0f, 2.5f, 10f), new float3(3f, 3f, 3f), model, material);
            entity.AddComponent(new city.rendering.DirectionalShadowOrbitComponent {
                OrbitCenter = new float3(0f, 0f, 0f),
                OrbitRadius = 10f,
                OrbitHeight = 2.5f,
                BaseAngleRadians = 0.15f,
                AngularSpeedRadians = -0.18f
            });
            return entity;
        }

        /// <summary>
        /// Creates one shared mesh entity for the directional-shadow plaza showcase.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime model assigned to the mesh.</param>
        /// <param name="material">Runtime material assigned to the mesh.</param>
        /// <returns>Live authored mesh entity.</returns>
        Entity CreateMeshEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (model == null) {
                throw new ArgumentNullException(nameof(model));
            } else if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = SceneObjectsLayerMask;
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font assigned to generated runtime overlays during live authoring.
        /// </summary>
        /// <returns>Editor font asset used by the generated overlays.</returns>
        FontAsset ResolveRequiredEditorFont() {
            EditorCore editorCore = Core.Instance as EditorCore;
            if (editorCore == null || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the directional-shadow plaza scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Stores the generated editor-font reference on the entity save state for the supplied FPS component.
        /// </summary>
        /// <param name="entity">Entity that owns the FPS component.</param>
        /// <param name="component">FPS component whose font reference should be stored.</param>
        void ApplyEditorFontReference(Entity entity, FPSComponent component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, FontReferenceName, DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }
    }
}



