using System;
using helengine;

namespace city.rendering {
    /// <summary>
    /// Provides deterministic stateless random samples for the software path tracer.
    /// </summary>
    public static class SoftwarePathSampler {
        /// <summary>
        /// First fixed hash constant used to combine signed pixel and path keys.
        /// </summary>
        const uint HashOffset = 2166136261u;

        /// <summary>
        /// Prime used by the input-key avalanche step.
        /// </summary>
        const uint HashPrime = 16777619u;

        /// <summary>
        /// Golden-ratio constant used to separate integer dimensions.
        /// </summary>
        const uint DimensionConstant = 2654435761u;

        /// <summary>
        /// Converts the upper 24 hash bits into a half-open unit float.
        /// </summary>
        const float UnitScale = 1f / 16777216f;

        /// <summary>
        /// Two times pi as a single precision constant for cosine sampling.
        /// </summary>
        const float TwoPi = 6.28318530717958647692f;

        /// <summary>
        /// Tolerance used when validating unit surface normals.
        /// </summary>
        const float UnitNormalTolerance = 0.002f;

        /// <summary>
        /// Returns one deterministic hash sample for a pixel, pass, bounce, and dimension.
        /// </summary>
        /// <param name="pixelX">Non-negative pixel x identity.</param>
        /// <param name="pixelY">Non-negative pixel y identity.</param>
        /// <param name="completedPass">Completed progressive pass identity.</param>
        /// <param name="bounce">Diffuse-bounce identity.</param>
        /// <param name="dimension">Reserved sampler dimension.</param>
        /// <returns>A finite value in the half-open interval [0, 1).</returns>
        public static float Sample01(int pixelX, int pixelY, int completedPass, int bounce, int dimension) {
            uint hash = HashOffset;
            hash = Mix(hash, unchecked((uint)pixelX), 0x9E3779B9u);
            hash = Mix(hash, unchecked((uint)pixelY), 0x85EBCA77u);
            hash = Mix(hash, unchecked((uint)completedPass), 0xC2B2AE3Du);
            hash = Mix(hash, unchecked((uint)bounce), 0x27D4EB2Fu);
            hash = Mix(hash, unchecked((uint)dimension), DimensionConstant);
            unchecked {
                hash ^= hash >> 16;
                hash *= 2246822519u;
                hash ^= hash >> 13;
                hash *= 3266489917u;
                hash ^= hash >> 16;
                return (hash >> 8) * UnitScale;
            }
        }

        /// <summary>
        /// Creates one cosine-weighted direction in the hemisphere around a unit normal.
        /// </summary>
        /// <param name="normal">Unit surface normal defining the hemisphere.</param>
        /// <param name="firstSample">Radial sample in [0, 1).</param>
        /// <param name="secondSample">Azimuth sample in [0, 1).</param>
        /// <returns>A finite unit direction in the supplied normal hemisphere.</returns>
        public static float3 SampleCosineHemisphere(float3 normal, float firstSample, float secondSample) {
            ValidateUnitNormal(normal, nameof(normal));
            ValidateUnitSample(firstSample, nameof(firstSample));
            ValidateUnitSample(secondSample, nameof(secondSample));

            float radial = (float)Math.Sqrt(firstSample);
            float phi = TwoPi * secondSample;
            float tangentX = radial * (float)Math.Cos(phi);
            float tangentY = radial * (float)Math.Sin(phi);
            float normalPart = (float)Math.Sqrt(Math.Max(0f, 1f - firstSample));

            float3 reference = Math.Abs(normal.Y) < 0.9f ? new float3(0f, 1f, 0f) : new float3(1f, 0f, 0f);
            float3 tangent = float3.Normalize(float3.Cross(reference, normal));
            float3 bitangent = float3.Cross(normal, tangent);
            float3 result = Add(Add(Scale(tangent, tangentX), Scale(bitangent, tangentY)), Scale(normal, normalPart));
            if (!IsFinite(result)) {
                throw new ArgumentOutOfRangeException(nameof(normal), "The cosine sample was not finite.");
            }
            return result;
        }

        /// <summary>
        /// Samples one point from a rectangular area light using affine coordinates.
        /// </summary>
        /// <param name="light">Rectangular light to sample.</param>
        /// <param name="firstSample">First rectangle coordinate in [0, 1).</param>
        /// <param name="secondSample">Second rectangle coordinate in [0, 1).</param>
        /// <returns>The sampled world-space point.</returns>
        public static float3 SampleAreaLight(ref SoftwareAreaLight light, float firstSample, float secondSample) {
            ValidateUnitSample(firstSample, nameof(firstSample));
            ValidateUnitSample(secondSample, nameof(secondSample));
            ValidateLightGeometry(light);

            float3 result = Add(light.Corner, Add(Scale(light.Edge1, firstSample), Scale(light.Edge2, secondSample)));
            if (!IsFinite(result)) {
                throw new ArgumentOutOfRangeException(nameof(light), "The sampled area-light point was not finite.");
            }
            return result;
        }

        /// <summary>
        /// Combines one integer key into the running hash.
        /// </summary>
        /// <param name="hash">Current hash value.</param>
        /// <param name="value">Unsigned key bits.</param>
        /// <param name="constant">Fixed key-separation constant.</param>
        /// <returns>The mixed hash value.</returns>
        static uint Mix(uint hash, uint value, uint constant) {
            unchecked {
                hash ^= value + constant + (hash << 6) + (hash >> 2);
                hash *= HashPrime;
                return hash;
            }
        }

        /// <summary>
        /// Validates one half-open unit interval sample.
        /// </summary>
        /// <param name="value">Sample to validate.</param>
        /// <param name="name">Argument name for the exception.</param>
        static void ValidateUnitSample(float value, string name) {
            if (!float.IsFinite(value) || value < 0f || value >= 1f) {
                throw new ArgumentOutOfRangeException(name, value, "Sampler inputs must be finite and in the half-open unit interval.");
            }
        }

        /// <summary>
        /// Validates one finite unit normal.
        /// </summary>
        /// <param name="normal">Normal to validate.</param>
        /// <param name="name">Argument name for the exception.</param>
        internal static void ValidateUnitNormal(float3 normal, string name) {
            float lengthSquared = LengthSquared(normal);
            if (!IsFinite(normal) || !float.IsFinite(lengthSquared) || lengthSquared <= 0f || Math.Abs(lengthSquared - 1f) > UnitNormalTolerance) {
                throw new ArgumentOutOfRangeException(name, "Sampler normals must be finite unit vectors.");
            }
        }

        /// <summary>
        /// Validates finite geometry and emission for an area light.
        /// </summary>
        /// <param name="light">Area light to validate.</param>
        internal static void ValidateLightGeometry(SoftwareAreaLight light) {
            if (!float.IsFinite(light.Area) || light.Area <= 0f || !IsFinite(light.Corner) || !IsFinite(light.Edge1) || !IsFinite(light.Edge2) || !IsFinite(light.Emission) || light.Emission.X < 0f || light.Emission.Y < 0f || light.Emission.Z < 0f) {
                throw new ArgumentOutOfRangeException(nameof(light), "Area-light geometry must be finite and have positive area.");
            }
            ValidateUnitNormal(light.InwardNormal, nameof(light));
            float3 cross = float3.Cross(light.Edge1, light.Edge2);
            float crossLengthSquared = LengthSquared(cross);
            if (!float.IsFinite(crossLengthSquared) || crossLengthSquared <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(light), "Area-light edges must span a non-degenerate rectangle.");
            }
            float edge1LengthSquared = LengthSquared(light.Edge1);
            float edge2LengthSquared = LengthSquared(light.Edge2);
            float edgeLengthProduct = (float)Math.Sqrt(edge1LengthSquared * edge2LengthSquared);
            if (!float.IsFinite(edgeLengthProduct) || edgeLengthProduct <= 0f || Math.Abs(float3.Dot(light.Edge1, light.Edge2)) > 0.002f * edgeLengthProduct) {
                throw new ArgumentOutOfRangeException(nameof(light), "Area-light edges must be finite and orthogonal.");
            }
            float geometricArea = (float)Math.Sqrt(crossLengthSquared);
            float areaTolerance = 0.002f * Math.Max(1f, geometricArea);
            if (!float.IsFinite(geometricArea) || Math.Abs(geometricArea - light.Area) > areaTolerance) {
                throw new ArgumentOutOfRangeException(nameof(light), "Area-light area must match its edge vectors.");
            }
            float3 unitCross = Scale(cross, 1f / geometricArea);
            if (Math.Abs(float3.Dot(unitCross, light.InwardNormal)) < 0.998f) {
                throw new ArgumentOutOfRangeException(nameof(light), "Area-light normal must be perpendicular to its edges.");
            }
        }

        /// <summary>
        /// Computes a vector squared length without allocating.
        /// </summary>
        /// <param name="value">Vector to measure.</param>
        /// <returns>The squared length.</returns>
        static float LengthSquared(float3 value) {
            return (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z);
        }

        /// <summary>
        /// Tests all vector components for finite values.
        /// </summary>
        /// <param name="value">Vector to inspect.</param>
        /// <returns>True when every component is finite.</returns>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }

        /// <summary>
        /// Adds two vectors component by component.
        /// </summary>
        /// <param name="first">First vector.</param>
        /// <param name="second">Second vector.</param>
        /// <returns>The component-wise sum.</returns>
        static float3 Add(float3 first, float3 second) {
            return new float3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        /// <summary>
        /// Scales a vector by one scalar.
        /// </summary>
        /// <param name="value">Vector to scale.</param>
        /// <param name="scale">Scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        static float3 Scale(float3 value, float scale) {
            return new float3(value.X * scale, value.Y * scale, value.Z * scale);
        }
    }

    /// <summary>
    /// Traces one finite scalar CPU path through compact DemoDisc geometry.
    /// </summary>
    public sealed class SoftwarePathTracer {
        /// <summary>
        /// Maximum number of diffuse surface hits considered by one sample.
        /// </summary>
        public const int MaximumDiffuseBounces = 4;

        /// <summary>
        /// Shared world-space origin offset used for all secondary and shadow rays.
        /// </summary>
        public const float RayEpsilon = 0.0001f;

        /// <summary>
        /// Single precision pi constant used by the Lambertian estimator.
        /// </summary>
        const float Pi = 3.14159265358979323846f;

        /// <summary>
        /// Tolerance used when validating normalized primary directions.
        /// </summary>
        const float PrimaryDirectionTolerance = 0.002f;

        /// <summary>
        /// Compact triangles borrowed from the trace scene.
        /// </summary>
        readonly SoftwareTriangle[] triangles;

        /// <summary>
        /// Compact materials borrowed from the trace scene.
        /// </summary>
        readonly SoftwareMaterialData[] materials;

        /// <summary>
        /// One rectangular light borrowed from the trace scene.
        /// </summary>
        readonly SoftwareAreaLight areaLight;

        /// <summary>
        /// BVH borrowed from the trace scene owner.
        /// </summary>
        readonly SoftwareBvh bvh;

        /// <summary>
        /// One worker-owned reusable traversal stack borrowed from the caller.
        /// </summary>
        readonly int[] traversalStack;

        /// <summary>
        /// Number of primary, bounce, and shadow rays launched by this tracer.
        /// </summary>
        long rayCount;

        /// <summary>
        /// Number of samples discarded after a non-finite intermediate value.
        /// </summary>
        long nonFiniteSampleCount;

        /// <summary>
        /// Gets the number of BVH rays launched by this tracer.
        /// </summary>
        public long RayCount {
            get { return rayCount; }
        }

        /// <summary>
        /// Gets the number of discarded samples caused by non-finite intermediates.
        /// </summary>
        public long NonFiniteSampleCount {
            get { return nonFiniteSampleCount; }
        }

        /// <summary>
        /// Initializes one scalar path tracer over borrowed compact scene state.
        /// </summary>
        /// <param name="triangles">Exact triangle array used to build the BVH.</param>
        /// <param name="materials">Compact diffuse and emission materials.</param>
        /// <param name="areaLight">Explicit rectangular area light.</param>
        /// <param name="bvh">Built BVH over the exact triangle array.</param>
        /// <param name="traversalStack">Caller-owned scratch with fixed BVH capacity.</param>
        public SoftwarePathTracer(SoftwareTriangle[] triangles, SoftwareMaterialData[] materials, SoftwareAreaLight areaLight, SoftwareBvh bvh, int[] traversalStack) {
            ValidateSceneInputs(triangles, materials, areaLight, bvh, traversalStack);
            this.triangles = triangles;
            this.materials = materials;
            this.areaLight = areaLight;
            this.bvh = bvh;
            this.traversalStack = traversalStack;
        }

        /// <summary>
        /// Traces one deterministic finite sample from a normalized primary ray.
        /// </summary>
        /// <param name="primaryRay">Finite non-zero normalized primary ray.</param>
        /// <param name="pixelX">Non-negative pixel x identity.</param>
        /// <param name="pixelY">Non-negative pixel y identity.</param>
        /// <param name="completedPass">Non-negative completed-pass identity.</param>
        /// <returns>Finite linear RGB radiance, or zero for a discarded non-finite sample.</returns>
        public float3 TraceSample(ref SoftwareRay primaryRay, int pixelX, int pixelY, int completedPass) {
            ValidateSampleBoundary(ref primaryRay, pixelX, pixelY, completedPass);

            float3 radiance = float3.Zero;
            float3 throughput = float3.One;
            SoftwareRay ray = primaryRay;

            for (int bounce = 0; bounce < MaximumDiffuseBounces; bounce++) {
                rayCount++;
                if (!bvh.Intersect(triangles, ref ray, RayEpsilon, float.PositiveInfinity, traversalStack, out SoftwareHit hit, out int triangleIndex)) {
                    return radiance;
                }

                SoftwareTriangle triangle = triangles[triangleIndex];
                SoftwareMaterialData material = materials[triangle.MaterialIndex];
                float3 orientedNormal = OrientAgainstIncoming(triangle.GeometricNormal, ray.Direction);

                if (HasEmission(material.Emission)) {
                    if (bounce == 0) {
                        radiance = AddColor(radiance, MultiplyColor(throughput, material.Emission));
                        if (!IsFinite(radiance)) {
                            return DiscardSample();
                        }
                    }
                    return radiance;
                }

                float firstLightSample = SoftwarePathSampler.Sample01(pixelX, pixelY, completedPass, bounce, 0);
                float secondLightSample = SoftwarePathSampler.Sample01(pixelX, pixelY, completedPass, bounce, 1);
                SoftwareAreaLight sampledLight = areaLight;
                float3 lightPoint = SoftwarePathSampler.SampleAreaLight(ref sampledLight, firstLightSample, secondLightSample);
                if (!IsFinite(lightPoint)) {
                    return DiscardSample();
                }

                float3 toLight = Subtract(lightPoint, hit.Position);
                float distanceSquared = LengthSquared(toLight);
                if (!float.IsFinite(distanceSquared)) {
                    return DiscardSample();
                }
                if (distanceSquared > 0f) {
                    float distance = (float)Math.Sqrt(distanceSquared);
                    if (!float.IsFinite(distance) || distance <= 0f) {
                        return DiscardSample();
                    }

                    float3 lightDirection = Scale(toLight, 1f / distance);
                    if (!IsFinite(lightDirection)) {
                        return DiscardSample();
                    }
                    float cosSurface = float3.Dot(orientedNormal, lightDirection);
                    float cosLight = float3.Dot(sampledLight.InwardNormal, Negate(lightDirection));
                    if (!float.IsFinite(cosSurface) || !float.IsFinite(cosLight)) {
                        return DiscardSample();
                    }
                    cosSurface = Math.Max(0f, cosSurface);
                    cosLight = Math.Max(0f, cosLight);
                    float geometry = (cosSurface * cosLight) / distanceSquared;
                    if (!float.IsFinite(geometry)) {
                        return DiscardSample();
                    }

                    if (geometry > 0f && distance > RayEpsilon) {
                        float3 shadowOrigin = Add(hit.Position, Scale(orientedNormal, RayEpsilon));
                        float3 shadowToLight = Subtract(lightPoint, shadowOrigin);
                        float shadowDistanceSquared = LengthSquared(shadowToLight);
                        if (!float.IsFinite(shadowDistanceSquared) || shadowDistanceSquared <= 0f) {
                            return DiscardSample();
                        }
                        float shadowDistance = (float)Math.Sqrt(shadowDistanceSquared);
                        if (!float.IsFinite(shadowDistance) || shadowDistance <= RayEpsilon) {
                            return DiscardSample();
                        }
                        float3 shadowDirection = Scale(shadowToLight, 1f / shadowDistance);
                        if (!IsFinite(shadowDirection)) {
                            return DiscardSample();
                        }
                        float shadowMaximum = shadowDistance - RayEpsilon;
                        SoftwareRay shadowRay = new SoftwareRay(shadowOrigin, shadowDirection);
                        rayCount++;
                        bool blocked = bvh.Intersect(triangles, ref shadowRay, RayEpsilon, shadowMaximum, traversalStack, out _, out _);
                        if (!blocked) {
                            float3 direct = MultiplyColor(throughput, material.DiffuseColor);
                            direct = MultiplyColor(direct, sampledLight.Emission);
                            direct = Scale(direct, (sampledLight.Area / Pi) * geometry);
                            if (!IsFinite(direct)) {
                                return DiscardSample();
                            }
                            radiance = AddColor(radiance, direct);
                            if (!IsFinite(radiance)) {
                                return DiscardSample();
                            }
                        }
                    }
                }

                throughput = MultiplyColor(throughput, material.DiffuseColor);
                if (!IsFinite(throughput)) {
                    return DiscardSample();
                }
                if (bounce == MaximumDiffuseBounces - 1) {
                    break;
                }

                float firstBounceSample = SoftwarePathSampler.Sample01(pixelX, pixelY, completedPass, bounce, 2);
                float secondBounceSample = SoftwarePathSampler.Sample01(pixelX, pixelY, completedPass, bounce, 3);
                float3 outgoingDirection = SoftwarePathSampler.SampleCosineHemisphere(orientedNormal, firstBounceSample, secondBounceSample);
                if (!IsFinite(outgoingDirection)) {
                    return DiscardSample();
                }
                float3 nextOrigin = Add(hit.Position, Scale(orientedNormal, RayEpsilon));
                if (!IsFinite(nextOrigin)) {
                    return DiscardSample();
                }
                ray = new SoftwareRay(nextOrigin, outgoingDirection);
            }

            return IsFinite(radiance) ? radiance : DiscardSample();
        }

        /// <summary>
        /// Validates all borrowed compact scene state once at construction.
        /// </summary>
        /// <param name="triangles">Triangle array to validate.</param>
        /// <param name="materials">Material array to validate.</param>
        /// <param name="areaLight">Area light to validate.</param>
        /// <param name="bvh">BVH to retain.</param>
        /// <param name="traversalStack">Traversal scratch to retain.</param>
        static void ValidateSceneInputs(SoftwareTriangle[] triangles, SoftwareMaterialData[] materials, SoftwareAreaLight areaLight, SoftwareBvh bvh, int[] traversalStack) {
            if (triangles == null) {
                throw new ArgumentNullException(nameof(triangles));
            }
            if (triangles.Length == 0) {
                throw new ArgumentException("A software path tracer requires at least one triangle.", nameof(triangles));
            }
            if (materials == null) {
                throw new ArgumentNullException(nameof(materials));
            }
            if (materials.Length == 0) {
                throw new ArgumentException("A software path tracer requires at least one material.", nameof(materials));
            }
            if (bvh == null) {
                throw new ArgumentNullException(nameof(bvh));
            }
            if (traversalStack == null) {
                throw new ArgumentNullException(nameof(traversalStack));
            }
            if (traversalStack.Length < SoftwareBvh.TraversalStackCapacity) {
                throw new ArgumentException("Traversal scratch is smaller than the fixed BVH capacity.", nameof(traversalStack));
            }

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++) {
                SoftwareMaterialData material = materials[materialIndex];
                if (!IsFinite(material.DiffuseColor) || !IsFinite(material.Emission)) {
                    throw new ArgumentOutOfRangeException(nameof(materials), "Compact material values must be finite.");
                }
                if (HasNegativeComponent(material.DiffuseColor) || HasNegativeComponent(material.Emission)) {
                    throw new ArgumentOutOfRangeException(nameof(materials), "Compact material colors must be non-negative.");
                }
            }

            for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex++) {
                SoftwareTriangle triangle = triangles[triangleIndex];
                if ((uint)triangle.MaterialIndex >= (uint)materials.Length) {
                    throw new ArgumentOutOfRangeException(nameof(triangles), "A compact triangle references an invalid material.");
                }
                if (!IsFinite(triangle.P0) || !IsFinite(triangle.Edge1) || !IsFinite(triangle.Edge2) || !IsFinite(triangle.GeometricNormal) || !IsFinite(triangle.Centroid) || !IsFinite(triangle.BoundsMin) || !IsFinite(triangle.BoundsMax)) {
                    throw new ArgumentOutOfRangeException(nameof(triangles), "Compact triangle values must be finite.");
                }
                SoftwarePathSampler.ValidateUnitNormal(triangle.GeometricNormal, nameof(triangles));
                if (triangle.BoundsMin.X > triangle.BoundsMax.X || triangle.BoundsMin.Y > triangle.BoundsMax.Y || triangle.BoundsMin.Z > triangle.BoundsMax.Z) {
                    throw new ArgumentOutOfRangeException(nameof(triangles), "Compact triangle bounds must be ordered.");
                }
            }

            SoftwarePathSampler.ValidateLightGeometry(areaLight);
            if ((uint)areaLight.FirstTriangleIndex >= (uint)triangles.Length || (uint)areaLight.SecondTriangleIndex >= (uint)triangles.Length || areaLight.FirstTriangleIndex == areaLight.SecondTriangleIndex) {
                throw new ArgumentOutOfRangeException(nameof(areaLight), "The area light references invalid emitter triangles.");
            }
        }

        /// <summary>
        /// Validates the public primary-sample boundary before changing counters.
        /// </summary>
        /// <param name="primaryRay">Primary ray to validate.</param>
        /// <param name="pixelX">Pixel x identity.</param>
        /// <param name="pixelY">Pixel y identity.</param>
        /// <param name="completedPass">Completed-pass identity.</param>
        static void ValidateSampleBoundary(ref SoftwareRay primaryRay, int pixelX, int pixelY, int completedPass) {
            if (pixelX < 0) {
                throw new ArgumentOutOfRangeException(nameof(pixelX));
            }
            if (pixelY < 0) {
                throw new ArgumentOutOfRangeException(nameof(pixelY));
            }
            if (completedPass < 0) {
                throw new ArgumentOutOfRangeException(nameof(completedPass));
            }
            if (!IsFinite(primaryRay.Origin) || !IsFinite(primaryRay.Direction)) {
                throw new ArgumentOutOfRangeException(nameof(primaryRay), "Primary ray values must be finite.");
            }
            float directionLengthSquared = LengthSquared(primaryRay.Direction);
            if (!float.IsFinite(directionLengthSquared) || directionLengthSquared <= 0f || Math.Abs(directionLengthSquared - 1f) > PrimaryDirectionTolerance) {
                throw new ArgumentOutOfRangeException(nameof(primaryRay), "Primary direction must be finite, non-zero, and normalized.");
            }
        }

        /// <summary>
        /// Orients a geometric normal against the incoming ray direction.
        /// </summary>
        /// <param name="normal">Stored geometric normal.</param>
        /// <param name="incomingDirection">Direction from the previous ray origin to the hit.</param>
        /// <returns>A normal facing the incoming ray origin.</returns>
        static float3 OrientAgainstIncoming(float3 normal, float3 incomingDirection) {
            return float3.Dot(normal, incomingDirection) > 0f ? Negate(normal) : normal;
        }

        /// <summary>
        /// Tests whether an emission vector contributes radiance.
        /// </summary>
        /// <param name="emission">Emission vector.</param>
        /// <returns>True when at least one channel is non-zero.</returns>
        static bool HasEmission(float3 emission) {
            return emission.X != 0f || emission.Y != 0f || emission.Z != 0f;
        }

        /// <summary>
        /// Tests whether one color has a negative component.
        /// </summary>
        /// <param name="color">Color to inspect.</param>
        /// <returns>True when any component is negative.</returns>
        static bool HasNegativeComponent(float3 color) {
            return color.X < 0f || color.Y < 0f || color.Z < 0f;
        }

        /// <summary>
        /// Increments the discard counter and returns an empty radiance value.
        /// </summary>
        /// <returns>Zero radiance for the discarded sample.</returns>
        float3 DiscardSample() {
            nonFiniteSampleCount++;
            return float3.Zero;
        }

        /// <summary>
        /// Adds two RGB colors component by component.
        /// </summary>
        /// <param name="first">First RGB color.</param>
        /// <param name="second">Second RGB color.</param>
        /// <returns>The component-wise sum.</returns>
        static float3 AddColor(float3 first, float3 second) {
            return new float3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        /// <summary>
        /// Adds two geometric vectors component by component.
        /// </summary>
        /// <param name="first">First vector.</param>
        /// <param name="second">Second vector.</param>
        /// <returns>The component-wise sum.</returns>
        static float3 Add(float3 first, float3 second) {
            return new float3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        /// <summary>
        /// Subtracts one vector from another component by component.
        /// </summary>
        /// <param name="first">Minuend.</param>
        /// <param name="second">Subtrahend.</param>
        /// <returns>The component-wise difference.</returns>
        static float3 Subtract(float3 first, float3 second) {
            return new float3(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
        }

        /// <summary>
        /// Negates one vector component by component.
        /// </summary>
        /// <param name="value">Vector to negate.</param>
        /// <returns>The negated vector.</returns>
        static float3 Negate(float3 value) {
            return new float3(-value.X, -value.Y, -value.Z);
        }

        /// <summary>
        /// Multiplies two RGB colors component by component.
        /// </summary>
        /// <param name="first">First RGB color.</param>
        /// <param name="second">Second RGB color.</param>
        /// <returns>The component-wise product.</returns>
        static float3 MultiplyColor(float3 first, float3 second) {
            return new float3(first.X * second.X, first.Y * second.Y, first.Z * second.Z);
        }

        /// <summary>
        /// Scales one RGB color component by one scalar.
        /// </summary>
        /// <param name="value">RGB color to scale.</param>
        /// <param name="scale">Scalar multiplier.</param>
        /// <returns>The scaled RGB color.</returns>
        static float3 Scale(float3 value, float scale) {
            return new float3(value.X * scale, value.Y * scale, value.Z * scale);
        }

        /// <summary>
        /// Tests all components of one vector for finite values.
        /// </summary>
        /// <param name="value">Vector to inspect.</param>
        /// <returns>True when all vector components are finite.</returns>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }

        /// <summary>
        /// Computes a vector squared length without allocation.
        /// </summary>
        /// <param name="value">Vector to measure.</param>
        /// <returns>The squared length.</returns>
        static float LengthSquared(float3 value) {
            return (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z);
        }
    }
}
