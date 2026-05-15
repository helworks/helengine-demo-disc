using city.scene.tools;

namespace city.rendering.tools {
    /// <summary>
    /// Generates the complete city rendering showcase scene set inside the active project.
    /// </summary>
    public sealed class RenderingSceneGenerator {
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
        /// Writer used to persist generated live-authored scenes through the editor save pipeline.
        /// </summary>
        readonly GeneratedAuthoringSceneWriteService AuthoringSceneWriteService;

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
            AuthoringSceneWriteService = new GeneratedAuthoringSceneWriteService();
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
        /// <param name="assets">Prepared runtime assets required by the showcase scene factories.</param>
        public void Generate(string projectRootPath, RenderingSceneGenerationAssets assets) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (assets == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedCubeModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedPlaneModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedSphereModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedStandardMaterial == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedArrowModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.AxisMaterials == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.RacerMaterials == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.LamppostModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.RacerModel == null) {
                throw new ArgumentNullException(nameof(assets));
            }

            GeneratedAuthoringSceneDefinition cubeTestSceneDefinition = CubeTestFactory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedStandardMaterial);
            GeneratedAuthoringSceneDefinition coloredCubeGridSceneDefinition;
            GeneratedAuthoringSceneDefinition texturedCubeGridSceneDefinition;
            GeneratedAuthoringSceneDefinition axisTestSceneDefinition = AxisTestFactory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedArrowModel, assets.AxisMaterials);
            GeneratedAuthoringSceneDefinition axisTest2SceneDefinition = AxisTest2Factory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedArrowModel, assets.AxisMaterials);
            GeneratedAuthoringSceneDefinition directionalShadowPlazaSceneDefinition = DirectionalShadowPlazaFactory.CreateSceneDefinition(assets.GeneratedPlaneModel, assets.GeneratedCubeModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial);
            GeneratedAuthoringSceneDefinition spotlightStreetSliceSceneDefinition = SpotlightStreetSliceFactory.CreateSceneDefinition(assets.GeneratedPlaneModel, assets.GeneratedCubeModel, assets.GeneratedStandardMaterial, assets.LamppostModel, assets.RacerModel, assets.RacerMaterials);
            coloredCubeGridSceneDefinition = ColoredCubeGridFactory.CreateSceneDefinition(assets.GeneratedCubeModel, ColoredCubeGridFactory.CreateRuntimeMaterials());
            texturedCubeGridSceneDefinition = TexturedCubeGridFactory.CreateSceneDefinition(assets.GeneratedCubeModel, TexturedCubeGridFactory.CreateRuntimeMaterials(assets.GeneratedStandardMaterial));
            AuthoringSceneWriteService.WriteScene(projectRootPath, cubeTestSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, coloredCubeGridSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, texturedCubeGridSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, axisTestSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, axisTest2SceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, directionalShadowPlazaSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, spotlightStreetSliceSceneDefinition);
        }
    }
}
