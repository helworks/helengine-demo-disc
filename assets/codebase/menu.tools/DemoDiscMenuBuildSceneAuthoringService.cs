using city.rendering.tools;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Applies platform-specific menu item and panel existence overrides based on the local build scene selections stored in <c>user_settings/build_config.json</c>.
    /// </summary>
    public sealed class DemoDiscMenuBuildSceneAuthoringService {
        /// <summary>
        /// Helper used to persist high-level platform subtree existence overrides without touching raw save metadata directly.
        /// </summary>
        readonly PlatformSceneAuthoringHelperService PlatformSceneAuthoringHelperService;

        /// <summary>
        /// Initializes one demo-disc menu build-scene authoring service.
        /// </summary>
        public DemoDiscMenuBuildSceneAuthoringService() {
            PlatformSceneAuthoringHelperService = new PlatformSceneAuthoringHelperService();
        }

        /// <summary>
        /// Applies platform-specific scene availability exclusions to the generated demo-disc menu scene definition.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated scene definition whose menu entities should receive platform exclusions.</param>
        /// <param name="definition">Canonical menu definition used to author the scene hierarchy.</param>
        public void ApplyBuildSceneAvailability(string projectRootPath, GeneratedAuthoringSceneDefinition sceneDefinition, MenuDefinition definition) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            Dictionary<string, HashSet<string>> configuredSceneIdsByPlatform = LoadConfiguredSceneIdsByPlatform(projectRootPath);
            if (configuredSceneIdsByPlatform.Count < 1) {
                return;
            }

            Dictionary<string, IReadOnlyList<string>> excludedPlatformsByPanelId = BuildExcludedPlatformsByPanelId(definition, configuredSceneIdsByPlatform);
            ApplySceneItemExclusions(projectRootPath, sceneDefinition, definition, configuredSceneIdsByPlatform);
            ApplyPanelExclusions(projectRootPath, sceneDefinition, excludedPlatformsByPanelId);
            ApplyOpenPanelItemExclusions(projectRootPath, sceneDefinition, definition, excludedPlatformsByPanelId);
        }

        /// <summary>
        /// Loads the explicitly configured per-platform selected scene ids from the local build configuration document.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <returns>Configured selected scene ids keyed by platform id.</returns>
        static Dictionary<string, HashSet<string>> LoadConfiguredSceneIdsByPlatform(string projectRootPath) {
            EditorBuildConfigDocument buildConfig = new EditorBuildConfigService(projectRootPath).TryLoadExisting();
            Dictionary<string, HashSet<string>> configuredSceneIdsByPlatform = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (buildConfig == null || buildConfig.Platforms == null) {
                return configuredSceneIdsByPlatform;
            }

            for (int index = 0; index < buildConfig.Platforms.Count; index++) {
                EditorBuildPlatformConfigDocument platformConfig = buildConfig.Platforms[index];
                if (platformConfig == null || string.IsNullOrWhiteSpace(platformConfig.PlatformId)) {
                    continue;
                } else if (platformConfig.SelectedSceneIds == null || platformConfig.SelectedSceneIds.Count < 1) {
                    continue;
                }

                HashSet<string> sceneIds = new HashSet<string>(StringComparer.Ordinal);
                for (int sceneIndex = 0; sceneIndex < platformConfig.SelectedSceneIds.Count; sceneIndex++) {
                    string sceneId = platformConfig.SelectedSceneIds[sceneIndex];
                    if (!string.IsNullOrWhiteSpace(sceneId)) {
                        sceneIds.Add(sceneId);
                    }
                }

                if (sceneIds.Count > 0) {
                    configuredSceneIdsByPlatform[platformConfig.PlatformId] = sceneIds;
                }
            }

            return configuredSceneIdsByPlatform;
        }

        /// <summary>
        /// Applies scene-loading item exclusions for platforms that do not package the target scene.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated scene definition whose item entities should receive exclusions.</param>
        /// <param name="definition">Canonical menu definition used to author the scene hierarchy.</param>
        /// <param name="configuredSceneIdsByPlatform">Configured selected scene ids keyed by platform id.</param>
        void ApplySceneItemExclusions(
            string projectRootPath,
            GeneratedAuthoringSceneDefinition sceneDefinition,
            MenuDefinition definition,
            Dictionary<string, HashSet<string>> configuredSceneIdsByPlatform) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (configuredSceneIdsByPlatform == null) {
                throw new ArgumentNullException(nameof(configuredSceneIdsByPlatform));
            }

            for (int panelIndex = 0; panelIndex < definition.Panels.Length; panelIndex++) {
                MenuPanelDefinition panelDefinition = definition.Panels[panelIndex];
                if (panelDefinition == null || panelDefinition.Items == null) {
                    continue;
                }

                for (int itemIndex = 0; itemIndex < panelDefinition.Items.Length; itemIndex++) {
                    MenuItemDefinition itemDefinition = panelDefinition.Items[itemIndex];
                    if (itemDefinition == null || itemDefinition.Action == null) {
                        continue;
                    } else if (itemDefinition.Action.Kind != MenuActionKind.LoadScene) {
                        continue;
                    } else if (string.IsNullOrWhiteSpace(itemDefinition.Action.TargetId)) {
                        continue;
                    }

                    IReadOnlyList<string> excludedPlatformIds = ResolveExcludedPlatformsForSceneId(itemDefinition.Action.TargetId, configuredSceneIdsByPlatform);
                    ApplyEntityExclusionsByName(projectRootPath, sceneDefinition, BuildItemEntityName(itemDefinition.ItemId), excludedPlatformIds);
                }
            }
        }

        /// <summary>
        /// Builds the excluded platform list for every scene-bearing panel in the canonical menu definition.
        /// </summary>
        /// <param name="definition">Canonical menu definition used to author the scene hierarchy.</param>
        /// <param name="configuredSceneIdsByPlatform">Configured selected scene ids keyed by platform id.</param>
        /// <returns>Excluded platform lists keyed by panel id.</returns>
        static Dictionary<string, IReadOnlyList<string>> BuildExcludedPlatformsByPanelId(
            MenuDefinition definition,
            Dictionary<string, HashSet<string>> configuredSceneIdsByPlatform) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (configuredSceneIdsByPlatform == null) {
                throw new ArgumentNullException(nameof(configuredSceneIdsByPlatform));
            }

            Dictionary<string, IReadOnlyList<string>> excludedPlatformsByPanelId = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
            for (int panelIndex = 0; panelIndex < definition.Panels.Length; panelIndex++) {
                MenuPanelDefinition panelDefinition = definition.Panels[panelIndex];
                if (panelDefinition == null) {
                    continue;
                }

                List<string> sceneIds = CollectPanelSceneIds(panelDefinition);
                if (sceneIds.Count < 1) {
                    continue;
                }

                List<string> excludedPlatformIds = [];
                foreach (KeyValuePair<string, HashSet<string>> configuredScenesByPlatformEntry in configuredSceneIdsByPlatform) {
                    bool hasAnyScene = false;
                    for (int sceneIndex = 0; sceneIndex < sceneIds.Count; sceneIndex++) {
                        if (configuredScenesByPlatformEntry.Value.Contains(sceneIds[sceneIndex])) {
                            hasAnyScene = true;
                            break;
                        }
                    }

                    if (!hasAnyScene) {
                        excludedPlatformIds.Add(configuredScenesByPlatformEntry.Key);
                    }
                }

                excludedPlatformsByPanelId[panelDefinition.PanelId] = excludedPlatformIds;
            }

            return excludedPlatformsByPanelId;
        }

        /// <summary>
        /// Applies scene-panel exclusions for platforms that do not package any of the panel's scene targets.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated scene definition whose panel entities should receive exclusions.</param>
        /// <param name="excludedPlatformsByPanelId">Excluded platform lists keyed by scene-bearing panel id.</param>
        void ApplyPanelExclusions(
            string projectRootPath,
            GeneratedAuthoringSceneDefinition sceneDefinition,
            Dictionary<string, IReadOnlyList<string>> excludedPlatformsByPanelId) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (excludedPlatformsByPanelId == null) {
                throw new ArgumentNullException(nameof(excludedPlatformsByPanelId));
            }

            foreach (KeyValuePair<string, IReadOnlyList<string>> excludedPlatformsByPanelEntry in excludedPlatformsByPanelId) {
                ApplyEntityExclusionsByName(
                    projectRootPath,
                    sceneDefinition,
                    BuildPanelEntityName(excludedPlatformsByPanelEntry.Key),
                    excludedPlatformsByPanelEntry.Value);
            }
        }

        /// <summary>
        /// Applies exclusions to top-level main-menu open-panel items when their target panels do not exist for a platform build.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated scene definition whose item entities should receive exclusions.</param>
        /// <param name="definition">Canonical menu definition used to author the scene hierarchy.</param>
        /// <param name="excludedPlatformsByPanelId">Excluded platform lists keyed by scene-bearing panel id.</param>
        void ApplyOpenPanelItemExclusions(
            string projectRootPath,
            GeneratedAuthoringSceneDefinition sceneDefinition,
            MenuDefinition definition,
            Dictionary<string, IReadOnlyList<string>> excludedPlatformsByPanelId) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (excludedPlatformsByPanelId == null) {
                throw new ArgumentNullException(nameof(excludedPlatformsByPanelId));
            }

            for (int panelIndex = 0; panelIndex < definition.Panels.Length; panelIndex++) {
                MenuPanelDefinition panelDefinition = definition.Panels[panelIndex];
                if (panelDefinition == null || panelDefinition.Items == null) {
                    continue;
                }

                for (int itemIndex = 0; itemIndex < panelDefinition.Items.Length; itemIndex++) {
                    MenuItemDefinition itemDefinition = panelDefinition.Items[itemIndex];
                    IReadOnlyList<string> excludedOpenPanelPlatformIds;
                    if (itemDefinition == null || itemDefinition.Action == null) {
                        continue;
                    } else if (itemDefinition.Action.Kind != MenuActionKind.OpenPanel) {
                        continue;
                    } else if (string.IsNullOrWhiteSpace(itemDefinition.Action.TargetId)) {
                        continue;
                    } else if (!excludedPlatformsByPanelId.TryGetValue(itemDefinition.Action.TargetId, out excludedOpenPanelPlatformIds)) {
                        continue;
                    }

                    ApplyEntityExclusionsByName(projectRootPath, sceneDefinition, BuildItemEntityName(itemDefinition.ItemId), excludedOpenPanelPlatformIds);
                }
            }
        }

        /// <summary>
        /// Resolves the configured excluded platform ids for one scene-loading target.
        /// </summary>
        /// <param name="sceneId">Logical scene id targeted by one menu item.</param>
        /// <param name="configuredSceneIdsByPlatform">Configured selected scene ids keyed by platform id.</param>
        /// <returns>Configured platform ids that should exclude the target scene item.</returns>
        static IReadOnlyList<string> ResolveExcludedPlatformsForSceneId(string sceneId, Dictionary<string, HashSet<string>> configuredSceneIdsByPlatform) {
            if (string.IsNullOrWhiteSpace(sceneId)) {
                throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
            } else if (configuredSceneIdsByPlatform == null) {
                throw new ArgumentNullException(nameof(configuredSceneIdsByPlatform));
            }

            List<string> excludedPlatformIds = [];
            foreach (KeyValuePair<string, HashSet<string>> configuredScenesByPlatformEntry in configuredSceneIdsByPlatform) {
                if (!configuredScenesByPlatformEntry.Value.Contains(sceneId)) {
                    excludedPlatformIds.Add(configuredScenesByPlatformEntry.Key);
                }
            }

            return excludedPlatformIds;
        }

        /// <summary>
        /// Collects the scene-loading targets contained by one menu panel.
        /// </summary>
        /// <param name="panelDefinition">Menu panel being inspected.</param>
        /// <returns>Logical scene ids targeted by the panel.</returns>
        static List<string> CollectPanelSceneIds(MenuPanelDefinition panelDefinition) {
            if (panelDefinition == null) {
                throw new ArgumentNullException(nameof(panelDefinition));
            } else if (panelDefinition.Items == null) {
                throw new InvalidOperationException($"Menu panel '{panelDefinition.PanelId}' must provide items.");
            }

            List<string> sceneIds = [];
            for (int itemIndex = 0; itemIndex < panelDefinition.Items.Length; itemIndex++) {
                MenuItemDefinition itemDefinition = panelDefinition.Items[itemIndex];
                if (itemDefinition == null || itemDefinition.Action == null) {
                    continue;
                } else if (itemDefinition.Action.Kind != MenuActionKind.LoadScene) {
                    continue;
                } else if (string.IsNullOrWhiteSpace(itemDefinition.Action.TargetId)) {
                    continue;
                }

                sceneIds.Add(itemDefinition.Action.TargetId);
            }

            return sceneIds;
        }

        /// <summary>
        /// Applies subtree exclusions to every generated menu entity that matches the supplied stable entity name.
        /// </summary>
        /// <param name="projectRootPath">Absolute or relative city project root path.</param>
        /// <param name="sceneDefinition">Generated scene definition whose roots should be searched.</param>
        /// <param name="entityName">Stable generated entity name to search for.</param>
        /// <param name="excludedPlatformIds">Configured platform ids that should exclude the subtree.</param>
        void ApplyEntityExclusionsByName(string projectRootPath, GeneratedAuthoringSceneDefinition sceneDefinition, string entityName, IReadOnlyList<string> excludedPlatformIds) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneDefinition == null) {
                throw new ArgumentNullException(nameof(sceneDefinition));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (excludedPlatformIds == null) {
                throw new ArgumentNullException(nameof(excludedPlatformIds));
            } else if (excludedPlatformIds.Count < 1) {
                return;
            }

            List<EditorEntity> matches = [];
            CollectMatchingEntities(sceneDefinition.RootEntities, entityName, matches);
            if (sceneDefinition.NintendoDsScene != null) {
                CollectMatchingEntities(sceneDefinition.NintendoDsScene.RootEntities, entityName, matches);
                CollectMatchingEntities(sceneDefinition.NintendoDsScene.BottomScreenRootEntities, entityName, matches);
            }

            for (int index = 0; index < matches.Count; index++) {
                PlatformSceneAuthoringHelperService.ExcludeEntitySubtreeFromPlatforms(projectRootPath, matches[index], excludedPlatformIds);
            }
        }

        /// <summary>
        /// Recursively collects generated entities whose stable name matches the requested value.
        /// </summary>
        /// <param name="roots">Root entities whose subtrees should be searched.</param>
        /// <param name="entityName">Stable generated entity name to search for.</param>
        /// <param name="matches">Mutable match collection to populate.</param>
        static void CollectMatchingEntities(Entity[] roots, string entityName, List<EditorEntity> matches) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (matches == null) {
                throw new ArgumentNullException(nameof(matches));
            } else if (roots == null) {
                return;
            }

            for (int index = 0; index < roots.Length; index++) {
                CollectMatchingEntities(roots[index], entityName, matches);
            }
        }

        /// <summary>
        /// Recursively collects one generated entity and its descendants when their stable names match the requested value.
        /// </summary>
        /// <param name="entity">Current entity being inspected.</param>
        /// <param name="entityName">Stable generated entity name to search for.</param>
        /// <param name="matches">Mutable match collection to populate.</param>
        static void CollectMatchingEntities(Entity entity, string entityName, List<EditorEntity> matches) {
            if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (matches == null) {
                throw new ArgumentNullException(nameof(matches));
            } else if (entity == null) {
                return;
            }

            if (entity is EditorEntity editorEntity && string.Equals(editorEntity.Name, entityName, StringComparison.Ordinal)) {
                matches.Add(editorEntity);
            }

            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (entity.Children[childIndex] is Entity childEntity) {
                    CollectMatchingEntities(childEntity, entityName, matches);
                }
            }
        }

        /// <summary>
        /// Builds the stable generated entity name for one menu item.
        /// </summary>
        /// <param name="itemId">Stable menu item id.</param>
        /// <returns>Stable generated entity name.</returns>
        static string BuildItemEntityName(string itemId) {
            if (string.IsNullOrWhiteSpace(itemId)) {
                throw new ArgumentException("Item id must be provided.", nameof(itemId));
            }

            return "Item-" + itemId;
        }

        /// <summary>
        /// Builds the stable generated entity name for one menu panel.
        /// </summary>
        /// <param name="panelId">Stable menu panel id.</param>
        /// <returns>Stable generated entity name.</returns>
        static string BuildPanelEntityName(string panelId) {
            if (string.IsNullOrWhiteSpace(panelId)) {
                throw new ArgumentException("Panel id must be provided.", nameof(panelId));
            }

            return "Panel-" + panelId;
        }
    }
}
