namespace city.game {
    /// <summary>
    /// Drives the Zombislayer Windows first-person camera root using keyboard movement and relative mouse look.
    /// </summary>
    public sealed class ZombislayerFpsControllerComponent : UpdateComponent {
        /// <summary>
        /// Stable child entity name used by the generated scene for the pitch-only camera pivot.
        /// </summary>
        public const string CameraPivotEntityName = "ZombislayerCameraPivot";

        /// <summary>
        /// Stable session root entity name used by the generated scene for pause-state ownership.
        /// </summary>
        public const string SessionRootEntityName = "ZombislayerUiRoot";

        /// <summary>
        /// Minimum supported local pitch in degrees.
        /// </summary>
        public const float MinimumPitchDegrees = -80f;

        /// <summary>
        /// Maximum supported local pitch in degrees.
        /// </summary>
        public const float MaximumPitchDegrees = 80f;

        /// <summary>
        /// Cached pitch-only camera pivot entity resolved from the generated gameplay scene.
        /// </summary>
        Entity CameraPivotEntity;

        /// <summary>
        /// Cached session component used to suppress movement while the gameplay session is paused.
        /// </summary>
        ZombislayerSessionComponent SessionComponent;

        /// <summary>
        /// Accumulated local yaw in degrees.
        /// </summary>
        float CurrentYawDegrees;

        /// <summary>
        /// Accumulated local pitch in degrees.
        /// </summary>
        float CurrentPitchDegrees;

        /// <summary>
        /// Gets or sets the planar movement speed in world units per second.
        /// </summary>
        public float MoveSpeedUnitsPerSecond { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the mouse look sensitivity in degrees per pixel.
        /// </summary>
        public float LookSensitivityDegrees { get; set; } = 0.12f;

        /// <summary>
        /// Clamps one requested pitch angle into the supported first-person range.
        /// </summary>
        /// <param name="pitchDegrees">Requested pitch in degrees.</param>
        /// <returns>Clamped pitch angle in degrees.</returns>
        public static float ClampPitchDegrees(float pitchDegrees) {
            return (float)Math.Clamp(pitchDegrees, MinimumPitchDegrees, MaximumPitchDegrees);
        }

        /// <summary>
        /// Builds one normalized planar movement direction from yaw-relative forward and right input amounts.
        /// </summary>
        /// <param name="yawRadians">Current root yaw in radians.</param>
        /// <param name="forwardAmount">Signed forward input amount.</param>
        /// <param name="rightAmount">Signed right input amount.</param>
        /// <returns>Normalized planar movement direction, or zero when no input is active.</returns>
        public static float3 BuildPlanarMoveDirection(float yawRadians, float forwardAmount, float rightAmount) {
            double yaw = yawRadians;
            double forwardX = Math.Sin(yaw);
            double forwardZ = -Math.Cos(yaw);
            double rightX = Math.Cos(yaw);
            double rightZ = Math.Sin(yaw);
            double combinedX = (forwardX * forwardAmount) + (rightX * rightAmount);
            double combinedZ = (forwardZ * forwardAmount) + (rightZ * rightAmount);
            double length = Math.Sqrt((combinedX * combinedX) + (combinedZ * combinedZ));
            if (length <= 0.0001d) {
                return float3.Zero;
            }

            double inverseLength = 1d / length;
            return new float3((float)(combinedX * inverseLength), 0f, (float)(combinedZ * inverseLength));
        }

        /// <summary>
        /// Advances the first-person controller when the gameplay session is not paused.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("ZombislayerFpsControllerComponent requires an attached player root entity.");
            }

            ResolveRuntimeDependenciesWhenNeeded();
            if (SessionComponent != null && SessionComponent.CurrentSessionState == ZombislayerSessionState.Paused) {
                return;
            }

            double frameDeltaSeconds = Core.Instance.FrameDeltaSeconds;
            if (double.IsNaN(frameDeltaSeconds) || double.IsInfinity(frameDeltaSeconds) || frameDeltaSeconds < 0d) {
                throw new ArgumentOutOfRangeException(nameof(Core.Instance.FrameDeltaSeconds), "Zombislayer first-person movement requires a finite non-negative frame delta.");
            } else if (frameDeltaSeconds == 0d) {
                return;
            }

            UpdateLook(Core.Instance.Input);
            UpdateMovement(Core.Instance.Input, frameDeltaSeconds);
        }

        /// <summary>
        /// Resolves the generated camera pivot and gameplay session component the first time the controller updates.
        /// </summary>
        void ResolveRuntimeDependenciesWhenNeeded() {
            if (CameraPivotEntity == null) {
                CameraPivotEntity = FindRequiredChildEntity(Parent, 0, CameraPivotEntityName);
            }

            if (SessionComponent == null) {
                Entity sceneRoot = FindSceneRootEntity(Parent);
                SessionComponent = FindRequiredComponentRecursive<ZombislayerSessionComponent>(sceneRoot);
            }
        }

        /// <summary>
        /// Applies relative mouse look to the root yaw and child pitch transforms.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        void UpdateLook(InputSystem inputSystem) {
            inputSystem.RequestPointerWrapEnabled();
            CurrentYawDegrees += inputSystem.GetMouseDeltaX() * LookSensitivityDegrees;
            CurrentPitchDegrees = ClampPitchDegrees(CurrentPitchDegrees - (inputSystem.GetMouseDeltaY() * LookSensitivityDegrees));

            float4 yawOrientation;
            float4.CreateFromYawPitchRoll(CurrentYawDegrees * (float)(Math.PI / 180d), 0f, 0f, out yawOrientation);
            yawOrientation.Normalize();
            Parent.LocalOrientation = yawOrientation;

            float4 pitchOrientation;
            float4.CreateFromYawPitchRoll(0f, CurrentPitchDegrees * (float)(Math.PI / 180d), 0f, out pitchOrientation);
            pitchOrientation.Normalize();
            CameraPivotEntity.LocalOrientation = pitchOrientation;
        }

        /// <summary>
        /// Applies planar keyboard movement in the current yaw-relative direction.
        /// </summary>
        /// <param name="inputSystem">Input system supplying the current frame state.</param>
        /// <param name="frameDeltaSeconds">Frame delta in seconds.</param>
        void UpdateMovement(InputSystem inputSystem, double frameDeltaSeconds) {
            float forwardAmount = ResolveAxisAmount(inputSystem.IsKeyDown(Keys.W), inputSystem.IsKeyDown(Keys.S));
            float rightAmount = ResolveAxisAmount(inputSystem.IsKeyDown(Keys.D), inputSystem.IsKeyDown(Keys.A));
            float3 direction = BuildPlanarMoveDirection(CurrentYawDegrees * (float)(Math.PI / 180d), forwardAmount, rightAmount);
            if (direction == float3.Zero) {
                return;
            }

            float distance = (float)(MoveSpeedUnitsPerSecond * frameDeltaSeconds);
            float3 localPosition = Parent.LocalPosition;
            Parent.LocalPosition = new float3(
                localPosition.X + (direction.X * distance),
                localPosition.Y + (direction.Y * distance),
                localPosition.Z + (direction.Z * distance));
        }

        /// <summary>
        /// Resolves one signed input axis amount from positive and negative digital input states.
        /// </summary>
        /// <param name="positivePressed">True when the positive direction is pressed.</param>
        /// <param name="negativePressed">True when the negative direction is pressed.</param>
        /// <returns>Signed axis value in the range [-1, 1].</returns>
        float ResolveAxisAmount(bool positivePressed, bool negativePressed) {
            if (positivePressed == negativePressed) {
                return 0f;
            }

            return positivePressed ? 1f : -1f;
        }

        /// <summary>
        /// Walks upward through the parent chain until the root entity for the generated scene is reached.
        /// </summary>
        /// <param name="entity">Starting entity.</param>
        /// <returns>Scene-root entity.</returns>
        static Entity FindSceneRootEntity(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            Entity current = entity;
            while (current.Parent != null) {
                current = current.Parent;
            }

            return current;
        }

        /// <summary>
        /// Finds one required direct child entity at the supplied index.
        /// </summary>
        /// Finds one required component within the supplied entity subtree.
        /// </summary>
        /// <typeparam name="TComponent">Required component type.</typeparam>
        /// <param name="entity">Subtree root to search.</param>
        /// <returns>Matching component instance.</returns>
        static TComponent FindRequiredComponentRecursive<TComponent>(Entity entity) where TComponent : Component {
            TComponent match = TryFindComponentRecursive<TComponent>(entity);
            if (match == null) {
                throw new InvalidOperationException($"Zombislayer first-person controller could not resolve required component '{typeof(TComponent).Name}'.");
            }

            return match;
        }

        /// <summary>
        /// <param name="entity">Parent entity that should own the required child.</param>
        /// <param name="childIndex">Zero-based child index.</param>
        /// <param name="entityRole">Human-readable role used when building exception messages.</param>
        /// <returns>Matching child entity instance.</returns>
        static Entity FindRequiredChildEntity(Entity entity, int childIndex, string entityRole) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (childIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(childIndex), "Child index must be non-negative.");
            } else if (entity.Children == null || entity.Children.Count <= childIndex) {
                throw new InvalidOperationException($"Zombislayer first-person controller could not resolve required child entity '{entityRole}'.");
            }

            return entity.Children[childIndex];
        }

        /// <summary>
        /// Recursively searches one entity subtree for the first component of the requested type.
        /// </summary>
        /// <typeparam name="TComponent">Required component type.</typeparam>
        /// <param name="entity">Subtree root to search.</param>
        /// <returns>Matching component instance, or null when none exists.</returns>
        static TComponent TryFindComponentRecursive<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null) {
                return null;
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is TComponent typedComponent) {
                        return typedComponent;
                    }
                }
            }
            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                TComponent match = TryFindComponentRecursive<TComponent>(entity.Children[childIndex]);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }
    }
}
