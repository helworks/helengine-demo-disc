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
        /// Duration, in seconds, used to fade the splash from transparent to opaque.
        /// </summary>
        public const double FadeDurationSeconds = 0.75d;

        /// <summary>
        /// Duration, in seconds, that the splash remains fully opaque before fading out.
        /// </summary>
        public const double HoldDurationSeconds = 3d;

        /// <summary>
        /// Zero-based generated child index used by the full-screen black background sprite.
        /// </summary>
        public const int BackgroundChildIndex = 0;

        /// <summary>
        /// Zero-based generated child index used by the centered logo sprite.
        /// </summary>
        public const int LogoChildIndex = 1;

        /// <summary>
        /// Background sprite whose alpha is driven by the splash phase.
        /// </summary>
        SpriteComponent BackgroundSprite;

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
        /// Binds the generated splash sprites and queues the additive main-menu load.
        /// </summary>
        /// <param name="entity">Generated splash root entity that owns the component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);

            BackgroundSprite = FindRequiredSprite(entity, BackgroundChildIndex);
            LogoSprite = FindRequiredSprite(entity, LogoChildIndex);
            SetSpriteAlpha(0);
            if (Core.Instance != null && Core.Instance.SceneManager != null) {
                RequestMainMenuLoad();
            }
        }

        /// <summary>
        /// Advances the splash animation and queues scene cleanup after the fade-out completes.
        /// </summary>
        public override void Update() {
            base.Update();

            RequestMainMenuLoad();
            ElapsedSeconds += Core.Instance.FrameDeltaSeconds;
            int alpha = ResolveAlphaForElapsedSeconds(ElapsedSeconds);
            SetSpriteAlpha(alpha);

            double totalDurationSeconds = (FadeDurationSeconds * 2d) + HoldDurationSeconds;
            if (ElapsedSeconds >= totalDurationSeconds && !SplashUnloadWasRequested) {
                SplashUnloadWasRequested = true;
                Core.Instance.SceneManager.UnloadScene(SplashSceneId);
            }
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
            if (BackgroundSprite == null || LogoSprite == null) {
                throw new InvalidOperationException("Splash sprites must be bound before their alpha can be updated.");
            }

            double fadeOutStartSeconds = FadeDurationSeconds + HoldDurationSeconds;
            byte backgroundAlpha = ElapsedSeconds >= fadeOutStartSeconds ? (byte)alpha : (byte)255;
            BackgroundSprite.Color = new byte4(0, 0, 0, backgroundAlpha);
            LogoSprite.Color = new byte4(255, 255, 255, (byte)alpha);
        }

        /// <summary>
        /// Resolves one cardinal sprite by generated child index from the splash root.
        /// </summary>
        /// <param name="rootEntity">Generated splash root entity.</param>
        /// <param name="childIndex">Required direct child index.</param>
        /// <returns>The required sprite component.</returns>
        SpriteComponent FindRequiredSprite(Entity rootEntity, int childIndex) {
            if (rootEntity == null) {
                throw new ArgumentNullException(nameof(rootEntity));
            } else if (childIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Splash sprite child index cannot be negative.");
            } else if (rootEntity.Children == null) {
                throw new InvalidOperationException("The splash root must expose its generated child entities.");
            }

            if (childIndex >= rootEntity.Children.Count || rootEntity.Children[childIndex] == null) {
                throw new InvalidOperationException($"Splash sprite child index '{childIndex}' must exist in the generated hierarchy.");
            }

            Entity childEntity = rootEntity.Children[childIndex];
            for (int componentIndex = 0; componentIndex < childEntity.Components.Count; componentIndex++) {
                if (childEntity.Components[componentIndex] is SpriteComponent spriteComponent) {
                    return spriteComponent;
                }
            }

            throw new InvalidOperationException($"Splash sprite child index '{childIndex}' must contain one SpriteComponent.");
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
