using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Verifies the generated demo-disc menu scenes remain silent until music is intentionally reintroduced.
    /// </summary>
    public sealed class DemoDiscMainMenuAudioSourceTests {
        static string ProjectRootPath => global::city.testing.DemoDiscTestProject.RootPath;
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

        /// <summary>
        /// Ensures both menu scene factories no longer author music entities.
        /// </summary>
        [Fact]
        public void Menu_scene_factories_do_not_author_music() {
            string standardFactorySource = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "menu.tools", "DemoDiscStandardMainMenuSceneFactory.cs"));
            string handheldFactorySource = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "menu.tools", "DemoDiscHandheldMainMenuSceneFactory.cs"));
            string themeSource = File.ReadAllText(global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase", "menu.authoring", "DemoDiscMenuTheme.cs"));

            Assert.DoesNotContain("AudioSourceComponent", standardFactorySource, StringComparison.Ordinal);
            Assert.DoesNotContain("AudioSourceComponent", handheldFactorySource, StringComparison.Ordinal);
            Assert.DoesNotContain("ThemeMusic", themeSource, StringComparison.Ordinal);
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
