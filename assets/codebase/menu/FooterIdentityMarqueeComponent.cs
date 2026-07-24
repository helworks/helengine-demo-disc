namespace city.menu {
    /// <summary>
    /// Moves the serialized footer identity text left across the menu strip and restarts it beyond the right screen edge.
    /// </summary>
    public sealed class FooterIdentityMarqueeComponent : UpdateComponent {
        /// <summary>
        /// Serialized scene reference identifying the footer text entity that this component moves.
        /// </summary>
        public SceneEntityReference TextEntityReference { get; set; }

        /// <summary>
        /// Full reference-canvas width where the text restarts after leaving the left edge.
        /// </summary>
        public float StripWidth { get; set; }

        /// <summary>
        /// Authored text-box width used to determine when the complete line has left the screen.
        /// </summary>
        public float TextWidth { get; set; }

        /// <summary>
        /// Constant marquee speed expressed in reference-canvas pixels per second.
        /// </summary>
        public float PixelsPerSecond { get; set; }

        /// <summary>
        /// Authored viewport width used as the one-times speed baseline.
        /// </summary>
        public float ReferenceViewportWidth { get; set; }

        /// <summary>
        /// Runtime text entity resolved from <see cref="TextEntityReference"/> after scene loading completes.
        /// </summary>
        Entity TextEntity;

        /// <summary>
        /// Text component on the resolved footer entity whose content includes the runtime platform metadata.
        /// </summary>
        TextComponent FooterTextComponent;

        /// <summary>
        /// Advances the footer text at a constant speed once its serialized scene reference is available at runtime.
        /// </summary>
        public override void Update() {
            base.Update();

            ResolveTextEntityWhenNeeded();
            if (TextEntity == null) {
                return;
            }

            double viewportScale = ResolveViewportScale();
            double movement = (double)PixelsPerSecond * viewportScale * Core.Instance.FrameDeltaSeconds;
            float3 localPosition = TextEntity.LocalPosition;
            float nextPositionX = localPosition.X - (float)movement;
            if (nextPositionX + TextWidth <= 0f) {
                nextPositionX = StripWidth;
            }

            TextEntity.LocalPosition = new float3(nextPositionX, localPosition.Y, localPosition.Z);
        }

        /// <summary>
        /// Resolves the text entity once the asynchronous scene loader has created its runtime entity id component.
        /// </summary>
        void ResolveTextEntityWhenNeeded() {
            if (TextEntity != null) {
                return;
            } else if (TextEntityReference == null || TextEntityReference.EntityId == 0u) {
                throw new InvalidOperationException("Footer marquee requires a serialized text entity reference.");
            } else if (Core.Instance == null || Core.Instance.ObjectManager == null) {
                throw new InvalidOperationException("Footer marquee requires an initialized object manager.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidateEntity = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(candidateEntity) != TextEntityReference.EntityId) {
                    continue;
                }

                TextEntity = candidateEntity;
                TextEntity.LocalPosition = new float3(StripWidth, TextEntity.LocalPosition.Y, TextEntity.LocalPosition.Z);
                FooterTextComponent = FindRequiredTextComponent(TextEntity);
                FooterTextComponent.Text = BuildFooterText();
                return;
            }
        }

        /// <summary>
        /// Builds the footer signature and its current platform/version suffix.
        /// </summary>
        /// <returns>Fully formatted marquee text.</returns>
        string BuildFooterText() {
            if (Core.Instance.PlatformInfo == null) {
                throw new InvalidOperationException("Footer marquee requires initialized runtime platform metadata.");
            }

            return string.Concat(
                "MADE BY HELENA / HELEN OF CODE / ",
                Core.Instance.PlatformInfo.Name,
                " ",
                Core.Instance.PlatformInfo.Version);
        }

        /// <summary>
        /// Finds the text component attached to the resolved footer text entity.
        /// </summary>
        /// <param name="entity">Resolved footer text entity.</param>
        /// <returns>Attached text component.</returns>
        TextComponent FindRequiredTextComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException("Footer marquee text entity must contain initialized components.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TextComponent textComponent) {
                    return textComponent;
                }
            }

            throw new InvalidOperationException("Footer marquee text entity must contain one TextComponent.");
        }

        /// <summary>
        /// Resolves the current viewport-width multiplier from the nearest viewport owner in the entity hierarchy.
        /// </summary>
        /// <returns>Multiplier that keeps marquee movement proportional to the active viewport width.</returns>
        double ResolveViewportScale() {
            if (ReferenceViewportWidth <= 0f) {
                throw new InvalidOperationException("Footer marquee requires a positive reference viewport width.");
            }

            ViewportComponent viewportComponent = FindRequiredViewportComponent();
            int2 resolvedViewportSize = viewportComponent.ResolvedViewportSize;
            return (double)resolvedViewportSize.X / ReferenceViewportWidth;
        }

        /// <summary>
        /// Finds the nearest viewport component that owns the footer's reference-canvas subtree.
        /// </summary>
        /// <returns>Viewport component that resolves the active screen width.</returns>
        ViewportComponent FindRequiredViewportComponent() {
            Entity currentEntity = Parent;
            while (currentEntity != null) {
                if (currentEntity.Components != null) {
                    for (int componentIndex = 0; componentIndex < currentEntity.Components.Count; componentIndex++) {
                        if (currentEntity.Components[componentIndex] is ViewportComponent viewportComponent) {
                            return viewportComponent;
                        }
                    }
                }

                currentEntity = currentEntity.Parent;
            }

            throw new InvalidOperationException("Footer marquee must be inside a viewport-owned menu hierarchy.");
        }

        /// <summary>
        /// Finds the stable serialized scene id attached to an entity, or returns zero before scene loading attaches one.
        /// </summary>
        /// <param name="entity">Runtime entity whose stable scene id should be inspected.</param>
        /// <returns>Serialized scene entity id, or zero when unavailable.</returns>
        uint FindSceneEntityRuntimeIdOrZero(Entity entity) {
            if (entity == null || entity.Components == null) {
                return 0u;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is SceneEntityRuntimeIdComponent runtimeIdComponent) {
                    return runtimeIdComponent.SceneEntityId;
                }
            }

            return 0u;
        }
    }
}
