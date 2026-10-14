namespace DemoDisc.rendering {
    /// <summary>
    /// Enables the authored scene-name overlay only for debug-environment builds.
    /// </summary>
    public sealed class DemoDiscDebugSceneLabelComponent : UpdateComponent {
        Entity OwnerEntity;
        Entity LabelEntity;

        /// <summary>The authored label entity, identified independently of its render depth.</summary>
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
            if (LabelEntity == null) {
                return;
            }

#if HELENGINE_ENV_DEBUG
            LabelEntity.Enabled = true;
#else
            LabelEntity.Enabled = false;
#endif
        }

    }
}
