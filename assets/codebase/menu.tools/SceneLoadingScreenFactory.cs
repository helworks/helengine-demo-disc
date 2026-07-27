using city.menu;
using city.rendering.tools;
using helengine;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Authors the persistent full-screen loading overlay used by normal Demo Disc scene transitions.
    /// </summary>
    public sealed class SceneLoadingScreenFactory {
        /// <summary>
        /// Stable authored scene path used by the runtime catalog.
        /// </summary>
        public const string SceneId = "Scenes/SceneLoadingScreen.helen";

        /// <summary>
        /// Runtime layer used exclusively by the final loading overlay camera.
        /// </summary>
        const ushort RuntimeLayerMask = 0b0000000000000100;

        /// <summary>
        /// Creates the persistent loading-scene definition.
        /// </summary>
        /// <returns>Generated authored loading scene.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition() {
            Entity camera = Core.Instance.EntityFactory.Create("SceneLoadingScreenCamera");
            camera.LayerMask = RuntimeLayerMask;
            camera.AddComponent(new CameraComponent {
                CameraDrawOrder = byte.MaxValue,
                LayerMask = RuntimeLayerMask,
                Viewport = new float4(0f, 0f, 1f, 1f),
                ClearSettings = new CameraClearSettings(false, new float4(0f, 0f, 0f, 1f), false, 1f, false, 0)
            });

            Entity background = CreateRectangle(camera, "LoadingBackground", new float3(0f, 0f, 0f), new int2(DemoMenuLayout.CanvasWidth, DemoMenuLayout.CanvasHeight), 1, new byte4(0, 0, 0, 0));
            Entity root = Core.Instance.EntityFactory.CreateChild(camera, SceneId);
            root.LayerMask = RuntimeLayerMask;
            root.AddComponent(new ViewportComponent { BindingMode = ViewportComponent.AncestorCameraBindingMode, FixedSize = new int2(DemoMenuLayout.CanvasWidth, DemoMenuLayout.CanvasHeight) });
            root.AddComponent(new ReferenceCanvasFitComponent { ReferenceWidth = DemoMenuLayout.CanvasWidth, ReferenceHeight = DemoMenuLayout.CanvasHeight });
            Entity track = CreateRectangle(root, "LoadingTrack", new float3(128f, DemoMenuLayout.CanvasHeight - 72f, 0.1f), new int2(DemoMenuLayout.CanvasWidth - 256, 22), 2, new byte4(40, 26, 56, 0));
            Entity fill = CreateRectangle(root, "LoadingFill", new float3(128f, DemoMenuLayout.CanvasHeight - 72f, 0.2f), new int2(0, 22), 3, new byte4(135, 94, 163, 0));
            root.AddComponent(new SceneLoadingScreenComponent {
                ProgressTrackWidth = DemoMenuLayout.CanvasWidth - 256,
                BackgroundEntityReference = CreateEntityReference(background),
                TrackEntityReference = CreateEntityReference(track),
                FillEntityReference = CreateEntityReference(fill)
            });

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset { DontUnload = true, CanvasProfile = new SceneCanvasProfile { Width = DemoMenuLayout.CanvasWidth, Height = DemoMenuLayout.CanvasHeight } },
                RootEntities = new[] { camera }
            };
        }

        /// <summary>
        /// Creates one generated rectangle child with the loading overlay runtime layer.
        /// </summary>
        /// <param name="parent">Overlay root that owns the rectangle.</param>
        /// <param name="name">Stable child name.</param>
        /// <param name="position">Authored local position.</param>
        /// <param name="size">Authored rectangle size.</param>
        /// <param name="renderOrder">Overlay draw order.</param>
        /// <param name="color">Initial transparent rectangle color.</param>
        /// <returns>Created rectangle entity with a stable persisted scene id.</returns>
        Entity CreateRectangle(Entity parent, string name, float3 position, int2 size, byte renderOrder, byte4 color) {
            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, name);
            entity.LayerMask = RuntimeLayerMask;
            entity.LocalPosition = position;
            entity.AddComponent(new RoundedRectComponent { Size = size, Radius = 0f, BorderThickness = 1f, FillColor = color, BorderColor = color, RenderOrder2D = renderOrder });
            return entity;
        }

        /// <summary>
        /// Creates a stable serialized scene reference for one generated rectangle entity.
        /// </summary>
        /// <param name="entity">Generated entity that should receive a persisted scene id.</param>
        /// <returns>Serialized reference resolving the generated entity at runtime.</returns>
        SceneEntityReference CreateEntityReference(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            if (saveComponent.EntityId == 0u) {
                if (Core.Instance is not EditorCore editorCore || editorCore.SceneEntityIdAllocator == null) {
                    throw new InvalidOperationException("Generated loading-screen references require an active editor scene-entity id allocator.");
                }

                saveComponent.EntityId = editorCore.SceneEntityIdAllocator.Allocate();
            }

            return new SceneEntityReference { EntityId = saveComponent.EntityId };
        }

        /// <summary>
        /// Finds the editor persistence component attached to one generated entity.
        /// </summary>
        /// <param name="entity">Generated entity whose persisted metadata is required.</param>
        /// <returns>Entity persistence component carrying the stable scene id.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException("Generated loading-screen entities must contain initialized components.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated loading-screen entities require one EntitySaveComponent.");
        }
    }
}
