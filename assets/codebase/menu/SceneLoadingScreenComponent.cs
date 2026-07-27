namespace city.menu {
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
            ResolveRectanglesWhenNeeded();
            FitBackgroundToViewport();
            SceneManager sceneManager = Core.Instance.SceneManager;
            bool visible = sceneManager != null && sceneManager.IsSceneTransitionActive;
            float progress = visible ? sceneManager.SceneTransitionProgress : 0f;
            SetVisible(visible, progress);
        }

        /// <summary>
        /// Resolves the authored rectangles after the complete serialized scene hierarchy is available to the object manager.
        /// </summary>
        void ResolveRectanglesWhenNeeded() {
            if (Background != null && Track != null && Fill != null) {
                return;
            } else if (Core.Instance == null || Core.Instance.ObjectManager == null) {
                throw new InvalidOperationException("Loading screen rectangle resolution requires an initialized object manager.");
            }

            Background = FindRequiredRoundedRect(BackgroundEntityReference, "background");
            Track = FindRequiredRoundedRect(TrackEntityReference, "track");
            Fill = FindRequiredRoundedRect(FillEntityReference, "fill");
            SetVisible(false, 0f);
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
        /// Returns the rounded rectangle attached to one serialized entity reference.
        /// </summary>
        /// <param name="entityReference">Stable reference identifying the generated rectangle entity.</param>
        /// <param name="description">Human-readable rectangle role.</param>
        /// <returns>The required rounded rectangle component.</returns>
        RoundedRectComponent FindRequiredRoundedRect(SceneEntityReference entityReference, string description) {
            if (entityReference == null || entityReference.EntityId == 0u) {
                throw new InvalidOperationException($"The loading screen requires a serialized {description} entity reference.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity entity = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(entity) != entityReference.EntityId || entity.Components == null) {
                    continue;
                }

                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is RoundedRectComponent rectangle) {
                        return rectangle;
                    }
                }
            }

            throw new InvalidOperationException($"The loading screen {description} entity '{entityReference.EntityId}' must contain one RoundedRectComponent.");
        }

        /// <summary>
        /// Returns the stable authored scene id attached to one runtime entity.
        /// </summary>
        /// <param name="entity">Runtime entity whose persisted scene id should be inspected.</param>
        /// <returns>Stable authored scene id, or zero when no persisted id is attached.</returns>
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
