namespace DemoDisc.menu {
    /// <summary>
    /// Presents the persistent scene-transition overlay and maps engine loading progress onto its bottom progress bar.
    /// </summary>
    public sealed class SceneLoadingScreenComponent : UpdateComponent {
        /// <summary>
        /// Width of the authored progress track in reference-canvas pixels.
        /// </summary>
        public int ProgressTrackWidth { get; set; }

        /// <summary>
        /// Stable serialized reference to the full-screen background rectangle entity.
        /// </summary>
        public SceneEntityReference BackgroundEntityReference { get; set; }

        /// <summary>
        /// Stable serialized reference to the bottom progress-track rectangle entity.
        /// </summary>
        public SceneEntityReference TrackEntityReference { get; set; }

        /// <summary>
        /// Stable serialized reference to the bottom progress-fill rectangle entity.
        /// </summary>
        public SceneEntityReference FillEntityReference { get; set; }

        /// <summary>
        /// Opaque overlay rectangle resolved from the first generated child.
        /// </summary>
        RoundedRectComponent Background;

        /// <summary>
        /// Last viewport size applied to the camera-owned loading blackout rectangle.
        /// </summary>
        int2 BackgroundViewportSize;

        /// <summary>
        /// Progress-track rectangle resolved from the second generated child.
        /// </summary>
        RoundedRectComponent Track;

        /// <summary>
        /// Progress-fill rectangle resolved from the third generated child.
        /// </summary>
        RoundedRectComponent Fill;

        /// <summary>
        /// Initializes the component before the generated child hierarchy is materialized.
        /// </summary>
        /// <param name="entity">Persistent loading-scene root that owns the generated rectangles.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
        }

        /// <summary>
        /// Updates overlay visibility and the fill width from the active engine scene transition.
        /// </summary>
        public override void Update() {
            base.Update();

            FitBackgroundToViewport();
            SceneManager sceneManager = Core.Instance.SceneManager;
            bool visible = sceneManager != null && sceneManager.IsSceneTransitionActive;
            float progress = visible ? sceneManager.SceneTransitionProgress : 0f;
            SetVisible(visible, progress);
        }

        /// <summary>
        /// Binds the authored loading-screen rectangles before the first frame update.
        /// </summary>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            Background = FindRequiredRoundedRect(BackgroundEntityReference?.ResolvedEntity, "background");
            Track = FindRequiredRoundedRect(TrackEntityReference?.ResolvedEntity, "track");
            Fill = FindRequiredRoundedRect(FillEntityReference?.ResolvedEntity, "fill");
        }
        /// <summary>
        /// Applies presentation alpha and a clamped fill width for one transition state.
        /// </summary>
        /// <param name="visible">Whether the loading overlay should obscure the active scene.</param>
        /// <param name="progress">Normalized engine loading progress.</param>
        void SetVisible(bool visible, float progress) {
            byte alpha = visible ? byte.MaxValue : (byte)0;
            Background.FillColor = new byte4(0, 0, 0, alpha);
            Background.BorderColor = new byte4(0, 0, 0, alpha);
            Track.FillColor = new byte4(40, 26, 56, alpha);
            Track.BorderColor = new byte4(135, 94, 163, alpha);
            Fill.FillColor = new byte4(135, 94, 163, alpha);
            Fill.BorderColor = new byte4(135, 94, 163, alpha);
            float clampedProgress = Math.Clamp(progress, 0f, 1f);
            float2 canvasScale = ResolveCanvasScale();
            int fittedFillWidth = (int)Math.Round(ProgressTrackWidth * clampedProgress * canvasScale.X);
            Fill.Size = new int2(fittedFillWidth, Fill.Size.Y);
        }

        /// <summary>
        /// Resizes the camera-owned blackout rectangle to the live viewport without affecting the fitted loading-bar canvas.
        /// </summary>
        void FitBackgroundToViewport() {
            if (Background == null || Core.Instance == null || Core.Instance.RenderManager3D == null) {
                throw new InvalidOperationException("Loading background fitting requires initialized background and render manager instances.");
            }

            int2 viewportSize = Core.Instance.RenderManager3D.MainWindowSize;
            if (viewportSize.X < 1 || viewportSize.Y < 1) {
                throw new InvalidOperationException("Loading background fitting requires a non-empty live viewport.");
            } else if (BackgroundViewportSize.X == viewportSize.X && BackgroundViewportSize.Y == viewportSize.Y) {
                return;
            }

            Background.Size = Core.Instance.RenderManager3D.MainWindowSize;
            BackgroundViewportSize = viewportSize;
        }

        /// <summary>
        /// Resolves the current fit factors from the reference canvas attached to the persistent loading-screen root.
        /// </summary>
        /// <returns>Scale factors that convert authored loading-bar measurements into active viewport coordinates.</returns>
        float2 ResolveCanvasScale() {
            if (Parent == null || Parent.Components == null) {
                throw new InvalidOperationException("The loading screen requires an initialized root entity.");
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is ReferenceCanvasFitComponent referenceCanvasFitComponent) {
                    return referenceCanvasFitComponent.CalculateScale();
                }
            }

            throw new InvalidOperationException("The loading screen root must contain one ReferenceCanvasFitComponent.");
        }

        /// <summary>
        /// Returns the required rounded rectangle on one already-resolved authored entity.
        /// </summary>
        /// <param name="entity">Resolved entity that should own the rectangle.</param>
        /// <param name="description">Human-readable rectangle role.</param>
        /// <returns>The required rounded rectangle component.</returns>
        RoundedRectComponent FindRequiredRoundedRect(Entity entity, string description) {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException($"The loading screen requires a resolved {description} entity reference.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is RoundedRectComponent rectangle) {
                    return rectangle;
                }
            }

            throw new InvalidOperationException($"The loading screen {description} entity must contain one RoundedRectComponent.");
        }    }
}
