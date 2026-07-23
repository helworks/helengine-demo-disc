using helengine;

namespace city.game {
#if DESKTOP_PLATFORM
    /// <summary>
    /// Toggles one Windows-only physics-bounds debug overlay that draws wireframe bounds around supported collider volumes.
    /// </summary>
    public sealed class TiltTrialPhysicsBoundsDebugDrawComponent : UpdateComponent {
        const string WindowsPlatformId = "windows";
        const string StandardShaderAssetId = "ForwardStandardShader";
        const string StandardVertexProgramName = "ForwardStandardShader.vs";
        const string StandardPixelProgramName = "ForwardStandardShader.ps";
        const string StandardShaderVariantName = "Mesh";
        const string BaseColorBufferName = "BaseColorBuffer";
        const string EmissiveColorBufferName = "EmissiveColorBuffer";
        const float BoundsPadding = 0.02f;
        const Keys ToggleKey = Keys.F3;

        readonly List<BoundsVisualRecord> VisualRecords = new List<BoundsVisualRecord>();

        RuntimeModel boundsWireframeModel;
        RuntimeMaterial edgeMaterial;
        bool wasToggleKeyDown;
        bool visible;

        /// <summary>
        /// Gets whether the collider-bounds overlay is currently visible.
        /// </summary>
        public bool Visible => visible;

        /// <summary>
        /// Ensures the host entity can own dynamically created visual children.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Children == null) {
                entity.InitChildren();
            }
        }

        /// <summary>
        /// Updates the visibility toggle and synchronizes visual bounds proxies while the overlay is enabled.
        /// </summary>
        public override void Update() {
            Core core = Core.Instance;
            if (core == null) {
                return;
            }

            if (!IsSupportedWindowsRuntime(core)) {
                if (visible) {
                    visible = false;
                    SetAllVisualsEnabled(false);
                }

                return;
            }

            bool isToggleKeyDown = core.Input.IsKeyDown(ToggleKey);
            if (core.Input.WasKeyPressed(ToggleKey) || (isToggleKeyDown && !wasToggleKeyDown)) {
                visible = !visible;
                SetAllVisualsEnabled(visible);
            }
            wasToggleKeyDown = isToggleKeyDown;

            if (!visible) {
                return;
            }

            EnsureResources(core);
            RefreshVisuals(core);
        }

        /// <summary>
        /// Releases owned visuals before the component detaches from its host entity.
        /// </summary>
        /// <param name="entity">Detaching parent entity.</param>
        public override void ComponentRemoved(Entity entity) {
            Cleanup();
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Releases transient runtime resources created by the overlay.
        /// </summary>
        public override void Dispose() {
            Cleanup();
            base.Dispose();
        }

        void RefreshVisuals(Core core) {
            SceneManager sceneManager = core.SceneManager;
            if (sceneManager == null) {
                RemoveAllVisuals();
                return;
            }

            List<Entity> activeSourceEntities = new List<Entity>();
            IReadOnlyList<LoadedSceneRecord> loadedScenes = sceneManager.LoadedScenes;
            for (int sceneIndex = 0; sceneIndex < loadedScenes.Count; sceneIndex++) {
                IReadOnlyList<Entity> rootEntities = loadedScenes[sceneIndex].RootEntities;
                for (int rootIndex = 0; rootIndex < rootEntities.Count; rootIndex++) {
                    CollectColliderBounds(rootEntities[rootIndex], activeSourceEntities);
                }
            }

            if (VisualRecords.Count != activeSourceEntities.Count) {
                for (int index = VisualRecords.Count - 1; index >= 0; index--) {
                    if (!ContainsEntity(activeSourceEntities, VisualRecords[index].SourceEntity)) {
                        RemoveVisualRecordAt(index);
                    }
                }
            }
        }

        void CollectColliderBounds(Entity entity, List<Entity> activeSourceEntities) {
            if (entity == null || activeSourceEntities == null) {
                return;
            }

            if (!ReferenceEquals(entity, Parent) && TryComputeColliderBounds(entity, out ushort layerMask, out float3 min, out float3 max)) {
                if (!ContainsEntity(activeSourceEntities, entity)) {
                    activeSourceEntities.Add(entity);
                }

                BoundsVisualRecord visualRecord = EnsureVisualRecord(entity);
                UpdateVisualRecord(visualRecord, layerMask, min, max);
            }

            List<Entity> children = entity.Children;
            if (children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < children.Count; childIndex++) {
                CollectColliderBounds(children[childIndex], activeSourceEntities);
            }
        }

        bool TryComputeColliderBounds(Entity entity, out ushort layerMask, out float3 min, out float3 max) {
            layerMask = 0;
            min = float3.Zero;
            max = float3.Zero;
            if (entity == null || entity.Components == null) {
                return false;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TiltTrialPhysicsDebugBoxBoundsComponent boxBounds) {
                    float3 halfExtents = CreateBoxHalfExtents(boxBounds.Size);
                    float3 axisAlignedHalfExtents = CreateBoxAxisAlignedHalfExtents(halfExtents, entity.Orientation) + CreatePaddingVector();
                    layerMask = entity.LayerMask;
                    min = entity.Position - axisAlignedHalfExtents;
                    max = entity.Position + axisAlignedHalfExtents;
                    return true;
                }

                if (entity.Components[componentIndex] is TiltTrialPhysicsDebugSphereBoundsComponent sphereBounds) {
                    float sphereBoundsRadius = CreateScaledSphereRadius(sphereBounds.Radius, entity.LocalScale) + BoundsPadding;
                    float3 radiusVector = new float3(sphereBoundsRadius, sphereBoundsRadius, sphereBoundsRadius);
                    layerMask = entity.LayerMask;
                    min = entity.Position - radiusVector;
                    max = entity.Position + radiusVector;
                    return true;
                }
            }

            return false;
        }

        BoundsVisualRecord EnsureVisualRecord(Entity sourceEntity) {
            BoundsVisualRecord existingRecord = FindVisualRecord(sourceEntity);
            if (existingRecord != null) {
                existingRecord.VisualEntity.Enabled = true;
                return existingRecord;
            }

            Entity visualEntity = Core.Instance.EntityFactory.Create("PhysicsBoundsDebugWireframe");
            visualEntity.InitComponents();
            visualEntity.LayerMask = sourceEntity != null ? sourceEntity.LayerMask : (ushort)0b00000001;
            visualEntity.LocalPosition = float3.Zero;
            visualEntity.LocalScale = float3.One;
            visualEntity.LocalOrientation = float4.Identity;
            visualEntity.AddComponent(new MeshComponent {
                Model = boundsWireframeModel,
                Materials = new RuntimeMaterial[] { edgeMaterial },
                RenderOrder3D = 250
            });
            Parent.AddChild(visualEntity);

            BoundsVisualRecord createdRecord = new BoundsVisualRecord(sourceEntity, visualEntity);
            VisualRecords.Add(createdRecord);
            return createdRecord;
        }

        void UpdateVisualRecord(BoundsVisualRecord visualRecord, ushort layerMask, float3 min, float3 max) {
            if (visualRecord == null || visualRecord.VisualEntity == null) {
                return;
            }

            Entity visualEntity = visualRecord.VisualEntity;
            visualEntity.LayerMask = layerMask;
            visualEntity.LocalPosition = (min + max) * 0.5f;
            visualEntity.LocalScale = max - min;
            visualEntity.LocalOrientation = float4.Identity;
            visualEntity.Enabled = true;
        }

        void EnsureResources(Core core) {
            if (boundsWireframeModel == null) {
                boundsWireframeModel = core.RenderManager3D.BuildModelFromRaw(CreateBoundsWireframeModelAsset());
                boundsWireframeModel.SetSubmeshes(new[] {
                    new RuntimeSubmesh {
                        MaterialSlotName = string.Empty,
                        IndexStart = 0,
                        IndexCount = 24,
                        PrimitiveTopology = ModelPrimitiveTopology.LineList
                    }
                });
            }

            if (edgeMaterial != null) {
                return;
            }

            ContentManager contentManager = core.ContentManager;
            if (contentManager == null) {
                throw new InvalidOperationException("Physics bounds debug drawing requires an initialized content manager.");
            }
            ShaderRuntimeContentRegistration.Register(contentManager);
            IShaderCompileTargetProvider shaderTargetProvider = core.RenderManager3D as IShaderCompileTargetProvider;
            if (shaderTargetProvider == null) {
                throw new InvalidOperationException("Physics bounds debug drawing requires a shader-capable render manager.");
            }

            string shaderPackagePath = ShaderRuntimeMaterialLoader.ResolveShaderPackagePath(StandardShaderAssetId, shaderTargetProvider.ShaderCompileTarget);
            ShaderAsset shaderAsset = contentManager.Load<ShaderAsset>(shaderPackagePath, ShaderRuntimeContentProcessorIds.ShaderAsset);
            ShaderMaterialAsset materialAsset = new ShaderMaterialAsset();
            materialAsset.Id = "city.debug.physics_bounds";
            materialAsset.ShaderAssetId = shaderAsset.Id;
            materialAsset.VertexProgram = StandardVertexProgramName;
            materialAsset.PixelProgram = StandardPixelProgramName;
            materialAsset.Variant = StandardShaderVariantName;
            materialAsset.RenderState = new MaterialRenderState();
            materialAsset.RenderState.CullMode = MaterialCullMode.None;
            materialAsset.CastsShadows = false;
            materialAsset.ReceivesShadows = false;

            MaterialConstantBufferAsset[] constantBuffers = new MaterialConstantBufferAsset[5];

            MaterialConstantBufferAsset baseColorBuffer = new MaterialConstantBufferAsset();
            baseColorBuffer.Name = BaseColorBufferName;
            baseColorBuffer.Data = CreateFloat4ConstantBufferData(new float4(0.12f, 0.98f, 0.72f, 1f));
            constantBuffers[0] = baseColorBuffer;

            MaterialConstantBufferAsset emissiveColorBuffer = new MaterialConstantBufferAsset();
            emissiveColorBuffer.Name = EmissiveColorBufferName;
            emissiveColorBuffer.Data = CreateFloat4ConstantBufferData(new float4(0.12f, 0.98f, 0.72f, 0.75f));
            constantBuffers[1] = emissiveColorBuffer;

            MaterialConstantBufferAsset roughnessBuffer = new MaterialConstantBufferAsset();
            roughnessBuffer.Name = StandardMaterialRoughnessDefaults.RoughnessBufferName;
            roughnessBuffer.Data = StandardMaterialRoughnessDefaults.CreateConstantBufferData(0.2f);
            constantBuffers[2] = roughnessBuffer;

            MaterialConstantBufferAsset metallicBuffer = new MaterialConstantBufferAsset();
            metallicBuffer.Name = StandardMaterialMetallicDefaults.MetallicBufferName;
            metallicBuffer.Data = StandardMaterialMetallicDefaults.CreateConstantBufferData(0f);
            constantBuffers[3] = metallicBuffer;

            MaterialConstantBufferAsset specularBuffer = new MaterialConstantBufferAsset();
            specularBuffer.Name = StandardMaterialSpecularDefaults.SpecularBufferName;
            specularBuffer.Data = StandardMaterialSpecularDefaults.CreateConstantBufferData(0f);
            constantBuffers[4] = specularBuffer;

            materialAsset.ConstantBuffers = constantBuffers;

            edgeMaterial = core.RenderManager3D.BuildMaterialFromRaw(materialAsset, shaderAsset);
            StandardMaterialTextureBindingDefaults.Apply(ShaderRuntimeMaterialAccess.Require(edgeMaterial));
        }

        bool IsSupportedWindowsRuntime(Core core) {
            return core != null
                && core.PlatformInfo != null
                && string.Equals(core.PlatformInfo.Name, WindowsPlatformId, StringComparison.OrdinalIgnoreCase);
        }

        void SetAllVisualsEnabled(bool enabled) {
            for (int index = 0; index < VisualRecords.Count; index++) {
                BoundsVisualRecord visualRecord = VisualRecords[index];
                if (visualRecord == null || visualRecord.VisualEntity == null) {
                    continue;
                }

                visualRecord.VisualEntity.Enabled = enabled;
            }
        }

        void RemoveAllVisuals() {
            for (int index = VisualRecords.Count - 1; index >= 0; index--) {
                RemoveVisualRecordAt(index);
            }
        }

        void Cleanup() {
            RemoveAllVisuals();
            wasToggleKeyDown = false;

            Core core = Core.Instance;
            if (core != null && core.RenderManager3D != null) {
                if (edgeMaterial != null) {
                    core.RenderManager3D.ReleaseMaterial(edgeMaterial);
                    edgeMaterial = null;
                }

                if (boundsWireframeModel != null) {
                    core.RenderManager3D.ReleaseModel(boundsWireframeModel);
                    boundsWireframeModel = null;
                }
            } else {
                edgeMaterial = null;
                boundsWireframeModel = null;
            }
        }

        static byte[] CreateFloat4ConstantBufferData(float4 value) {
            byte[] data = new byte[16];
            Array.Copy(BitConverter.GetBytes(value.X), 0, data, 0, 4);
            Array.Copy(BitConverter.GetBytes(value.Y), 0, data, 4, 4);
            Array.Copy(BitConverter.GetBytes(value.Z), 0, data, 8, 4);
            Array.Copy(BitConverter.GetBytes(value.W), 0, data, 12, 4);
            return data;
        }

        static float3 CreatePaddingVector() {
            return new float3(BoundsPadding, BoundsPadding, BoundsPadding);
        }

        static ModelAsset CreateBoundsWireframeModelAsset() {
            return new ModelAsset {
                Positions = new[] {
                    new float3(-0.5f, -0.5f, -0.5f),
                    new float3(0.5f, -0.5f, -0.5f),
                    new float3(0.5f, 0.5f, -0.5f),
                    new float3(-0.5f, 0.5f, -0.5f),
                    new float3(-0.5f, -0.5f, 0.5f),
                    new float3(0.5f, -0.5f, 0.5f),
                    new float3(0.5f, 0.5f, 0.5f),
                    new float3(-0.5f, 0.5f, 0.5f)
                },
                Normals = new[] {
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f),
                    new float3(0f, 1f, 0f)
                },
                TexCoords = new[] {
                    new float2(0f, 0f),
                    new float2(0f, 0f),
                    new float2(0f, 0f),
                    new float2(0f, 0f),
                    new float2(0f, 0f),
                    new float2(0f, 0f),
                    new float2(0f, 0f),
                    new float2(0f, 0f)
                },
                Indices16 = new ushort[] {
                    0, 1, 1, 2, 2, 3, 3, 0,
                    4, 5, 5, 6, 6, 7, 7, 4,
                    0, 4, 1, 5, 2, 6, 3, 7
                }
            };
        }

        static float3 CreateBoxHalfExtents(float3 size) {
            return new float3(
                Math.Abs(size.X) * 0.5f,
                Math.Abs(size.Y) * 0.5f,
                Math.Abs(size.Z) * 0.5f);
        }

        static float3 CreateBoxAxisAlignedHalfExtents(float3 halfExtents, float4 orientation) {
            float3 axisX = float4.RotateVector(new float3(1f, 0f, 0f), orientation);
            float3 axisY = float4.RotateVector(new float3(0f, 1f, 0f), orientation);
            float3 axisZ = float4.RotateVector(new float3(0f, 0f, 1f), orientation);
            return new float3(
                (Math.Abs(axisX.X) * halfExtents.X) + (Math.Abs(axisY.X) * halfExtents.Y) + (Math.Abs(axisZ.X) * halfExtents.Z),
                (Math.Abs(axisX.Y) * halfExtents.X) + (Math.Abs(axisY.Y) * halfExtents.Y) + (Math.Abs(axisZ.Y) * halfExtents.Z),
                (Math.Abs(axisX.Z) * halfExtents.X) + (Math.Abs(axisY.Z) * halfExtents.Y) + (Math.Abs(axisZ.Z) * halfExtents.Z));
        }

        static float CreateScaledSphereRadius(float radius, float3 scale) {
            float maximumScale = Math.Max(Math.Abs(scale.X), Math.Max(Math.Abs(scale.Y), Math.Abs(scale.Z)));
            return radius * maximumScale;
        }

        static bool ContainsEntity(List<Entity> entities, Entity entity) {
            if (entities == null || entity == null) {
                return false;
            }

            for (int index = 0; index < entities.Count; index++) {
                if (ReferenceEquals(entities[index], entity)) {
                    return true;
                }
            }

            return false;
        }

        BoundsVisualRecord FindVisualRecord(Entity sourceEntity) {
            if (sourceEntity == null) {
                return null;
            }

            for (int index = 0; index < VisualRecords.Count; index++) {
                if (ReferenceEquals(VisualRecords[index].SourceEntity, sourceEntity)) {
                    return VisualRecords[index];
                }
            }

            return null;
        }

        void RemoveVisualRecordAt(int index) {
            if (index < 0 || index >= VisualRecords.Count) {
                return;
            }

            BoundsVisualRecord visualRecord = VisualRecords[index];
            VisualRecords.RemoveAt(index);
            if (visualRecord != null && visualRecord.VisualEntity != null) {
                visualRecord.VisualEntity.Dispose();
            }
        }

        sealed class BoundsVisualRecord {
            public BoundsVisualRecord(Entity sourceEntity, Entity visualEntity) {
                SourceEntity = sourceEntity;
                VisualEntity = visualEntity;
            }

            public Entity SourceEntity { get; }

            public Entity VisualEntity { get; }
        }
    }
#else
    /// <summary>
    /// Preserves the serialized Windows-only bounds-debug component reference on non-desktop builds without retaining its shader-backed diagnostic implementation.
    /// </summary>
    public sealed class TiltTrialPhysicsBoundsDebugDrawComponent : UpdateComponent {
        /// <summary>
        /// Gets whether the Windows-only diagnostic overlay is visible on this runtime.
        /// </summary>
        public bool Visible => false;
    }
#endif
}
