using helengine;
using helengine.editor;
using DemoDisc.EditorTools;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
    public sealed class DemoDiscMainMenuBackgroundTests {
        static string ProjectRootPath => global::DemoDisc.testing.DemoDiscTestProject.RootPath;

        [Fact]
        public void Standard_menu_persists_the_shared_rotating_texture_background() {
            string scenePath = Path.Combine(ProjectRootPath, @"assets\scenes\DemoDiscMainMenu.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset menuRoot = Assert.Single(scene.RootEntities, entity => entity.Name == "DemoDiscMenuRoot");
            SceneEntityAsset generatedRoot = Assert.Single(menuRoot.Children, entity => entity.Name == DemoMenuLayout.GeneratedRootEntityName);
            SceneEntityAsset background = Assert.Single(generatedRoot.Children, entity => entity.Name == "DemoDiscAnimatedBackground");
            SceneEntityAsset[] tiles = background.Children ?? Array.Empty<SceneEntityAsset>();
            const string spriteTypeId = "helengine.SpriteComponent";
            string rotationTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(DemoDisc.rendering.AxisRotationComponent));

            Assert.Equal(4, tiles.Length);
            Assert.All(tiles, tile => Assert.Contains(tile.Components ?? Array.Empty<SceneComponentAssetRecord>(), component => component.ComponentTypeId == spriteTypeId));
            Assert.Contains(background.Components ?? Array.Empty<SceneComponentAssetRecord>(), component => component.ComponentTypeId == rotationTypeId);
            Assert.Contains(scene.AssetReferences ?? Array.Empty<SceneAssetReference>(), reference => reference.RelativePath == "textures/menu/main_menu_background.png");
            Assert.DoesNotContain(EnumerateComponents(background), component => component.ComponentTypeId == "helengine.RoundedRectComponent");
        }

        static IEnumerable<SceneComponentAssetRecord> EnumerateComponents(SceneEntityAsset root) {
            foreach (SceneComponentAssetRecord component in root.Components ?? Array.Empty<SceneComponentAssetRecord>()) yield return component;
            foreach (SceneEntityAsset child in root.Children ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneComponentAssetRecord component in EnumerateComponents(child)) yield return component;
            }
        }
    }
}