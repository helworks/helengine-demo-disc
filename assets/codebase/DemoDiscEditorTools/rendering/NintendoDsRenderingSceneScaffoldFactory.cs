using DemoDisc.menu;
using DemoDisc.rendering;
using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Builds the shared dual-screen bottom screen: its camera, its viewport, and the chrome Blueprint every
    /// handheld scene instances. The scene's own content is authored once and shared, so nothing here inspects
    /// or rewrites it.
    /// </summary>
    public sealed class NintendoDsRenderingSceneScaffoldFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Initializes the scaffold with the host-owned public asset authoring capability.
        /// </summary>
        public NintendoDsRenderingSceneScaffoldFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        /// <summary>
        /// Runtime layer mask used by generated bottom-screen entities so the handheld cameras can render them.
        /// </summary>
        const ushort PersistedSceneLayerMask = EditorLayerMasks.SceneObjects;

        /// <summary>
        /// Fixed screen width the shared bottom layout is authored against.
        /// </summary>
        const int ScreenWidth = 256;

        /// <summary>
        /// Fixed screen height the shared bottom layout is authored against.
        /// </summary>
        const int ScreenHeight = 192;

        /// <summary>
        /// Exact demo-disc lilac clear color used by the shared bottom-screen camera.
        /// </summary>
        static readonly float4 NintendoDsBottomScreenClearColor = new float4(30f / 255f, 17f / 255f, 41f / 255f, 1f);

        /// <summary>
        /// Creates the dual-screen presentation roots for one generated scene.
        /// </summary>
        /// <param name="bottomScreenRoots">Scene-authored content mounted on the bottom screen.</param>
        /// <returns>Dual-screen presentation roots, restricted to the dual-screen group by the caller.</returns>
        public Entity[] CreateBottomScreenRoots(Entity[] bottomScreenRoots) {
            if (bottomScreenRoots == null) {
                throw new ArgumentNullException(nameof(bottomScreenRoots));
            }

            Entity bottomScreenCameraEntity = CreateBottomScreenCameraEntity();
            Entity bottomScreenViewportRoot = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(bottomScreenCameraEntity, "DemoDiscBottomScreenRoot");
            bottomScreenViewportRoot.LayerMask = PersistedSceneLayerMask;
            bottomScreenViewportRoot.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(ScreenWidth, ScreenHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = ScreenWidth,
                ReferenceHeight = ScreenHeight
            });

            AttachBottomScreenChrome(bottomScreenViewportRoot);
            AttachBottomScreenRoots(bottomScreenViewportRoot, bottomScreenRoots);
            return new[] { bottomScreenCameraEntity };
        }

        /// <summary>
        /// Creates the dedicated bottom-screen camera entity.
        /// </summary>
        /// <returns>Bottom-screen camera entity.</returns>
        Entity CreateBottomScreenCameraEntity() {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("DemoDiscBottomScreenCamera");
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 1,
                // The pointer hit resolver rejects interactables whose entity mask misses the camera mask,
                // so the bottom camera must render the same SceneObjects layer its buttons live on.
                LayerMask = PersistedSceneLayerMask,
                Viewport = new float4(0f, 1f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    true,
                    NintendoDsBottomScreenClearColor,
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
        /// Attaches the shared bottom-screen chrome Blueprint beneath the bottom viewport root. The light and
        /// back buttons are identical in every handheld scene, so they arrive as one Blueprint instance.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the chrome.</param>
        void AttachBottomScreenChrome(Entity bottomScreenViewportRoot) {
            Entity chromeEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(
                bottomScreenViewportRoot,
                HandheldBottomScreenChromeBlueprintGenerator.ChromeRootEntityName);
            chromeEntity.LayerMask = PersistedSceneLayerMask;
            chromeEntity.LocalPosition = float3.Zero;
            chromeEntity.LocalScale = float3.One;
            chromeEntity.LocalOrientation = float4.Identity;
            chromeEntity.AddComponent(new BlueprintInstanceComponent {
                BlueprintAssetReference = AssetAuthoringService.CreateFileReference(
                    HandheldBottomScreenChromeBlueprintGenerator.BlueprintRelativePath,
                    AssetEntryKind.Blueprint)
            });
        }

        /// <summary>
        /// Mounts the scene-authored bottom-screen content beneath the bottom viewport root.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Viewport root that should own the content.</param>
        /// <param name="bottomScreenRoots">Scene-authored bottom-screen roots.</param>
        void AttachBottomScreenRoots(Entity bottomScreenViewportRoot, Entity[] bottomScreenRoots) {
            for (int index = 0; index < bottomScreenRoots.Length; index++) {
                Entity rootEntity = bottomScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                }

                if (rootEntity.Parent != null) {
                    rootEntity.Parent.RemoveChild(rootEntity);
                }

                bottomScreenViewportRoot.AddChild(rootEntity);
            }
        }
    }
}
