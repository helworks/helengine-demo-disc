namespace city.menu {
    /// <summary>
    /// Starts one looping playful idle transform animation on the demo-disc logo sprite.
    /// </summary>
    public sealed class DemoDiscLogoIdleAnimationComponent : UpdateComponent {
        /// <summary>
        /// Maximum playback-time delta still considered unchanged across one frame.
        /// </summary>
        const float PlaybackTimeEpsilon = 0.0001f;

        /// <summary>
        /// Gets or sets the authored looping transform clip played by the sibling animation player.
        /// </summary>
        public AnimationClipAsset IdleClip { get; set; }

        /// <summary>
        /// Sibling animation player that owns runtime transform playback for the logo entity.
        /// </summary>
        AnimationPlayerComponent AnimationPlayerValue;

        /// <summary>
        /// Most recent playback time observed on the sibling animation player.
        /// </summary>
        float LastObservedPlaybackTimeValue;

        /// <summary>
        /// Tracks whether one playback sample has already been captured from the sibling animation player.
        /// </summary>
        bool HasObservedPlaybackTimeValue;

        /// <summary>
        /// Starts the authored looping logo clip as soon as the component joins the generated menu logo entity.
        /// </summary>
        /// <param name="entity">Owning logo entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);

            AnimationPlayerValue = ResolveRequiredAnimationPlayer(entity);
            PlayIdleClip();
        }

        /// <summary>
        /// Restarts the idle clip after the full entity hierarchy has initialized so the animation base transform matches the final anchored menu position.
        /// </summary>
        /// <param name="entity">Owning logo entity.</param>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            AnimationPlayerValue = ResolveRequiredAnimationPlayer(entity);
            PlayIdleClip();
        }

        /// <summary>
        /// Keeps the authored idle clip alive even when the animation player was not advanced by the standard update registration path.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                return;
            } else if (AnimationPlayerValue == null) {
                AnimationPlayerValue = ResolveRequiredAnimationPlayer(Parent);
                PlayIdleClip();
                return;
            }

            if (AnimationPlayerValue.CurrentClip != IdleClip || !AnimationPlayerValue.IsPlaying) {
                PlayIdleClip();
                return;
            }

            if (HasObservedPlaybackTimeValue && Math.Abs(AnimationPlayerValue.CurrentTime - LastObservedPlaybackTimeValue) <= PlaybackTimeEpsilon) {
                AnimationPlayerValue.Advance(AnimationPlayerValue.FrameDeltaTime);
            }

            LastObservedPlaybackTimeValue = AnimationPlayerValue.CurrentTime;
            HasObservedPlaybackTimeValue = true;
        }

        /// <summary>
        /// Resolves the sibling animation player that owns runtime clip playback for the logo entity.
        /// </summary>
        /// <param name="entity">Owning logo entity.</param>
        /// <returns>Sibling animation player component.</returns>
        AnimationPlayerComponent ResolveRequiredAnimationPlayer(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("DemoDiscLogoIdleAnimationComponent requires the owning entity component collection to be initialized.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is AnimationPlayerComponent animationPlayer) {
                    return animationPlayer;
                }
            }

            throw new InvalidOperationException("DemoDiscLogoIdleAnimationComponent requires a sibling AnimationPlayerComponent.");
        }

        /// <summary>
        /// Starts the shared idle clip from the current anchored base transform and resets the local playback observation state.
        /// </summary>
        void PlayIdleClip() {
            if (AnimationPlayerValue == null) {
                throw new InvalidOperationException("DemoDiscLogoIdleAnimationComponent requires one resolved AnimationPlayerComponent before playback can begin.");
            }
            if (IdleClip == null) {
                throw new InvalidOperationException("DemoDiscLogoIdleAnimationComponent requires one authored IdleClip asset before playback can begin.");
            }

            AnimationPlayerValue.Play(IdleClip, true);
            LastObservedPlaybackTimeValue = AnimationPlayerValue.CurrentTime;
            HasObservedPlaybackTimeValue = true;
        }
    }
}
