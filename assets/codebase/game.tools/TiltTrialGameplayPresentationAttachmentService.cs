using helengine.editor;
using city.rendering.tools;

namespace city.game.tools {
    /// <summary>
    /// Attaches platform-pruned Tilt Trial presentation Blueprint roots to existing authored gameplay scenes.
    /// </summary>
    public sealed class TiltTrialGameplayPresentationAttachmentService {
        /// <summary>
        /// Script type resolver supplied by the editor host for generated project components.
        /// </summary>
        readonly IScriptTypeResolver ScriptTypeResolverValue;

        /// <summary>
        /// Host-owned capability used to load, reference, and rewrite current native assets.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Platform ids that should receive only the handheld presentation root.
        /// </summary>
        static readonly string[] HandheldOnlyPlatformIds = ["windows", "ps2", "psp", "gamecube", "wii", "wiiu", "psvita", "switch"];

        /// <summary>
        /// Initializes one Tilt Trial presentation attachment service.
        /// </summary>
        /// <param name="scriptTypeResolver">Editor resolver for generated project component types.</param>
        /// <param name="assetAuthoringService">Host-owned capability for current native asset authoring.</param>
        public TiltTrialGameplayPresentationAttachmentService(
            IScriptTypeResolver scriptTypeResolver,
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            ScriptTypeResolverValue = scriptTypeResolver;
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Attaches both presentation roots to every authored Tilt Trial gameplay scene found in the project.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets folder.</param>
        public void AttachToAuthoredGameplayScenes(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            ApplyWindowsOnlyDebugStatusOverrideToConsoleBlueprint(fullProjectRootPath);
            foreach (global::city.game.TiltTrialLevelCatalogEntry levelEntry in global::city.game.TiltTrialLevelCatalog.CreateEntries()) {
                string scenePath = ResolveAuthoredScenePath(fullProjectRootPath, levelEntry.SceneId);
                SceneAsset sceneAsset = LoadScene(fullProjectRootPath, scenePath);
                RemoveCurrentPresentationRoots(sceneAsset);
                ApplyWindowsOnlyDebugRootOverride(sceneAsset);
                AddPresentationRoot(fullProjectRootPath, sceneAsset, "TiltTrialConsolePresentation", TiltTrialGameplayPresentationBlueprintGenerator.ConsoleBlueprintRelativePath, CreateConsolePresentationPlatformOverrides());
                AddPresentationRoot(fullProjectRootPath, sceneAsset, "TiltTrialHandheldPresentation", TiltTrialGameplayPresentationBlueprintGenerator.HandheldBlueprintRelativePath, CreateHandheldOnlyPlatformOverrides());
                SaveScene(fullProjectRootPath, scenePath, sceneAsset);
            }
        }

        /// <summary>
        /// Resolves the first existing authored path for a logical Tilt Trial level id.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root path.</param>
        /// <param name="sceneId">Logical Tilt Trial scene id.</param>
        /// <returns>Absolute authored scene path.</returns>
        string ResolveAuthoredScenePath(string projectRootPath, string sceneId) {
            string[] candidatePaths = [
                Path.Combine(projectRootPath, "assets", "scenes", "games", "tilt", sceneId + ".helen"),
                Path.Combine(projectRootPath, "assets", "scenes", "games", sceneId + ".helen")
            ];
            for (int index = 0; index < candidatePaths.Length; index++) {
                if (File.Exists(candidatePaths[index])) {
                    return candidatePaths[index];
                }
            }

            throw new FileNotFoundException($"Authored Tilt Trial gameplay scene '{sceneId}' was not found.", candidatePaths[0]);
        }

        /// <summary>
        /// Loads one serialized authored scene asset.
        /// </summary>
        /// <param name="scenePath">Absolute scene asset path.</param>
        /// <returns>Loaded scene asset.</returns>
        SceneAsset LoadScene(string projectRootPath, string scenePath) {
            string relativePath = Path.GetRelativePath(
                Path.Combine(projectRootPath, "assets"),
                scenePath).Replace('\\', '/');
            return AssetAuthoringService.LoadNativeAsset<SceneAsset>(relativePath);
        }

        /// <summary>
        /// Removes the current generated presentation roots so rerunning the authoring command is idempotent.
        /// </summary>
        /// <param name="sceneAsset">Scene being updated.</param>
        void RemoveCurrentPresentationRoots(SceneAsset sceneAsset) {
            List<SceneEntityAsset> retainedRoots = new List<SceneEntityAsset>();
            SceneEntityAsset[] roots = sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>();
            for (int index = 0; index < roots.Length; index++) {
                SceneEntityAsset root = roots[index];
                if (root == null
                    || string.Equals(root.Name, "TiltTrialConsolePresentation", StringComparison.Ordinal)
                    || string.Equals(root.Name, "TiltTrialHandheldPresentation", StringComparison.Ordinal)) {
                    continue;
                }

                retainedRoots.Add(root);
            }

            sceneAsset.RootEntities = retainedRoots.ToArray();
        }

        /// <summary>
        /// Marks the Windows-only physics bounds debug root as absent from handheld scene cooks.
        /// </summary>
        /// <param name="sceneAsset">Authored Tilt Trial scene receiving the platform restriction.</param>
        void ApplyWindowsOnlyDebugRootOverride(SceneAsset sceneAsset) {
            SceneEntityAsset[] roots = sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>();
            for (int index = 0; index < roots.Length; index++) {
                SceneEntityAsset root = roots[index];
                if (root == null || !string.Equals(root.Name, "TiltTrialPhysicsBoundsDebug", StringComparison.Ordinal)) {
                    continue;
                }

                root.PlatformExistenceOverrides = CreateWindowsOnlyDebugPlatformOverrides();
                return;
            }

            throw new InvalidOperationException("Tilt Trial scene is missing the Windows-only physics bounds debug root.");
        }

        /// <summary>
        /// Marks the F3 status row inside the console presentation Blueprint as absent from non-Windows and Windows Release cooks.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root that owns the presentation Blueprint.</param>
        void ApplyWindowsOnlyDebugStatusOverrideToConsoleBlueprint(string projectRootPath) {
            string blueprintPath = TiltTrialGameplayPresentationBlueprintGenerator.ConsoleBlueprintRelativePath;
            BlueprintAsset blueprintAsset = LoadBlueprintAsset(blueprintPath);
            SceneEntityAsset statusText = FindEntityByName(blueprintAsset.RootEntity, "TiltTrialPhysicsBoundsStatusText");
            if (statusText == null) {
                throw new InvalidOperationException("Tilt Trial console presentation Blueprint is missing the Windows-only physics bounds status row.");
            }

            statusText.PlatformExistenceOverrides = CreateWindowsOnlyDebugStatusOverrides();
            SaveBlueprintAsset(blueprintPath, blueprintAsset);
        }

        /// <summary>
        /// Adds one Blueprint instance root with its target platform existence overrides.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root that owns the referenced Blueprint asset.</param>
        /// <param name="sceneAsset">Scene receiving the Blueprint root.</param>
        /// <param name="name">Instance root name.</param>
        /// <param name="blueprintPath">Project-relative Blueprint path.</param>
        /// <param name="existenceOverrides">Platform existence rules for the instance root.</param>
        void AddPresentationRoot(string projectRootPath, SceneAsset sceneAsset, string name, string blueprintPath, SceneEntityPlatformExistenceOverrideAsset[] existenceOverrides) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            } else if (sceneAsset == null) {
                throw new ArgumentNullException(nameof(sceneAsset));
            } else if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Blueprint instance name must be provided.", nameof(name));
            } else if (string.IsNullOrWhiteSpace(blueprintPath)) {
                throw new ArgumentException("Blueprint path must be provided.", nameof(blueprintPath));
            }

            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create(ScriptTypeResolverValue);
            BlueprintInstanceComponent blueprintInstance = new BlueprintInstanceComponent {
                BlueprintAssetReference = AssetAuthoringService.CreateFileReference(
                    blueprintPath,
                    AssetEntryKind.Blueprint)
            };
            BlueprintAsset blueprintAsset = LoadBlueprintAsset(blueprintPath);
            BlueprintEntityReferenceOverrideService overrideService = new BlueprintEntityReferenceOverrideService(registry);
            overrideService.BindAllEntityReferences(blueprintInstance, blueprintAsset, FindRequiredPlayerEntity(sceneAsset).Id);
            SceneEntityAsset instanceRoot = new SceneEntityAsset {
                Id = FindNextEntityId(sceneAsset),
                Name = name,
                Enabled = true,
                LayerMask = EditorLayerMasks.SceneObjects,
                LocalPosition = float3.Zero,
                LocalScale = float3.One,
                LocalOrientation = float4.Identity,
                Components = [registry.GetDescriptor(blueprintInstance).SerializeComponent(blueprintInstance, 0, new EntityComponentSaveState())],
                PlatformExistenceOverrides = existenceOverrides,
                Children = Array.Empty<SceneEntityAsset>()
            };

            List<SceneEntityAsset> roots = new List<SceneEntityAsset>(sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>()) {
                instanceRoot
            };
            sceneAsset.RootEntities = roots.ToArray();
        }

        /// <summary>
        /// Loads one project-relative presentation Blueprint for authoring-time override discovery.
        /// </summary>
        /// <param name="projectRootPath">Absolute project root that owns the Blueprint asset.</param>
        /// <param name="blueprintPath">Project-relative Blueprint asset path.</param>
        /// <returns>Loaded presentation Blueprint.</returns>
        BlueprintAsset LoadBlueprintAsset(string blueprintPath) {
            return AssetAuthoringService.LoadNativeAsset<BlueprintAsset>(blueprintPath);
        }

        /// <summary>
        /// Saves one project-relative presentation Blueprint after applying authoring-time existence overrides.
        /// </summary>
        /// <param name="blueprintPath">Project-relative Blueprint asset path.</param>
        /// <param name="blueprintAsset">Blueprint asset to serialize.</param>
        void SaveBlueprintAsset(string blueprintPath, BlueprintAsset blueprintAsset) {
            blueprintAsset.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(blueprintPath);
            blueprintAsset.FormerAuthoringAssetIds = Array.Empty<string>();
            Transaction.WriteAsset(blueprintPath, blueprintAsset);
        }

        /// <summary>
        /// Finds the authored PlayerSphere entity required by the presentation bindings.
        /// </summary>
        /// <param name="sceneAsset">Scene containing the target entity.</param>
        /// <returns>Authored PlayerSphere entity.</returns>
        static SceneEntityAsset FindRequiredPlayerEntity(SceneAsset sceneAsset) {
            SceneEntityAsset targetEntity = FindEntityByName(sceneAsset.RootEntities, "PlayerSphere");
            if (targetEntity == null || targetEntity.Id == 0u) {
                throw new InvalidOperationException($"Tilt Trial scene '{sceneAsset.Id}' must contain a non-zero PlayerSphere entity id before presentation bindings can be authored.");
            }

            return targetEntity;
        }

        /// <summary>
        /// Finds one serialized scene entity by its authored name.
        /// </summary>
        /// <param name="entities">Serialized entity roots to search.</param>
        /// <param name="name">Authored entity name to locate.</param>
        /// <returns>Matching entity, or null when no entity has the requested name.</returns>
        static SceneEntityAsset FindEntityByName(SceneEntityAsset[] entities, string name) {
            SceneEntityAsset[] roots = entities ?? Array.Empty<SceneEntityAsset>();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++) {
                SceneEntityAsset match = FindEntityByName(roots[rootIndex], name);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds one serialized scene entity by its authored name within one hierarchy.
        /// </summary>
        /// <param name="entity">Current serialized entity to inspect.</param>
        /// <param name="name">Authored entity name to locate.</param>
        /// <returns>Matching entity, or null when the subtree does not contain the requested name.</returns>
        static SceneEntityAsset FindEntityByName(SceneEntityAsset entity, string name) {
            if (entity == null) {
                return null;
            } else if (string.Equals(entity.Name, name, StringComparison.Ordinal)) {
                return entity;
            }

            SceneEntityAsset[] children = entity.Children ?? Array.Empty<SceneEntityAsset>();
            for (int childIndex = 0; childIndex < children.Length; childIndex++) {
                SceneEntityAsset match = FindEntityByName(children[childIndex], name);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates platform exclusions that leave the handheld presentation only on DS and 3DS.
        /// </summary>
        /// <returns>Handheld-only platform existence overrides.</returns>
        static SceneEntityPlatformExistenceOverrideAsset[] CreateHandheldOnlyPlatformOverrides() {
            SceneEntityPlatformExistenceOverrideAsset[] overrides = new SceneEntityPlatformExistenceOverrideAsset[HandheldOnlyPlatformIds.Length];
            for (int index = 0; index < HandheldOnlyPlatformIds.Length; index++) {
                overrides[index] = new SceneEntityPlatformExistenceOverrideAsset {
                    PlatformId = HandheldOnlyPlatformIds[index],
                    Exists = false
                };
            }

            return overrides;
        }

        /// <summary>
        /// Creates platform exclusions that leave the console presentation absent from Nintendo handheld platforms while preserving it on Windows Release.
        /// </summary>
        /// <returns>Console presentation platform existence overrides.</returns>
        static SceneEntityPlatformExistenceOverrideAsset[] CreateConsolePresentationPlatformOverrides() {
            return [
                new SceneEntityPlatformExistenceOverrideAsset { PlatformId = "ds", Exists = false },
                new SceneEntityPlatformExistenceOverrideAsset { PlatformId = "3ds", Exists = false }
            ];
        }

        /// <summary>
        /// Creates platform and environment exclusions that leave the debug-only entity absent from Nintendo handheld and Windows Release platforms.
        /// </summary>
        /// <returns>Windows-only debug platform and environment existence overrides.</returns>
        static SceneEntityPlatformExistenceOverrideAsset[] CreateWindowsOnlyDebugPlatformOverrides() {
            return [
                new SceneEntityPlatformExistenceOverrideAsset { PlatformId = "ds", Exists = false },
                new SceneEntityPlatformExistenceOverrideAsset { PlatformId = "3ds", Exists = false },
                new SceneEntityPlatformExistenceOverrideAsset { PlatformId = "windows", EnvironmentId = "release", Exists = false }
            ];
        }

        /// <summary>
        /// Creates the full platform and environment exclusions for the separately authored F3 status row.
        /// </summary>
        /// <returns>F3 status-row platform and environment existence overrides.</returns>
        static SceneEntityPlatformExistenceOverrideAsset[] CreateWindowsOnlyDebugStatusOverrides() {
            string[] nonWindowsPlatformIds = ["ps2", "psp", "psvita", "gamecube", "wii", "wiiu", "switch", "ds", "3ds"];
            SceneEntityPlatformExistenceOverrideAsset[] overrides = new SceneEntityPlatformExistenceOverrideAsset[nonWindowsPlatformIds.Length + 1];
            for (int platformIndex = 0; platformIndex < nonWindowsPlatformIds.Length; platformIndex++) {
                overrides[platformIndex] = new SceneEntityPlatformExistenceOverrideAsset {
                    PlatformId = nonWindowsPlatformIds[platformIndex],
                    Exists = false
                };
            }

            overrides[^1] = new SceneEntityPlatformExistenceOverrideAsset {
                PlatformId = "windows",
                EnvironmentId = "release",
                Exists = false
            };
            return overrides;
        }

        /// <summary>
        /// Finds an unused entity id larger than every id in the current scene hierarchy.
        /// </summary>
        /// <param name="sceneAsset">Scene whose entity ids should be scanned.</param>
        /// <returns>Next available positive entity id.</returns>
        uint FindNextEntityId(SceneAsset sceneAsset) {
            uint maximumId = 0u;
            SceneEntityAsset[] roots = sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>();
            for (int index = 0; index < roots.Length; index++) {
                maximumId = Math.Max(maximumId, FindMaximumEntityId(roots[index]));
            }

            return maximumId == uint.MaxValue ? throw new InvalidOperationException("Tilt Trial scene entity ids are exhausted.") : maximumId + 1u;
        }

        /// <summary>
        /// Finds the largest entity id in one serialized hierarchy.
        /// </summary>
        /// <param name="entity">Current entity hierarchy root.</param>
        /// <returns>Largest id in the hierarchy.</returns>
        static uint FindMaximumEntityId(SceneEntityAsset entity) {
            if (entity == null) {
                return 0u;
            }

            uint maximumId = entity.Id;
            SceneEntityAsset[] children = entity.Children ?? Array.Empty<SceneEntityAsset>();
            for (int index = 0; index < children.Length; index++) {
                maximumId = Math.Max(maximumId, FindMaximumEntityId(children[index]));
            }

            return maximumId;
        }

        /// <summary>
        /// Serializes the modified scene back to its original authored path.
        /// </summary>
        /// <param name="scenePath">Absolute authored scene path.</param>
        /// <param name="sceneAsset">Scene asset to write.</param>
        void SaveScene(string projectRootPath, string scenePath, SceneAsset sceneAsset) {
            string relativePath = Path.GetRelativePath(Path.Combine(projectRootPath, "assets"), scenePath).Replace('\\', '/');
            sceneAsset.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetSceneIdentity(relativePath);
            sceneAsset.FormerAuthoringAssetIds = Array.Empty<string>();
            Transaction.WriteAsset(relativePath, sceneAsset);
        }
    }
}
