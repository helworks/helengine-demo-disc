using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies gameplay generation reaches file-backed assets only through the public editor authoring capability.
    /// </summary>
    public sealed class PublicAuthoringBoundarySourceTests {
        static int CountOccurrences(string source, string value) {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0) {
                count++;
                offset += value.Length;
            }
            return count;
        }

        /// <summary>
        /// Ensures the generated native output catalog includes the less frequently used shared Tilt Trial assets and the full PBR grid.
        /// </summary>
        [Fact]
        public void Project_authoring_identity_catalog_covers_all_stable_shared_asset_slots() {
            string[] nativeAssetPaths = {
                "models/games/tilt/rotating_platform.hasset",
                "models/games/tilt/pendulum_hammer.hasset",
                "models/games/tilt/pendulum_hammer_ds.hasset",
                "blueprints/games/tilt/RotatingPlatform.hblueprint",
                "blueprints/games/tilt/PendulumHammer.hblueprint"
            };
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (string relativePath in nativeAssetPaths) {
                string identity = global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath);
                Assert.Matches("^[0-9a-f]{32}$", identity);
                Assert.True(identities.Add(identity), $"Duplicate project identity '{identity}' for '{relativePath}'.");
            }

            for (int metallicIndex = 0; metallicIndex < 5; metallicIndex++) {
                for (int roughnessIndex = 0; roughnessIndex < 5; roughnessIndex++) {
                    string relativePath = $"materials/rendering/pbr_gallery/M{metallicIndex}R{roughnessIndex}.hasset";
                    string identity = global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetMaterialIdentity(relativePath);
                    Assert.Matches("^[0-9a-f]{32}$", identity);
                    Assert.True(identities.Add(identity), $"Duplicate project identity '{identity}' for '{relativePath}'.");
                }
            }
        }

        /// <summary>
        /// Executes every PBR gallery identity slot and verifies each stable identity is deterministic and unique.
        /// </summary>
        [Fact]
        public void Pbr_gallery_identity_catalog_returns_distinct_deterministic_ids_for_all_25_slots() {
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            for (int metallicIndex = 0; metallicIndex < 5; metallicIndex++) {
                for (int roughnessIndex = 0; roughnessIndex < 5; roughnessIndex++) {
                    string relativePath = $"materials/rendering/pbr_gallery/M{metallicIndex}R{roughnessIndex}.hasset";
                    string expectedIdentity = $"220000000000000000000000000000{metallicIndex:X1}{roughnessIndex:X1}";
                    string identity = global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetMaterialIdentity(relativePath);

                    Assert.Equal(expectedIdentity, identity);
                    Assert.Equal(identity, global::DemoDisc.EditorTools.ProjectAuthoringAssetIdentityCatalog.GetMaterialIdentity(relativePath));
                    Assert.Matches("^[0-9a-f]{32}$", identity);
                    Assert.True(identities.Add(identity), $"Duplicate PBR gallery identity '{identity}' for '{relativePath}'.");
                }
            }

            Assert.Equal(25, identities.Count);
        }
    }
}
