using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical authored scene asset for the directional-shadow plaza showcase.
    /// </summary>
    public sealed class DirectionalShadowPlazaSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated directional-shadow plaza asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.DirectionalShadowPlazaSceneId;

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
        /// Initializes the directional-shadow plaza scene factory with the persistence descriptors required for authored output.
        /// </summary>
        public DirectionalShadowPlazaSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical directional-shadow plaza scene asset.
        /// </summary>
        /// <param name="planeReference">Stable generated plane model reference.</param>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <param name="sphereReference">Stable generated sphere model reference.</param>
        /// <param name="standardMaterialReference">Stable generated standard material reference.</param>
        /// <returns>Authored scene asset for the directional-shadow plaza showcase.</returns>
        public SceneAsset CreateSceneAsset(
            SceneAssetReference planeReference,
            SceneAssetReference cubeReference,
            SceneAssetReference sphereReference,
            SceneAssetReference standardMaterialReference) {
            if (planeReference == null) {
                throw new ArgumentNullException(nameof(planeReference));
            } else if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            } else if (sphereReference == null) {
                throw new ArgumentNullException(nameof(sphereReference));
            } else if (standardMaterialReference == null) {
                throw new ArgumentNullException(nameof(standardMaterialReference));
            }

            return new SceneAsset {
                Id = SceneId,
                AssetReferences = new[] {
                    planeReference,
                    cubeReference,
                    sphereReference,
                    standardMaterialReference,
                    DemoDiscSceneComponentRecordFactory.CreateEditorFontReference()
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateGroundEntity(planeReference, standardMaterialReference),
                    CreateShadowMastEntity(cubeReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-tower-left", "DirectionalShadowPlazaWestTower", new float3(-16f, 7f, -9f), new float3(6f, 14f, 6f), cubeReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-tower-center", "DirectionalShadowPlazaCentralTower", new float3(0f, 9f, -12f), new float3(7f, 18f, 7f), cubeReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-tower-right", "DirectionalShadowPlazaEastTower", new float3(15f, 6f, -7f), new float3(5f, 12f, 5f), cubeReference, standardMaterialReference),
                    CreateOrbitHeroEntity(sphereReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-receiver-a", "DirectionalShadowPlazaSouthwestBlock", new float3(-15f, 3f, 12f), new float3(6f, 6f, 6f), cubeReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-receiver-b", "DirectionalShadowPlazaSouthCentralBlock", new float3(-4f, 2.5f, 14f), new float3(5f, 5f, 5f), cubeReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-receiver-c", "DirectionalShadowPlazaNortheastBlock", new float3(13f, 2f, 11f), new float3(4f, 4f, 4f), cubeReference, standardMaterialReference),
                    CreateBuildingEntity("directional-shadow-plaza-receiver-d", "DirectionalShadowPlazaMidriseBlock", new float3(8f, 3.5f, 2f), new float3(5f, 7f, 5f), cubeReference, standardMaterialReference)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the showcase scene.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.28f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "directional-shadow-plaza-camera",
                Name = "DirectionalShadowPlazaCamera",
                LocalPosition = new float3(0f, 24f, 64f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    RenderingScriptComponentRecordFactory.CreateCameraOrbitRecord(1, new float3(0f, 0f, 0f), 64f, 24f, 0f, 0.07f, -0.28f),
                    DemoDiscSceneComponentRecordFactory.CreateFpsComponentRecord(2),
                    DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(3)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored directional light entity for the showcase scene.
        /// </summary>
        /// <returns>Serialized directional light entity.</returns>
        SceneEntityAsset CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.72f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "directional-shadow-plaza-sun",
                Name = "DirectionalShadowPlazaSun",
                LocalPosition = new float3(0f, 18f, 0f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateDirectionalLightComponentRecord(1f, 80f),
                    RenderingScriptComponentRecordFactory.CreateSunSweepRecord(1, -0.18f, 0.18f, -0.72f, 0.05f)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one tall, narrow occluder that throws a clear directional shadow across the plaza.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized shadow mast entity.</returns>
        SceneEntityAsset CreateShadowMastEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = "directional-shadow-plaza-shadow-mast",
                Name = "DirectionalShadowPlazaShadowMast",
                LocalPosition = new float3(-9f, 7f, 4f),
                LocalScale = new float3(1.4f, 14f, 1.4f),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReference)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one static building entity for the city-block composition.
        /// </summary>
        /// <param name="id">Stable entity id.</param>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized building entity.</returns>
        SceneEntityAsset CreateBuildingEntity(
            string id,
            string name,
            float3 localPosition,
            float3 localScale,
            SceneAssetReference modelReference,
            SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = id,
                Name = name,
                LocalPosition = localPosition,
                LocalScale = localScale,
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReference)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the orbiting sphere landmark for the showcase scene.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized orbiting sphere entity.</returns>
        SceneEntityAsset CreateOrbitHeroEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = "directional-shadow-plaza-hero",
                Name = "DirectionalShadowPlazaHeroSphere",
                LocalPosition = new float3(0f, 2.5f, 10f),
                LocalScale = new float3(3f, 3f, 3f),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReference),
                    RenderingScriptComponentRecordFactory.CreateOrbitRecord(1, new float3(0f, 0f, 0f), 10f, 2.5f, 0.15f, -0.18f)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the ground receiver mesh for the showcase scene.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized ground entity.</returns>
        SceneEntityAsset CreateGroundEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = "directional-shadow-plaza-ground",
                Name = "DirectionalShadowPlazaGround",
                LocalPosition = new float3(0f, 0f, 0f),
                LocalScale = new float3(48f, 1f, 48f),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReference)
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
            writer.WriteField("FarPlaneDistance", fieldWriter => fieldWriter.WriteSingle(200f));
            writer.WriteField(
                "ClearSettings",
                fieldWriter => SceneComponentBinaryFieldEncoding.WriteCameraClearSettings(
                    fieldWriter,
                    new CameraClearSettings(
                        true,
                        new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
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
                        ShadowDistance = 80f,
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
                Color = new float4(1f, 0.96f, 0.90f, 1f),
                Intensity = intensity,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = shadowDistance
            };
            return DirectionalLightDescriptor.SerializeComponent(lightComponent, 0, null).Payload;
        }
    }
}
