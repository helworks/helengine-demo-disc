using city.menu;

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
        /// Initializes one directional-shadow plaza scene factory.
        /// </summary>
        public DirectionalShadowPlazaSceneFactory() { }

        /// <summary>
        /// Creates the live-authored directional-shadow plaza scene definition that the editor save pipeline will serialize.
        /// </summary>
        /// <param name="planeModel">Generated plane runtime model used by the ground mesh.</param>
        /// <param name="cubeModel">Generated cube runtime model used by the buildings and shadow mast.</param>
        /// <param name="sphereModel">Generated sphere runtime model used by the orbiting hero landmark.</param>
        /// <param name="standardMaterial">Runtime standard material assigned to every plaza mesh.</param>
        /// <returns>Live-authored scene definition for the directional-shadow plaza showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(
            RuntimeModel planeModel,
            RuntimeModel cubeModel,
            RuntimeModel sphereModel,
            RuntimeMaterial standardMaterial) {
            if (planeModel == null) {
                throw new ArgumentNullException(nameof(planeModel));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (sphereModel == null) {
                throw new ArgumentNullException(nameof(sphereModel));
            } else if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            }

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateFpsEntity(),
                    CreateDirectionalLightEntity(),
                    CreateGroundEntity(planeModel, standardMaterial),
                    CreateShadowMastEntity(cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaWestTower", new float3(-16f, 7f, -9f), new float3(6f, 14f, 6f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaCentralTower", new float3(0f, 9f, -12f), new float3(7f, 18f, 7f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaEastTower", new float3(15f, 6f, -7f), new float3(5f, 12f, 5f), cubeModel, standardMaterial),
                    CreateOrbitHeroEntity(sphereModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaSouthwestBlock", new float3(-15f, 3f, 12f), new float3(6f, 6f, 6f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaSouthCentralBlock", new float3(-4f, 2.5f, 14f), new float3(5f, 5f, 5f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaNortheastBlock", new float3(13f, 2f, 11f), new float3(4f, 4f, 4f), cubeModel, standardMaterial),
                    CreateBuildingEntity("DirectionalShadowPlazaMidriseBlock", new float3(8f, 3.5f, 2f), new float3(5f, 7f, 5f), cubeModel, standardMaterial)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the live directional-shadow plaza scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.28f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("DirectionalShadowPlazaCamera");
            entity.LocalPosition = new float3(0f, 24f, 64f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = SceneObjectsLayerMask,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 200f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 80f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new gameplay.rendering.DirectionalShadowCameraOrbitComponent {
                OrbitCenter = new float3(0f, 0f, 0f),
                OrbitRadius = 64f,
                OrbitHeight = 24f,
                BaseAngleRadians = 0f,
                AngularSpeedRadians = 0.07f,
                LookDownPitchRadians = -0.28f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored FPS overlay entity for the live directional-shadow plaza scene.
        /// </summary>
        /// <returns>Live authored FPS overlay entity.</returns>
        Entity CreateFpsEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("DirectionalShadowPlazaFps");
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont()
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored directional light entity for the live directional-shadow plaza scene.
        /// </summary>
        /// <returns>Live authored directional light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.72f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("DirectionalShadowPlazaSun");
            entity.LocalPosition = new float3(0f, 18f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.95f, 0.9f, 1f),
                Intensity = 1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 80f
            });
            entity.AddComponent(new gameplay.rendering.DirectionalShadowSunSweepComponent {
                MinYawRadians = -0.18f,
                MaxYawRadians = 0.18f,
                PitchRadians = -0.72f,
                SweepSpeedRadians = 0.05f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored ground receiver mesh for the live directional-shadow plaza scene.
        /// </summary>
        /// <param name="model">Runtime plane model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity("DirectionalShadowPlazaGround", new float3(0f, 0f, 0f), new float3(48f, 1f, 48f), model, material);
        }

        /// <summary>
        /// Creates the authored shadow mast mesh for the live directional-shadow plaza scene.
        /// </summary>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored shadow mast entity.</returns>
        Entity CreateShadowMastEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity("DirectionalShadowPlazaShadowMast", new float3(-9f, 7f, 4f), new float3(1.4f, 14f, 1.4f), model, material);
        }

        /// <summary>
        /// Creates one live authored building entity for the directional-shadow plaza scene.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored building entity.</returns>
        Entity CreateBuildingEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity(name, localPosition, localScale, model, material);
        }

        /// <summary>
        /// Creates the authored orbiting sphere landmark for the live directional-shadow plaza scene.
        /// </summary>
        /// <param name="model">Runtime sphere model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored orbit hero entity.</returns>
        Entity CreateOrbitHeroEntity(RuntimeModel model, RuntimeMaterial material) {
            Entity entity = CreateMeshEntity("DirectionalShadowPlazaHeroSphere", new float3(0f, 2.5f, 10f), new float3(3f, 3f, 3f), model, material);
            entity.AddComponent(new gameplay.rendering.DirectionalShadowOrbitComponent {
                OrbitCenter = new float3(0f, 0f, 0f),
                OrbitRadius = 10f,
                OrbitHeight = 2.5f,
                BaseAngleRadians = 0.15f,
                AngularSpeedRadians = -0.18f
            });
            return entity;
        }

        /// <summary>
        /// Creates one shared mesh entity for the directional-shadow plaza showcase.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime model assigned to the mesh.</param>
        /// <param name="material">Runtime material assigned to the mesh.</param>
        /// <returns>Live authored mesh entity.</returns>
        Entity CreateMeshEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (model == null) {
                throw new ArgumentNullException(nameof(model));
            } else if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Material = material,
                RenderOrder3D = 0
            });
            return entity;
        }

        /// <summary>
        /// Resolves the editor font used by the live camera entity.
        /// </summary>
        /// <returns>Loaded default editor font.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance == null || Core.Instance.DefaultFontAsset == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the scene can be generated.");
            }

            return Core.Instance.DefaultFontAsset;
        }
    }
}
