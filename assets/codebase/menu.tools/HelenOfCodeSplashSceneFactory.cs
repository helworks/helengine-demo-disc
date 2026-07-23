using city.menu;
using city.rendering.tools;
using helengine;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Authors the additive Helen of Code splash scene used before the standard demo-disc menu.
    /// </summary>
    public sealed class HelenOfCodeSplashSceneFactory {
        /// <summary>
        /// Stable scene id used by the startup package and runtime self-unload request.
        /// </summary>
        public const string SceneId = "HelenOfCodeSplash";

        /// <summary>
        /// Project-relative path to the splash logo texture.
        /// </summary>
        public const string LogoTexturePath = "images/splash/helen_of_code_logo.png";

        /// <summary>
        /// Runtime layer mask used by the splash sprites and camera.
        /// </summary>
        const byte RuntimeLayerMask = 0b00000001;

        /// <summary>
        /// Draw order used by the splash camera after the main-menu camera.
        /// </summary>
        const byte SplashCameraDrawOrder = 1;

        /// <summary>
        /// Creates the generated splash scene definition with a post-menu camera and centered sprites.
        /// </summary>
        /// <returns>Generated authored splash scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset {
                    CanvasProfile = new SceneCanvasProfile {
                        Width = DemoMenuLayout.CanvasWidth,
                        Height = DemoMenuLayout.CanvasHeight
                    }
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateSplashRootEntity()
                }
            };
        }

        /// <summary>
        /// Creates the overlay camera that draws after the additive main-menu camera without clearing it.
        /// </summary>
        /// <returns>Authored splash overlay camera.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("HelenOfCodeSplashCamera");
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = SplashCameraDrawOrder,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    false,
                    new float4(0f, 0f, 0f, 1f),
                    false,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the screen-fit splash root, background sprite, logo sprite, and runtime transition component.
        /// </summary>
        /// <returns>Authored splash root entity.</returns>
        Entity CreateSplashRootEntity() {
            Entity entity = Core.Instance.EntityFactory.Create(SceneId);
            entity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(DemoMenuLayout.CanvasWidth, DemoMenuLayout.CanvasHeight)
            });
            entity.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = DemoMenuLayout.CanvasWidth,
                ReferenceHeight = DemoMenuLayout.CanvasHeight
            });

            CreateBackgroundEntity(entity);
            CreateLogoEntity(entity);
            entity.AddComponent(new HelenOfCodeSplashComponent());
            return entity;
        }

        /// <summary>
        /// Creates the opaque black sprite that masks the additive menu during the splash.
        /// </summary>
        /// <param name="parent">Splash root entity that owns the background.</param>
        void CreateBackgroundEntity(Entity parent) {
            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "HelenOfCodeSplashBackground");
            entity.LocalPosition = new float3(0f, 0f, 0f);
            entity.AddComponent(new SpriteComponent {
                Size = new int2(DemoMenuLayout.CanvasWidth, DemoMenuLayout.CanvasHeight),
                Color = new byte4(0, 0, 0, 255),
                RenderOrder2D = 1,
                LayerMask = RuntimeLayerMask
            });
        }

        /// <summary>
        /// Creates the centered logo sprite sized to 90 percent of the authored canvas height.
        /// </summary>
        /// <param name="parent">Splash root entity that owns the logo.</param>
        void CreateLogoEntity(Entity parent) {
            int logoSize = (int)Math.Round(DemoMenuLayout.CanvasHeight * 0.9d);
            int logoOffset = (DemoMenuLayout.CanvasHeight - logoSize) / 2;
            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "HelenOfCodeSplashLogo");
            entity.LocalPosition = new float3(
                (DemoMenuLayout.CanvasWidth - logoSize) / 2f,
                logoOffset,
                0.1f);
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = new int2(logoSize, logoSize),
                Color = new byte4(255, 255, 255, 0),
                RenderOrder2D = 2,
                LayerMask = RuntimeLayerMask
            };
            entity.AddComponent(spriteComponent);
            ApplyTextureReference(entity, spriteComponent, LogoTexturePath);
        }

        /// <summary>
        /// Stores the file-backed logo reference on the generated sprite entity.
        /// </summary>
        /// <param name="entity">Logo entity that owns the save metadata.</param>
        /// <param name="component">Logo sprite receiving the texture reference.</param>
        /// <param name="texturePath">Project-relative logo texture path.</param>
        void ApplyTextureReference(Entity entity, Component component, string texturePath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(texturePath)) {
                throw new ArgumentException("Splash logo texture path must be provided.", nameof(texturePath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                component,
                TextureAssetScenePersistenceSupport.TextureReferenceName,
                global::helengine.SceneAssetReferenceFactory.CreateFileSystemTexture(texturePath));
        }

        /// <summary>
        /// Resolves the editor save component attached to one generated entity.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Required generated entity save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated splash entities must expose initialized component collections.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException($"Generated splash entity '{SceneId}' is missing its editor save component.");
        }
    }
}
