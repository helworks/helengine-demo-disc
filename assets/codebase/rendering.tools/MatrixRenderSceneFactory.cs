namespace city.rendering.tools {
    /// <summary>
    /// Authors the Matrix Render transform-inspection scene through the live rendering scene pipeline.
    /// </summary>
    public sealed class MatrixRenderSceneFactory {
        /// <summary>
        /// Host-owned capability used to resolve generated control icons and fonts.
        /// </summary>
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;
        /// <summary>
        /// Stable scene id used by the generated Matrix Render asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.MatrixRenderSceneId;

        /// <summary>
        /// Stable standard shader asset identifier used by compatibility material payloads.
        /// </summary>
        const string StandardShaderAssetId = "ForwardStandardShader";

        /// <summary>
        /// Stable standard shader source file used by the generated hero runtime material.
        /// </summary>
        const string StandardShaderSourceFileName = "ForwardStandardShader.hlsl";

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

        const string WindowsMaterialSchemaId = "standard-shader";
        const string Ps2MaterialSchemaId = "ps2-simple-lit";
        const string GameCubeMaterialSchemaId = "gamecube-standard";
        const string UseCustomShaderFieldId = "use-custom-shader";
        const string ShaderAssetIdFieldId = "shader-asset-id";
        const string TextureIdFieldId = "texture-id";
        const string CastsShadowFieldId = "casts-shadow";
        const string Ps2CastShadowsFieldId = "cast-shadows";
        const string ReceivesShadowFieldId = "receives-shadow";
        const string BaseColorFieldId = "base-color";
        const string AlphaModeFieldId = "alpha-mode";
        const string DoubleSidedFieldId = "double-sided";
        const string VertexColorModeFieldId = "vertex-color-mode";
        const string LightingModeFieldId = "lighting-mode";

        /// <summary>
        /// Stable authored hero cube color matching the historical PhysicsDemoRed material.
        /// </summary>
        const string HeroMaterialColor = "#E6524AFF";

        /// <summary>
        /// Stable project-relative path of the generated hero material.
        /// </summary>
        const string HeroMaterialRelativePath = "Materials/rendering/matrix_render/Hero.hasset";

        /// <summary>
        /// Stable material asset id stored inside the serialized hero material.
        /// </summary>
        const string HeroMaterialAssetId = "Materials.rendering.matrix_render.Hero";

        /// <summary>
        /// Service used to persist the generated hero material asset plus its per-platform material settings.
        /// </summary>
        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes the Matrix Render scene factory with the services required for authored output.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used by the shared instruction overlay.</param>
        public MatrixRenderSceneFactory(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            MaterialWriteService = new GeneratedMaterialAssetWriteService(AssetAuthoringService);
        }

        /// <summary>
        /// Creates the canonical Matrix Render live-authored scene definition.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated prompt icons.</param>
        /// <param name="cubeModel">Generated cube runtime model assigned to the hero cube.</param>
        /// <returns>Live-authored scene definition for the Matrix Render scene.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel cubeModel) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory(AssetAuthoringService);
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
            ConsoleCameraLightInstructionsSceneAttachmentService consoleInstructionAttachmentService = new ConsoleCameraLightInstructionsSceneAttachmentService();
            Entity consoleInstructionBlueprintEntity = consoleInstructionAttachmentService.CreateBlueprintInstanceRoot(projectRootPath, AssetAuthoringService);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = Array.Empty<Entity>()
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateUiEntity(),
                    CreateKeyLightEntity(),
                    instructionOverlayEntity,
                    consoleInstructionBlueprintEntity,
                    CreateHeroEntity(cubeModel, CreateRuntimeMaterial())
                }
            };
        }

        /// <summary>
        /// Writes the file-backed hero material asset and settings document used by the Matrix Render scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void WriteMaterialAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            MaterialWriteService.WriteMaterial(HeroMaterialRelativePath, CreateGeneratedMaterialDefinition());
        }

        /// <summary>
        /// Creates the authored orbit camera entity centered on the hero cube's motion path.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("MatrixRenderCamera");
            entity.LocalPosition = new float3(0f, 3.5f, 10.5f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = CreateYawPitchRollDegrees(0.0, -18.0, 0.0);
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100f,
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
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = new float3(0f, 0f, 2.5f),
                AutoYawSpeedRadians = 0.08f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity carrying the shared overlay kit plus the phase-status readout.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = new DemoDiscSceneUiKitFactory(AssetAuthoringService).CreateStandardSceneUi("MatrixRenderUi", "6. Matrix Render");
            Entity phaseStatusEntity = Core.Instance.EntityFactory.CreateChild(entity, "MatrixRenderPhaseStatus");
            phaseStatusEntity.LocalPosition = new float3(16f, 112f, 0f);
            phaseStatusEntity.Static = false;
            phaseStatusEntity.AddComponent(new TextComponent {
                Text = "Operation: Translation",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(1024, 56),
                FontScale = 1.5f,
                RenderOrder2D = 1,
            });
            phaseStatusEntity.AddComponent(new city.rendering.MatrixRenderPhaseStatusTextComponent());
            return entity;
        }

        /// <summary>
        /// Creates the dedicated key light so the animated cube reads clearly from a front-left three-quarter angle.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateKeyLightEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("MatrixRenderKeyLight");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(-3f, 5f, 4f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = CreateYawPitchRollDegrees(26.0, -28.0, 0.0);
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1.0f, 0.96f, 0.90f, 1.0f),
                Intensity = 1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f
            });
            return entity;
        }

        /// <summary>
        /// Creates the animated hero cube that cycles through each transform combination.
        /// </summary>
        /// <param name="cubeModel">Generated cube runtime model.</param>
        /// <param name="heroMaterial">Generated hero runtime material.</param>
        /// <returns>Live authored hero cube entity.</returns>
        Entity CreateHeroEntity(RuntimeModel cubeModel, RuntimeMaterial heroMaterial) {
            Entity entity = Core.Instance.EntityFactory.Create("HeroMotionCube");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = new float3(2f, 2f, 2f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = cubeModel,
                Materials = new[] { heroMaterial },
                RenderOrder3D = 0
            });
            entity.AddComponent(new city.rendering.MatrixRenderComponent {
                BaseLocalPosition = float3.Zero,
                MotionOffset = new float3(0f, 0f, 5f),
                BaseLocalScale = new float3(2f, 2f, 2f),
                ScaledLocalScale = new float3(4f, 1f, 2f),
                RotatedLocalOrientation = CreateYawPitchRollDegrees(0.0, 0.0, 18.0),
                PhaseDurationSeconds = 1.5d
            });
            return entity;
        }

        /// <summary>
        /// Creates the hero runtime material used while authoring the Matrix Render scene.
        /// </summary>
        /// <returns>Runtime material instance for the hero cube.</returns>
        RuntimeMaterial CreateRuntimeMaterial() {
            ShaderMaterialAsset materialAsset = new ShaderMaterialAsset {
                Id = HeroMaterialAssetId,
                ShaderAssetId = StandardShaderAssetId,
                VertexProgram = StandardVertexProgramName,
                PixelProgram = StandardPixelProgramName,
                Variant = MeshVariantName,
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };
            ShaderAsset shaderAsset = helengine.editor.EditorBuiltInShaderAssetLibrary.LoadShaderAsset(Core.Instance.RenderManager3D, StandardShaderSourceFileName);
            materialAsset.ConstantBuffers = new[] {
                new MaterialConstantBufferAsset {
                    Name = helengine.editor.StandardMaterialBaseColorDefaults.BaseColorBufferName,
                    Data = helengine.editor.StandardMaterialBaseColorDefaults.CreateConstantBufferData(ParseColor(HeroMaterialColor))
                }
            };

            RuntimeMaterial runtimeMaterial = Core.Instance.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(runtimeMaterial));
            return runtimeMaterial;
        }

        /// <summary>
        /// Creates the generated authored material definition for the hero material.
        /// </summary>
        /// <returns>Generated authored material definition.</returns>
        GeneratedMaterialAssetDefinition CreateGeneratedMaterialDefinition() {
            GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition();
            definition.MaterialAsset = new ShaderMaterialAsset {
                Id = HeroMaterialAssetId,
                RenderState = new MaterialRenderState(),
                CastsShadows = true,
                ReceivesShadows = true
            };

            GeneratedMaterialPlatformDefinition windowsSettings = definition.GetOrCreatePlatform("windows");
            windowsSettings.SchemaId = WindowsMaterialSchemaId;
            windowsSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            windowsSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            windowsSettings.SetFieldValue(TextureIdFieldId, string.Empty);
            windowsSettings.SetFieldValue(CastsShadowFieldId, "true");
            windowsSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            windowsSettings.SetFieldValue(BaseColorFieldId, HeroMaterialColor);
            windowsSettings.SetFieldValue("metallic", "0.0");
            windowsSettings.SetFieldValue("specular", "0.0");

            GeneratedMaterialPlatformDefinition ps2Settings = definition.GetOrCreatePlatform("ps2");
            ps2Settings.SchemaId = Ps2MaterialSchemaId;
            ps2Settings.SetFieldValue(AlphaModeFieldId, "opaque");
            ps2Settings.SetFieldValue(DoubleSidedFieldId, "false");
            ps2Settings.SetFieldValue(Ps2CastShadowsFieldId, "true");
            ps2Settings.SetFieldValue(VertexColorModeFieldId, "ignore");
            ps2Settings.SetFieldValue(BaseColorFieldId, HeroMaterialColor);

            GeneratedMaterialPlatformDefinition gameCubeSettings = definition.GetOrCreatePlatform("gamecube");
            gameCubeSettings.SchemaId = GameCubeMaterialSchemaId;
            gameCubeSettings.SetFieldValue(DoubleSidedFieldId, "false");
            gameCubeSettings.SetFieldValue(VertexColorModeFieldId, "ignore");
            gameCubeSettings.SetFieldValue(BaseColorFieldId, HeroMaterialColor);
            gameCubeSettings.SetFieldValue(LightingModeFieldId, "lit");

            GeneratedMaterialPlatformDefinition pspSettings = definition.GetOrCreatePlatform("psp");
            pspSettings.SchemaId = WindowsMaterialSchemaId;
            pspSettings.SetFieldValue(UseCustomShaderFieldId, "false");
            pspSettings.SetFieldValue(ShaderAssetIdFieldId, StandardShaderAssetId);
            pspSettings.SetFieldValue(TextureIdFieldId, string.Empty);
            pspSettings.SetFieldValue(CastsShadowFieldId, "true");
            pspSettings.SetFieldValue(ReceivesShadowFieldId, "true");
            pspSettings.SetFieldValue(BaseColorFieldId, HeroMaterialColor);
            return definition;
        }

        /// <summary>
        /// Creates one authored orientation from yaw, pitch, and roll in degrees.
        /// </summary>
        /// <param name="yawDegrees">Yaw in degrees.</param>
        /// <param name="pitchDegrees">Pitch in degrees.</param>
        /// <param name="rollDegrees">Roll in degrees.</param>
        /// <returns>Authored orientation quaternion.</returns>
        static float4 CreateYawPitchRollDegrees(double yawDegrees, double pitchDegrees, double rollDegrees) {
            float4.CreateFromYawPitchRoll(
                (float)(yawDegrees * (Math.PI / 180.0)),
                (float)(pitchDegrees * (Math.PI / 180.0)),
                (float)(rollDegrees * (Math.PI / 180.0)),
                out float4 orientation);
            return orientation;
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
        /// Resolves the editor font that backs the phase-status readout during live authoring.
        /// </summary>
        /// <returns>Editor font asset.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the Matrix Render scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
