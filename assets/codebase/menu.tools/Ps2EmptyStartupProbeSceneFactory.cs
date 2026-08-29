using city.rendering.tools;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Builds one temporary camera-only startup scene that overwrites the demo-disc main menu during PS2 leak isolation.
    /// </summary>
    public sealed class Ps2EmptyStartupProbeSceneFactory {
        /// <summary>
        /// Session-owned authoring graph used for the temporary probe entities.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        /// <summary>
        /// Initializes one empty-startup probe scene factory.
        /// </summary>
        public Ps2EmptyStartupProbeSceneFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }

        /// <summary>
        /// Creates one camera-only authored scene definition that reuses the demo-disc main menu scene id.
        /// </summary>
        /// <returns>Generated authored scene definition used for temporary PS2 startup probing.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = DemoDiscMainMenuSceneFactory.SceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = new[] {
                    CreateCameraEntity()
                }
            };
        }

        /// <summary>
        /// Creates the lone camera entity used by the empty startup probe.
        /// </summary>
        /// <returns>Camera entity that clears the frame and renders no scene content.</returns>
        Entity CreateCameraEntity() {
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("Ps2EmptyStartupProbeCamera");
            entity.LocalPosition = new float3(0f, 0f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 64f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(0f, 0f, 0f, 1f),
                    true,
                    1f,
                    false,
                    0)
            });
            return entity;
        }
    }
}
