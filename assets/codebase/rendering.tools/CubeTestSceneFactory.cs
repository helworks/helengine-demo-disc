using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical authored scene asset for the minimal cube rendering test.
    /// </summary>
    public sealed class CubeTestSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated cube-test asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.CubeTestSceneId;

        /// <summary>
        /// Stable serialized component identifier used by mesh records.
        /// </summary>
        const string MeshComponentTypeId = "helengine.MeshComponent";

        /// <summary>
        /// Stable serialized component identifier used by camera records.
        /// </summary>
        const string CameraComponentTypeId = "helengine.CameraComponent";

        /// <summary>
        /// Stable serialized component identifier used by directional-light records.
        /// </summary>
        const string DirectionalLightComponentTypeId = "helengine.DirectionalLightComponent";

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
        /// Descriptor used to serialize authored mesh payloads for committed editor scenes.
        /// </summary>
        readonly MeshComponentPersistenceDescriptor MeshDescriptor;

        /// <summary>
        /// Descriptor used to serialize authored directional-light payloads for committed editor scenes.
        /// </summary>
        readonly DirectionalLightComponentPersistenceDescriptor DirectionalLightDescriptor;

        /// <summary>
        /// Placeholder runtime model used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly AuthoringPlaceholderRuntimeModel PlaceholderModel;

        /// <summary>
        /// Placeholder runtime material used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly RuntimeMaterial PlaceholderMaterial;

        /// <summary>
        /// Initializes the cube-test scene factory with the persistence descriptors required for authored output.
        /// </summary>
        public CubeTestSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical cube-test scene asset.
        /// </summary>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <param name="standardMaterialReference">Stable generated standard material reference.</param>
        /// <returns>Authored scene asset for the minimal cube test.</returns>
        public SceneAsset CreateSceneAsset(SceneAssetReference cubeReference, SceneAssetReference standardMaterialReference) {
            if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            } else if (standardMaterialReference == null) {
                throw new ArgumentNullException(nameof(standardMaterialReference));
            }

            return new SceneAsset {
                Id = SceneId,
                AssetReferences = new[] {
                    cubeReference,
                    standardMaterialReference
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateCubeEntity(cubeReference, standardMaterialReference)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the cube-test scene.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            return new SceneEntityAsset {
                Id = "cube-test-camera",
                Name = "CubeTestCamera",
                LocalPosition = new float3(0f, 0f, 6f),
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(1)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored directional light entity for the cube-test scene.
        /// </summary>
        /// <returns>Serialized directional light entity.</returns>
        SceneEntityAsset CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.65f, -0.85f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "cube-test-sun",
                Name = "CubeTestSun",
                LocalPosition = new float3(0f, 4f, 0f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateDirectionalLightComponentRecord(1f, 24f)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored cube mesh entity for the minimal rendering scene.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized cube entity.</returns>
        SceneEntityAsset CreateCubeEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = "cube-test-cube",
                Name = "CubeTestCube",
                LocalPosition = float3.Zero,
                LocalScale = new float3(2f, 2f, 2f),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReference),
                    RenderingScriptComponentRecordFactory.CreateTowerSpinRecord(1, 0f, (float)(Math.PI / 2.0))
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one serialized camera component record.
        /// </summary>
        /// <returns>Serialized camera component record.</returns>
        SceneComponentAssetRecord CreateCameraComponentRecord() {
            return new SceneComponentAssetRecord {
                ComponentTypeId = CameraComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteCameraPayload()
            };
        }

        /// <summary>
        /// Creates one serialized mesh component record.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized mesh component record.</returns>
        SceneComponentAssetRecord CreateMeshComponentRecord(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = MeshComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteMeshPayload(modelReference, materialReference)
            };
        }

        /// <summary>
        /// Creates one serialized directional-light component record.
        /// </summary>
        /// <param name="intensity">Authored directional-light intensity.</param>
        /// <param name="shadowDistance">Authored directional-light shadow cutoff distance.</param>
        /// <returns>Serialized directional-light component record.</returns>
        SceneComponentAssetRecord CreateDirectionalLightComponentRecord(float intensity, float shadowDistance) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = DirectionalLightComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteDirectionalLightPayload(intensity, shadowDistance)
            };
        }

        /// <summary>
        /// Writes one serialized camera component payload.
        /// </summary>
        /// <returns>Serialized camera component payload.</returns>
        byte[] WriteCameraPayload() {
            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("CameraDrawOrder", fieldWriter => fieldWriter.WriteByte(0));
            writer.WriteField("LayerMask", fieldWriter => fieldWriter.WriteUInt16(SceneObjectsLayerMask));
            writer.WriteField("Viewport", fieldWriter => fieldWriter.WriteFloat4(new float4(0f, 0f, 1f, 1f)));
            writer.WriteField("NearPlaneDistance", fieldWriter => fieldWriter.WriteSingle(0.1f));
            writer.WriteField("FarPlaneDistance", fieldWriter => fieldWriter.WriteSingle(64f));
            writer.WriteField(
                "ClearSettings",
                fieldWriter => SceneComponentBinaryFieldEncoding.WriteCameraClearSettings(
                    fieldWriter,
                    new CameraClearSettings(
                        true,
                        new float4(0.04f, 0.04f, 0.05f, 1f),
                        true,
                        1f,
                        false,
                        0)));
            writer.WriteField(
                "RenderSettings",
                fieldWriter => SceneComponentBinaryFieldEncoding.WriteCameraRenderSettings(
                    fieldWriter,
                    new CameraRenderSettings {
                        DepthPrepassMode = DepthPrepassMode.Auto,
                        ShadowDistance = 24f,
                        PostProcessTier = PostProcessTier.Disabled
                    }));
            return writer.BuildPayload();
        }

        /// <summary>
        /// Writes one serialized mesh component payload.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh.</param>
        /// <returns>Serialized mesh component payload.</returns>
        byte[] WriteMeshPayload(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            } else if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            MeshComponent meshComponent = new MeshComponent {
                Model = PlaceholderModel,
                Material = PlaceholderMaterial,
                RenderOrder3D = 0
            };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference(MeshModelReferenceName, modelReference);
            saveState.SetAssetReference(MeshMaterialReferenceName, materialReference);
            return MeshDescriptor.SerializeComponent(meshComponent, 0, saveState).Payload;
        }

        /// <summary>
        /// Writes one serialized directional-light component payload.
        /// </summary>
        /// <param name="intensity">Authored directional-light intensity.</param>
        /// <param name="shadowDistance">Authored directional-light shadow cutoff distance.</param>
        /// <returns>Serialized directional-light component payload.</returns>
        byte[] WriteDirectionalLightPayload(float intensity, float shadowDistance) {
            DirectionalLightComponent lightComponent = new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = intensity,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = shadowDistance
            };
            return DirectionalLightDescriptor.SerializeComponent(lightComponent, 0, null).Payload;
        }
    }
}
