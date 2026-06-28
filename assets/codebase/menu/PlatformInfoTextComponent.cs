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
        /// Cached layout component that anchors the platform name text.
        /// </summary>
        LayoutComponent PlatformNameLayoutComponent;
        /// <summary>
        /// Cached text component that renders the platform version.
        /// </summary>
        TextComponent PlatformVersionTextComponent;
        /// <summary>
        /// Cached layout component that anchors the platform version text.
        /// </summary>
        LayoutComponent PlatformVersionLayoutComponent;

        /// <summary>
        /// Captures the authored local position assigned to the platform-name text entity before runtime layout adjusts it.
        /// </summary>
        float3 PlatformNameBaseLocalPosition;

        /// <summary>
        /// Captures the authored layout size assigned to the platform-name text entity before runtime text measurement adjusts it.
        /// </summary>
        int2 PlatformNameBaseSize;

        /// <summary>
        /// Captures the authored local position assigned to the platform-version text entity before runtime layout adjusts it.
        /// </summary>
        float3 PlatformVersionBaseLocalPosition;

        /// <summary>
        /// Captures the authored layout size assigned to the platform-version text entity before runtime text measurement adjusts it.
        /// </summary>
        int2 PlatformVersionBaseSize;

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

            bool useHorizontalRowLayout = Math.Abs(PlatformNameBaseLocalPosition.Y - PlatformVersionBaseLocalPosition.Y) < 0.01f;
            if (useHorizontalRowLayout) {
                ApplyHorizontalText(PlatformNameTextEntity, PlatformNameTextComponent, PlatformNameLayoutComponent, Core.Instance.PlatformInfo.Name, PlatformNameBaseLocalPosition.X, PlatformNameBaseLocalPosition.Y, PlatformNameBaseSize, TextAlignment.Left);
                ApplyHorizontalText(PlatformVersionTextEntity, PlatformVersionTextComponent, PlatformVersionLayoutComponent, Core.Instance.PlatformInfo.Version, PlatformVersionBaseLocalPosition.X, PlatformVersionBaseLocalPosition.Y, PlatformVersionBaseSize, TextAlignment.Right);
                return;
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

            Entity firstTextEntity = null;
            Entity secondTextEntity = null;
            if (!TryCollectFirstTwoTextEntities(entity, ref firstTextEntity, ref secondTextEntity)) {
                return false;
            }

            PlatformNameTextEntity = firstTextEntity;
            PlatformVersionTextEntity = secondTextEntity;
            PlatformNameTextComponent = FindTextComponent(PlatformNameTextEntity);
            PlatformVersionTextComponent = FindTextComponent(PlatformVersionTextEntity);
            PlatformNameLayoutComponent = FindLayoutComponent(PlatformNameTextEntity);
            PlatformVersionLayoutComponent = FindLayoutComponent(PlatformVersionTextEntity);
            PlatformNameBaseLocalPosition = PlatformNameTextEntity.LocalPosition;
            PlatformNameBaseSize = PlatformNameTextComponent.Size;
            PlatformVersionBaseLocalPosition = PlatformVersionTextEntity.LocalPosition;
            PlatformVersionBaseSize = PlatformVersionTextComponent.Size;
            return true;
        }

        /// <summary>
        /// Collects the first two descendant entities that carry a text component in stable depth-first order without allocating a temporary runtime list.
        /// </summary>
        /// <param name="parentEntity">Current subtree root to scan.</param>
        /// <param name="firstTextEntity">First discovered descendant text entity.</param>
        /// <param name="secondTextEntity">Second discovered descendant text entity.</param>
        /// <returns><c>true</c> when two descendant text entities were discovered; otherwise <c>false</c>.</returns>
        bool TryCollectFirstTwoTextEntities(Entity parentEntity, ref Entity firstTextEntity, ref Entity secondTextEntity) {
            if (parentEntity == null) {
                throw new ArgumentNullException(nameof(parentEntity));
            } else if (parentEntity.Children == null) {
                return false;
            }

            for (int childIndex = 0; childIndex < parentEntity.Children.Count; childIndex++) {
                Entity childEntity = parentEntity.Children[childIndex];
                if (childEntity == null) {
                    continue;
                }

                if (TryFindTextComponent(childEntity, out TextComponent textComponent)) {
                    if (firstTextEntity == null) {
                        firstTextEntity = childEntity;
                    } else if (secondTextEntity == null) {
                        secondTextEntity = childEntity;
                        return true;
                    }
                }

                if (TryCollectFirstTwoTextEntities(childEntity, ref firstTextEntity, ref secondTextEntity)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears every cached runtime overlay reference.
        /// </summary>
        void ClearBinding() {
            PlatformNameTextEntity = null;
            PlatformVersionTextEntity = null;
            PlatformNameTextComponent = null;
            PlatformVersionTextComponent = null;
            PlatformNameLayoutComponent = null;
            PlatformVersionLayoutComponent = null;
            PlatformNameBaseLocalPosition = float3.Zero;
            PlatformNameBaseSize = new int2(0, 0);
            PlatformVersionBaseLocalPosition = float3.Zero;
            PlatformVersionBaseSize = new int2(0, 0);
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

            textComponent.Alignment = TextAlignment.Right;
            textComponent.Text = text;
            float2 measuredSize = textComponent.Font.MeasureString(text);
            double fontScale = textComponent.FontScale;
            textComponent.Size = new int2(
                (int)Math.Ceiling(measuredSize.X * fontScale),
                (int)Math.Ceiling(measuredSize.Y * fontScale));
            entity.LocalPosition = new float3(-textComponent.Size.X, topOffset, 0f);
        }

        /// <summary>
        /// Applies one platform-info text line using authored horizontal anchors so the overlay can present one left/right row.
        /// </summary>
        /// <param name="entity">Child text entity that should be updated.</param>
        /// <param name="textComponent">Text component that renders the value.</param>
        /// <param name="text">Text content to display.</param>
        /// <param name="baseX">Authored local X anchor.</param>
        /// <param name="baseY">Authored local Y anchor.</param>
        /// <param name="baseSize">Authored layout box preserved for anchored horizontal alignment.</param>
        /// <param name="alignment">Requested horizontal text alignment.</param>
        void ApplyHorizontalText(Entity entity, TextComponent textComponent, LayoutComponent layoutComponent, string text, float baseX, float baseY, int2 baseSize, TextAlignment alignment) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (textComponent == null) {
                throw new ArgumentNullException(nameof(textComponent));
            }

            textComponent.Alignment = alignment;
            textComponent.Text = text;
            float2 measuredSize = textComponent.Font.MeasureString(text);
            double fontScale = textComponent.FontScale;
            textComponent.Size = new int2(
                Math.Max(baseSize.X, (int)Math.Ceiling(measuredSize.X * fontScale)),
                Math.Max(baseSize.Y, (int)Math.Ceiling(measuredSize.Y * fontScale)));
            if (layoutComponent != null && layoutComponent.IsAnchored) {
                layoutComponent.RefreshAnchoring();
                return;
            }
            if (alignment == TextAlignment.Right) {
                entity.LocalPosition = new float3(baseX - textComponent.Size.X, baseY, 0f);
                return;
            }

            entity.LocalPosition = new float3(baseX, baseY, 0f);
        }

        /// <summary>
        /// Finds the layout component attached to one child entity.
        /// </summary>
        /// <param name="entity">Child entity whose layout component should be searched.</param>
        /// <returns>Resolved layout component.</returns>
        LayoutComponent FindLayoutComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is LayoutComponent layoutComponent) {
                    return layoutComponent;
                }
            }

            return null;
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
