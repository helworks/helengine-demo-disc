using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>Authors Android density settings on the existing Cube Test scene without regenerating its content.</summary>
    public sealed class ConfigureCubeTestDensityCommand : IEditorCommand {
        /// <summary>Sets the Cube Test overlay's authored size multiplier; at 420 DPI this produces an approximately 650 by 348 pixel instruction panel.</summary>
        const float AndroidUiScale = 0.5625f;
        /// <summary>Gets the command used by the editor CLI to configure the existing scene.</summary>
        public string CommandId => "menu.configure-cube-test-density";
        /// <summary>Gets the menu label for the density configuration action.</summary>
        public string DisplayName => "Configure Cube Test Density";
        /// <summary>Publishes Android viewport, font and edge-anchor overrides through the normal authoring transaction.</summary>
        public void Execute(IEditorCommandContext context) {
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create(context.ScriptTypeResolver);
            SceneAsset scene = context.Authoring.LoadNativeAsset<SceneAsset>(CubeTestSceneFactory.SceneId);
            ISceneAssetReferenceResolver resolver = context.Authoring.CreateSceneAssetReferenceResolver();
            SelectBlueprintInstructionsForAndroid(scene);
            foreach (SceneEntityAsset root in scene.RootEntities) {
                if (root.Name != "DemoSceneInstructionViewport") {
                    ConfigureEntity(root, registry, resolver, false);
                }
            }
            BlueprintAsset blueprint = context.Authoring.LoadNativeAsset<BlueprintAsset>(ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath);
            ConfigureEntity(blueprint.RootEntity, registry, resolver, true);
            using EditorAuthoringTransaction transaction = context.Authoring.BeginTransaction();
            transaction.WriteAsset(CubeTestSceneFactory.SceneId, scene);
            transaction.WriteAsset(ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath, blueprint);
            transaction.Commit();
        }

        /// <summary>Selects the shared instruction Blueprint on Android while retaining the legacy panel and other platform configurations in the authored scene.</summary>
        /// <param name="scene">Existing authored scene whose common layout remains unchanged.</param>
        void SelectBlueprintInstructionsForAndroid(SceneAsset scene) {
            EditorOverrideScope scope = new EditorOverrideScope("android");
            SceneEntityAsset blueprint = scene.RootEntities.Single(entity => entity.Name == "ConsoleCameraLightInstructions");
            blueprint.PlatformExistenceOverrides = blueprint.PlatformExistenceOverrides.Where(item => !EditorOverrideScope.FromSteps(item.Scope).Equals(scope)).ToArray();
            SceneEntityAsset legacy = scene.RootEntities.Single(entity => entity.Name == "DemoSceneInstructionViewport");
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
        void ConfigureEntity(SceneEntityAsset entity, ComponentPersistenceRegistry registry, ISceneAssetReferenceResolver resolver, bool configureAnchors) {
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
                    viewport.PixelsPerCentimeter = (float)(160d / 2.54d);
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
                    AddAndroidComponent(entity, new ViewportComponent { ScalingMode = ViewportComponent.DensityScalingMode, UiScale = AndroidUiScale }, registry, "c0de0000000000000000000000000001");
                }
                android.Payload = descriptor.SerializeComponent(component, record.ComponentIndex, state).Payload;
                foreach (KeyValuePair<string, SceneAssetReference> reference in state.EnumerateNamedAssetReferences()) {
                    android.SetAssetReference(reference.Key, reference.Value);
                }
                state.SetPlatformOverride("android", android);
                entity.Components[index] = payloads.Wrap(payloads.UnwrapBaseRecord(record), state);
            }
            if (configureAnchors && entity.Name == "ConsoleCameraLightInstructionsPanel") {
                int2 size = ReadLayoutSize(entity, registry, resolver, payloads);
                bool label = entity.Name == "DemoDiscSceneLabelText";
                AddAndroidComponent(entity, new LayoutComponent {
                    LayoutSpace = LayoutComponent.CameraViewportLayoutSpace,
                    AnchorFlags = label ? (byte)(LayoutComponent.RightAnchorFlag | LayoutComponent.TopAnchorFlag) : (byte)(LayoutComponent.LeftAnchorFlag | LayoutComponent.BottomAnchorFlag),
                    AnchorDistances = label
                        ? new float4(0f, 1280f - entity.LocalPosition.X - size.X, entity.LocalPosition.Y, 0f)
                        : new float4(entity.LocalPosition.X, 0f, 0f, 720f - entity.LocalPosition.Y - size.Y)
                }, registry, "c0de0000000000000000000000000002");
            }
            foreach (SceneEntityAsset child in entity.Children) {
                ConfigureEntity(child, registry, resolver, configureAnchors);
            }
        }

        /// <summary>Reads the authored widget bounds needed to preserve its reference edge margins.</summary>
        int2 ReadLayoutSize(SceneEntityAsset entity, ComponentPersistenceRegistry registry, ISceneAssetReferenceResolver resolver, ComponentPlatformOverridePayloadService payloads) {
            foreach (SceneComponentAssetRecord record in entity.Components) {
                if (record.ComponentTypeId == "helengine.TextComponent" || record.ComponentTypeId == "helengine.RoundedRectComponent") {
                    Component component = registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(payloads.UnwrapBaseRecord(record), new EntitySaveComponent(), resolver);
                    if (component is TextComponent text) {
                        return text.Size;
                    } else if (component is RoundedRectComponent panel) {
                        return panel.Size;
                    }
                }
            }
            throw new InvalidOperationException("Anchored Cube Test widget has no authored bounds: " + entity.Name);
        }

        /// <summary>Adds or replaces an Android-only layout component while preserving every other platform's serialized content.</summary>
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