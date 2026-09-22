using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the Zombislayer game-scene generator writes a dedicated gameplay scene backed by imported environment and weapon assets.
    /// </summary>
    public sealed class ZombislayerSceneGenerationSourceTests {
        /// <summary>
        /// Ensures only the canonical current authored path is registered for the runtime Zombislayer scene id.
        /// </summary>
        [Fact]
        public void Zombislayer_scene_identity_catalog_uses_only_the_canonical_current_path() {
            const string canonicalPath = "scenes/games/zombislayer.helen";
            const string runtimeSceneId = "zombislayer";
            const string expectedIdentity = "10000000000000000000000000000038";

            Assert.Equal(expectedIdentity, global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(canonicalPath));
            Assert.Throws<InvalidOperationException>(() => global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(runtimeSceneId));
            Assert.Throws<InvalidOperationException>(() => global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity("zombislayer.helen"));
        }

        /// <summary>
        /// Loads the generated current-format Zombislayer scene and verifies its embedded identity and authored model references.
        /// </summary>
        [Fact]
        public void Generated_zombislayer_scene_loads_from_the_canonical_current_path() {
            const string scenePath = @"C:\dev\helprojs\demodisc\assets\scenes\games\zombislayer.helen";
            const string sidecarPath = scenePath + ".hmeta";
            const string expectedIdentity = "10000000000000000000000000000038";

            Assert.True(File.Exists(scenePath), $"Expected generated scene '{scenePath}'.");
            Assert.False(File.Exists(sidecarPath), $"Current native scene must not have a sidecar '{sidecarPath}'.");
            using FileStream stream = File.OpenRead(scenePath);
            byte[] header = new byte[4];
            stream.ReadExactly(header);
            Assert.Equal("HELE", global::System.Text.Encoding.ASCII.GetString(header));
            stream.Position = 0;
            SceneAsset scene = Assert.IsType<SceneAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));

            Assert.Equal(expectedIdentity, scene.AuthoringAssetId);
            Assert.Equal("zombislayer", global::DemoDisc.Zombislayer.ZombislayerSceneIds.GameplaySceneId);
            Assert.Contains(scene.AssetReferences, reference => string.Equals(
                reference.RelativePath,
                global::DemoDisc.EditorTools.ZombislayerAssetCatalog.EnvironmentModelRelativePath,
                StringComparison.OrdinalIgnoreCase));
            Assert.Contains(scene.AssetReferences, reference => string.Equals(
                reference.RelativePath,
                global::DemoDisc.EditorTools.ZombislayerAssetCatalog.WeaponModelRelativePath,
                StringComparison.OrdinalIgnoreCase));
        }
    }
}
