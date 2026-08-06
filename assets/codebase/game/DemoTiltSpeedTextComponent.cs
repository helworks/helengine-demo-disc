namespace city.game {
    /// <summary>
    /// Updates the Tilt Trial HUD with the current player-ball speed in kilometers per hour.
    /// </summary>
    public sealed class DemoTiltSpeedTextComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets whether HUD text updates should be skipped for the current frame.
        /// </summary>
        public bool UpdatesAreSuppressed { get; set; }

        /// <summary>
        /// Conversion factor from meters per second to kilometers per hour.
        /// </summary>
        const float MetersPerSecondToKilometersPerHour = 3.6f;

        /// <summary>
        /// Serialized scene reference that identifies the tracked player ball entity.
        /// </summary>
        public SceneEntityReference TargetEntityReference { get; set; }

        /// <summary>
        /// Gets or sets the stable authored entity name used to resolve the tracked player across Blueprint boundaries.
        /// </summary>
        public string TargetEntityName { get; set; }

        /// <summary>
        /// Gets or sets the serialized gameplay role used to resolve the tracked entity at runtime.
        /// </summary>
        public string TargetEntityRole { get; set; }

        /// <summary>
        /// Cached live runtime entity resolved from the serialized target reference.
        /// </summary>
        Entity TargetEntity;

        /// <summary>
        /// Cached rigid body attached to the tracked player ball.
        /// </summary>
        RigidBody3DComponent TargetRigidBody;

        /// <summary>
        /// Cached text component that renders the HUD label.
        /// </summary>
        TextComponent SpeedTextComponent;

        /// <summary>
        /// Initializes one Tilt Trial speed HUD updater.
        /// </summary>
        public DemoTiltSpeedTextComponent() {
            UpdatesAreSuppressed = false;
            TargetEntityReference = null;
            TargetEntityName = string.Empty;
            TargetEntityRole = string.Empty;
            TargetEntity = null;
            TargetRigidBody = null;
            SpeedTextComponent = null;
            UpdateOrder = 2;
        }

        /// <summary>
        /// Advances the HUD label so it reflects the latest tracked ball speed.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Parent == null) {
                throw new InvalidOperationException("DemoTiltSpeedTextComponent requires an attached HUD text entity.");
            } else if (UpdatesAreSuppressed) {
                return;
            }

            ResolveTextComponentWhenNeeded();
            ResolveTargetEntityWhenNeeded();
            ResolveTargetRigidBodyWhenNeeded();

            string speedText = FormatSpeedKilometersPerHour(TargetRigidBody.GetLinearVelocity());
            if (!string.Equals(SpeedTextComponent.Text, speedText, StringComparison.Ordinal)) {
                SpeedTextComponent.Text = speedText;
            }
        }

        /// <summary>
        /// Formats one authored rigid-body velocity as a rounded kilometers-per-hour label.
        /// </summary>
        /// <param name="linearVelocity">Current world-space rigid-body velocity.</param>
        /// <returns>Rounded HUD label expressed in kilometers per hour.</returns>
        public static string FormatSpeedKilometersPerHour(float3 linearVelocity) {
            int kilometersPerHour = (int)Math.Round(
                ResolveSpeedKilometersPerHour(linearVelocity),
                MidpointRounding.AwayFromZero);
            return $"{kilometersPerHour}\nkm/h";
        }

        /// <summary>
        /// Converts one authored rigid-body velocity into kilometers per hour.
        /// </summary>
        /// <param name="linearVelocity">Current world-space rigid-body velocity.</param>
        /// <returns>Speed in kilometers per hour.</returns>
        public static float ResolveSpeedKilometersPerHour(float3 linearVelocity) {
            return linearVelocity.Length() * MetersPerSecondToKilometersPerHour;
        }

        /// <summary>
        /// Resolves the text component attached to the current HUD entity.
        /// </summary>
        void ResolveTextComponentWhenNeeded() {
            if (SpeedTextComponent != null) {
                return;
            } else if (Parent.Components == null) {
                throw new InvalidOperationException("DemoTiltSpeedTextComponent requires the HUD entity to expose initialized components.");
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is TextComponent textComponent) {
                    SpeedTextComponent = textComponent;
                    return;
                }
            }

            throw new InvalidOperationException("DemoTiltSpeedTextComponent requires a TextComponent on the same HUD entity.");
        }

        /// <summary>
        /// Resolves the tracked ball entity from the serialized target reference when the runtime cache is still empty.
        /// </summary>
        void ResolveTargetEntityWhenNeeded() {
            if (TargetEntity != null) {
                return;
            } else if (TargetEntityReference == null) {
                throw new InvalidOperationException("DemoTiltSpeedTextComponent requires a serialized target entity reference.");
            } else if (TargetEntityReference.EntityId == 0u) {
                throw new InvalidOperationException("DemoTiltSpeedTextComponent requires a non-zero target scene entity id.");
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("A core instance must exist before Tilt Trial HUD updates can run.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidate = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(candidate) != TargetEntityReference.EntityId) {
                    continue;
                }

                TargetEntity = candidate;
                return;
            }

            throw new InvalidOperationException($"DemoTiltSpeedTextComponent could not resolve target scene entity id {TargetEntityReference.EntityId}.");
        }

        /// <summary>
        /// Finds one runtime entity by its stable authored name.
        /// </summary>
        /// <param name="name">Entity name to resolve.</param>
        /// <returns>Matching runtime entity, or null when it is not loaded yet.</returns>
        [NativeBorrowedReturn]
        Entity FindEntityByName(string name) {
            return FindEntityByRole(name);
        }

        /// <summary>
        /// Finds one runtime entity carrying the requested serialized gameplay role.
        /// </summary>
        /// <param name="role">Gameplay role to resolve.</param>
        /// <returns>Matching runtime entity, or null when it is not loaded yet.</returns>
        [NativeBorrowedReturn]
        Entity FindEntityByRole(string role) {
            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity match = FindEntityByRoleRecursive(entities[entityIndex], role);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches one entity hierarchy for a serialized gameplay role.
        /// </summary>
        /// <param name="entity">Current hierarchy entity.</param>
        /// <param name="role">Gameplay role to resolve.</param>
        /// <returns>Matching entity, or null when the subtree does not contain it.</returns>
        [NativeBorrowedReturn]
        static Entity FindEntityByRoleRecursive(Entity entity, string role) {
            if (entity == null) {
                return null;
            }
            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is TiltTrialEntityRoleComponent roleComponent
                        && string.Equals(roleComponent.Role, role, StringComparison.Ordinal)) {
                        return entity;
                    }
                }
            }
            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                Entity match = FindEntityByRoleRecursive(entity.Children[childIndex], role);
                if (match != null) {
                    return match;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches one entity hierarchy for a stable authored name.
        /// </summary>
        /// <param name="entity">Current hierarchy entity.</param>
        /// <param name="name">Entity name to resolve.</param>
        /// <returns>Matching entity, or null when the subtree does not contain it.</returns>
        [NativeBorrowedReturn]
        static Entity FindEntityByNameRecursive(Entity entity, string name) {
            return FindEntityByRoleRecursive(entity, name);
        }

        /// <summary>
        /// Resolves the rigid body attached to the tracked ball when the runtime cache is still empty.
        /// </summary>
        void ResolveTargetRigidBodyWhenNeeded() {
            if (TargetRigidBody != null) {
                return;
            } else if (TargetEntity == null) {
                throw new InvalidOperationException("DemoTiltSpeedTextComponent requires a resolved target entity before rigid-body lookup can run.");
            } else if (TargetEntity.Components == null) {
                throw new InvalidOperationException("DemoTiltSpeedTextComponent requires the tracked ball to expose initialized components.");
            }

            for (int componentIndex = 0; componentIndex < TargetEntity.Components.Count; componentIndex++) {
                if (TargetEntity.Components[componentIndex] is RigidBody3DComponent rigidBody) {
                    TargetRigidBody = rigidBody;
                    return;
                }
            }

            throw new InvalidOperationException("DemoTiltSpeedTextComponent requires a RigidBody3DComponent on the tracked ball.");
        }

        /// <summary>
        /// Resolves the authored scene entity id attached to one runtime entity when present.
        /// </summary>
        /// <param name="entity">Runtime entity to inspect.</param>
        /// <returns>Authored scene entity id when present; otherwise <c>0</c>.</returns>
        uint FindSceneEntityRuntimeIdOrZero(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
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
