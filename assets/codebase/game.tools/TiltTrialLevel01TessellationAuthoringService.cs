using System.Globalization;

namespace city.game.tools {
    /// <summary>
    /// Updates only the scalable course MeshComponents in the existing authored Tilt Trial Level 01 scene with cook-time tessellation settings.
    /// </summary>
    public sealed class TiltTrialLevel01TessellationAuthoringService {
        /// <summary>
        /// Relative authored scene path for the playable first Tilt Trial level.
        /// </summary>
        const string Level01SceneRelativePath = "assets/scenes/games/tilt/tilt_trial_level_01.helen";

        /// <summary>
        /// Stable serialized type id for MeshComponent records.
        /// </summary>
        const string MeshComponentTypeId = "helengine.MeshComponent";

        /// <summary>
        /// Stable target platform identifier for PlayStation 2 cooking.
        /// </summary>
        const string Ps2PlatformId = "ps2";

        /// <summary>
        /// Stable target platform identifier for PlayStation Portable cooking.
        /// </summary>
        const string PspPlatformId = "psp";

        /// <summary>
        /// Stable detached PSP MeshComponent member that marks one cooked render variant as already scale-baked.
        /// </summary>
        const string MeshBakeScaleMemberName = "MeshBakeScale";

        /// <summary>
        /// World-space maximum edge length used for the constrained-platform course tessellation variants.
        /// </summary>
        const double TessellationMaxEdgeLength = 0.5d;

        /// <summary>
        /// Names of the only scalable playable Level 01 course entities that receive tessellation metadata.
        /// </summary>
        static readonly string[] TessellatedEntityNames = ["StartPad", "Ramp", "Bridge", "FinalPlatform", "LeftWall", "RightWall", "BridgeBlockerLeft", "BridgeBlockerRight"];

        /// <summary>
        /// Reads and rewrites editor-only component platform override payloads.
        /// </summary>
        readonly ComponentPlatformOverridePayloadService OverridePayloadService = new ComponentPlatformOverridePayloadService();

        /// <summary>
        /// Reads and writes the current detached MeshComponent modifier stack.
        /// </summary>
        readonly MeshComponentModifierStackService ModifierStackService = new MeshComponentModifierStackService();

        /// <summary>
        /// Applies PS2- and PSP-only tessellation metadata to the authored playable Level 01 scene without replacing its gameplay content.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the authored Level 01 scene asset.</param>
        public void ApplyToAuthoredLevel01Scene(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string scenePath = Path.Combine(Path.GetFullPath(projectRootPath), Level01SceneRelativePath);
            SceneAsset sceneAsset = LoadScene(scenePath);
            for (int index = 0; index < TessellatedEntityNames.Length; index++) {
                ApplyTessellationToRequiredEntity(sceneAsset, TessellatedEntityNames[index]);
            }

            SaveScene(projectRootPath, scenePath, sceneAsset);
        }

        /// <summary>
        /// Applies the configured detached platform settings to one named course entity.
        /// </summary>
        /// <param name="sceneAsset">Scene that owns the required course entity.</param>
        /// <param name="entityName">Stable authored entity name.</param>
        void ApplyTessellationToRequiredEntity(SceneAsset sceneAsset, string entityName) {
            if (sceneAsset == null) {
                throw new ArgumentNullException(nameof(sceneAsset));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            SceneEntityAsset entity = FindEntityByName(sceneAsset.RootEntities, entityName);
            if (entity == null) {
                throw new InvalidOperationException($"Tilt Trial Level 01 is missing required course entity '{entityName}'.");
            }

            int meshComponentIndex = FindRequiredMeshComponentIndex(entity, entityName);
            SceneComponentAssetRecord componentRecord = entity.Components[meshComponentIndex];
            EntityComponentSaveState saveState = CreateSaveStateWithExistingPlatformOverrides(componentRecord);
            MeshComponentModifier ps2Modifier = new MeshComponentModifier(MeshComponentModifier.TessellateKind) {
                MaxEdgeLength = TessellationMaxEdgeLength
            };
            MeshComponentModifier pspModifier = new MeshComponentModifier(MeshComponentModifier.TessellateKind) {
                MaxEdgeLength = TessellationMaxEdgeLength
            };
            ModifierStackService.SetStack(saveState, Ps2PlatformId, new[] { ps2Modifier });
            ModifierStackService.SetStack(saveState, PspPlatformId, new[] { pspModifier });
            EntityComponentPlatformOverrideState pspOverride = saveState.GetOrCreatePlatformOverride(PspPlatformId);
            pspOverride.SetMemberValue(MeshBakeScaleMemberName, true.ToString(CultureInfo.InvariantCulture));
            SceneComponentAssetRecord baseRecord = OverridePayloadService.UnwrapBaseRecord(componentRecord);
            entity.Components[meshComponentIndex] = OverridePayloadService.Wrap(baseRecord, saveState);
        }

        /// <summary>
        /// Reconstructs a save state from the detached platform override metadata currently stored in one component record.
        /// </summary>
        /// <param name="componentRecord">Persisted component record whose overrides should be retained.</param>
        /// <returns>Save state containing all existing component platform overrides.</returns>
        EntityComponentSaveState CreateSaveStateWithExistingPlatformOverrides(SceneComponentAssetRecord componentRecord) {
            if (componentRecord == null) {
                throw new ArgumentNullException(nameof(componentRecord));
            }

            EntityComponentSaveState saveState = new EntityComponentSaveState();
            IReadOnlyList<EntityComponentPlatformOverrideState> overrides = OverridePayloadService.ReadOverrideStates(componentRecord);
            for (int index = 0; index < overrides.Count; index++) {
                EntityComponentPlatformOverrideState overrideState = overrides[index];
                if (overrideState != null) {
                    saveState.SetPlatformOverride(overrideState.PlatformId, overrideState);
                }
            }

            return saveState;
        }

        /// <summary>
        /// Finds the MeshComponent record index on one required course entity.
        /// </summary>
        /// <param name="entity">Course entity that owns the MeshComponent.</param>
        /// <param name="entityName">Stable entity name used for failure diagnostics.</param>
        /// <returns>Index of the MeshComponent record inside the entity component array.</returns>
        int FindRequiredMeshComponentIndex(SceneEntityAsset entity, string entityName) {
            SceneComponentAssetRecord[] components = entity.Components ?? Array.Empty<SceneComponentAssetRecord>();
            for (int index = 0; index < components.Length; index++) {
                SceneComponentAssetRecord component = components[index];
                if (component != null && string.Equals(component.ComponentTypeId, MeshComponentTypeId, StringComparison.Ordinal)) {
                    return index;
                }
            }

            throw new InvalidOperationException($"Tilt Trial Level 01 course entity '{entityName}' is missing a MeshComponent.");
        }

        /// <summary>
        /// Finds one named entity within every root hierarchy in the supplied scene entity array.
        /// </summary>
        /// <param name="entities">Scene entity roots to search.</param>
        /// <param name="entityName">Stable entity name to locate.</param>
        /// <returns>Matching entity when one exists; otherwise null.</returns>
        SceneEntityAsset FindEntityByName(SceneEntityAsset[] entities, string entityName) {
            SceneEntityAsset[] roots = entities ?? Array.Empty<SceneEntityAsset>();
            for (int index = 0; index < roots.Length; index++) {
                SceneEntityAsset entity = FindEntityByName(roots[index], entityName);
                if (entity != null) {
                    return entity;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds one named entity inside a single entity hierarchy.
        /// </summary>
        /// <param name="entity">Current hierarchy node to inspect.</param>
        /// <param name="entityName">Stable entity name to locate.</param>
        /// <returns>Matching entity when one exists; otherwise null.</returns>
        SceneEntityAsset FindEntityByName(SceneEntityAsset entity, string entityName) {
            if (entity == null) {
                return null;
            } else if (string.Equals(entity.Name, entityName, StringComparison.Ordinal)) {
                return entity;
            }

            SceneEntityAsset[] children = entity.Children ?? Array.Empty<SceneEntityAsset>();
            for (int index = 0; index < children.Length; index++) {
                SceneEntityAsset match = FindEntityByName(children[index], entityName);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads one serialized authored Level 01 scene asset.
        /// </summary>
        /// <param name="scenePath">Absolute scene file path.</param>
        /// <returns>Deserialized scene asset.</returns>
        SceneAsset LoadScene(string scenePath) {
            if (!File.Exists(scenePath)) {
                throw new FileNotFoundException("Tilt Trial Level 01 scene was not found.", scenePath);
            }

            using FileStream stream = File.OpenRead(scenePath);
            Asset asset = helengine.editor.AssetSerializer.Deserialize(stream);
            if (asset is SceneAsset sceneAsset) {
                return sceneAsset;
            }

            throw new InvalidOperationException("Tilt Trial Level 01 file did not contain a SceneAsset.");
        }

        /// <summary>
        /// Writes one modified Level 01 scene asset back to its original authored file path.
        /// </summary>
        /// <param name="scenePath">Absolute scene file path.</param>
        /// <param name="sceneAsset">Modified scene asset to serialize.</param>
        void SaveScene(string projectRootPath, string scenePath, SceneAsset sceneAsset) {
            if (sceneAsset == null) {
                throw new ArgumentNullException(nameof(sceneAsset));
            }

            new helengine.editor.GeneratedAssetWriteService().WriteAsset(projectRootPath, Level01SceneRelativePath.Substring("assets/".Length), sceneAsset);
        }
    }
}
