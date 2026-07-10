using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Verifies the generated demo-disc menu scenes and scene factories author looping theme music through the shared audio asset pipeline.
    /// </summary>
    public sealed class DemoDiscMainMenuAudioSourceTests {
        const string ProjectRootPath = @"C:\dev\helprojs\city";
        const string ThemeAudioRelativePath = "audio/menu/helen_of_code_high_code_v2.wav";

        /// <summary>
        /// Ensures the persisted standard and handheld menu scenes both include the shared menu music asset reference and one serialized audio source component.
        /// </summary>
        [Fact]
        public void Generated_menu_scenes_include_theme_audio_reference_and_audio_source_component() {
            SceneAsset standardScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenu.helen");
            SceneAsset handheldScene = LoadSceneAsset(@"assets\scenes\DemoDiscMainMenuHandheld.helen");
            string audioSourceComponentTypeId = AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(AudioSourceComponent));

            Assert.Contains(standardScene.AssetReferences, reference => string.Equals(reference.RelativePath, ThemeAudioRelativePath, StringComparison.Ordinal));
            Assert.Contains(handheldScene.AssetReferences, reference => string.Equals(reference.RelativePath, ThemeAudioRelativePath, StringComparison.Ordinal));
            Assert.Contains(FlattenComponents(standardScene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
            Assert.Contains(FlattenComponents(handheldScene.RootEntities), component => string.Equals(component.ComponentTypeId, audioSourceComponentTypeId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Ensures both menu scene factories author the same looping music configuration and file-backed audio reference path.
        /// </summary>
        [Fact]
        public void Menu_scene_factories_author_looping_music_configuration() {
            string standardFactorySource = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\menu.tools\DemoDiscStandardMainMenuSceneFactory.cs");
            string handheldFactorySource = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\menu.tools\DemoDiscHandheldMainMenuSceneFactory.cs");
            string themeSource = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\menu\DemoDiscMenuTheme.cs");

            AssertContainsMusicConfiguration(standardFactorySource);
            AssertContainsMusicConfiguration(handheldFactorySource);
            Assert.Contains("ThemeMusicAudioPath", themeSource, StringComparison.Ordinal);
            Assert.Contains(ThemeAudioRelativePath, themeSource, StringComparison.Ordinal);
            Assert.Contains("ThemeMusicGain => 0.3f", themeSource, StringComparison.Ordinal);
        }

        static void AssertContainsMusicConfiguration(string source) {
            Assert.Contains("new AudioSourceComponent", source, StringComparison.Ordinal);
            Assert.Contains("PlayOnStart = true", source, StringComparison.Ordinal);
            Assert.Contains("Loop = true", source, StringComparison.Ordinal);
            Assert.Contains("BusId = \"music\"", source, StringComparison.Ordinal);
            Assert.Contains("Gain = Theme.ThemeMusicGain", source, StringComparison.Ordinal);
            Assert.Contains("CreateFileSystemAudio", source, StringComparison.Ordinal);
            Assert.Contains("ThemeMusicAudioPath", source, StringComparison.Ordinal);
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
