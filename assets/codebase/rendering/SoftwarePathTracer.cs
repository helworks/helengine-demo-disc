using System;
using helengine;

namespace city.rendering {
    /// <summary>
    /// Describes one validated CPU-trace image resolution and its accumulator footprint.
    /// </summary>
    public readonly struct SoftwareTraceResolution {
        /// <summary>Bytes owned by one accumulated float3 pixel.</summary>
        public const int AccumulatorBytesPerPixel = 12;

        /// <summary>Image width in pixels.</summary>
        public int Width { get; }

        /// <summary>Image height in pixels.</summary>
        public int Height { get; }

        /// <summary>Total number of image pixels.</summary>
        public int PixelCount { get; }

        /// <summary>Exact byte count of the float3 accumulation array.</summary>
        public long AccumulatorBytes { get; }

        /// <summary>
        /// Initializes one positive checked image resolution.
        /// </summary>
        /// <param name="width">Positive image width.</param>
        /// <param name="height">Positive image height.</param>
        public SoftwareTraceResolution(int width, int height) {
            if (width <= 0) {
                throw new ArgumentOutOfRangeException(nameof(width), "Trace dimensions must be positive.");
            }
            if (height <= 0) {
                throw new ArgumentOutOfRangeException(nameof(height), "Trace dimensions must be positive.");
            }

            int pixelCount;
            try {
                pixelCount = checked(width * height);
            }
            catch (OverflowException) {
                throw new ArgumentOutOfRangeException(nameof(width), "Trace dimensions exceed the supported pixel count.");
            }

            Width = width;
            Height = height;
            PixelCount = pixelCount;
            AccumulatorBytes = checked((long)pixelCount * AccumulatorBytesPerPixel);
        }

        /// <summary>
        /// Resolves the fixed showcase resolution for one exact platform identifier.
        /// </summary>
        /// <param name="platformId">Ordinal platform identifier.</param>
        /// <returns>256x192 for the exact ds identifier, otherwise 320x240 for a supported platform.</returns>
        public static SoftwareTraceResolution ForPlatform(string platformId) {
            if (platformId == null) {
                throw new ArgumentNullException(nameof(platformId));
            }

            if (string.Equals(platformId, "ds", StringComparison.Ordinal)) {
                return new SoftwareTraceResolution(256, 192);
            }

            if (string.Equals(platformId, "3ds", StringComparison.Ordinal) || string.Equals(platformId, "gamecube", StringComparison.Ordinal) || string.Equals(platformId, "ps2", StringComparison.Ordinal) || string.Equals(platformId, "psp", StringComparison.Ordinal) || string.Equals(platformId, "psvita", StringComparison.Ordinal) || string.Equals(platformId, "switch", StringComparison.Ordinal) || string.Equals(platformId, "wii", StringComparison.Ordinal) || string.Equals(platformId, "wiiu", StringComparison.Ordinal) || string.Equals(platformId, "windows", StringComparison.Ordinal)) {
                return new SoftwareTraceResolution(320, 240);
            }

            throw new ArgumentOutOfRangeException(nameof(platformId), platformId, "The platform is not supported by the software path tracer.");
        }
    }

    /// <summary>
    /// Stores one validated world-space camera basis for progressive tracing.
    /// </summary>
    public readonly struct SoftwareTraceCamera {
        /// <summary>Tolerance for unit-length and orthogonality checks.</summary>
        public const float BasisTolerance = 0.002f;

        /// <summary>Camera world-space origin.</summary>
        public float3 Origin { get; }

        /// <summary>Unit direction into the image.</summary>
        public float3 Forward { get; }

        /// <summary>Unit image-space right direction.</summary>
        public float3 Right { get; }

        /// <summary>Unit image-space up direction.</summary>
        public float3 Up { get; }

        /// <summary>Vertical field of view in degrees.</summary>
        public float VerticalFieldOfViewDegrees { get; }

        /// <summary>
        /// Initializes and validates one right/up/forward camera basis.
        /// </summary>
        /// <param name="origin">Finite camera origin.</param>
        /// <param name="forward">Unit direction into the image.</param>
        /// <param name="right">Unit image-space right direction.</param>
        /// <param name="up">Unit image-space up direction.</param>
        /// <param name="verticalFieldOfViewDegrees">FOV strictly between zero and 179 degrees.</param>
        public SoftwareTraceCamera(float3 origin, float3 forward, float3 right, float3 up, float verticalFieldOfViewDegrees) {
            Validate(origin, forward, right, up, verticalFieldOfViewDegrees);
            Origin = origin;
            Forward = forward;
            Right = right;
            Up = up;
            VerticalFieldOfViewDegrees = verticalFieldOfViewDegrees;
        }

        /// <summary>
        /// Validates camera finiteness, basis lengths, handedness, and field of view.
        /// </summary>
        /// <param name="origin">Camera origin.</param>
        /// <param name="forward">Forward basis.</param>
        /// <param name="right">Right basis.</param>
        /// <param name="up">Up basis.</param>
        /// <param name="verticalFieldOfViewDegrees">Vertical FOV.</param>
        static void Validate(float3 origin, float3 forward, float3 right, float3 up, float verticalFieldOfViewDegrees) {
            if (!IsFinite(origin) || !IsFinite(forward) || !IsFinite(right) || !IsFinite(up)) {
                throw new ArgumentOutOfRangeException(nameof(origin), "Camera values must be finite.");
            }
            if (!float.IsFinite(verticalFieldOfViewDegrees) || verticalFieldOfViewDegrees <= 0f || verticalFieldOfViewDegrees >= 179f) {
                throw new ArgumentOutOfRangeException(nameof(verticalFieldOfViewDegrees), "Camera FOV must be finite and lie strictly between zero and 179 degrees.");
            }

            ValidateUnit(forward, nameof(forward));
            ValidateUnit(right, nameof(right));
            ValidateUnit(up, nameof(up));
            if (Math.Abs(float3.Dot(forward, right)) > BasisTolerance || Math.Abs(float3.Dot(forward, up)) > BasisTolerance || Math.Abs(float3.Dot(right, up)) > BasisTolerance) {
                throw new ArgumentOutOfRangeException(nameof(up), "Camera basis vectors must be mutually orthogonal.");
            }

            float handedness = float3.Dot(float3.Cross(right, up), forward);
            if (!float.IsFinite(handedness) || handedness >= -1f + (BasisTolerance * 2f)) {
                throw new ArgumentOutOfRangeException(nameof(up), "Camera basis must use the forward/right/up handedness convention.");
            }
        }

        /// <summary>
        /// Validates one finite unit basis vector.
        /// </summary>
        /// <param name="value">Basis vector.</param>
        /// <param name="name">Argument name.</param>
        static void ValidateUnit(float3 value, string name) {
            float lengthSquared = (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z);
            if (!float.IsFinite(lengthSquared) || lengthSquared <= 0f || Math.Abs(lengthSquared - 1f) > BasisTolerance) {
                throw new ArgumentOutOfRangeException(name, "Camera basis vectors must be finite unit vectors.");
            }
        }

        /// <summary>
        /// Tests every vector component for finiteness.
        /// </summary>
        /// <param name="value">Vector to inspect.</param>
        /// <returns>True when every component is finite.</returns>
        static bool IsFinite(float3 value) {
            return float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
        }
    }

    /// <summary>
    /// Describes one edge-clipped progressive tile.
    /// </summary>
    public readonly struct SoftwareTraceTile {
        /// <summary>Global pixel x coordinate.</summary>
        public int X { get; }

        /// <summary>Global pixel y coordinate.</summary>
        public int Y { get; }

        /// <summary>Clipped tile width.</summary>
        public int Width { get; }

        /// <summary>Clipped tile height.</summary>
        public int Height { get; }

        /// <summary>Unpermuted tile index within the image grid.</summary>
        public int TileIndex { get; }

        /// <summary>
        /// Initializes one progressive tile descriptor.
        /// </summary>
        /// <param name="x">Global x coordinate.</param>
        /// <param name="y">Global y coordinate.</param>
        /// <param name="width">Clipped width.</param>
        /// <param name="height">Clipped height.</param>
        /// <param name="tileIndex">Unpermuted tile index.</param>
        public SoftwareTraceTile(int x, int y, int width, int height, int tileIndex) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            TileIndex = tileIndex;
        }
    }

    /// <summary>
    /// Provides the narrow allocation seam used to test progressive rollback.
    /// </summary>
    public interface ISoftwareTraceBufferAllocator {
        /// <summary>
        /// Allocates the one float3-per-pixel accumulation buffer.
        /// </summary>
        /// <param name="pixelCount">Number of pixels to allocate.</param>
        /// <returns>A buffer with exactly pixelCount elements.</returns>
        float3[] AllocateAccumulator(int pixelCount);

        /// <summary>
        /// Allocates the reusable RGBA8 tile staging buffer.
        /// </summary>
        /// <param name="byteCount">Number of bytes to allocate.</param>
        /// <returns>A buffer with exactly byteCount elements.</returns>
        byte[] AllocateTileRgba8(int byteCount);
    }

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
    /// Traces one finite scalar CPU path through compact DemoDisc geometry and one progressive worker.
    /// The progressive traversal and tile scratch are currently worker-owned; future multithreading
    /// requires worker-local scratch plus synchronized or partitioned accumulator ownership.
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
        /// Width and height of the reusable progressive tile in pixels.
        /// </summary>
        public const int TileSize = 8;

        /// <summary>
        /// Exact byte capacity of one reusable RGBA8 tile staging buffer.
        /// </summary>
        public const int TileRgba8Bytes = TileSize * TileSize * 4;

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
        /// Progressive accumulator owned by this tracer; scene and BVH arrays remain borrowed.
        /// </summary>
        float3[] accumulation = Array.Empty<float3>();

        /// <summary>
        /// Reusable RGBA8 tile staging buffer owned by this tracer.
        /// </summary>
        byte[] tileRgba8 = Array.Empty<byte>();

        /// <summary>
        /// Fixed image resolution selected during progressive initialization.
        /// </summary>
        SoftwareTraceResolution resolution;

        /// <summary>
        /// Validated camera retained for jittered primary-ray generation.
        /// </summary>
        SoftwareTraceCamera camera;

        /// <summary>
        /// Positive exposure retained for CPU tone mapping.
        /// </summary>
        float exposure;

        /// <summary>
        /// Number of fully completed image passes.
        /// </summary>
        int completedPasses;

        /// <summary>
        /// Number of tiles already rendered in the current pass.
        /// </summary>
        int nextTilePosition;

        /// <summary>
        /// Number of horizontal tiles in the progressive image grid.
        /// </summary>
        int tilesX;

        /// <summary>
        /// Number of vertical tiles in the progressive image grid.
        /// </summary>
        int tilesY;

        /// <summary>
        /// Total number of progressive tiles in one pass.
        /// </summary>
        int tileCount;

        /// <summary>
        /// Coprime permutation step used for tile scheduling.
        /// </summary>
        int permutationStep;

        /// <summary>
        /// Indicates that both progressive buffers and scheduling state are active.
        /// </summary>
        bool progressiveInitialized;

        /// <summary>
        /// Stable error used when progressive buffers cannot be allocated or validated.
        /// </summary>
        const string ProgressiveAllocationFailureMessage = "Software progressive buffers could not be allocated.";

        /// <summary>
        /// Stable error used when progressive work is requested before initialization or after disposal.
        /// </summary>
        const string ProgressiveUnavailableMessage = "Software progressive tracing is not initialized.";

        /// <summary>
        /// Stable error used when a progressive pass identity would overflow.
        /// </summary>
        const string ProgressivePassOverflowMessage = "Software progressive pass count cannot be incremented further.";

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
        /// Gets a value indicating whether progressive buffers and scheduling are initialized.
        /// </summary>
        public bool IsProgressiveInitialized {
            get { return progressiveInitialized; }
        }

        /// <summary>
        /// Gets the validated progressive image resolution.
        /// </summary>
        public SoftwareTraceResolution Resolution {
            get { return resolution; }
        }

        /// <summary>
        /// Gets the number of image passes completed before the current pass.
        /// </summary>
        public int CompletedPasses {
            get { return completedPasses; }
        }

        /// <summary>
        /// Gets the fixed byte pitch of the reusable RGBA8 tile staging buffer.
        /// </summary>
        public int TileRowPitch {
            get { return TileSize * 4; }
        }

        /// <summary>
        /// Gets the progressive accumulator owned by this tracer. Callers may reference it for
        /// verification, but must not replace or mutate the array.
        /// </summary>
        public float3[] Accumulation {
            get { return accumulation; }
        }

        /// <summary>
        /// Gets the reusable RGBA8 tile staging buffer owned by this tracer. Callers may reference
        /// it for upload, but must not replace or mutate the array.
        /// </summary>
        public byte[] TileRgba8 {
            get { return tileRgba8; }
        }

        /// <summary>
        /// Gets the exact persistent byte count of the progressive buffers.
        /// </summary>
        public long ProgressiveOwnedBytes {
            get {
                if (!progressiveInitialized) {
                    return 0L;
                }
                return resolution.AccumulatorBytes + TileRgba8Bytes;
            }
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
        /// Allocates progressive accumulation and tile buffers after scene and BVH construction.
        /// </summary>
        /// <param name="resolution">Positive checked output resolution.</param>
        /// <param name="camera">Finite orthonormal camera basis.</param>
        /// <param name="exposure">Positive finite CPU tone-mapping exposure.</param>
        /// <param name="allocator">Optional deterministic buffer allocator used by failure tests.</param>
        public void InitializeProgressive(SoftwareTraceResolution resolution, SoftwareTraceCamera camera, float exposure, ISoftwareTraceBufferAllocator allocator = null) {
            if (progressiveInitialized) {
                throw new InvalidOperationException("Software progressive tracing is already initialized.");
            }
            ValidateProgressiveCamera(camera);
            if (!float.IsFinite(exposure) || exposure <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(exposure), "Progressive exposure must be finite and positive.");
            }

            int localTilesX;
            int localTilesY;
            int localTileCount;
            try {
                localTilesX = checked((resolution.Width / TileSize) + (resolution.Width % TileSize == 0 ? 0 : 1));
                localTilesY = checked((resolution.Height / TileSize) + (resolution.Height % TileSize == 0 ? 0 : 1));
                localTileCount = checked(localTilesX * localTilesY);
            }
            catch (OverflowException) {
                throw new ArgumentOutOfRangeException(nameof(resolution), "Progressive tile dimensions exceed the supported count.");
            }

            if (localTilesX <= 0 || localTilesY <= 0 || localTileCount <= 0) {
                throw new ArgumentOutOfRangeException(nameof(resolution), "Progressive tile dimensions must be positive.");
            }

            float3[] allocatedAccumulator = null;
            byte[] allocatedTile = null;
            try {
                allocatedAccumulator = allocator == null
                    ? new float3[resolution.PixelCount]
                    : allocator.AllocateAccumulator(resolution.PixelCount);
                if (allocatedAccumulator == null || allocatedAccumulator.Length != resolution.PixelCount) {
                    throw new InvalidOperationException("The progressive accumulator allocator returned an invalid buffer.");
                }
                allocatedTile = allocator == null
                    ? new byte[TileRgba8Bytes]
                    : allocator.AllocateTileRgba8(TileRgba8Bytes);
                if (allocatedTile == null || allocatedTile.Length != TileRgba8Bytes) {
                    throw new InvalidOperationException("The progressive tile allocator returned an invalid buffer.");
                }

                this.resolution = resolution;
                this.camera = camera;
                this.exposure = exposure;
                this.tilesX = localTilesX;
                this.tilesY = localTilesY;
                this.tileCount = localTileCount;
                this.permutationStep = ChoosePermutationStep(localTileCount);
                this.nextTilePosition = 0;
                this.completedPasses = 0;
                this.accumulation = allocatedAccumulator;
                this.tileRgba8 = allocatedTile;
                this.progressiveInitialized = true;
            }
            catch (Exception exception) {
                ResetProgressiveState();
                throw new InvalidOperationException(ProgressiveAllocationFailureMessage, exception);
            }
        }

        /// <summary>
        /// Renders one deterministic edge-clipped tile, updates its accumulation, and tone-maps it.
        /// </summary>
        /// <returns>The tile rectangle whose bytes are currently in TileRgba8.</returns>
        public SoftwareTraceTile RenderNextTile() {
            if (!progressiveInitialized) {
                throw new InvalidOperationException(ProgressiveUnavailableMessage);
            }
            if (completedPasses == int.MaxValue) {
                throw new InvalidOperationException(ProgressivePassOverflowMessage);
            }

            int tileIndex = MapTileIndex(nextTilePosition, completedPasses, permutationStep, tileCount);
            int tileX = tileIndex % tilesX;
            int tileY = tileIndex / tilesX;
            int originX = tileX * TileSize;
            int originY = tileY * TileSize;
            int tileWidth = Math.Min(TileSize, resolution.Width - originX);
            int tileHeight = Math.Min(TileSize, resolution.Height - originY);
            float divisor = completedPasses + 1f;

            for (int localY = 0; localY < tileHeight; localY++) {
                int globalY = originY + localY;
                for (int localX = 0; localX < tileWidth; localX++) {
                    int globalX = originX + localX;
                    int pixelIndex = (globalY * resolution.Width) + globalX;
                    SoftwareRay primaryRay = CreateCameraRayInternal(globalX, globalY, completedPasses);
                    float3 sample = TraceSample(ref primaryRay, globalX, globalY, completedPasses);
                    float3 previous = accumulation[pixelIndex];
                    float3 updated = AddColor(previous, sample);
                    if (IsFinite(updated)) {
                        accumulation[pixelIndex] = updated;
                    }
                    else {
                        updated = previous;
                    }

                    float3 average = Scale(updated, 1f / divisor);
                    int destination = (localY * TileRowPitch) + (localX * 4);
                    WriteToneMappedPixel(average, destination);
                }
            }

            nextTilePosition++;
            if (nextTilePosition >= tileCount) {
                nextTilePosition = 0;
                completedPasses++;
            }

            return new SoftwareTraceTile(originX, originY, tileWidth, tileHeight, tileIndex);
        }

        /// <summary>
        /// Releases only progressive-owned arrays and state; borrowed scene state remains valid.
        /// </summary>
        public void DisposeProgressive() {
            ResetProgressiveState();
        }

        /// <summary>
        /// Generates a camera ray for the current completed pass and validates the pixel identity.
        /// </summary>
        /// <param name="pixelX">Non-negative pixel x coordinate.</param>
        /// <param name="pixelY">Non-negative pixel y coordinate.</param>
        /// <returns>A finite normalized jittered primary ray.</returns>
        public SoftwareRay CreateCameraRay(int pixelX, int pixelY) {
            if (!progressiveInitialized) {
                throw new InvalidOperationException(ProgressiveUnavailableMessage);
            }
            return CreateCameraRay(pixelX, pixelY, completedPasses);
        }

        /// <summary>
        /// Generates a deterministic camera ray for an explicit non-negative pass identity.
        /// </summary>
        /// <param name="pixelX">Non-negative pixel x coordinate.</param>
        /// <param name="pixelY">Non-negative pixel y coordinate.</param>
        /// <param name="pass">Non-negative completed-pass identity.</param>
        /// <returns>A finite normalized jittered primary ray.</returns>
        public SoftwareRay CreateCameraRay(int pixelX, int pixelY, int pass) {
            if (!progressiveInitialized) {
                throw new InvalidOperationException(ProgressiveUnavailableMessage);
            }
            if (pixelX < 0 || pixelX >= resolution.Width) {
                throw new ArgumentOutOfRangeException(nameof(pixelX));
            }
            if (pixelY < 0 || pixelY >= resolution.Height) {
                throw new ArgumentOutOfRangeException(nameof(pixelY));
            }
            if (pass < 0) {
                throw new ArgumentOutOfRangeException(nameof(pass));
            }
            return CreateCameraRayInternal(pixelX, pixelY, pass);
        }

        /// <summary>
        /// Generates one normalized jittered primary ray without public-boundary checks.
        /// </summary>
        /// <param name="pixelX">Pixel x coordinate.</param>
        /// <param name="pixelY">Pixel y coordinate.</param>
        /// <param name="pass">Completed-pass identity.</param>
        /// <returns>The normalized world-space camera ray.</returns>
        SoftwareRay CreateCameraRayInternal(int pixelX, int pixelY, int pass) {
            float jitterX = SoftwarePathSampler.Sample01(pixelX, pixelY, pass, -1, 0);
            float jitterY = SoftwarePathSampler.Sample01(pixelX, pixelY, pass, -1, 1);
            float normalizedX = (((pixelX + jitterX) / resolution.Width) * 2f) - 1f;
            float normalizedY = 1f - (((pixelY + jitterY) / resolution.Height) * 2f);
            float aspect = (float)resolution.Width / resolution.Height;
            float tangent = (float)Math.Tan((camera.VerticalFieldOfViewDegrees * (Pi / 180f)) * 0.5f);
            float3 direction = Add(camera.Forward, Add(Scale(camera.Right, normalizedX * aspect * tangent), Scale(camera.Up, normalizedY * tangent)));
            direction = float3.Normalize(direction);
            return new SoftwareRay(camera.Origin, direction);
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
                        if (!IsFinite(shadowOrigin)) {
                            return DiscardSample();
                        }
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
        /// Releases progressive buffers and resets all progressive scheduling state.
        /// </summary>
        void ResetProgressiveState() {
            progressiveInitialized = false;
            accumulation = Array.Empty<float3>();
            tileRgba8 = Array.Empty<byte>();
            resolution = default;
            camera = default;
            exposure = 0f;
            completedPasses = 0;
            nextTilePosition = 0;
            tilesX = 0;
            tilesY = 0;
            tileCount = 0;
            permutationStep = 0;
        }

        /// <summary>
        /// Validates camera values again at the progressive initialization boundary.
        /// </summary>
        /// <param name="value">Camera to validate.</param>
        static void ValidateProgressiveCamera(SoftwareTraceCamera value) {
            if (!IsFinite(value.Origin) || !IsFinite(value.Forward) || !IsFinite(value.Right) || !IsFinite(value.Up)) {
                throw new ArgumentOutOfRangeException(nameof(value), "Camera values must be finite.");
            }
            if (!float.IsFinite(value.VerticalFieldOfViewDegrees) || value.VerticalFieldOfViewDegrees <= 0f || value.VerticalFieldOfViewDegrees >= 179f) {
                throw new ArgumentOutOfRangeException(nameof(value), "Camera FOV must be finite and lie strictly between zero and 179 degrees.");
            }

            ValidateUnitCameraVector(value.Forward, nameof(value));
            ValidateUnitCameraVector(value.Right, nameof(value));
            ValidateUnitCameraVector(value.Up, nameof(value));
            if (Math.Abs(float3.Dot(value.Forward, value.Right)) > SoftwareTraceCamera.BasisTolerance || Math.Abs(float3.Dot(value.Forward, value.Up)) > SoftwareTraceCamera.BasisTolerance || Math.Abs(float3.Dot(value.Right, value.Up)) > SoftwareTraceCamera.BasisTolerance) {
                throw new ArgumentOutOfRangeException(nameof(value), "Camera basis vectors must be mutually orthogonal.");
            }
            float handedness = float3.Dot(float3.Cross(value.Right, value.Up), value.Forward);
            if (!float.IsFinite(handedness) || handedness >= -1f + (SoftwareTraceCamera.BasisTolerance * 2f)) {
                throw new ArgumentOutOfRangeException(nameof(value), "Camera basis must use the forward/right/up handedness convention.");
            }
        }

        /// <summary>
        /// Validates one camera basis vector at the progressive boundary.
        /// </summary>
        /// <param name="value">Basis vector.</param>
        /// <param name="name">Argument name.</param>
        static void ValidateUnitCameraVector(float3 value, string name) {
            float lengthSquared = LengthSquared(value);
            if (!float.IsFinite(lengthSquared) || lengthSquared <= 0f || Math.Abs(lengthSquared - 1f) > SoftwareTraceCamera.BasisTolerance) {
                throw new ArgumentOutOfRangeException(name, "Camera basis vectors must be finite unit vectors.");
            }
        }

        /// <summary>
        /// Maps one sequence position to a deterministic coprime-permuted tile index.
        /// </summary>
        /// <param name="position">Position within the current pass.</param>
        /// <param name="pass">Completed-pass identity.</param>
        /// <param name="step">Coprime permutation step.</param>
        /// <param name="count">Number of tiles in the image.</param>
        /// <returns>The unpermuted tile index.</returns>
        static int MapTileIndex(int position, int pass, int step, int count) {
            int offset = GetPassOffset(pass, count);
            return (int)((((long)position * step) + offset) % count);
        }

        /// <summary>
        /// Computes a deterministic non-zero offset for every pass after the first.
        /// </summary>
        /// <param name="pass">Completed-pass identity.</param>
        /// <param name="count">Number of tiles in the image.</param>
        /// <returns>A valid tile offset.</returns>
        static int GetPassOffset(int pass, int count) {
            if (pass <= 0 || count <= 1) {
                return 0;
            }

            long offset = (((long)pass * 2654435761L) + 1013904223L) % count;
            if (offset == 0L) {
                offset = 1L;
            }
            return (int)offset;
        }

        /// <summary>
        /// Chooses the near-golden-ratio coprime step for a tile permutation.
        /// </summary>
        /// <param name="count">Number of tiles in the image.</param>
        /// <returns>A non-zero step relatively prime to count.</returns>
        static int ChoosePermutationStep(int count) {
            if (count <= 1) {
                return 1;
            }

            long candidateValue = (long)Math.Floor(count * 0.6180339887498948482d);
            if (candidateValue <= 0L) {
                candidateValue = 1L;
            }
            candidateValue %= count;
            if (candidateValue == 0L) {
                candidateValue = 1L;
            }

            int candidate = (int)candidateValue;
            while (GreatestCommonDivisor(candidate, count) != 1) {
                candidate++;
                if (candidate >= count) {
                    candidate = 1;
                }
            }
            return candidate;
        }

        /// <summary>
        /// Computes the greatest common divisor without allocating.
        /// </summary>
        /// <param name="first">First positive integer.</param>
        /// <param name="second">Second positive integer.</param>
        /// <returns>The greatest common divisor.</returns>
        static int GreatestCommonDivisor(int first, int second) {
            while (second != 0) {
                int remainder = first % second;
                first = second;
                second = remainder;
            }
            return first;
        }

        /// <summary>
        /// Writes one tone-mapped average color into the reusable tile staging buffer.
        /// </summary>
        /// <param name="average">Linear average radiance.</param>
        /// <param name="destination">RGBA byte offset in TileRgba8.</param>
        void WriteToneMappedPixel(float3 average, int destination) {
            tileRgba8[destination] = ToneMapChannel(average.X);
            tileRgba8[destination + 1] = ToneMapChannel(average.Y);
            tileRgba8[destination + 2] = ToneMapChannel(average.Z);
            tileRgba8[destination + 3] = 255;
        }

        /// <summary>
        /// Applies exposure, ACES fitted tone mapping, sRGB conversion, and nearest-byte quantization.
        /// </summary>
        /// <param name="value">One linear average radiance channel.</param>
        /// <returns>The finite clamped sRGB channel as an RGBA8 byte.</returns>
        byte ToneMapChannel(float value) {
            if (float.IsNaN(value) || value <= 0f) {
                return 0;
            }
            if (float.IsPositiveInfinity(value)) {
                return 255;
            }

            float exposed = value * exposure;
            if (float.IsNaN(exposed) || exposed <= 0f) {
                return 0;
            }
            if (float.IsPositiveInfinity(exposed)) {
                return 255;
            }

            float numerator = exposed * ((2.51f * exposed) + 0.03f);
            float denominator = (exposed * ((2.43f * exposed) + 0.59f)) + 0.14f;
            if (float.IsPositiveInfinity(numerator) || float.IsPositiveInfinity(denominator)) {
                return 255;
            }
            if (!float.IsFinite(numerator) || !float.IsFinite(denominator) || denominator <= 0f) {
                return 0;
            }
            float mapped = numerator / denominator;
            if (float.IsNaN(mapped) || mapped <= 0f) {
                return 0;
            }
            if (float.IsPositiveInfinity(mapped) || mapped >= 1f) {
                mapped = 1f;
            }

            float srgb = mapped <= 0.0031308f
                ? 12.92f * mapped
                : (1.055f * (float)Math.Pow(mapped, 1f / 2.4f)) - 0.055f;
            if (!float.IsFinite(srgb) || srgb <= 0f) {
                return 0;
            }
            if (srgb >= 1f) {
                return 255;
            }

            int quantized = (int)((srgb * 255f) + 0.5f);
            if (quantized <= 0) {
                return 0;
            }
            if (quantized >= 255) {
                return 255;
            }
            return (byte)quantized;
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
