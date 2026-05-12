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
        /// Stable scene id used by the axis-test showcase.
        /// </summary>
        public const string AxisTestSceneId = "scenes/rendering/axis_test.helen";

        /// <summary>
        /// Stable scene id used by the axis-test-2 showcase.
        /// </summary>
        public const string AxisTest2SceneId = "scenes/rendering/axis_test2.helen";

        /// <summary>
        /// Writer used to persist generated scene assets into the active city project.
        /// </summary>
        readonly GeneratedSceneWriteService SceneWriteService;

        /// <summary>
        /// Factory used to author the directional-shadow plaza scene.
        /// </summary>
        readonly DirectionalShadowPlazaSceneFactory DirectionalShadowPlazaFactory;

        /// <summary>
        /// Factory used to author the spotlight street-slice scene.
        /// </summary>
        readonly SpotlightStreetSliceSceneFactory SpotlightStreetSliceFactory;

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
        /// Factory used to author the axis-test scene and its material assets.
        /// </summary>
        readonly AxisTestSceneFactory AxisTestFactory;

        /// <summary>
        /// Factory used to author the axis-test-2 scene and its material assets.
        /// </summary>
        readonly AxisTest2SceneFactory AxisTest2Factory;

        /// <summary>
        /// Initializes one city rendering scene generator.
        /// </summary>
        public RenderingSceneGenerator() {
            SceneWriteService = new GeneratedSceneWriteService();
            DirectionalShadowPlazaFactory = new DirectionalShadowPlazaSceneFactory();
            SpotlightStreetSliceFactory = new SpotlightStreetSliceSceneFactory();
            CubeTestFactory = new CubeTestSceneFactory();
            ColoredCubeGridFactory = new ColoredCubeGridSceneFactory();
            TexturedCubeGridFactory = new TexturedCubeGridSceneFactory();
            AxisTestFactory = new AxisTestSceneFactory();
            AxisTest2Factory = new AxisTest2SceneFactory();
        }

        /// <summary>
        /// Writes the current rendering showcase scene set into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        public void Generate(string projectRootPath) {
            SceneAssetReference cubeReference = CreateGeneratedReference(EngineGeneratedAssetProvider.CubeRelativePath, EngineGeneratedModelCache.CubeAssetId);
            SceneAssetReference planeReference = CreateGeneratedReference(EngineGeneratedAssetProvider.PlaneRelativePath, EngineGeneratedModelCache.PlaneAssetId);
            SceneAssetReference sphereReference = CreateGeneratedReference(EngineGeneratedAssetProvider.SphereRelativePath, EngineGeneratedModelCache.SphereAssetId);
            SceneAssetReference standardMaterialReference = CreateGeneratedReference(EngineGeneratedAssetProvider.StandardMaterialRelativePath, EngineGeneratedMaterialCache.StandardAssetId);
            SceneAssetReference lamppostReference = CreateFileReference("models/Riemers/lamppost.x");
            SceneAssetReference racerReference = CreateFileReference("models/Riemers/racer.x");
            SceneAssetReference[] racerMaterialReferences = new[] {
                CreateFileReference("models/Riemers/racer/x3ds_mat_ruedas.helmat"),
                CreateFileReference("models/Riemers/racer/x3ds_mat_Material__0_3.helmat"),
                CreateFileReference("models/Riemers/racer/x3ds_mat_Material_1_2.helmat"),
                CreateFileReference("models/Riemers/racer/x3ds_mat_Material_2_1.helmat")
            };

            SceneAsset cubeTestSceneAsset = CubeTestFactory.CreateSceneAsset(cubeReference, standardMaterialReference);
            SceneAsset coloredCubeGridSceneAsset = ColoredCubeGridFactory.CreateSceneAsset(cubeReference);
            SceneAsset texturedCubeGridSceneAsset = TexturedCubeGridFactory.CreateSceneAsset(cubeReference);
            SceneAsset axisTestSceneAsset = AxisTestFactory.CreateSceneAsset(cubeReference);
            SceneAsset axisTest2SceneAsset = AxisTest2Factory.CreateSceneAsset(cubeReference);
            SceneAsset directionalShadowPlazaSceneAsset = DirectionalShadowPlazaFactory.CreateSceneAsset(planeReference, cubeReference, sphereReference, standardMaterialReference);
            SceneAsset spotlightStreetSliceSceneAsset = SpotlightStreetSliceFactory.CreateSceneAsset(planeReference, cubeReference, standardMaterialReference, lamppostReference, racerReference, racerMaterialReferences);
            ColoredCubeGridFactory.WriteMaterialAssets(projectRootPath);
            TexturedCubeGridFactory.WriteAssets(projectRootPath);
            AxisTestFactory.WriteAssets(projectRootPath);
            AxisTest2Factory.WriteAssets(projectRootPath);
            SceneWriteService.WriteScene(projectRootPath, CubeTestSceneId, cubeTestSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, ColoredCubeGridSceneId, coloredCubeGridSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, TexturedCubeGridSceneId, texturedCubeGridSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, AxisTestSceneId, axisTestSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, AxisTest2SceneId, axisTest2SceneAsset);
            SceneWriteService.WriteScene(projectRootPath, DirectionalShadowPlazaSceneId, directionalShadowPlazaSceneAsset);
            SceneWriteService.WriteScene(projectRootPath, SpotlightStreetSliceSceneId, spotlightStreetSliceSceneAsset);
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

        /// <summary>
        /// Creates one file-backed scene asset reference for an authored project asset.
        /// </summary>
        /// <param name="relativePath">Project-relative asset path.</param>
        /// <returns>Stable file-backed scene asset reference.</returns>
        SceneAssetReference CreateFileReference(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = relativePath.Replace('\\', '/'),
                ProviderId = string.Empty,
                AssetId = string.Empty
            };
        }

    }
}
