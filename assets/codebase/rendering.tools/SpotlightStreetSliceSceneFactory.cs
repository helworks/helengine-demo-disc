using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical authored scene asset for the spotlight street-slice showcase.
    /// </summary>
    public sealed class SpotlightStreetSliceSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated spotlight street-slice asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.SpotlightStreetSliceSceneId;

        /// <summary>
        /// Stable serialized component identifier used by mesh records.
        /// </summary>
        const string MeshComponentTypeId = "helengine.MeshComponent";

        /// <summary>
        /// Stable serialized component identifier used by camera records.
        /// </summary>
        const string CameraComponentTypeId = "helengine.CameraComponent";

        /// <summary>
        /// Stable serialized component identifier used by spotlight records.
        /// </summary>
        const string SpotLightComponentTypeId = "helengine.SpotLightComponent";

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
        /// Descriptor used to serialize authored spotlight payloads for committed editor scenes.
        /// </summary>
        readonly SpotLightComponentPersistenceDescriptor SpotLightDescriptor;

        /// <summary>
        /// Placeholder runtime model used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly AuthoringPlaceholderRuntimeModel PlaceholderModel;

        /// <summary>
        /// Placeholder runtime material used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly RuntimeMaterial PlaceholderMaterial;

        /// <summary>
        /// Initializes the spotlight street-slice scene factory with the persistence descriptors required for authored output.
        /// </summary>
        public SpotlightStreetSliceSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            SpotLightDescriptor = new SpotLightComponentPersistenceDescriptor();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical spotlight street-slice scene asset.
        /// </summary>
        /// <param name="planeReference">Stable generated plane model reference.</param>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <param name="standardMaterialReference">Stable generated standard material reference.</param>
        /// <param name="lamppostReference">Stable file-backed lamppost model reference.</param>
        /// <param name="racerReference">Stable file-backed racer model reference.</param>
        /// <param name="racerMaterialReferences">Stable file-backed racer material references ordered by imported submesh slot.</param>
        /// <returns>Authored scene asset for the spotlight street-slice showcase.</returns>
        public SceneAsset CreateSceneAsset(
            SceneAssetReference planeReference,
            SceneAssetReference cubeReference,
            SceneAssetReference standardMaterialReference,
            SceneAssetReference lamppostReference,
            SceneAssetReference racerReference,
            SceneAssetReference[] racerMaterialReferences) {
            if (planeReference == null) {
                throw new ArgumentNullException(nameof(planeReference));
            } else if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            } else if (standardMaterialReference == null) {
                throw new ArgumentNullException(nameof(standardMaterialReference));
            } else if (lamppostReference == null) {
                throw new ArgumentNullException(nameof(lamppostReference));
            } else if (racerReference == null) {
                throw new ArgumentNullException(nameof(racerReference));
            } else if (racerMaterialReferences == null) {
                throw new ArgumentNullException(nameof(racerMaterialReferences));
            }

            return new SceneAsset {
                Id = SceneId,
                AssetReferences = new[] {
                    planeReference,
                    cubeReference,
                    standardMaterialReference,
                    lamppostReference,
                    racerReference
                }.Concat(racerMaterialReferences).ToArray(),
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateSpotLightEntity(),
                    CreateStreetEntity(planeReference, standardMaterialReference),
                    CreateStreetEdgeEntity("spotlight-street-slice-curb-left", "SpotlightStreetSliceCurbLeft", new float3(-9f, 0.25f, 0f), new float3(1f, 0.5f, 28f), cubeReference, standardMaterialReference),
                    CreateStreetEdgeEntity("spotlight-street-slice-curb-right", "SpotlightStreetSliceCurbRight", new float3(9f, 0.25f, 0f), new float3(1f, 0.5f, 28f), cubeReference, standardMaterialReference),
                    CreateStreetEdgeEntity("spotlight-street-slice-back-wall", "SpotlightStreetSliceBackWall", new float3(0f, 6f, -12f), new float3(20f, 12f, 1f), cubeReference, standardMaterialReference),
                    CreateStreetEdgeEntity("spotlight-street-slice-side-block", "SpotlightStreetSliceSideBlock", new float3(12f, 2.5f, 6f), new float3(4f, 5f, 8f), cubeReference, standardMaterialReference),
                    CreateImportedMeshEntity("spotlight-street-slice-lamppost", "SpotlightStreetSliceLamppost", new float3(-4f, 0f, -2f), new float3(2.2f, 2.2f, 2.2f), CreateYawOrientation(0.0), lamppostReference, new[] { standardMaterialReference }),
                    CreateImportedMeshEntity("spotlight-street-slice-racer", "SpotlightStreetSliceRacer", new float3(1.8f, 0f, 2f), new float3(2.8f, 2.8f, 2.8f), CreateYawOrientation(-0.42), racerReference, racerMaterialReferences)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the spotlight showcase.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.24f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "spotlight-street-slice-camera",
                Name = "SpotlightStreetSliceCamera",
                LocalPosition = new float3(0f, 12f, 28f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    RenderingScriptComponentRecordFactory.CreateCameraOrbitRecord(1, new float3(0f, 2f, 0f), 28f, 12f, 0f, 0.05f, -0.24f),
                    DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored spotlight entity for the showcase scene.
        /// </summary>
        /// <returns>Serialized spotlight entity.</returns>
        SceneEntityAsset CreateSpotLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0.28f, -1.22f, 0f, out orientation);
            return new SceneEntityAsset {
                Id = "spotlight-street-slice-light",
                Name = "SpotlightStreetSliceLight",
                LocalPosition = new float3(-3.2f, 9.5f, -1.4f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateSpotLightComponentRecord(34f, 22f, 35f, 1f)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the ground street slice for the spotlight showcase.
        /// </summary>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized ground entity.</returns>
        SceneEntityAsset CreateStreetEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneEntityAsset {
                Id = "spotlight-street-slice-street",
                Name = "SpotlightStreetSliceStreet",
                LocalPosition = new float3(0f, -0.05f, 0f),
                LocalScale = new float3(20f, 1f, 28f),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, new[] { materialReference })
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one supporting static street-edge mass for the spotlight scene.
        /// </summary>
        /// <param name="id">Stable entity id.</param>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized street-edge entity.</returns>
        SceneEntityAsset CreateStreetEdgeEntity(
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
                    CreateMeshComponentRecord(modelReference, new[] { materialReference })
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates one imported model entity that uses the shared standard material path.
        /// </summary>
        /// <param name="id">Stable entity id.</param>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="localOrientation">Local orientation assigned to the entity.</param>
        /// <param name="modelReference">Stable file-backed model reference.</param>
        /// <param name="materialReferences">Stable generated material references used by the mesh payload.</param>
        /// <returns>Serialized imported mesh entity.</returns>
        SceneEntityAsset CreateImportedMeshEntity(
            string id,
            string name,
            float3 localPosition,
            float3 localScale,
            float4 localOrientation,
            SceneAssetReference modelReference,
            SceneAssetReference[] materialReferences) {
            return new SceneEntityAsset {
                Id = id,
                Name = name,
                LocalPosition = localPosition,
                LocalScale = localScale,
                LocalOrientation = localOrientation,
                Components = new[] {
                    CreateMeshComponentRecord(modelReference, materialReferences)
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
        /// <param name="modelReference">Stable model reference used by the mesh payload.</param>
        /// <param name="materialReferences">Stable material references used by the mesh payload.</param>
        /// <returns>Serialized mesh component record.</returns>
        SceneComponentAssetRecord CreateMeshComponentRecord(SceneAssetReference modelReference, SceneAssetReference[] materialReferences) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = MeshComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteMeshPayload(modelReference, materialReferences)
            };
        }

        /// <summary>
        /// Creates one serialized spotlight component record.
        /// </summary>
        /// <param name="range">Authored spotlight range.</param>
        /// <param name="innerConeAngleDegrees">Authored inner cone angle.</param>
        /// <param name="outerConeAngleDegrees">Authored outer cone angle.</param>
        /// <param name="intensity">Authored spotlight intensity.</param>
        /// <returns>Serialized spotlight component record.</returns>
        SceneComponentAssetRecord CreateSpotLightComponentRecord(float range, float innerConeAngleDegrees, float outerConeAngleDegrees, float intensity) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = SpotLightComponentTypeId,
                ComponentIndex = 0,
                Payload = WriteSpotLightPayload(range, innerConeAngleDegrees, outerConeAngleDegrees, intensity)
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
                        new float4(0.015f, 0.015f, 0.03f, 1f),
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
        /// <param name="modelReference">Stable model reference used by the mesh.</param>
        /// <param name="materialReferences">Stable material references used by the mesh.</param>
        /// <returns>Serialized mesh component payload.</returns>
        byte[] WriteMeshPayload(SceneAssetReference modelReference, SceneAssetReference[] materialReferences) {
            if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            } else if (materialReferences == null) {
                throw new ArgumentNullException(nameof(materialReferences));
            }

            MeshComponent meshComponent = new MeshComponent {
                Model = PlaceholderModel,
                RenderOrder3D = 0
            };
            RuntimeMaterial[] placeholderMaterials = new RuntimeMaterial[materialReferences.Length];
            for (int materialIndex = 0; materialIndex < placeholderMaterials.Length; materialIndex++) {
                placeholderMaterials[materialIndex] = PlaceholderMaterial;
            }

            meshComponent.SetMaterials(placeholderMaterials);
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference(MeshModelReferenceName, modelReference);
            for (int materialIndex = 0; materialIndex < materialReferences.Length; materialIndex++) {
                SceneAssetReference materialReference = materialReferences[materialIndex];
                if (materialReference == null) {
                    throw new ArgumentNullException(nameof(materialReferences), "Imported model material references must not contain null entries.");
                }

                saveState.SetAssetReference(BuildMaterialReferenceName(materialIndex), materialReference);
            }

            return MeshDescriptor.SerializeComponent(meshComponent, 0, saveState).Payload;
        }

        /// <summary>
        /// Resolves one stable save-state material-reference name for the supplied slot index.
        /// </summary>
        /// <param name="slotIndex">Zero-based material slot index.</param>
        /// <returns>Stable save-state reference name.</returns>
        static string BuildMaterialReferenceName(int slotIndex) {
            if (slotIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(slotIndex), "Material slot index must be non-negative.");
            }

            return slotIndex == 0
                ? MeshMaterialReferenceName
                : string.Concat(MeshMaterialReferenceName, "[", slotIndex.ToString(), "]");
        }

        /// <summary>
        /// Writes one serialized spotlight component payload.
        /// </summary>
        /// <param name="range">Authored spotlight range.</param>
        /// <param name="innerConeAngleDegrees">Authored inner cone angle.</param>
        /// <param name="outerConeAngleDegrees">Authored outer cone angle.</param>
        /// <param name="intensity">Authored spotlight intensity.</param>
        /// <returns>Serialized spotlight component payload.</returns>
        byte[] WriteSpotLightPayload(float range, float innerConeAngleDegrees, float outerConeAngleDegrees, float intensity) {
            SpotLightComponent lightComponent = new SpotLightComponent {
                Color = new float4(1f, 0.95f, 0.84f, 1f),
                Intensity = intensity,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                Range = range,
                InnerConeAngleDegrees = innerConeAngleDegrees,
                OuterConeAngleDegrees = outerConeAngleDegrees
            };
            return SpotLightDescriptor.SerializeComponent(lightComponent, 0, null).Payload;
        }

        /// <summary>
        /// Creates one pure yaw orientation.
        /// </summary>
        /// <param name="yawRadians">Yaw angle in radians.</param>
        /// <returns>Quaternion containing only the requested yaw.</returns>
        float4 CreateYawOrientation(double yawRadians) {
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)yawRadians, 0f, 0f, out orientation);
            return orientation;
        }
    }
}
