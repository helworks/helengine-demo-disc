using city.scene.tools;

namespace city.rendering.tools {
    /// <summary>
    /// Generates the complete city rendering showcase scene set inside the active project.
    /// </summary>
    public sealed class RenderingSceneGenerator {
        /// <summary>
        /// Stable generated provider identifier used for built-in primitive assets.
        /// </summary>
        const string GeneratedProviderId = EngineGeneratedAssetProvider.ProviderIdValue;

        /// <summary>
        /// Stable scene id used by the cube-test showcase.
        /// </summary>
        public const string CubeTestSceneId = "scenes/rendering/cube_test.helen";

        /// <summary>
        /// Stable scene id used by the colored cube-grid showcase.
        /// </summary>
        public const string ColoredCubeGridSceneId = "scenes/rendering/colored_cube_grid.helen";

        /// <summary>
        /// Stable scene id used by the directional-shadow plaza showcase.
        /// </summary>
        public const string DirectionalShadowPlazaSceneId = "scenes/rendering/directional_shadow_plaza.helen";

        /// <summary>
        /// Stable scene id used by the textured cube-grid showcase.
        /// </summary>
        public const string TexturedCubeGridSceneId = "scenes/rendering/textured_cube_grid.helen";

        /// <summary>
        /// Stable scene id used by the spotlight street-slice showcase.
        /// </summary>
        public const string SpotlightStreetSliceSceneId = "scenes/rendering/spotlight_street_slice.helen";

        /// <summary>
        /// Writer used to persist generated scene assets into the active city project.
        /// </summary>
        readonly GeneratedSceneWriteService SceneWriteService;

        /// <summary>
        /// Factory used to author the minimal cube-test scene.
        /// </summary>
        readonly CubeTestSceneFactory CubeTestFactory;

        /// <summary>
        /// Factory used to author the colored cube-grid scene and its material assets.
        /// </summary>
        readonly ColoredCubeGridSceneFactory ColoredCubeGridFactory;

        /// <summary>
        /// Factory used to author the textured cube-grid scene, its texture sources, and its material assets.
        /// </summary>
        readonly TexturedCubeGridSceneFactory TexturedCubeGridFactory;

        /// <summary>
        /// Initializes one city rendering scene generator.
        /// </summary>
        public RenderingSceneGenerator() {
            SceneWriteService = new GeneratedSceneWriteService();
            CubeTestFactory = new CubeTestSceneFactory();
            ColoredCubeGridFactory = new ColoredCubeGridSceneFactory();
            TexturedCubeGridFactory = new TexturedCubeGridSceneFactory();
        }

        /// <summary>
        /// Writes the current rendering showcase scene set into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            SceneAssetReference cubeReference = CreateGeneratedReference(EngineGeneratedAssetProvider.CubeRelativePath, EngineGeneratedModelCache.CubeAssetId);
            SceneAssetReference standardMaterialReference = CreateGeneratedReference(EngineGeneratedAssetProvider.StandardMaterialRelativePath, EngineGeneratedMaterialCache.StandardAssetId);

            SceneAsset cubeTestSceneAsset = CubeTestFactory.CreateSceneAsset(cubeReference, standardMaterialReference);
            SceneAsset coloredCubeGridSceneAsset = ColoredCubeGridFactory.CreateSceneAsset(cubeReference);
            SceneAsset texturedCubeGridSceneAsset = TexturedCubeGridFactory.CreateSceneAsset(cubeReference);
            ColoredCubeGridFactory.WriteMaterialAssets(projectRootPath);
            TexturedCubeGridFactory.WriteAssets(projectRootPath);
            SceneWriteService.WriteScene(projectRootPath, CubeTestSceneId, cubeTestSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, ColoredCubeGridSceneId, coloredCubeGridSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, TexturedCubeGridSceneId, texturedCubeGridSceneAsset);
        }

        /// <summary>
        /// Creates one generated scene asset reference for a built-in engine asset.
        /// </summary>
        /// <param name="relativePath">Relative generated asset path.</param>
        /// <param name="assetId">Stable generated asset identifier.</param>
        /// <returns>Stable generated scene asset reference.</returns>
        SceneAssetReference CreateGeneratedReference(string relativePath, string assetId) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            } else if (string.IsNullOrWhiteSpace(assetId)) {
                throw new ArgumentException("Asset id must be provided.", nameof(assetId));
            }

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.Generated,
                RelativePath = relativePath,
                ProviderId = GeneratedProviderId,
                AssetId = assetId
            };
        }

    }
}
