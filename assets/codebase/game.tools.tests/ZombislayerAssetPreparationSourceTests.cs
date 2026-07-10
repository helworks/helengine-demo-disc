namespace city.tests {
    /// <summary>
    /// Verifies the Zombislayer asset-preparation layer stages the imported environment and weapon models through explicit project-relative asset catalog entries.
    /// </summary>
    public sealed class ZombislayerAssetPreparationSourceTests {
        /// <summary>
        /// Ensures the Zombislayer asset catalog centralizes the imported environment and weapon model paths.
        /// </summary>
        [Fact]
        public void Zombislayer_asset_catalog_exposes_environment_and_weapon_model_paths() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\ZombislayerAssetCatalog.cs");

            Assert.Contains("public const string EnvironmentModelRelativePath = \"models/games/zombislayer/level/level.X\";", source, StringComparison.Ordinal);
            Assert.Contains("public const string WeaponModelRelativePath = \"models/games/zombislayer/weapons/m4a1.X\";", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the Zombislayer asset-preparation service loads the imported environment and weapon models through the shared helper path.
        /// </summary>
        [Fact]
        public void Zombislayer_asset_preparation_service_loads_environment_and_weapon_models() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\game.tools\ZombislayerAssetPreparationService.cs");

            Assert.Contains("LoadImportedModelRuntime(projectRootPath, ZombislayerAssetCatalog.EnvironmentModelRelativePath)", source, StringComparison.Ordinal);
            Assert.Contains("LoadImportedModelRuntime(projectRootPath, ZombislayerAssetCatalog.WeaponModelRelativePath)", source, StringComparison.Ordinal);
            Assert.Contains("return new ZombislayerGenerationAssets", source, StringComparison.Ordinal);
        }
    }
}
