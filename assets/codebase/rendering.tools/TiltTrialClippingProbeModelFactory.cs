using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Creates the canonical six-colored-face cube model used to isolate PlayStation 2 clipping behavior from shared scene geometry.
    /// </summary>
    public sealed class TiltTrialClippingProbeModelFactory {
        /// <summary>
        /// Stable project-relative model path written by the clipping probe authoring workflow.
        /// </summary>
        public const string ModelRelativePath = "models/rendering/tilt_trial/clipping_probe_face_colors.hasset";

        /// <summary>
        /// Stable model asset identifier consumed by the clipping probe scene authoring path.
        /// </summary>
        public const string ModelAssetId = "Models.rendering.tilt_trial.ClippingProbeFaceColors";

        /// <summary>
        /// Width of the color atlas used to normalize the model texture coordinates.
        /// </summary>
        const int TextureWidth = 128;

        /// <summary>
        /// Height of the color atlas used to normalize the model texture coordinates.
        /// </summary>
        const int TextureHeight = 64;

        /// <summary>
        /// Atlas coordinates for the negative-Z back face's padded red cell.
        /// </summary>
        static readonly float2[] BackFaceUv = [
            new float2(9f / TextureWidth, 5f / TextureHeight),
            new float2(38f / TextureWidth, 5f / TextureHeight),
            new float2(38f / TextureWidth, 26f / TextureHeight),
            new float2(9f / TextureWidth, 26f / TextureHeight)
        ];

        /// <summary>
        /// Atlas coordinates for the positive-Z front face's padded green cell.
        /// </summary>
        static readonly float2[] FrontFaceUv = [
            new float2(49f / TextureWidth, 5f / TextureHeight),
            new float2(78f / TextureWidth, 5f / TextureHeight),
            new float2(78f / TextureWidth, 26f / TextureHeight),
            new float2(49f / TextureWidth, 26f / TextureHeight)
        ];

        /// <summary>
        /// Atlas coordinates for the positive-X right face's padded blue cell.
        /// </summary>
        static readonly float2[] RightFaceUv = [
            new float2(89f / TextureWidth, 5f / TextureHeight),
            new float2(118f / TextureWidth, 5f / TextureHeight),
            new float2(118f / TextureWidth, 26f / TextureHeight),
            new float2(89f / TextureWidth, 26f / TextureHeight)
        ];

        /// <summary>
        /// Atlas coordinates for the negative-X left face's padded yellow cell.
        /// </summary>
        static readonly float2[] LeftFaceUv = [
            new float2(9f / TextureWidth, 37f / TextureHeight),
            new float2(38f / TextureWidth, 37f / TextureHeight),
            new float2(38f / TextureWidth, 58f / TextureHeight),
            new float2(9f / TextureWidth, 58f / TextureHeight)
        ];

        /// <summary>
        /// Atlas coordinates for the positive-Y top face's padded magenta cell.
        /// </summary>
        static readonly float2[] TopFaceUv = [
            new float2(49f / TextureWidth, 37f / TextureHeight),
            new float2(78f / TextureWidth, 37f / TextureHeight),
            new float2(78f / TextureWidth, 58f / TextureHeight),
            new float2(49f / TextureWidth, 58f / TextureHeight)
        ];

        /// <summary>
        /// Atlas coordinates for the negative-Y bottom face's padded cyan cell.
        /// </summary>
        static readonly float2[] BottomFaceUv = [
            new float2(89f / TextureWidth, 37f / TextureHeight),
            new float2(118f / TextureWidth, 37f / TextureHeight),
            new float2(118f / TextureWidth, 58f / TextureHeight),
            new float2(89f / TextureWidth, 58f / TextureHeight)
        ];

        /// <summary>
        /// Creates the fixed 12-triangle cube mesh with one normal and atlas region per canonical face.
        /// </summary>
        /// <returns>Serializable model asset for the colored-face clipping probe.</returns>
        public ModelAsset CreateModelAsset() {
            return new ModelAsset {
                Id = ModelAssetId,
                Positions = [
                    new float3(-0.5f, -0.5f, -0.5f), new float3(-0.5f, 0.5f, -0.5f), new float3(0.5f, 0.5f, -0.5f), new float3(0.5f, -0.5f, -0.5f),
                    new float3(-0.5f, -0.5f, 0.5f), new float3(0.5f, -0.5f, 0.5f), new float3(0.5f, 0.5f, 0.5f), new float3(-0.5f, 0.5f, 0.5f),
                    new float3(0.5f, -0.5f, -0.5f), new float3(0.5f, 0.5f, -0.5f), new float3(0.5f, 0.5f, 0.5f), new float3(0.5f, -0.5f, 0.5f),
                    new float3(-0.5f, -0.5f, -0.5f), new float3(-0.5f, -0.5f, 0.5f), new float3(-0.5f, 0.5f, 0.5f), new float3(-0.5f, 0.5f, -0.5f),
                    new float3(-0.5f, 0.5f, -0.5f), new float3(-0.5f, 0.5f, 0.5f), new float3(0.5f, 0.5f, 0.5f), new float3(0.5f, 0.5f, -0.5f),
                    new float3(-0.5f, -0.5f, -0.5f), new float3(0.5f, -0.5f, -0.5f), new float3(0.5f, -0.5f, 0.5f), new float3(-0.5f, -0.5f, 0.5f)
                ],
                Normals = [
                    new float3(0f, 0f, -1f), new float3(0f, 0f, -1f), new float3(0f, 0f, -1f), new float3(0f, 0f, -1f),
                    new float3(0f, 0f, 1f), new float3(0f, 0f, 1f), new float3(0f, 0f, 1f), new float3(0f, 0f, 1f),
                    new float3(1f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 0f, 0f),
                    new float3(-1f, 0f, 0f), new float3(-1f, 0f, 0f), new float3(-1f, 0f, 0f), new float3(-1f, 0f, 0f),
                    new float3(0f, 1f, 0f), new float3(0f, 1f, 0f), new float3(0f, 1f, 0f), new float3(0f, 1f, 0f),
                    new float3(0f, -1f, 0f), new float3(0f, -1f, 0f), new float3(0f, -1f, 0f), new float3(0f, -1f, 0f)
                ],
                TexCoords = [.. BackFaceUv, .. FrontFaceUv, .. RightFaceUv, .. LeftFaceUv, .. TopFaceUv, .. BottomFaceUv],
                Indices16 = [
                    0, 1, 2, 0, 2, 3,
                    4, 5, 6, 4, 6, 7,
                    8, 9, 10, 8, 10, 11,
                    12, 13, 14, 12, 14, 15,
                    16, 17, 18, 16, 18, 19,
                    20, 21, 22, 20, 22, 23
                ],
                BoundsMin = new float3(-0.5f, -0.5f, -0.5f),
                BoundsMax = new float3(0.5f, 0.5f, 0.5f),
                Submeshes = [
                    new ModelSubmeshAsset {
                        MaterialSlotName = "DefaultMaterial",
                        IndexStart = 0,
                        IndexCount = 36
                    }
                ]
            };
        }

        /// <summary>
        /// Serializes the colored-face probe model into its deterministic project asset location.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative DemoDisc project root path.</param>
        public void WriteModelAsset(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullModelPath = Path.Combine(Path.GetFullPath(projectRootPath), "assets", ModelRelativePath.Replace('/', Path.DirectorySeparatorChar));
            string modelDirectoryPath = Path.GetDirectoryName(fullModelPath);
            if (string.IsNullOrWhiteSpace(modelDirectoryPath)) {
                throw new InvalidOperationException($"Could not resolve a model directory for '{ModelRelativePath}'.");
            }

            Directory.CreateDirectory(modelDirectoryPath);
            using FileStream stream = File.Create(fullModelPath);
            global::helengine.editor.AssetSerializer.Serialize(stream, CreateModelAsset());
        }
    }
}
