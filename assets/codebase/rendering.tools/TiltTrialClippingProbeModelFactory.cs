using helengine;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Creates the positive-Y-only model used to isolate the implicated PlayStation 2 textured fast-path face from shared scene geometry.
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
        /// Atlas coordinates for the positive-Y top face's padded magenta cell.
        /// </summary>
        static readonly float2[] TopFaceUv = [
            new float2(49f / TextureWidth, 37f / TextureHeight),
            new float2(78f / TextureWidth, 37f / TextureHeight),
            new float2(78f / TextureWidth, 58f / TextureHeight),
            new float2(49f / TextureWidth, 58f / TextureHeight)
        ];

        /// <summary>
        /// Creates the fixed two-triangle positive-Y mesh while retaining full-cube bounds for unchanged frustum behavior.
        /// </summary>
        /// <returns>Serializable model asset for the colored-face clipping probe.</returns>
        public ModelAsset CreateModelAsset() {
            return new ModelAsset {
                Id = ModelAssetId,
                Positions = [
                    new float3(-0.5f, 0.5f, -0.5f), new float3(-0.5f, 0.5f, 0.5f), new float3(0.5f, 0.5f, 0.5f), new float3(0.5f, 0.5f, -0.5f)
                ],
                Normals = [
                    new float3(0f, 1f, 0f), new float3(0f, 1f, 0f), new float3(0f, 1f, 0f), new float3(0f, 1f, 0f)
                ],
                TexCoords = [.. TopFaceUv],
                Indices16 = [
                    0, 1, 2, 0, 2, 3
                ],
                BoundsMin = new float3(-0.5f, -0.5f, -0.5f),
                BoundsMax = new float3(0.5f, 0.5f, 0.5f),
                Submeshes = [
                    new ModelSubmeshAsset {
                        MaterialSlotName = "DefaultMaterial",
                        IndexStart = 0,
                        IndexCount = 6
                    }
                ]
            };
        }

        /// <summary>
        /// Writes the top-face-only probe model through the public editor asset API.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to author the native model.</param>
        public void WriteModelAsset(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            if (assetAuthoringService == null) {
                throw new ArgumentNullException(nameof(assetAuthoringService));
            }
            if (transaction == null) {
                throw new ArgumentNullException(nameof(transaction));
            }

            ModelAsset modelAsset = CreateModelAsset();
            modelAsset.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(ModelRelativePath);
            modelAsset.FormerAuthoringAssetIds = Array.Empty<string>();
            transaction.WriteAsset(ModelRelativePath, modelAsset);
        }
    }
}
