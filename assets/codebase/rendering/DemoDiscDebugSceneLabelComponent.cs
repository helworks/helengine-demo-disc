namespace DemoDisc.rendering {
    /// <summary>
    /// Enables the authored scene-name overlay only for debug-environment builds.
    /// </summary>
    public sealed class DemoDiscDebugSceneLabelComponent : UpdateComponent {
        const byte DebugLabelRenderOrder = 7;
        Entity OwnerEntity;
        Entity LabelEntity;

        /// <summary>The authored label entity; older scenes resolve once by render order until regenerated.</summary>
        public SceneEntityReference LabelEntityReference { get; set; }

        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            OwnerEntity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            OwnerEntity = entity ?? throw new ArgumentNullException(nameof(entity));
            ResolveAndSetVisibility();
        }

        public override void ComponentRemoved(Entity entity) {
            OwnerEntity = null;
            LabelEntity = null;
            base.ComponentRemoved(entity);
        }

        void ResolveAndSetVisibility() {
            if (OwnerEntity == null) {
                return;
            }

            LabelEntity = LabelEntityReference?.ResolvedEntity;
            // Older authored scenes do not carry this link yet; Task 7 scene regeneration removes this compatibility search.
            if (LabelEntity == null) {
                LabelEntity = FindLabelByRenderOrder(OwnerEntity);
            }
            if (LabelEntity == null) {
                return;
            }

#if HELENGINE_ENV_DEBUG
            LabelEntity.Enabled = true;
#else
            LabelEntity.Enabled = false;
#endif
        }

        static Entity FindLabelByRenderOrder(Entity parent) {
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
                Entity nestedMatch = FindLabelByRenderOrder(child);
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