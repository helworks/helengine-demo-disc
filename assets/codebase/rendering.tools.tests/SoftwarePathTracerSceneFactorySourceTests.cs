namespace city.tests {
    /// <summary>
    /// Guards the source-level boundary between the fixed common scene and later platform rollout tasks.
    /// </summary>
    public sealed class SoftwarePathTracerSceneFactorySourceTests {
        [Fact]
        public void Factory_source_declares_the_fixed_contract_and_accepted_constants() {
            string source = File.ReadAllText(FactorySourcePath());

            Assert.Contains("public const string SceneId = \"scenes/rendering/software_path_tracer.helen\";", source, StringComparison.Ordinal);
            Assert.Contains("CreateSceneDefinition(string projectRootPath, SceneAssetReference cubeReference, FontAsset hudFont)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(0f, 0f, 3f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(0f, 0f, -1f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(1f, 0f, 0f)", source, StringComparison.Ordinal);
            Assert.Contains("new float3(0f, 1f, 0f)", source, StringComparison.Ordinal);
            Assert.Contains("VerticalFieldOfViewDegrees = 55f", source, StringComparison.Ordinal);
            Assert.Contains("Exposure = 1f", source, StringComparison.Ordinal);
            Assert.Contains("EmissionStrength = 14f", source, StringComparison.Ordinal);
            Assert.Contains("new SoftwareModelComponent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("MeshComponent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PlatformEditingService", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ApplyPlatform", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Catalog_source_declares_the_reserved_software_scene_identity() {
            string catalogPath = ResolveProjectRootFile("assets", "codebase", "scene.tools", "ProjectAuthoringAssetIdentityCatalog.cs");
            string source = File.ReadAllText(catalogPath);

            Assert.Contains("[\"scenes/rendering/software_path_tracer.helen\"] = \"1000000000000000000000000000001f\",", source, StringComparison.Ordinal);
        }

        static string FactorySourcePath() {
            return ResolveProjectRootFile("assets", "codebase", "rendering.tools", "SoftwarePathTracerSceneFactory.cs");
        }

        static string ResolveProjectRootFile(params string[] relativeParts) {
            string sourceFilePath = typeof(SoftwarePathTracerSceneFactorySourceTests).Assembly.Location;
            DirectoryInfo currentDirectory = new DirectoryInfo(Path.GetDirectoryName(sourceFilePath));
            while (currentDirectory != null) {
                string assetsPath = Path.Combine(currentDirectory.FullName, "assets");
                string projectFilePath = Path.Combine(currentDirectory.FullName, "project.heproj");
                if (Directory.Exists(assetsPath) && File.Exists(projectFilePath)) {
                    return Path.Combine(new[] { currentDirectory.FullName }.Concat(relativeParts).ToArray());
                }
                currentDirectory = currentDirectory.Parent;
            }

            // Source tests are also run from the generated EditorFull project, where the source file is available to the test assembly only through this checkout-relative fallback.
            string checkoutRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", ".."));
            return Path.Combine(new[] { checkoutRoot }.Concat(relativeParts).ToArray());
        }
    }
}
