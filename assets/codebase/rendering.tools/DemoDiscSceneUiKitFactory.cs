namespace city.rendering.tools {
    /// <summary>
    /// Builds the standard demo-disc scene UI root so every rendering and physics showcase scene shares one 2D overlay kit.
    /// </summary>
    public sealed class DemoDiscSceneUiKitFactory {
        readonly IEditorProjectAssetAuthoringService AssetAuthoringService;

        /// <summary>
        /// Initializes the shared UI kit with the host-owned public asset authoring capability.
        /// </summary>
        public DemoDiscSceneUiKitFactory(IEditorProjectAssetAuthoringService assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }
        /// <summary>
        /// Creates one authored UI root carrying the shared demo-disc overlay kit: FPS diagnostics, return-to-menu handling, the light toggle with its indicator swatch, and the debug-gated scene label.
        /// </summary>
        /// <param name="entityName">Stable name for the generated UI root entity.</param>
        /// <param name="sceneLabel">Numbered scene label shown by debug-environment builds; empty for probe scenes that never appear in a menu.</param>
        /// <returns>Live authored UI root entity.</returns>
        public Entity CreateStandardSceneUi(string entityName, string sceneLabel) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("UI root entity name must be provided.", nameof(entityName));
            }

            FontAsset font = ResolveRequiredEditorFont();
            Entity entity = Core.Instance.EntityFactory.Create(entityName);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new FPSComponent {
                Font = font,
                FontScale = 2f
            });
            PspFpsComponentOverrideService.Apply(entity);
            entity.AddComponent(new city.menu.DemoDiscReturnToMenuComponent());
            entity.AddComponent(new city.rendering.DemoDiscLightToggleComponent());
            DemoDiscLightIndicatorOverlayFactory lightIndicatorOverlayFactory = new DemoDiscLightIndicatorOverlayFactory(AssetAuthoringService);
            lightIndicatorOverlayFactory.AttachToSceneUi(entity, font);
            if (!string.IsNullOrWhiteSpace(sceneLabel)) {
                DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory(AssetAuthoringService);
                sceneLabelOverlayFactory.AttachToSceneUi(entity, font, sceneLabel);
            }
            return entity;
        }

        /// <summary>
        /// Resolves the shared editor default font required by the overlay kit's text components.
        /// </summary>
        /// <returns>Editor default font asset.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before demo-disc scene UI can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }
    }
}
