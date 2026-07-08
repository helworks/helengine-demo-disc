namespace city.menu {
    /// <summary>
    /// Resolves the runtime demo-disc main-menu scene id, including the dedicated Nintendo handheld fallback when the logical desktop menu id is not packaged.
    /// </summary>
    public static class DemoDiscMainMenuSceneResolver {
        /// <summary>
        /// Resolves the runtime scene id that should be used when demo-disc content returns to the main menu.
        /// </summary>
        /// <returns>Packaged runtime scene id for the demo-disc main menu.</returns>
        public static string ResolveRuntimeSceneId() {
            string resolvedSceneId = SceneMapComponent.ResolveSceneId(PlatformMenuSceneResolver.DesktopMainMenuSceneId);
            if (CanLoadRuntimeScene(resolvedSceneId)) {
                return resolvedSceneId;
            }
            if (CanLoadRuntimeScene(PlatformMenuSceneResolver.NintendoHandheldMainMenuSceneId)) {
                return PlatformMenuSceneResolver.NintendoHandheldMainMenuSceneId;
            }

            return resolvedSceneId;
        }

        /// <summary>
        /// Returns whether the supplied runtime scene id is currently available in the active packaged scene catalog.
        /// </summary>
        /// <param name="sceneId">Runtime scene id being evaluated for a load request.</param>
        /// <returns>True when the scene exists in the active runtime catalog or no restrictive catalog is available.</returns>
        static bool CanLoadRuntimeScene(string sceneId) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                return false;
            }
            if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before loading runtime scenes.");
            }
            if (Core.Instance.SceneManager == null) {
                throw new InvalidOperationException("Core scene manager must be initialized before runtime scene loading can occur.");
            }

            CoreInitializationOptions initializationOptions = Core.Instance.InitializationOptions;
            RuntimeSceneCatalog sceneCatalog = initializationOptions != null ? initializationOptions.SceneCatalog : null;
            if (sceneCatalog == null || sceneCatalog.Entries == null) {
                return true;
            }

            RuntimeSceneCatalogEntry[] entries = sceneCatalog.Entries;
            for (int entryIndex = 0; entryIndex < entries.Length; entryIndex++) {
                RuntimeSceneCatalogEntry entry = entries[entryIndex];
                if (entry != null && string.Equals(entry.SceneId, sceneId, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }
    }
}
