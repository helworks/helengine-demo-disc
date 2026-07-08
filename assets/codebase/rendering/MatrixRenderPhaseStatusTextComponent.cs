namespace city.rendering {
    /// <summary>
    /// Updates the Matrix Render scene status label so the active transform operation remains visible in the bottom-left overlay.
    /// </summary>
    public sealed class MatrixRenderPhaseStatusTextComponent : UpdateComponent {
        /// <summary>
        /// Stable prefix displayed ahead of the current phase label.
        /// </summary>
        const string OperationPrefix = "Operation: ";

        /// <summary>
        /// Cached owning entity that hosts the runtime text component.
        /// </summary>
        Entity OwnerEntity;

        /// <summary>
        /// Cached runtime text component updated by this presenter.
        /// </summary>
        TextComponent StatusTextComponent;

        /// <summary>
        /// Cached Matrix Render controller resolved from the loaded scene roots.
        /// </summary>
        MatrixRenderComponent MatrixRenderController;

        /// <summary>
        /// Binds the owning entity and clears any runtime-only cached references.
        /// </summary>
        /// <param name="entity">Owning entity that hosts the status text.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            OwnerEntity = entity;
            StatusTextComponent = null;
            MatrixRenderController = null;
        }

        /// <summary>
        /// Clears cached runtime references after the overlay label leaves the scene.
        /// </summary>
        /// <param name="entity">Owning entity that is removing this component.</param>
        public override void ComponentRemoved(Entity entity) {
            StatusTextComponent = null;
            MatrixRenderController = null;
            OwnerEntity = null;
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Updates the visible overlay label to match the current Matrix Render phase.
        /// </summary>
        public override void Update() {
            if (StatusTextComponent == null) {
                StatusTextComponent = FindOwnedTextComponent();
                if (StatusTextComponent == null) {
                    return;
                }
            }

            if (MatrixRenderController == null) {
                MatrixRenderController = FindMatrixRenderController();
                if (MatrixRenderController == null) {
                    return;
                }
            }

            string nextText = OperationPrefix + MatrixRenderController.GetCurrentOperationLabel();
            if (string.Equals(StatusTextComponent.Text, nextText, StringComparison.Ordinal)) {
                return;
            }

            StatusTextComponent.Text = nextText;
        }

        /// <summary>
        /// Finds the runtime text component hosted by the owning label entity.
        /// </summary>
        /// <returns>Owned runtime text component.</returns>
        TextComponent FindOwnedTextComponent() {
            if (OwnerEntity == null || OwnerEntity.Components == null) {
                return null;
            }

            for (int componentIndex = 0; componentIndex < OwnerEntity.Components.Count; componentIndex++) {
                if (OwnerEntity.Components[componentIndex] is TextComponent textComponent) {
                    return textComponent;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the active Matrix Render controller across the currently loaded runtime scenes.
        /// </summary>
        /// <returns>Active Matrix Render controller.</returns>
        MatrixRenderComponent FindMatrixRenderController() {
            if (Core.Instance == null || Core.Instance.SceneManager == null) {
                return null;
            }

            IReadOnlyList<LoadedSceneRecord> loadedScenes = Core.Instance.SceneManager.LoadedScenes;
            for (int sceneIndex = 0; sceneIndex < loadedScenes.Count; sceneIndex++) {
                LoadedSceneRecord loadedScene = loadedScenes[sceneIndex];
                MatrixRenderComponent component = FindMatrixRenderController(loadedScene.RootEntities);
                if (component != null) {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches one loaded-scene root set for the Matrix Render controller.
        /// </summary>
        /// <param name="rootEntities">Loaded root entities to inspect.</param>
        /// <returns>Active Matrix Render controller when one is present.</returns>
        MatrixRenderComponent FindMatrixRenderController(IReadOnlyList<Entity> rootEntities) {
            if (rootEntities == null) {
                return null;
            }

            for (int entityIndex = 0; entityIndex < rootEntities.Count; entityIndex++) {
                MatrixRenderComponent component = FindMatrixRenderController(rootEntities[entityIndex]);
                if (component != null) {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches one runtime entity subtree for the Matrix Render controller.
        /// </summary>
        /// <param name="entity">Runtime entity subtree to inspect.</param>
        /// <returns>Active Matrix Render controller when one is present.</returns>
        MatrixRenderComponent FindMatrixRenderController(Entity entity) {
            if (entity == null) {
                return null;
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is MatrixRenderComponent matrixRenderComponent) {
                        return matrixRenderComponent;
                    }
                }
            }

            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                MatrixRenderComponent component = FindMatrixRenderController(entity.Children[childIndex]);
                if (component != null) {
                    return component;
                }
            }

            return null;
        }
    }
}
