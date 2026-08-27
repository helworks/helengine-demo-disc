namespace city.tests {
    /// <summary>
    /// Verifies gameplay generation reaches file-backed assets only through the public editor authoring capability.
    /// </summary>
    public sealed class PublicAuthoringBoundarySourceTests {
        /// <summary>
        /// Ensures gameplay scene factories do not depend on the scene-tools reference wrapper.
        /// </summary>
        [Fact]
        public void Gameplay_scene_factories_use_the_public_authoring_capability_for_references() {
            string gameSceneFactorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");
            string zombislayerSceneFactorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\ZombislayerSceneFactory.cs");

            Assert.DoesNotContain("DemoDiscEditorAssetReferenceFactory", gameSceneFactorySource, StringComparison.Ordinal);
            Assert.DoesNotContain("DemoDiscEditorAssetReferenceFactory", zombislayerSceneFactorySource, StringComparison.Ordinal);
            Assert.Contains("AssetAuthoringService.CreateFileReference", gameSceneFactorySource, StringComparison.Ordinal);
            Assert.Contains("AssetAuthoringService.CreateFileReference", zombislayerSceneFactorySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures Task 6 command paths do not construct editor readers, writers, content managers, or resolvers directly.
        /// </summary>
        [Fact]
        public void Current_generation_paths_use_only_the_public_authoring_boundary() {
            string[] sourcePaths = {
                @"C:\dev\helprojs\demodisc\assets\codebase\game.tools\TiltTrialLevel01TessellationAuthoringService.cs",
                @"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\NormalizeRenderingAndPhysicsMusicGainCommand.cs",
                @"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsNintendoDsSceneGenerator.cs",
                @"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneFactory.cs",
                @"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\GeneratedAuthoringSceneWriteService.cs",
                @"C:\dev\helprojs\demodisc\assets\codebase\game.tools\TiltTrialGameplayPresentationAttachmentService.cs"
            };

            foreach (string sourcePath in sourcePaths) {
                string source = File.ReadAllText(sourcePath);
                Assert.DoesNotContain("AssetSerializer", source, StringComparison.Ordinal);
                Assert.DoesNotContain("GeneratedAssetWriteService", source, StringComparison.Ordinal);
                Assert.DoesNotContain("new ContentManager", source, StringComparison.Ordinal);
                Assert.DoesNotContain("new EditorSceneAssetReferenceResolver", source, StringComparison.Ordinal);
                Assert.DoesNotContain("RemoveLegacyPresentationRoots", source, StringComparison.Ordinal);
                Assert.DoesNotContain("ExcludeLegacyOverlayFromConsoles", source, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Ensures every generated native writer call supplies the project-owned stable identity catalog.
        /// </summary>
        [Fact]
        public void Generated_native_writers_supply_explicit_project_identities() {
            string codebasePath = Path.Combine(@"C:\dev\helprojs\demodisc", "assets", "codebase");
            string generatedSceneWriterSource = File.ReadAllText(Path.Combine(codebasePath, "rendering.tools", "GeneratedAuthoringSceneWriteService.cs"));
            Assert.Contains("global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity", generatedSceneWriterSource, StringComparison.Ordinal);

            string[] productionSourcePaths = Directory.GetFiles(codebasePath, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(".tests", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string sourcePath in productionSourcePaths) {
                string source = File.ReadAllText(sourcePath);
                if (source.Contains("WriteNativeAsset(", StringComparison.Ordinal)
                    || source.Contains("WriteNativeBlueprint(", StringComparison.Ordinal)
                    || source.Contains("WriteNativeMaterial(", StringComparison.Ordinal)
                    || source.Contains("WriteNativeScene(", StringComparison.Ordinal)) {
                    Assert.Contains("ProjectAuthoringAssetIdentityCatalog", source, StringComparison.Ordinal);
                }
            }
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
                string identity = global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(relativePath);
                Assert.Matches("^[0-9a-f]{32}$", identity);
                Assert.True(identities.Add(identity), $"Duplicate project identity '{identity}' for '{relativePath}'.");
            }

            for (int metallicIndex = 0; metallicIndex < 5; metallicIndex++) {
                for (int roughnessIndex = 0; roughnessIndex < 5; roughnessIndex++) {
                    string relativePath = $"materials/rendering/pbr_gallery/M{metallicIndex}R{roughnessIndex}.hasset";
                    string identity = global::city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetMaterialIdentity(relativePath);
                    Assert.Matches("^[0-9a-f]{32}$", identity);
                    Assert.True(identities.Add(identity), $"Duplicate project identity '{identity}' for '{relativePath}'.");
                }
            }
        }
    }
}
