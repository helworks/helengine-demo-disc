namespace city.rendering.tools {
    /// <summary>
    /// Bundles the runtime models and materials required to compose the city rendering showcase scenes.
    /// </summary>
    public sealed class RenderingSceneGenerationAssets {
        /// <summary>
        /// Gets or sets the generated cube runtime model shared by most showcase scenes.
        /// </summary>
        public RuntimeModel GeneratedCubeModel { get; set; }

        /// <summary>
        /// Gets or sets the generated plane runtime model used by the plaza and street scenes.
        /// </summary>
        public RuntimeModel GeneratedPlaneModel { get; set; }

        /// <summary>
        /// Gets or sets the generated sphere runtime model used by the directional-shadow plaza scene.
        /// </summary>
        public RuntimeModel GeneratedSphereModel { get; set; }

        /// <summary>
        /// Gets or sets the generated standard runtime material shared by the showcase scenes.
        /// </summary>
        public RuntimeMaterial GeneratedStandardMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored marble runtime material assigned only to the Tilt Trial player sphere.
        /// </summary>
        public RuntimeMaterial TiltTrialPlayerSphereMarbleMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored runtime material assigned to the Tilt Trial course geometry and catch floor.
        /// </summary>
        public RuntimeMaterial TiltTrialCourseMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored six-colored-face runtime model used only by the Tilt Trial clipping probe render scene.
        /// </summary>
        public RuntimeModel TiltTrialClippingProbeModel { get; set; }

        /// <summary>
        /// Gets or sets the authored textured runtime material used only by the Tilt Trial clipping probe render scene.
        /// </summary>
        public RuntimeMaterial TiltTrialClippingProbeMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored golden-coin runtime model used by render-only Tilt Trial probes.
        /// </summary>
        public RuntimeModel GoldenCoinModel { get; set; }

        /// <summary>
        /// Gets or sets the authored golden-coin runtime material used by render-only Tilt Trial probes.
        /// </summary>
        public RuntimeMaterial GoldenCoinMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored goal-flag runtime model used by render-only Tilt Trial probes.
        /// </summary>
        public RuntimeModel GoalFlagModel { get; set; }

        /// <summary>
        /// Gets or sets the authored goal-flag pole material used by render-only Tilt Trial probes.
        /// </summary>
        public RuntimeMaterial GoalFlagPoleMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored goal-flag banner material used by render-only Tilt Trial probes.
        /// </summary>
        public RuntimeMaterial GoalFlagBannerMaterial { get; set; }

        /// <summary>
        /// Gets or sets the generated shared solid-color runtime material used by the cube-test scene.
        /// </summary>
        public RuntimeMaterial GeneratedCubeTestSolidMaterial { get; set; }

        /// <summary>
        /// Gets or sets the generated shared blue solid-color runtime material used by the depth-clip-probe scene.
        /// </summary>
        public RuntimeMaterial DepthClipProbeMaterial { get; set; }

        /// <summary>
        /// Gets or sets the generated shared green solid-color runtime material used by the depth-clip-probe scene's center box.
        /// </summary>
        public RuntimeMaterial DepthClipProbeCenterMaterial { get; set; }

        /// <summary>
        /// Gets or sets the generated directional-light arrow runtime model used by the axis showcase scenes.
        /// </summary>
        public RuntimeModel GeneratedArrowModel { get; set; }

        /// <summary>
        /// Gets or sets the runtime materials assigned to the axis showcase scenes in X, Y, Z, ground, and marker order.
        /// </summary>
        public RuntimeMaterial[] AxisMaterials { get; set; }

        /// <summary>
        /// Gets or sets the runtime materials assigned to the racer imported model in submesh order.
        /// </summary>
        public RuntimeMaterial[] RacerMaterials { get; set; }

        /// <summary>
        /// Gets or sets the runtime lamppost model used by the spotlight street-slice scene.
        /// </summary>
        public RuntimeModel LamppostModel { get; set; }

        /// <summary>
        /// Gets or sets the runtime racer model used by the spotlight street-slice scene.
        /// </summary>
        public RuntimeModel RacerModel { get; set; }

        /// <summary>
        /// Gets or sets the authored scuffed-metal runtime material used by the PBR textured showcase scene.
        /// </summary>
        public RuntimeMaterial PbrTexturedShowcaseMetalMaterial { get; set; }

        /// <summary>
        /// Gets or sets the authored wood-plank runtime material used by the PBR textured showcase scene.
        /// </summary>
        public RuntimeMaterial PbrTexturedShowcaseWoodMaterial { get; set; }
    }
}
