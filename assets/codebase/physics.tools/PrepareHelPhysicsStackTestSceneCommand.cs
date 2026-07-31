namespace city.physics.tools {
    /// <summary>
    /// Prepares a copied stacked-box scene for the isolated HelPhysics Windows proof without rewriting authored gameplay content.
    /// </summary>
    public sealed class PrepareHelPhysicsStackTestSceneCommand : IEditorCommand {
        /// <summary>
        /// Relative project path of the real authored stacked-box scene copied by the guarded build.
        /// </summary>
        const string StackSceneRelativePath = @"assets\scenes\physics\test_scene_dynamic_stack_boxes.helen";

        /// <summary>
        /// Stable automatic persistence id of the gameplay return component that is incompatible with a one-scene proof package.
        /// </summary>
        const string ReturnToMenuComponentTypeId = "city.menu.DemoDiscReturnToMenuComponent, gameplay";

        /// <summary>
        /// Gets the stable command identifier invoked by the Windows release prebuild configuration.
        /// </summary>
        public string CommandId => "physics.prepare-helphysics-stack-test-scene";

        /// <summary>
        /// Gets the human-readable command label surfaced by the editor command catalog.
        /// </summary>
        public string DisplayName => "Prepare HelPhysics Stack Test Scene";

        /// <summary>
        /// Removes exactly one return-to-menu component from the command context's copied scene and preserves every other serialized record.
        /// </summary>
        /// <param name="context">Editor-safe command context rooted at the copied build project.</param>
        public void Execute(IEditorCommandContext context) {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            string scenePath = Path.GetFullPath(Path.Combine(context.ProjectRootPath, StackSceneRelativePath));
            if (!File.Exists(scenePath)) {
                throw new FileNotFoundException("The HelPhysics stacked-box proof scene was not found in the copied project.", scenePath);
            }

            SceneAsset sceneAsset;
            using (FileStream inputStream = File.OpenRead(scenePath)) {
                Asset asset = global::helengine.editor.AssetSerializer.Deserialize(inputStream);
                if (asset is not SceneAsset deserializedSceneAsset) {
                    throw new InvalidOperationException($"The HelPhysics proof scene '{scenePath}' did not contain a scene asset.");
                }

                sceneAsset = deserializedSceneAsset;
            }

            int removedComponentCount = RemoveReturnComponents(sceneAsset.RootEntities ?? Array.Empty<SceneEntityAsset>());
            if (removedComponentCount != 1) {
                throw new InvalidOperationException($"The HelPhysics proof scene must contain exactly one '{ReturnToMenuComponentTypeId}' record, but found '{removedComponentCount}'.");
            }

            using FileStream outputStream = new FileStream(scenePath, FileMode.Create, FileAccess.Write, FileShare.None);
            global::helengine.editor.AssetSerializer.Serialize(outputStream, sceneAsset);
        }

        /// <summary>
        /// Removes matching return records recursively while preserving entity order and all non-matching component objects.
        /// </summary>
        /// <param name="entities">Serialized entity hierarchy to filter in place.</param>
        /// <returns>Total number of matching records removed from the hierarchy.</returns>
        static int RemoveReturnComponents(IReadOnlyList<SceneEntityAsset> entities) {
            if (entities == null) {
                throw new ArgumentNullException(nameof(entities));
            }

            int removedComponentCount = 0;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                SceneEntityAsset entity = entities[entityIndex]
                    ?? throw new InvalidOperationException("The HelPhysics proof scene contains a null entity record.");
                SceneComponentAssetRecord[] components = entity.Components ?? Array.Empty<SceneComponentAssetRecord>();
                List<SceneComponentAssetRecord> retainedComponents = new List<SceneComponentAssetRecord>(components.Length);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++) {
                    SceneComponentAssetRecord component = components[componentIndex]
                        ?? throw new InvalidOperationException($"Entity '{entity.Name}' contains a null component record.");
                    if (string.Equals(component.ComponentTypeId, ReturnToMenuComponentTypeId, StringComparison.Ordinal)) {
                        removedComponentCount++;
                    } else {
                        retainedComponents.Add(component);
                    }
                }

                entity.Components = retainedComponents.ToArray();
                removedComponentCount += RemoveReturnComponents(entity.Children ?? Array.Empty<SceneEntityAsset>());
            }

            return removedComponentCount;
        }
    }
}
