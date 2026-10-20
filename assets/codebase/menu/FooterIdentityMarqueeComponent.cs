namespace DemoDisc.menu {
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
        public float StripWidth {
            get { return StripWidthValue; }
            set {
                StripWidthValue = value;
                IsCanvasLayoutDirty = true;
            }
        }

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
        /// Tight runtime text width expressed in active viewport pixels, since <see cref="TextComponent.FontScale"/>
        /// is already fitted to the viewport by the owning <see cref="ReferenceCanvasFitComponent"/> before it is read here.
        /// </summary>
        float MeasuredTextWidth;

        /// <summary>
        /// Tracks whether the runtime footer text has been measured after its font resolved.
        /// </summary>
        bool HasMeasuredTextWidth;

        /// <summary>Authored restart coordinate whose setter invalidates the fitted cache.</summary>
        float StripWidthValue;

        /// <summary>Viewport found once after the scene hierarchy has been prepared.</summary>
        ViewportComponent LayoutViewport;

        /// <summary>Legacy reference canvas used when no viewport owns this footer.</summary>
        ReferenceCanvasFitComponent LayoutReferenceCanvas;

        /// <summary>Horizontal canvas scale retained until the owning layout changes.</summary>
        float CanvasScaleX;

        /// <summary>Fitted restart coordinate retained until the layout or strip width changes.</summary>
        float FittedStripStartX;

        /// <summary>Tracks layout changes without walking the footer ancestry from Update.</summary>
        bool IsCanvasLayoutDirty;

        /// <summary>
        /// Initializes one footer marquee with every runtime-resolved reference in a known state, because native builds do not zero-initialize C# instance fields automatically.
        /// </summary>
        public FooterIdentityMarqueeComponent() {
            TextEntity = null;
            FooterTextComponent = null;
            MeasuredTextWidth = 0f;
            HasMeasuredTextWidth = false;
            StripWidthValue = 0f;
            LayoutViewport = null;
            LayoutReferenceCanvas = null;
            CanvasScaleX = 0f;
            FittedStripStartX = 0f;
            IsCanvasLayoutDirty = true;
        }

        /// <summary>Binds the serialized text fixup and canvas owner once, after initial scene layout has completed.</summary>
        /// <param name="entity">Prepared entity owning this marquee.</param>
        public override void ScenePrepared(Entity entity) {
            base.ScenePrepared(entity);
            ReleaseRuntimeBindings();
            if (TextEntityReference == null || TextEntityReference.EntityId == 0u) {
                throw new InvalidOperationException("Footer marquee requires a serialized text entity reference.");
            }
            TextEntity = TextEntityReference.ResolvedEntity
                ?? throw new InvalidOperationException("Footer text reference was not resolved before preparation.");
            FooterTextComponent = FindRequiredTextComponent(TextEntity);
            BindCanvasOwner();
            FooterTextComponent.Text = BuildFooterText();
            PositionTextAtStripStart();
        }

        /// <summary>Detaches layout listeners and drops borrowed references when the footer leaves its owner.</summary>
        /// <param name="entity">Entity losing the marquee component.</param>
        public override void ComponentRemoved(Entity entity) {
            ReleaseRuntimeBindings();
            base.ComponentRemoved(entity);
        }

        /// <summary>Releases layout listeners before native component destruction.</summary>
        public override void Dispose() {
            ReleaseRuntimeBindings();
            base.Dispose();
        }

        /// <summary>
        /// Advances the footer text at a constant speed once its serialized scene reference is available at runtime.
        /// </summary>
        public override void Update() {
            base.Update();

            if (TextEntity == null) {
                throw new InvalidOperationException("Footer marquee must be prepared before its first update.");
            }
            RefreshCanvasLayoutCache();
            if (!EnsureMeasuredTextWidth()) {
                return;
            }

            float textWidth = MeasuredTextWidth;
            double movement = (double)PixelsPerSecond * CanvasScaleX * Core.Instance.FrameDeltaSeconds;
            float3 localPosition = TextEntity.LocalPosition;
            float nextPositionX = (float)(localPosition.X - (float)movement);
            if (nextPositionX + textWidth <= 0f) {
                PositionTextAtStripStart();
                return;
            }

            TextEntity.LocalPosition = new float3(nextPositionX, localPosition.Y, localPosition.Z);
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

            MeasuredTextWidth = (float)(FooterTextComponent.Font.MeasureTight(FooterTextComponent.Text).Width
                * FooterTextComponent.FontScale);
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
            RefreshCanvasLayoutCache();
            float3 localPosition = TextEntity.LocalPosition;
            TextEntity.LocalPosition = new float3(FittedStripStartX, localPosition.Y, localPosition.Z);
        }

        /// <summary>Finds and subscribes the nearest authored canvas owner during scene preparation.</summary>
        void BindCanvasOwner() {
            Entity currentEntity = Parent;
            while (currentEntity != null) {
                if (currentEntity.Components != null) {
                    for (int componentIndex = 0; componentIndex < currentEntity.Components.Count; componentIndex++) {
                        if (currentEntity.Components[componentIndex] is ViewportComponent viewport
                            && (viewport.ScalingMode == ViewportComponent.ReferenceCanvasScalingMode
                                || viewport.ScalingMode == ViewportComponent.ReferenceCanvasStretchScalingMode
                                || viewport.ScalingMode == ViewportComponent.DensityScalingMode)) {
                            LayoutViewport = viewport;
                            LayoutViewport.AnchorBoundsChanged += HandleCanvasBoundsChanged;
                            return;
                        }
                        if (currentEntity.Components[componentIndex] is ReferenceCanvasFitComponent referenceCanvas) {
                            LayoutReferenceCanvas = referenceCanvas;
                            LayoutReferenceCanvas.AnchorBoundsChanged += HandleCanvasBoundsChanged;
                            return;
                        }
                    }
                }
                currentEntity = currentEntity.Parent;
            }
            throw new InvalidOperationException("Footer marquee must be inside a reference-canvas-owned menu hierarchy.");
        }

        /// <summary>Invalidates fitted values and text measurement when the existing canvas owner reports a layout change.</summary>
        void HandleCanvasBoundsChanged() {
            IsCanvasLayoutDirty = true;
            HasMeasuredTextWidth = false;
        }

        /// <summary>Recomputes fitted values only after preparation, resize or an authored strip-width change.</summary>
        void RefreshCanvasLayoutCache() {
            if (!IsCanvasLayoutDirty) {
                return;
            }
            if (LayoutViewport != null) {
                CanvasScaleX = (float)LayoutViewport.PreparedLayoutMetrics.ScaleX;
                FittedStripStartX = StripWidthValue * CanvasScaleX;
            } else if (LayoutReferenceCanvas != null) {
                CanvasScaleX = LayoutReferenceCanvas.CalculateScale().X.ToSingle();
                FittedStripStartX = LayoutReferenceCanvas.CalculatePosition(new float3(StripWidthValue, 0f, 0f)).X.ToSingle();
            } else {
                throw new InvalidOperationException("Footer marquee requires a prepared canvas binding.");
            }
            IsCanvasLayoutDirty = false;
        }

        /// <summary>Unsubscribes callbacks and clears runtime aliases without disposing scene-owned components.</summary>
        void ReleaseRuntimeBindings() {
            if (LayoutViewport != null) {
                LayoutViewport.AnchorBoundsChanged -= HandleCanvasBoundsChanged;
            }
            if (LayoutReferenceCanvas != null) {
                LayoutReferenceCanvas.AnchorBoundsChanged -= HandleCanvasBoundsChanged;
            }
            LayoutViewport = null;
            LayoutReferenceCanvas = null;
            TextEntity = null;
            FooterTextComponent = null;
            HasMeasuredTextWidth = false;
            IsCanvasLayoutDirty = true;
        }
    }
}
