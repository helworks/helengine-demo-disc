using city.menu;
using gameplay.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical live-authored scene definition for the spotlight street-slice showcase.
    /// </summary>
    public sealed class SpotlightStreetSliceSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated spotlight street-slice asset.
        /// </summary>
        public const string SceneId = "scenes/rendering/spotlight_street_slice.helen";

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
        /// Stable save-state slot name used for serialized font references.
        /// </summary>
        const string FontReferenceName = "Font";

        /// <summary>
        /// Stable project-relative path to the imported lamppost model source.
        /// </summary>
        const string LamppostModelRelativePath = "models/Riemers/lamppost.x";

        /// <summary>
        /// Stable project-relative path to the imported racer model source.
        /// </summary>
        const string RacerModelRelativePath = "models/Riemers/racer.x";

        /// <summary>
        /// Stable project-relative material paths used by the imported racer model.
        /// </summary>
        static readonly string[] RacerMaterialRelativePaths = {
            "models/Riemers/racer/x3ds_mat_ruedas.hasset",
            "models/Riemers/racer/x3ds_mat_Material__0_3.hasset",
            "models/Riemers/racer/x3ds_mat_Material_1_2.hasset",
            "models/Riemers/racer/x3ds_mat_Material_2_1.hasset"
        };

        /// <summary>
        /// Placeholder font assigned during live authoring before the real generated editor-font reference is serialized.
        /// </summary>
        readonly FontAsset PlaceholderFont;

        /// <summary>
        /// Initializes one spotlight street-slice scene factory.
        /// </summary>
        public SpotlightStreetSliceSceneFactory() {
            PlaceholderFont = new FontAsset(
                new FontInfo("SpotlightStreetSlicePlaceholder", 16, 4f),
                new ManagedRuntimeTexture {
                    Width = 1,
                    Height = 1
                },
                new Dictionary<char, FontChar>(),
                16f,
                1,
                1);
        }

        /// <summary>
        /// Creates the live-authored spotlight street-slice scene definition that the editor save pipeline will serialize.
        /// </summary>
        /// <param name="planeModel">Generated plane runtime model used by the street mesh.</param>
        /// <param name="cubeModel">Generated cube runtime model used by the static street geometry.</param>
        /// <param name="standardMaterial">Runtime standard material used by the street and lamppost meshes.</param>
        /// <param name="lamppostModel">Runtime imported lamppost model.</param>
        /// <param name="racerModel">Runtime imported racer model.</param>
        /// <param name="racerMaterials">Runtime imported racer materials ordered by imported submesh slot.</param>
        /// <returns>Live-authored scene definition for the spotlight street-slice showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(
            RuntimeModel planeModel,
            RuntimeModel cubeModel,
            RuntimeMaterial standardMaterial,
            RuntimeModel lamppostModel,
            RuntimeModel racerModel,
            RuntimeMaterial[] racerMaterials) {
            if (planeModel == null) {
                throw new ArgumentNullException(nameof(planeModel));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (standardMaterial == null) {
                throw new ArgumentNullException(nameof(standardMaterial));
            } else if (lamppostModel == null) {
                throw new ArgumentNullException(nameof(lamppostModel));
            } else if (racerModel == null) {
                throw new ArgumentNullException(nameof(racerModel));
            } else if (racerMaterials == null) {
                throw new ArgumentNullException(nameof(racerMaterials));
            }

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    SceneId = RenderingSceneGenerator.SpotlightStreetSliceNintendoDsSceneId,
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = Array.Empty<Entity>()
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateFpsEntity(),
                    CreateSpotLightEntity(),
                    CreateStreetEntity(planeModel, standardMaterial),
                    CreateStreetEdgeEntity("SpotlightStreetSliceCurbLeft", new float3(-9f, 0.25f, 0f), new float3(1f, 0.5f, 28f), cubeModel, standardMaterial),
                    CreateStreetEdgeEntity("SpotlightStreetSliceCurbRight", new float3(9f, 0.25f, 0f), new float3(1f, 0.5f, 28f), cubeModel, standardMaterial),
                    CreateStreetEdgeEntity("SpotlightStreetSliceBackWall", new float3(0f, 6f, -12f), new float3(20f, 12f, 1f), cubeModel, standardMaterial),
                    CreateStreetEdgeEntity("SpotlightStreetSliceSideBlock", new float3(12f, 2.5f, 6f), new float3(4f, 5f, 8f), cubeModel, standardMaterial),
                    CreateImportedMeshEntity("SpotlightStreetSliceLamppost", new float3(-4f, 0f, -2f), new float3(2.2f, 2.2f, 2.2f), CreateYawOrientation(0.0), lamppostModel, LamppostModelRelativePath, new[] { standardMaterial }, Array.Empty<string>()),
                    CreateImportedMeshEntity("SpotlightStreetSliceRacer", new float3(1.8f, 0f, 2f), new float3(2.8f, 2.8f, 2.8f), CreateYawOrientation(-0.42), racerModel, RacerModelRelativePath, racerMaterials, RacerMaterialRelativePaths)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity for the live spotlight showcase scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.24f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("SpotlightStreetSliceCamera");
            entity.LocalPosition = new float3(0f, 12f, 28f);
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
                    new float4(0.015f, 0.015f, 0.03f, 1f),
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
                OrbitCenter = new float3(0f, 2f, 0f),
                OrbitRadius = 28f,
                OrbitHeight = 12f,
                BaseAngleRadians = 0f,
                AngularSpeedRadians = 0.05f,
                LookDownPitchRadians = -0.24f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
        }

        /// <summary>
        /// Creates the authored FPS overlay entity for the live spotlight street-slice scene.
        /// </summary>
        /// <returns>Live authored FPS overlay entity.</returns>
        Entity CreateFpsEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("SpotlightStreetSliceFps");
            FPSComponent fpsComponent = new FPSComponent {
                Font = PlaceholderFont
            };
            entity.AddComponent(fpsComponent);
            ApplyEditorFontReference(entity, fpsComponent);
            return entity;
        }

        /// <summary>
        /// Creates the authored spotlight entity for the live spotlight showcase scene.
        /// </summary>
        /// <returns>Live authored spotlight entity.</returns>
        Entity CreateSpotLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0.28f, -1.22f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("SpotlightStreetSliceLight");
            entity.LocalPosition = new float3(-3.2f, 9.5f, -1.4f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new SpotLightComponent {
                Color = new float4(1f, 0.95f, 0.84f, 1f),
                Range = 34f,
                InnerConeAngleDegrees = 22f,
                OuterConeAngleDegrees = 35f,
                Intensity = 1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored street mesh for the live spotlight showcase scene.
        /// </summary>
        /// <param name="model">Runtime plane model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored street entity.</returns>
        Entity CreateStreetEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity("SpotlightStreetSliceStreet", new float3(0f, -0.05f, 0f), new float3(20f, 1f, 28f), model, new[] { material });
        }

        /// <summary>
        /// Creates one supporting static street-edge mass for the live spotlight scene.
        /// </summary>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime cube model used by the mesh.</param>
        /// <param name="material">Runtime standard material used by the mesh.</param>
        /// <returns>Live authored street-edge entity.</returns>
        Entity CreateStreetEdgeEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            return CreateMeshEntity(name, localPosition, localScale, model, new[] { material });
        }

        /// <summary>
        /// Creates one imported model entity that uses one or more authored materials.
        /// </summary>
        /// <param name="name">Display name stored on the entity.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="localOrientation">Local orientation assigned to the entity.</param>
        /// <param name="model">Runtime imported model.</param>
        /// <param name="materials">Runtime materials assigned to the mesh in slot order.</param>
        /// <returns>Live authored imported-mesh entity.</returns>
        Entity CreateImportedMeshEntity(
            string name,
            float3 localPosition,
            float3 localScale,
            float4 localOrientation,
            RuntimeModel model,
            string modelRelativePath,
            RuntimeMaterial[] materials,
            string[] materialRelativePaths) {
            if (materials == null) {
                throw new ArgumentNullException(nameof(materials));
            } else if (string.IsNullOrWhiteSpace(modelRelativePath)) {
                throw new ArgumentException("Model path must be provided.", nameof(modelRelativePath));
            } else if (materialRelativePaths == null) {
                throw new ArgumentNullException(nameof(materialRelativePaths));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = localOrientation;
            MeshComponent meshComponent = new MeshComponent {
                Model = model,
                RenderOrder3D = 0
            };
            meshComponent.SetMaterials(materials);
            entity.AddComponent(meshComponent);
            ApplyImportedMeshAssetReferences(entity, meshComponent, modelRelativePath, materialRelativePaths);
            return entity;
        }

        /// <summary>
        /// Creates one shared mesh entity for the live spotlight showcase.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime model assigned to the mesh.</param>
        /// <param name="materials">Runtime materials assigned to the mesh.</param>
        /// <returns>Live authored mesh entity.</returns>
        Entity CreateMeshEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial[] materials) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (model == null) {
                throw new ArgumentNullException(nameof(model));
            } else if (materials == null) {
                throw new ArgumentNullException(nameof(materials));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = model,
                RenderOrder3D = 0
            };
            meshComponent.SetMaterials(materials);
            entity.AddComponent(meshComponent);
            return entity;
        }

        /// <summary>
        /// Stores the generated editor-font reference on the entity save state for the supplied FPS component.
        /// </summary>
        /// <param name="entity">Entity that owns the FPS component.</param>
        /// <param name="component">FPS component whose font reference should be stored.</param>
        void ApplyEditorFontReference(Entity entity, FPSComponent component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, FontReferenceName, DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

        /// <summary>
        /// Stores the stable imported model and optional material references required by scene serialization.
        /// </summary>
        /// <param name="entity">Entity that owns the imported mesh.</param>
        /// <param name="component">Mesh component whose save references should be stored.</param>
        /// <param name="modelRelativePath">Project-relative imported model path.</param>
        /// <param name="materialRelativePaths">Project-relative material paths ordered by mesh slot.</param>
        void ApplyImportedMeshAssetReferences(Entity entity, MeshComponent component, string modelRelativePath, string[] materialRelativePaths) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(modelRelativePath)) {
                throw new ArgumentException("Model path must be provided.", nameof(modelRelativePath));
            } else if (materialRelativePaths == null) {
                throw new ArgumentNullException(nameof(materialRelativePaths));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, MeshModelReferenceName, CreateFileReference(modelRelativePath));
            for (int materialIndex = 0; materialIndex < materialRelativePaths.Length; materialIndex++) {
                string materialRelativePath = materialRelativePaths[materialIndex];
                if (string.IsNullOrWhiteSpace(materialRelativePath)) {
                    throw new InvalidOperationException("Imported mesh material paths must be provided for every authored slot.");
                }

                saveComponent.SetAssetReference(component, BuildMaterialReferenceName(materialIndex), CreateFileReference(materialRelativePath));
            }
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Builds the stable mesh-material reference slot name for the supplied submesh index.
        /// </summary>
        /// <param name="materialIndex">Zero-based material slot index.</param>
        /// <returns>Stable material reference slot name.</returns>
        string BuildMaterialReferenceName(int materialIndex) {
            if (materialIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(materialIndex), "Material index must be non-negative.");
            }

            return materialIndex == 0
                ? MeshMaterialReferenceName
                : string.Concat(MeshMaterialReferenceName, "[", materialIndex.ToString(), "]");
        }

        /// <summary>
        /// Builds one file-system scene asset reference for the supplied project-relative asset path.
        /// </summary>
        /// <param name="relativePath">Project-relative asset path.</param>
        /// <returns>Scene asset reference that resolves through the project file system.</returns>
        SceneAssetReference CreateFileReference(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = relativePath,
                ProviderId = string.Empty,
                AssetId = string.Empty
            };
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



