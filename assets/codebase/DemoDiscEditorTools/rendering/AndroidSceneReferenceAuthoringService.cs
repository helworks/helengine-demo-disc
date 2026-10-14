using helengine.editor;
using DemoDisc.rendering;
namespace DemoDisc.EditorTools {
    /// <summary>Authors explicit scene bindings for Android light indicators and the shared Tilt presentation Blueprints.</summary>
    public sealed class AndroidSceneReferenceAuthoringService {
        /// <summary>Rebinds references to their actual authored scene entities without changing transforms or gameplay content.</summary>
        public void ConfigureScene(SceneAsset scene, ComponentPersistenceRegistry registry, IEditorProjectAuthoringSession authoring) {
            List<SceneEntityAsset> entities = new List<SceneEntityAsset>();
            foreach (SceneEntityAsset root in scene.RootEntities) { Collect(root, entities); }
            if (entities.Any(entity => entity.Name == "TiltTrialConsolePresentation")) {
                foreach (SceneEntityAsset root in scene.RootEntities) {
                    if (root.Components.Any(record => record.ComponentTypeId == "DemoDisc.TiltPlay.TiltTrialSessionComponent, TiltPlay"
                        || record.ComponentTypeId == "helengine.DirectionalLightComponent" || record.ComponentTypeId == "helengine.AmbientLightComponent")) { ExcludeForAndroid(root); }
                }
            }
            ISceneAssetReferenceResolver resolver = authoring.CreateSceneAssetReferenceResolver();
            foreach (SceneEntityAsset owner in entities) {
                for (int index = 0; index < owner.Components.Length; index++) {
                    SceneComponentAssetRecord record = owner.Components[index];
                    if (record.ComponentTypeId == "DemoDisc.rendering.DemoDiscLightToggleComponent, DemoDisc"
                        || record.ComponentTypeId == "DemoDisc.rendering.DemoDiscDebugSceneLabelComponent, DemoDisc") {
                        bool lightIndicator = record.ComponentTypeId == "DemoDisc.rendering.DemoDiscLightToggleComponent, DemoDisc";
                        string targetName = lightIndicator ? DemoDiscLightIndicatorOverlayFactory.IndicatorSwatchEntityName : "DemoDiscSceneLabelText";
                        string propertyName = lightIndicator ? nameof(DemoDiscLightToggleComponent.IndicatorSwatchEntityReference) : "LabelEntityReference";
                        List<SceneEntityAsset> children = new List<SceneEntityAsset>();
                        Collect(owner, children);
                        SceneEntityAsset swatch = children.Single(entity => entity.Name == targetName);
                        ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
                        IComponentPersistenceDescriptor descriptor = registry.GetDescriptor(record.ComponentTypeId);
                        EntitySaveComponent save = new EntitySaveComponent();
                        Component component = descriptor.DeserializeComponent(payloads.UnwrapBaseRecord(record), save, resolver);
                        EntityComponentSaveState state = save.GetOrCreateComponentState(component);
                        EntityComponentPlatformOverrideState android = new EntityComponentPlatformOverrideState();
                        foreach (EntityComponentPlatformOverrideState previous in payloads.ReadOverrideStates(record)) {
                            state.SetScopedPlatformOverride(previous.Scope, previous);
                            if (previous.Scope.Equals(new EditorOverrideScope("android"))) { android = previous; }
                        }
                        component.GetType().GetProperty(propertyName).SetValue(component, new SceneEntityReference { EntityId = swatch.Id });
                        android.SetPropertyOverride(propertyName);
                        android.Payload = descriptor.SerializeComponent(component, record.ComponentIndex, state).Payload;
                        state.SetPlatformOverride("android", android);
                        owner.Components[index] = payloads.Wrap(payloads.UnwrapBaseRecord(record), state);
                    } else if (owner.Name == "TiltTrialConsolePresentation" || owner.Name == "TiltTrialHandheldPresentation") {
                        BindTiltPresentation(owner, index, record, entities, registry, authoring, resolver);
                    }
                }
            }
        }
        /// <summary>Keeps the scene-owned camera used by the stage controller while selecting one Blueprint HUD and one lighting rig.</summary>
        public void ConfigureTiltBlueprint(BlueprintAsset blueprint) {
            List<SceneEntityAsset> entities = new List<SceneEntityAsset>();
            Collect(blueprint.RootEntity, entities);
            foreach (SceneEntityAsset entity in entities) {
                if (entity.Components.Any(record => record.ComponentTypeId == "helengine.CameraComponent")) { ExcludeForAndroid(entity); }
            }
        }
        /// <summary>Disables a duplicate presentation entity only for Android, preserving every other scope.</summary>
        void ExcludeForAndroid(SceneEntityAsset entity) {
            EditorOverrideScope scope = new EditorOverrideScope("android");
            List<SceneEntityPlatformExistenceOverrideAsset> states = entity.PlatformExistenceOverrides.Where(state => !EditorOverrideScope.FromSteps(state.Scope).Equals(scope)).ToList();
            states.Add(new SceneEntityPlatformExistenceOverrideAsset { Scope = scope.ToSteps(), Exists = false });
            entity.PlatformExistenceOverrides = states.ToArray();
        }
        /// <summary>Rebuilds only external Blueprint bindings, keeping all local HUD references in the Blueprint.</summary>
        void BindTiltPresentation(SceneEntityAsset owner, int index, SceneComponentAssetRecord record, List<SceneEntityAsset> entities, ComponentPersistenceRegistry registry, IEditorProjectAuthoringSession authoring, ISceneAssetReferenceResolver resolver) {
            ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
            IComponentPersistenceDescriptor descriptor = registry.GetDescriptor(record.ComponentTypeId);
            EntitySaveComponent save = new EntitySaveComponent();
            BlueprintInstanceComponent component = (BlueprintInstanceComponent)descriptor.DeserializeComponent(payloads.UnwrapBaseRecord(record), save, resolver);
            BlueprintAsset blueprint = authoring.LoadNativeAsset<BlueprintAsset>(component.BlueprintAssetReference.RelativePath);
            uint player = entities.Single(entity => entity.Name == "PlayerSphere").Id;
            uint stage = entities.Single(entity => entity.Components.Any(componentRecord => componentRecord.ComponentTypeId == "DemoDisc.TiltPlay.DemoTiltStageComponent, TiltPlay")).Id;
            uint goal = FindGoal(entities, registry, resolver);
            BlueprintEntityReferenceOverrideAsset[] discovered = new BlueprintEntityReferenceOverrideService(registry).CreateAllEntityReferenceOverrides(blueprint, player);
            component.EntityReferenceOverrides = new TiltTrialGameplayPresentationRootFactory(null, authoring).CreateRuntimeReferenceOverrides(blueprint.RootEntity, registry, discovered, player, stage, goal);
            EntityComponentSaveState state = save.GetOrCreateComponentState(component);
            foreach (EntityComponentPlatformOverrideState previous in payloads.ReadOverrideStates(record)) { state.SetScopedPlatformOverride(previous.Scope, previous); }
            owner.Components[index] = payloads.Wrap(descriptor.SerializeComponent(component, record.ComponentIndex, state), state);
        }
        /// <summary>Finds the authored goal instance by its asset reference, independent of the level's entity label.</summary>
        uint FindGoal(List<SceneEntityAsset> entities, ComponentPersistenceRegistry registry, ISceneAssetReferenceResolver resolver) {
            ComponentPlatformOverridePayloadService payloads = new ComponentPlatformOverridePayloadService();
            foreach (SceneEntityAsset entity in entities) {
                foreach (SceneComponentAssetRecord record in entity.Components) {
                    if (record.ComponentTypeId == "helengine.editor.BlueprintInstanceComponent, helengine.editor") {
                        BlueprintInstanceComponent instance = (BlueprintInstanceComponent)registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(payloads.UnwrapBaseRecord(record), new EntitySaveComponent(), resolver);
                        if (instance.BlueprintAssetReference.RelativePath == SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath) { return entity.Id; }
                    }
                }
            }
            throw new InvalidOperationException("Tilt Trial Android presentation requires its authored goal Blueprint instance.");
        }
        /// <summary>Flattens a scene subtree for exact authored entity lookup.</summary>
        void Collect(SceneEntityAsset entity, List<SceneEntityAsset> entities) {
            entities.Add(entity);
            foreach (SceneEntityAsset child in entity.Children) { Collect(child, entities); }
        }
    }
}
