using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the handheld Tilt Trial selector uses a two-stage full-width touch layout.
    /// </summary>
    public sealed class TiltTrialLevelSelectLayoutSourceTests {
        /// <summary>
        /// Ensures the generated desktop selector carries the detail Back and Play actions its runtime controller
        /// needs, keeps them scoped to the dual-screen group, and drops the whole selector on dual-screen devices
        /// without naming any single platform.
        /// </summary>
        [Fact]
        public void Generated_desktop_selector_scopes_detail_action_buttons_by_platform_group() {
            string scenePath = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial.helen");
            using FileStream stream = File.OpenRead(scenePath);
            SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
            SceneEntityAsset[] entities = sceneAsset.RootEntities.SelectMany(EnumerateEntities).ToArray();
            SceneEntityAsset selectorUi = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectUi");
            SceneEntityAsset backButton = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectBackButton");
            SceneEntityAsset playButton = Assert.Single(entities, entity => entity.Name == "TiltTrialLevelSelectPlayButton");
            string[] singleScreenPlatformIds = ["windows", "ps2", "psp", "n64", "dc", "ps3", "x360"];
            string[] dualScreenPlatformIds = ["ds", "3ds"];

            for (int index = 0; index < singleScreenPlatformIds.Length; index++) {
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(selectorUi, singleScreenPlatformIds[index]));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(backButton, singleScreenPlatformIds[index]));
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(playButton, singleScreenPlatformIds[index]));
            }

            for (int index = 0; index < dualScreenPlatformIds.Length; index++) {
                Assert.False(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(selectorUi, dualScreenPlatformIds[index]));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(backButton, dualScreenPlatformIds[index]));
                Assert.True(global::DemoDisc.testing.DemoDiscOverrideScopeReader.ExistsOnPlatform(playButton, dualScreenPlatformIds[index]));
            }
        }

        static IEnumerable<SceneEntityAsset> EnumerateEntities(SceneEntityAsset root) {
            if (root == null) {
                yield break;
            }

            yield return root;
            foreach (SceneEntityAsset child in root.Children ?? Array.Empty<SceneEntityAsset>()) {
                foreach (SceneEntityAsset descendant in EnumerateEntities(child)) {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Counts exact non-overlapping occurrences of one source fragment.
        /// </summary>
        /// <param name="source">Source text to inspect.</param>
        /// <param name="value">Fragment whose occurrences should be counted.</param>
        /// <returns>Number of exact fragment occurrences.</returns>
        static int CountOccurrences(string source, string value) {
            int count = 0;
            int searchStart = 0;
            while ((searchStart = source.IndexOf(value, searchStart, StringComparison.Ordinal)) >= 0) {
                count++;
                searchStart += value.Length;
            }

            return count;
        }
    }
}
