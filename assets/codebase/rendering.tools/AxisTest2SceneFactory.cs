using System.Globalization;
using System.Text;
using city.menu;
using gameplay.rendering;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the live-authored axis-test-2 scene and its generated directional-light arrow model.
    /// </summary>
    public sealed class AxisTest2SceneFactory {
        /// <summary>
        /// Stable scene id used by the generated axis-test-2 showcase.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.AxisTest2SceneId;

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
        /// Stable project-relative path to the imported directional-light arrow model source.
        /// </summary>
        const string ArrowModelRelativePath = "models/rendering/axis_test/directional_light_arrow.obj";

        /// <summary>
        /// Stable project-relative path to the marker material settings used by the directional-light arrow.
        /// </summary>
        const string MarkerMaterialRelativePath = "materials/rendering/axis_test/Marker.hasset";

        /// <summary>
        /// Stable material paths used by the axis-test-2 scene.
        /// </summary>
        static readonly string[] AxisMaterialRelativePaths = {
            "materials/rendering/axis_test/X.hasset",
            "materials/rendering/axis_test/Y.hasset",
            "materials/rendering/axis_test/Z.hasset",
            "materials/rendering/axis_test/Ground.hasset",
            "materials/rendering/axis_test/Marker.hasset"
        };

        /// <summary>
        /// World-space position used to keep the directional-light arrow centered in the authored camera view.
        /// </summary>
        static readonly float3 ArrowRigLocalPosition = new float3(5f, 6f, 5f);

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
        /// Initializes one axis-test-2 scene factory.
        /// </summary>
        public AxisTest2SceneFactory() { }

        /// <summary>
        /// Creates the live-authored axis-test-2 scene definition.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative project root path used to resolve generated prompt icons.</param>
        /// <param name="cubeModel">Generated cube runtime model assigned to the authored mesh entities.</param>
        /// <param name="arrowModel">Runtime model used by the directional-light arrow.</param>
        /// <param name="axisMaterials">Loaded runtime materials ordered as X, Y, Z, ground, and marker.</param>
        /// <returns>Live-authored scene definition for the axis-test-2 showcase.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel cubeModel, RuntimeModel arrowModel, RuntimeMaterial[] axisMaterials) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (cubeModel == null) {
                throw new ArgumentNullException(nameof(cubeModel));
            } else if (arrowModel == null) {
                throw new ArgumentNullException(nameof(arrowModel));
            } else if (axisMaterials == null) {
                throw new ArgumentNullException(nameof(axisMaterials));
            } else if (axisMaterials.Length != AxisMaterialRelativePaths.Length) {
                throw new ArgumentException("Axis-test generation requires five runtime materials.", nameof(axisMaterials));
            }

            FontAsset instructionFont = ResolveRequiredEditorFont();
            DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
            Entity cameraEntity = CreateCameraEntity();
            Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset(),
                NintendoDsScene = new GeneratedDsSceneDefinition {
                    UseDefaultBottomOverlay = true,
                    BottomScreenRootEntities = instructionOverlayFactory.CreateNintendoDsBottomInstructionRoots(instructionFont)
                },
                RootEntities = new[] {
                    cameraEntity,
                    instructionOverlayEntity,
                    CreateUiEntity(),
                    CreateDirectionalLightRigEntity(arrowModel, axisMaterials[4]),
                    CreateFloorEntity(cubeModel, axisMaterials[3]),
                    CreateGroundEntity(cubeModel, axisMaterials[3]),
                    CreateXAxisEntity(cubeModel, axisMaterials[0]),
                    CreateYAxisEntity(cubeModel, axisMaterials[1]),
                    CreateZAxisEntity(cubeModel, axisMaterials[2]),
                    CreateOriginMarkerEntity(cubeModel, axisMaterials[4]),
                    CreateXAxisMarkerEntity(cubeModel, axisMaterials[4]),
                    CreateYAxisMarkerEntity(cubeModel, axisMaterials[4]),
                    CreateZAxisMarkerEntity(cubeModel, axisMaterials[4])
                }
            };
        }

        /// <summary>
        /// Creates the generated directional-light arrow runtime model used by the live-authored axis-test-2 scene.
        /// </summary>
        /// <returns>Runtime directional-light arrow model.</returns>
        public RuntimeModel CreateArrowRuntimeModel() {
            return Core.Instance.RenderManager3D.BuildModelFromRaw(CreateArrowModelAsset());
        }

        /// <summary>
        /// Creates the authored camera entity for the live axis-test-2 scene.
        /// </summary>
        /// <returns>Live authored camera entity.</returns>
        Entity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)(Math.PI * 0.5), 0f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("AxisTest2Camera");
            entity.LocalPosition = new float3(30f, 6f, 5f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = SceneObjectsLayerMask,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 64f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 32f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.rendering.DemoDiscOrbitCameraComponent {
                OrbitCenter = float3.Zero,
                AutoYawSpeedRadians = 0.08f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root entity for the live axis-test-2 scene.
        /// </summary>
        /// <returns>Live authored UI entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("AxisTest2Ui");
            entity.LayerMask = SceneObjectsLayerMask;
            entity.AddComponent(new FPSComponent {
                Font = ResolveRequiredEditorFont(),
                FontScale = 2f
            });
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            entity.AddComponent(new DemoDiscLightToggleComponent());
            DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory();
            lightIndicatorOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont());
            return entity;
        }

        /// <summary>
        /// Creates the authored directional-light rig entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="arrowModel">Runtime model used by the directional-light arrow.</param>
        /// <param name="markerMaterial">Runtime marker material used by the directional-light arrow.</param>
        /// <returns>Live authored directional-light rig entity.</returns>
        Entity CreateDirectionalLightRigEntity(RuntimeModel arrowModel, RuntimeMaterial markerMaterial) {
            if (arrowModel == null) {
                throw new ArgumentNullException(nameof(arrowModel));
            } else if (markerMaterial == null) {
                throw new ArgumentNullException(nameof(markerMaterial));
            }

            Entity entity = Core.Instance.EntityFactory.Create("AxisTest2SunRig");
            entity.LayerMask = SceneObjectsLayerMask;
            entity.LocalPosition = ArrowRigLocalPosition;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddChild(CreateDirectionalLightArrowEntity(arrowModel, markerMaterial));
            return entity;
        }

        /// <summary>
        /// Creates the authored directional-light arrow entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="arrowModel">Runtime model used by the arrow mesh.</param>
        /// <param name="markerMaterial">Runtime marker material used by the arrow mesh.</param>
        /// <returns>Live authored directional-light arrow entity.</returns>
        Entity CreateDirectionalLightArrowEntity(RuntimeModel arrowModel, RuntimeMaterial markerMaterial) {
            if (arrowModel == null) {
                throw new ArgumentNullException(nameof(arrowModel));
            } else if (markerMaterial == null) {
                throw new ArgumentNullException(nameof(markerMaterial));
            }

            Entity entity = Core.Instance.EntityFactory.Create("AxisTest2SunArrow");
            entity.LayerMask = SceneObjectsLayerMask;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = new float3(ArrowVisualScale, ArrowVisualScale, ArrowVisualScale);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = arrowModel,
                Materials = new[] { markerMaterial },
                RenderOrder3D = 0
            });
            ApplyArrowMeshAssetReferences(entity);
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 1f, 1f, 1f),
                Intensity = 1.2f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 1f,
                ShadowDistance = 32f
            });
            entity.AddComponent(new gameplay.rendering.AxisRotationComponent {
                Axis = new float3(1f, 0f, 0f),
                AngularSpeedRadiansPerSecond = ArrowAngularSpeedRadians
            });
            return entity;
        }

        /// <summary>
        /// Stores the stable imported arrow-model and marker-material references required by scene serialization.
        /// </summary>
        /// <param name="entity">Arrow entity that owns the generated mesh component.</param>
        void ApplyArrowMeshAssetReferences(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            MeshComponent meshComponent = FindRequiredComponent<MeshComponent>(entity);
            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(meshComponent, MeshModelReferenceName, global::helengine.SceneAssetReferenceFactory.CreateFileSystemModel(ArrowModelRelativePath));
            saveComponent.SetAssetReference(meshComponent, MeshMaterialReferenceName, global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(MarkerMaterialRelativePath));
        }

        /// <summary>
        /// Creates the authored X-axis mesh entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime X-axis material.</param>
        /// <returns>Live authored X-axis entity.</returns>
        Entity CreateXAxisEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2XAxis", new float3(5f, 0f, 0f), new float3(10f, 0.5f, 0.5f), model, material);
        }

        /// <summary>
        /// Creates the authored Y-axis mesh entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime Y-axis material.</param>
        /// <returns>Live authored Y-axis entity.</returns>
        Entity CreateYAxisEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2YAxis", new float3(0f, 5f, 0f), new float3(0.5f, 10f, 0.5f), model, material);
        }

        /// <summary>
        /// Creates the authored Z-axis mesh entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime Z-axis material.</param>
        /// <returns>Live authored Z-axis entity.</returns>
        Entity CreateZAxisEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2ZAxis", new float3(0f, 0f, 5f), new float3(0.5f, 0.5f, 10f), model, material);
        }

        /// <summary>
        /// Creates the authored floor entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime ground material.</param>
        /// <returns>Live authored floor entity.</returns>
        Entity CreateFloorEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2Floor", new float3(5f, -5f, 5f), new float3(14f, 0.5f, 14f), model, material);
        }

        /// <summary>
        /// Creates the authored ground entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime ground material.</param>
        /// <returns>Live authored ground entity.</returns>
        Entity CreateGroundEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2Ground", new float3(5f, 5f, -5f), new float3(14f, 14f, 0.5f), model, material);
        }

        /// <summary>
        /// Creates the authored origin marker entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime marker material.</param>
        /// <returns>Live authored origin marker entity.</returns>
        Entity CreateOriginMarkerEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2OriginMarker", float3.Zero, new float3(0.5f, 0.5f, 0.5f), model, material);
        }

        /// <summary>
        /// Creates the authored X-axis marker entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime marker material.</param>
        /// <returns>Live authored X-axis marker entity.</returns>
        Entity CreateXAxisMarkerEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2XAxisMarker", new float3(5f, 0f, 0f), new float3(0.35f, 0.35f, 0.35f), model, material);
        }

        /// <summary>
        /// Creates the authored Y-axis marker entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime marker material.</param>
        /// <returns>Live authored Y-axis marker entity.</returns>
        Entity CreateYAxisMarkerEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2YAxisMarker", new float3(0f, 5f, 0f), new float3(0.35f, 0.35f, 0.35f), model, material);
        }

        /// <summary>
        /// Creates the authored Z-axis marker entity for the live axis-test-2 scene.
        /// </summary>
        /// <param name="model">Generated cube runtime model used by the mesh.</param>
        /// <param name="material">Runtime marker material.</param>
        /// <returns>Live authored Z-axis marker entity.</returns>
        Entity CreateZAxisMarkerEntity(RuntimeModel model, RuntimeMaterial material) {
            return CreateAxisEntity("AxisTest2ZAxisMarker", new float3(0f, 0f, 5f), new float3(0.35f, 0.35f, 0.35f), model, material);
        }

        /// <summary>
        /// Creates one shared mesh entity for the axis-test-2 showcase.
        /// </summary>
        /// <param name="name">Stable entity name.</param>
        /// <param name="localPosition">Local position assigned to the entity.</param>
        /// <param name="localScale">Local scale assigned to the entity.</param>
        /// <param name="model">Runtime model assigned to the mesh.</param>
        /// <param name="material">Runtime material assigned to the mesh.</param>
        /// <returns>Live authored mesh entity.</returns>
        Entity CreateAxisEntity(string name, float3 localPosition, float3 localScale, RuntimeModel model, RuntimeMaterial material) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Entity name must be provided.", nameof(name));
            } else if (model == null) {
                throw new ArgumentNullException(nameof(model));
            } else if (material == null) {
                throw new ArgumentNullException(nameof(material));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = SceneObjectsLayerMask;
            entity.LocalPosition = localPosition;
            entity.LocalScale = localScale;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = model,
                Materials = new[] { material },
                RenderOrder3D = 0
            });
            return entity;
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
        /// Resolves the editor font used by the live camera entity.
        /// </summary>
        /// <returns>Loaded default editor font.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
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
        /// Resolves one required component from the supplied generated entity.
        /// </summary>
        /// <typeparam name="TComponent">Component type to resolve.</typeparam>
        /// <param name="entity">Entity whose component should be returned.</param>
        /// <returns>Attached component instance.</returns>
        TComponent FindRequiredComponent<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is TComponent component) {
                    return component;
                }
            }

            throw new InvalidOperationException($"Generated entity is missing required component '{typeof(TComponent).Name}'.");
        }
    }
}
