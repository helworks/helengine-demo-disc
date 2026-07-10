using helengine.editor;
using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Builds the generated authored Zombislayer gameplay scene contributed by the city demo-disc project.
    /// </summary>
    public sealed class ZombislayerSceneFactory {
        /// <summary>
        /// Stable authored source-font path used by the Zombislayer pause overlay.
        /// </summary>
        const string PauseOverlayFontRelativePath = "Fonts/Fredoka.ttf";

        /// <summary>
        /// Stable save-state reference slot used by generated imported mesh entities.
        /// </summary>
        const string MeshModelReferenceName = "Model";

        /// <summary>
        /// Prepared imported environment runtime model.
        /// </summary>
        readonly RuntimeModel EnvironmentModel;

        /// <summary>
        /// Prepared imported weapon runtime model.
        /// </summary>
        readonly RuntimeModel WeaponModel;

        /// <summary>
        /// Initializes one Zombislayer scene factory backed by the prepared imported runtime assets.
        /// </summary>
        /// <param name="assets">Prepared imported runtime assets.</param>
        public ZombislayerSceneFactory(ZombislayerGenerationAssets assets) {
            if (assets == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.EnvironmentModel == null) {
                throw new ArgumentException("Zombislayer scene generation requires the imported environment runtime model.", nameof(assets));
            } else if (assets.WeaponModel == null) {
                throw new ArgumentException("Zombislayer scene generation requires the imported weapon runtime model.", nameof(assets));
            }

            EnvironmentModel = assets.EnvironmentModel;
            WeaponModel = assets.WeaponModel;
        }

        /// <summary>
        /// Creates the generated authored Zombislayer gameplay scene.
        /// </summary>
        /// <returns>Generated authored gameplay scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateGameplayScene() {
            Entity sceneRootEntity = CreateSceneRootEntity();
            CreateEnvironmentEntity(sceneRootEntity);
            CreateDirectionalLightEntity(sceneRootEntity);
            CreatePlayerRootEntity(sceneRootEntity);
            CreateUiRootEntity(sceneRootEntity);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = GameSceneCatalog.ZombislayerSceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = [
                    sceneRootEntity
                ]
            };
        }

        /// <summary>
        /// Creates the shared top-level root used by the generated Zombislayer scene.
        /// </summary>
        /// <returns>Generated root entity.</returns>
        Entity CreateSceneRootEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("ZombislayerSceneRoot");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            return entity;
        }

        /// <summary>
        /// Creates the imported environment entity that owns the static level mesh.
        /// </summary>
        /// <param name="parent">Scene root that should own the environment entity.</param>
        /// <returns>Generated environment entity.</returns>
        Entity CreateEnvironmentEntity(Entity parent) {
            SceneAssetReference environmentModelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(ZombislayerAssetCatalog.EnvironmentModelRelativePath);
            return CreateImportedMeshEntity(
                parent,
                "ZombislayerEnvironment",
                float3.Zero,
                float3.One,
                float4.Identity,
                EnvironmentModel,
                ZombislayerAssetCatalog.EnvironmentModelRelativePath,
                environmentModelReference);
        }

        /// <summary>
        /// Creates the authored directional light used by the Zombislayer scene.
        /// </summary>
        /// <param name="parent">Scene root that should own the light entity.</param>
        /// <returns>Generated light entity.</returns>
        Entity CreateDirectionalLightEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.72f, -0.58f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "ZombislayerSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 12f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.96f, 0.92f, 1f),
                Intensity = 1.1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.9f,
                ShadowDistance = 90f
            });
            return entity;
        }

        /// <summary>
        /// Creates the player root that owns the first-person controller and camera hierarchy.
        /// </summary>
        /// <param name="parent">Scene root that should own the player root.</param>
        /// <returns>Generated player root entity.</returns>
        EditorEntity CreatePlayerRootEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "ZombislayerPlayerRoot");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 1.65f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new city.game.ZombislayerFpsControllerComponent());
            Entity cameraPivotEntity = CreateCameraPivotEntity(entity);
            CreateCameraEntity(cameraPivotEntity);
            CreateWeaponEntity(cameraPivotEntity);

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Zombislayer player-root generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the pitch-only camera pivot consumed by the first-person controller.
        /// </summary>
        /// <param name="parent">Player root that should own the camera pivot.</param>
        /// <returns>Generated camera pivot entity.</returns>
        Entity CreateCameraPivotEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, city.game.ZombislayerFpsControllerComponent.CameraPivotEntityName);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            return entity;
        }

        /// <summary>
        /// Creates the first-person gameplay camera.
        /// </summary>
        /// <param name="parent">Camera pivot that should own the gameplay camera.</param>
        /// <returns>Generated gameplay camera entity.</returns>
        Entity CreateCameraEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "ZombislayerCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.05f,
                FarPlaneDistance = 180f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(18f / 255f, 27f / 255f, 43f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 90f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the imported first-person M4 viewmodel entity.
        /// </summary>
        /// <param name="parent">Camera pivot that should own the weapon entity.</param>
        /// <returns>Generated weapon entity.</returns>
        Entity CreateWeaponEntity(Entity parent) {
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)(Math.PI * 0.5d), 0f, 0f, out orientation);
            SceneAssetReference weaponModelReference = global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(ZombislayerAssetCatalog.WeaponModelRelativePath);
            return CreateImportedMeshEntity(
                parent,
                "ZombislayerWeapon",
                new float3(0.24f, -0.22f, -0.42f),
                new float3(0.014f, 0.014f, 0.014f),
                orientation,
                WeaponModel,
                ZombislayerAssetCatalog.WeaponModelRelativePath,
                weaponModelReference);
        }

        /// <summary>
        /// Creates the authored UI root that owns the gameplay session component and pause overlay.
        /// </summary>
        /// <param name="parent">Scene root that should own the UI root.</param>
        /// <returns>Generated UI root entity.</returns>
        EditorEntity CreateUiRootEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, city.game.ZombislayerFpsControllerComponent.SessionRootEntityName);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new city.game.ZombislayerSessionComponent());
            entity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(1280, 720)
            });
            entity.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = 1280,
                ReferenceHeight = 720
            });

            Entity pauseOverlayEntity = CreatePauseOverlayEntity(entity);
            pauseOverlayEntity.Enabled = false;

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Zombislayer UI generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the generated pause overlay shown when the Zombislayer session is paused.
        /// </summary>
        /// <param name="parent">UI root that should own the pause overlay.</param>
        /// <returns>Generated pause overlay entity.</returns>
        Entity CreatePauseOverlayEntity(Entity parent) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            }

            Entity panelEntity = CreateRoundedPanelEntity(
                parent,
                "ZombislayerPauseOverlay",
                new float3(360f, 160f, 0f),
                new int2(560, 260),
                28f,
                3f,
                new byte4(18, 27, 43, 236),
                new byte4(104, 134, 178, 255),
                4);
            CreateUiTextEntity(panelEntity, "ZombislayerPauseTitle", new float3(36f, 28f, 0.1f), "Paused", new int2(220, 48), 2.1f, 5, new byte4(247, 248, 252, 255), TextAlignment.Left);
            CreateUiTextEntity(panelEntity, "ZombislayerPauseResumeText", new float3(36f, 102f, 0.1f), "Esc or Enter  Resume", new int2(420, 44), 1.25f, 5, new byte4(247, 248, 252, 255), TextAlignment.Left);
            CreateUiTextEntity(panelEntity, "ZombislayerPauseReturnText", new float3(36f, 152f, 0.1f), "Backspace  Return to Demo Disc", new int2(480, 44), 1.25f, 5, new byte4(214, 226, 244, 255), TextAlignment.Left);
            return panelEntity;
        }

        /// <summary>
        /// Creates one imported mesh entity and stores its file-backed model reference on the hidden editor save state.
        /// </summary>
        /// <param name="parent">Parent entity that should own the imported mesh.</param>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="localOrientation">Local orientation assigned to the entity.</param>
        /// <param name="model">Prepared imported runtime model.</param>
        /// <param name="modelRelativePath">Project-relative imported model path.</param>
        /// <param name="modelReference">File-backed model reference written into the save state.</param>
        /// <returns>Generated imported mesh entity.</returns>
        Entity CreateImportedMeshEntity(
            Entity parent,
            string name,
            float3 localPosition,
            float3 localScale,
            float4 localOrientation,
            RuntimeModel model,
            string modelRelativePath,
            SceneAssetReference modelReference) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (model == null) {
                throw new ArgumentNullException(nameof(model));
            } else if (string.IsNullOrWhiteSpace(modelRelativePath)) {
                throw new ArgumentException("Model path must be provided.", nameof(modelRelativePath));
            } else if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = localOrientation;
            MeshComponent meshComponent = new MeshComponent {
                Model = model,
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyImportedModelAssetReference(entity, meshComponent, modelRelativePath, modelReference);
            return entity;
        }

        /// <summary>
        /// Creates one rounded panel entity for the pause overlay.
        /// </summary>
        /// <param name="parent">Parent entity that should own the panel.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the panel.</param>
        /// <param name="size">Panel size in authored pixels.</param>
        /// <param name="radius">Rounded corner radius.</param>
        /// <param name="borderThickness">Border thickness in pixels.</param>
        /// <param name="fillColor">Panel fill color.</param>
        /// <param name="borderColor">Panel border color.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <returns>Generated panel entity.</returns>
        Entity CreateRoundedPanelEntity(Entity parent, string entityName, float3 localPosition, int2 size, float radius, float borderThickness, byte4 fillColor, byte4 borderColor, byte renderOrder2D) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = false;
            entity.AddComponent(new RoundedRectComponent {
                Size = size,
                Radius = radius,
                BorderThickness = borderThickness,
                FillColor = fillColor,
                BorderColor = borderColor,
                RenderOrder2D = renderOrder2D,
                LayerMask = 1
            });
            return entity;
        }

        /// <summary>
        /// Creates one reusable UI text entity using the shared pause-overlay font.
        /// </summary>
        /// <param name="parent">Parent entity that should own the text.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local entity position.</param>
        /// <param name="text">Initial text content.</param>
        /// <param name="size">Text layout size in authored pixels.</param>
        /// <param name="fontScale">Uniform glyph scale.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <param name="color">Text color.</param>
        /// <param name="alignment">Requested text alignment.</param>
        /// <returns>Created text entity.</returns>
        Entity CreateUiTextEntity(Entity parent, string entityName, float3 localPosition, string text, int2 size, float fontScale, byte renderOrder2D, byte4 color, TextAlignment alignment) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = false;
            TextComponent textComponent = new TextComponent {
                Text = text ?? string.Empty,
                Font = ResolveRequiredEditorFont(),
                Color = color,
                Size = size,
                FontScale = fontScale,
                Alignment = alignment,
                RenderOrder2D = renderOrder2D,
                LayerMask = 1
            };
            entity.AddComponent(textComponent);
            ApplyFontReference(entity, textComponent, PauseOverlayFontRelativePath);
            return entity;
        }

        /// <summary>
        /// Resolves the editor font used by the generated pause-overlay text entities.
        /// </summary>
        /// <returns>Loaded default editor font.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the Zombislayer scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Stores the supplied file-backed font reference on the entity save state for the supplied text component.
        /// </summary>
        /// <param name="entity">Entity that owns the text component.</param>
        /// <param name="component">Text component whose font reference should be stored.</param>
        /// <param name="fontPath">Project-relative font path.</param>
        void ApplyFontReference(Entity entity, TextComponent component, string fontPath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(fontPath)) {
                throw new ArgumentException("Font path must be provided.", nameof(fontPath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", global::helengine.SceneAssetReferenceFactory.CreateFileSystemFont(fontPath));
        }

        /// <summary>
        /// Stores the generated imported model reference on one mesh entity save state.
        /// </summary>
        /// <param name="entity">Entity that owns the mesh component.</param>
        /// <param name="component">Mesh component whose model reference should be stored.</param>
        /// <param name="modelRelativePath">Project-relative imported model path.</param>
        /// <param name="modelReference">File-backed model reference written into the save state.</param>
        void ApplyImportedModelAssetReference(Entity entity, MeshComponent component, string modelRelativePath, SceneAssetReference modelReference) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(modelRelativePath)) {
                throw new ArgumentException("Model path must be provided.", nameof(modelRelativePath));
            } else if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, MeshModelReferenceName, modelReference);
        }

        /// <summary>
        /// Resolves the hidden save component attached to one generated editor entity.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }
    }
}
