using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    public sealed class CubeTestSceneReferencePersistenceTests {
        [Fact]
        public void Cube_test_persists_its_fixed_light_and_label_links() {
            string path = DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "rendering", "cube_test.helen");
            using FileStream stream = File.OpenRead(path);
            SceneAsset scene = (SceneAsset)global::helengine.AssetSerializer.Deserialize(stream);

            SceneEntityAsset sun = Find(scene.RootEntities, "CubeTestSun");
            SceneEntityAsset ui = Find(scene.RootEntities, "CubeTestUi");
            SceneEntityAsset swatch = Find(scene.RootEntities, "DemoDiscLightIndicatorSwatch");
            SceneEntityAsset label = Find(scene.RootEntities, "DemoDiscSceneLabelText");

            DemoDiscLightToggleComponent toggle = ReadComponent<DemoDiscLightToggleComponent>(ui);
            DemoDiscDebugSceneLabelComponent debugLabel = ReadComponent<DemoDiscDebugSceneLabelComponent>(ui);
            Assert.Equal(sun.Id, Assert.Single(toggle.LightEntityReferences).EntityId);
            Assert.Equal(swatch.Id, toggle.IndicatorSwatchEntityReference?.EntityId);
            Assert.Equal(label.Id, debugLabel.LabelEntityReference?.EntityId);
        }

        static SceneEntityAsset Find(SceneEntityAsset[] roots, string name) {
            foreach (SceneEntityAsset root in roots ?? Array.Empty<SceneEntityAsset>()) {
                SceneEntityAsset found = Find(root, name);
                if (found != null) return found;
            }
            throw new InvalidOperationException($"Authored cube scene is missing '{name}'.");
        }

        static SceneEntityAsset Find(SceneEntityAsset entity, string name) {
            if (entity.Name == name) return entity;
            foreach (SceneEntityAsset child in entity.Children ?? Array.Empty<SceneEntityAsset>()) {
                SceneEntityAsset found = Find(child, name);
                if (found != null) return found;
            }
            return null;
        }

        static TComponent ReadComponent<TComponent>(SceneEntityAsset entity) where TComponent : Component {
            ComponentPersistenceRegistry registry = new ComponentPersistenceRegistry();
            string typeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TComponent));
            foreach (SceneComponentAssetRecord record in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                if (record.ComponentTypeId == typeId) {
                    return (TComponent)registry.GetDescriptor(typeId).DeserializeComponent(record, null, null);
                }
            }
            throw new InvalidOperationException($"Authored '{entity.Name}' is missing {typeof(TComponent).Name}.");
        }
    }
}
