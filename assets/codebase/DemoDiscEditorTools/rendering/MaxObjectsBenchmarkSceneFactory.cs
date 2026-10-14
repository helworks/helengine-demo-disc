using DemoDisc.menu;
using DemoDisc.rendering;
using helengine;
using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Builds the generated Max Objects scene, which grows a cube grid until its sampled frame rate reaches 30 FPS.
    /// </summary>
    public sealed class MaxObjectsBenchmarkSceneFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Stable scene id used by the generated benchmark asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.MaxObjectsBenchmarkSceneId;

        /// <summary>
        /// Initializes the benchmark factory with the project authoring capability used by its scene entities.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned project authoring service.</param>
        public MaxObjectsBenchmarkSceneFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        /// <summary>
        /// Creates the live-authored Max Objects benchmark scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube model shared by every benchmark object.</param>
        /// <param name="cubeMaterial">Generated solid-color material shared by every benchmark object.</param>
        /// <returns>Generated benchmark scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(RuntimeModel cubeModel, RuntimeMaterial cubeMaterial) {
            if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (cubeMaterial == null) {
                throw new ArgumentNullException(nameof(cubeMaterial));
            }

            FontAsset font = ResolveRequiredEditorFont();
            Entity uiEntity = new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateStandardSceneUi("MaxObjectsBenchmarkUi", "Benchmarks / Max Objects");
            uiEntity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Materials = new[] { cubeMaterial },
                RenderOrder3D = 0
            });
            CreateCameraEntity(uiEntity);
            CreateStatusTextEntity(uiEntity, font);
            uiEntity.AddComponent(new MaxObjectsBenchmarkComponent {
                TargetFramesPerSecond = 30f,
                SpawnIntervalSeconds = 1d
            });

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    BottomScreenRootEntities = [CreateHandheldUiEntity()]
                },
                RootEntities = [CreateDirectionalLightEntity()],
                DesktopPresentationRootEntities = [uiEntity]
            };
        }

        Entity CreateCameraEntity(Entity parent) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, "MaxObjectsBenchmarkCamera");
            entity.LocalPosition = new float3(0f, 0f, 18f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 1000f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
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

        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("MaxObjectsBenchmarkLight");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(-3f, 5f, 4f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0f,
                ShadowDistance = 0f
            });
            return entity;
        }

        Entity CreateStatusTextEntity(Entity parent, FontAsset font) {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(parent, "MaxObjectsBenchmarkStatus");
            entity.LocalPosition = new float3(16f, 192f, 0.1f);
            entity.Static = false;
            SceneUiDepth.SetWorldDepth(entity, 1);
            TextComponent statusText = new TextComponent {
                Text = "Objects: 1    Avg FPS: measuring    Target: 30\nAdding 1 cube/sec",
                Font = font,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(1024, 2 * DemoDiscSceneUiFont.ReferenceRowHeight),
                NativeFontPixels = DemoDiscSceneUiFont.Mode,
            };
            entity.AddComponent(statusText);
            DemoDiscSceneUiFont.StoreReference(AssetAuthoringService, entity, statusText);
            return entity;
        }

        Entity CreateHandheldUiEntity() {
            return new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateHandheldSceneUi("MaxObjectsBenchmarkHandheldUi", "Benchmarks / Max Objects");
        }

        FontAsset ResolveRequiredEditorFont() {
            if (AssetAuthoringService.RendererResources.DefaultFontAsset == null) {
                throw new InvalidOperationException("The Max Objects benchmark requires the default editor UI font to be loaded before scene generation.");
            }

            return AssetAuthoringService.RendererResources.DefaultFontAsset;
        }
    }
}
