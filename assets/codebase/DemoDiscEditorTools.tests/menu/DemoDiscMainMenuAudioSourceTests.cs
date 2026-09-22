using helengine;
using helengine.editor;
using DemoDisc.EditorTools;
using DemoDisc.menu;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the generated demo-disc menu scenes remain silent until music is intentionally reintroduced.
    /// </summary>
    public sealed class DemoDiscMainMenuAudioSourceTests {
        static string ProjectRootPath => global::DemoDisc.testing.DemoDiscTestProject.RootPath;
        /// <summary>
        /// Ensures the persisted standard and handheld menu scenes contain no serialized audio source component or menu music reference.
        /// </summary>
        [Fact]
        public void Generated_menu_scenes_are_silent() {
            SceneAsset standardScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneAsset handheldScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenuHandheld.helen");
            string audioSourceComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(AudioSourceComponent));

            Assert.DoesNotContain(standardScene.AssetReferences, reference => reference != null && reference.RelativePath.Contains("audio/", StringComparison.Ordinal));
            Assert.DoesNotContain(handheldScene.AssetReferences, reference => reference != null && reference.RelativePath.Contains("audio/", StringComparison.Ordinal));
            Assert.DoesNotContain(FlattenComponents(standardScene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
            Assert.DoesNotContain(FlattenComponents(handheldScene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
        }

        [Fact]
        public void Standard_menu_uses_viewport_scaling_for_the_shared_720p_layout() {
            SceneAsset scene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneEntityAsset menuRoot = Assert.Single(scene.RootEntities, entity => entity.Name == "DemoDiscMenuRoot");
            string viewportTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(ViewportComponent));
            string canvasFitTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(ReferenceCanvasFitComponent));
            SceneComponentAssetRecord viewportRecord = Assert.Single(
                menuRoot.Components ?? Array.Empty<SceneComponentAssetRecord>(),
                component => component.ComponentTypeId == viewportTypeId);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create();
            ViewportComponent viewport = Assert.IsType<ViewportComponent>(
                registry.GetDescriptor(viewportRecord.ComponentTypeId).DeserializeComponent(viewportRecord, new EntitySaveComponent(), null));

            Assert.Equal(ViewportComponent.ScreenBindingMode, viewport.BindingMode);
            Assert.Equal(ViewportComponent.ReferenceCanvasScalingMode, viewport.ScalingMode);
            Assert.Equal(new int2(1280, 720), viewport.FixedSize);
            Assert.Equal(1280, viewport.ReferenceWidth);
            Assert.Equal(720, viewport.ReferenceHeight);
            Assert.DoesNotContain(FlattenComponents(scene.RootEntities), component => component.ComponentTypeId == canvasFitTypeId);
        }

        static SceneAsset LoadSceneAsset(string relativePath) {
            string fullPath = Path.Combine(ProjectRootPath, relativePath);
            using FileStream stream = File.OpenRead(fullPath);
            return Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
        }

        static IReadOnlyList<SceneComponentAssetRecord> FlattenComponents(SceneEntityAsset[] rootEntities) {
            List<SceneComponentAssetRecord> components = new List<SceneComponentAssetRecord>();
            AppendComponents(rootEntities, components);
            return components;
        }

        static void AppendComponents(SceneEntityAsset[] entities, List<SceneComponentAssetRecord> components) {
            if (entities == null) {
                return;
            }

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                SceneEntityAsset entity = entities[entityIndex];
                if (entity == null) {
                    continue;
                }

                SceneComponentAssetRecord[] entityComponents = entity.Components ?? Array.Empty<SceneComponentAssetRecord>();
                for (int componentIndex = 0; componentIndex < entityComponents.Length; componentIndex++) {
                    SceneComponentAssetRecord component = entityComponents[componentIndex];
                    if (component != null) {
                        components.Add(component);
                    }
                }

                AppendComponents(entity.Children, components);
            }
        }
    }
}
