using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>Authors the shared viewport density and instruction selection used by Android scene overlays.</summary>
    public sealed class AndroidSceneViewportAuthoringService {
        /// <summary>Approved Android overlay multiplier, shared with the generators that author it on live entities.</summary>
        const float AndroidUiScale = AndroidDensityPresentationAuthoringService.AndroidUiScale;
        /// <summary>Applies scene-local Android diagnostics and selects the shared instruction Blueprint when present.</summary>
        /// <param name="scene">Existing authored scene whose common content is retained.</param>
        /// <param name="registry">Persistence descriptors for the current project components.</param>
        /// <param name="resolver">Session-owned reference resolver used to preserve font dependencies.</param>
        public void ConfigureScene(SceneAsset scene, ComponentPersistenceRegistry registry, ISceneAssetReferenceResolver resolver) {
            SelectBlueprintInstructionsForAndroid(scene);
            bool hasBlueprint = scene.RootEntities.Any(entity => entity.Name == "ConsoleCameraLightInstructions");
            foreach (SceneEntityAsset root in scene.RootEntities) {
                if (root.Name != "DemoSceneInstructionViewport" || !hasBlueprint) {
                    ConfigureEntity(root, registry, resolver);
                }
            }
        }

        /// <summary>Selects the shared instruction Blueprint on Android while retaining the legacy panel and other platform configurations in the authored scene.</summary>
        /// <param name="scene">Existing authored scene whose common layout remains unchanged.</param>
        void SelectBlueprintInstructionsForAndroid(SceneAsset scene) {
            EditorOverrideScope scope = new EditorOverrideScope("android");
            SceneEntityAsset blueprint = scene.RootEntities.FirstOrDefault(entity => entity.Name == "ConsoleCameraLightInstructions");
            if (blueprint == null) { return; }
            blueprint.PlatformExistenceOverrides = blueprint.PlatformExistenceOverrides.Where(item => !EditorOverrideScope.FromSteps(item.Scope).Equals(scope)).ToArray();
            SceneEntityAsset legacy = scene.RootEntities.FirstOrDefault(entity => entity.Name == "DemoSceneInstructionViewport");
            if (legacy == null) { return; }
            List<SceneEntityPlatformExistenceOverrideAsset> overrides = legacy.PlatformExistenceOverrides.ToList();
            SceneEntityPlatformExistenceOverrideAsset target = overrides.FirstOrDefault(item => EditorOverrideScope.FromSteps(item.Scope).Equals(scope));
            if (target == null) {
                target = new SceneEntityPlatformExistenceOverrideAsset { Scope = scope.ToSteps() };
                overrides.Add(target);
            }
            target.Exists = false;
            legacy.PlatformExistenceOverrides = overrides.ToArray();
        }
        /// <summary>Applies density to each authored viewport and enables scalable glyphs only for Android.</summary>
        void ConfigureEntity(SceneEntityAsset entity, ComponentPersistenceRegistry registry, ISceneAssetReferenceResolver resolver) {
            ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
            for (int index = 0; index < entity.Components.Length; index++) {
                SceneComponentAssetRecord record = entity.Components[index];
                if (record.ComponentTypeId != "helengine.ViewportComponent" && record.ComponentTypeId != "helengine.TextComponent" && record.ComponentTypeId != "helengine.FPSComponent") {
                    continue;
                }
                IComponentPersistenceDescriptor descriptor = registry.GetDescriptor(record.ComponentTypeId);
                EntitySaveComponent save = new EntitySaveComponent();
                Component component = descriptor.DeserializeComponent(payloads.UnwrapBaseRecord(record), save, resolver);
                EntityComponentSaveState state = save.GetOrCreateComponentState(component);
                foreach (EntityComponentPlatformOverrideState previous in payloads.ReadOverrideStates(record)) {
                    state.SetScopedPlatformOverride(previous.Scope, previous);
                }
                EntityComponentPlatformOverrideState android = new EntityComponentPlatformOverrideState();
                if (component is ViewportComponent viewport) {
                    viewport.ScalingMode = ViewportComponent.DensityScalingMode;
                    viewport.PixelsPerCentimeter = AndroidDensityPresentationAuthoringService.AndroidPixelsPerCentimeter;
                    viewport.UiScale = AndroidUiScale;
                    android.SetPropertyOverride("ScalingMode");
                    android.SetPropertyOverride("PixelsPerCentimeter");
                    android.SetPropertyOverride("UiScale");
                } else if (component is TextComponent text) {
                    text.NativeFontPixels = TextNativePixelMode.Disabled;
                    android.SetPropertyOverride("NativeFontPixels");
                } else if (component is FPSComponent fps) {
                    fps.NativeFontPixels = TextNativePixelMode.Disabled;
                    android.SetPropertyOverride("NativeFontPixels");
                    AddAndroidComponent(entity, new ViewportComponent { ScalingMode = ViewportComponent.DensityScalingMode, UiScale = AndroidUiScale }, registry, AndroidDensityPresentationAuthoringService.FpsDensityViewportComponentKey);
                }
                android.Payload = descriptor.SerializeComponent(component, record.ComponentIndex, state).Payload;
                foreach (KeyValuePair<string, SceneAssetReference> reference in state.EnumerateNamedAssetReferences()) {
                    android.SetAssetReference(reference.Key, reference.Value);
                }
                state.SetPlatformOverride("android", android);
                entity.Components[index] = payloads.Wrap(payloads.UnwrapBaseRecord(record), state);
            }
            foreach (SceneEntityAsset child in entity.Children) {
                ConfigureEntity(child, registry, resolver);
            }
        }

        /// <summary>Adds or replaces an Android-only component while preserving every other platform's serialized content.</summary>
        void AddAndroidComponent(SceneEntityAsset entity, Component component, ComponentPersistenceRegistry registry, string key) {
            EditorOverrideScope scope = new EditorOverrideScope("android");
            List<SceneEntityPlatformComponentOverrideAsset> overrides = entity.PlatformComponentOverrides.ToList();
            SceneEntityPlatformComponentOverrideAsset target = overrides.FirstOrDefault(item => EditorOverrideScope.FromSteps(item.Scope).Equals(scope));
            if (target == null) {
                target = new SceneEntityPlatformComponentOverrideAsset { Scope = scope.ToSteps() };
                overrides.Add(target);
            }
            List<SceneEntityPlatformAddedComponentAsset> added = target.AddedComponents.Where(item => item.Component.ComponentKey != key).ToList();
            SceneComponentAssetRecord record = registry.GetDescriptor(component).SerializeComponent(component, entity.Components.Length + added.Count, new EntityComponentSaveState());
            record.ComponentKey = key;
            added.Add(new SceneEntityPlatformAddedComponentAsset { Component = record });
            target.AddedComponents = added.ToArray();
            entity.PlatformComponentOverrides = overrides.ToArray();
        }
    }
}
