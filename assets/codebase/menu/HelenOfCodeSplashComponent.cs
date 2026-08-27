namespace city.menu {
    /// <summary>
    /// Loads the standard main menu behind the initial Helen of Code splash and fades the splash away after a short hold.
    /// </summary>
    public sealed class HelenOfCodeSplashComponent : UpdateComponent {
        /// <summary>
        /// Stable scene id used by the generated splash scene and its deferred self-unload request.
        /// </summary>
        public const string SplashSceneId = "HelenOfCodeSplash";

        /// <summary>
        /// Stable scene id resolved by the additive startup load.
        /// </summary>
        public const string MainMenuSceneId = "DemoDiscMainMenu";

        /// <summary>
        /// Stable scene id of the persistent overlay loaded alongside the main menu.
        /// </summary>
        public const string LoadingScreenSceneId = "SceneLoadingScreen";

        /// <summary>
        /// Duration, in seconds, used to fade the splash from transparent to opaque.
        /// </summary>
        public const double FadeDurationSeconds = 0.75d;

        /// <summary>
        /// Hold duration, in seconds, that keeps the splash visible between its two 0.75-second fades.
        /// The complete splash presentation therefore lasts five seconds.
        /// </summary>
        public const double HoldDurationSeconds = 3.5d;

        /// <summary>
        /// Maximum elapsed time that one update may contribute to the splash animation.
        /// Disc-backed scene loads can block the main thread for several seconds; those
        /// I/O stalls must not consume the authored splash display interval.
        /// </summary>
        public const double MaximumAnimationFrameDeltaSeconds = 0.1d;

        /// <summary>
        /// Stable serialized scene reference identifying the full-screen black background entity.
        /// </summary>
        public SceneEntityReference BackgroundSpriteEntityReference { get; set; }

        /// <summary>
        /// Stable serialized scene reference identifying the centered logo entity.
        /// </summary>
        public SceneEntityReference LogoSpriteEntityReference { get; set; }

        /// <summary>
        /// Full-screen solid background whose alpha is driven by the splash phase.
        /// </summary>
        RoundedRectComponent BackgroundRectangle;

        /// <summary>
        /// Last viewport size applied to the camera-owned blackout rectangle.
        /// </summary>
        int2 BackgroundViewportSize;

        /// <summary>
        /// Logo sprite whose alpha is driven by the splash phase.
        /// </summary>
        SpriteComponent LogoSprite;

        /// <summary>
        /// Elapsed splash time in seconds.
        /// </summary>
        double ElapsedSeconds;

        /// <summary>
        /// Tracks whether the additive main-menu request has already been queued.
        /// </summary>
        bool MainMenuLoadWasRequested;

        /// <summary>
        /// Tracks whether this splash scene has already requested its own unload.
        /// </summary>
        bool SplashUnloadWasRequested;

        /// <summary>
        /// Tracks whether the first input snapshot after the splash became active has been observed and must no longer be ignored.
        /// </summary>
        bool HasCompletedFirstUpdate;

        /// <summary>
        /// Binds the generated splash sprites and queues the additive main-menu load.
        /// </summary>
        /// <param name="entity">Generated splash root entity that owns the component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            StartupInputGate.Acquire();

            if (Core.Instance != null && Core.Instance.SceneManager != null) {
                RequestMainMenuLoad();
            }
        }

        /// <summary>
        /// Returns menu input ownership when the splash component is removed after its update pass.
        /// </summary>
        public override void Dispose() {
            StartupInputGate.Release();
            base.Dispose();
        }

        /// <summary>
        /// Advances the splash animation and queues scene cleanup after the fade-out completes.
        /// </summary>
        public override void Update() {
            base.Update();

            RequestMainMenuLoad();
            if (HasCompletedFirstUpdate && IsAcceptPressed()) {
                RequestSplashUnload();
                return;
            }

            HasCompletedFirstUpdate = true;

            ResolveSpritesWhenNeeded();
            FitBackgroundToViewport();
            ElapsedSeconds += ResolveAnimationFrameDeltaSeconds(Core.Instance.FrameDeltaSeconds);
            int alpha = ResolveAlphaForElapsedSeconds(ElapsedSeconds);
            SetSpriteAlpha(alpha);

            double totalDurationSeconds = (FadeDurationSeconds * 2d) + HoldDurationSeconds;
            if (ElapsedSeconds >= totalDurationSeconds) {
                RequestSplashUnload();
            }
        }

        /// <summary>
        /// Bounds one frame's contribution to splash time so synchronous asset loading cannot skip the presentation.
        /// </summary>
        /// <param name="frameDeltaSeconds">Raw elapsed time reported by the platform update loop.</param>
        /// <returns>The non-negative frame time limited to the splash animation maximum.</returns>
        public double ResolveAnimationFrameDeltaSeconds(double frameDeltaSeconds) {
            if (double.IsNaN(frameDeltaSeconds) || double.IsInfinity(frameDeltaSeconds) || frameDeltaSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(frameDeltaSeconds), "Splash frame time must be finite and non-negative.");
            }

            return Math.Min(frameDeltaSeconds, MaximumAnimationFrameDeltaSeconds);
        }

        /// <summary>
        /// Determines whether the user pressed one of the desktop menu accept inputs this frame.
        /// Console startup input is intentionally excluded because the first controller snapshot can
        /// contain a transient button edge while the platform backend is synchronizing hardware state.
        /// </summary>
        /// <returns>True when keyboard, platform, or primary gamepad accept input was pressed.</returns>
        bool IsAcceptPressed() {
#if !DESKTOP_PLATFORM
            return false;
#else
            InputSystem inputSystem = Core.Instance.Input;
            if (inputSystem.WasKeyPressed(Keys.Enter) || inputSystem.WasKeyPressed(Keys.Space) || inputSystem.WasKeyPressed(Keys.J)) {
                return true;
            }
            return Core.Instance.StandardPlatformInput.WasActionPressed(StandardPlatformAction.Accept)
                || DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South);
#endif
        }

        /// <summary>
        /// Queues the splash scene for unload while retaining the persistent loading overlay for later scene transitions.
        /// </summary>
        void RequestSplashUnload() {
            if (SplashUnloadWasRequested) {
                return;
            }

            SplashUnloadWasRequested = true;
            Core.Instance.SceneManager.UnloadScene(SplashSceneId);
        }

        /// <summary>
        /// Resolves the visible alpha for one elapsed splash time.
        /// </summary>
        /// <param name="elapsedSeconds">Elapsed splash time in seconds.</param>
        /// <returns>Alpha channel in the inclusive byte range 0 through 255.</returns>
        public int ResolveAlphaForElapsedSeconds(double elapsedSeconds) {
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds)) {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Elapsed splash time must be finite.");
            } else if (elapsedSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Elapsed splash time cannot be negative.");
            }

            if (elapsedSeconds < FadeDurationSeconds) {
                return ResolveFadeAlpha(elapsedSeconds / FadeDurationSeconds);
            }

            double fadeOutStartSeconds = FadeDurationSeconds + HoldDurationSeconds;
            if (elapsedSeconds < fadeOutStartSeconds) {
                return 255;
            }

            double fadeOutProgress = (elapsedSeconds - fadeOutStartSeconds) / FadeDurationSeconds;
            return ResolveFadeAlpha(1d - fadeOutProgress);
        }

        /// <summary>
        /// Applies one alpha value to the opaque black background and the logo sprite.
        /// </summary>
        /// <param name="alpha">Alpha channel in the inclusive byte range 0 through 255.</param>
        void SetSpriteAlpha(int alpha) {
            if (BackgroundRectangle == null || LogoSprite == null) {
                throw new InvalidOperationException("Splash background and logo must be bound before their alpha can be updated.");
            }

            double fadeOutStartSeconds = FadeDurationSeconds + HoldDurationSeconds;
            byte backgroundAlpha = ElapsedSeconds >= fadeOutStartSeconds ? (byte)alpha : (byte)255;
            BackgroundRectangle.FillColor = new byte4(0, 0, 0, backgroundAlpha);
            BackgroundRectangle.BorderColor = new byte4(0, 0, 0, backgroundAlpha);
            LogoSprite.Color = new byte4(255, 255, 255, (byte)alpha);
        }

        /// <summary>
        /// Resizes the camera-owned blackout rectangle to the live viewport without changing the fitted splash-content canvas.
        /// </summary>
        void FitBackgroundToViewport() {
            if (BackgroundRectangle == null || Core.Instance == null || Core.Instance.RenderManager3D == null) {
                throw new InvalidOperationException("Splash background fitting requires initialized background and render manager instances.");
            }

            int2 viewportSize = Core.Instance.RenderManager3D.MainWindowSize;
            if (viewportSize.X < 1 || viewportSize.Y < 1) {
                throw new InvalidOperationException("Splash background fitting requires a non-empty live viewport.");
            } else if (BackgroundViewportSize.X == viewportSize.X && BackgroundViewportSize.Y == viewportSize.Y) {
                return;
            }

            BackgroundRectangle.Size = Core.Instance.RenderManager3D.MainWindowSize;
            BackgroundViewportSize = viewportSize;
        }

        /// <summary>
        /// Resolves both serialized splash sprite entity references once the scene hierarchy is available.
        /// </summary>
        void ResolveSpritesWhenNeeded() {
            if (BackgroundRectangle != null && LogoSprite != null) {
                return;
            } else if (Core.Instance == null || Core.Instance.ObjectManager == null) {
                throw new InvalidOperationException("Helen of Code splash sprite resolution requires an initialized object manager.");
            }

            BackgroundRectangle = ResolveBackgroundRectangle(BackgroundSpriteEntityReference);
            LogoSprite = FindRequiredSprite(LogoSpriteEntityReference, "logo");
            SetSpriteAlpha(0);
        }

        /// <summary>
        /// Resolves the full-screen solid rectangle used to hide the menu while the splash is visible.
        /// </summary>
        /// <param name="entityReference">Stable reference identifying the authored background entity.</param>
        /// <returns>The required background rectangle component.</returns>
        RoundedRectComponent ResolveBackgroundRectangle(SceneEntityReference entityReference) {
            return FindRequiredBackgroundRectangle(entityReference);
        }

        /// <summary>
        /// Resolves one serialized scene entity reference to its required sprite component.
        /// </summary>
        /// <param name="entityReference">Stable reference identifying the sprite entity.</param>
        /// <param name="description">Human-readable role of the required sprite.</param>
        /// <returns>The sprite component attached to the referenced entity.</returns>
        SpriteComponent FindRequiredSprite(SceneEntityReference entityReference, string description) {
            if (entityReference == null || entityReference.EntityId == 0u) {
                throw new InvalidOperationException($"Helen of Code splash requires a serialized {description} sprite entity reference.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidateEntity = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(candidateEntity) != entityReference.EntityId) {
                    continue;
                }

                return FindRequiredSpriteOnEntity(candidateEntity, description);
            }

            throw new InvalidOperationException($"Helen of Code splash could not resolve the serialized {description} sprite entity reference '{entityReference.EntityId}'.");
        }

        /// <summary>
        /// Resolves the rounded rectangle on the entity identified by the persisted background reference.
        /// </summary>
        /// <param name="entityReference">Stable reference identifying the background entity.</param>
        /// <returns>The background rectangle component.</returns>
        RoundedRectComponent FindRequiredBackgroundRectangle(SceneEntityReference entityReference) {
            if (entityReference == null || entityReference.EntityId == 0u) {
                throw new InvalidOperationException("Helen of Code splash requires a serialized background entity reference.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidateEntity = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(candidateEntity) != entityReference.EntityId) {
                    continue;
                }

                if (candidateEntity.Components == null) {
                    break;
                }

                for (int componentIndex = 0; componentIndex < candidateEntity.Components.Count; componentIndex++) {
                    if (candidateEntity.Components[componentIndex] is RoundedRectComponent backgroundRectangle) {
                        return backgroundRectangle;
                    }
                }

                break;
            }

            throw new InvalidOperationException($"Helen of Code splash background entity '{entityReference.EntityId}' must contain one RoundedRectComponent.");
        }

        /// <summary>
        /// Finds the sprite component attached to one resolved splash entity.
        /// </summary>
        /// <param name="entity">Resolved splash sprite entity.</param>
        /// <param name="description">Human-readable role of the required sprite.</param>
        /// <returns>The required sprite component.</returns>
        SpriteComponent FindRequiredSpriteOnEntity(Entity entity, string description) {
            if (entity == null || entity.Components == null) {
                throw new InvalidOperationException($"Helen of Code splash {description} sprite entity must contain initialized components.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is SpriteComponent spriteComponent) {
                    return spriteComponent;
                }
            }

            throw new InvalidOperationException($"Helen of Code splash {description} sprite entity must contain one SpriteComponent.");
        }

        /// <summary>
        /// Finds the stable scene id attached to one runtime entity.
        /// </summary>
        /// <param name="entity">Entity whose scene id should be inspected.</param>
        /// <returns>The authored scene entity id, or zero when unavailable.</returns>
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

        /// <summary>
        /// Queues the standard main-menu scene exactly once while preserving the splash scene.
        /// </summary>
        void RequestMainMenuLoad() {
            if (MainMenuLoadWasRequested) {
                return;
            } else if (Core.Instance == null || Core.Instance.SceneManager == null) {
                throw new InvalidOperationException("Helen of Code splash startup requires an initialized scene manager.");
            }

            MainMenuLoadWasRequested = true;
            Core.Instance.SceneManager.LoadScene(LoadingScreenSceneId, SceneLoadMode.Additive);
            Core.Instance.SceneManager.LoadScene(MainMenuSceneId, SceneLoadMode.Additive);
        }

        /// <summary>
        /// Converts a normalized fade progress value into a clamped alpha byte.
        /// </summary>
        /// <param name="normalizedProgress">Fade progress where zero is transparent and one is opaque.</param>
        /// <returns>Alpha channel in the inclusive byte range 0 through 255.</returns>
        int ResolveFadeAlpha(double normalizedProgress) {
            double clampedProgress = Math.Max(0d, Math.Min(1d, normalizedProgress));
            return (int)Math.Round(clampedProgress * 255d);
        }
    }
}
