using city.menu;
using gameplay.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the minimal two-cube depth-ordering and near-plane-clipping probe.
    /// </summary>
    public sealed class DepthClipProbeSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated depth-clip-probe asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/depth_clip_probe.helen";

        /// <summary>
        /// Stable PlayStation 2 platform identifier used by the shared component tessellation service.
        /// </summary>
        const string Ps2PlatformId = "ps2";

        /// <summary>
        /// Stable PSP platform identifier used by the shared component tessellation service.
        /// </summary>
        const string PspPlatformId = "psp";

        /// <summary>
        /// Maximum world-space triangle edge length permitted before cooking subdivides the wide box's model, matching the Tilt Trial course convention.
        /// </summary>
        const double WideBoxTessellationMaxEdgeLength = 1d;

        /// <summary>
        /// Service used to read and write editor-only MeshComponent tessellation metadata during live authoring.
        /// </summary>
        readonly MeshComponentTessellationSettingsService MeshComponentTessellationSettingsServiceValue = new MeshComponentTessellationSettingsService();

        /// <summary>
        /// Initializes one depth-clip-probe scene factory.
        /// </summary>
        public DepthClipProbeSceneFactory() { }

        /// <summary>
        /// Creates the canonical depth-clip-probe live scene definition.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to both authored meshes.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the wide outer box.</param>
        /// <param name="centerMaterial">Generated standard runtime material assigned to the tall center box.</param>
        /// <returns>Live-authored depth-clip-probe scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel cubeModel, RuntimeMaterial standardMaterial, RuntimeMaterial centerMaterial) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            } else if (centerMaterial == null) {
                throw new ArgumentNullException(nameof(centerMaterial));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
            ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new ConsoleCameraLightInstructionsSceneAttachmentService();
            consoleInstructionAttachmentService.ExcludeLegacyOverlayFromConsoles(projectRootPath, instructionOverlayEntity);
            Entity consoleInstructionBlueprintEntity = consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateUiEntity(),
                    CreateDirectionalLightEntity(),
                    instructionOverlayEntity,
                    consoleInstructionBlueprintEntity,
                    CreateTallBoxEntity(cubeModel, centerMaterial),
                    CreateWideBoxEntity(cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the depth-clip-probe scene, close enough by default to encourage near-plane clipping when zoomed further in.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.28f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("DepthClipProbeCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 3f, 7f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 64f,
                ClearSettings = new CameraClearSettings(true, new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f), true, 1f, false, 0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 24f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = float3.Zero,
                AutoYawSpeedRadians = 0f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the depth-clip-probe scene.
        /// </summary>
        /// <returns>Live authored UI root entity.</returns>
        Entity CreateUiEntity() {
            return new DemoDiscSceneUiKitFactory().CreateStandardSceneUi("DepthClipProbeUi", "Depth Clip Probe");
        }

        /// <summary>
        /// Creates the authored directional-light entity for the depth-clip-probe scene.
        /// </summary>
        /// <returns>Live authored directional-light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("DepthClipProbeSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 4f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 24f
            });
            return entity;
        }

        /// <summary>
        /// Creates the wide, flat 5-by-1-by-5 box centered at the origin.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored wide-box entity.</returns>
        Entity CreateWideBoxEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("DepthClipProbeWideBox");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = new float3(5f, 1f, 5f);
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = cubeModel,
                Materials = new[] { standardMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            return entity;
        }

        /// <summary>
        /// Marks a mesh component for cook-time tessellation on PlayStation 2 and PSP so its large flat faces are subdivided into small triangles, keeping the PS2 GS's screen-space Z interpolation accurate near the clip plane.
        /// </summary>
        /// <param name="entity">Entity that owns <paramref name="meshComponent"/> and must already carry an <see cref="EntitySaveComponent"/>.</param>
        /// <param name="meshComponent">Mesh component whose model should be tessellated at cook time.</param>
        void ApplyConstrainedPlatformTessellation(Entity entity, MeshComponent meshComponent) {
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            EntityComponentSaveState saveState = saveComponent.GetOrCreateComponentState(meshComponent);
            MeshComponentTessellationSettings settings = new MeshComponentTessellationSettings(true, WideBoxTessellationMaxEdgeLength);
            MeshComponentTessellationSettingsServiceValue.SetForPlatform(saveState, Ps2PlatformId, settings);
            MeshComponentTessellationSettingsServiceValue.SetForPlatform(saveState, PspPlatformId, settings);
        }

        /// <summary>
        /// Finds the required <see cref="EntitySaveComponent"/> on a generated editor entity.
        /// </summary>
        /// <param name="entity">Entity expected to carry a preassigned <see cref="EntitySaveComponent"/>.</param>
        /// <returns>The entity's save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    if (saveComponent.EntityId == 0u) {
                        throw new InvalidOperationException("Generated editor entities must have a preassigned numeric scene entity id.");
                    }
                    return saveComponent;
                }
            }
            throw new InvalidOperationException("Generated editor entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Creates the tall, thin 1-by-5-by-1 box centered at the origin so it intersects the wide box.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model assigned to the mesh.</param>
        /// <param name="standardMaterial">Generated standard runtime material assigned to the mesh.</param>
        /// <returns>Live authored tall-box entity.</returns>
        Entity CreateTallBoxEntity(RuntimeModel cubeModel, RuntimeMaterial standardMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("DepthClipProbeTallBox");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = new float3(1f, 5f, 1f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Materials = new[] { standardMaterial },
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font assigned to the generated FPS overlay during live authoring.
        /// </summary>
        /// <returns>Editor font asset used by the FPS overlay.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the depth-clip-probe scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
