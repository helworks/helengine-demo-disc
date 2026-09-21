using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Generates the complete city rendering showcase scene set inside the active project.
    /// </summary>
    public sealed class RenderingSceneGenerator {
        /// <summary>
        /// Host-owned capability used by all generated scene factories to resolve current imported assets and settings.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;
        /// <summary>
        /// Stable scene id used by the cube-test showcase.
        /// </summary>
        public const string CubeTestSceneId = "scenes/rendering/cube_test.helen";

        /// <summary>
        /// Stable scene id used by the software path tracer showcase.
        /// </summary>
        public const string SoftwarePathTracerSceneId = "scenes/rendering/software_path_tracer.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the cube-test showcase.
        /// </summary>
        public const string CubeTestNintendoDsSceneId = "scenes/rendering/ds/cube_test_ds.helen";

        /// <summary>
        /// Stable scene id used by the colored cube-grid showcase.
        /// </summary>
        public const string ColoredCubeGridSceneId = "scenes/rendering/colored_cube_grid.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the colored cube-grid showcase.
        /// </summary>
        public const string ColoredCubeGridNintendoDsSceneId = "scenes/rendering/ds/colored_cube_grid_ds.helen";

        /// <summary>
        /// Stable scene id used by the directional-shadow plaza showcase.
        /// </summary>
        public const string DirectionalShadowPlazaSceneId = "scenes/rendering/directional_shadow_plaza.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the directional-shadow plaza showcase.
        /// </summary>
        public const string DirectionalShadowPlazaNintendoDsSceneId = "scenes/rendering/ds/directional_shadow_plaza_ds.helen";

        /// <summary>
        /// Stable scene id used by the textured cube-grid showcase.
        /// </summary>
        public const string TexturedCubeGridSceneId = "scenes/rendering/textured_cube_grid.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the textured cube-grid showcase.
        /// </summary>
        public const string TexturedCubeGridNintendoDsSceneId = "scenes/rendering/ds/textured_cube_grid_ds.helen";

        /// <summary>
        /// Stable scene id used by the axis-test showcase.
        /// </summary>
        public const string AxisTestSceneId = "scenes/rendering/axis_test.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the axis-test showcase.
        /// </summary>
        public const string AxisTestNintendoDsSceneId = "scenes/rendering/ds/axis_test_ds.helen";

        /// <summary>
        /// Stable scene id used by the axis-test-2 showcase.
        /// </summary>
        public const string AxisTest2SceneId = "scenes/rendering/axis_test2.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the axis-test-2 showcase.
        /// </summary>
        public const string AxisTest2NintendoDsSceneId = "scenes/rendering/ds/axis_test2_ds.helen";

        /// <summary>
        /// Stable scene id used by the PBR material gallery showcase.
        /// </summary>
        public const string PbrMaterialGallerySceneId = "scenes/rendering/pbr_material_gallery.helen";

        /// <summary>
        /// Stable scene id used by the PBR textured showcase.
        /// </summary>
        public const string PbrTexturedShowcaseSceneId = "scenes/rendering/pbr_textured_showcase.helen";

        /// <summary>
        /// Stable scene id used by the PBR shadow theater showcase.
        /// </summary>
        public const string PbrShadowTheaterSceneId = "scenes/rendering/pbr_shadow_theater.helen";

        /// <summary>
        /// Stable scene id used by the Matrix Render transform-inspection scene.
        /// </summary>
        public const string MatrixRenderSceneId = "scenes/rendering/test_scene_matrix_render.helen";

        /// <summary>
        /// Writer used to persist generated live-authored scenes through the editor save pipeline.
        /// </summary>
        readonly GeneratedAuthoringSceneWriteService AuthoringSceneWriteService;

        /// <summary>
        /// Factory used to author the directional-shadow plaza scene.
        /// </summary>
        readonly DirectionalShadowPlazaSceneFactory DirectionalShadowPlazaFactory;

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
        /// Factory used to author the PBR material gallery materials.
        /// </summary>
        readonly PbrMaterialGalleryMaterialFactory PbrMaterialGalleryMaterials;

        /// <summary>
        /// Factory used to author the PBR material gallery scene.
        /// </summary>
        readonly PbrMaterialGallerySceneFactory PbrMaterialGalleryScene;

        /// <summary>
        /// Factory used to author the PBR textured showcase materials.
        /// </summary>
        readonly PbrTexturedShowcaseMaterialFactory PbrTexturedShowcaseMaterials;

        /// <summary>
        /// Factory used to author the PBR textured showcase scene.
        /// </summary>
        readonly PbrTexturedShowcaseSceneFactory PbrTexturedShowcaseScene;

        /// <summary>
        /// Factory used to author the PBR shadow theater scene.
        /// </summary>
        readonly PbrShadowTheaterSceneFactory PbrShadowTheaterScene;

        /// <summary>
        /// Factory used to author the Matrix Render scene and its hero material.
        /// </summary>
        readonly MatrixRenderSceneFactory MatrixRenderFactory;

        /// <summary>
        /// Factory used to author the shared software path tracer scene.
        /// </summary>
        readonly SoftwarePathTracerSceneFactory SoftwarePathTracerFactory;

        /// <summary>
        /// Initializes one city rendering scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to restore project-authored components during temporary handheld clone loads.</param>
        /// <param name="assetAuthoringService">Host-owned capability used by all generated scene factories.</param>
        public RenderingSceneGenerator(
            IScriptTypeResolver scriptTypeResolver,
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            AuthoringSceneWriteService = new GeneratedAuthoringSceneWriteService(scriptTypeResolver, AssetAuthoringService, Transaction);
            DirectionalShadowPlazaFactory = new DirectionalShadowPlazaSceneFactory(AssetAuthoringService, Transaction);
            CubeTestFactory = new CubeTestSceneFactory(AssetAuthoringService, Transaction);
            ColoredCubeGridFactory = new ColoredCubeGridSceneFactory(AssetAuthoringService, Transaction);
            TexturedCubeGridFactory = new TexturedCubeGridSceneFactory(AssetAuthoringService, Transaction);
            AxisTestFactory = new AxisTestSceneFactory(AssetAuthoringService, Transaction);
            AxisTest2Factory = new AxisTest2SceneFactory(AssetAuthoringService, Transaction);
            PbrMaterialGalleryMaterials = new PbrMaterialGalleryMaterialFactory(AssetAuthoringService, Transaction);
            PbrMaterialGalleryScene = new PbrMaterialGallerySceneFactory(AssetAuthoringService, Transaction);
            PbrTexturedShowcaseMaterials = new PbrTexturedShowcaseMaterialFactory(AssetAuthoringService, Transaction);
            PbrTexturedShowcaseScene = new PbrTexturedShowcaseSceneFactory(AssetAuthoringService, Transaction);
            PbrShadowTheaterScene = new PbrShadowTheaterSceneFactory(AssetAuthoringService, Transaction);
            MatrixRenderFactory = new MatrixRenderSceneFactory(AssetAuthoringService, Transaction);
            SoftwarePathTracerFactory = new SoftwarePathTracerSceneFactory(AssetAuthoringService);
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
            } else if (assets.GeneratedCubeTestSolidMaterial == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedArrowModel == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.AxisMaterials == null) {
                throw new ArgumentNullException(nameof(assets));
            }

            if (AssetAuthoringService.OwningCore is not EditorCore editorCore) {
                throw new InvalidOperationException("Rendering scene generation requires an editor core for the console instruction Blueprint.");
            } else if (editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("Rendering scene generation requires the editor default font for the console instruction Blueprint.");
            }

            ConsoleCameraLightInstructionsBlueprintGenerator consoleInstructionBlueprintGenerator = new ConsoleCameraLightInstructionsBlueprintGenerator(AssetAuthoringService, Transaction);
            consoleInstructionBlueprintGenerator.Generate(
                projectRootPath,
                new DemoSceneInstructionOverlayFactory(AssetAuthoringService, Transaction),
                editorCore.DefaultFontAssetForEditor);

            GeneratedAuthoringSceneDefinition cubeTestSceneDefinition = CubeTestFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedCubeTestSolidMaterial);
            GeneratedAuthoringSceneDefinition coloredCubeGridSceneDefinition;
            GeneratedAuthoringSceneDefinition texturedCubeGridSceneDefinition;
            GeneratedAuthoringSceneDefinition axisTestSceneDefinition = AxisTestFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedArrowModel, assets.AxisMaterials);
            GeneratedAuthoringSceneDefinition axisTest2SceneDefinition = AxisTest2Factory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedArrowModel, assets.AxisMaterials);
            GeneratedAuthoringSceneDefinition directionalShadowPlazaSceneDefinition = DirectionalShadowPlazaFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedPlaneModel, assets.GeneratedCubeModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial);
            PbrMaterialGalleryMaterials.WriteMaterialAssets(projectRootPath);
            RuntimeMaterial[] pbrGalleryMaterials = PbrMaterialGalleryMaterials.CreateRuntimeMaterials();
            GeneratedAuthoringSceneDefinition pbrMaterialGallerySceneDefinition = PbrMaterialGalleryScene.CreateSceneDefinition(projectRootPath, assets.GeneratedPlaneModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial, pbrGalleryMaterials);
            GeneratedAuthoringSceneDefinition pbrTexturedShowcaseSceneDefinition = PbrTexturedShowcaseScene.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedPlaneModel, assets.GeneratedStandardMaterial, assets.PbrTexturedShowcaseMetalMaterial, assets.PbrTexturedShowcaseWoodMaterial);
            GeneratedAuthoringSceneDefinition pbrShadowTheaterSceneDefinition = PbrShadowTheaterScene.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial, pbrGalleryMaterials);
            MatrixRenderFactory.WriteMaterialAssets(projectRootPath);
            GeneratedAuthoringSceneDefinition matrixRenderSceneDefinition = MatrixRenderFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel);
            GeneratedAuthoringSceneDefinition softwarePathTracerSceneDefinition = SoftwarePathTracerFactory.CreateSceneDefinition(
                projectRootPath,
                EngineSceneAssetReferenceFactory.CreateCubeModel(),
                editorCore.DefaultFontAssetForEditor);
            ColoredCubeGridFactory.WriteMaterialAssets(projectRootPath);
            TexturedCubeGridFactory.WriteAssets(projectRootPath);
            coloredCubeGridSceneDefinition = ColoredCubeGridFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, ColoredCubeGridFactory.CreateRuntimeMaterials());
            texturedCubeGridSceneDefinition = TexturedCubeGridFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, TexturedCubeGridFactory.CreateRuntimeMaterials(assets.GeneratedStandardMaterial));
            AuthoringSceneWriteService.WriteScene(cubeTestSceneDefinition);
            AuthoringSceneWriteService.WriteScene(coloredCubeGridSceneDefinition);
            AuthoringSceneWriteService.WriteScene(texturedCubeGridSceneDefinition);
            AuthoringSceneWriteService.WriteScene(axisTestSceneDefinition);
            AuthoringSceneWriteService.WriteScene(axisTest2SceneDefinition);
            AuthoringSceneWriteService.WriteScene(directionalShadowPlazaSceneDefinition);
            AuthoringSceneWriteService.WriteScene(pbrMaterialGallerySceneDefinition);
            AuthoringSceneWriteService.WriteScene(pbrTexturedShowcaseSceneDefinition);
            AuthoringSceneWriteService.WriteScene(pbrShadowTheaterSceneDefinition);
            AuthoringSceneWriteService.WriteScene(matrixRenderSceneDefinition);
            AuthoringSceneWriteService.WriteScene(softwarePathTracerSceneDefinition);
        }

    }
}
