namespace city.menu {
    /// <summary>
    /// Applies the current platform name and version to the demo-disc menu overlay text.
    /// </summary>
    public sealed class PlatformInfoTextComponent : UpdateComponent {
        /// <summary>
        /// Owning entity that hosts the platform-info overlay hierarchy.
        /// </summary>
        Entity OwnerEntity;

        /// <summary>
        /// Cached child entity that renders the platform name.
        /// </summary>
        Entity PlatformNameTextEntity;
        /// <summary>
        /// Cached child entity that renders the platform version.
        /// </summary>
        Entity PlatformVersionTextEntity;
        /// <summary>
        /// Cached text component that renders the platform name.
        /// </summary>
        TextComponent PlatformNameTextComponent;
        /// <summary>
        /// Cached text component that renders the platform version.
        /// </summary>
        TextComponent PlatformVersionTextComponent;

        /// <summary>
        /// Tracks whether the runtime overlay hierarchy has been bound successfully.
        /// </summary>
        bool IsInitialized;

        /// <summary>
        /// Captures the owning entity and attempts initialization once the runtime hierarchy becomes available.
        /// </summary>
        /// <param name="entity">Owning initialized entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            OwnerEntity = entity;
            IsInitialized = false;
            ClearBinding();
            TryInitialize();
        }

        /// <summary>
        /// Releases cached runtime-only binding state when the component leaves the scene hierarchy.
        /// </summary>
        /// <param name="entity">Owning entity that is removing this component.</param>
        public override void ComponentRemoved(Entity entity) {
            ClearBinding();
            OwnerEntity = null;
            IsInitialized = false;
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Waits until the runtime scene loader has attached the child text hierarchy, then applies the live platform information.
        /// </summary>
        public override void Update() {
            if (!IsInitialized) {
                TryInitialize();
            }
        }

        /// <summary>
        /// Attempts to bind the overlay subtree once the scene loader has attached all child entities.
        /// </summary>
        void TryInitialize() {
            if (OwnerEntity == null) {
                return;
            }
            if (!TryBindTextEntities(OwnerEntity)) {
                return;
            } else if (!AreTextComponentsReadyForLayout()) {
                return;
            }

            ApplyCurrentPlatformInfo();
            IsInitialized = true;
        }

        /// <summary>
        /// Returns whether the bound overlay text components have the runtime font assets required for measurement and layout.
        /// </summary>
        /// <returns><c>true</c> when both cached text components expose non-null runtime fonts; otherwise <c>false</c>.</returns>
        bool AreTextComponentsReadyForLayout() {
            if (PlatformNameTextComponent == null || PlatformVersionTextComponent == null) {
                return false;
            }

            return PlatformNameTextComponent.Font != null
                && PlatformVersionTextComponent.Font != null;
        }

        /// <summary>
        /// Applies the current runtime platform name and version to the two overlay rows.
        /// </summary>
        void ApplyCurrentPlatformInfo() {
            if (Core.Instance == null) {
                throw new InvalidOperationException("Platform info requires an active Core instance.");
            } else if (Core.Instance.PlatformInfo == null) {
                throw new InvalidOperationException("Platform info requires initialized runtime platform metadata.");
            }

            ApplyText(PlatformNameTextEntity, PlatformNameTextComponent, Core.Instance.PlatformInfo.Name, 0f);
            ApplyText(PlatformVersionTextEntity, PlatformVersionTextComponent, Core.Instance.PlatformInfo.Version, PlatformNameTextComponent.Size.Y + 6f);
        }

        /// <summary>
        /// Resolves and caches the first two descendant text entities beneath the overlay host.
        /// </summary>
        /// <param name="entity">Owning initialized entity.</param>
        /// <returns><c>true</c> when the platform-info rows were bound successfully; otherwise <c>false</c>.</returns>
        bool TryBindTextEntities(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            List<Entity> textEntities = new List<Entity>();
            CollectChildTextEntities(entity, textEntities);
            if (textEntities.Count < 2) {
                return false;
            }

            PlatformNameTextEntity = textEntities[0];
            PlatformVersionTextEntity = textEntities[1];
            PlatformNameTextComponent = FindTextComponent(PlatformNameTextEntity);
            PlatformVersionTextComponent = FindTextComponent(PlatformVersionTextEntity);
            return true;
        }

        /// <summary>
        /// Collects descendant entities that carry a text component in stable depth-first order.
        /// </summary>
        /// <param name="parentEntity">Current subtree root to scan.</param>
        /// <param name="textEntities">Accumulated descendant text entities.</param>
        void CollectChildTextEntities(Entity parentEntity, List<Entity> textEntities) {
            if (parentEntity == null) {
                throw new ArgumentNullException(nameof(parentEntity));
            } else if (textEntities == null) {
                throw new ArgumentNullException(nameof(textEntities));
            } else if (parentEntity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < parentEntity.Children.Count; childIndex++) {
                Entity childEntity = parentEntity.Children[childIndex];
                if (childEntity == null) {
                    continue;
                }

                if (TryFindTextComponent(childEntity, out TextComponent textComponent)) {
                    textEntities.Add(childEntity);
                    if (textEntities.Count >= 2) {
                        return;
                    }
                }

                CollectChildTextEntities(childEntity, textEntities);
                if (textEntities.Count >= 2) {
                    return;
                }
            }
        }

        /// <summary>
        /// Clears every cached runtime overlay reference.
        /// </summary>
        void ClearBinding() {
            PlatformNameTextEntity = null;
            PlatformVersionTextEntity = null;
            PlatformNameTextComponent = null;
            PlatformVersionTextComponent = null;
        }

        /// <summary>
        /// Applies one platform-info text line to one child entity.
        /// </summary>
        /// <param name="entity">Child text entity that should be updated.</param>
        /// <param name="textComponent">Text component that renders the value.</param>
        /// <param name="text">Text content to display.</param>
        /// <param name="topOffset">Vertical offset from the overlay container.</param>
        void ApplyText(Entity entity, TextComponent textComponent, string text, float topOffset) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (textComponent == null) {
                throw new ArgumentNullException(nameof(textComponent));
            }

            textComponent.Text = text;
            float2 measuredSize = textComponent.Font.MeasureString(text);
            textComponent.Size = new int2(
                (int)Math.Ceiling(measuredSize.X),
                (int)Math.Ceiling(measuredSize.Y));
            entity.LocalPosition = new float3(-textComponent.Size.X, topOffset, 0f);
        }

        /// <summary>
        /// Finds the text component attached to one child entity.
        /// </summary>
        /// <param name="entity">Child entity whose text component should be returned.</param>
        /// <returns>Attached text component.</returns>
        TextComponent FindTextComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is TextComponent textComponent) {
                    return textComponent;
                }
            }

            throw new InvalidOperationException("Platform-info overlay child must include a text component.");
        }

        /// <summary>
        /// Attempts to find a text component attached to one child entity.
        /// </summary>
        /// <param name="entity">Child entity whose text component should be searched.</param>
        /// <param name="textComponent">Resolved text component when one exists.</param>
        /// <returns><c>true</c> when the entity contains a text component; otherwise <c>false</c>.</returns>
        bool TryFindTextComponent(Entity entity, out TextComponent textComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is TextComponent foundTextComponent) {
                    textComponent = foundTextComponent;
                    return true;
                }
            }

            textComponent = null;
            return false;
        }
    }
}
