using System.Globalization;
using System.Text;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the authored axis-test-2 scene and its generated file-backed assets.
    /// </summary>
    public sealed class AxisTest2SceneFactory {
        /// <summary>
        /// Stable scene id used by the generated axis-test-2 asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.AxisTest2SceneId;

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
        /// Stable material importer identifier stored on generated sidecar settings.
        /// </summary>
        const string MaterialImporterId = "helengine.material";

        /// <summary>
        /// Stable Windows standard-material schema identifier used by the shared editor material pipeline.
        /// </summary>
        const string WindowsMaterialSchemaId = "standard-shader";

        /// <summary>
        /// Stable PS2 lit material schema identifier used by the PS2 runtime path.
        /// </summary>
        const string Ps2MaterialSchemaId = "ps2-simple-lit-textured";

        /// <summary>
        /// Stable standard shader asset identifier used by compatibility material payloads.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable standard shader vertex program used by compatibility material payloads.
        /// </summary>
        const string StandardVertexProgramName = "ForwardStandardShader.vs";

        /// <summary>
        /// Stable standard shader pixel program used by compatibility material payloads.
        /// </summary>
        const string StandardPixelProgramName = "ForwardStandardShader.ps";

        /// <summary>
        /// Stable mesh variant used by compatibility material payloads.
        /// </summary>
        const string MeshVariantName = "Mesh";

        /// <summary>
        /// Stable material field identifier used to opt into standard-shader defaults.
        /// </summary>
        const string UseCustomShaderFieldId = "use-custom-shader";

        /// <summary>
        /// Stable material field identifier used for authored texture bindings.
        /// </summary>
        const string TextureIdFieldId = "texture-id";

        /// <summary>
        /// Stable material field identifier used for shadow-casting participation.
        /// </summary>
        const string CastsShadowFieldId = "casts-shadow";

        /// <summary>
        /// Stable PS2 material field identifier used for shadow-casting participation.
        /// </summary>
        const string Ps2CastShadowsFieldId = "cast-shadows";

        /// <summary>
        /// Stable material field identifier used for shadow receiving.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Stable material field identifier used for authored base color.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Stable PS2 material field identifier used for alpha mode.
        /// </summary>
        const string AlphaModeFieldId = "alpha-mode";

        /// <summary>
        /// Stable PS2 material field identifier used for double-sided control.
        /// </summary>
        const string DoubleSidedFieldId = "double-sided";

        /// <summary>
        /// Stable PS2 material field identifier used for vertex-color control.
        /// </summary>
        const string VertexColorModeFieldId = "vertex-color-mode";

        /// <summary>
        /// Layer mask used by authored scene objects in packaged runtime scenes.
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
        /// Relative project folder used for the generated axis-test materials.
        /// </summary>
        const string MaterialRootRelativePath = "materials/rendering/axis_test";

        /// <summary>
        /// Relative project path used by the generated directional-light arrow model source.
        /// </summary>
        const string ArrowModelRelativePath = "models/rendering/axis_test/directional_light_arrow.obj";

        /// <summary>
        /// Radius of the generated directional-light arrow shaft.
        /// </summary>
        const float ArrowShaftRadius = 0.05f;

        /// <summary>
        /// Length of the generated directional-light arrow shaft.
        /// </summary>
        const float ArrowShaftLength = 0.58f;

        /// <summary>
        /// Radius of the generated directional-light arrow head.
        /// </summary>
        const float ArrowHeadRadius = 0.18f;

        /// <summary>
        /// Length of the generated directional-light arrow head.
        /// </summary>
        const float ArrowHeadLength = 0.28f;

        /// <summary>
        /// Segment count used for the generated directional-light arrow round details.
        /// </summary>
        const int ArrowRoundSegments = 18;

        /// <summary>
        /// Uniform scale applied to the generated directional-light arrow so it remains readable from the authored camera.
        /// </summary>
        const float ArrowVisualScale = 8f;

        /// <summary>
        /// Angular speed applied to the directional-light arrow sweep in radians per second.
        /// </summary>
        const float ArrowAngularSpeedRadians = (float)(-Math.PI / 8.0);

        /// <summary>
        /// Authored colors used by the axis shafts, neutral ground, and lighting markers.
        /// </summary>
        static readonly string[] AxisMaterialColors = {
            "#FF4040FF",
            "#40FF40FF",
            "#4080FFFF",
            "#B8C2CCFF",
            "#FFFFFFFF"
        };

        /// <summary>
        /// Stable authored material paths used by the axis-test scene.
        /// </summary>
        static readonly string[] AxisMaterialRelativePaths = BuildMaterialRelativePaths();

        /// <summary>
        /// World-space position used to keep the directional-light arrow centered in the authored camera view.
        /// </summary>
        static readonly float3 ArrowRigLocalPosition = new float3(5f, 6f, 5f);

        /// <summary>
        /// Descriptor used to serialize authored mesh payloads for committed editor scenes.
        /// </summary>
        readonly MeshComponentPersistenceDescriptor MeshDescriptor;

        /// <summary>
        /// Descriptor used to serialize authored directional-light payloads for committed editor scenes.
        /// </summary>
        readonly DirectionalLightComponentPersistenceDescriptor DirectionalLightDescriptor;

        /// <summary>
        /// Service used to write generated material settings sidecars.
        /// </summary>
        readonly MaterialAssetSettingsService MaterialSettingsService;

        /// <summary>
        /// Placeholder runtime model used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly AuthoringPlaceholderRuntimeModel PlaceholderModel;

        /// <summary>
        /// Placeholder runtime material used only to satisfy authored mesh serialization before stable asset references are applied.
        /// </summary>
        readonly RuntimeMaterial PlaceholderMaterial;

        /// <summary>
        /// Initializes the axis-test scene factory with the descriptors and services required for authored output.
        /// </summary>
        public AxisTest2SceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            MaterialSettingsService = new MaterialAssetSettingsService();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical axis-test-2 scene asset.
        /// </summary>
        /// <param name="cubeReference">Stable generated cube model reference.</param>
        /// <returns>Authored scene asset for the camera-forward-axis directional-light validation test.</returns>
        public SceneAsset CreateSceneAsset(SceneAssetReference cubeReference) {
            if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            }

            SceneAssetReference arrowModelReference = CreateArrowModelReference();
            SceneAssetReference markerMaterialReference = CreateAxisMaterialReference(4);

            return new SceneAsset {
                Id = "axis_test2",
                AssetReferences = new[] {
                    cubeReference,
                    arrowModelReference,
                    CreateAxisMaterialReference(0),
                    CreateAxisMaterialReference(1),
                    CreateAxisMaterialReference(2),
                    CreateAxisMaterialReference(3),
                    markerMaterialReference,
                    DemoDiscSceneComponentRecordFactory.CreateEditorFontReference()
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightRigEntity(arrowModelReference, markerMaterialReference),
                    CreateFloorEntity(cubeReference, CreateAxisMaterialReference(3)),
                    CreateGroundEntity(cubeReference, CreateAxisMaterialReference(3)),
                    CreateXAxisEntity(cubeReference, CreateAxisMaterialReference(0)),
                    CreateYAxisEntity(cubeReference, CreateAxisMaterialReference(1)),
                    CreateZAxisEntity(cubeReference, CreateAxisMaterialReference(2)),
                    CreateOriginMarkerEntity(cubeReference, markerMaterialReference),
                    CreateXAxisMarkerEntity(cubeReference, markerMaterialReference),
                    CreateYAxisMarkerEntity(cubeReference, markerMaterialReference),
                    CreateZAxisMarkerEntity(cubeReference, markerMaterialReference)
                }
            };
        }

        /// <summary>
        /// Writes the generated file-backed assets used by the axis-test-2 scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            for (int materialIndex = 0; materialIndex < AxisMaterialRelativePaths.Length; materialIndex++) {
                WriteMaterialAsset(projectRootPath, materialIndex);
            }

            WriteArrowModelSource(projectRootPath);
        }

        /// <summary>
        /// Creates the authored camera entity for the axis-test-2 scene.
        /// </summary>
        /// <returns>Serialized camera entity.</returns>
        SceneEntityAsset CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)(Math.PI * 0.5), 0f, 0f, out orientation);

            return new SceneEntityAsset {
                Id = "axis-test-2-camera",
                Name = "AxisTest2Camera",
                LocalPosition = new float3(30f, 6f, 5f),
                LocalScale = float3.One,
                LocalOrientation = orientation,
                Components = new[] {
                    CreateCameraComponentRecord(),
                    DemoDiscSceneComponentRecordFactory.CreateFpsComponentRecord(1),
                    DemoDiscSceneComponentRecordFactory.CreateReturnToMainMenuRecord(2)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored directional-light rig root that rotates around the camera forward axis.
        /// </summary>
        /// <param name="modelReference">Stable generated directional-light arrow model reference.</param>
        /// <param name="materialReference">Stable file-backed marker material reference used by the arrow mesh.</param>
        /// <returns>Serialized directional-light rig root entity.</returns>
        SceneEntityAsset CreateDirectionalLightRigEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            } else if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = "axis-test-2-sun-rig",
                Name = "AxisTest2SunRig",
                LocalPosition = ArrowRigLocalPosition,
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = new[] {
                    RenderingScriptComponentRecordFactory.CreateAxisTestCameraForwardSpinRecord(0, 0f, ArrowAngularSpeedRadians, -1f, 0f, 0f)
                },
                Children = new[] {
                    CreateDirectionalLightArrowEntity(modelReference, materialReference)
                }
            };
        }

        /// <summary>
        /// Creates the authored directional-light arrow entity that carries the visible mesh and the real light component.
        /// </summary>
        /// <param name="modelReference">Stable generated directional-light arrow model reference.</param>
        /// <param name="materialReference">Stable file-backed marker material reference used by the arrow mesh.</param>
        /// <returns>Serialized directional-light arrow entity.</returns>
        SceneEntityAsset CreateDirectionalLightArrowEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            if (modelReference == null) {
                throw new ArgumentNullException(nameof(modelReference));
            } else if (materialReference == null) {
                throw new ArgumentNullException(nameof(materialReference));
            }

            return new SceneEntityAsset {
                Id = "axis-test-2-sun-arrow",
                Name = "AxisTest2SunArrow",
                LocalPosition = float3.Zero,
                LocalScale = new float3(ArrowVisualScale, ArrowVisualScale, ArrowVisualScale),
                LocalOrientation = float4.Identity,
                Components = new[] {
                    CreateMeshComponentRecord(0, modelReference, materialReference),
                    CreateDirectionalLightComponentRecord(1, 1.2f, 32f)
                },
                Children = Array.Empty<SceneEntityAsset>()
            };
        }

        /// <summary>
        /// Creates the authored X-axis mesh entity.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed red material reference.</param>
        /// <returns>Serialized X-axis entity.</returns>
        SceneEntityAsset CreateXAxisEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-x-axis",
                "AxisTestXAxis",
                new float3(5f, 0f, 0f),
                new float3(10f, 0.5f, 0.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored Y-axis mesh entity.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed green material reference.</param>
        /// <returns>Serialized Y-axis entity.</returns>
        SceneEntityAsset CreateYAxisEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-y-axis",
                "AxisTestYAxis",
                new float3(0f, 5f, 0f),
                new float3(0.5f, 10f, 0.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored Z-axis mesh entity.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed blue material reference.</param>
        /// <returns>Serialized Z-axis entity.</returns>
        SceneEntityAsset CreateZAxisEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-z-axis",
                "AxisTestZAxis",
                new float3(0f, 0f, 5f),
                new float3(0.5f, 0.5f, 10f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored neutral floor entity used to make directional lighting readable.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed ground material reference.</param>
        /// <returns>Serialized floor entity.</returns>
        SceneEntityAsset CreateFloorEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-floor",
                "AxisTestFloor",
                new float3(5f, -5f, 5f),
                new float3(14f, 0.5f, 14f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored neutral wall entity used to read light direction against one large surface from the new camera position.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed ground material reference.</param>
        /// <returns>Serialized wall entity.</returns>
        SceneEntityAsset CreateGroundEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-2-ground",
                "AxisTest2Ground",
                new float3(16f, 5f, -6f),
                new float3(14f, 12f, 0.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the world origin.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized origin marker entity.</returns>
        SceneEntityAsset CreateOriginMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-origin-marker",
                "AxisTestOriginMarker",
                float3.Zero,
                new float3(1.25f, 1.25f, 1.25f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the positive X-axis endpoint.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized positive X marker entity.</returns>
        SceneEntityAsset CreateXAxisMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-x-marker",
                "AxisTestXMarker",
                new float3(10f, 0f, 0f),
                new float3(1.5f, 1.5f, 1.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the positive Y-axis endpoint.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized positive Y marker entity.</returns>
        SceneEntityAsset CreateYAxisMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-y-marker",
                "AxisTestYMarker",
                new float3(0f, 10f, 0f),
                new float3(1.5f, 1.5f, 1.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates the authored white cube placed at the positive Z-axis endpoint.
        /// </summary>
        /// <param name="modelReference">Stable generated cube model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed marker material reference.</param>
        /// <returns>Serialized positive Z marker entity.</returns>
        SceneEntityAsset CreateZAxisMarkerEntity(SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return CreateAxisEntity(
                "axis-test-z-marker",
                "AxisTestZMarker",
                new float3(0f, 0f, 10f),
                new float3(1.5f, 1.5f, 1.5f),
                modelReference,
                materialReference);
        }

        /// <summary>
        /// Creates one axis mesh entity with the supplied transform.
        /// </summary>
        /// <param name="id">Stable entity id.</param>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable file-backed material reference used by the mesh payload.</param>
        /// <returns>Serialized axis mesh entity.</returns>
        SceneEntityAsset CreateAxisEntity(
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
            return CreateMeshComponentRecord(0, modelReference, materialReference);
        }

        /// <summary>
        /// Creates one serialized mesh component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="modelReference">Stable generated model reference used by the mesh payload.</param>
        /// <param name="materialReference">Stable generated material reference used by the mesh payload.</param>
        /// <returns>Serialized mesh component record.</returns>
        SceneComponentAssetRecord CreateMeshComponentRecord(int componentIndex, SceneAssetReference modelReference, SceneAssetReference materialReference) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = MeshComponentTypeId,
                ComponentIndex = componentIndex,
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
            return CreateDirectionalLightComponentRecord(0, intensity, shadowDistance);
        }

        /// <summary>
        /// Creates one serialized directional-light component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="intensity">Authored directional-light intensity.</param>
        /// <param name="shadowDistance">Authored directional-light shadow cutoff distance.</param>
        /// <returns>Serialized directional-light component record.</returns>
        SceneComponentAssetRecord CreateDirectionalLightComponentRecord(int componentIndex, float intensity, float shadowDistance) {
            return new SceneComponentAssetRecord {
                ComponentTypeId = DirectionalLightComponentTypeId,
                ComponentIndex = componentIndex,
                Payload = WriteDirectionalLightPayload(intensity, shadowDistance)
            };
        }

        /// <summary>
        /// Creates one stable file-backed material reference for the supplied axis-test material index.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>Scene asset reference targeting one file-backed axis-test material.</returns>
        SceneAssetReference CreateAxisMaterialReference(int materialIndex) {
            if (materialIndex < 0 || materialIndex >= AxisMaterialRelativePaths.Length) {
                throw new ArgumentOutOfRangeException(nameof(materialIndex), "Axis-test material index must address one generated material.");
            }

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = AxisMaterialRelativePaths[materialIndex],
                ProviderId = string.Empty,
                AssetId = string.Empty
            };
        }

        /// <summary>
        /// Creates one stable file-backed model reference for the generated directional-light arrow source.
        /// </summary>
        /// <returns>Scene asset reference targeting the generated directional-light arrow source file.</returns>
        SceneAssetReference CreateArrowModelReference() {
            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = ArrowModelRelativePath,
                ProviderId = string.Empty,
                AssetId = string.Empty
            };
        }

        /// <summary>
        /// Writes one file-backed axis-test material asset and its settings sidecar.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        void WriteMaterialAsset(string projectRootPath, int materialIndex) {
            string relativePath = AxisMaterialRelativePaths[materialIndex];
            string fullPath = Path.Combine(projectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a material directory for '{relativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);

            using (FileStream stream = File.Create(fullPath)) {
                global::helengine.editor.AssetSerializer.Serialize(stream, CreateMaterialAsset(materialIndex));
            }

            MaterialSettingsService.Save(fullPath, CreateMaterialSettings(materialIndex));
        }

        /// <summary>
        /// Writes the generated directional-light arrow model source used by the axis-test light rig.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        void WriteArrowModelSource(string projectRootPath) {
            string fullPath = Path.Combine(projectRootPath, "assets", ArrowModelRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a model directory for '{ArrowModelRelativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(fullPath, BuildArrowModelSource());
        }

        /// <summary>
        /// Creates one compatibility material asset that resolves to the shared standard shader.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>File-backed material asset for the supplied axis-test material.</returns>
        MaterialAsset CreateMaterialAsset(int materialIndex) {
            return new MaterialAsset {
                Id = CreateMaterialAssetId(materialIndex),
                ShaderAssetId = StandardShaderAssetId,
                VertexProgram = StandardVertexProgramName,
                PixelProgram = StandardPixelProgramName,
                Variant = MeshVariantName,
                RenderState = new MaterialRenderState(),
                ConstantBuffers = Array.Empty<MaterialConstantBufferAsset>(),
                CastsShadows = true,
                ReceivesShadows = true
            };
        }

        /// <summary>
        /// Creates one per-platform settings sidecar for the supplied axis-test material.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>Generated import-settings payload for the axis-test material.</returns>
        MaterialAssetImportSettings CreateMaterialSettings(int materialIndex) {
            MaterialAssetImportSettings settings = new MaterialAssetImportSettings();
            settings.Importer.ImporterId = MaterialImporterId;
            settings.Importer.SourceChecksum = string.Empty;
            settings.Importer.AssetId = CreateMaterialAssetId(materialIndex);

            string baseColor = AxisMaterialColors[materialIndex];

            MaterialAssetProcessorSettings windowsSettings = new MaterialAssetProcessorSettings();
            windowsSettings.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.FieldValues[UseCustomShaderFieldId] = "false";
            windowsSettings.FieldValues[TextureIdFieldId] = string.Empty;
            windowsSettings.FieldValues[CastsShadowFieldId] = "true";
            windowsSettings.FieldValues[ReceivesShadowFieldId] = "true";
            windowsSettings.FieldValues[BaseColorFieldId] = baseColor;
            settings.Processor.Platforms["windows"] = windowsSettings;

            MaterialAssetProcessorSettings ps2Settings = new MaterialAssetProcessorSettings();
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.FieldValues[AlphaModeFieldId] = "opaque";
            ps2Settings.FieldValues[DoubleSidedFieldId] = "false";
            ps2Settings.FieldValues[Ps2CastShadowsFieldId] = "true";
            ps2Settings.FieldValues[VertexColorModeFieldId] = "ignore";
            ps2Settings.FieldValues[BaseColorFieldId] = baseColor;
            settings.Processor.Platforms["ps2"] = ps2Settings;

            MaterialAssetProcessorSettings pspSettings = new MaterialAssetProcessorSettings();
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.FieldValues[UseCustomShaderFieldId] = "false";
            pspSettings.FieldValues[TextureIdFieldId] = string.Empty;
            pspSettings.FieldValues[CastsShadowFieldId] = "true";
            pspSettings.FieldValues[ReceivesShadowFieldId] = "true";
            pspSettings.FieldValues[BaseColorFieldId] = baseColor;
            settings.Processor.Platforms["psp"] = pspSettings;
            return settings;
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
                        ShadowDistance = 32f,
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
                Color = new float4(1f, 0.95f, 0.9f, 1f),
                Intensity = intensity,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = shadowDistance
            };
            return DirectionalLightDescriptor.SerializeComponent(lightComponent, 0, null).Payload;
        }

        /// <summary>
        /// Builds the wavefront OBJ source used by the directional-light arrow mesh.
        /// </summary>
        /// <returns>Wavefront OBJ text for the generated directional-light arrow.</returns>
        string BuildArrowModelSource() {
            ModelAsset arrowModel = CreateArrowModelAsset();
            if (arrowModel.Indices16 == null || arrowModel.Indices16.Length == 0) {
                throw new InvalidOperationException("Directional-light arrow generation produced no triangle indices.");
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Generated axis-test directional-light arrow");
            builder.AppendLine("o directional_light_arrow");

            for (int vertexIndex = 0; vertexIndex < arrowModel.Positions.Length; vertexIndex++) {
                builder.Append("v ");
                AppendInvariantFloat(builder, arrowModel.Positions[vertexIndex].X);
                builder.Append(' ');
                AppendInvariantFloat(builder, arrowModel.Positions[vertexIndex].Y);
                builder.Append(' ');
                AppendInvariantFloat(builder, arrowModel.Positions[vertexIndex].Z);
                builder.AppendLine();
            }

            for (int texCoordIndex = 0; texCoordIndex < arrowModel.TexCoords.Length; texCoordIndex++) {
                builder.Append("vt ");
                AppendInvariantFloat(builder, arrowModel.TexCoords[texCoordIndex].X);
                builder.Append(' ');
                AppendInvariantFloat(builder, arrowModel.TexCoords[texCoordIndex].Y);
                builder.AppendLine();
            }

            for (int normalIndex = 0; normalIndex < arrowModel.Normals.Length; normalIndex++) {
                builder.Append("vn ");
                AppendInvariantFloat(builder, arrowModel.Normals[normalIndex].X);
                builder.Append(' ');
                AppendInvariantFloat(builder, arrowModel.Normals[normalIndex].Y);
                builder.Append(' ');
                AppendInvariantFloat(builder, arrowModel.Normals[normalIndex].Z);
                builder.AppendLine();
            }

            for (int index = 0; index < arrowModel.Indices16.Length; index += 3) {
                builder.Append("f ");
                AppendObjFaceVertex(builder, arrowModel.Indices16[index]);
                builder.Append(' ');
                AppendObjFaceVertex(builder, arrowModel.Indices16[index + 1]);
                builder.Append(' ');
                AppendObjFaceVertex(builder, arrowModel.Indices16[index + 2]);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        /// <summary>
        /// Builds one combined model asset that matches the editor directional-light icon orientation.
        /// </summary>
        /// <returns>Combined directional-light arrow model asset.</returns>
        ModelAsset CreateArrowModelAsset() {
            List<float3> positions = new List<float3>();
            List<float3> normals = new List<float3>();
            List<float2> texCoords = new List<float2>();
            List<ushort> indices = new List<ushort>();

            float4 forwardOrientation = CreateNegativeZAxisOrientation();
            AppendModelAsset(
                positions,
                normals,
                texCoords,
                indices,
                TransformGizmoMeshFactory.CreateCylinder(ArrowShaftRadius, ArrowShaftLength, ArrowRoundSegments),
                forwardOrientation,
                float3.Zero);
            AppendModelAsset(
                positions,
                normals,
                texCoords,
                indices,
                TransformGizmoMeshFactory.CreateCone(ArrowHeadRadius, ArrowHeadLength, ArrowRoundSegments),
                forwardOrientation,
                new float3(0f, 0f, -ArrowShaftLength));

            return new ModelAsset {
                Id = "Models.rendering.axis_test.directional_light_arrow",
                Positions = positions.ToArray(),
                Normals = normals.ToArray(),
                TexCoords = texCoords.ToArray(),
                Indices16 = indices.ToArray()
            };
        }

        /// <summary>
        /// Appends one source model asset into the supplied combined directional-light arrow mesh.
        /// </summary>
        /// <param name="positions">Destination position stream.</param>
        /// <param name="normals">Destination normal stream.</param>
        /// <param name="texCoords">Destination texture-coordinate stream.</param>
        /// <param name="indices">Destination 16-bit triangle-index stream.</param>
        /// <param name="source">Source model asset to append.</param>
        /// <param name="orientation">Orientation applied to positions and normals.</param>
        /// <param name="translation">Translation applied after rotation.</param>
        void AppendModelAsset(
            List<float3> positions,
            List<float3> normals,
            List<float2> texCoords,
            List<ushort> indices,
            ModelAsset source,
            float4 orientation,
            float3 translation) {
            if (positions == null) {
                throw new ArgumentNullException(nameof(positions));
            } else if (normals == null) {
                throw new ArgumentNullException(nameof(normals));
            } else if (texCoords == null) {
                throw new ArgumentNullException(nameof(texCoords));
            } else if (indices == null) {
                throw new ArgumentNullException(nameof(indices));
            } else if (source == null) {
                throw new ArgumentNullException(nameof(source));
            } else if (source.Positions == null || source.Normals == null || source.TexCoords == null || source.Indices16 == null) {
                throw new InvalidOperationException("Directional-light arrow generation requires complete 16-bit mesh data.");
            }

            int vertexOffset = positions.Count;
            if (vertexOffset > ushort.MaxValue) {
                throw new InvalidOperationException("Directional-light arrow vertex count exceeds 16-bit index capacity.");
            }

            for (int vertexIndex = 0; vertexIndex < source.Positions.Length; vertexIndex++) {
                positions.Add(float4.RotateVector(source.Positions[vertexIndex], orientation) + translation);
                normals.Add(float4.RotateVector(source.Normals[vertexIndex], orientation));
                texCoords.Add(source.TexCoords[vertexIndex]);
            }

            for (int index = 0; index < source.Indices16.Length; index++) {
                int combinedIndex = source.Indices16[index] + vertexOffset;
                if (combinedIndex > ushort.MaxValue) {
                    throw new InvalidOperationException("Directional-light arrow index exceeds 16-bit capacity.");
                }

                indices.Add((ushort)combinedIndex);
            }
        }

        /// <summary>
        /// Creates the fixed child orientation that points the authored light arrow upward in camera space.
        /// </summary>
        /// <returns>Quaternion rotating local -Z into world +Y.</returns>
        float4 CreateArrowFacingUpOrientation() {
            float3 xAxis = new float3(1f, 0f, 0f);
            float4 orientation;
            float4.CreateFromAxisAngle(ref xAxis, (float)(Math.PI * 0.5), out orientation);
            return orientation;
        }

        /// <summary>
        /// Creates the rotation that maps +Y-aligned primitive meshes into the local -Z forward axis.
        /// </summary>
        /// <returns>Quaternion rotating +Y into -Z.</returns>
        float4 CreateNegativeZAxisOrientation() {
            float3 xAxis = new float3(1f, 0f, 0f);
            float4 orientation;
            float4.CreateFromAxisAngle(ref xAxis, (float)(-Math.PI * 0.5), out orientation);
            return orientation;
        }

        /// <summary>
        /// Appends one float to the supplied OBJ builder using invariant formatting.
        /// </summary>
        /// <param name="builder">Destination text builder.</param>
        /// <param name="value">Float value to append.</param>
        void AppendInvariantFloat(StringBuilder builder, float value) {
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Append(value.ToString("G9", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Appends one OBJ face vertex reference using the shared position, texture-coordinate, and normal index.
        /// </summary>
        /// <param name="builder">Destination text builder.</param>
        /// <param name="zeroBasedIndex">Zero-based mesh vertex index.</param>
        void AppendObjFaceVertex(StringBuilder builder, ushort zeroBasedIndex) {
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }

            int oneBasedIndex = zeroBasedIndex + 1;
            builder.Append(oneBasedIndex);
            builder.Append('/');
            builder.Append(oneBasedIndex);
            builder.Append('/');
            builder.Append(oneBasedIndex);
        }

        /// <summary>
        /// Builds the stable material relative paths used by the axis-test scene.
        /// </summary>
        /// <returns>Stable project-relative material paths.</returns>
        static string[] BuildMaterialRelativePaths() {
            return new[] {
                MaterialRootRelativePath + "/X.helmat",
                MaterialRootRelativePath + "/Y.helmat",
                MaterialRootRelativePath + "/Z.helmat",
                MaterialRootRelativePath + "/Ground.helmat",
                MaterialRootRelativePath + "/Marker.helmat"
            };
        }

        /// <summary>
        /// Creates one stable material asset id for the supplied axis-test material index.
        /// </summary>
        /// <param name="materialIndex">Stable zero-based axis-test material index.</param>
        /// <returns>Material asset id stored inside the serialized file-backed asset.</returns>
        static string CreateMaterialAssetId(int materialIndex) {
            switch (materialIndex) {
                case 0:
                    return "Materials.rendering.axis_test.X";
                case 1:
                    return "Materials.rendering.axis_test.Y";
                case 2:
                    return "Materials.rendering.axis_test.Z";
                case 3:
                    return "Materials.rendering.axis_test.Ground";
                case 4:
                    return "Materials.rendering.axis_test.Marker";
                default:
                    throw new ArgumentOutOfRangeException(nameof(materialIndex), "Axis-test material index must be between zero and four.");
            }
        }
    }
}
