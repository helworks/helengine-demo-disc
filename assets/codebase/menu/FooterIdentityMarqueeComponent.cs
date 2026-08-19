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
        /// Constant marquee speed expressed in reference-canvas pixels per second.
        /// </summary>
        public float PixelsPerSecond { get; set; }

        /// <summary>
        /// Runtime text entity resolved from <see cref="TextEntityReference"/> after scene loading completes.
        /// </summary>
        Entity TextEntity;

        /// <summary>
        /// Text component on the resolved footer entity whose content includes the runtime platform metadata.
        /// </summary>
        TextComponent FooterTextComponent;

        /// <summary>
        /// Tight runtime text width expressed in authored reference-canvas pixels.
        /// </summary>
        float MeasuredTextWidth;

        /// <summary>
        /// Tracks whether the runtime footer text has been measured after its font resolved.
        /// </summary>
        bool HasMeasuredTextWidth;

        /// <summary>
        /// Initializes one footer marquee with every runtime-resolved reference in a known state, because native builds do not zero-initialize C# instance fields automatically.
        /// </summary>
        public FooterIdentityMarqueeComponent() {
            TextEntity = null;
            FooterTextComponent = null;
            MeasuredTextWidth = 0f;
            HasMeasuredTextWidth = false;
        }

        /// <summary>
        /// Advances the footer text at a constant speed once its serialized scene reference is available at runtime.
        /// </summary>
        public override void Update() {
            base.Update();

            ResolveTextEntityWhenNeeded();
            if (TextEntity == null || !EnsureMeasuredTextWidth()) {
                return;
            }

            float2 canvasScale = ResolveCanvasScale();
            float textWidth = MeasuredTextWidth * canvasScale.X;
            double movement = (double)PixelsPerSecond * canvasScale.X * Core.Instance.FrameDeltaSeconds;
            float3 localPosition = TextEntity.LocalPosition;
            float nextPositionX = localPosition.X - (float)movement;
            if (nextPositionX + textWidth <= 0f) {
                PositionTextAtStripStart();
                return;
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
                FooterTextComponent = FindRequiredTextComponent(TextEntity);
                PositionTextAtStripStart();
                FooterTextComponent.Text = BuildFooterText();
                return;
            }
        }

        /// <summary>
        /// Measures the runtime footer line once its font asset has resolved.
        /// </summary>
        /// <returns>True after the complete runtime text width is available.</returns>
        bool EnsureMeasuredTextWidth() {
            if (HasMeasuredTextWidth) {
                return true;
            } else if (FooterTextComponent == null || FooterTextComponent.Font == null) {
                return false;
            }

            MeasuredTextWidth = FooterTextComponent.Font.MeasureTight(FooterTextComponent.Text).Width
                * FooterTextComponent.FontScale;
            HasMeasuredTextWidth = true;
            return true;
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
        /// Places the text at the authored strip start after converting that position into the active canvas coordinates.
        /// </summary>
        void PositionTextAtStripStart() {
            float3 localPosition = TextEntity.LocalPosition;
            TextEntity.LocalPosition = new float3(ResolveFittedStripStartX(), localPosition.Y, localPosition.Z);
        }

        /// <summary>
        /// Resolves the current horizontal and vertical conversion factors from the footer's owning reference canvas.
        /// </summary>
        /// <returns>Scale factors that convert the footer's authored measurements into active canvas coordinates.</returns>
        float2 ResolveCanvasScale() {
            Entity currentEntity = Parent;
            while (currentEntity != null) {
                if (currentEntity.Components != null) {
                    for (int componentIndex = 0; componentIndex < currentEntity.Components.Count; componentIndex++) {
                        if (currentEntity.Components[componentIndex] is ReferenceCanvasFitComponent referenceCanvas) {
                            return referenceCanvas.CalculateScale();
                        }
                    }
                }

                currentEntity = currentEntity.Parent;
            }

            throw new InvalidOperationException("Footer marquee must be inside a reference-canvas-owned menu hierarchy.");
        }

        /// <summary>
        /// Resolves the strip's authored restart coordinate through the footer's owning reference canvas.
        /// </summary>
        /// <returns>Fitted horizontal coordinate at the right edge of the authored strip.</returns>
        float ResolveFittedStripStartX() {
            Entity currentEntity = Parent;
            while (currentEntity != null) {
                if (currentEntity.Components != null) {
                    for (int componentIndex = 0; componentIndex < currentEntity.Components.Count; componentIndex++) {
                        if (currentEntity.Components[componentIndex] is ReferenceCanvasFitComponent referenceCanvas) {
                            return referenceCanvas.CalculatePosition(new float3(StripWidth, 0f, 0f)).X;
                        }
                    }
                }

                currentEntity = currentEntity.Parent;
            }

            throw new InvalidOperationException("Footer marquee must be inside a reference-canvas-owned menu hierarchy.");
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
