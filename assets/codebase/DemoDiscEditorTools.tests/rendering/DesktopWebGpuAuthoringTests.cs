using helengine;
using helengine.editor;
using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>Checks the actual serialized desktop menu and external scenario light bindings.</summary>
    public sealed class DesktopWebGpuAuthoringTests {
        const string Root = "C:/dev/helprojs/demodisc";

        [Fact]
        public void Desktop_definition_and_persisted_menu_expose_every_physics_scene_and_both_games() {
            var definition = new DemoDiscDesktopMenuDefinitionFactory().Create();
            var expected = definition.Panels.SelectMany(panel => panel.Items).Where(item => item.Action.Kind == DemoDisc.menu.MenuActionKind.LoadScene).Select(item => item.Action.TargetId).ToArray();
            var scene = Load("assets/scenes/DemoDiscMainMenu.helen");
            var actual = Flatten(scene.RootEntities).SelectMany(entity => entity.Components).Where(record => record.ComponentTypeId == "DemoDisc.menu.MenuItemComponent, DemoDisc")
                .Select(record => ReadString(record, "TargetId")).ToArray();
            foreach(string id in expected) Assert.Single(actual.Where(target => target == id));
            Assert.Equal(12, definition.Panels.Single(panel => panel.PanelId == "physics-select").Items.Count(item => item.Action.Kind == DemoDisc.menu.MenuActionKind.LoadScene));
            Assert.Equal(2, definition.Panels.Single(panel => panel.PanelId == "games-select").Items.Count(item => item.Action.Kind == DemoDisc.menu.MenuActionKind.LoadScene));
        }

        [Fact]
        public void Persisted_physics_light_references_target_the_scenario_directional_light() {
            foreach(string path in Directory.GetFiles(Path.Combine(Root,"assets/scenes/physics"),"*.helen")) {
                using var stream=File.OpenRead(path);
                var scene=(SceneAsset)helengine.editor.AssetSerializer.Deserialize(stream);
                var entities=Flatten(scene.RootEntities).ToDictionary(entity=>entity.Id);
                foreach(var record in entities.Values.SelectMany(entity=>entity.Components).Where(record=>record.ComponentTypeId=="DemoDisc.rendering.DemoDiscLightToggleComponent, DemoDisc")) {
                    var fields=new EditorTaggedSceneComponentFieldReader(new ComponentPlatformOverridePayloadService().UnwrapBaseRecord(record).Payload);
                    Assert.True(fields.TryGetFieldReader("LightEntityReferences",out var reader));
                    using(reader) foreach(var reference in reader.ReadArray(value=>value.ReadSceneEntityReference())) {
                        Assert.True(entities.TryGetValue(reference.EntityId,out var target),path);
                        Assert.Contains(target.Components,value=>value.ComponentTypeId=="helengine.DirectionalLightComponent");
                    }
                }
            }
        }

        static SceneAsset Load(string relative) {using var stream=File.OpenRead(Path.Combine(Root,relative));return (SceneAsset)helengine.editor.AssetSerializer.Deserialize(stream);}
        static IEnumerable<SceneEntityAsset> Flatten(IEnumerable<SceneEntityAsset> roots) {foreach(var entity in roots){yield return entity;foreach(var child in Flatten(entity.Children)) yield return child;}}
        static string ReadString(SceneComponentAssetRecord record,string name) {var fields=new EditorTaggedSceneComponentFieldReader(new ComponentPlatformOverridePayloadService().UnwrapBaseRecord(record).Payload);Assert.True(fields.TryGetFieldReader(name,out var reader));using(reader)return reader.ReadString();}
    }
}
