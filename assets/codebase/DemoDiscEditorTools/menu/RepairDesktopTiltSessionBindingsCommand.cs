using helengine;
using helengine.editor;
using DemoDisc.TiltPlay;

namespace DemoDisc.EditorTools {
    /// <summary>Repairs omitted external gameplay references in the five existing desktop Tilt levels.</summary>
    public sealed class RepairDesktopTiltSessionBindingsCommand : IEditorCommand {
        public string CommandId => "game.repair-desktop-tilt-session-bindings";
        public string DisplayName => "Repair Desktop Tilt Session Bindings";
        public void Execute(IEditorCommandContext context) {
            var registry=GeneratedScenePersistenceRegistryFactory.Create(context.ScriptTypeResolver);
            var overrides=new ComponentPlatformOverridePayloadService();
            foreach(string path in Directory.GetFiles(Path.Combine(context.ProjectRootPath,"assets/scenes/games/tilt"),"tilt_trial_level_*.helen")) {
                SceneAsset scene;using(var input=new MemoryStream(File.ReadAllBytes(path))) scene=(SceneAsset)helengine.editor.AssetSerializer.Deserialize(input);
                var entities=Flatten(scene.RootEntities).ToArray();
                var player=entities.Single(entity=>entity.Name=="PlayerSphere");
                var stage=entities.Single(entity=>entity.Components.Any(record=>record.ComponentTypeId=="DemoDisc.TiltPlay.DemoTiltStageComponent, TiltPlay"));
                var goals=entities.Where(entity=>entity.Components.Any(record=>record.ComponentTypeId.Contains("BlueprintInstanceComponent",StringComparison.Ordinal)) && entity.Name.Contains("Goal",StringComparison.OrdinalIgnoreCase)).ToArray();
                if(goals.Length!=1) throw new InvalidOperationException("Expected exactly one goal Blueprint root in "+path+": "+string.Join(",",entities.Where(entity=>entity.Name.Contains("Goal",StringComparison.OrdinalIgnoreCase)).Select(entity=>entity.Name)));
                foreach(var record in entities.SelectMany(entity=>entity.Components).Where(record=>record.ComponentTypeId=="DemoDisc.TiltPlay.TiltTrialSessionComponent, TiltPlay")) {
                    var descriptor=registry.GetDescriptor(record.ComponentTypeId);
                    var live=descriptor.DeserializeComponent(overrides.UnwrapBaseRecord(record),null,null);
                    live.GetType().GetProperty("PlayerSphereReference").SetValue(live,new SceneEntityReference {EntityId=player.Id});
                    live.GetType().GetProperty("StageRootReference").SetValue(live,new SceneEntityReference {EntityId=stage.Id});
                    live.GetType().GetProperty("GoalEntityReference").SetValue(live,new SceneEntityReference {EntityId=goals[0].Id});
                    var save=new EntityComponentSaveState {ComponentKey=record.ComponentKey};
                    foreach(var state in overrides.ReadOverrideStates(record)) save.SetScopedPlatformOverride(state.Scope,state);
                    var replacement=descriptor.SerializeComponent(live,record.ComponentIndex,save);replacement.ComponentKey=record.ComponentKey;
                    record.Payload=overrides.Wrap(replacement,save).Payload;
                    live.Dispose();
                }
                using var output=new MemoryStream();helengine.editor.AssetSerializer.Serialize(output,scene);File.WriteAllBytes(path,output.ToArray());
                Console.WriteLine("Bound original Tilt session player="+player.Id+" stage="+stage.Id+" goal="+goals[0].Id+": "+path);
            }
        }
        static IEnumerable<SceneEntityAsset> Flatten(IEnumerable<SceneEntityAsset> roots) {foreach(var entity in roots){yield return entity;foreach(var child in Flatten(entity.Children))yield return child;}}
    }
}
