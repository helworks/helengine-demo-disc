using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the authored Tilt Trial camera stays on the intended closer orbit without changing its framing.
    /// </summary>
    public sealed class TiltTrialCameraAuthoringTests {
        /// <summary>
        /// Ensures the serialized first Tilt Trial gameplay scene carries the current tighter camera pose instead of a stale farther view.
        /// </summary>
        [Fact]
        public void Tilt_trial_scene_asset_camera_uses_the_current_close_view_pose() {
            string sceneAssetPath = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial_level_01.helen");
            string bytes = BitConverter.ToString(File.ReadAllBytes(sceneAssetPath));

            Assert.Contains("54-69-6C-74-54-72-69-61-6C-43-61-6D-65-72-61-00-01-00-40-00-00-00-00-CB-A1-2F-40-52-B8-2E-C1", bytes, StringComparison.Ordinal);
            Assert.DoesNotContain("54-69-6C-74-54-72-69-61-6C-43-61-6D-65-72-61-00-01-00-40-00-00-00-00-CB-A1-2F-40-B8-1E-45-C0", bytes, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the generated handheld selector keeps its authored Fredoka dependency in the serialized scene asset.
        /// </summary>
        [Fact]
        public void Tilt_trial_handheld_selector_scene_asset_keeps_fredoka_dependency() {
            string sceneAssetPath = global::DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", "tilt_trial_ds.helen");
            using FileStream stream = File.OpenRead(sceneAssetPath);
            SceneAsset sceneAsset = Assert.IsType<SceneAsset>(global::helengine.AssetSerializer.Deserialize(stream));

            Assert.Contains(
                sceneAsset.AssetReferences,
                reference => reference.SourceKind == SceneAssetReferenceSourceKind.FileSystem &&
                    string.Equals(reference.RelativePath, "fonts/Fredoka.ttf", StringComparison.Ordinal));
        }
    }
}
