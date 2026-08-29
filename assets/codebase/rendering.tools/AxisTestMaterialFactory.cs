using helengine;

namespace city.rendering.tools {
    /// <summary>
    /// Authors the five fixed-color materials used by the axis showcase scenes.
    /// </summary>
    public sealed class AxisTestMaterialFactory {
        const string SchemaId = "standard-shader";
        const string BaseColorFieldId = "base-color";
        const string TextureIdFieldId = "texture-id";

        static readonly (string RelativePath, string AssetId, string BaseColor)[] MaterialDefinitions = {
            ("materials/rendering/axis_test/X.hasset", "Materials.rendering.axis_test.X", "#FF4040FF"),
            ("materials/rendering/axis_test/Y.hasset", "Materials.rendering.axis_test.Y", "#40FF40FF"),
            ("materials/rendering/axis_test/Z.hasset", "Materials.rendering.axis_test.Z", "#4080FFFF"),
            ("materials/rendering/axis_test/Ground.hasset", "Materials.rendering.axis_test.Ground", "#B8C2CCFF"),
            ("materials/rendering/axis_test/Marker.hasset", "Materials.rendering.axis_test.Marker", "#FFFFFFFF")
        };

        readonly GeneratedMaterialAssetWriteService MaterialWriteService;

        /// <summary>
        /// Initializes the axis material authoring service.
        /// </summary>
        public AxisTestMaterialFactory(IEditorProjectAuthoringSession assetAuthoringService, EditorAuthoringTransaction transaction) {
            MaterialWriteService = new GeneratedMaterialAssetWriteService(assetAuthoringService, transaction);
        }

        /// <summary>
        /// Writes all axis-test materials through the shared generated material API.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets directory.</param>
        public void WriteMaterialAssets(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            for (int index = 0; index < MaterialDefinitions.Length; index++) {
                (string relativePath, string assetId, string baseColor) = MaterialDefinitions[index];
                GeneratedMaterialAssetDefinition definition = new GeneratedMaterialAssetDefinition {
                    MaterialAsset = new ShaderMaterialAsset {
                        Id = assetId,
                        RenderState = new MaterialRenderState(),
                        CastsShadows = true,
                        ReceivesShadows = true
                    }
                };
                GeneratedMaterialPlatformDefinition platform = definition.GetOrCreatePlatform("windows");
                platform.SchemaId = SchemaId;
                platform.SetFieldValue(BaseColorFieldId, baseColor);
                platform.SetFieldValue(TextureIdFieldId, string.Empty);
                MaterialWriteService.WriteMaterial(relativePath, definition);
            }
        }
    }
}
