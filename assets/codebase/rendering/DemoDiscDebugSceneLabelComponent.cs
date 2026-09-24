namespace DemoDisc.rendering {
    /// <summary>
    /// Enables the authored scene-name overlay only for debug-environment builds.
    /// </summary>
    public sealed class DemoDiscDebugSceneLabelComponent : UpdateComponent {
        const byte DebugLabelRenderOrder = 7;
        Entity OwnerEntity;
        Entity LabelEntity;
        bool ResolvedAfterLoad;

        /// <summary>The authored label entity; older scenes without this reference resolve it once by render order.</summary>
        public SceneEntityReference LabelEntityReference { get; set; }

        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            OwnerEntity = entity ?? throw new ArgumentNullException(nameof(entity));
            ResolveAndSetVisibility();
        }

        public override void ComponentRemoved(Entity entity) {
            OwnerEntity = null;
            LabelEntity = null;
            ResolvedAfterLoad = false;
            base.ComponentRemoved(entity);
        }

        public override void Update() {
            // Scene children may not exist yet during ComponentAdded. Retry once after scene loading,
            // then keep the direct entity reference rather than walking the hierarchy every frame.
            if (LabelEntity != null || ResolvedAfterLoad) {
                return;
            }
            ResolvedAfterLoad = true;
            ResolveAndSetVisibility();
        }

        void ResolveAndSetVisibility() {
            if (OwnerEntity == null) {
                return;
            }

            uint labelId = LabelEntityReference == null ? 0u : LabelEntityReference.EntityId;
            LabelEntity = labelId == 0u ? null : FindLabelById(OwnerEntity, labelId);
            // Editor entities have save IDs but no runtime IDs; existing scenes have no authored reference.
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

        static Entity FindLabelById(Entity parent, uint labelId) {
            if (parent == null || parent.Children == null) {
                return null;
            }
            for (int index = 0; index < parent.Children.Count; index++) {
                Entity child = parent.Children[index];
                if (child == null) {
                    continue;
                }
                if (child.SceneEntityRuntimeId == labelId) {
                    return child;
                }
                Entity nestedMatch = FindLabelById(child, labelId);
                if (nestedMatch != null) {
                    return nestedMatch;
                }
            }
            return null;
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
