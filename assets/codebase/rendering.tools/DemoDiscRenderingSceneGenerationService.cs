namespace city.rendering.tools {
    /// <summary>
    /// Generates the two city-owned rendering showcase scenes that remain part of the demo-disc flow.
    /// </summary>
    public sealed class DemoDiscRenderingSceneGenerationService {
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
        /// Initializes one focused demo-disc rendering scene generation service.
        /// </summary>
        public DemoDiscRenderingSceneGenerationService() {
            AuthoringSceneWriteService = new GeneratedAuthoringSceneWriteService();
            DirectionalShadowPlazaFactory = new DirectionalShadowPlazaSceneFactory();
            SpotlightStreetSliceFactory = new SpotlightStreetSliceSceneFactory();
        }

        /// <summary>
        /// Writes the current directional-light and spotlight showcase scenes into the supplied city project.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="assets">Prepared runtime assets required by the remaining showcase scene factories.</param>
        public void Generate(string projectRootPath, RenderingSceneGenerationAssets assets) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (assets == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedPlaneModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedCubeModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedSphereModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedStandardMaterial == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.LamppostModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.RacerModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.RacerMaterials == null) {
                throw new ArgumentNullException(nameof(assets));
            }

            GeneratedAuthoringSceneDefinition directionalShadowPlazaSceneDefinition = DirectionalShadowPlazaFactory.CreateSceneDefinition(
                assets.GeneratedPlaneModel,
                assets.GeneratedCubeModel,
                assets.GeneratedSphereModel,
                assets.GeneratedStandardMaterial);
            GeneratedAuthoringSceneDefinition spotlightStreetSliceSceneDefinition = SpotlightStreetSliceFactory.CreateSceneDefinition(
                assets.GeneratedPlaneModel,
                assets.GeneratedCubeModel,
                assets.GeneratedStandardMaterial,
                assets.LamppostModel,
                assets.RacerModel,
                assets.RacerMaterials);

            AuthoringSceneWriteService.WriteScene(projectRootPath, directionalShadowPlazaSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, spotlightStreetSliceSceneDefinition);
        }
    }
}
