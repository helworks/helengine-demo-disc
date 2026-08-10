namespace city.rendering {
    /// <summary>
    /// Enables the authored scene-name overlay only for debug-environment builds.
    /// </summary>
    public sealed class DemoDiscDebugSceneLabelComponent : UpdateComponent {
        const byte DebugLabelRenderOrder = 7;
        Entity OwnerEntity;

        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            OwnerEntity = entity ?? throw new ArgumentNullException(nameof(entity));
            SetOverlayVisibility();
        }

        public override void ComponentRemoved(Entity entity) {
            OwnerEntity = null;
            base.ComponentRemoved(entity);
        }

        public override void Update() {
            SetOverlayVisibility();
        }

        void SetOverlayVisibility() {
            Entity overlayEntity = FindLabel(OwnerEntity);
            if (overlayEntity == null) {
                return;
            }

#if HELENGINE_ENV_DEBUG
            overlayEntity.Enabled = true;
#else
            overlayEntity.Enabled = false;
#endif
        }

        static Entity FindLabel(Entity parent) {
            if (parent == null || parent.Children == null) {
                return null;
            }

            for (int index = 0; index < parent.Children.Count; index++) {
                Entity child = parent.Children[index];
                if (child == null) {
                    continue;
                }
                if (ContainsDebugLabelText(child)) {
                    return child;
                }

                Entity nestedMatch = FindLabel(child);
                if (nestedMatch != null) {
                    return nestedMatch;
                }
            }

            return null;
        }

        static bool ContainsDebugLabelText(Entity entity) {
            if (entity.Components == null) {
                return false;
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is TextComponent textComponent
                    && textComponent.RenderOrder2D == DebugLabelRenderOrder) {
                    return true;
                }
            }

            return false;
        }

    }
}
