namespace city.game {
    /// <summary>
    /// Mirrors the Windows-only Tilt Trial physics-bounds debug visibility into one gameplay HUD text row.
    /// </summary>
    public sealed class TiltTrialPhysicsBoundsStatusTextComponent : UpdateComponent {
        static readonly byte4 DisabledColor = new byte4(196, 210, 226, 255);
        static readonly byte4 EnabledColor = new byte4(132, 255, 196, 255);

        Entity ownerEntity;
        TextComponent statusTextComponent;
        TiltTrialPhysicsBoundsDebugDrawComponent boundsDebugComponent;

        /// <summary>
        /// Captures the owning text entity and clears cached runtime bindings.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            ownerEntity = entity ?? throw new ArgumentNullException(nameof(entity));
            statusTextComponent = null;
            boundsDebugComponent = null;
        }

        /// <summary>
        /// Clears runtime-only state when the presenter detaches.
        /// </summary>
        /// <param name="entity">Detaching entity.</param>
        public override void ComponentRemoved(Entity entity) {
            statusTextComponent = null;
            boundsDebugComponent = null;
            ownerEntity = null;
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Keeps the HUD text synchronized with the current bounds-debug toggle state.
        /// </summary>
        public override void Update() {
            if (statusTextComponent == null) {
                statusTextComponent = FindOwnedTextComponent();
                if (statusTextComponent == null) {
                    return;
                }
            }

            if (boundsDebugComponent == null) {
                boundsDebugComponent = FindBoundsDebugComponent();
                if (boundsDebugComponent == null) {
                    return;
                }
            }

            bool visible = boundsDebugComponent.Visible;
            string nextText = visible ? "F3 Bounds On" : "F3 Bounds Off";
            byte4 nextColor = visible ? EnabledColor : DisabledColor;

            if (!string.Equals(statusTextComponent.Text, nextText, StringComparison.Ordinal)) {
                statusTextComponent.Text = nextText;
            }

            if (statusTextComponent.Color.X != nextColor.X
                || statusTextComponent.Color.Y != nextColor.Y
                || statusTextComponent.Color.Z != nextColor.Z
                || statusTextComponent.Color.W != nextColor.W) {
                statusTextComponent.Color = nextColor;
            }
        }

        TextComponent FindOwnedTextComponent() {
            if (ownerEntity == null || ownerEntity.Components == null) {
                return null;
            }

            for (int componentIndex = 0; componentIndex < ownerEntity.Components.Count; componentIndex++) {
                if (ownerEntity.Components[componentIndex] is TextComponent textComponent) {
                    return textComponent;
                }
            }

            return null;
        }

        TiltTrialPhysicsBoundsDebugDrawComponent FindBoundsDebugComponent() {
            if (Core.Instance == null || Core.Instance.SceneManager == null) {
                return null;
            }

            IReadOnlyList<LoadedSceneRecord> loadedScenes = Core.Instance.SceneManager.LoadedScenes;
            for (int sceneIndex = 0; sceneIndex < loadedScenes.Count; sceneIndex++) {
                TiltTrialPhysicsBoundsDebugDrawComponent component = FindBoundsDebugComponent(loadedScenes[sceneIndex].RootEntities);
                if (component != null) {
                    return component;
                }
            }

            return null;
        }

        TiltTrialPhysicsBoundsDebugDrawComponent FindBoundsDebugComponent(IReadOnlyList<Entity> rootEntities) {
            if (rootEntities == null) {
                return null;
            }

            for (int entityIndex = 0; entityIndex < rootEntities.Count; entityIndex++) {
                TiltTrialPhysicsBoundsDebugDrawComponent component = FindBoundsDebugComponent(rootEntities[entityIndex]);
                if (component != null) {
                    return component;
                }
            }

            return null;
        }

        TiltTrialPhysicsBoundsDebugDrawComponent FindBoundsDebugComponent(Entity entity) {
            if (entity == null) {
                return null;
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is TiltTrialPhysicsBoundsDebugDrawComponent component) {
                        return component;
                    }
                }
            }

            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                TiltTrialPhysicsBoundsDebugDrawComponent component = FindBoundsDebugComponent(entity.Children[childIndex]);
                if (component != null) {
                    return component;
                }
            }

            return null;
        }
    }
}
