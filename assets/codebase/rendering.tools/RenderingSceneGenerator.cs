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
        /// Stable scene id used by the scaled-cube showcase.
        /// </summary>
        public const string ScaledCubeSceneId = "scenes/rendering/scaled_cube.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the scaled-cube showcase.
        /// </summary>
        public const string ScaledCubeNintendoDsSceneId = "scenes/rendering/ds/scaled_cube_ds.helen";

        /// <summary>
        /// Stable scene id used by the directional-shadow plaza showcase.
        /// </summary>
        public const string DirectionalShadowPlazaSceneId = "scenes/rendering/directional_shadow_plaza.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the directional-shadow plaza showcase.
        /// </summary>
        public const string DirectionalShadowPlazaNintendoDsSceneId = "scenes/rendering/ds/directional_shadow_plaza_ds.helen";

        /// <summary>
        /// Stable scene id used by the ground-cube probe showcase.
        /// </summary>
        public const string GroundCubeProbeSceneId = "scenes/rendering/ground_cube_probe.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the ground-cube probe showcase.
        /// </summary>
        public const string GroundCubeProbeNintendoDsSceneId = "scenes/rendering/ds/ground_cube_probe_ds.helen";

        /// <summary>
        /// Stable scene id used by the textured cube-grid showcase.
        /// </summary>
        public const string TexturedCubeGridSceneId = "scenes/rendering/textured_cube_grid.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the textured cube-grid showcase.
        /// </summary>
        public const string TexturedCubeGridNintendoDsSceneId = "scenes/rendering/ds/textured_cube_grid_ds.helen";

        /// <summary>
        /// Stable scene id used by the spotlight street-slice showcase.
        /// </summary>
        public const string SpotlightStreetSliceSceneId = "scenes/rendering/spotlight_street_slice.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the spotlight street-slice showcase.
        /// </summary>
        public const string SpotlightStreetSliceNintendoDsSceneId = "scenes/rendering/ds/spotlight_street_slice_ds.helen";

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
        /// Stable scene id used by the two-cube depth-ordering and near-plane-clipping probe showcase.
        /// </summary>
        public const string DepthClipProbeSceneId = "scenes/rendering/depth_clip_probe.helen";

        /// <summary>
        /// Stable scene id used by the persistent scene-memory probe showcase.
        /// </summary>
        public const string SceneMemoryProbeSceneId = "scenes/rendering/scene_memory_probe.helen";

        /// <summary>
        /// Stable Nintendo DS companion-scene id used by the persistent scene-memory probe showcase.
        /// </summary>
        public const string SceneMemoryProbeNintendoDsSceneId = "scenes/rendering/ds/scene_memory_probe_ds.helen";

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
        /// Factory used to author the minimal ground-cube probe scene.
        /// </summary>
        readonly GroundCubeProbeSceneFactory GroundCubeProbeFactory;

        /// <summary>
        /// Factory used to author the scaled-cube scene.
        /// </summary>
        readonly ScaledCubeSceneFactory ScaledCubeFactory;

        /// <summary>
        /// Factory used to author the depth-clip-probe scene.
        /// </summary>
        readonly DepthClipProbeSceneFactory DepthClipProbeFactory;

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
        /// Factory used to author the persistent scene-memory probe scene.
        /// </summary>
        readonly SceneMemoryProbeSceneFactory SceneMemoryProbeFactory;

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
        /// Initializes one city rendering scene generator.
        /// </summary>
        /// <param name="scriptTypeResolver">Resolver used to restore project-authored components during temporary handheld clone loads.</param>
        public RenderingSceneGenerator(IScriptTypeResolver scriptTypeResolver = null) {
            AuthoringSceneWriteService = new GeneratedAuthoringSceneWriteService(scriptTypeResolver);
            DirectionalShadowPlazaFactory = new DirectionalShadowPlazaSceneFactory();
            SpotlightStreetSliceFactory = new SpotlightStreetSliceSceneFactory();
            CubeTestFactory = new CubeTestSceneFactory();
            GroundCubeProbeFactory = new GroundCubeProbeSceneFactory();
            ScaledCubeFactory = new ScaledCubeSceneFactory();
            DepthClipProbeFactory = new DepthClipProbeSceneFactory();
            ColoredCubeGridFactory = new ColoredCubeGridSceneFactory();
            TexturedCubeGridFactory = new TexturedCubeGridSceneFactory();
            AxisTestFactory = new AxisTestSceneFactory();
            AxisTest2Factory = new AxisTest2SceneFactory();
            SceneMemoryProbeFactory = new SceneMemoryProbeSceneFactory();
            PbrMaterialGalleryMaterials = new PbrMaterialGalleryMaterialFactory();
            PbrMaterialGalleryScene = new PbrMaterialGallerySceneFactory();
            PbrTexturedShowcaseMaterials = new PbrTexturedShowcaseMaterialFactory();
            PbrTexturedShowcaseScene = new PbrTexturedShowcaseSceneFactory();
            PbrShadowTheaterScene = new PbrShadowTheaterSceneFactory();
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
            } else if (assets.DepthClipProbeMaterial == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.DepthClipProbeCenterMaterial == null) {
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

            if (Core.Instance is not EditorCore editorCore) {
                throw new InvalidOperationException("Rendering scene generation requires an editor core for the console instruction Blueprint.");
            } else if (editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("Rendering scene generation requires the editor default font for the console instruction Blueprint.");
            }

            ConsoleCameraLightInstructionsBlueprintGenerator consoleInstructionBlueprintGenerator = new ConsoleCameraLightInstructionsBlueprintGenerator();
            consoleInstructionBlueprintGenerator.Generate(
                projectRootPath,
                new DemoSceneInstructionOverlayFactory(),
                editorCore.DefaultFontAssetForEditor);

            DeleteObsoleteRenderMatrixProbeScene(projectRootPath);
            DeleteObsoleteNintendoHandheldCompanionScenes(projectRootPath);
            GeneratedAuthoringSceneDefinition cubeTestSceneDefinition = CubeTestFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedCubeTestSolidMaterial);
            GeneratedAuthoringSceneDefinition groundCubeProbeSceneDefinition = GroundCubeProbeFactory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedStandardMaterial);
            GeneratedAuthoringSceneDefinition scaledCubeSceneDefinition = ScaledCubeFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedStandardMaterial);
            GeneratedAuthoringSceneDefinition depthClipProbeSceneDefinition = DepthClipProbeFactory.CreateSceneDefinition(assets.GeneratedCubeModel, assets.DepthClipProbeMaterial, assets.DepthClipProbeCenterMaterial);
            GeneratedAuthoringSceneDefinition coloredCubeGridSceneDefinition;
            GeneratedAuthoringSceneDefinition texturedCubeGridSceneDefinition;
            GeneratedAuthoringSceneDefinition axisTestSceneDefinition = AxisTestFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedArrowModel, assets.AxisMaterials);
            GeneratedAuthoringSceneDefinition axisTest2SceneDefinition = AxisTest2Factory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, assets.GeneratedArrowModel, assets.AxisMaterials);
            GeneratedAuthoringSceneDefinition sceneMemoryProbeSceneDefinition = SceneMemoryProbeFactory.CreateSceneDefinition();
            GeneratedAuthoringSceneDefinition directionalShadowPlazaSceneDefinition = DirectionalShadowPlazaFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedPlaneModel, assets.GeneratedCubeModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial);
            GeneratedAuthoringSceneDefinition spotlightStreetSliceSceneDefinition = SpotlightStreetSliceFactory.CreateSceneDefinition(assets.GeneratedPlaneModel, assets.GeneratedCubeModel, assets.GeneratedStandardMaterial, assets.LamppostModel, assets.RacerModel, assets.RacerMaterials);
            PbrMaterialGalleryMaterials.WriteMaterialAssets(projectRootPath);
            RuntimeMaterial[] pbrGalleryMaterials = PbrMaterialGalleryMaterials.CreateRuntimeMaterials();
            GeneratedAuthoringSceneDefinition pbrMaterialGallerySceneDefinition = PbrMaterialGalleryScene.CreateSceneDefinition(assets.GeneratedPlaneModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial, pbrGalleryMaterials);
            GeneratedAuthoringSceneDefinition pbrTexturedShowcaseSceneDefinition = PbrTexturedShowcaseScene.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedPlaneModel, assets.GeneratedStandardMaterial, assets.PbrTexturedShowcaseMetalMaterial, assets.PbrTexturedShowcaseWoodMaterial);
            GeneratedAuthoringSceneDefinition pbrShadowTheaterSceneDefinition = PbrShadowTheaterScene.CreateSceneDefinition(assets.GeneratedCubeModel, assets.GeneratedSphereModel, assets.GeneratedStandardMaterial, pbrGalleryMaterials);
            ColoredCubeGridFactory.WriteMaterialAssets(projectRootPath);
            TexturedCubeGridFactory.WriteAssets(projectRootPath);
            coloredCubeGridSceneDefinition = ColoredCubeGridFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, ColoredCubeGridFactory.CreateRuntimeMaterials());
            texturedCubeGridSceneDefinition = TexturedCubeGridFactory.CreateSceneDefinition(projectRootPath, assets.GeneratedCubeModel, TexturedCubeGridFactory.CreateRuntimeMaterials(assets.GeneratedStandardMaterial));
            AuthoringSceneWriteService.WriteScene(projectRootPath, cubeTestSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, groundCubeProbeSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, scaledCubeSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, depthClipProbeSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, coloredCubeGridSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, texturedCubeGridSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, axisTestSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, axisTest2SceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, sceneMemoryProbeSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, directionalShadowPlazaSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, spotlightStreetSliceSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, pbrMaterialGallerySceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, pbrTexturedShowcaseSceneDefinition);
            AuthoringSceneWriteService.WriteScene(projectRootPath, pbrShadowTheaterSceneDefinition);
        }

        /// <summary>
        /// Deletes the obsolete generated Matrix Probe scene so stale authored output cannot be packaged after the feature removal.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        static void DeleteObsoleteRenderMatrixProbeScene(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string obsoleteScenePath = Path.Combine(Path.GetFullPath(projectRootPath), "assets", "scenes", "rendering", "test_scene_render_matrix_probe.helen");
            if (File.Exists(obsoleteScenePath)) {
                File.Delete(obsoleteScenePath);
            }
        }

        /// <summary>
        /// Deletes the obsolete Nintendo handheld companion rendering scenes so stale generated output does not remain discoverable in the project scene catalog.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        static void DeleteObsoleteNintendoHandheldCompanionScenes(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string[] obsoleteRelativePaths = [
                CubeTestNintendoDsSceneId,
                ColoredCubeGridNintendoDsSceneId,
                ScaledCubeNintendoDsSceneId,
                DirectionalShadowPlazaNintendoDsSceneId,
                GroundCubeProbeNintendoDsSceneId,
                TexturedCubeGridNintendoDsSceneId,
                SpotlightStreetSliceNintendoDsSceneId,
                AxisTestNintendoDsSceneId,
                AxisTest2NintendoDsSceneId,
                SceneMemoryProbeNintendoDsSceneId
            ];

            for (int index = 0; index < obsoleteRelativePaths.Length; index++) {
                string obsoleteScenePath = Path.Combine(fullProjectRootPath, "assets", obsoleteRelativePaths[index].Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(obsoleteScenePath)) {
                    File.Delete(obsoleteScenePath);
                }
            }
        }
    }
}
