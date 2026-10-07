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

            SceneEntityAsset backgroundSprite = Assert.Single(tiles);
            Assert.Equal("DemoDiscAnimatedBackgroundSprite", backgroundSprite.Name);
            Assert.Equal(new float3(DemoMenuLayout.CanvasWidth / 2f, DemoMenuLayout.CanvasHeight / 2f, 0f), background.LocalPosition);
            Assert.Equal(new float3(-768f, -768f, 2f), backgroundSprite.LocalPosition);
            SceneComponentAssetRecord spriteRecord = Assert.Single(
                backgroundSprite.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == spriteTypeId);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            SpriteComponent sprite = Assert.IsType<SpriteComponent>(
                registry.GetDescriptor(spriteRecord.ComponentTypeId).DeserializeComponent(spriteRecord, new EntitySaveComponent(), null));
            Assert.Equal(new int2(1536, 1536), sprite.Size);
            double canvasDiagonal = Math.Sqrt((double)DemoMenuLayout.CanvasWidth * DemoMenuLayout.CanvasWidth
                + (double)DemoMenuLayout.CanvasHeight * DemoMenuLayout.CanvasHeight);
            Assert.True(Math.Min(sprite.Size.X, sprite.Size.Y) > canvasDiagonal,
                "The centered rotating background must exceed the canvas diagonal to cover every angle.");
            Assert.Contains(background.Components ?? Array.Empty<SceneComponentAssetRecord>(), component => component.ComponentTypeId == rotationTypeId);
            Assert.Contains(scene.AssetReferences ?? Array.Empty<SceneAssetReference>(), reference => reference.RelativePath == "textures/menu/main_menu_background.png");
            Assert.DoesNotContain(EnumerateComponents(background), component => component.ComponentTypeId == "helengine.RoundedRectComponent");
        }

        [Fact]
        public void Shared_menu_background_source_is_512_by_512() {
            byte[] png = File.ReadAllBytes(Path.Combine(ProjectRootPath, "assets", "textures", "menu", "main_menu_background.png"));
            Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, png.Take(8).ToArray());
            Assert.Equal(512, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)));
            Assert.Equal(512, System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
        }

        static IEnumerable<SceneComponentAssetRecord> EnumerateComponents(SceneEntityAsset root) {
            foreach (SceneComponentAssetRecord component in root.Components ?? Array.Empty<SceneComponentAssetRecord>()) yield return component;
            foreach (SceneEntityAsset child in root.Children ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneComponentAssetRecord component in EnumerateComponents(child)) yield return component;
            }
        }
    }
}
