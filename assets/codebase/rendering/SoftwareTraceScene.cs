using helengine;

namespace city.rendering {
    /// <summary>
    /// Loads owned CPU-readable model assets for software trace-scene construction.
    /// </summary>
    public interface ISoftwareModelAssetSource {
        /// <summary>
        /// Loads one owned raw model for a stable scene reference.
        /// </summary>
        /// <param name="reference">Stable scene asset reference.</param>
        /// <returns>An owned raw model asset that the caller must dispose.</returns>
        ModelAsset LoadOwned(SceneAssetReference reference);
    }

    /// <summary>
    /// Loads CPU-readable model assets through the normal content pipeline.
    /// </summary>
    public sealed class ContentSoftwareModelAssetSource : ISoftwareModelAssetSource {
        /// <summary>
        /// Content manager borrowed by this source for model loads.
        /// </summary>
        readonly ContentManager contentManager;

        /// <summary>
        /// Initializes one content-backed software model source.
        /// </summary>
        /// <param name="contentManager">Content manager used to resolve model references.</param>
        public ContentSoftwareModelAssetSource(ContentManager contentManager) {
            this.contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
        }

        /// <summary>
        /// Loads an owned CPU-readable model using the reference relative path.
        /// </summary>
        /// <param name="reference">File-backed or generated scene asset reference.</param>
        /// <returns>An owned raw model asset.</returns>
        public ModelAsset LoadOwned(SceneAssetReference reference) {
            if (reference == null) {
                throw new ArgumentNullException(nameof(reference));
            }
            if (string.IsNullOrWhiteSpace(reference.RelativePath)) {
                throw new InvalidOperationException("Software model references must include a relative path.");
            }

            return contentManager.Load<ModelAsset>(reference.RelativePath, RuntimeContentProcessorIds.ModelAsset);
        }
    }

    /// <summary>
    /// Stores one compact transformed triangle for CPU ray intersection.
    /// </summary>
    public readonly struct SoftwareTriangle {
        /// <summary>
        /// Explicit owned bytes for one compact triangle element.
        /// </summary>
        public const int OwnedBytes = 88;

        /// <summary>
        /// Gets the first transformed triangle corner.
        /// </summary>
        public float3 P0 { get; }

        /// <summary>
        /// Gets the transformed edge from the first to the second corner.
        /// </summary>
        public float3 Edge1 { get; }

        /// <summary>
        /// Gets the transformed edge from the first to the third corner.
        /// </summary>
        public float3 Edge2 { get; }

        /// <summary>
        /// Gets the normalized geometric normal recomputed from transformed edges.
        /// </summary>
        public float3 GeometricNormal { get; }

        /// <summary>
        /// Gets the flattened material index for this triangle.
        /// </summary>
        public int MaterialIndex { get; }

        /// <summary>
        /// Gets the transformed triangle centroid.
        /// </summary>
        public float3 Centroid { get; }

        /// <summary>
        /// Gets the transformed triangle minimum bounds.
        /// </summary>
        public float3 BoundsMin { get; }

        /// <summary>
        /// Gets the transformed triangle maximum bounds.
        /// </summary>
        public float3 BoundsMax { get; }

        /// <summary>
        /// Initializes one compact triangle.
        /// </summary>
        /// <param name="p0">First triangle corner.</param>
        /// <param name="edge1">First transformed edge.</param>
        /// <param name="edge2">Second transformed edge.</param>
        /// <param name="geometricNormal">Normalized geometric normal.</param>
        /// <param name="materialIndex">Flattened material index.</param>
        /// <param name="centroid">Triangle centroid.</param>
        /// <param name="boundsMin">Triangle minimum bounds.</param>
        /// <param name="boundsMax">Triangle maximum bounds.</param>
        public SoftwareTriangle(float3 p0, float3 edge1, float3 edge2, float3 geometricNormal, int materialIndex, float3 centroid, float3 boundsMin, float3 boundsMax) {
            P0 = p0;
            Edge1 = edge1;
            Edge2 = edge2;
            GeometricNormal = geometricNormal;
            MaterialIndex = materialIndex;
            Centroid = centroid;
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
        }
    }

    /// <summary>
    /// Stores one compact diffuse and pre-multiplied emissive material.
    /// </summary>
    public readonly struct SoftwareMaterialData {
        /// <summary>
        /// Explicit owned bytes for one compact material element.
        /// </summary>
        public const int OwnedBytes = 24;

        /// <summary>
        /// Gets the diffuse color.
        /// </summary>
        public float3 DiffuseColor { get; }

        /// <summary>
        /// Gets the emission color after strength has been applied.
        /// </summary>
        public float3 Emission { get; }

        /// <summary>
        /// Initializes one compact material.
        /// </summary>
        /// <param name="diffuseColor">Diffuse color.</param>
        /// <param name="emission">Pre-multiplied emission.</param>
        public SoftwareMaterialData(float3 diffuseColor, float3 emission) {
            DiffuseColor = diffuseColor;
            Emission = emission;
        }
    }

    /// <summary>
    /// Stores the one rectangular area emitter derived from the authored scene.
    /// </summary>
    public readonly struct SoftwareAreaLight {
        /// <summary>
        /// Explicit owned bytes for one compact area-light element.
        /// </summary>
        public const int OwnedBytes = 72;

        /// <summary>
        /// Gets one rectangle corner.
        /// </summary>
        public float3 Corner { get; }

        /// <summary>
        /// Gets the first rectangle edge vector.
        /// </summary>
        public float3 Edge1 { get; }

        /// <summary>
        /// Gets the second rectangle edge vector.
        /// </summary>
        public float3 Edge2 { get; }

        /// <summary>
        /// Gets the rectangle normal pointing into the containing box.
        /// </summary>
        public float3 InwardNormal { get; }

        /// <summary>
        /// Gets the positive rectangle area.
        /// </summary>
        public float Area { get; }

        /// <summary>
        /// Gets the pre-multiplied rectangle emission.
        /// </summary>
        public float3 Emission { get; }

        /// <summary>
        /// Gets the first selected emitter triangle index.
        /// </summary>
        public int FirstTriangleIndex { get; }

        /// <summary>
        /// Gets the second selected emitter triangle index.
        /// </summary>
        public int SecondTriangleIndex { get; }

        /// <summary>
        /// Initializes one compact rectangle light.
        /// </summary>
        /// <param name="corner">Rectangle corner.</param>
        /// <param name="edge1">First rectangle edge.</param>
        /// <param name="edge2">Second rectangle edge.</param>
        /// <param name="inwardNormal">Inward normal.</param>
        /// <param name="area">Rectangle area.</param>
        /// <param name="emission">Pre-multiplied emission.</param>
        /// <param name="firstTriangleIndex">First selected triangle index.</param>
        /// <param name="secondTriangleIndex">Second selected triangle index.</param>
        public SoftwareAreaLight(float3 corner, float3 edge1, float3 edge2, float3 inwardNormal, float area, float3 emission, int firstTriangleIndex, int secondTriangleIndex) {
            Corner = corner;
            Edge1 = edge1;
            Edge2 = edge2;
            InwardNormal = inwardNormal;
            Area = area;
            Emission = emission;
            FirstTriangleIndex = firstTriangleIndex;
            SecondTriangleIndex = secondTriangleIndex;
        }
    }

    /// <summary>
    /// Owns compact transformed geometry, compact materials, and exactly one rectangular area emitter.
    /// </summary>
    public sealed class SoftwareTraceScene {
        /// <summary>
        /// Explicit owned bytes for one compact triangle element.
        /// </summary>
        public const int SoftwareTriangleBytes = SoftwareTriangle.OwnedBytes;

        /// <summary>
        /// Explicit owned bytes for one compact material element.
        /// </summary>
        public const int SoftwareMaterialDataBytes = SoftwareMaterialData.OwnedBytes;

        /// <summary>
        /// Explicit owned bytes for one compact area-light element.
        /// </summary>
        public const int SoftwareAreaLightBytes = SoftwareAreaLight.OwnedBytes;

        /// <summary>
        /// Numerical tolerance used for geometric validation.
        /// </summary>
        const float GeometryTolerance = 0.0001f;

        /// <summary>
        /// Stores compact transformed triangles.
        /// </summary>
        public SoftwareTriangle[] Triangles { get; }

        /// <summary>
        /// Stores compact per-instance material data.
        /// </summary>
        public SoftwareMaterialData[] Materials { get; }

        /// <summary>
        /// Stores the single derived rectangular area light.
        /// </summary>
        public SoftwareAreaLight AreaLight { get; }

        /// <summary>
        /// Gets the compact owned bytes retained after initialization.
        /// </summary>
        public long SteadyStateOwnedBytes { get; }

        /// <summary>
        /// Gets the peak owned bytes observed while raw assets were converted.
        /// </summary>
        public long InitializationPeakOwnedBytes { get; }

        /// <summary>
        /// Initializes one software trace scene from entities and an owned-asset source.
        /// </summary>
        /// <param name="entities">Entities whose attached software model components are consumed.</param>
        /// <param name="source">Source that returns owned raw model assets.</param>
        public SoftwareTraceScene(IReadOnlyList<Entity> entities, ISoftwareModelAssetSource source) {
            if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            }
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            List<ModelInstance> instances = new List<ModelInstance>();
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                CollectInstances(entities[entityIndex], instances);
            }
            if (instances.Count == 0) {
                throw new InvalidOperationException("Software trace scenes require at least one SoftwareModelComponent.");
            }

            List<ModelGroup> groups = CreateStableGroups(instances);
            List<SoftwareTriangle> triangles = new List<SoftwareTriangle>();
            List<SoftwareMaterialData> materials = new List<SoftwareMaterialData>();
            List<EmitterInstance> emitterInstances = new List<EmitterInstance>();
            int emissiveComponentCount = 0;
            long peakRawOwnedBytes = 0;

            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++) {
                ModelGroup group = groups[groupIndex];
                ModelAsset owned = source.LoadOwned(group.Reference);
                try {
                    ValidateModelAsset(owned, group);
                    long rawOwnedBytes = ComputeRawOwnedBytes(owned);
                    if (rawOwnedBytes > peakRawOwnedBytes) {
                        peakRawOwnedBytes = rawOwnedBytes;
                    }

                    for (int instanceIndex = 0; instanceIndex < group.Instances.Count; instanceIndex++) {
                        ModelInstance instance = group.Instances[instanceIndex];
                        int materialOffset = materials.Count;
                        AppendMaterials(instance.Component, materials, out bool isEmissive);
                        if (isEmissive) {
                            emissiveComponentCount++;
                        }

                        int triangleOffset = triangles.Count;
                        AppendTriangles(owned, instance, materialOffset, triangles);
                        if (isEmissive) {
                            emitterInstances.Add(new EmitterInstance(triangleOffset, triangles.Count - triangleOffset));
                        }
                    }
                } finally {
                    if (owned != null) {
                        owned.Dispose();
                    }
                }
            }

            if (emissiveComponentCount != 1) {
                throw new InvalidOperationException($"Software trace scenes require exactly one emissive SoftwareModelComponent; found {emissiveComponentCount}.");
            }
            if (emitterInstances.Count != 1) {
                throw new InvalidOperationException("Software trace scenes require exactly one emissive component instance.");
            }

            Triangles = triangles.ToArray();
            Materials = materials.ToArray();
            AreaLight = DeriveAreaLight(Triangles, Materials, emitterInstances[0]);
            SteadyStateOwnedBytes = ((long)Triangles.Length * SoftwareTriangleBytes) + ((long)Materials.Length * SoftwareMaterialDataBytes) + SoftwareAreaLightBytes;
            InitializationPeakOwnedBytes = SteadyStateOwnedBytes + peakRawOwnedBytes;
        }

        /// <summary>
        /// Builds one software trace scene from entities and an owned-asset source.
        /// </summary>
        /// <param name="entities">Entities whose attached software model components are consumed.</param>
        /// <param name="source">Source that returns owned raw model assets.</param>
        /// <returns>A compact software trace scene.</returns>
        public static SoftwareTraceScene Build(IReadOnlyList<Entity> entities, ISoftwareModelAssetSource source) {
            return new SoftwareTraceScene(entities, source);
        }

        /// <summary>
        /// Creates one software trace scene from entities and an owned-asset source.
        /// </summary>
        /// <param name="entities">Entities whose attached software model components are consumed.</param>
        /// <param name="source">Source that returns owned raw model assets.</param>
        /// <returns>A compact software trace scene.</returns>
        public static SoftwareTraceScene Create(IReadOnlyList<Entity> entities, ISoftwareModelAssetSource source) {
            return new SoftwareTraceScene(entities, source);
        }

        /// <summary>
        /// Collects software model components from one entity and its initialized descendants.
        /// </summary>
        /// <param name="entity">Entity to inspect.</param>
        /// <param name="instances">Destination instance list.</param>
        static void CollectInstances(Entity entity, List<ModelInstance> instances) {
            if (entity == null) {
                throw new InvalidOperationException("Software trace scene entity entries cannot be null.");
            }

            List<Component> components = entity.Components;
            if (components != null) {
                for (int componentIndex = 0; componentIndex < components.Count; componentIndex++) {
                    if (components[componentIndex] is SoftwareModelComponent softwareComponent) {
                        if (softwareComponent.ModelReference == null) {
                            throw new InvalidOperationException("SoftwareModelComponent.ModelReference is required.");
                        }
                        instances.Add(new ModelInstance(entity, softwareComponent));
                    }
                }
            }

            List<Entity> children = entity.Children;
            if (children != null) {
                for (int childIndex = 0; childIndex < children.Count; childIndex++) {
                    CollectInstances(children[childIndex], instances);
                }
            }
        }

        /// <summary>
        /// Groups instances by stable source/provider/asset/path identity in first-seen order.
        /// </summary>
        /// <param name="instances">Instances to group.</param>
        /// <returns>Stable ordered groups.</returns>
        static List<ModelGroup> CreateStableGroups(List<ModelInstance> instances) {
            List<ModelGroup> groups = new List<ModelGroup>();
            for (int instanceIndex = 0; instanceIndex < instances.Count; instanceIndex++) {
                ModelInstance instance = instances[instanceIndex];
                SceneAssetReference reference = instance.Component.ModelReference;
                ModelGroup matchingGroup = null;
                for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++) {
                    if (groups[groupIndex].Matches(reference)) {
                        matchingGroup = groups[groupIndex];
                        break;
                    }
                }

                if (matchingGroup == null) {
                    matchingGroup = new ModelGroup(reference);
                    groups.Add(matchingGroup);
                }
                matchingGroup.Instances.Add(instance);
            }
            return groups;
        }

        /// <summary>
        /// Validates one raw model and all instance material contracts before indexing its arrays.
        /// </summary>
        /// <param name="asset">Raw model asset to validate.</param>
        /// <param name="group">Instances that consume the model.</param>
        static void ValidateModelAsset(ModelAsset asset, ModelGroup group) {
            if (asset == null) {
                throw new InvalidOperationException("Software model source returned a null ModelAsset.");
            }
            if (asset.Positions == null || asset.Positions.Length == 0) {
                throw new InvalidOperationException("ModelAsset.Positions must be a non-empty array.");
            }
            bool hasIndices16 = asset.Indices16 != null && asset.Indices16.Length > 0;
            bool hasIndices32 = asset.Indices32 != null && asset.Indices32.Length > 0;
            if (hasIndices16 && hasIndices32) {
                throw new InvalidOperationException("ModelAsset must populate exactly one index width; both Indices16 and Indices32 are populated.");
            }
            if (!hasIndices16 && !hasIndices32) {
                throw new InvalidOperationException("ModelAsset must populate exactly one index width; neither Indices16 nor Indices32 is populated.");
            }
            int indexCount = hasIndices16 ? asset.Indices16.Length : asset.Indices32.Length;
            if (indexCount % 3 != 0) {
                throw new InvalidOperationException("ModelAsset index count must be divisible by three.");
            }
            if (asset.Submeshes == null || asset.Submeshes.Length == 0) {
                throw new InvalidOperationException("ModelAsset.Submeshes must be a non-empty array.");
            }

            int[] materialForIndex = new int[indexCount];
            for (int index = 0; index < materialForIndex.Length; index++) {
                materialForIndex[index] = -1;
            }
            for (int submeshIndex = 0; submeshIndex < asset.Submeshes.Length; submeshIndex++) {
                ModelSubmeshAsset submesh = asset.Submeshes[submeshIndex];
                if (submesh == null) {
                    throw new InvalidOperationException($"ModelAsset.Submeshes[{submeshIndex}] cannot be null.");
                }
                if (submesh.IndexStart < 0 || submesh.IndexStart % 3 != 0 || submesh.IndexCount <= 0 || submesh.IndexCount % 3 != 0 || submesh.IndexStart > indexCount - submesh.IndexCount) {
                    throw new InvalidOperationException($"ModelAsset.Submeshes[{submeshIndex}] has an invalid triangle-aligned index range.");
                }
                for (int index = submesh.IndexStart; index < submesh.IndexStart + submesh.IndexCount; index++) {
                    if (materialForIndex[index] >= 0) {
                        throw new InvalidOperationException($"ModelAsset.Submeshes overlap at index {index}.");
                    }
                    materialForIndex[index] = submeshIndex;
                }
            }
            for (int index = 0; index < materialForIndex.Length; index++) {
                if (materialForIndex[index] < 0) {
                    throw new InvalidOperationException($"ModelAsset.Submeshes do not cover index {index}.");
                }
                uint value = hasIndices16 ? asset.Indices16[index] : asset.Indices32[index];
                if (value >= asset.Positions.Length) {
                    throw new InvalidOperationException($"ModelAsset index {value} at position {index} is outside Positions.");
                }
            }

            for (int instanceIndex = 0; instanceIndex < group.Instances.Count; instanceIndex++) {
                SoftwareMaterial[] instanceMaterials = group.Instances[instanceIndex].Component.Materials;
                if (instanceMaterials == null || instanceMaterials.Length < asset.Submeshes.Length) {
                    throw new InvalidOperationException("SoftwareModelComponent.Materials must provide at least one material for every ModelAsset submesh.");
                }
                for (int materialIndex = 0; materialIndex < instanceMaterials.Length; materialIndex++) {
                    if (instanceMaterials[materialIndex] == null) {
                        throw new InvalidOperationException($"SoftwareModelComponent.Materials[{materialIndex}] cannot be null.");
                    }
                }
            }
        }

        /// <summary>
        /// Appends one instance's compact material values and reports whether it emits.
        /// </summary>
        /// <param name="component">Authored component to flatten.</param>
        /// <param name="materials">Destination material list.</param>
        /// <param name="isEmissive">Receives whether any material emits.</param>
        static void AppendMaterials(SoftwareModelComponent component, List<SoftwareMaterialData> materials, out bool isEmissive) {
            isEmissive = false;
            for (int materialIndex = 0; materialIndex < component.Materials.Length; materialIndex++) {
                SoftwareMaterial material = component.Materials[materialIndex];
                float3 emission = material.EmissionColor * material.EmissionStrength;
                materials.Add(new SoftwareMaterialData(material.DiffuseColor, emission));
                if (material.EmissionStrength > 0f && emission.LengthSquared() > GeometryTolerance * GeometryTolerance) {
                    isEmissive = true;
                }
            }
        }

        /// <summary>
        /// Appends all transformed triangles for one validated instance.
        /// </summary>
        /// <param name="asset">Validated raw model.</param>
        /// <param name="instance">Instance transform and materials.</param>
        /// <param name="materialOffset">Offset into compact material data.</param>
        /// <param name="triangles">Destination triangle list.</param>
        static void AppendTriangles(ModelAsset asset, ModelInstance instance, int materialOffset, List<SoftwareTriangle> triangles) {
            bool hasIndices16 = asset.Indices16 != null && asset.Indices16.Length > 0;
            for (int submeshIndex = 0; submeshIndex < asset.Submeshes.Length; submeshIndex++) {
                ModelSubmeshAsset submesh = asset.Submeshes[submeshIndex];
                for (int index = submesh.IndexStart; index < submesh.IndexStart + submesh.IndexCount; index += 3) {
                    uint index0 = hasIndices16 ? asset.Indices16[index] : asset.Indices32[index];
                    uint index1 = hasIndices16 ? asset.Indices16[index + 1] : asset.Indices32[index + 1];
                    uint index2 = hasIndices16 ? asset.Indices16[index + 2] : asset.Indices32[index + 2];
                    float3 p0 = TransformPoint(asset.Positions[index0], instance.WorldTransform);
                    float3 p1 = TransformPoint(asset.Positions[index1], instance.WorldTransform);
                    float3 p2 = TransformPoint(asset.Positions[index2], instance.WorldTransform);
                    float3 edge1 = p1 - p0;
                    float3 edge2 = p2 - p0;
                    float3 cross = float3.Cross(edge1, edge2);
                    if (cross.LengthSquared() <= GeometryTolerance * GeometryTolerance) {
                        throw new InvalidOperationException($"ModelAsset triangle at index {index} is degenerate after WorldTransformMatrix.");
                    }
                    float3 normal = float3.Normalize(cross);
                    float3 centroid = (p0 + p1 + p2) / 3f;
                    float3 boundsMin = float3.Min(p0, float3.Min(p1, p2));
                    float3 boundsMax = float3.Max(p0, float3.Max(p1, p2));
                    triangles.Add(new SoftwareTriangle(p0, edge1, edge2, normal, materialOffset + submeshIndex, centroid, boundsMin, boundsMax));
                }
            }
        }

        /// <summary>
        /// Derives one rectangular area light from the selected emissive component instance.
        /// </summary>
        /// <param name="triangles">Flattened scene triangles.</param>
        /// <param name="materials">Flattened scene materials.</param>
        /// <param name="emitter">Flattened emitter triangle range.</param>
        /// <returns>One compact rectangular area light.</returns>
        static SoftwareAreaLight DeriveAreaLight(SoftwareTriangle[] triangles, SoftwareMaterialData[] materials, EmitterInstance emitter) {
            Candidate bestAny = default(Candidate);
            int bestAnyCount = 0;
            Candidate bestInward = default(Candidate);
            int bestInwardCount = 0;
            float3 modelMin = new float3(float.PositiveInfinity);
            float3 modelMax = new float3(float.NegativeInfinity);
            for (int index = emitter.TriangleOffset; index < emitter.TriangleOffset + emitter.TriangleCount; index++) {
                modelMin = float3.Min(modelMin, triangles[index].BoundsMin);
                modelMax = float3.Max(modelMax, triangles[index].BoundsMax);
            }
            float3 modelCenter = (modelMin + modelMax) * 0.5f;
            for (int first = emitter.TriangleOffset; first < emitter.TriangleOffset + emitter.TriangleCount; first++) {
                for (int second = first + 1; second < emitter.TriangleOffset + emitter.TriangleCount; second++) {
                    if (!TryCreateCandidate(triangles[first], triangles[second], first, second, out Candidate candidate)) {
                        continue;
                    }
                    float3 toCenter = modelCenter - candidate.Corner;
                    bool inward = float3.Dot(candidate.Normal, toCenter) > GeometryTolerance;
                    if (inward) {
                        candidate.IsInward = true;
                    }
                    if (!bestAny.IsValid || candidate.Area > bestAny.Area + GeometryTolerance) {
                        bestAny = candidate;
                        bestAnyCount = 1;
                    } else if (Math.Abs(bestAny.Area - candidate.Area) <= GeometryTolerance) {
                        bestAnyCount++;
                    }
                    if (candidate.IsInward) {
                        if (!bestInward.IsValid || candidate.Area > bestInward.Area + GeometryTolerance) {
                            bestInward = candidate;
                            bestInwardCount = 1;
                        } else if (Math.Abs(bestInward.Area - candidate.Area) <= GeometryTolerance) {
                            bestInwardCount++;
                        }
                    }
                }
            }

            Candidate best = bestInward.IsValid ? bestInward : bestAny;
            int candidateCount = bestInward.IsValid ? bestInwardCount : bestAnyCount;
            if (!best.IsValid || candidateCount != 1) {
                throw new InvalidOperationException("Emissive geometry must contain exactly one unambiguous rectangular inward-facing triangle pair.");
            }
            SoftwareMaterialData firstMaterial = materials[triangles[best.FirstTriangle].MaterialIndex];
            SoftwareMaterialData secondMaterial = materials[triangles[best.SecondTriangle].MaterialIndex];
            if (!NearlyEqual(firstMaterial.Emission, secondMaterial.Emission)) {
                throw new InvalidOperationException("Selected emissive rectangle triangles must share one emission value.");
            }
            return new SoftwareAreaLight(best.Corner, best.Edge1, best.Edge2, best.Normal, best.Area, firstMaterial.Emission, best.FirstTriangle, best.SecondTriangle);
        }

        /// <summary>
        /// Attempts to combine two coplanar triangles into one rectangle candidate.
        /// </summary>
        /// <param name="first">First triangle.</param>
        /// <param name="second">Second triangle.</param>
        /// <param name="firstIndex">First triangle index.</param>
        /// <param name="secondIndex">Second triangle index.</param>
        /// <param name="candidate">Receives the rectangle candidate.</param>
        /// <returns>True when the triangles form a non-degenerate rectangle.</returns>
        static bool TryCreateCandidate(SoftwareTriangle first, SoftwareTriangle second, int firstIndex, int secondIndex, out Candidate candidate) {
            candidate = default(Candidate);
            if (float3.Dot(first.GeometricNormal, second.GeometricNormal) < 1f - GeometryTolerance) {
                return false;
            }
            if (Math.Abs(float3.Dot(first.GeometricNormal, second.P0 - first.P0)) > GeometryTolerance) {
                return false;
            }

            float3[] firstPoints = { first.P0, first.P0 + first.Edge1, first.P0 + first.Edge2 };
            float3[] secondPoints = { second.P0, second.P0 + second.Edge1, second.P0 + second.Edge2 };
            float3[] points = new float3[4];
            int pointCount = 0;
            int sharedCount = 0;
            for (int firstPointIndex = 0; firstPointIndex < 3; firstPointIndex++) {
                bool found = false;
                for (int secondPointIndex = 0; secondPointIndex < 3; secondPointIndex++) {
                    if (NearlyEqual(firstPoints[firstPointIndex], secondPoints[secondPointIndex])) {
                        sharedCount++;
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    if (pointCount >= points.Length) {
                        return false;
                    }
                    points[pointCount++] = firstPoints[firstPointIndex];
                }
            }
            for (int secondPointIndex = 0; secondPointIndex < 3; secondPointIndex++) {
                bool found = false;
                for (int firstPointIndex = 0; firstPointIndex < 3; firstPointIndex++) {
                    if (NearlyEqual(secondPoints[secondPointIndex], firstPoints[firstPointIndex])) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    if (pointCount >= points.Length) {
                        return false;
                    }
                    points[pointCount++] = secondPoints[secondPointIndex];
                }
            }
            if (sharedCount != 2 || pointCount != 2) {
                return false;
            }

            float3[] corners = new float3[4];
            int cornerCount = 0;
            for (int sourcePointIndex = 0; sourcePointIndex < 3 + 3; sourcePointIndex++) {
                float3 sourcePoint = sourcePointIndex < 3 ? firstPoints[sourcePointIndex] : secondPoints[sourcePointIndex - 3];
                bool alreadyAdded = false;
                for (int existingIndex = 0; existingIndex < cornerCount; existingIndex++) {
                    if (NearlyEqual(corners[existingIndex], sourcePoint)) {
                        alreadyAdded = true;
                        break;
                    }
                }
                if (!alreadyAdded && cornerCount < corners.Length) {
                    corners[cornerCount++] = sourcePoint;
                }
            }
            if (cornerCount != 4) {
                return false;
            }
            for (int cornerIndex = 0; cornerIndex < cornerCount; cornerIndex++) {
                float3 corner = corners[cornerIndex];
                int firstOtherIndex = -1;
                int secondOtherIndex = -1;
                for (int otherIndex = 0; otherIndex < 4; otherIndex++) {
                    bool isCorner = false;
                    if (isCorner || NearlyEqual(corner, corners[otherIndex])) {
                        continue;
                    }
                    if (firstOtherIndex < 0) {
                        firstOtherIndex = otherIndex;
                    } else {
                        secondOtherIndex = otherIndex;
                        break;
                    }
                }
                if (firstOtherIndex < 0 || secondOtherIndex < 0) {
                    continue;
                }
                float3 edge1 = corners[firstOtherIndex] - corner;
                float3 edge2 = corners[secondOtherIndex] - corner;
                float edge1Length = edge1.Length();
                float edge2Length = edge2.Length();
                if (edge1Length <= GeometryTolerance || edge2Length <= GeometryTolerance || Math.Abs(float3.Dot(edge1, edge2)) > GeometryTolerance * edge1Length * edge2Length) {
                    continue;
                }
                float3 expectedOpposite = corner + edge1 + edge2;
                bool foundOpposite = false;
                for (int otherIndex = 0; otherIndex < cornerCount; otherIndex++) {
                    if (otherIndex != cornerIndex && otherIndex != firstOtherIndex && otherIndex != secondOtherIndex && NearlyEqual(corners[otherIndex], expectedOpposite)) {
                        foundOpposite = true;
                        break;
                    }
                }
                if (!foundOpposite) {
                    continue;
                }
                float3 rectangleNormal = float3.Normalize(float3.Cross(edge1, edge2));
                if (float3.Dot(rectangleNormal, first.GeometricNormal) < 0f) {
                    float3 swap = edge1;
                    edge1 = edge2;
                    edge2 = swap;
                }
                candidate = new Candidate {
                    IsValid = true,
                    IsInward = false,
                    Corner = corner,
                    Edge1 = edge1,
                    Edge2 = edge2,
                    Normal = first.GeometricNormal,
                    Area = edge1.Length() * edge2.Length(),
                    FirstTriangle = firstIndex,
                    SecondTriangle = secondIndex
                };
                return true;
            }
            return false;
        }

        /// <summary>
        /// Transforms one point with full row-vector affine matrix math.
        /// </summary>
        /// <param name="point">Source point.</param>
        /// <param name="matrix">World transform matrix.</param>
        /// <returns>Transformed point.</returns>
        static float3 TransformPoint(float3 point, float4x4 matrix) {
            return new float3(
                point.X * matrix.M11 + point.Y * matrix.M21 + point.Z * matrix.M31 + matrix.M41,
                point.X * matrix.M12 + point.Y * matrix.M22 + point.Z * matrix.M32 + matrix.M42,
                point.X * matrix.M13 + point.Y * matrix.M23 + point.Z * matrix.M33 + matrix.M43);
        }

        /// <summary>
        /// Computes a deterministic estimate of raw owned bytes present during one load.
        /// </summary>
        /// <param name="asset">Raw model asset.</param>
        /// <returns>Estimated raw owned bytes.</returns>
        static long ComputeRawOwnedBytes(ModelAsset asset) {
            long bytes = 0;
            if (asset.Positions != null) {
                bytes += (long)asset.Positions.Length * 12;
            }
            if (asset.Normals != null) {
                bytes += (long)asset.Normals.Length * 12;
            }
            if (asset.TexCoords != null) {
                bytes += (long)asset.TexCoords.Length * 8;
            }
            if (asset.Indices16 != null) {
                bytes += (long)asset.Indices16.Length * 2;
            }
            if (asset.Indices32 != null) {
                bytes += (long)asset.Indices32.Length * 4;
            }
            if (asset.Submeshes != null) {
                bytes += (long)asset.Submeshes.Length * 12;
            }
            return bytes;
        }

        /// <summary>
        /// Compares two vectors using the scene geometric tolerance.
        /// </summary>
        /// <param name="first">First vector.</param>
        /// <param name="second">Second vector.</param>
        /// <returns>True when all components are within tolerance.</returns>
        static bool NearlyEqual(float3 first, float3 second) {
            return Math.Abs(first.X - second.X) <= GeometryTolerance && Math.Abs(first.Y - second.Y) <= GeometryTolerance && Math.Abs(first.Z - second.Z) <= GeometryTolerance;
        }

        /// <summary>
        /// Stores one attached component together with its copied world transform.
        /// </summary>
        sealed class ModelInstance {
            /// <summary>
            /// Entity owning the component.
            /// </summary>
            public readonly Entity Entity;
            /// <summary>
            /// Attached authored software model component.
            /// </summary>
            public readonly SoftwareModelComponent Component;
            /// <summary>
            /// World transform captured before raw model ingestion.
            /// </summary>
            public readonly float4x4 WorldTransform;

            /// <summary>
            /// Initializes one model instance.
            /// </summary>
            /// <param name="entity">Owning entity.</param>
            /// <param name="component">Attached software component.</param>
            public ModelInstance(Entity entity, SoftwareModelComponent component) {
                Entity = entity;
                Component = component;
                WorldTransform = entity.WorldTransformMatrix;
            }
        }

        /// <summary>
        /// Stores one stable identity group and its ordered instances.
        /// </summary>
        sealed class ModelGroup {
            /// <summary>
            /// Stable reference represented by this group.
            /// </summary>
            public readonly SceneAssetReference Reference;
            /// <summary>
            /// Instances in first-seen entity/component order.
            /// </summary>
            public readonly List<ModelInstance> Instances = new List<ModelInstance>();

            /// <summary>
            /// Initializes one identity group.
            /// </summary>
            /// <param name="reference">Stable group reference.</param>
            public ModelGroup(SceneAssetReference reference) {
                Reference = reference;
            }

            /// <summary>
            /// Determines whether a reference has the same stable group identity.
            /// </summary>
            /// <param name="reference">Reference to compare.</param>
            /// <returns>True when source, provider, asset, and path all match.</returns>
            public bool Matches(SceneAssetReference reference) {
                return Reference.SourceKind == reference.SourceKind && string.Equals(Reference.ProviderId, reference.ProviderId, StringComparison.Ordinal) && string.Equals(Reference.AssetId, reference.AssetId, StringComparison.Ordinal) && string.Equals(Reference.RelativePath, reference.RelativePath, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Stores the flattened triangle range for one emissive component instance.
        /// </summary>
        readonly struct EmitterInstance {
            /// <summary>
            /// First flattened triangle index.
            /// </summary>
            public readonly int TriangleOffset;
            /// <summary>
            /// Number of flattened triangles.
            /// </summary>
            public readonly int TriangleCount;

            /// <summary>
            /// Initializes one emitter range.
            /// </summary>
            /// <param name="triangleOffset">First flattened triangle index.</param>
            /// <param name="triangleCount">Flattened triangle count.</param>
            public EmitterInstance(int triangleOffset, int triangleCount) {
                TriangleOffset = triangleOffset;
                TriangleCount = triangleCount;
            }
        }

        /// <summary>
        /// Stores one candidate rectangle during emitter selection.
        /// </summary>
        struct Candidate {
            /// <summary>Whether this candidate has valid rectangle geometry.</summary>
            public bool IsValid;
            /// <summary>Whether this candidate is oriented toward the containing box.</summary>
            public bool IsInward;
            /// <summary>Rectangle corner.</summary>
            public float3 Corner;
            /// <summary>First rectangle edge.</summary>
            public float3 Edge1;
            /// <summary>Second rectangle edge.</summary>
            public float3 Edge2;
            /// <summary>Rectangle inward normal.</summary>
            public float3 Normal;
            /// <summary>Rectangle area.</summary>
            public float Area;
            /// <summary>First selected triangle index.</summary>
            public int FirstTriangle;
            /// <summary>Second selected triangle index.</summary>
            public int SecondTriangle;
        }
    }
}
