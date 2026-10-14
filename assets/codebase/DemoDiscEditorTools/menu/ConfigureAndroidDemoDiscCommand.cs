using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>Authors Android scene overlays and standard-material parameters across the selected DemoDisc package.</summary>
    public sealed class ConfigureAndroidDemoDiscCommand : IEditorCommand {
        /// <summary>Gets the CLI command that configures the full Android disc.</summary>
        public string CommandId => "menu.configure-android-demo-disc";
        /// <summary>Gets the editor label for the Android package preparation action.</summary>
        public string DisplayName => "Configure Android DemoDisc";
        /// <summary>Preserves common and other-platform content while selecting one instruction panel and density-scaled scene diagnostics for Android.</summary>
        /// <param name="context">Host-owned authoring session and loaded component descriptors.</param>
        public void Execute(IEditorCommandContext context) {
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create(context.ScriptTypeResolver);
            AndroidSceneViewportAuthoringService viewports = new AndroidSceneViewportAuthoringService();
            AndroidSceneReferenceAuthoringService references = new AndroidSceneReferenceAuthoringService();
            EditorProjectBuildConfigDocument config = new EditorBuildConfigService(context.ProjectRootPath).TryLoadProjectBuildConfig();
            EditorProjectPlatformBuildConfigDocument android = config?.Platforms.SingleOrDefault(platform => platform.PlatformId == "android")
                ?? throw new InvalidOperationException("The Android scene package must be configured before authoring its overrides.");
            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            foreach (SceneAssetReference reference in android.SelectedSceneReferences) {
                if (!reference.RelativePath.StartsWith("scenes/rendering/", StringComparison.OrdinalIgnoreCase)
                    && !reference.RelativePath.StartsWith("scenes/physics/", StringComparison.OrdinalIgnoreCase)
                    && !reference.RelativePath.StartsWith("scenes/benchmarks/", StringComparison.OrdinalIgnoreCase)
                    && !reference.RelativePath.StartsWith("scenes/games/tilt/tilt_trial_level_", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                } else if (string.Equals(reference.RelativePath, CubeTestSceneFactory.SceneId, StringComparison.OrdinalIgnoreCase)) {
                    // The Cube Test generator authors its Android presentation itself.
                    continue;
                }
                SceneAsset scene = context.Authoring.LoadNativeAsset<SceneAsset>(reference.RelativePath);
                if (!reference.RelativePath.StartsWith("scenes/games/", StringComparison.OrdinalIgnoreCase)) {
                    viewports.ConfigureScene(scene, registry, context.Authoring.CreateSceneAssetReferenceResolver());
                }
                references.ConfigureScene(scene, registry, context.Authoring);
                transaction.WriteAsset(reference.RelativePath, scene);
            }
            BlueprintAsset tiltBlueprint = context.Authoring.LoadNativeAsset<BlueprintAsset>(TiltTrialGameplayPresentationBlueprintGenerator.ConsoleBlueprintRelativePath);
            references.ConfigureTiltBlueprint(tiltBlueprint);
            transaction.WriteAsset(TiltTrialGameplayPresentationBlueprintGenerator.ConsoleBlueprintRelativePath, tiltBlueprint);
            transaction.Commit();
            new AndroidMaterialAuthoringService().Configure(context.Authoring);
        }
    }
}
