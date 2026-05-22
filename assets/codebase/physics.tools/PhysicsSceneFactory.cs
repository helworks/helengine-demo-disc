namespace city.physics.tools {
    /// <summary>
    /// Creates exportable scene assets for physics validation and demo playback.
    /// </summary>
    public sealed class PhysicsSceneFactory {
        /// <summary>
        /// Stable generated provider identifier used for built-in primitive assets.
        /// </summary>
        const string GeneratedProviderId = EngineGeneratedAssetProvider.ProviderIdValue;

        /// <summary>
        /// Stable scene-asset source kind byte used for generated primitive references.
        /// </summary>
        const SceneAssetReferenceSourceKind GeneratedSourceKind = SceneAssetReferenceSourceKind.Generated;

        /// <summary>
        /// Stable render order assigned to generated debug geometry meshes.
        /// </summary>
        const byte DefaultMeshRenderOrder = 0;

        /// <summary>
        /// Stable camera draw order assigned to validation-scene cameras.
        /// </summary>
        const byte DefaultCameraDrawOrder = 0;

        /// <summary>
        /// File-system scene-asset source kind used for authored shader and material assets.
        /// </summary>
        const SceneAssetReferenceSourceKind FileSystemSourceKind = SceneAssetReferenceSourceKind.FileSystem;

        /// <summary>
        /// Relative project asset path for the shared physics demo mesh shader.
        /// </summary>
        const string PhysicsDemoShaderRelativePath = "Shaders/physics/PhysicsDemoMesh.hlsl";

        /// <summary>
        /// Relative project asset path for the neutral physics demo material.
        /// </summary>
        const string PhysicsDemoNeutralMaterialRelativePath = "Materials/physics/PhysicsDemoNeutral" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the blue physics demo material.
        /// </summary>
        const string PhysicsDemoBlueMaterialRelativePath = "Materials/physics/PhysicsDemoBlue" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the green physics demo material.
        /// </summary>
        const string PhysicsDemoGreenMaterialRelativePath = "Materials/physics/PhysicsDemoGreen" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the magenta physics demo material.
        /// </summary>
        const string PhysicsDemoMagentaMaterialRelativePath = "Materials/physics/PhysicsDemoMagenta" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the yellow physics demo material.
        /// </summary>
        const string PhysicsDemoYellowMaterialRelativePath = "Materials/physics/PhysicsDemoYellow" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Relative project asset path for the cyan physics demo material.
        /// </summary>
        const string PhysicsDemoCyanMaterialRelativePath = "Materials/physics/PhysicsDemoCyan" + EditorFileTemplateRegistry.MaterialExtension;

        /// <summary>
        /// Stable material importer identifier stored on generated material settings.
        /// </summary>
        const string MaterialImporterId = "helengine.material";

        /// <summary>
        /// Stable Windows standard-material schema identifier used by the shared editor material pipeline.
        /// </summary>
        const string WindowsMaterialSchemaId = "standard-shader";

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
        /// Stable material field identifier used for shadow receiving.
        /// </summary>
        const string ReceivesShadowFieldId = "receives-shadow";

        /// <summary>
        /// Stable material field identifier used for authored base color.
        /// </summary>
        const string BaseColorFieldId = "base-color";

        /// <summary>
        /// Shared shader source used to render the exported physics demo meshes with per-material colors and shadowed forward lighting.
        /// </summary>
        const string PhysicsDemoShaderSource =
            "cbuffer TransformBuffer : register(b0)\n" +
            "{\n" +
            "    float4x4 world;\n" +
            "    float4x4 worldViewProj;\n" +
            "    float4x4 normalMatrix;\n" +
            "    float4 cameraPosition;\n" +
            "};\n" +
            "\n" +
            "cbuffer ForwardLightBuffer : register(b1)\n" +
            "{\n" +
            "    float4 lightMetadata;\n" +
            "    float4 light0ColorAndType;\n" +
            "    float4 light0DirectionAndShadow;\n" +
            "    float4 light0PositionAndRange;\n" +
            "    float4 light0SpotAngles;\n" +
            "    float4 light1ColorAndType;\n" +
            "    float4 light1DirectionAndShadow;\n" +
            "    float4 light1PositionAndRange;\n" +
            "    float4 light1SpotAngles;\n" +
            "    float4 light2ColorAndType;\n" +
            "    float4 light2DirectionAndShadow;\n" +
            "    float4 light2PositionAndRange;\n" +
            "    float4 light2SpotAngles;\n" +
            "    float4 light3ColorAndType;\n" +
            "    float4 light3DirectionAndShadow;\n" +
            "    float4 light3PositionAndRange;\n" +
            "    float4 light3SpotAngles;\n" +
            "};\n" +
            "\n" +
            "cbuffer ShadowBuffer : register(b2)\n" +
            "{\n" +
            "    float4 shadowMetadata;\n" +
            "    float4 shadowLight0AtlasRect;\n" +
            "    float4 shadowLight0Metadata;\n" +
            "    float4x4 shadowLight0WorldToShadowClip;\n" +
            "    float4 shadowLight1AtlasRect;\n" +
            "    float4 shadowLight1Metadata;\n" +
            "    float4x4 shadowLight1WorldToShadowClip;\n" +
            "    float4 shadowLight2AtlasRect;\n" +
            "    float4 shadowLight2Metadata;\n" +
            "    float4x4 shadowLight2WorldToShadowClip;\n" +
            "    float4 shadowLight3AtlasRect;\n" +
            "    float4 shadowLight3Metadata;\n" +
            "    float4x4 shadowLight3WorldToShadowClip;\n" +
            "};\n" +
            "\n" +
            "cbuffer MaterialColorBuffer : register(b3)\n" +
            "{\n" +
            "    float4 surfaceColor;\n" +
            "};\n" +
            "\n" +
            "Texture2D shadowAtlasTexture : register(t1);\n" +
            "SamplerState shadowAtlasSampler : register(s1);\n" +
            "TextureCube pointShadowTexture0 : register(t2);\n" +
            "TextureCube pointShadowTexture1 : register(t3);\n" +
            "TextureCube pointShadowTexture2 : register(t4);\n" +
            "TextureCube pointShadowTexture3 : register(t5);\n" +
            "SamplerState pointShadowSampler : register(s2);\n" +
            "\n" +
            "struct VS_IN\n" +
            "{\n" +
            "    float3 pos : POSITION;\n" +
            "    float3 normal : NORMAL;\n" +
            "    float2 texCoord : TEXCOORD0;\n" +
            "};\n" +
            "\n" +
            "struct PS_IN\n" +
            "{\n" +
            "    float4 pos : SV_POSITION;\n" +
            "    float3 worldPos : TEXCOORD0;\n" +
            "    float3 normal : TEXCOORD1;\n" +
            "};\n" +
            "\n" +
            "PS_IN VS(VS_IN input)\n" +
            "{\n" +
            "    PS_IN output;\n" +
            "    float4 worldPosition = mul(float4(input.pos, 1.0f), world);\n" +
            "    output.pos = mul(float4(input.pos, 1.0f), worldViewProj);\n" +
            "    output.worldPos = worldPosition.xyz;\n" +
            "    output.normal = mul(float4(input.normal, 0.0f), normalMatrix).xyz;\n" +
            "    return output;\n" +
            "}\n" +
            "\n" +
            "float SamplePointShadowTexture(int textureIndex, float3 sampleDirection)\n" +
            "{\n" +
            "    if (textureIndex == 0)\n" +
            "    {\n" +
            "        return pointShadowTexture0.Sample(pointShadowSampler, sampleDirection).r;\n" +
            "    }\n" +
            "\n" +
            "    if (textureIndex == 1)\n" +
            "    {\n" +
            "        return pointShadowTexture1.Sample(pointShadowSampler, sampleDirection).r;\n" +
            "    }\n" +
            "\n" +
            "    if (textureIndex == 2)\n" +
            "    {\n" +
            "        return pointShadowTexture2.Sample(pointShadowSampler, sampleDirection).r;\n" +
            "    }\n" +
            "\n" +
            "    return pointShadowTexture3.Sample(pointShadowSampler, sampleDirection).r;\n" +
            "}\n" +
            "\n" +
            "float3 EvaluateForwardLight(\n" +
            "    float4 colorAndType,\n" +
            "    float4 directionAndShadow,\n" +
            "    float4 positionAndRange,\n" +
            "    float4 spotAngles,\n" +
            "    float4 shadowAtlasRect,\n" +
            "    float4 shadowSlotMetadata,\n" +
            "    float4x4 worldToShadowClip,\n" +
            "    float3 litSurfaceColor,\n" +
            "    float3 worldPos,\n" +
            "    float3 normal,\n" +
            "    float3 viewDirection)\n" +
            "{\n" +
            "    int lightType = (int)(colorAndType.w + 0.5f);\n" +
            "    float3 radiance = colorAndType.xyz;\n" +
            "    float3 lightDirection = float3(0.0f, 0.0f, 0.0f);\n" +
            "    float attenuation = 1.0f;\n" +
            "\n" +
            "    if (lightType == 0)\n" +
            "    {\n" +
            "        lightDirection = normalize(-directionAndShadow.xyz);\n" +
            "    }\n" +
            "    else\n" +
            "    {\n" +
            "        float3 toLight = positionAndRange.xyz - worldPos;\n" +
            "        float distanceToLight = length(toLight);\n" +
            "        if (distanceToLight <= 0.0001f || positionAndRange.w <= 0.0f)\n" +
            "        {\n" +
            "            return float3(0.0f, 0.0f, 0.0f);\n" +
            "        }\n" +
            "\n" +
            "        lightDirection = toLight / distanceToLight;\n" +
            "        float normalizedDistance = saturate(distanceToLight / positionAndRange.w);\n" +
            "        float rangeAttenuation = 1.0f - (normalizedDistance * normalizedDistance);\n" +
            "        attenuation = rangeAttenuation * rangeAttenuation;\n" +
            "\n" +
            "        if (lightType == 2)\n" +
            "        {\n" +
            "            float3 lightForward = normalize(directionAndShadow.xyz);\n" +
            "            float3 lightToSurface = normalize(worldPos - positionAndRange.xyz);\n" +
            "            float cone = dot(lightForward, lightToSurface);\n" +
            "            float coneRange = max(spotAngles.x - spotAngles.y, 0.0001f);\n" +
            "            float spotAttenuation = saturate((cone - spotAngles.y) / coneRange);\n" +
            "            attenuation *= spotAttenuation * spotAttenuation;\n" +
            "        }\n" +
            "    }\n" +
            "\n" +
            "    if (attenuation <= 0.0f)\n" +
            "    {\n" +
            "        return float3(0.0f, 0.0f, 0.0f);\n" +
            "    }\n" +
            "\n" +
            "    if (shadowSlotMetadata.x > 0.5f && shadowSlotMetadata.z < 1.5f && shadowMetadata.x > 0.5f)\n" +
            "    {\n" +
            "        float4 shadowClip = mul(float4(worldPos, 1.0f), worldToShadowClip);\n" +
            "        if (abs(shadowClip.w) > 0.0001f)\n" +
            "        {\n" +
            "            float3 shadowNdc = shadowClip.xyz / shadowClip.w;\n" +
            "            float2 shadowUv = float2((shadowNdc.x * 0.5f) + 0.5f, (-shadowNdc.y * 0.5f) + 0.5f);\n" +
            "            if (shadowUv.x >= 0.0f && shadowUv.x <= 1.0f && shadowUv.y >= 0.0f && shadowUv.y <= 1.0f && shadowNdc.z >= 0.0f && shadowNdc.z <= 1.0f)\n" +
            "            {\n" +
            "                float2 atlasUv = shadowAtlasRect.xy + (shadowUv * shadowAtlasRect.zw);\n" +
            "                float sampledDepth = shadowAtlasTexture.Sample(shadowAtlasSampler, atlasUv).r;\n" +
            "                float shadowBias = 0.0015f;\n" +
            "                float shadowVisibility = (shadowNdc.z - shadowBias) <= sampledDepth ? 1.0f : 0.0f;\n" +
            "                attenuation *= lerp(1.0f, shadowVisibility, shadowSlotMetadata.y);\n" +
            "            }\n" +
            "        }\n" +
            "    }\n" +
            "    else if (shadowSlotMetadata.x > 0.5f && shadowSlotMetadata.z > 1.5f && lightType == 1)\n" +
            "    {\n" +
            "        float3 lightToSurface = worldPos - positionAndRange.xyz;\n" +
            "        float distanceToSurface = length(lightToSurface);\n" +
            "        if (distanceToSurface > 0.0001f && positionAndRange.w > 0.0f)\n" +
            "        {\n" +
            "            int pointShadowTextureIndex = (int)(shadowSlotMetadata.w + 0.5f);\n" +
            "            float3 sampleDirection = lightToSurface / distanceToSurface;\n" +
            "            float currentDepth = saturate(distanceToSurface / positionAndRange.w);\n" +
            "            float sampledDepth = SamplePointShadowTexture(pointShadowTextureIndex, sampleDirection);\n" +
            "            float shadowBias = 0.01f;\n" +
            "            float shadowVisibility = (currentDepth - shadowBias) <= sampledDepth ? 1.0f : 0.0f;\n" +
            "            attenuation *= lerp(1.0f, shadowVisibility, shadowSlotMetadata.y);\n" +
            "        }\n" +
            "    }\n" +
            "\n" +
            "    float diffuse = saturate(dot(normal, lightDirection));\n" +
            "    if (diffuse <= 0.0f)\n" +
            "    {\n" +
            "        return float3(0.0f, 0.0f, 0.0f);\n" +
            "    }\n" +
            "\n" +
            "    float3 halfVector = normalize(lightDirection + viewDirection);\n" +
            "    float specular = pow(saturate(dot(normal, halfVector)), 32.0f);\n" +
            "    float3 diffuseColor = litSurfaceColor * radiance * diffuse * attenuation;\n" +
            "    float3 specularColor = radiance * specular * 0.35f * attenuation;\n" +
            "    return diffuseColor + specularColor;\n" +
            "}\n" +
            "\n" +
            "float4 PS(PS_IN input) : SV_Target\n" +
            "{\n" +
            "    float3 ambientColor = float3(0.14f, 0.16f, 0.18f);\n" +
            "    float3 normal = normalize(input.normal);\n" +
            "    float3 viewDirection = normalize(cameraPosition.xyz - input.worldPos);\n" +
            "    float3 color = surfaceColor.xyz * ambientColor;\n" +
            "    int activeLightCount = (int)(lightMetadata.x + 0.5f);\n" +
            "\n" +
            "    if (activeLightCount > 0)\n" +
            "    {\n" +
            "        color += EvaluateForwardLight(light0ColorAndType, light0DirectionAndShadow, light0PositionAndRange, light0SpotAngles, shadowLight0AtlasRect, shadowLight0Metadata, shadowLight0WorldToShadowClip, surfaceColor.xyz, input.worldPos, normal, viewDirection);\n" +
            "    }\n" +
            "\n" +
            "    if (activeLightCount > 1)\n" +
            "    {\n" +
            "        color += EvaluateForwardLight(light1ColorAndType, light1DirectionAndShadow, light1PositionAndRange, light1SpotAngles, shadowLight1AtlasRect, shadowLight1Metadata, shadowLight1WorldToShadowClip, surfaceColor.xyz, input.worldPos, normal, viewDirection);\n" +
            "    }\n" +
            "\n" +
            "    if (activeLightCount > 2)\n" +
            "    {\n" +
            "        color += EvaluateForwardLight(light2ColorAndType, light2DirectionAndShadow, light2PositionAndRange, light2SpotAngles, shadowLight2AtlasRect, shadowLight2Metadata, shadowLight2WorldToShadowClip, surfaceColor.xyz, input.worldPos, normal, viewDirection);\n" +
            "    }\n" +
            "\n" +
            "    if (activeLightCount > 3)\n" +
            "    {\n" +
            "        color += EvaluateForwardLight(light3ColorAndType, light3DirectionAndShadow, light3PositionAndRange, light3SpotAngles, shadowLight3AtlasRect, shadowLight3Metadata, shadowLight3WorldToShadowClip, surfaceColor.xyz, input.worldPos, normal, viewDirection);\n" +
            "    }\n" +
            "\n" +
            "    return float4(saturate(color), surfaceColor.w);\n" +
            "}\n";

        /// <summary>
        /// Current payload version for serialized rigid-body component scene records.
        /// </summary>
        const byte RigidBodyComponentPayloadVersion = 1;

        /// <summary>
        /// Current payload version for serialized box-collider component scene records.
        /// </summary>
        const byte BoxColliderComponentPayloadVersion = 2;

        /// <summary>
        /// Current payload version for serialized kinematic-motion component scene records.
        /// </summary>
        const byte KinematicMotionComponentPayloadVersion = 1;

        /// <summary>
        /// Current payload version for serialized character-controller component scene records.
        /// </summary>
        const byte CharacterControllerComponentPayloadVersion = 1;

        /// <summary>
        /// Serialized rigid-body kind byte for static bodies.
        /// </summary>
        const byte StaticBodyKindCode = 0;

        /// <summary>
        /// Serialized rigid-body kind byte for dynamic bodies.
        /// </summary>
        const byte DynamicBodyKindCode = 2;

        /// <summary>
        /// Serialized rigid-body kind byte for kinematic bodies.
        /// </summary>
        const byte KinematicBodyKindCode = 1;

        /// <summary>
        /// Generated cube model assigned to each visible physics primitive.
        /// </summary>
        readonly RuntimeModel CubeModel;

        /// <summary>
        /// Runtime material used by neutral physics demo geometry while authoring generated scenes.
        /// </summary>
        readonly RuntimeMaterial NeutralMaterial;

        /// <summary>
        /// Runtime material used by blue physics demo geometry while authoring generated scenes.
        /// </summary>
        readonly RuntimeMaterial BlueMaterial;

        /// <summary>
        /// Runtime material used by green physics demo geometry while authoring generated scenes.
        /// </summary>
        readonly RuntimeMaterial GreenMaterial;

        /// <summary>
        /// Runtime material used by magenta physics demo geometry while authoring generated scenes.
        /// </summary>
        readonly RuntimeMaterial MagentaMaterial;

        /// <summary>
        /// Runtime material used by yellow physics demo geometry while authoring generated scenes.
        /// </summary>
        readonly RuntimeMaterial YellowMaterial;

        /// <summary>
        /// Runtime material used by cyan physics demo geometry while authoring generated scenes.
        /// </summary>
        readonly RuntimeMaterial CyanMaterial;

        /// <summary>
        /// Initializes the validation-scene factory with generated runtime assets used during authoring.
        /// </summary>
        public PhysicsSceneFactory() {
            CubeModel = EngineGeneratedModelCache.GetRuntimeModel(EngineGeneratedModelCache.CubeAssetId);
            NeutralMaterial = CreateRuntimeMaterial("PhysicsDemoNeutral", "#C4CCD6FF");
            BlueMaterial = CreateRuntimeMaterial("PhysicsDemoBlue", "#548FE6FF");
            GreenMaterial = CreateRuntimeMaterial("PhysicsDemoGreen", "#61C27DFF");
            MagentaMaterial = CreateRuntimeMaterial("PhysicsDemoMagenta", "#D66BBAFF");
            YellowMaterial = CreateRuntimeMaterial("PhysicsDemoYellow", "#EBC954FF");
            CyanMaterial = CreateRuntimeMaterial("PhysicsDemoCyan", "#4FC9D1FF");
        }

        /// <summary>
        /// Creates one live-authored physics validation scene definition for the requested scene id.
        /// </summary>
        /// <param name="sceneId">Stable relative scene id to author.</param>
        /// <returns>Generated scene definition ready for editor-owned persistence.</returns>
        public PhysicsAuthoringSceneDefinition CreateSceneDefinition(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }

            if (string.Equals(sceneId, PhysicsSceneCatalog.CharacterSlopeSceneId, StringComparison.Ordinal)) {
                return CreateCharacterSlopeScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.CharacterStepsSceneId, StringComparison.Ordinal)) {
                return CreateCharacterStepsScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.CharacterMovingPlatformSceneId, StringComparison.Ordinal)) {
                return CreateCharacterMovingPlatformScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicStackBoxesSceneId, StringComparison.Ordinal)) {
                return CreateDynamicStackBoxesScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicSphereRampSceneId, StringComparison.Ordinal)) {
                return CreateDynamicSphereRampScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.KinematicPushSceneId, StringComparison.Ordinal)) {
                return CreateKinematicPushScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.MeshGroundStabilitySceneId, StringComparison.Ordinal)) {
                return CreateMeshGroundStabilityScene();
            } else if (string.Equals(sceneId, PhysicsSceneCatalog.TriggerVolumeSceneId, StringComparison.Ordinal)) {
                return CreateTriggerVolumeScene();
            }

            throw new InvalidOperationException($"Unsupported physics validation scene id '{sceneId}'.");
        }

        /// <summary>
        /// Creates the character slope validation scene.
        /// </summary>
        /// <returns>Authored slope validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateCharacterSlopeScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "character_slope.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("character_slope.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(14f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, NeutralMaterial),
                    CreatePhysicsBoxMeshEntity("character_slope.ramp", "SlopeRamp", new float3(2.25f, 0.6f, 0f), new float3(5f, 0.6f, 3f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), StaticBodyKindCode, false, GreenMaterial),
                    CreateCharacterControllerBoxMeshEntity("character_slope.controller", "CharacterController", new float3(-4f, 0.75f, 0f), new float3(0.9f, 1.5f, 0.9f), float4.Identity, new float3(1f, 0f, 0f), 3d, 1d, 0.75d, 0.3d, MagentaMaterial),
                    CreateMarkerEntity("character_slope.spawn", "ControllerSpawn", new float3(-4f, 0.75f, 0f)),
                    CreateMarkerEntity("character_slope.goal", "SlopeGoal", new float3(4.25f, 1.75f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("character_slope.camera", new float3(0f, 5.5f, 18f), CreateYawPitchRollDegrees(0.0, -18.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.CharacterSlopeSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the character steps validation scene.
        /// </summary>
        /// <returns>Authored steps validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateCharacterStepsScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "character_steps.scenario",
                new[] {
                    CreateCubeMeshEntity("character_steps.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 12f), float4.Identity, NeutralMaterial),
                    CreateCubeMeshEntity("character_steps.step01", "Step01", new float3(0.75f, 0.15f, 0f), new float3(1.5f, 0.3f, 3f), float4.Identity, BlueMaterial),
                    CreateCubeMeshEntity("character_steps.step02", "Step02", new float3(2.25f, 0.45f, 0f), new float3(1.5f, 0.9f, 3f), float4.Identity, GreenMaterial),
                    CreateCubeMeshEntity("character_steps.step03", "Step03", new float3(3.75f, 0.75f, 0f), new float3(1.5f, 1.5f, 3f), float4.Identity, YellowMaterial),
                    CreateCubeMeshEntity("character_steps.step04", "Step04", new float3(5.25f, 1.05f, 0f), new float3(1.5f, 2.1f, 3f), float4.Identity, MagentaMaterial),
                    CreateMarkerEntity("character_steps.spawn", "ControllerSpawn", new float3(-4.5f, 0.75f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("character_steps.camera", new float3(0f, 6f, 18f), CreateYawPitchRollDegrees(0.0, -20.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.CharacterStepsSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the character moving-platform validation scene.
        /// </summary>
        /// <returns>Authored moving-platform validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateCharacterMovingPlatformScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "character_moving_platform.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("character_moving_platform.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(18f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, NeutralMaterial),
                    CreatePhysicsBoxMeshEntity("character_moving_platform.gap_a", "GapEdgeA", new float3(-1.75f, 0.25f, 0f), new float3(4f, 0.5f, 4f), float4.Identity, StaticBodyKindCode, false, GreenMaterial),
                    CreatePhysicsBoxMeshEntity("character_moving_platform.gap_b", "GapEdgeB", new float3(4.75f, 0.25f, 0f), new float3(4f, 0.5f, 4f), float4.Identity, StaticBodyKindCode, false, YellowMaterial),
                    CreateKinematicPhysicsBoxMeshEntity(
                        "character_moving_platform.platform",
                        "MovingPlatform",
                        new float3(-0.5f, 0.75f, 0f),
                        new float3(2.5f, 0.35f, 2.5f),
                        float4.Identity,
                        new float3(-0.5f, 0.75f, 0f),
                        new float3(3.5f, 0.75f, 0f),
                        2d,
                        true,
                        CyanMaterial),
                    CreateMarkerEntity("character_moving_platform.platform_start", "PlatformStart", new float3(-0.5f, 0.75f, 0f)),
                    CreateMarkerEntity("character_moving_platform.platform_end", "PlatformEnd", new float3(3.5f, 0.75f, 0f)),
                    CreateMarkerEntity("character_moving_platform.spawn", "ControllerSpawn", new float3(-5f, 0.75f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("character_moving_platform.camera", new float3(0f, 6f, 20f), CreateYawPitchRollDegrees(0.0, -18.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.CharacterMovingPlatformSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the readable two-box offset dynamic-body validation scene.
        /// </summary>
        /// <returns>Authored two-box offset-stack validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateDynamicStackBoxesScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "dynamic_stack_boxes.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("dynamic_stack_boxes.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(14f, 1f, 14f), float4.Identity, StaticBodyKindCode, false, NeutralMaterial),
                    CreateOffsetPhysicsBoxMeshEntity("dynamic_stack_boxes.box01", "StackBox01", new float3(0f, 1f, 0f), float4.Identity, float3.Zero, BlueMaterial),
                    CreateOffsetPhysicsBoxMeshEntity("dynamic_stack_boxes.box02", "StackBox02", new float3(0.9f, 3f, 0f), float4.Identity, float3.Zero, GreenMaterial),
                    CreateMarkerEntity("dynamic_stack_boxes.spawn", "DynamicSpawn", new float3(-2.5f, 1.5f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("dynamic_stack_boxes.camera", new float3(8f, 5.5f, 12f), CreateYawPitchRollDegrees(34.0, -14.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.DynamicStackBoxesSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the sphere-ramp validation scene.
        /// </summary>
        /// <returns>Authored sphere-ramp validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateDynamicSphereRampScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "dynamic_sphere_ramp.scenario",
                new[] {
                    CreateCubeMeshEntity("dynamic_sphere_ramp.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 14f), float4.Identity, NeutralMaterial),
                    CreateCubeMeshEntity("dynamic_sphere_ramp.ramp", "Ramp", new float3(2.5f, 0.8f, 0f), new float3(6f, 0.6f, 4f), CreateYawPitchRollDegrees(0.0, 0.0, -16.0), CyanMaterial),
                    CreateMarkerEntity("dynamic_sphere_ramp.spawn", "SphereSpawn", new float3(-3.5f, 1.5f, 0f)),
                    CreateMarkerEntity("dynamic_sphere_ramp.goal", "RampGoal", new float3(5.5f, 1.75f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("dynamic_sphere_ramp.camera", new float3(0f, 6f, 18f), CreateYawPitchRollDegrees(0.0, -18.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.DynamicSphereRampSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the kinematic push validation scene.
        /// </summary>
        /// <returns>Authored kinematic push validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateKinematicPushScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "kinematic_push.scenario",
                new[] {
                    CreatePhysicsBoxMeshEntity("kinematic_push.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(16f, 1f, 12f), float4.Identity, StaticBodyKindCode, false, NeutralMaterial),
                    CreatePhysicsBoxMeshEntity("kinematic_push.block", "DynamicTarget", new float3(1.5f, 0.5f, 0f), new float3(1f, 1f, 1f), float4.Identity, DynamicBodyKindCode, true, YellowMaterial),
                    CreateKinematicPhysicsBoxMeshEntity(
                        "kinematic_push.pusher",
                        "KinematicPusher",
                        new float3(-2f, 0.5f, 0f),
                        new float3(1.5f, 1f, 1.5f),
                        float4.Identity,
                        new float3(-2f, 0.5f, 0f),
                        new float3(0.5f, 0.5f, 0f),
                        1d,
                        true,
                        CyanMaterial),
                    CreateMarkerEntity("kinematic_push.start", "PusherStart", new float3(-3.5f, 0.5f, 0f)),
                    CreateMarkerEntity("kinematic_push.end", "PusherEnd", new float3(0.5f, 0.5f, 0f)),
                    CreateMarkerEntity("kinematic_push.dynamic_spawn", "DynamicSpawn", new float3(1.5f, 0.5f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("kinematic_push.camera", new float3(0f, 5f, 17f), CreateYawPitchRollDegrees(0.0, -16.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.KinematicPushSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the static-mesh ground stability validation scene.
        /// </summary>
        /// <returns>Authored static-ground stability validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateMeshGroundStabilityScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "mesh_ground_stability.scenario",
                new[] {
                    CreateCubeMeshEntity("mesh_ground_stability.base", "GroundBase", new float3(0f, -0.5f, 0f), new float3(20f, 1f, 14f), float4.Identity, NeutralMaterial),
                    CreateCubeMeshEntity("mesh_ground_stability.section01", "StaticMeshGround01", new float3(-2.5f, 0.15f, 0f), new float3(3f, 0.3f, 4f), float4.Identity, BlueMaterial),
                    CreateCubeMeshEntity("mesh_ground_stability.section02", "StaticMeshGround02", new float3(0.5f, 0.35f, 0f), new float3(3f, 0.7f, 4f), float4.Identity, GreenMaterial),
                    CreateCubeMeshEntity("mesh_ground_stability.section03", "StaticMeshGround03", new float3(3.5f, 0.2f, 0f), new float3(3f, 0.4f, 4f), CreateYawPitchRollDegrees(0.0, 0.0, -6.0), MagentaMaterial),
                    CreateCubeMeshEntity("mesh_ground_stability.section04", "StaticMeshGround04", new float3(6.5f, 0.45f, 0f), new float3(3f, 0.9f, 4f), CreateYawPitchRollDegrees(0.0, 0.0, 5.0), YellowMaterial),
                    CreateMarkerEntity("mesh_ground_stability.spawn", "WalkerSpawn", new float3(-5.5f, 0.75f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("mesh_ground_stability.camera", new float3(0f, 6.5f, 22f), CreateYawPitchRollDegrees(0.0, -18.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.MeshGroundStabilitySceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the trigger-volume validation scene.
        /// </summary>
        /// <returns>Authored trigger-volume validation scene asset.</returns>
        PhysicsAuthoringSceneDefinition CreateTriggerVolumeScene() {
            Entity scenarioEntity = CreateScenarioRoot(
                "trigger_volume.scenario",
                new[] {
                    CreateCubeMeshEntity("trigger_volume.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(18f, 1f, 12f), float4.Identity, NeutralMaterial),
                    CreateCubeMeshEntity("trigger_volume.arch", "TriggerVolume", new float3(1.5f, 1.5f, 0f), new float3(2.5f, 3f, 2.5f), float4.Identity, CyanMaterial),
                    CreateMarkerEntity("trigger_volume.start", "PlayerPathStart", new float3(-5f, 0.75f, 0f)),
                    CreateMarkerEntity("trigger_volume.end", "PlayerPathEnd", new float3(5.5f, 0.75f, 0f))
                });
            Entity cameraEntity = CreateCameraEntity("trigger_volume.camera", new float3(0f, 5.5f, 19f), CreateYawPitchRollDegrees(0.0, -18.0, 0.0));
            return CreateSceneDefinition(PhysicsSceneCatalog.TriggerVolumeSceneId, cameraEntity, scenarioEntity);
        }

        /// <summary>
        /// Creates the final live-authored scene definition shared by every validation scenario.
        /// </summary>
        /// <param name="sceneId">Stable relative scene id.</param>
        /// <param name="cameraEntity">Root camera entity.</param>
        /// <param name="scenarioEntity">Root scenario entity.</param>
        /// <returns>Scene definition ready for editor-owned persistence.</returns>
        PhysicsAuthoringSceneDefinition CreateSceneDefinition(
            string sceneId,
            Entity cameraEntity,
            Entity scenarioEntity) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            }
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            }
            if (scenarioEntity == null) {
                throw new ArgumentNullException(nameof(scenarioEntity));
            }

            return new PhysicsAuthoringSceneDefinition {
                SceneId = sceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = new[] { cameraEntity, scenarioEntity, CreateDebugOverlayEntity() }
            };
        }

        /// <summary>
        /// Creates the debug overlay root included in every generated physics scene for runtime diagnostics.
        /// </summary>
        /// <returns>Root entity that owns one configured debug overlay component.</returns>
        Entity CreateDebugOverlayEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("DebugOverlay");
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new DebugComponent {
                Font = ResolveRequiredEditorFont(),
                Padding = new int2(8, 8),
                RenderOrder2D = 250,
                RefreshIntervalSeconds = 0.25d
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor default font required by the generated debug overlays.
        /// </summary>
        /// <returns>Editor default font asset used by debug overlay text rows.</returns>
        FontAsset ResolveRequiredEditorFont() {
            EditorCore editorCore = Core.Instance as EditorCore;
            if (editorCore == null || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before physics validation scenes can be generated with debug overlays.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Creates the scenario root entity that owns the authored test geometry and markers.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="children">Authored scenario children.</param>
        /// <returns>Scenario root entity.</returns>
        Entity CreateScenarioRoot(string entityId, Entity[] children) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Scenario entity id must be provided.", nameof(entityId));
            }
            if (children == null) {
                throw new ArgumentNullException(nameof(children));
            }

            Entity scenario = Core.Instance.EntityFactory.Create("Scenario");
            scenario.LocalPosition = float3.Zero;
            scenario.LocalScale = float3.One;
            scenario.LocalOrientation = float4.Identity;
            for (int index = 0; index < children.Length; index++) {
                scenario.AddChild(children[index]);
            }

            scenario.AddChild(CreateKeyLightEntity());
            return scenario;
        }

        /// <summary>
        /// Creates one camera root entity for a validation scene.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="position">Camera position.</param>
        /// <param name="orientation">Camera orientation.</param>
        /// <returns>Camera entity with a live camera component.</returns>
        Entity CreateCameraEntity(string entityId, float3 position, float4 orientation) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Camera entity id must be provided.", nameof(entityId));
            }

            Entity entity = Core.Instance.EntityFactory.Create("Camera");
            entity.LocalPosition = position;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = DefaultCameraDrawOrder,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                ClearSettings = new CameraClearSettings(true, new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f), true, 1f, false, 0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates one mesh-backed cube entity for the validation scene.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <returns>Mesh-backed entity.</returns>
        Entity CreateCubeMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Mesh entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Mesh entity name must be provided.", nameof(name));
            }
            if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new MeshComponent {
                Model = CubeModel,
                Material = material,
                RenderOrder3D = DefaultMeshRenderOrder
            });
            return entity;
        }

        /// <summary>
        /// Creates one mesh-backed box entity that also carries serialized 3D physics records.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale used to size the rendered cube and its unit collider in world space.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="bodyKindCode">Rigid-body participation mode byte to serialize.</param>
        /// <param name="useGravity">True when the serialized rigid body should receive gravity.</param>
        /// <returns>Mesh-backed entity with live rigid-body and box-collider components.</returns>
        Entity CreatePhysicsBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            byte bodyKindCode,
            bool useGravity,
            RuntimeMaterial material) {
            return CreatePhysicsBoxMeshEntity(entityId, name, position, scale, orientation, bodyKindCode, useGravity, float3.Zero, material);
        }

        /// <summary>
        /// Creates one mesh-backed box entity with explicit rigid-body angular velocity.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale used to size the rendered cube and its unit collider in world space.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="bodyKindCode">Rigid-body participation mode byte to serialize.</param>
        /// <param name="useGravity">True when the serialized rigid body should receive gravity.</param>
        /// <param name="angularVelocity">Initial angular velocity in radians per second.</param>
        /// <param name="material">Runtime material assigned to the cube mesh.</param>
        /// <returns>Mesh-backed entity with live rigid-body and box-collider components.</returns>
        Entity CreatePhysicsBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            byte bodyKindCode,
            bool useGravity,
            float3 angularVelocity,
            RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Physics entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Physics entity name must be provided.", nameof(name));
            }
            if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = CreateCubeMeshEntity(entityId, name, position, scale, orientation, material);
            entity.AddComponent(new RigidBody3DComponent {
                BodyKind = (BodyKind3D)bodyKindCode,
                UseGravity = useGravity,
                Mass = 1d,
                GravityScale = 1d,
                LinearVelocity = float3.Zero,
                AngularVelocity = angularVelocity
            });
            entity.AddComponent(new BoxCollider3DComponent {
                Size = float3.One
            });
            return entity;
        }

        /// <summary>
        /// Creates one dynamic box with authored initial rotation for deterministic physics validation scenes.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity spawn position.</param>
        /// <param name="orientation">Initial visual orientation.</param>
        /// <param name="angularVelocity">Initial angular velocity in radians per second.</param>
        /// <param name="material">Runtime material assigned to the cube mesh.</param>
        /// <returns>Mesh-backed dynamic physics box with authored initial rotation.</returns>
        Entity CreateOffsetPhysicsBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float4 orientation,
            float3 angularVelocity,
            RuntimeMaterial material) {
            return CreatePhysicsBoxMeshEntity(entityId, name, position, float3.One, orientation, DynamicBodyKindCode, true, angularVelocity, material);
        }

        /// <summary>
        /// Creates one mesh-backed box entity that also carries serialized 3D kinematic-motion records.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale used to size the rendered cube and its unit collider in world space.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="startLocalPosition">Kinematic motion start position.</param>
        /// <param name="endLocalPosition">Kinematic motion end position.</param>
        /// <param name="travelDurationSeconds">One-way travel duration in seconds.</param>
        /// <param name="pingPong">True when the motion should reverse at the end.</param>
        /// <returns>Mesh-backed entity with live rigid-body, box-collider, and kinematic-motion components.</returns>
        Entity CreateKinematicPhysicsBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            float3 startLocalPosition,
            float3 endLocalPosition,
            double travelDurationSeconds,
            bool pingPong,
            RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Physics entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Physics entity name must be provided.", nameof(name));
            }
            if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = CreatePhysicsBoxMeshEntity(entityId, name, position, scale, orientation, KinematicBodyKindCode, false, material);
            entity.AddComponent(new KinematicMotion3DComponent {
                StartLocalPosition = startLocalPosition,
                EndLocalPosition = endLocalPosition,
                TravelDurationSeconds = travelDurationSeconds,
                PingPong = pingPong
            });
            return entity;
        }

        /// <summary>
        /// Creates one mesh-backed box entity that carries serialized 3D character-controller records.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Entity position.</param>
        /// <param name="scale">Entity scale and collider size.</param>
        /// <param name="orientation">Entity orientation.</param>
        /// <param name="desiredMoveDirection">Desired local move direction used by the controller.</param>
        /// <param name="moveSpeed">Horizontal move speed in world units per second.</param>
        /// <param name="gravityScale">Gravity multiplier used by the controller.</param>
        /// <param name="stepHeight">Maximum upward snap height used while climbing support surfaces.</param>
        /// <param name="groundSnapDistance">Maximum downward snap distance used to keep the controller grounded.</param>
        /// <returns>Mesh-backed entity with live box-collider and character-controller components.</returns>
        Entity CreateCharacterControllerBoxMeshEntity(
            string entityId,
            string name,
            float3 position,
            float3 scale,
            float4 orientation,
            float3 desiredMoveDirection,
            double moveSpeed,
            double gravityScale,
            double stepHeight,
            double groundSnapDistance,
            RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Character controller entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Character controller entity name must be provided.", nameof(name));
            }
            if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = CreateCubeMeshEntity(entityId, name, position, scale, orientation, material);
            entity.AddComponent(new BoxCollider3DComponent {
                Size = float3.One
            });
            entity.AddComponent(new CharacterController3DComponent {
                DesiredMoveDirection = desiredMoveDirection,
                MoveSpeed = moveSpeed,
                GravityScale = gravityScale,
                StepHeight = stepHeight,
                GroundSnapDistance = groundSnapDistance
            });
            return entity;
        }

        /// <summary>
        /// Creates one empty marker entity used as a future spawn, target, or motion reference.
        /// </summary>
        /// <param name="entityId">Stable authoring id used to name generated roots during creation.</param>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Marker position.</param>
        /// <returns>Marker entity without components.</returns>
        Entity CreateMarkerEntity(string entityId, string name, float3 position) {
            if (string.IsNullOrWhiteSpace(entityId)) {
                throw new ArgumentException("Marker entity id must be provided.", nameof(entityId));
            }
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Marker entity name must be provided.", nameof(name));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LocalPosition = position;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            return entity;
        }

        /// <summary>
        /// Creates the shared directional light used to give the exported validation scenes stronger shape and visible shadows.
        /// </summary>
        /// <returns>Directional light entity appended to each scenario root.</returns>
        Entity CreateKeyLightEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("KeyLight");
            entity.LocalPosition = new float3(0f, 6f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = CreateYawPitchRollDegrees(-48.0, -44.0, 0.0);
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1.0f, 0.96f, 0.90f, 1.0f),
                Intensity = 2.35f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f,
                ShadowDistance = 72f
            });
            return entity;
        }

        /// <summary>
        /// Creates one runtime material used while authoring generated physics scenes.
        /// </summary>
        /// <param name="assetId">Stable material asset id written to material settings.</param>
        /// <param name="surfaceColor">Authored color string in <c>#RRGGBBAA</c> form.</param>
        /// <returns>Runtime material with an id that the editor save pipeline can map back to the authored material file.</returns>
        RuntimeMaterial CreateRuntimeMaterial(string assetId, string surfaceColor) {
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            } else if (string.IsNullOrWhiteSpace(surfaceColor)) {
                throw new ArgumentException("Surface color must be provided.", nameof(surfaceColor));
            }

            MaterialAsset materialAsset = new MaterialAsset {
                Id = assetId,
                ShaderAssetId = "ForwardStandardShader",
                VertexProgram = "ForwardStandardShader.vs",
                PixelProgram = "ForwardStandardShader.ps",
                Variant = "Mesh",
                RenderState = new MaterialRenderState(),
                ConstantBuffers = new[] {
                    new MaterialConstantBufferAsset {
                        Name = StandardMaterialBaseColorDefaults.BaseColorBufferName,
                        Data = StandardMaterialBaseColorDefaults.CreateConstantBufferData(ParseColor(surfaceColor))
                    }
                },
                CastsShadows = true,
                ReceivesShadows = true
            };

            ShaderAsset shaderAsset = EditorBuiltInShaderAssetLibrary.LoadShaderAsset(Core.Instance.RenderManager3D, "ForwardStandardShader.hlsl");
            RuntimeMaterial runtimeMaterial = Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(runtimeMaterial);
            return runtimeMaterial;
        }

        /// <summary>
        /// Parses one authored hex color string into a normalized float4 color.
        /// </summary>
        /// <param name="colorValue">Authored color string in <c>#RRGGBBAA</c> form.</param>
        /// <returns>Normalized float4 color.</returns>
        static float4 ParseColor(string colorValue) {
            if (string.IsNullOrWhiteSpace(colorValue)) {
                throw new ArgumentException("Color value must be provided.", nameof(colorValue));
            } else if (!colorValue.StartsWith('#') || colorValue.Length != 9) {
                throw new InvalidOperationException($"Color value '{colorValue}' must use #RRGGBBAA format.");
            }

            uint rgba = Convert.ToUInt32(colorValue.Substring(1, 8), 16);
            return new float4(
                ((rgba >> 24) & 0xFF) / 255f,
                ((rgba >> 16) & 0xFF) / 255f,
                ((rgba >> 8) & 0xFF) / 255f,
                (rgba & 0xFF) / 255f);
        }

        /// <summary>
        /// Writes the shared shader and material assets consumed by the exported physics validation scenes.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        public void WriteSupportAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            WriteMaterialAsset(projectRootPath, PhysicsDemoNeutralMaterialRelativePath, "PhysicsDemoNeutral", "#C4CCD6FF");
            WriteMaterialAsset(projectRootPath, PhysicsDemoBlueMaterialRelativePath, "PhysicsDemoBlue", "#548FE6FF");
            WriteMaterialAsset(projectRootPath, PhysicsDemoGreenMaterialRelativePath, "PhysicsDemoGreen", "#61C27DFF");
            WriteMaterialAsset(projectRootPath, PhysicsDemoMagentaMaterialRelativePath, "PhysicsDemoMagenta", "#D66BBAFF");
            WriteMaterialAsset(projectRootPath, PhysicsDemoYellowMaterialRelativePath, "PhysicsDemoYellow", "#EBC954FF");
            WriteMaterialAsset(projectRootPath, PhysicsDemoCyanMaterialRelativePath, "PhysicsDemoCyan", "#4FC9D1FF");
        }

        /// <summary>
        /// Writes the shared file-backed HLSL shader used by the exported physics validation scenes.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        static void WriteShaderAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullPath = Path.Combine(projectRootPath, "assets", PhysicsDemoShaderRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a directory path for shader '{PhysicsDemoShaderRelativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(fullPath, PhysicsDemoShaderSource);
        }

        /// <summary>
        /// Writes one settings-backed material asset used by the exported physics validation scenes.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path that owns the `assets` directory.</param>
        /// <param name="relativePath">Relative project asset path for the material file.</param>
        /// <param name="assetId">Stable material asset identifier stored in importer settings.</param>
        /// <param name="surfaceColor">HTML color used by the standard material base-color field.</param>
        static void WriteMaterialAsset(string projectRootPath, string relativePath, string assetId, string surfaceColor) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }
            if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            }

            string fullPath = Path.Combine(projectRootPath, "assets", relativePath.Replace('/', Path.DirectorySeparatorChar));
            string directoryPath = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directoryPath)) {
                throw new InvalidOperationException($"Could not resolve a directory path for material '{relativePath}'.");
            }

            Directory.CreateDirectory(directoryPath);

            MaterialAssetImportSettings settings = new MaterialAssetImportSettings();
            settings.Importer.ImporterId = MaterialImporterId;
            settings.Importer.SourceChecksum = string.Empty;
            settings.Importer.AssetId = assetId;

            MaterialAssetProcessorSettings windowsSettings = new MaterialAssetProcessorSettings();
            windowsSettings.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.FieldValues[UseCustomShaderFieldId] = "false";
            windowsSettings.FieldValues[TextureIdFieldId] = string.Empty;
            windowsSettings.FieldValues[CastsShadowFieldId] = "true";
            windowsSettings.FieldValues[ReceivesShadowFieldId] = "true";
            windowsSettings.FieldValues[BaseColorFieldId] = surfaceColor;
            settings.Processor.Platforms["windows"] = windowsSettings;

            MaterialAssetSettingsService settingsService = new MaterialAssetSettingsService();
            settingsService.Save(fullPath, settings);
        }

        /// <summary>
        /// Creates one quaternion from yaw, pitch, and roll angles expressed in degrees.
        /// </summary>
        /// <param name="yawDegrees">Yaw around the Y axis in degrees.</param>
        /// <param name="pitchDegrees">Pitch around the X axis in degrees.</param>
        /// <param name="rollDegrees">Roll around the Z axis in degrees.</param>
        /// <returns>Converted quaternion.</returns>
        static float4 CreateYawPitchRollDegrees(double yawDegrees, double pitchDegrees, double rollDegrees) {
            float4.CreateFromYawPitchRoll(
                (float)(yawDegrees * Math.PI / 180.0),
                (float)(pitchDegrees * Math.PI / 180.0),
                (float)(rollDegrees * Math.PI / 180.0),
                out float4 result);
            return result;
        }

    }
}
